using Corela15.Domain.Documentos;

namespace Corela15.Application.Documentos;

public record CrearDocumentoRequest(
    string Titulo, string Area, string Tipo, string Version,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas,
    string RegistradoPor);

public record ActualizarDocumentoRequest(
    string Titulo, string Area, string Tipo, string Estado,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas,
    string ModificadoPor);

public record SubirNuevaVersionRequest(string Version, string ModificadoPor);

public record ListarDocumentosFiltro(string? Area, string? Tipo, string? Estado, string? Busqueda);

public record DocumentoListItem(
    Guid Id, string Titulo, string Area, string Tipo, string Version,
    string Estado, string NombreArchivoOriginal, long TamanoBytes,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas,
    string CreadoPor, DateTimeOffset CreadoEn, DateTimeOffset? ModificadoEn);

public record MatrizCeldaResult(int Total, int Vigentes);
public record MatrizFilaResult(string Area, IReadOnlyDictionary<string, MatrizCeldaResult> Celdas);
public record MatrizDocumentalResult(IReadOnlyList<MatrizFilaResult> Filas);

public record DescargaDocumentoResult(Stream Contenido, string NombreArchivo, string ContentType);
