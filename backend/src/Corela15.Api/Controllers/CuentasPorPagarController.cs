using Corela15.Api.Idempotencia;
using Corela15.Application.CuentasPorCobrar;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record CuentaPorPagarListItem(
    Guid Id, string Concepto, string Persona, int Cuotas, decimal MontoInicial, decimal Saldo,
    DateOnly FechaCreacion, DateOnly FechaVencimiento, string Estado);

public record FormaCancelacionListItem(string Codigo, string Nombre, bool Activo);

[ApiController]
[Route("api/tesoreria/cuentas-por-pagar")]
[Authorize(Policy = "Menu:tesoreria")]
public class CuentasPorPagarController(Corela15DbContext db, ICuentaPorPagarService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CuentaPorPagarListItem>>> Listar(CancellationToken cancellationToken)
    {
        var resultado = await db.CuentasPorPagar
            .Include(c => c.Persona)
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new CuentaPorPagarListItem(
                c.Id, c.Concepto, c.Persona.Nombre, c.Cuotas, c.MontoInicial, c.Saldo,
                c.FechaCreacion, c.FechaVencimiento, c.Estado.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("formas-cancelacion")]
    public async Task<ActionResult<IReadOnlyList<FormaCancelacionListItem>>> FormasCancelacion(CancellationToken cancellationToken)
    {
        var resultado = await db.FormasCancelacion
            .OrderBy(f => f.Codigo)
            .Select(f => new FormaCancelacionListItem(f.Codigo, f.Nombre, f.Activo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost]
    [RequireIdempotencyKey]
    public async Task<ActionResult<CuentaPorPagarRegistradaResult>> Registrar(
        [FromBody] RegistrarCuentaPorPagarBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.RegistrarAsync(
            new RegistrarCuentaPorPagarRequest(
                body.Concepto, body.IdAgencia, body.IdPersona, body.Cuotas, body.MontoInicial,
                body.FechaVencimiento, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/tesoreria/cuentas-por-pagar/{resultado.IdCuentaPorPagar}", resultado);
    }

    [HttpPost("{id:guid}/pagos")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PagoCuentaPorPagarRegistradoResult>> Pagar(
        Guid id, [FromBody] PagarCuentaPorPagarBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.PagarAsync(
            new PagarCuentaPorPagarRequest(id, body.Monto, body.CodigoFormaCancelacion, User.Identity!.Name!),
            cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/anular")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<CuentaPorPagarAnuladaResult>> Anular(
        Guid id, [FromBody] AnularCuentaPorPagarBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.AnularAsync(
            new AnularCuentaPorPagarRequest(id, body.Motivo, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }
}

public record RegistrarCuentaPorPagarBody(
    string Concepto, int IdAgencia, Guid IdPersona, int Cuotas, decimal MontoInicial, DateOnly FechaVencimiento);

public record PagarCuentaPorPagarBody(decimal Monto, string CodigoFormaCancelacion);

public record AnularCuentaPorPagarBody(string Motivo);
