using Corela15.Api.Idempotencia;
using Corela15.Application.Inversion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record DepositoListItem(
    Guid Id, string Codigo, string Socio, decimal Monto, decimal Tasa, int PlazoDias,
    DateOnly FechaCreacion, DateOnly FechaVencimiento, string Estado);

[ApiController]
[Route("api/plazofijo")]
[Authorize(Policy = "Menu:creditos")]
public class PlazoFijoController(Corela15DbContext db, IDepositoService depositoService) : ControllerBase
{
    [HttpGet("depositos")]
    public async Task<ActionResult<IReadOnlyList<DepositoListItem>>> Depositos(CancellationToken cancellationToken)
    {
        var resultado = await db.Depositos
            .OrderByDescending(d => d.FechaCreacion)
            .Select(d => new DepositoListItem(
                d.Id, d.Codigo,
                db.DepositosClientes
                    .Where(dc => dc.IdDeposito == d.Id && dc.Principal)
                    .Select(dc => dc.Cliente.Persona.Nombre)
                    .FirstOrDefault() ?? "—",
                d.Monto, d.Tasa, d.PlazoDias, d.FechaCreacion, d.FechaVencimiento, d.Estado.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("depositos")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<DepositoAbiertoResult>> Abrir(
        [FromBody] AbrirDepositoBody body, CancellationToken cancellationToken)
    {
        var resultado = await depositoService.AbrirAsync(
            new AbrirDepositoRequest(body.IdCliente, body.IdAgencia, body.Monto, body.PlazoDias, body.EsPersonaJuridica, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/plazofijo/depositos/{resultado.IdDeposito}", resultado);
    }

    [HttpPost("depositos/{idDeposito:guid}/cancelar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<DepositoCanceladoResult>> Cancelar(
        Guid idDeposito, CancellationToken cancellationToken)
    {
        var resultado = await depositoService.CancelarAsync(
            new CancelarDepositoRequest(idDeposito, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }
}

public record AbrirDepositoBody(Guid IdCliente, int IdAgencia, decimal Monto, int PlazoDias, bool EsPersonaJuridica);
