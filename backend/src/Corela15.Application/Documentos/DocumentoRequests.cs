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

/// <summary>
/// Resuelto una vez por request en el controller (rol ADMINISTRADOR +
/// claim de menú `biblioteca-documentos-gerencia` + las filas reales de
/// <see cref="Corela15.Domain.Documentos.AreaAccesoUsuario"/> del usuario
/// actual) y pasado a cada método de <see cref="IDocumentoService"/> —
/// el servicio nunca vuelve a consultar quién es el usuario, solo aplica
/// este contexto ya resuelto.
/// </summary>
public record ContextoAccesoDocumental(bool VeTodo, IReadOnlyDictionary<string, string> AccesoPorArea)
{
    public bool TieneLectura(string area) => VeTodo || AccesoPorArea.ContainsKey(area);
    public bool TieneEscritura(string area) => VeTodo || (AccesoPorArea.TryGetValue(area, out var nivel) && nivel == "Escritura");
}

public record AreaAccesoItem(Guid Id, Guid IdUsuario, string NombreUsuario, string Area, string NivelAcceso, DateTimeOffset CreadoEn, string CreadoPor);

public record OtorgarAccesoAreaRequest(Guid IdUsuario, string Area, string NivelAcceso, string RegistradoPor);

public record UsuarioParaAccesoItem(Guid Id, string NombreUsuario);
