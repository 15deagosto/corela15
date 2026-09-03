namespace Corela15.Domain.MesaServicio;

/// <summary>
/// Catálogo real de prioridades — define el tiempo máximo de atención
/// esperado (SLA en horas) por prioridad. Editable desde Configuración
/// (el número de horas sí es algo que la cooperativa puede necesitar
/// ajustar con el tiempo, no es un valor fijo regulatorio).
/// </summary>
public class PrioridadTicket
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int HorasSla { get; set; }
    public bool Activo { get; set; } = true;
}
