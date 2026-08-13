namespace Corela15.Domain.Planificacion;

/// <summary>
/// Plan estratégico anual — verificado contra PLANIFICACION.PLANIFICACION_ANUAL.
/// El detalle por asesor/indicador (PLANIFICACION_ANUAL_ASESOR,
/// PLANIFICACION_ANUAL_INDICADOR, seguimiento mensual) queda fuera de
/// alcance inicial — se agrega cuando se construya el caso de uso real.
/// </summary>
public class PlanificacionAnual
{
    public Guid Id { get; set; }
    public int Anio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
