namespace Corela15.Domain.Planificacion;

/// <summary>
/// Un KPI real cargado dentro de la planificación semanal de un área --
/// meta (lo que se propuso) vs real (lo que realmente se logró), con un
/// comentario corto opcional. Mismo patrón cabecera+detalle que
/// PlanSemanalBloque: varios por plan, reemplazados por completo en cada
/// GuardarAsync (nunca se acumulan versiones viejas sueltas).
/// </summary>
public class PlanSemanalIndicador
{
    public Guid Id { get; set; }

    public Guid IdPlanSemanal { get; set; }
    public PlanSemanal PlanSemanal { get; set; } = null!;

    public int IdIndicador { get; set; }
    public Indicador Indicador { get; set; } = null!;

    public decimal? Meta { get; set; }
    public decimal? Real { get; set; }

    public string? Comentario { get; set; }
}
