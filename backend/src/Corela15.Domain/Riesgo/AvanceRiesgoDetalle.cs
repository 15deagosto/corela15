namespace Corela15.Domain.Riesgo;

/// <summary>El plan en sí (evento detectado, causa, tratamiento) — verificado contra RIESGOOPERATIVO.AVANCERIESGO_DETALLE.</summary>
public class AvanceRiesgoDetalle
{
    public Guid Id { get; set; }

    public Guid IdAvanceRiesgo { get; set; }
    public AvanceRiesgo AvanceRiesgo { get; set; } = null!;

    public string EventoDetectado { get; set; } = string.Empty;
    public string PosibleCausa { get; set; } = string.Empty;
    public string Tratamiento { get; set; } = string.Empty;
    public string Inconvenientes { get; set; } = string.Empty;
    public string AccionesSugeridas { get; set; } = string.Empty;
}
