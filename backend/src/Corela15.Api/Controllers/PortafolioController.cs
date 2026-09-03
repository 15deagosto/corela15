using Corela15.Api.Idempotencia;
using Corela15.Application.Portafolio;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record InstitucionListItem(string Codigo, string Nombre, string TipoInstitucion, bool Activa);

public record InversionListItem(
    Guid Id, string Documento, string Institucion, decimal ValorNominal, decimal Tasa,
    DateOnly FechaCompra, DateOnly FechaVencimiento, string Estado);

[ApiController]
[Route("api/portafolio")]
[Authorize(Policy = "Menu:portafolio")]
public class PortafolioController(Corela15DbContext db, IInversionPortafolioService inversionService) : ControllerBase
{
    [HttpGet("instituciones")]
    public async Task<ActionResult<IReadOnlyList<InstitucionListItem>>> Instituciones(CancellationToken ct)
    {
        var resultado = await db.Instituciones
            .Include(i => i.TipoInstitucion)
            .Where(i => i.Activa)
            .OrderBy(i => i.Nombre)
            .Select(i => new InstitucionListItem(i.Codigo, i.Nombre, i.TipoInstitucion.Nombre, i.Activa))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("inversiones")]
    public async Task<ActionResult<IReadOnlyList<InversionListItem>>> Inversiones(CancellationToken ct)
    {
        var resultado = await db.InversionesPortafolio
            .Include(i => i.Institucion)
            .OrderByDescending(i => i.FechaCompra)
            .Select(i => new InversionListItem(
                i.Id, i.Documento, i.Institucion.Nombre, i.ValorNominal, i.Tasa, i.FechaCompra, i.FechaVencimiento, i.Estado.ToString()))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("inversiones")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<InversionAbiertaResult>> Abrir([FromBody] AbrirInversionBody body, CancellationToken ct)
    {
        var resultado = await inversionService.AbrirAsync(
            new AbrirInversionRequest(
                body.Documento, body.IdAgencia, body.CodigoInstitucion, body.ValorNominal, body.Tasa,
                body.FechaCompra, body.FechaVencimiento, User.Identity!.Name!),
            ct);
        return Created($"/api/portafolio/inversiones/{resultado.IdInversion}", resultado);
    }

    [HttpPost("inversiones/{id:guid}/cancelar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<InversionCanceladaResult>> Cancelar(Guid id, CancellationToken ct)
    {
        var resultado = await inversionService.CancelarAsync(new CancelarInversionRequest(id, User.Identity!.Name!), ct);
        return Ok(resultado);
    }

    [HttpPost("inversiones/{id:guid}/renovar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<InversionRenovadaResult>> Renovar(Guid id, [FromBody] RenovarInversionBody body, CancellationToken ct)
    {
        var resultado = await inversionService.RenovarAsync(
            new RenovarInversionRequest(id, body.DocumentoNuevo, body.FechaVencimientoNueva, User.Identity!.Name!), ct);
        return Ok(resultado);
    }
}

public record AbrirInversionBody(
    string Documento, int IdAgencia, string CodigoInstitucion, decimal ValorNominal, decimal Tasa,
    DateOnly FechaCompra, DateOnly FechaVencimiento);

public record RenovarInversionBody(string DocumentoNuevo, DateOnly FechaVencimientoNueva);
