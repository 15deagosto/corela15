namespace Corela15.Domain.Planificacion;

/// <summary>Catálogo de indicadores de planificación estratégica — verificado contra PLANIFICACION.INDICADOR.</summary>
public class Indicador
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
