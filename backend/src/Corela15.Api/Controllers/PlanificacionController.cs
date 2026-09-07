using Corela15.Application.Planificacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record BloqueBody(int DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, string CodigoEtiqueta, string Descripcion);

public record GuardarPlanBody(
    string CodigoArea, DateOnly FechaInicioSemana,
    string NombreResponsable, string CargoResponsable,
    IReadOnlyList<BloqueBody> Bloques);

/// <summary>
/// Planificación semanal real de cada área -- ver <see cref="IPlanificacionService"/>
/// para el modelo de permisos real (rol-gated, nunca universal; visibilidad
/// propia salvo permiso de gerencia).
/// </summary>
[ApiController]
[Route("api/planificacion")]
[Authorize(Policy = "Menu:planificacion")]
public class PlanificacionController(IPlanificacionService service, Corela15DbContext db) : ControllerBase
{
    private bool EsGerencia => User.HasClaim("menu", "planificacion-gerencia");

    [HttpPost("planes")]
    public async Task<ActionResult<PlanSemanalDto>> Guardar(GuardarPlanBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.GuardarAsync(new GuardarPlanSemanalRequest(
            body.CodigoArea, body.FechaInicioSemana, body.NombreResponsable, body.CargoResponsable,
            body.Bloques.Select(b => new BloqueRequest(b.DiaSemana, b.HoraInicio, b.HoraFin, b.CodigoEtiqueta, b.Descripcion)).ToList(),
            User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("planes")]
    public async Task<ActionResult<IReadOnlyList<PlanSemanalListItemDto>>> Listar(
        [FromQuery] string? codigoArea, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken cancellationToken)
    {
        var resultado = await service.ListarAsync(new ListarPlanesFiltro(codigoArea, desde, hasta, User.Identity!.Name!, EsGerencia), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("planes/{id:guid}")]
    public async Task<ActionResult<PlanSemanalDto>> Obtener(Guid id, CancellationToken cancellationToken)
        => Ok(await service.ObtenerAsync(id, User.Identity!.Name!, EsGerencia, cancellationToken));

    [HttpGet("areas")]
    public async Task<ActionResult<IReadOnlyList<object>>> Areas(CancellationToken cancellationToken)
        => Ok(await db.AreasPlanificacion.Where(a => a.Activo).OrderBy(a => a.Nombre)
            .Select(a => new { a.Codigo, a.Nombre }).ToListAsync(cancellationToken));

    [HttpGet("etiquetas")]
    public async Task<ActionResult<IReadOnlyList<object>>> Etiquetas([FromQuery] string? codigoArea, CancellationToken cancellationToken)
    {
        var query = db.EtiquetasPlanificacion.Where(e => e.Activo);
        if (!string.IsNullOrWhiteSpace(codigoArea))
            query = query.Where(e => e.CodigoArea == codigoArea);

        return Ok(await query.OrderBy(e => e.Nombre).Select(e => new { e.Codigo, e.Nombre, e.ColorHex }).ToListAsync(cancellationToken));
    }

    public record ObtenerOCrearEtiquetaBody(string CodigoArea, string Nombre);

    /// <summary>Combo editable real -- ver <see cref="IPlanificacionService.ObtenerOCrearEtiquetaAsync"/>.</summary>
    [HttpPost("etiquetas/obtener-o-crear")]
    public async Task<ActionResult<EtiquetaDto>> ObtenerOCrearEtiqueta(ObtenerOCrearEtiquetaBody body, CancellationToken cancellationToken)
        => Ok(await service.ObtenerOCrearEtiquetaAsync(body.CodigoArea, body.Nombre, cancellationToken));
}
