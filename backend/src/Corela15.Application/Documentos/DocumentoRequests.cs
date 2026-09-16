using Corela15.Domain.Documentos;

namespace Corela15.Application.Documentos;

public record CrearDocumentoRequest(
    string Titulo, Guid IdCarpeta, string Area, string Tipo, string Version,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas,
    string RegistradoPor);

public record ActualizarDocumentoRequest(
    string Titulo, Guid IdCarpeta, string Area, string Tipo, string Estado,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas,
    string ModificadoPor);

public record SubirNuevaVersionRequest(string Version, string ModificadoPor);

public record ListarDocumentosFiltro(Guid? IdCarpeta, string? Area, string? Tipo, string? Estado, string? Busqueda);

public record DocumentoListItem(
    Guid Id, Guid IdCarpeta, string NombreCarpeta, string Area, string Tipo, string Titulo, string Version,
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
/// claim de menú `biblioteca-documentos-gerencia` + el ACL real efectivo
/// por carpeta del usuario actual, ver <see cref="Corela15.Domain.Documentos.CarpetaAcceso"/>)
/// y pasado a cada método de <see cref="IDocumentoService"/>/<see cref="ICarpetaService"/>
/// — el servicio nunca vuelve a resolver quién es el usuario, solo aplica
/// este contexto ya resuelto.
/// </summary>
public record ContextoAccesoDocumental(bool VeTodo, IReadOnlyDictionary<Guid, string> AccesoPorCarpeta)
{
    public bool TieneLectura(Guid idCarpeta) => VeTodo || AccesoPorCarpeta.ContainsKey(idCarpeta);
    public bool TieneEscritura(Guid idCarpeta) => VeTodo || (AccesoPorCarpeta.TryGetValue(idCarpeta, out var nivel) && nivel == "Escritura");
}

public record CrearCarpetaRequest(string Nombre, Guid? IdCarpetaPadre, string CreadoPor);
public record RenombrarCarpetaRequest(string Nombre, string ModificadoPor);

public record CarpetaItem(
    Guid Id, string Nombre, Guid? IdCarpetaPadre, bool Activa,
    string NivelAcceso, bool PuedeEscribir, bool PuedeAdministrarAcceso);

public record CarpetaAccesoItem(Guid Id, Guid IdUsuario, string NombreUsuario, Guid IdCarpeta, string NombreCarpeta, string NivelAcceso, DateTimeOffset CreadoEn, string CreadoPor);

public record OtorgarAccesoCarpetaRequest(Guid IdUsuario, string NivelAcceso, string RegistradoPor);

public record UsuarioParaAccesoItem(Guid Id, string NombreUsuario);
