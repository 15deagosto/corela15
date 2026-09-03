using Corela15.Api.Idempotencia;
using Corela15.Application.Ahorros;
using Corela15.Application.Cajas;
using Corela15.Application.Common;
using Corela15.Domain.Cajas;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record UsuarioParaCajaListItem(Guid Id, string NombreUsuario);

public record VentanillaListItem(
    Guid Id, string Usuario, string Agencia, DateOnly Fecha, bool Cerrada, bool Cuadrada, decimal SaldoEfectivo);

public record VentanillaMovimientoItem(
    DateTimeOffset Fecha, decimal Valor, decimal SaldoResultante, string Descripcion, string RegistradoPor, Guid? IdComprobanteContable);

[ApiController]
[Route("api/cajas")]
[Authorize(Policy = "Menu:cajas")]
public class CajasController(
    Corela15DbContext db, IVentanillaService ventanillaService, IAutorizacionTransaccionService autorizacionService,
    IPagoExternoService pagoExternoService) : ControllerBase
{
    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaCajaListItem>>> Usuarios(CancellationToken cancellationToken)
    {
        var resultado = await db.Usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioParaCajaListItem(u.Id, u.NombreUsuario))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("ventanillas")]
    public async Task<ActionResult<IReadOnlyList<VentanillaListItem>>> Ventanillas(CancellationToken cancellationToken)
    {
        var resultado = await db.Ventanillas
            .Include(v => v.Usuario)
            .Include(v => v.Agencia)
            .OrderByDescending(v => v.Fecha)
            .Select(v => new VentanillaListItem(
                v.Id, v.Usuario.NombreUsuario, v.Agencia.Nombre, v.Fecha, v.Cerrada, v.Cuadrada,
                db.VentanillasItemCaja.Where(ic => ic.IdVentanilla == v.Id && ic.ItemCaja.Codigo == "EFE")
                    .Select(ic => (decimal?)ic.Saldo).FirstOrDefault() ?? 0))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Bitácora real de movimientos de efectivo — cierra el gap real de
    // "el cuadre no compara contra un monto esperado calculado" (ver
    // VentanillaItemCaja.cs / ComprobanteContableService).
    [HttpGet("ventanillas/{idVentanilla:guid}/movimientos")]
    public async Task<ActionResult<IReadOnlyList<VentanillaMovimientoItem>>> Movimientos(
        Guid idVentanilla, CancellationToken cancellationToken)
    {
        var resultado = await db.VentanillasItemCajaMovimiento
            .Where(m => m.VentanillaItemCaja.IdVentanilla == idVentanilla)
            .OrderByDescending(m => m.Fecha)
            .Select(m => new VentanillaMovimientoItem(
                m.Fecha, m.Valor, m.SaldoResultante, m.Descripcion, m.RegistradoPor, m.IdComprobanteContable))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("ventanillas")]
    public async Task<ActionResult<VentanillaAbiertaResult>> Abrir(
        [FromBody] AbrirVentanillaBody body, CancellationToken cancellationToken)
    {
        var idUsuario = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")!.Value);
        var resultado = await ventanillaService.AbrirAsync(
            new AbrirVentanillaRequest(idUsuario, body.IdAgencia), cancellationToken);
        return Created($"/api/cajas/ventanillas/{resultado.IdVentanilla}", resultado);
    }

    [HttpPost("ventanillas/{idVentanilla:guid}/cerrar")]
    public async Task<ActionResult<VentanillaCerradaResult>> Cerrar(
        Guid idVentanilla, [FromBody] CerrarVentanillaBody body, CancellationToken cancellationToken)
    {
        var resultado = await ventanillaService.CerrarAsync(
            new CerrarVentanillaRequest(idVentanilla, body.TotalEfectivoContado, body.TotalCheque, User.Identity!.Name!),
            cancellationToken);
        return Ok(resultado);
    }
    // Bóveda es configuración persistente por agencia (no una sesión
    // diaria como Ventanilla) — ver Boveda.cs. CRUD directo contra el
    // DbContext, mismo patrón que los catálogos simples de Configuración:
    // no hay invariante de negocio más allá de unicidad por agencia.
    [HttpGet("bovedas")]
    public async Task<ActionResult<IReadOnlyList<BovedaListItem>>> Bovedas(CancellationToken cancellationToken)
    {
        var resultado = await db.Bovedas
            .Include(b => b.Agencia)
            .Include(b => b.UsuarioResponsable)
            .OrderBy(b => b.Agencia.Nombre)
            .Select(b => new BovedaListItem(
                b.Id, b.IdAgencia, b.Agencia.Nombre, b.IdUsuarioResponsable, b.UsuarioResponsable.NombreUsuario,
                b.ExistenciaMinima, b.ExistenciaMaxima, b.ExistenciaMinimaCaja, b.ExistenciaMaximaCaja,
                b.ExistenciaMinimaCajaChica, b.ExistenciaMaximaCajaChica, b.Activa,
                db.BovedasItemBoveda.Where(s => s.IdBoveda == b.Id)
                    .Select(s => new BovedaSaldoItem(s.ItemBoveda.Codigo, s.ItemBoveda.Nombre, s.Moneda.Codigo, s.Saldo))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("bovedas")]
    public async Task<ActionResult> CrearBoveda([FromBody] BovedaBody body, CancellationToken cancellationToken)
    {
        if (await db.Bovedas.AnyAsync(b => b.IdAgencia == body.IdAgencia, cancellationToken))
        {
            throw new CodigoDuplicadoException("una bóveda para esta agencia", body.IdAgencia.ToString());
        }

        var boveda = new Boveda
        {
            Id = Guid.NewGuid(),
            IdAgencia = body.IdAgencia,
            IdUsuarioResponsable = body.IdUsuarioResponsable,
            ExistenciaMinima = body.ExistenciaMinima,
            ExistenciaMaxima = body.ExistenciaMaxima,
            ExistenciaMinimaCaja = body.ExistenciaMinimaCaja,
            ExistenciaMaximaCaja = body.ExistenciaMaximaCaja,
            ExistenciaMinimaCajaChica = body.ExistenciaMinimaCajaChica,
            ExistenciaMaximaCajaChica = body.ExistenciaMaximaCajaChica,
            Activa = true,
        };
        db.Bovedas.Add(boveda);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/cajas/bovedas/{boveda.Id}", new { boveda.Id });
    }

    [HttpPut("bovedas/{id:guid}")]
    public async Task<IActionResult> ActualizarBoveda(Guid id, [FromBody] BovedaActualizarBody body, CancellationToken cancellationToken)
    {
        var boveda = await db.Bovedas.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (boveda is null)
        {
            return NotFound();
        }

        boveda.IdUsuarioResponsable = body.IdUsuarioResponsable;
        boveda.ExistenciaMinima = body.ExistenciaMinima;
        boveda.ExistenciaMaxima = body.ExistenciaMaxima;
        boveda.ExistenciaMinimaCaja = body.ExistenciaMinimaCaja;
        boveda.ExistenciaMaximaCaja = body.ExistenciaMaximaCaja;
        boveda.ExistenciaMinimaCajaChica = body.ExistenciaMinimaCajaChica;
        boveda.ExistenciaMaximaCajaChica = body.ExistenciaMaximaCajaChica;
        boveda.Activa = body.Activa;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Ajuste manual del saldo de bóveda — sin caso de uso transaccional
    // real todavía (ver BovedaItemBoveda.cs), así que el saldo se
    // mantiene por ajuste administrativo directo hasta que exista el
    // flujo real de transferencia caja↔bóveda.
    [HttpPost("bovedas/{id:guid}/saldos/ajustar")]
    public async Task<IActionResult> AjustarSaldoBoveda(
        Guid id, [FromBody] AjustarSaldoBovedaBody body, CancellationToken cancellationToken)
    {
        if (!await db.Bovedas.AnyAsync(b => b.Id == id, cancellationToken))
        {
            return NotFound();
        }

        var saldo = await db.BovedasItemBoveda.FirstOrDefaultAsync(
            s => s.IdBoveda == id && s.IdItemBoveda == body.IdItemBoveda && s.IdMoneda == body.IdMoneda, cancellationToken);
        if (saldo is null)
        {
            saldo = new BovedaItemBoveda
            {
                Id = Guid.NewGuid(),
                IdBoveda = id,
                IdItemBoveda = body.IdItemBoveda,
                IdMoneda = body.IdMoneda,
            };
            db.BovedasItemBoveda.Add(saldo);
        }

        saldo.Saldo = body.NuevoSaldo;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("items-boveda")]
    public async Task<ActionResult<IReadOnlyList<ItemBovedaListItem>>> ItemsBoveda(CancellationToken cancellationToken)
    {
        var resultado = await db.ItemsBoveda
            .Where(i => i.Activo)
            .OrderBy(i => i.Nombre)
            .Select(i => new ItemBovedaListItem(i.Id, i.Codigo, i.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("autorizaciones-transaccion")]
    public async Task<ActionResult<IReadOnlyList<AutorizacionPendienteDetalle>>> AutorizacionesPendientes(CancellationToken cancellationToken)
    {
        var resultado = await autorizacionService.ListarPendientesAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("autorizaciones-transaccion/{idAutorizacion:guid}/aprobar")]
    public async Task<ActionResult<MovimientoCuentaRegistradoResult>> AprobarAutorizacion(
        Guid idAutorizacion, CancellationToken cancellationToken)
    {
        var resultado = await autorizacionService.AprobarAsync(idAutorizacion, User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("autorizaciones-transaccion/{idAutorizacion:guid}/rechazar")]
    public async Task<IActionResult> RechazarAutorizacion(
        Guid idAutorizacion, [FromBody] RechazarAutorizacionBody body, CancellationToken cancellationToken)
    {
        await autorizacionService.RechazarAsync(idAutorizacion, body.Comentario, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpGet("pago-externo/productos")]
    public async Task<ActionResult<IReadOnlyList<PagoExternoProductoListItem>>> PagoExternoProductos(CancellationToken cancellationToken)
    {
        var resultado = await db.PagoExternoProductos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new PagoExternoProductoListItem(p.Id, p.Nombre, p.TituloReferencia, p.RequiereDatosFactura, p.TieneComision))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("pago-externo/transacciones")]
    public async Task<ActionResult<IReadOnlyList<PagoExternoTransaccionListItem>>> PagoExternoTransacciones(CancellationToken cancellationToken)
    {
        var resultado = await db.PagoExternoTransacciones
            .Include(t => t.Producto)
            .OrderByDescending(t => t.FechaProceso)
            .Select(t => new PagoExternoTransaccionListItem(
                t.Id, t.Producto.Nombre, t.Referencia, t.Documento, t.Valor, t.Comision, t.FechaProceso, t.Reversada, t.RegistradoPor))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("pago-externo/transacciones")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PagoExternoRegistradoResult>> RegistrarPagoExterno(
        [FromBody] RegistrarPagoExternoBody body, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios.FirstAsync(u => u.NombreUsuario == User.Identity!.Name, cancellationToken);
        var resultado = await pagoExternoService.RegistrarAsync(
            new RegistrarPagoExternoRequest(
                body.IdProducto, body.Referencia, body.Documento, body.Valor, body.Comision, usuario.IdAgencia, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/cajas/pago-externo/transacciones/{resultado.Id}", resultado);
    }

    [HttpPost("pago-externo/transacciones/{id:guid}/reversar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PagoExternoReversadoResult>> ReversarPagoExterno(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await pagoExternoService.ReversarAsync(
            new ReversarPagoExternoRequest(id, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    // Reportes reales verificados contra el catálogo de Softbank
    // (SEGURIDAD.MENU_REPORTE, ver 06-catalogo-reportes-softbank.md) —
    // Cajas.VentanillasAperturadasPorAgencia, Cajas.ConsolidadoTransacciones.
    [HttpGet("reportes/ventanillas-aperturadas")]
    public async Task<ActionResult<IReadOnlyList<VentanillaAperturadaItem>>> ReporteVentanillasAperturadas(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.Ventanillas
            .Include(v => v.Usuario).Include(v => v.Agencia)
            .Where(v => v.Fecha >= desde && v.Fecha <= hasta)
            .OrderBy(v => v.Fecha).ThenBy(v => v.Agencia.Nombre)
            .Select(v => new VentanillaAperturadaItem(
                v.Agencia.Nombre, v.Usuario.NombreUsuario, v.Fecha, v.Cerrada, v.Cuadrada))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("reportes/consolidado-transacciones")]
    public async Task<ActionResult<IReadOnlyList<ConsolidadoTransaccionCajaItem>>> ReporteConsolidadoTransacciones(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var desdeUtc = new DateTimeOffset(desde.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var hastaUtc = new DateTimeOffset(hasta.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var movimientos = await db.VentanillasItemCajaMovimiento
            .Include(m => m.VentanillaItemCaja).ThenInclude(v => v.Ventanilla).ThenInclude(v => v.Usuario)
            .Where(m => m.Fecha >= desdeUtc && m.Fecha <= hastaUtc)
            .Select(m => new
            {
                Cajero = m.VentanillaItemCaja.Ventanilla.Usuario.NombreUsuario,
                Fecha = m.Fecha,
                m.Valor,
            })
            .ToListAsync(cancellationToken);

        var resultado = movimientos
            .GroupBy(m => new { m.Cajero, Fecha = DateOnly.FromDateTime(m.Fecha.Date) })
            .Select(g => new ConsolidadoTransaccionCajaItem(
                g.Key.Cajero, g.Key.Fecha, g.Count(),
                g.Where(m => m.Valor > 0).Sum(m => m.Valor),
                g.Where(m => m.Valor < 0).Sum(m => -m.Valor)))
            .OrderBy(x => x.Fecha).ThenBy(x => x.Cajero)
            .ToList();

        return Ok(resultado);
    }

    [HttpGet("formas-numeradas/tipos")]
    public async Task<ActionResult<IReadOnlyList<TipoFormaNumeradaListItem>>> TiposFormaNumerada(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposFormaNumerada
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoFormaNumeradaListItem(t.Codigo, t.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("formas-numeradas")]
    public async Task<ActionResult<IReadOnlyList<FormaNumeradaListItem>>> FormasNumeradas(CancellationToken cancellationToken)
    {
        var resultado = await db.FormasNumeradas
            .Include(f => f.Agencia).Include(f => f.Tipo)
            .OrderByDescending(f => f.FechaAsignacion)
            .Select(f => new FormaNumeradaListItem(
                f.Id, f.IdAgencia, f.Agencia.Nombre, f.CodigoTipo, f.Tipo.Nombre, f.Inicio, f.Fin,
                f.FechaAsignacion, f.RegistradoPor, f.Activo))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("formas-numeradas")]
    public async Task<ActionResult> CrearFormaNumerada([FromBody] FormaNumeradaBody body, CancellationToken cancellationToken)
    {
        if (body.Fin <= body.Inicio)
        {
            throw new SolicitudInvalidaExceptionGenerica("El número final debe ser mayor al inicial");
        }

        if (!await db.TiposFormaNumerada.AnyAsync(t => t.Codigo == body.CodigoTipo && t.Activo, cancellationToken))
        {
            throw new SolicitudInvalidaExceptionGenerica($"El tipo de forma numerada '{body.CodigoTipo}' no existe o está inactivo");
        }

        var forma = new FormaNumerada
        {
            Id = Guid.NewGuid(),
            IdAgencia = body.IdAgencia,
            CodigoTipo = body.CodigoTipo,
            Inicio = body.Inicio,
            Fin = body.Fin,
            FechaAsignacion = DateOnly.FromDateTime(DateTime.UtcNow),
            RegistradoPor = User.Identity!.Name!,
            Activo = true,
        };
        db.FormasNumeradas.Add(forma);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/cajas/formas-numeradas/{forma.Id}", new { forma.Id });
    }

    [HttpPatch("formas-numeradas/{id:guid}/estado")]
    public async Task<IActionResult> ActualizarEstadoFormaNumerada(
        Guid id, [FromBody] ActualizarEstadoFormaNumeradaBody body, CancellationToken cancellationToken)
    {
        var forma = await db.FormasNumeradas.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (forma is null)
        {
            return NotFound();
        }

        forma.Activo = body.Activo;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public record AbrirVentanillaBody(int IdAgencia);

public record CerrarVentanillaBody(decimal TotalEfectivoContado, decimal TotalCheque);

public record RechazarAutorizacionBody(string Comentario);

public record PagoExternoProductoListItem(int Id, string Nombre, string TituloReferencia, bool RequiereDatosFactura, bool TieneComision);

public record PagoExternoTransaccionListItem(
    Guid Id, string Producto, string Referencia, string? Documento, decimal Valor, decimal Comision,
    DateTimeOffset FechaProceso, bool Reversada, string RegistradoPor);

public record RegistrarPagoExternoBody(int IdProducto, string Referencia, string? Documento, decimal Valor, decimal Comision);

public record TipoFormaNumeradaListItem(string Codigo, string Nombre);

public record FormaNumeradaListItem(
    Guid Id, int IdAgencia, string Agencia, string CodigoTipo, string Tipo, int Inicio, int Fin,
    DateOnly FechaAsignacion, string RegistradoPor, bool Activo);

public record FormaNumeradaBody(int IdAgencia, string CodigoTipo, int Inicio, int Fin);

public record ActualizarEstadoFormaNumeradaBody(bool Activo);

public record BovedaSaldoItem(string CodigoItem, string NombreItem, string CodigoMoneda, decimal Saldo);

public record BovedaListItem(
    Guid Id, int IdAgencia, string Agencia, Guid IdUsuarioResponsable, string UsuarioResponsable,
    decimal ExistenciaMinima, decimal ExistenciaMaxima, decimal ExistenciaMinimaCaja, decimal ExistenciaMaximaCaja,
    decimal ExistenciaMinimaCajaChica, decimal ExistenciaMaximaCajaChica, bool Activa, IReadOnlyList<BovedaSaldoItem> Saldos);

public record BovedaBody(
    int IdAgencia, Guid IdUsuarioResponsable, decimal ExistenciaMinima, decimal ExistenciaMaxima,
    decimal ExistenciaMinimaCaja, decimal ExistenciaMaximaCaja, decimal ExistenciaMinimaCajaChica, decimal ExistenciaMaximaCajaChica);

public record BovedaActualizarBody(
    Guid IdUsuarioResponsable, decimal ExistenciaMinima, decimal ExistenciaMaxima,
    decimal ExistenciaMinimaCaja, decimal ExistenciaMaximaCaja, decimal ExistenciaMinimaCajaChica,
    decimal ExistenciaMaximaCajaChica, bool Activa);

public record AjustarSaldoBovedaBody(int IdItemBoveda, int IdMoneda, decimal NuevoSaldo);

public record ItemBovedaListItem(int Id, string Codigo, string Nombre);

public record VentanillaAperturadaItem(string Agencia, string Cajero, DateOnly Fecha, bool Cerrada, bool Cuadrada);

public record ConsolidadoTransaccionCajaItem(string Cajero, DateOnly Fecha, int NumeroTransacciones, decimal TotalIngresos, decimal TotalEgresos);
