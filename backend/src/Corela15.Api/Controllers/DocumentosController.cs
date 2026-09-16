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
    public Guid IdCarpeta { get; set; }
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
    string Titulo, Guid IdCarpeta, string Area, string Tipo, string Estado,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas);

public class NuevaVersionDocumentoBody
{
    public string? Version { get; set; }
    public IFormFile Archivo { get; set; } = null!;
}

public record CrearCarpetaBody(string Nombre, Guid? IdCarpetaPadre);
public record RenombrarCarpetaBody(string Nombre);
public record OtorgarAccesoBody(Guid IdUsuario, string NivelAcceso);

/// <summary>
/// Biblioteca documental institucional — portada real de CredVault
/// (`apps.documents`, la app externa de gestión de TI donde vivía hasta
/// ahora), reorganizada con árbol real de carpetas (ver
/// <see cref="Corela15.Domain.Documentos.Carpeta"/>) y ACL real por
/// carpeta (ver <see cref="Corela15.Domain.Documentos.CarpetaAcceso"/>)
/// — pedido explícito del usuario: "como compartir por red", carpeta por
/// carpeta, no por un área fija.
/// </summary>
[ApiController]
[Route("api/biblioteca-documentos")]
[Authorize(Policy = "Menu:biblioteca-documentos")]
public class DocumentosController(IDocumentoService service, ICarpetaService carpetaService, IAccesoDocumentalService accesoService) : ControllerBase
{
    /// <summary>ADMINISTRADOR o el menú dedicado ven/administran TODAS las carpetas sin ACL — mismo patrón ya usado en Planificación (planificacion-gerencia).</summary>
    private bool VeTodo => User.IsInRole("ADMINISTRADOR") || User.HasClaim("menu", "biblioteca-documentos-gerencia");

    private Guid IdUsuarioActual => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);

    private async Task<ContextoAccesoDocumental> ResolverContextoAsync(CancellationToken cancellationToken)
    {
        if (VeTodo) return new ContextoAccesoDocumental(true, new Dictionary<Guid, string>());
        var accesoPorCarpeta = await accesoService.ResolverAccesoEfectivoAsync(IdUsuarioActual, cancellationToken);
        return new ContextoAccesoDocumental(false, accesoPorCarpeta);
    }

    /// <summary>
    /// Mi propio ACL real — el frontend lo pide una vez al abrir el módulo
    /// para decidir qué mostrar, sin depender solo del 422 del backend al
    /// intentar la acción. Nunca se cachea en el JWT -- se resuelve en
    /// vivo contra la base en cada carga, así un cambio de acceso se ve
    /// inmediato, sin re-login.
    /// </summary>
    [HttpGet("mi-acceso")]
    public async Task<ActionResult<object>> MiAcceso(CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        return Ok(new { veTodo = contexto.VeTodo, accesoPorCarpeta = contexto.AccesoPorCarpeta });
    }

    // --- Carpetas (árbol real) ---

    [HttpGet("carpetas")]
    public async Task<ActionResult<IReadOnlyList<CarpetaItem>>> ListarCarpetas(CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        return Ok(await carpetaService.ListarAsync(contexto, cancellationToken));
    }

    [HttpPost("carpetas")]
    public async Task<ActionResult<object>> CrearCarpeta([FromBody] CrearCarpetaBody body, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        var id = await carpetaService.CrearAsync(new CrearCarpetaRequest(body.Nombre, body.IdCarpetaPadre, User.Identity!.Name!), contexto, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("carpetas/{id:guid}")]
    public async Task<IActionResult> RenombrarCarpeta(Guid id, [FromBody] RenombrarCarpetaBody body, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await carpetaService.RenombrarAsync(id, new RenombrarCarpetaRequest(body.Nombre, User.Identity!.Name!), contexto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("carpetas/{id:guid}")]
    public async Task<IActionResult> DesactivarCarpeta(Guid id, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await carpetaService.DesactivarAsync(id, User.Identity!.Name!, contexto, cancellationToken);
        return NoContent();
    }

    // --- ACL real por carpeta — exige Escritura efectiva sobre la carpeta ---
    // (o VeTodo) -- el dueño de una carpeta puede compartirla, mismo
    // criterio que una carpeta de red. Nunca Forbid()/403 acá: en este
    // sistema un 403 SIEMPRE significa "sesión con claims viejos" (ver
    // frontend/src/lib/api.ts, desloguea automático) -- esto es una regla
    // de negocio real, va como 422.

    [HttpGet("carpetas/{idCarpeta:guid}/accesos")]
    public async Task<ActionResult<IReadOnlyList<CarpetaAccesoItem>>> ListarAccesos(Guid idCarpeta, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        return Ok(await accesoService.ListarAsync(idCarpeta, contexto, cancellationToken));
    }

    [HttpGet("accesos/usuarios/buscar")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaAccesoItem>>> BuscarUsuarios([FromQuery] string q, CancellationToken cancellationToken)
    {
        return Ok(await accesoService.BuscarUsuariosAsync(q, cancellationToken));
    }

    [HttpPost("carpetas/{idCarpeta:guid}/accesos")]
    public async Task<IActionResult> OtorgarAcceso(Guid idCarpeta, [FromBody] OtorgarAccesoBody body, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await accesoService.OtorgarAsync(idCarpeta, new OtorgarAccesoCarpetaRequest(body.IdUsuario, body.NivelAcceso, User.Identity!.Name!), contexto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("accesos/{id:guid}")]
    public async Task<IActionResult> QuitarAcceso(Guid id, CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        await accesoService.QuitarAsync(id, contexto, cancellationToken);
        return NoContent();
    }

    // --- Documentos ---

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentoListItem>>> Listar(
        [FromQuery] Guid? idCarpeta, [FromQuery] string? area, [FromQuery] string? tipo,
        [FromQuery] string? estado, [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var contexto = await ResolverContextoAsync(cancellationToken);
        var resultado = await service.ListarAsync(new ListarDocumentosFiltro(idCarpeta, area, tipo, estado, q), contexto, cancellationToken);
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
                body.Titulo, body.IdCarpeta, body.Area, body.Tipo, body.Version ?? "1.0",
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
                body.Titulo, body.IdCarpeta, body.Area, body.Tipo, body.Estado,
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
