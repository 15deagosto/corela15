namespace Corela15.Domain.Riesgo;

/// <summary>Bitácora real de transición de estado del plan — verificado contra RIESGOOPERATIVO.AVANCERIESGO_DETALLE_ETAPA. Nunca se borra, mismo criterio de auditoría del resto del proyecto.</summary>
public class AvanceRiesgoEtapa
{
    public Guid Id { get; set; }

    public Guid IdAvanceRiesgoDetalle { get; set; }
    public AvanceRiesgoDetalle AvanceRiesgoDetalle { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoAvanceRiesgo Estado { get; set; } = null!;

    public string Comentario { get; set; } = string.Empty;
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
}
