using Corela15.Api.Idempotencia;
using Corela15.Application.Ahorros;
using Corela15.Application.Common;
using Corela15.Domain.Ahorros;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record ProductoAhorroListItem(
    int Id, string Codigo, string Nombre, bool PermiteDebitoPrestamo, bool Activo);

public record CuentaAhorroListItem(
    Guid Id, string Numero, string Producto, string Agencia, string Estado, DateOnly FechaApertura,
    decimal SaldoDisponible, bool PermiteDebitoPrestamo, bool AcreditaPrestamo);

[ApiController]
[Route("api/ahorros")]
[Authorize(Policy = "Menu:ahorros")]
public class AhorrosController(
    Corela15DbContext db, ICuentaAhorroService cuentaAhorroService, IDevengoInteresService devengoInteresService) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<ActionResult<IReadOnlyList<ProductoAhorroListItem>>> Productos(
        CancellationToken cancellationToken)
    {
        var resultado = await db.TiposCuenta
            .OrderBy(t => t.Nombre)
            .Select(t => new ProductoAhorroListItem(t.Id, t.Codigo, t.Nombre, t.PermiteDebitoPrestamo, t.Activo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("cuentas")]
    public async Task<ActionResult<IReadOnlyList<CuentaAhorroListItem>>> Cuentas(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Cuentas.Include(c => c.TipoCuenta).Include(c => c.Agencia).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c => EF.Functions.ILike(c.Numero, $"%{q}%"));
        }

        var resultado = await query
            .OrderByDescending(c => c.FechaApertura)
            .Select(c => new CuentaAhorroListItem(
                c.Id, c.Numero, c.TipoCuenta.Nombre, c.Agencia.Nombre, c.Estado.ToString(), c.FechaApertura,
                db.CuentasItemSaldo
                    .Where(i => i.IdCuenta == c.Id && i.ItemSaldo.Codigo == "DISP")
                    .Select(i => i.Saldo)
                    .FirstOrDefault(),
                c.TipoCuenta.PermiteDebitoPrestamo,
                db.CuentasItemSaldo
                    .Where(i => i.IdCuenta == c.Id && i.ItemSaldo.Codigo == "DISP")
                    .Select(i => i.AcreditaPrestamo)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Reportes reales verificados contra el catálogo de Softbank
    // (SEGURIDAD.MENU_REPORTE, ver 06-catalogo-reportes-softbank.md) —
    // Ahorros.CuentasAperturadas, Ahorros.CuentasBloqueadas,
    // Ahorros.TransaccionesAhorrosReport.
    [HttpGet("reportes/cuentas-aperturadas")]
    public async Task<ActionResult<IReadOnlyList<CuentaAperturadaItem>>> ReporteCuentasAperturadas(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.Cuentas
            .Include(c => c.TipoCuenta).Include(c => c.Agencia)
            .Where(c => c.FechaApertura >= desde && c.FechaApertura <= hasta)
            .OrderBy(c => c.FechaApertura)
            .Select(c => new CuentaAperturadaItem(
                c.Numero, c.TipoCuenta.Nombre, c.Agencia.Nombre,
                db.CuentasClientes.Where(cc => cc.IdCuenta == c.Id && cc.Principal)
                    .Select(cc => cc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                c.FechaApertura,
                db.CuentasItemSaldo.Where(i => i.IdCuenta == c.Id && i.ItemSaldo.Codigo == "DISP")
                    .Select(i => i.Saldo).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("reportes/cuentas-bloqueadas")]
    public async Task<ActionResult<IReadOnlyList<CuentaEstadoEspecialItem>>> ReporteCuentasBloqueadas(
        CancellationToken cancellationToken)
    {
        var resultado = await db.Cuentas
            .Include(c => c.TipoCuenta).Include(c => c.Agencia)
            .Where(c => c.Estado == Corela15.Domain.Ahorros.EstadoCuenta.Bloqueada)
            .OrderByDescending(c => c.ModificadoEn)
            .Select(c => new CuentaEstadoEspecialItem(
                c.Numero, c.TipoCuenta.Nombre, c.Agencia.Nombre,
                db.CuentasClientes.Where(cc => cc.IdCuenta == c.Id && cc.Principal)
                    .Select(cc => cc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                c.ModificadoEn, c.ModificadoPor))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("reportes/transacciones")]
    public async Task<ActionResult<IReadOnlyList<TransaccionAhorroItem>>> ReporteTransacciones(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var desdeUtc = new DateTimeOffset(desde.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var hastaUtc = new DateTimeOffset(hasta.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var resultado = await db.CuentasMovimientos
            .Include(m => m.Cuenta)
            .Where(m => m.FechaHora >= desdeUtc && m.FechaHora <= hastaUtc)
            .OrderByDescending(m => m.FechaHora)
            .Select(m => new TransaccionAhorroItem(
                m.Cuenta.Numero, m.Tipo.ToString(), m.Monto, m.SaldoResultante, m.FechaHora, m.RegistradoPor))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("tipos-transaccion")]
    public async Task<ActionResult<IReadOnlyList<TipoTransaccionListItem>>> TiposTransaccion(
        CancellationToken cancellationToken)
    {
        var resultado = await db.TiposTransaccion
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoTransaccionListItem(t.Id, t.Codigo, t.Nombre, t.SignoSaldoCuenta))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("cuentas")]
    public async Task<ActionResult<CuentaAhorroAbiertaResult>> AbrirCuenta(
        [FromBody] AbrirCuentaBody body, CancellationToken cancellationToken)
    {
        var resultado = await cuentaAhorroService.AbrirAsync(
            new AbrirCuentaAhorroRequest(body.IdCliente, body.IdTipoCuenta, body.IdAgencia, body.MontoInicial, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/ahorros/cuentas/{resultado.IdCuenta}", resultado);
    }

    [HttpPost("cuentas/{idCuenta:guid}/movimientos")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<MovimientoCuentaRegistradoResult>> RegistrarMovimiento(
        Guid idCuenta, [FromBody] RegistrarMovimientoBody body, CancellationToken cancellationToken)
    {
        var resultado = await cuentaAhorroService.RegistrarMovimientoAsync(
            new RegistrarMovimientoCuentaRequest(idCuenta, body.CodigoTipoTransaccion, body.Monto, User.Identity!.Name!),
            cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("devengo-interes/ejecutar")]
    public async Task<ActionResult<DevengoInteresEjecutadoResult>> EjecutarDevengoInteres(CancellationToken cancellationToken)
    {
        var resultado = await devengoInteresService.EjecutarDevengoDiarioAsync(User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }

    // Tercera de las tres configuraciones independientes del auto-débito de
    // cuota por SPI (ver CuentaItemSaldo.cs / AutoDebitoSpiLog.cs) — flag
    // simple sin invariante de negocio, se resuelve directo contra el
    // DbContext, mismo patrón que los catálogos de Configuración.
    [HttpPatch("cuentas/{idCuenta:guid}/acredita-prestamo")]
    public async Task<IActionResult> ConfigurarAcreditaPrestamo(
        Guid idCuenta, [FromBody] ConfigurarAcreditaPrestamoBody body, CancellationToken cancellationToken)
    {
        var item = await db.CuentasItemSaldo
            .Include(x => x.ItemSaldo)
            .FirstOrDefaultAsync(x => x.IdCuenta == idCuenta && x.ItemSaldo.Codigo == "DISP", cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        item.AcreditaPrestamo = body.Activar;
        item.ModificadoEn = DateTimeOffset.UtcNow;
        item.ModificadoPor = User.Identity!.Name!;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Bloqueo/desbloqueo/cierre real de cuenta — gap encontrado auditando
    // el catálogo real de reportes de Softbank (Ahorros.CuentasBloqueadas/
    // CuentasDesbloquedas): EstadoCuenta ya tenía Bloqueada/Cerrada desde
    // el diseño original de Nivel 2, pero ningún caso de uso los activaba
    // — toda cuenta se creaba Activa y nunca cambiaba. RegistrarMovimientoAsync
    // ya rechaza movimientos sobre una cuenta no Activa (CuentaInvalidaException),
    // así que el enforcement real ya existía, solo faltaba el disparador.
    // El cambio de estado queda auditado automáticamente vía cuenta_historico
    // (versionado real por trigger, ya construido en Nivel 2).
    [HttpPatch("cuentas/{idCuenta:guid}/estado")]
    public async Task<IActionResult> CambiarEstadoCuenta(
        Guid idCuenta, [FromBody] CambiarEstadoCuentaBody body, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EstadoCuenta>(body.Estado, out var nuevoEstado))
        {
            throw new SolicitudInvalidaExceptionGenerica($"Estado de cuenta inválido: '{body.Estado}'");
        }

        var cuenta = await db.Cuentas.FirstOrDefaultAsync(c => c.Id == idCuenta, cancellationToken);
        if (cuenta is null)
        {
            return NotFound();
        }

        if (nuevoEstado == EstadoCuenta.Cerrada)
        {
            var saldoDisponible = await db.CuentasItemSaldo
                .Where(i => i.IdCuenta == idCuenta && i.ItemSaldo.Codigo == "DISP")
                .Select(i => i.Saldo)
                .FirstOrDefaultAsync(cancellationToken);
            if (saldoDisponible != 0)
            {
                throw new SolicitudInvalidaExceptionGenerica(
                    $"No se puede cerrar la cuenta con saldo disponible distinto de cero ({saldoDisponible:0.00})");
            }
        }

        cuenta.Estado = nuevoEstado;
        cuenta.ModificadoEn = DateTimeOffset.UtcNow;
        cuenta.ModificadoPor = User.Identity!.Name!;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public record ConfigurarAcreditaPrestamoBody(bool Activar);

public record CambiarEstadoCuentaBody(string Estado);

public record CuentaAperturadaItem(string Numero, string Producto, string Agencia, string Cliente, DateOnly FechaApertura, decimal SaldoInicial);

public record CuentaEstadoEspecialItem(string Numero, string Producto, string Agencia, string Cliente, DateTimeOffset? Fecha, string? RegistradoPor);

public record TransaccionAhorroItem(string NumeroCuenta, string Tipo, decimal Monto, decimal SaldoResultante, DateTimeOffset Fecha, string RegistradoPor);

public record RegistrarMovimientoBody(string CodigoTipoTransaccion, decimal Monto);

public record AbrirCuentaBody(Guid IdCliente, int IdTipoCuenta, int IdAgencia, decimal MontoInicial);

public record TipoTransaccionListItem(int Id, string Codigo, string Nombre, int SignoSaldoCuenta);
