using Corela15.Api.Idempotencia;
using Corela15.Application.Planificacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record BloqueBody(int DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, string CodigoEtiqueta, string Descripcion, Guid? IdDisposicion);

public record IndicadorBody(int IdIndicador, decimal? Meta, decimal? Real, string? Comentario);

public record GuardarPlanBody(
    string CodigoArea, DateOnly FechaInicioSemana,
    string NombreResponsable, string CargoResponsable,
    IReadOnlyList<BloqueBody> Bloques,
    IReadOnlyList<IndicadorBody>? Indicadores);

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
            body.Bloques.Select(b => new BloqueRequest(b.DiaSemana, b.HoraInicio, b.HoraFin, b.CodigoEtiqueta, b.Descripcion, b.IdDisposicion)).ToList(),
            (body.Indicadores ?? []).Select(i => new IndicadorRequest(i.IdIndicador, i.Meta, i.Real, i.Comentario)).ToList(),
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

    [HttpPost("planes/{id:guid}/enviar")]
    public async Task<ActionResult<PlanSemanalDto>> Enviar(Guid id, CancellationToken cancellationToken)
        => Ok(await service.EnviarAsync(id, User.Identity!.Name!, EsGerencia, cancellationToken));

    public record NotaBody(string? Nota);

    /// <summary>Solo gerencia -- nota general sobre toda la semana.</summary>
    [HttpPost("planes/{id:guid}/nota-gerencia")]
    [Authorize(Policy = "Menu:planificacion-gerencia")]
    public async Task<ActionResult<PlanSemanalDto>> GuardarNotaGerencia(Guid id, NotaBody body, CancellationToken cancellationToken)
        => Ok(await service.GuardarNotaGerenciaAsync(id, body.Nota, cancellationToken));

    /// <summary>Solo gerencia -- nota puntual sobre un bloque (una hora específica).</summary>
    [HttpPost("bloques/{idBloque:guid}/nota-gerencia")]
    [Authorize(Policy = "Menu:planificacion-gerencia")]
    public async Task<ActionResult<PlanSemanalDto>> GuardarNotaBloque(Guid idBloque, NotaBody body, CancellationToken cancellationToken)
        => Ok(await service.GuardarNotaBloqueAsync(idBloque, body.Nota, cancellationToken));

    /// <summary>Solo gerencia -- aprueba el plan tal como está (exige que ya esté enviado).</summary>
    [HttpPost("planes/{id:guid}/aprobar")]
    [Authorize(Policy = "Menu:planificacion-gerencia")]
    public async Task<ActionResult<PlanSemanalDto>> Aprobar(Guid id, CancellationToken cancellationToken)
        => Ok(await service.AprobarAsync(id, User.Identity!.Name!, cancellationToken));

    public record SolicitarCambiosBody(string Comentario);

    /// <summary>Solo gerencia -- devuelve el plan con observaciones (comentario obligatorio).</summary>
    [HttpPost("planes/{id:guid}/solicitar-cambios")]
    [Authorize(Policy = "Menu:planificacion-gerencia")]
    public async Task<ActionResult<PlanSemanalDto>> SolicitarCambios(Guid id, SolicitarCambiosBody body, CancellationToken cancellationToken)
        => Ok(await service.SolicitarCambiosAsync(id, User.Identity!.Name!, body.Comentario, cancellationToken));

    [HttpGet("indicadores")]
    public async Task<ActionResult<IReadOnlyList<object>>> Indicadores([FromQuery] string? codigoArea, CancellationToken cancellationToken)
    {
        codigoArea = await ResolverEspacioConsultaAsync(codigoArea, cancellationToken);
        var query = db.Indicadores.Where(i => i.Activo);
        if (!string.IsNullOrWhiteSpace(codigoArea))
            query = query.Where(i => i.CodigoArea == codigoArea);

        return Ok(await query.OrderBy(i => i.Nombre).Select(i => new { i.Id, i.CodigoArea, i.Nombre, i.Unidad }).ToListAsync(cancellationToken));
    }

    public record ObtenerOCrearIndicadorBody(string CodigoArea, string Nombre, string? Unidad);

    /// <summary>Combo editable real para KPIs -- ver <see cref="IPlanificacionService.ObtenerOCrearIndicadorAsync"/>.</summary>
    [HttpPost("indicadores/obtener-o-crear")]
    public async Task<ActionResult<IndicadorDto>> ObtenerOCrearIndicador(ObtenerOCrearIndicadorBody body, CancellationToken cancellationToken)
    {
        await service.ValidarDuenoEspacioAsync(body.CodigoArea, User.Identity!.Name!, cancellationToken);
        return Ok(await service.ObtenerOCrearIndicadorAsync(body.CodigoArea, body.Nombre, body.Unidad, cancellationToken));
    }

    /// <summary>HoraLimite como texto ("10:00" o "10:00:00"): el input de hora del navegador manda "HH:mm" y TimeOnly de System.Text.Json solo acepta "HH:mm:ss".</summary>
    public record CrearDisposicionBody(string Titulo, string? Descripcion, DateOnly FechaLimite, string? HoraLimite, IReadOnlyList<string> CodigosArea);

    private static TimeOnly? ParsearHora(string? hora)
    {
        if (string.IsNullOrWhiteSpace(hora)) return null;
        if (TimeOnly.TryParse(hora, System.Globalization.CultureInfo.InvariantCulture, out var valor)) return valor;
        throw new BloqueHorarioInvalidoException($"La hora límite '{hora}' no es válida (usa HH:mm)");
    }

    /// <summary>Solo gerencia -- crea una disposición real con plazo, asignada a una o más áreas.</summary>
    [HttpPost("disposiciones")]
    [Authorize(Policy = "Menu:planificacion-gerencia")]
    public async Task<ActionResult<DisposicionGerenciaDto>> CrearDisposicion(CrearDisposicionBody body, CancellationToken cancellationToken)
        => Ok(await service.CrearDisposicionAsync(
            new CrearDisposicionRequest(body.Titulo, body.Descripcion, body.FechaLimite, ParsearHora(body.HoraLimite), body.CodigosArea, User.Identity!.Name!),
            cancellationToken));

    /// <summary>Solo gerencia -- cierra una disposición.</summary>
    [HttpPost("disposiciones/{id:guid}/cerrar")]
    [Authorize(Policy = "Menu:planificacion-gerencia")]
    public async Task<IActionResult> CerrarDisposicion(Guid id, CancellationToken cancellationToken)
    {
        await service.CerrarDisposicionAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Gerencia ve todas (o solo las de un área puntual, cualquier estado); un área normal ve solo las abiertas que tiene asignadas (para poder etiquetar bloques con ellas).</summary>
    [HttpGet("disposiciones")]
    public async Task<ActionResult<IReadOnlyList<DisposicionGerenciaDto>>> Disposiciones([FromQuery] string? codigoArea, CancellationToken cancellationToken)
        => Ok(await service.ListarDisposicionesAsync(EsGerencia, await ResolverEspacioConsultaAsync(codigoArea, cancellationToken), cancellationToken));

    public record CalificarCumplimientoBody(string Cumplimiento, string? Comentario);

    /// <summary>Solo gerencia -- califica si esta área concreta cumplió o no la disposición.</summary>
    [HttpPost("disposiciones/{id:guid}/areas/{codigoArea}/calificar")]
    [Authorize(Policy = "Menu:planificacion-gerencia")]
    public async Task<ActionResult<DisposicionGerenciaDto>> CalificarCumplimiento(Guid id, string codigoArea, CalificarCumplimientoBody body, CancellationToken cancellationToken)
        => Ok(await service.CalificarCumplimientoAsync(id, codigoArea, body.Cumplimiento, body.Comentario, User.Identity!.Name!, cancellationToken));

    public class SubirEntregableBody
    {
        public IFormFile Archivo { get; set; } = null!;
        public string? Comentario { get; set; }
    }

    /// <summary>La persona asignada sube un entregable de la disposición (multipart: archivo + comentario opcional).</summary>
    [HttpPost("disposiciones/{id:guid}/areas/{codigoArea}/entregables")]
    [RequireIdempotencyKey]
    [RequestSizeLimit(25_000_000)]
    public async Task<ActionResult<DisposicionGerenciaDto>> SubirEntregable(Guid id, string codigoArea, [FromForm] SubirEntregableBody body, CancellationToken cancellationToken)
    {
        if (body.Archivo is null) throw new ArchivoEntregableInvalidoException("Falta el archivo");
        await using var contenido = body.Archivo.OpenReadStream();
        return Ok(await service.SubirEntregableAsync(id, codigoArea,
            new EntregableArchivoEntrada(contenido, body.Archivo.FileName, body.Archivo.ContentType, body.Archivo.Length),
            body.Comentario, User.Identity!.Name!, cancellationToken));
    }

    /// <summary>Descarga/vista previa -- gerencia (y asistente de gerencia) cualquiera; la persona asignada, solo los suyos.</summary>
    [HttpGet("entregables/{idEntregable:guid}")]
    public async Task<IActionResult> DescargarEntregable(Guid idEntregable, CancellationToken cancellationToken)
    {
        var archivo = await service.DescargarEntregableAsync(idEntregable, User.Identity!.Name!, EsGerencia, cancellationToken);
        return File(archivo.Contenido, string.IsNullOrWhiteSpace(archivo.ContentType) ? "application/octet-stream" : archivo.ContentType, archivo.NombreArchivo);
    }

    /// <summary>La persona asignada quita un entregable suyo (queda inactivo, no se borra).</summary>
    [HttpDelete("entregables/{idEntregable:guid}")]
    public async Task<ActionResult<DisposicionGerenciaDto>> QuitarEntregable(Guid idEntregable, CancellationToken cancellationToken)
        => Ok(await service.QuitarEntregableAsync(idEntregable, User.Identity!.Name!, cancellationToken));

    /// <summary>
    /// Personas con planificación -- sin gerencia, solo la propia; con
    /// gerencia, todas las personas con acceso al módulo. (La ruta se llama
    /// "areas" por compatibilidad: cada "área" es hoy el espacio personal
    /// de un usuario.)
    /// </summary>
    [HttpGet("areas")]
    public async Task<ActionResult<IReadOnlyList<EspacioPlanificacionDto>>> Areas(CancellationToken cancellationToken)
        => Ok(await service.ListarEspaciosAsync(EsGerencia, User.Identity!.Name!, cancellationToken));

    /// <summary>Espacio de planificación propio -- se crea solo la primera vez.</summary>
    [HttpGet("mi-espacio")]
    public async Task<ActionResult<EspacioPlanificacionDto>> MiEspacio(CancellationToken cancellationToken)
        => Ok(await service.ObtenerMiEspacioAsync(User.Identity!.Name!, cancellationToken));

    /// <summary>Sin gerencia, siempre el espacio propio (nunca leer catálogos de otra persona).</summary>
    private async Task<string?> ResolverEspacioConsultaAsync(string? codigoArea, CancellationToken cancellationToken)
    {
        if (EsGerencia) return codigoArea;
        return (await service.ObtenerMiEspacioAsync(User.Identity!.Name!, cancellationToken)).Codigo;
    }

    [HttpGet("etiquetas")]
    public async Task<ActionResult<IReadOnlyList<object>>> Etiquetas([FromQuery] string? codigoArea, CancellationToken cancellationToken)
    {
        codigoArea = await ResolverEspacioConsultaAsync(codigoArea, cancellationToken);
        var query = db.EtiquetasPlanificacion.Where(e => e.Activo);
        if (!string.IsNullOrWhiteSpace(codigoArea))
            query = query.Where(e => e.CodigoArea == codigoArea);

        return Ok(await query.OrderBy(e => e.Nombre).Select(e => new { e.Codigo, e.Nombre, e.ColorHex }).ToListAsync(cancellationToken));
    }

    public record ObtenerOCrearEtiquetaBody(string CodigoArea, string Nombre);

    /// <summary>Combo editable real -- ver <see cref="IPlanificacionService.ObtenerOCrearEtiquetaAsync"/>.</summary>
    [HttpPost("etiquetas/obtener-o-crear")]
    public async Task<ActionResult<EtiquetaDto>> ObtenerOCrearEtiqueta(ObtenerOCrearEtiquetaBody body, CancellationToken cancellationToken)
    {
        await service.ValidarDuenoEspacioAsync(body.CodigoArea, User.Identity!.Name!, cancellationToken);
        return Ok(await service.ObtenerOCrearEtiquetaAsync(body.CodigoArea, body.Nombre, cancellationToken));
    }

    public record ActualizarEtiquetaBody(string Nombre, string ColorHex);

    /// <summary>Renombrar/recolorear una etiqueta real desde el propio combo -- ver <see cref="IPlanificacionService.ActualizarEtiquetaAsync"/>.</summary>
    [HttpPut("etiquetas/{codigo}")]
    public async Task<ActionResult<EtiquetaDto>> ActualizarEtiqueta(string codigo, ActualizarEtiquetaBody body, CancellationToken cancellationToken)
        => Ok(await service.ActualizarEtiquetaAsync(codigo, body.Nombre, body.ColorHex, User.Identity!.Name!, cancellationToken));
}
