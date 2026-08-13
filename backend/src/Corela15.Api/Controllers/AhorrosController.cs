using Corela15.Application.Ahorros;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record ProductoAhorroListItem(
    int Id, string Codigo, string Nombre, bool PermiteDebitoPrestamo, bool Activo);

public record CuentaAhorroListItem(
    Guid Id, string Numero, string Producto, string Agencia, string Estado, DateOnly FechaApertura, decimal SaldoDisponible);

[ApiController]
[Route("api/ahorros")]
public class AhorrosController(Corela15DbContext db, ICuentaAhorroService cuentaAhorroService) : ControllerBase
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
                    .FirstOrDefault()))
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
            .Select(t => new TipoTransaccionListItem(t.Codigo, t.Nombre, t.SignoSaldoCuenta))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("cuentas")]
    public async Task<ActionResult<CuentaAhorroAbiertaResult>> AbrirCuenta(
        [FromBody] AbrirCuentaAhorroRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await cuentaAhorroService.AbrirAsync(request, cancellationToken);
            return Created($"/api/ahorros/cuentas/{resultado.IdCuenta}", resultado);
        }
        catch (TipoCuentaInvalidoException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (ClienteInvalidoException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }

    [HttpPost("cuentas/{idCuenta:guid}/movimientos")]
    public async Task<ActionResult<MovimientoCuentaRegistradoResult>> RegistrarMovimiento(
        Guid idCuenta, [FromBody] RegistrarMovimientoBody body, CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await cuentaAhorroService.RegistrarMovimientoAsync(
                new RegistrarMovimientoCuentaRequest(idCuenta, body.CodigoTipoTransaccion, body.Monto, body.RegistradoPor),
                cancellationToken);
            return Ok(resultado);
        }
        catch (CuentaInvalidaException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (TipoTransaccionInvalidoException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (SaldoInsuficienteException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (ArgumentException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}

public record RegistrarMovimientoBody(string CodigoTipoTransaccion, decimal Monto, string RegistradoPor);

public record TipoTransaccionListItem(string Codigo, string Nombre, int SignoSaldoCuenta);
