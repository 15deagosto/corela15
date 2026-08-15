using Corela15.Application.Cobranza;
using Corela15.Application.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record AccionGestionListItem(int Id, string Codigo, string Nombre);

public record GestionCobranzaListItem(
    Guid Id, string PrestamoNumero, string Socio, string Accion, bool TieneCompromisoPago, string? Observacion, DateOnly Fecha);

public record PrestamoParaCobranzaListItem(
    Guid Id, Guid IdCliente, string Numero, string Socio, decimal Saldo, string Estado,
    int DiasMora, string CodigoPeriodoMora, string NombrePeriodoMora);

public record ResumenMoraTramoItem(string Codigo, string Nombre, int CantidadPrestamos, decimal SaldoTotal);

[ApiController]
[Route("api/cobranzas")]
[Authorize(Policy = "Menu:cobranzas-cumplimiento")]
public class CobranzasController(Corela15DbContext db, IGestionCobranzaService gestionService, IMoraCarteraService moraCartera) : ControllerBase
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

    // Solo préstamos con mora real (DiasMora > 0) — antes mostraba TODOS
    // los préstamos vigentes por igual, sin distinguir cuáles realmente
    // necesitan gestión de cobranza. El tramo (PeriodoMora, sembrado desde
    // Nivel 4 pero sin usar hasta ahora) sale del mismo cálculo real de
    // mora que usa el motor de provisiones — una sola fuente de verdad.
    [HttpGet("prestamos")]
    public async Task<ActionResult<IReadOnlyList<PrestamoParaCobranzaListItem>>> PrestamosVigentes(CancellationToken cancellationToken)
    {
        var moras = await moraCartera.CalcularAsync(cancellationToken);
        var tramos = await db.PeriodosMora
            .Where(t => t.Activo)
            .OrderBy(t => t.DiasInicio)
            .ToListAsync(cancellationToken);

        var titulares = await db.PrestamosClientes
            .Where(pc => pc.Principal)
            .Select(pc => new { pc.IdPrestamo, pc.IdCliente, Nombre = pc.Cliente.Persona.Nombre })
            .ToDictionaryAsync(x => x.IdPrestamo, cancellationToken);

        var resultado = moras
            .Where(m => m.DiasMora > 0)
            .OrderByDescending(m => m.DiasMora)
            .Select(m =>
            {
                var tramo = tramos.FirstOrDefault(t => m.DiasMora >= t.DiasInicio && m.DiasMora <= t.DiasFin) ?? tramos[^1];
                titulares.TryGetValue(m.IdPrestamo, out var titular);
                return new PrestamoParaCobranzaListItem(
                    m.IdPrestamo, titular?.IdCliente ?? Guid.Empty, m.Numero, titular?.Nombre ?? "—",
                    m.Saldo, "Vigente", m.DiasMora, tramo.Codigo, tramo.Nombre);
            })
            .ToList();

        return Ok(resultado);
    }

    [HttpGet("mora/resumen")]
    public async Task<ActionResult<IReadOnlyList<ResumenMoraTramoItem>>> ResumenMora(CancellationToken cancellationToken)
    {
        var moras = await moraCartera.CalcularAsync(cancellationToken);
        var tramos = await db.PeriodosMora
            .Where(t => t.Activo)
            .OrderBy(t => t.DiasInicio)
            .ToListAsync(cancellationToken);

        var resultado = tramos
            .Select(t =>
            {
                var enTramo = moras.Where(m => m.DiasMora >= t.DiasInicio && m.DiasMora <= t.DiasFin).ToList();
                return new ResumenMoraTramoItem(t.Codigo, t.Nombre, enTramo.Count, enTramo.Sum(m => m.Saldo));
            })
            .ToList();

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
