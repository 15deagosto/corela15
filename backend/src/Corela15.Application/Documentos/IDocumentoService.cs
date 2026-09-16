using Corela15.Application.Common;

namespace Corela15.Application.Documentos;

public interface IDocumentoService
{
    Task<Guid> CrearAsync(CrearDocumentoRequest request, Stream archivo, string nombreArchivoOriginal, string contentType, long tamanoBytes, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task ActualizarAsync(Guid id, ActualizarDocumentoRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    /// <summary>Sube un archivo nuevo sobre el mismo documento (re-papela bajo una versión nueva) — el archivo anterior nunca se borra del NAS, solo deja de ser el referenciado.</summary>
    Task SubirNuevaVersionAsync(Guid id, SubirNuevaVersionRequest request, Stream archivo, string nombreArchivoOriginal, string contentType, long tamanoBytes, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task DesactivarAsync(Guid id, string modificadoPor, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoListItem>> ListarAsync(ListarDocumentosFiltro filtro, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task<DocumentoListItem?> ObtenerAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task<DescargaDocumentoResult> DescargarAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    /// <summary>Cobertura documental real: por cada área, cuántos documentos vigentes/total tiene de cada tipo — reservado a quien ve todas las áreas (expone info de todas a la vez).</summary>
    Task<MatrizDocumentalResult> ObtenerMatrizAsync(ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);
}

/// <summary>Administra el ACL real por área — ver AreaAccesoUsuario.cs. Reservado a quien ve todas las áreas (ADMINISTRADOR o biblioteca-documentos-gerencia).</summary>
public interface IAccesoDocumentalService
{
    Task<IReadOnlyList<AreaAccesoItem>> ListarAsync(string? area, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioParaAccesoItem>> BuscarUsuariosAsync(string q, CancellationToken cancellationToken = default);

    Task OtorgarAsync(OtorgarAccesoAreaRequest request, CancellationToken cancellationToken = default);

    Task QuitarAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Resuelve el ACL real de un usuario puntual (sus filas de AreaAccesoUsuario) — usado por el controller para armar el ContextoAccesoDocumental de cada request.</summary>
    Task<IReadOnlyDictionary<string, string>> ObtenerAccesoPorAreaAsync(Guid idUsuario, CancellationToken cancellationToken = default);
}

public class DocumentoInvalidoException()
    : ReglaDeNegocioException("El documento no existe o fue desactivado");

public class TituloDocumentoRequeridoException()
    : SolicitudInvalidaExceptionGenerica("El título del documento es obligatorio");

public class ArchivoDocumentoRequeridoException()
    : SolicitudInvalidaExceptionGenerica("El archivo es obligatorio");

public class ExtensionNoPermitidaException(string extension)
    : SolicitudInvalidaExceptionGenerica($"Extensión '{extension}' no permitida — solo pdf, doc, docx, xls, xlsx, txt, ppt, pptx");

public class ArchivoDemasiadoGrandeException(int maxMb)
    : SolicitudInvalidaExceptionGenerica($"El archivo no puede pesar más de {maxMb}MB");

public class ValorCatalogoDocumentoInvalidoException(string campo, string valor)
    : SolicitudInvalidaExceptionGenerica($"'{valor}' no es un valor válido para {campo}");

/// <summary>
/// El usuario no tiene Escritura sobre el área del documento (o de la
/// nueva área, si intenta moverlo) — 422, nunca 403 (en este sistema un
/// 403 siempre significa sesión con claims viejos, ver
/// frontend/src/lib/api.ts, nunca una regla de negocio real).
/// </summary>
public class AccesoDocumentalDenegadoException(string area)
    : ReglaDeNegocioException($"No tenés acceso de escritura sobre el área '{area}'");

public class UsuarioAccesoInvalidoException()
    : ReglaDeNegocioException("El usuario no existe o está inactivo");

public class NivelAccesoInvalidoException(string valor)
    : SolicitudInvalidaExceptionGenerica($"'{valor}' no es un nivel de acceso válido — use Lectura o Escritura");
