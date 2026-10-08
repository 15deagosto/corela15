namespace Corela15.Domain.Planificacion;

/// <summary>
/// Estado real de revisión de gerencia sobre un plan semanal -- distinto
/// del envío (PlanSemanal.Enviada), que solo indica que el área terminó
/// de cargar su planificación. Un plan vuelve a Pendiente automáticamente
/// si el área lo reguarda después de haber sido Aprobado/ConObservaciones
/// (el contenido cambió, la revisión anterior ya no aplica).
/// </summary>
public enum EstadoAprobacionPlan
{
    Pendiente,
    Aprobado,
    ConObservaciones,
}
