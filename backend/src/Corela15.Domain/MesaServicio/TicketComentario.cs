namespace Corela15.Domain.MesaServicio;

/// <summary>Bitácora real de seguimiento de un ticket — nunca se borra, mismo criterio de auditoría del resto del proyecto.</summary>
public class TicketComentario
{
    public Guid Id { get; set; }

    public Guid IdTicket { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public string Comentario { get; set; } = string.Empty;
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
}
