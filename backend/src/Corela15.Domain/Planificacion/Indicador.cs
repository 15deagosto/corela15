namespace Corela15.Domain.Planificacion;

/// <summary>
/// Catálogo de indicadores (KPI) de planificación -- originalmente
/// verificado contra PLANIFICACION.INDICADOR sin caso de uso real
/// (ver PlanificacionAnual). Repurpuesto acá como el KPI real que cada
/// área define para su propia planificación semanal (mismo patrón que
/// EtiquetaPlanificacion -- catálogo editable por área, con combo
/// "obtener o crear" sobre la marcha desde el propio formulario).
/// </summary>
public class Indicador
{
    public int Id { get; set; }

    public string CodigoArea { get; set; } = string.Empty;
    public AreaPlanificacion Area { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Unidad real del KPI (%, $, cantidad, horas...) -- solo para mostrarlo junto al valor, sin validación de tipo.</summary>
    public string? Unidad { get; set; }

    public bool Activo { get; set; } = true;
}
