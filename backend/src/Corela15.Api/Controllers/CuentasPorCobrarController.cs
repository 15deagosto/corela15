using Corela15.Application.CuentasPorCobrar;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record PersonaParaCxCListItem(Guid Id, string Nombre);

public record CuentaPorCobrarListItem(
    Guid Id, string Concepto, string Persona, decimal MontoInicial, decimal Saldo,
    DateOnly FechaCreacion, DateOnly FechaVencimiento, string Estado);

[ApiController]
[Route("api/tesoreria/cuentas-por-cobrar")]
[Authorize(Policy = "Menu:tesoreria")]
public class CuentasPorCobrarController(Corela15DbContext db, ICuentaPorCobrarService service) : ControllerBase
{
    [HttpGet("personas")]
    public async Task<ActionResult<IReadOnlyList<PersonaParaCxCListItem>>> Personas(CancellationToken cancellationToken)
    {
        var resultado = await db.Personas
            .OrderBy(p => p.Nombre)
            .Select(p => new PersonaParaCxCListItem(p.Id, p.Nombre))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CuentaPorCobrarListItem>>> Listar(CancellationToken cancellationToken)
    {
        var resultado = await db.CuentasPorCobrar
            .Include(c => c.Persona)
            .OrderByDescending(c => c.FechaCreacion)
            .Select(c => new CuentaPorCobrarListItem(
                c.Id, c.Concepto, c.Persona.Nombre, c.MontoInicial, c.Saldo,
                c.FechaCreacion, c.FechaVencimiento, c.Estado.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<CuentaPorCobrarRegistradaResult>> Registrar(
        [FromBody] RegistrarCuentaPorCobrarBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.RegistrarAsync(
            new RegistrarCuentaPorCobrarRequest(
                body.Concepto, body.IdAgencia, body.IdPersona, body.Cuotas, body.MontoInicial,
                body.FechaVencimiento, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/tesoreria/cuentas-por-cobrar/{resultado.IdCuentaPorCobrar}", resultado);
    }

    [HttpPost("{id:guid}/abonos")]
    public async Task<ActionResult<AbonoCuentaPorCobrarRegistradoResult>> Abonar(
        Guid id, [FromBody] AbonarCuentaPorCobrarBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.AbonarAsync(
            new AbonarCuentaPorCobrarRequest(id, body.Monto, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }
}

public record RegistrarCuentaPorCobrarBody(
    string Concepto, int IdAgencia, Guid IdPersona, int Cuotas, decimal MontoInicial, DateOnly FechaVencimiento);

public record AbonarCuentaPorCobrarBody(decimal Monto);
