namespace Corela15.Domain.Planificacion;

public enum EstadoDisposicionGerencia
{
    Abierta,
    Cerrada,
}

/// <summary>
/// Una disposición real de gerencia con plazo -- pedido explícito del
/// usuario: "gerencia pone una disposición hasta tal día y yo en mi
/// planificación debería poder escoger qué tiempo voy a dedicarle". Solo
/// gerencia (<c>Menu:planificacion-gerencia</c>) la crea; cada área
/// asignada la ve y puede etiquetar bloques de su planificación semanal
/// con ella (ver <see cref="PlanSemanalBloque.IdDisposicion"/>) -- la suma
/// real de horas dedicadas por todas las áreas, en cualquier semana, es el
/// avance visible para gerencia, sin que nadie tenga que reportarlo aparte.
/// </summary>
public class DisposicionGerencia
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateOnly FechaLimite { get; set; }
    public EstadoDisposicionGerencia Estado { get; set; } = EstadoDisposicionGerencia.Abierta;

    public ICollection<DisposicionGerenciaArea> Areas { get; set; } = new List<DisposicionGerenciaArea>();

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}

/// <summary>Áreas reales destinatarias de una disposición -- una disposición puede aplicar a una sola área o a varias a la vez (ej. "todas entregan tal informe").</summary>
public class DisposicionGerenciaArea
{
    public Guid Id { get; set; }
    public Guid IdDisposicion { get; set; }
    public DisposicionGerencia Disposicion { get; set; } = null!;
    public string CodigoArea { get; set; } = string.Empty;
    public AreaPlanificacion Area { get; set; } = null!;
}
