namespace Corela15.Domain.MesaServicio;

/// <summary>Bitácora real de transición de estado del ticket — nunca se borra, mismo criterio de auditoría del resto del proyecto (ej. AvanceRiesgoEtapa, HallazgoEtapa).</summary>
public class TicketEtapaHist
{
    public Guid Id { get; set; }

    public Guid IdTicket { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public string CodigoEstadoAnterior { get; set; } = string.Empty;
    public string CodigoEstadoNuevo { get; set; } = string.Empty;

    public string? Comentario { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
}
