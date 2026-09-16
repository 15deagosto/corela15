using Corela15.Application.Common;

namespace Corela15.Application.Documentos;

public interface IDocumentoService
{
    Task<Guid> CrearAsync(CrearDocumentoRequest request, Stream archivo, string nombreArchivoOriginal, string contentType, long tamanoBytes, CancellationToken cancellationToken = default);

    Task ActualizarAsync(Guid id, ActualizarDocumentoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sube un archivo nuevo sobre el mismo documento (re-papela bajo una versión nueva) — el archivo anterior nunca se borra del NAS, solo deja de ser el referenciado.</summary>
    Task SubirNuevaVersionAsync(Guid id, SubirNuevaVersionRequest request, Stream archivo, string nombreArchivoOriginal, string contentType, long tamanoBytes, CancellationToken cancellationToken = default);

    Task DesactivarAsync(Guid id, string modificadoPor, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoListItem>> ListarAsync(ListarDocumentosFiltro filtro, CancellationToken cancellationToken = default);

    Task<DocumentoListItem?> ObtenerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DescargaDocumentoResult> DescargarAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Cobertura documental real: por cada área, cuántos documentos vigentes/total tiene de cada tipo — para ver de un vistazo qué jefaturas no han subido sus políticas/procedimientos.</summary>
    Task<MatrizDocumentalResult> ObtenerMatrizAsync(CancellationToken cancellationToken = default);
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
