namespace Corela15.Domain.General;

/// <summary>
/// Catálogo CIIU jerárquico real — verificado contra GENERAL.ACTIVIDAD_ECONOMICA
/// (3.063 filas reales; 3.046 sembradas — 6 anomalías de longitud de código y
/// 1 código duplicado real excluidos y documentados en la migración, mismo
/// criterio que la anomalía "671" del Catálogo Único de Cuentas). Jerarquía
/// derivada por prefijo de código (A → A01 → A011 → ...), igual método que
/// el CUC. Cierra el campo huérfano Persona.IdActividadEconomica, sembrado
/// desde Nivel 0 sin catálogo real detrás hasta ahora.
/// </summary>
public class ActividadEconomica
{
    public int Id { get; set; }

    public int IdTipoActividad { get; set; }
    public TipoActividad TipoActividad { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public int? IdActividadPadre { get; set; }
    public ActividadEconomica? ActividadPadre { get; set; }

    public bool Activo { get; set; } = true;
}
