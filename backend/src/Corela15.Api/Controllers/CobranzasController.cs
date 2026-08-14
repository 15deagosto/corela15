using Corela15.Application.Cobranza;
using Corela15.Domain.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record AccionGestionListItem(int Id, string Codigo, string Nombre);

public record GestionCobranzaListItem(
    Guid Id, string PrestamoNumero, string Socio, string Accion, bool TieneCompromisoPago, string? Observacion, DateOnly Fecha);

public record PrestamoParaCobranzaListItem(Guid Id, Guid IdCliente, string Numero, string Socio, decimal Saldo, string Estado);

[ApiController]
[Route("api/cobranzas")]
[Authorize(Policy = "Menu:cobranzas-cumplimiento")]
public class CobranzasController(Corela15DbContext db, IGestionCobranzaService gestionService) : ControllerBase
{
    [HttpGet("acciones")]
    public async Task<ActionResult<IReadOnlyList<AccionGestionListItem>>> Acciones(CancellationToken cancellationToken)
    {
        var resultado = await db.AccionesGestion
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombre)
            .Select(a => new AccionGestionListItem(a.Id, a.Codigo, a.Nombre))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("prestamos")]
    public async Task<ActionResult<IReadOnlyList<PrestamoParaCobranzaListItem>>> PrestamosVigentes(CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Where(p => p.Estado == EstadoPrestamo.Vigente)
            .Select(p => new PrestamoParaCobranzaListItem(
                p.Id,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == p.Id && pc.Principal).Select(pc => pc.IdCliente).FirstOrDefault(),
                p.Numero,
                db.PrestamosClientes
                    .Where(pc => pc.IdPrestamo == p.Id && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre)
                    .FirstOrDefault() ?? "—",
                p.Saldo, p.Estado.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("gestiones")]
    public async Task<ActionResult<IReadOnlyList<GestionCobranzaListItem>>> Gestiones(CancellationToken cancellationToken)
    {
        var resultado = await db.GestionesPrestamoCobranza
            .Include(g => g.Prestamo)
            .Include(g => g.Cliente).ThenInclude(c => c.Persona)
            .Include(g => g.AccionGestion)
            .OrderByDescending(g => g.Fecha)
            .Select(g => new GestionCobranzaListItem(
                g.Id, g.Prestamo.Numero, g.Cliente.Persona.Nombre, g.AccionGestion.Nombre,
                g.TieneCompromisoPago, g.Observacion, g.Fecha))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("gestiones")]
    public async Task<ActionResult<GestionCobranzaRegistradaResult>> Registrar(
        [FromBody] RegistrarGestionBody body, CancellationToken cancellationToken)
    {
        var resultado = await gestionService.RegistrarAsync(
            new RegistrarGestionCobranzaRequest(
                body.IdPrestamo, body.IdCliente, body.EsDeudor, body.CodigoAccionGestion,
                body.TieneCompromisoPago, body.Observacion, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/cobranzas/gestiones/{resultado.IdGestion}", resultado);
    }
}

public record RegistrarGestionBody(
    Guid IdPrestamo, Guid IdCliente, bool EsDeudor, string CodigoAccionGestion,
    bool TieneCompromisoPago, string? Observacion);
