namespace Corela15.Domain.Planificacion;

/// <summary>
/// Un bloque de tiempo real dentro de la planificación semanal de un
/// área -- un día + rango horario + qué categoría de actividad es +
/// una descripción corta. Varios bloques por día son normales (la
/// jornada se parte en tramos de actividad distinta).
/// </summary>
public class PlanSemanalBloque
{
    public Guid Id { get; set; }

    public Guid IdPlanSemanal { get; set; }
    public PlanSemanal PlanSemanal { get; set; } = null!;

    /// <summary>1=Lunes ... 6=Sábado (jornada real de la cooperativa, ver PDF de referencia).</summary>
    public int DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    public string CodigoEtiqueta { get; set; } = string.Empty;
    public EtiquetaPlanificacion Etiqueta { get; set; } = null!;

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Nota real de gerencia sobre este bloque puntual (una hora específica de la semana) -- distinta de PlanSemanal.NotaGerencia (la nota general).</summary>
    public string? NotaGerencia { get; set; }
}
