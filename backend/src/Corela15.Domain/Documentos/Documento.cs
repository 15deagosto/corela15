using Corela15.Domain.Common;
using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Documentos;

/// <summary>
/// Áreas reales de la cooperativa (levantadas por el Jefe de TIC en
/// CredVault, `Mapeo_Areas_Procesos_Cooperativa`) — portadas tal cual, no
/// inventadas. Cada documento pertenece a una sola área: la dueña/
/// responsable de mantenerlo actualizado.
/// </summary>
public enum AreaDocumental
{
    GerenciaGeneral = 1,
    NegociosComercial = 2,
    Credito = 3,
    Captacion = 4,
    CajasVentanilla = 5,
    AtencionCliente = 6,
    Cobranzas = 7,
    Cumplimiento = 8,
    Riesgos = 9,
    ContabilidadFinanzas = 10,
    TalentoHumano = 11,
    AuditoriaInterna = 12,
    AgenciasSucursales = 13,
    SistemasTi = 14,
    Otra = 15,
}

public enum TipoDocumento
{
    Politica = 1,
    Reglamento = 2,
    Manual = 3,
    Plan = 4,
    Procedimiento = 5,
    Formato = 6,
    Instructivo = 7,
    Informe = 8,
    Acta = 9,
    Registro = 10,
    Solicitud = 11,
}

public enum EstadoDocumento
{
    Borrador = 1,
    EnRevision = 2,
    Vigente = 3,
    Obsoleto = 4,
}

/// <summary>
/// Biblioteca documental institucional (políticas, reglamentos, manuales,
/// planes, procedimientos, formatos, instructivos, informes, actas,
/// registros, solicitudes) — portada real de CredVault
/// (`apps.documents.Document`, Django), la app externa de gestión de TI
/// donde vivía hasta ahora. Acá queda nativa del core, con el mismo
/// modelo/reglas ya probadas allá (área/tipo/estado, versión, instancias
/// de aprobación/revisión, próxima revisión), sobre el archivo real
/// alojado en el NAS de la cooperativa — nunca en el volumen del
/// contenedor (ver <see cref="IDocumentoStorageService"/> en
/// Application, la abstracción real de dónde vive el archivo físico).
/// </summary>
public class Documento : AuditableEntity
{
    public Guid Id { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public AreaDocumental Area { get; set; }
    public TipoDocumento Tipo { get; set; }
    public string Version { get; set; } = "1.0";
    public EstadoDocumento Estado { get; set; } = EstadoDocumento.Borrador;

    /// <summary>Nombre real del archivo tal como lo subió el usuario — para mostrarlo/descargarlo con su nombre original, nunca el GUID interno.</summary>
    public string NombreArchivoOriginal { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa dentro de la raíz configurada (ver
    /// <c>Documentos__RutaAlmacenamiento</c>, el punto de montaje real del
    /// NAS) — nunca una ruta absoluta ni expuesta directo al frontend, se
    /// resuelve siempre a través del endpoint de descarga.
    /// </summary>
    public string RutaAlmacenamiento { get; set; } = string.Empty;

    public long TamanoBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;

    public string? InstanciaAprobacion { get; set; }
    public string? InstanciaRevision { get; set; }
    public DateOnly? FechaAprobacion { get; set; }
    public DateOnly? ProximaRevision { get; set; }
    public string? Notas { get; set; }

    /// <summary>Nunca DELETE real, mismo criterio de todo el core — solo se desactiva.</summary>
    public bool Activo { get; set; } = true;
}

public enum NivelAccesoDocumental
{
    Lectura = 1,
    Escritura = 2,
}

/// <summary>
/// ACL real por área — pedido explícito del usuario ("como TI no quiero
/// que alguien más vea lo que subo ahí"), mismo modelo mental que una
/// carpeta compartida de red: se busca por usuario y se le da acceso de
/// Lectura o de Escritura a un área puntual. Sin fila acá, el usuario NO
/// ve nada de esa área — deny-by-default real, no una excepción. Un
/// usuario puede tener acceso a varias áreas (una fila por área,
/// única por usuario+área — para subir de Lectura a Escritura se edita
/// la fila existente, nunca se duplica). ADMINISTRADOR y quien tenga el
/// menú `biblioteca-documentos-gerencia` ven/administran TODAS las áreas
/// sin necesidad de ninguna fila acá — mismo patrón ya usado en
/// Planificación (`planificacion-gerencia`).
/// </summary>
public class AreaAccesoUsuario
{
    public Guid Id { get; set; }
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public AreaDocumental Area { get; set; }
    public NivelAccesoDocumental NivelAcceso { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}
