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

    /// <summary>La carpeta real donde vive el documento — es lo que decide quién puede verlo/editarlo (ver <see cref="CarpetaAcceso"/>). Área queda como metadato de clasificación para la matriz de cobertura, ya no controla acceso.</summary>
    public Guid IdCarpeta { get; set; }
    public Carpeta Carpeta { get; set; } = null!;

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
/// Carpeta real, jerárquica (árbol, vía <see cref="IdCarpetaPadre"/>) —
/// reemplaza el ACL fijo por las 15 áreas: el usuario pidió explícitamente
/// que sea "como compartir por red" — cualquiera con Escritura sobre una
/// carpeta puede crear subcarpetas adentro y decidir, carpeta por
/// carpeta, quién más entra (ver <see cref="CarpetaAcceso"/>). Las 15
/// áreas reales originales (`AreaDocumental`) quedaron sembradas como
/// carpetas raíz (migración `Documentos_CarpetasReales`) para no perder
/// la organización ya levantada por TI — desde ahí, cada jefatura puede
/// seguir creando su propio árbol de subcarpetas.
/// </summary>
public class Carpeta
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid? IdCarpetaPadre { get; set; }
    public Carpeta? CarpetaPadre { get; set; }
    public bool Activa { get; set; } = true;
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}

/// <summary>
/// ACL real por carpeta — pedido explícito del usuario ("por carpeta
/// debería ser... como compartir por red: en una carpeta con 5 adentro,
/// en una de esas solo quiero poner ciertos usuarios"). Se busca por
/// usuario y se le da acceso de Lectura o de Escritura a una carpeta
/// puntual. Sin fila acá, el usuario NO ve nada de esa carpeta —
/// deny-by-default real. Herencia real, no acumulativa: una carpeta SIN
/// filas propias hereda el ACL de la carpeta padre más cercana que sí
/// tenga filas (recursivo hasta la raíz); en cuanto una carpeta tiene AL
/// MENOS una fila propia, esas filas son las únicas que aplican para
/// ella y sus descendientes (hasta el próximo corte de herencia) — así
/// es como se logra el caso real pedido: la carpeta padre puede tener 10
/// personas con acceso, y una subcarpeta puntual puede reducirlo a solo
/// 2, agregando filas propias ahí (nunca hace falta "restar" a nadie,
/// solo declarar de nuevo la lista completa que sí aplica a esa
/// subcarpeta). ADMINISTRADOR y quien tenga el menú
/// `biblioteca-documentos-gerencia` ven/administran TODAS las carpetas
/// sin necesidad de ninguna fila acá.
/// </summary>
public class CarpetaAcceso
{
    public Guid Id { get; set; }
    public Guid IdCarpeta { get; set; }
    public Carpeta Carpeta { get; set; } = null!;
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public NivelAccesoDocumental NivelAcceso { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}
