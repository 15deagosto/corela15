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

/// <param name="CompartidosConmigo">Solo documentos compartidos directo con el usuario cuya carpeta no ve (la sección "Compartidos conmigo").</param>
public record ListarDocumentosFiltro(Guid? IdCarpeta, string? Area, string? Tipo, string? Estado, string? Busqueda, bool CompartidosConmigo = false);

/// <param name="NivelAcceso">Nivel efectivo del usuario actual sobre ESTE documento (SoloVer/Lectura/Escritura).</param>
/// <param name="AccesosPropios">true si el documento tiene permisos propios (ya no hereda los de la carpeta).</param>
public record DocumentoListItem(
    Guid Id, Guid IdCarpeta, string NombreCarpeta, string Area, string Tipo, string Titulo, string Version,
    string Estado, string NombreArchivoOriginal, long TamanoBytes,
    string? InstanciaAprobacion, string? InstanciaRevision,
    DateOnly? FechaAprobacion, DateOnly? ProximaRevision, string? Notas,
    string CreadoPor, DateTimeOffset CreadoEn, DateTimeOffset? ModificadoEn,
    string NivelAcceso, bool PuedeDescargar, bool PuedeEditar, bool AccesosPropios);

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
public record ContextoAccesoDocumental(
    bool VeTodo,
    IReadOnlyDictionary<Guid, string> AccesoPorCarpeta,
    IReadOnlySet<Guid>? DocumentosConAccesoPropio = null,
    IReadOnlyDictionary<Guid, string>? AccesoPorDocumento = null)
{
    public IReadOnlySet<Guid> DocumentosConAclPropio => DocumentosConAccesoPropio ?? new HashSet<Guid>();
    public IReadOnlyDictionary<Guid, string> AccesoDocumentos => AccesoPorDocumento ?? new Dictionary<Guid, string>();

    /// <summary>Ver la carpeta (cualquier nivel, incluido SoloVer).</summary>
    public bool TieneLectura(Guid idCarpeta) => VeTodo || AccesoPorCarpeta.ContainsKey(idCarpeta);
    public bool TieneEscritura(Guid idCarpeta) => VeTodo || (AccesoPorCarpeta.TryGetValue(idCarpeta, out var nivel) && nivel == "Escritura");

    /// <summary>
    /// Nivel efectivo sobre un documento: si tiene permisos propios, solo
    /// cuentan esos; si no, hereda el de su carpeta. null = no lo ve.
    /// </summary>
    public string? NivelDocumento(Guid idDocumento, Guid idCarpeta)
    {
        if (VeTodo) return "Escritura";
        if (DocumentosConAclPropio.Contains(idDocumento))
            return AccesoDocumentos.TryGetValue(idDocumento, out var propio) ? propio : null;
        return AccesoPorCarpeta.TryGetValue(idCarpeta, out var heredado) ? heredado : null;
    }

    public bool PuedeVerDocumento(Guid idDocumento, Guid idCarpeta) => NivelDocumento(idDocumento, idCarpeta) is not null;
    public bool PuedeDescargarDocumento(Guid idDocumento, Guid idCarpeta) => NivelDocumento(idDocumento, idCarpeta) is "Lectura" or "Escritura";
    public bool PuedeEditarDocumento(Guid idDocumento, Guid idCarpeta) => NivelDocumento(idDocumento, idCarpeta) == "Escritura";
}

public record DocumentoAccesoItem(Guid Id, Guid IdUsuario, string NombreUsuario, string NivelAcceso, bool Heredado, DateTimeOffset CreadoEn, string CreadoPor);

/// <param name="AccesosPropios">false = el documento hereda de su carpeta (Accesos muestra los heredados, solo lectura).</param>
public record DocumentoAccesosResult(bool AccesosPropios, string NombreCarpeta, IReadOnlyList<DocumentoAccesoItem> Accesos);

public record AccesoDocumentosUsuario(IReadOnlySet<Guid> DocumentosConAccesoPropio, IReadOnlyDictionary<Guid, string> AccesoPorDocumento);

public record CrearCarpetaRequest(string Nombre, Guid? IdCarpetaPadre, string CreadoPor);
public record RenombrarCarpetaRequest(string Nombre, string ModificadoPor);

public record CarpetaItem(
    Guid Id, string Nombre, Guid? IdCarpetaPadre, bool Activa,
    string NivelAcceso, bool PuedeEscribir, bool PuedeAdministrarAcceso,
    bool CompartidaConmigo = false, string? CompartidaPor = null);

public record CarpetaAccesoItem(Guid Id, Guid IdUsuario, string NombreUsuario, Guid IdCarpeta, string NombreCarpeta, string NivelAcceso, DateTimeOffset CreadoEn, string CreadoPor);

public record OtorgarAccesoCarpetaRequest(Guid IdUsuario, string NivelAcceso, string RegistradoPor);

public record UsuarioParaAccesoItem(Guid Id, string NombreUsuario);
