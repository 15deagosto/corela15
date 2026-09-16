using Corela15.Api.Idempotencia;
using Corela15.Application.Documentos;
using Corela15.Domain.Documentos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

/// <summary>Cuerpo del alta — multipart/form-data, el archivo viaja junto con los metadatos en el mismo request.</summary>
public class CrearDocumentoBody
{
    public string Titulo { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? InstanciaAprobacion { get; set; }
    public string? InstanciaRevision { get; set; }
    public DateOnly? FechaAprobacion { get; set; }
    public DateOnly? ProximaRevision { get; set; }
    public string? Notas { get; set; }
    public IFormFile Archivo { get; set; } = null!;
}

public record ActualizarDocumentoBody(
    string Titulo, string Area, string Tipo, string Estado,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas);

public class NuevaVersionDocumentoBody
{
    public string? Version { get; set; }
    public IFormFile Archivo { get; set; } = null!;
}

public record OtorgarAccesoBody(Guid IdUsuario, string Area, string NivelAcceso);

/// <summary>
/// Biblioteca documental institucional — portada real de CredVault
/// (`apps.documents`, la app externa de gestión de TI donde vivía hasta
/// ahora). Ver <see cref="Corela15.Domain.Documentos.Documento"/> y
/// <see cref="Corela15.Domain.Documentos.AreaAccesoUsuario"/> para el
/// detalle completo del modelo, el ACL real por área, y dónde vive el
/// archivo físico (NAS).
/// </summary>
[ApiController]
[Route("api/biblioteca-documentos")]
[Authorize(Policy = "Menu:biblioteca-documentos")]
public class DocumentosController(IDocumentoService service, IAccesoDocumentalService accesoService) : ControllerBase
{
    /// <summary>ADMINISTRADOR o el menú dedicado ven/administran TODAS las áreas sin ACL — mismo patrón ya usado en Planificación (planificacion-gerencia).</summary>
    private bool VeTodo => User.IsInRole("ADMINISTRADOR") || User.HasClaim("menu", "biblioteca-documentos-gerencia");

    private async Task<ContextoAccesoDocumental> ResolverContextoAsync(CancellationToken cancellationToken)
    {
        if (VeTodo) return new ContextoAccesoDocumental(true, new Dictionary<string, string>());

        var idUsuario = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);
        var accesoPorArea = await accesoService.ObtenerAccesoPorAreaAsync(idUsuario, cancellationToken);
        return new ContextoAccesoDocumental(false, accesoPorArea);
    }

    private void ExigirVeTodo()
    {
        if (!VeTodo) throw new AccesoDocumentalDenegadoException("todas (administración de accesos)");
    }

    /// <summary>
    /// Mi propio ACL real — el frontend lo pide una vez al abrir el módulo
    /// para decidir qué mostrar (botón "Nuevo documento", pestaña "Nueva
    /// versión", qué áreas ofrecer en los selectores), sin depender solo
    /// del 422 del backend al intentar la acción. Nunca se cachea en el
    /// JWT (a diferencia de menú/opción/dataset) -- se resuelve en vivo
    /// contra la base en cada carga, así un cambio de acceso se ve
    /// inmediato, sin re-login.
    /// </summary>
    [HttpGet("mi-acceso")]
    public async Task<ActionResult<object>> MiAcceso(CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        return Ok(new { veTodo = contexto.VeTodo, accesoPorArea = contexto.AccesoPorArea });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentoListItem>>> Listar(
        [FromQuery] string? area, [FromQuery] string? tipo,
        [FromQuery] string? estado, [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        var resultado = await service.ListarAsync(new ListarDocumentosFiltro(area, tipo, estado, q), contexto, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentoListItem>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        var resultado = await service.ObtenerAsync(id, contexto, cancellationToken);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpPost]
    [RequireIdempotencyKey]
    public async Task<ActionResult<object>> Crear([FromForm] CrearDocumentoBody body, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await using var stream = body.Archivo.OpenReadStream();
        var id = await service.CrearAsync(
            new CrearDocumentoRequest(
                body.Titulo, body.Area, body.Tipo, body.Version ?? "1.0",
                body.InstanciaAprobacion, body.InstanciaRevision, body.FechaAprobacion, body.ProximaRevision,
                body.Notas, User.Identity!.Name!),
            stream, body.Archivo.FileName, body.Archivo.ContentType, body.Archivo.Length, contexto, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarDocumentoBody body, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await service.ActualizarAsync(
            id,
            new ActualizarDocumentoRequest(
                body.Titulo, body.Area, body.Tipo, body.Estado,
                body.InstanciaAprobacion, body.InstanciaRevision, body.FechaAprobacion, body.ProximaRevision,
                body.Notas, User.Identity!.Name!),
            contexto, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/nueva-version")]
    [RequireIdempotencyKey]
    public async Task<IActionResult> SubirNuevaVersion(Guid id, [FromForm] NuevaVersionDocumentoBody body, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await using var stream = body.Archivo.OpenReadStream();
        await service.SubirNuevaVersionAsync(
            id, new SubirNuevaVersionRequest(body.Version ?? string.Empty, User.Identity!.Name!),
            stream, body.Archivo.FileName, body.Archivo.ContentType, body.Archivo.Length, contexto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Desactivar(Guid id, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await service.DesactivarAsync(id, User.Identity!.Name!, contexto, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/descargar")]
    public async Task<IActionResult> Descargar(Guid id, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        var resultado = await service.DescargarAsync(id, contexto, cancellationToken);
        return File(resultado.Contenido, string.IsNullOrWhiteSpace(resultado.ContentType) ? "application/octet-stream" : resultado.ContentType, resultado.NombreArchivo);
    }

    [HttpGet("matriz")]
    public async Task<ActionResult<MatrizDocumentalResult>> Matriz(CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        var resultado = await service.ObtenerMatrizAsync(contexto, cancellationToken);
        return Ok(resultado);
    }

    // --- ACL real por área — reservado a quien ve todas las áreas ---
    // Nunca Forbid()/403 acá: en este sistema un 403 SIEMPRE significa
    // "sesión con claims viejos" (ver frontend/src/lib/api.ts, desloguea
    // automático) -- esto es una regla de negocio real, va como 422.

    [HttpGet("accesos")]
    public async Task<ActionResult<IReadOnlyList<AreaAccesoItem>>> ListarAccesos([FromQuery] string? area, CancellationToken cancellationToken)
    {
        ExigirVeTodo();
        return Ok(await accesoService.ListarAsync(area, cancellationToken));
    }

    [HttpGet("accesos/usuarios/buscar")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaAccesoItem>>> BuscarUsuarios([FromQuery] string q, CancellationToken cancellationToken)
    {
        ExigirVeTodo();
        return Ok(await accesoService.BuscarUsuariosAsync(q, cancellationToken));
    }

    [HttpPost("accesos")]
    public async Task<IActionResult> OtorgarAcceso([FromBody] OtorgarAccesoBody body, CancellationToken cancellationToken)
    {
        ExigirVeTodo();
        await accesoService.OtorgarAsync(new OtorgarAccesoAreaRequest(body.IdUsuario, body.Area, body.NivelAcceso, User.Identity!.Name!), cancellationToken);
        return NoContent();
    }

    [HttpDelete("accesos/{id:guid}")]
    public async Task<IActionResult> QuitarAcceso(Guid id, CancellationToken cancellationToken)
    {
        ExigirVeTodo();
        await accesoService.QuitarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("areas")]
    public ActionResult<IReadOnlyList<object>> Areas() =>
        Ok(Enum.GetValues<AreaDocumental>().Select(a => new { codigo = a.ToString(), nombre = NombreArea(a) }));

    [HttpGet("tipos")]
    public ActionResult<IReadOnlyList<object>> Tipos() =>
        Ok(Enum.GetValues<TipoDocumento>().Select(t => new { codigo = t.ToString(), nombre = NombreTipo(t) }));

    [HttpGet("estados")]
    public ActionResult<IReadOnlyList<object>> Estados() =>
        Ok(Enum.GetValues<EstadoDocumento>().Select(e => new { codigo = e.ToString(), nombre = NombreEstado(e) }));

    private static string NombreArea(AreaDocumental a) => a switch
    {
        AreaDocumental.GerenciaGeneral => "Gerencia General",
        AreaDocumental.NegociosComercial => "Negocios / Comercial",
        AreaDocumental.Credito => "Crédito",
        AreaDocumental.Captacion => "Captación",
        AreaDocumental.CajasVentanilla => "Cajas / Ventanilla",
        AreaDocumental.AtencionCliente => "Atención al Cliente / Servicio al Socio",
        AreaDocumental.Cobranzas => "Cobranzas",
        AreaDocumental.Cumplimiento => "Cumplimiento (LA/FT)",
        AreaDocumental.Riesgos => "Riesgos",
        AreaDocumental.ContabilidadFinanzas => "Contabilidad / Finanzas",
        AreaDocumental.TalentoHumano => "Talento Humano",
        AreaDocumental.AuditoriaInterna => "Auditoría Interna",
        AreaDocumental.AgenciasSucursales => "Agencias / Sucursales",
        AreaDocumental.SistemasTi => "Sistemas / TI",
        AreaDocumental.Otra => "Otra área",
        _ => a.ToString(),
    };

    private static string NombreTipo(TipoDocumento t) => t switch
    {
        TipoDocumento.Politica => "Política",
        TipoDocumento.Reglamento => "Reglamento",
        TipoDocumento.Manual => "Manual",
        TipoDocumento.Plan => "Plan",
        TipoDocumento.Procedimiento => "Procedimiento",
        TipoDocumento.Formato => "Formato",
        TipoDocumento.Instructivo => "Instructivo",
        TipoDocumento.Informe => "Informe",
        TipoDocumento.Acta => "Acta",
        TipoDocumento.Registro => "Registro",
        TipoDocumento.Solicitud => "Solicitud",
        _ => t.ToString(),
    };

    private static string NombreEstado(EstadoDocumento e) => e switch
    {
        EstadoDocumento.Borrador => "Borrador",
        EstadoDocumento.EnRevision => "En revisión",
        EstadoDocumento.Vigente => "Vigente",
        EstadoDocumento.Obsoleto => "Obsoleto",
        _ => e.ToString(),
    };
}
