using Corela15.Application.Sujeto;
using Corela15.Domain.Sujeto;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record CodigoNombreItem(string Codigo, string Nombre);

public record TipoProductoReclamoItem(int Id, string Codigo, string Nombre);

public record ConceptoReclamoDetalleItem(string Codigo, string CodigoConcepto, string Concepto, string Descripcion);

public record ReclamoRespuestaItem(
    string CodigoTipoResolucion, string TipoResolucion, decimal? MontoRestituido, decimal? InteresSobreMonto,
    string Descripcion, DateTimeOffset CreadoEn, string RegistradoPor);

public record ReclamoItem(
    Guid Id, Guid IdPersona, string Persona, string Identificacion, string CodigoCanalRecepcion, string CanalRecepcion,
    DateOnly FechaRecepcion, int IdTipoProducto, string TipoProducto, string CodigoConceptoDetalle, string ConceptoDetalle,
    string CodigoEstado, string Estado, string Descripcion, DateTimeOffset CreadoEn, string RegistradoPor,
    ReclamoRespuestaItem? Respuesta);

public record RegistrarReclamoBody(
    Guid IdPersona, string CodigoCanalRecepcion, int IdTipoProducto, string CodigoConceptoDetalle, string Descripcion);

public record ResponderReclamoBody(
    string CodigoTipoResolucion, decimal? MontoRestituido, decimal? InteresSobreMonto, string Descripcion);

public record MiembroOrganoGobiernoItem(
    Guid Id, Guid IdPersona, string Persona, string Identificacion,
    bool EsAsambleaGeneral, DateOnly? FechaIniciaAsambleaGeneral, DateOnly? FechaTerminaAsambleaGeneral,
    bool EsConsejoAdministracion, DateOnly? FechaIniciaConsejoAdministracion, DateOnly? FechaTerminaConsejoAdministracion,
    bool EsConsejoVigilancia, DateOnly? FechaIniciaConsejoVigilancia, DateOnly? FechaTerminaConsejoVigilancia,
    bool Activo);

public record RegistrarMiembroOrganoGobiernoBody(
    Guid IdPersona,
    bool EsAsambleaGeneral, DateOnly? FechaIniciaAsambleaGeneral, DateOnly? FechaTerminaAsambleaGeneral,
    bool EsConsejoAdministracion, DateOnly? FechaIniciaConsejoAdministracion, DateOnly? FechaTerminaConsejoAdministracion,
    bool EsConsejoVigilancia, DateOnly? FechaIniciaConsejoVigilancia, DateOnly? FechaTerminaConsejoVigilancia);

[ApiController]
[Route("api/socios")]
[Authorize(Policy = "Menu:socios")]
public class ReclamoController(Corela15DbContext db, IReclamoService reclamoService) : ControllerBase
{
    [HttpGet("reclamos/canales")]
    public async Task<ActionResult<IReadOnlyList<CodigoNombreItem>>> Canales(CancellationToken cancellationToken) =>
        Ok(await db.CanalesReclamo.Where(c => c.Activo).OrderBy(c => c.Nombre)
            .Select(c => new CodigoNombreItem(c.Codigo, c.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("reclamos/tipos-producto")]
    public async Task<ActionResult<IReadOnlyList<TipoProductoReclamoItem>>> TiposProducto(CancellationToken cancellationToken) =>
        Ok(await db.TiposProductoReclamo.Where(t => t.Activo).OrderBy(t => t.Nombre)
            .Select(t => new TipoProductoReclamoItem(t.Id, t.Codigo, t.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("reclamos/conceptos")]
    public async Task<ActionResult<IReadOnlyList<ConceptoReclamoDetalleItem>>> Conceptos(CancellationToken cancellationToken) =>
        Ok(await db.ConceptosReclamoDetalle.Where(c => c.Activo)
            .OrderBy(c => c.CodigoConcepto).ThenBy(c => c.Codigo)
            .Select(c => new ConceptoReclamoDetalleItem(c.Codigo, c.CodigoConcepto, c.Concepto.Nombre, c.Descripcion))
            .ToListAsync(cancellationToken));

    [HttpGet("reclamos/tipos-resolucion")]
    public async Task<ActionResult<IReadOnlyList<CodigoNombreItem>>> TiposResolucion(CancellationToken cancellationToken) =>
        Ok(await db.TiposResolucionReclamo.Where(t => t.Activo).OrderBy(t => t.Nombre)
            .Select(t => new CodigoNombreItem(t.Codigo, t.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("reclamos")]
    public async Task<ActionResult<IReadOnlyList<ReclamoItem>>> Reclamos(CancellationToken cancellationToken)
    {
        var resultado = await db.Reclamos
            .Include(r => r.Persona)
            .Include(r => r.CanalRecepcion)
            .Include(r => r.TipoProducto)
            .Include(r => r.ConceptoDetalle)
            .Include(r => r.Estado)
            .Include(r => r.Respuesta).ThenInclude(res => res!.TipoResolucion)
            .OrderByDescending(r => r.CreadoEn)
            .Select(r => new ReclamoItem(
                r.Id, r.IdPersona, r.Persona.Nombre, r.Persona.Identificacion,
                r.CodigoCanalRecepcion, r.CanalRecepcion.Nombre, r.FechaRecepcion,
                r.IdTipoProducto, r.TipoProducto.Nombre, r.CodigoConceptoDetalle, r.ConceptoDetalle.Descripcion,
                r.CodigoEstado, r.Estado.Nombre, r.Descripcion, r.CreadoEn, r.RegistradoPor,
                r.Respuesta == null ? null : new ReclamoRespuestaItem(
                    r.Respuesta.CodigoTipoResolucion, r.Respuesta.TipoResolucion.Nombre, r.Respuesta.MontoRestituido,
                    r.Respuesta.InteresSobreMonto, r.Respuesta.Descripcion, r.Respuesta.CreadoEn, r.Respuesta.RegistradoPor)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("reclamos")]
    public async Task<ActionResult<ReclamoRegistradoResult>> RegistrarReclamo(
        [FromBody] RegistrarReclamoBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        var resultado = await reclamoService.RegistrarAsync(
            new RegistrarReclamoRequest(
                body.IdPersona, body.CodigoCanalRecepcion, body.IdTipoProducto, body.CodigoConceptoDetalle,
                body.Descripcion, registradoPor),
            cancellationToken);
        return Created($"/api/socios/reclamos/{resultado.Id}", resultado);
    }

    [HttpPost("reclamos/{id:guid}/responder")]
    public async Task<IActionResult> ResponderReclamo(
        Guid id, [FromBody] ResponderReclamoBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        await reclamoService.ResponderAsync(
            id,
            new ResponderReclamoRequest(body.CodigoTipoResolucion, body.MontoRestituido, body.InteresSobreMonto, body.Descripcion, registradoPor),
            cancellationToken);
        return NoContent();
    }

    // Consejo de Vigilancia / Consejo de Administración / Asamblea General
    // (órganos de gobierno reales y obligatorios en una COAC ecuatoriana)

    [HttpGet("organo-gobierno")]
    public async Task<ActionResult<IReadOnlyList<MiembroOrganoGobiernoItem>>> MiembrosOrganoGobierno(CancellationToken cancellationToken)
    {
        var resultado = await db.MiembrosOrganoGobierno
            .Include(m => m.Persona)
            .OrderByDescending(m => m.Activo).ThenBy(m => m.Persona.Nombre)
            .Select(m => new MiembroOrganoGobiernoItem(
                m.Id, m.IdPersona, m.Persona.Nombre, m.Persona.Identificacion,
                m.EsAsambleaGeneral, m.FechaIniciaAsambleaGeneral, m.FechaTerminaAsambleaGeneral,
                m.EsConsejoAdministracion, m.FechaIniciaConsejoAdministracion, m.FechaTerminaConsejoAdministracion,
                m.EsConsejoVigilancia, m.FechaIniciaConsejoVigilancia, m.FechaTerminaConsejoVigilancia,
                m.Activo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("organo-gobierno")]
    public async Task<ActionResult<MiembroOrganoGobiernoItem>> RegistrarMiembroOrganoGobierno(
        [FromBody] RegistrarMiembroOrganoGobiernoBody body, CancellationToken cancellationToken)
    {
        var persona = await db.Personas.FirstOrDefaultAsync(p => p.Id == body.IdPersona, cancellationToken);
        if (persona is null) return BadRequest(new { detail = $"La persona {body.IdPersona} no existe." });

        var registradoPor = User.Identity?.Name ?? "sistema";
        var miembro = new MiembroOrganoGobierno
        {
            Id = Guid.NewGuid(),
            IdPersona = body.IdPersona,
            EsAsambleaGeneral = body.EsAsambleaGeneral,
            FechaIniciaAsambleaGeneral = body.FechaIniciaAsambleaGeneral,
            FechaTerminaAsambleaGeneral = body.FechaTerminaAsambleaGeneral,
            EsConsejoAdministracion = body.EsConsejoAdministracion,
            FechaIniciaConsejoAdministracion = body.FechaIniciaConsejoAdministracion,
            FechaTerminaConsejoAdministracion = body.FechaTerminaConsejoAdministracion,
            EsConsejoVigilancia = body.EsConsejoVigilancia,
            FechaIniciaConsejoVigilancia = body.FechaIniciaConsejoVigilancia,
            FechaTerminaConsejoVigilancia = body.FechaTerminaConsejoVigilancia,
            Activo = true,
            RegistradoPor = registradoPor,
            CreadoEn = DateTimeOffset.UtcNow,
        };

        db.MiembrosOrganoGobierno.Add(miembro);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/socios/organo-gobierno/{miembro.Id}", new MiembroOrganoGobiernoItem(
            miembro.Id, miembro.IdPersona, persona.Nombre, persona.Identificacion,
            miembro.EsAsambleaGeneral, miembro.FechaIniciaAsambleaGeneral, miembro.FechaTerminaAsambleaGeneral,
            miembro.EsConsejoAdministracion, miembro.FechaIniciaConsejoAdministracion, miembro.FechaTerminaConsejoAdministracion,
            miembro.EsConsejoVigilancia, miembro.FechaIniciaConsejoVigilancia, miembro.FechaTerminaConsejoVigilancia,
            miembro.Activo));
    }

    [HttpPatch("organo-gobierno/{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstadoMiembro(Guid id, [FromBody] bool activo, CancellationToken cancellationToken)
    {
        var miembro = await db.MiembrosOrganoGobierno.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (miembro is null) return NotFound();

        miembro.Activo = activo;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
