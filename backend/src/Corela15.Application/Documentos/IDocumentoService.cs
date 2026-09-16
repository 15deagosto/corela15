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

    /// <summary>Cobertura documental real: por cada área, cuántos documentos vigentes/total tiene de cada tipo — reservado a quien ve todas las carpetas (expone info de toda la biblioteca a la vez).</summary>
    Task<MatrizDocumentalResult> ObtenerMatrizAsync(ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);
}

/// <summary>Administra el árbol real de carpetas — ver Carpeta.cs.</summary>
public interface ICarpetaService
{
    /// <summary>Solo las carpetas donde el usuario tiene al menos Lectura efectiva (o todas, si VeTodo).</summary>
    Task<IReadOnlyList<CarpetaItem>> ListarAsync(ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    /// <summary>Crear una carpeta raíz exige VeTodo; crear una subcarpeta exige Escritura sobre la carpeta padre.</summary>
    Task<Guid> CrearAsync(CrearCarpetaRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task RenombrarAsync(Guid id, RenombrarCarpetaRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task DesactivarAsync(Guid id, string modificadoPor, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);
}

/// <summary>Administra el ACL real por carpeta — ver CarpetaAcceso.cs. Otorgar/quitar/listar accesos de una carpeta exige Escritura efectiva sobre esa carpeta (o VeTodo) — el dueño de una carpeta puede compartirla, mismo criterio que una carpeta de red.</summary>
public interface IAccesoDocumentalService
{
    Task<IReadOnlyList<CarpetaAccesoItem>> ListarAsync(Guid idCarpeta, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioParaAccesoItem>> BuscarUsuariosAsync(string q, CancellationToken cancellationToken = default);

    Task OtorgarAsync(Guid idCarpeta, OtorgarAccesoCarpetaRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    Task QuitarAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default);

    /// <summary>Resuelve el ACL efectivo real de un usuario (con herencia por carpeta, ver CarpetaAcceso) — usado por el controller para armar el ContextoAccesoDocumental de cada request.</summary>
    Task<IReadOnlyDictionary<Guid, string>> ResolverAccesoEfectivoAsync(Guid idUsuario, CancellationToken cancellationToken = default);
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

public class CarpetaInvalidaException()
    : ReglaDeNegocioException("La carpeta no existe o fue desactivada");

public class NombreCarpetaRequeridoException()
    : SolicitudInvalidaExceptionGenerica("El nombre de la carpeta es obligatorio");

public class CarpetaConContenidoException()
    : ReglaDeNegocioException("No se puede desactivar una carpeta con documentos activos o subcarpetas activas — mové o desactivá el contenido primero");

/// <summary>
/// El usuario no tiene Escritura sobre la carpeta del documento (o de la
/// carpeta destino, si intenta moverlo) — 422, nunca 403 (en este sistema
/// un 403 siempre significa sesión con claims viejos, ver
/// frontend/src/lib/api.ts, nunca una regla de negocio real).
/// </summary>
public class AccesoDocumentalDenegadoException(string carpeta)
    : ReglaDeNegocioException($"No tenés acceso de escritura sobre la carpeta '{carpeta}'");

public class UsuarioAccesoInvalidoException()
    : ReglaDeNegocioException("El usuario no existe o está inactivo");

public class NivelAccesoInvalidoException(string valor)
    : SolicitudInvalidaExceptionGenerica($"'{valor}' no es un nivel de acceso válido — use Lectura o Escritura");
