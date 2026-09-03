namespace Corela15.Domain.Riesgo;

/// <summary>Catálogo real de estados del plan de acción — verificado contra RIESGOOPERATIVO.ESTADO_AVANCERIESGO (6 filas reales).</summary>
public class EstadoAvanceRiesgo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
