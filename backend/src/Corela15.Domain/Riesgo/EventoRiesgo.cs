namespace Corela15.Domain.Riesgo;

/// <summary>
/// Un evento de riesgo identificado sobre un proceso — el registro de
/// riesgo (matriz impacto × probabilidad → nivel). Simplificación del motor
/// completo de RIESGOOPERATIVO.EVENTO/TIPOEVENTO/EVENTO_PLANACCION (planes
/// de acción, etapas de avance) — se agrega cuando se construya el caso de
/// uso real de gestión de riesgo, esto modela solo el registro base.
/// </summary>
public class EventoRiesgo
{
    public Guid Id { get; set; }

    public int IdProceso { get; set; }
    public Proceso Proceso { get; set; } = null!;

    public string Descripcion { get; set; } = string.Empty;

    public int IdNivelImpacto { get; set; }
    public NivelImpacto NivelImpacto { get; set; } = null!;

    public int IdNivelProbabilidad { get; set; }
    public NivelProbabilidad NivelProbabilidad { get; set; } = null!;

    public int IdNivelRiesgo { get; set; }
    public NivelRiesgo NivelRiesgo { get; set; } = null!;

    public DateOnly FechaIdentificacion { get; set; }
    public bool Activo { get; set; } = true;
}
