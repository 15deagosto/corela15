namespace Corela15.Domain.Colocacion;

/// <summary>
/// Evento formal real de castigo de cartera — verificado contra
/// COLOCACION.PRESTAMO_CASTIGADO (55 filas reales). Complementa el
/// estado `Castigado` ya modelado en `Prestamo.Estado`: acá se
/// registra el evento en sí (cuándo, quién, saldo transferido) y se
/// genera el asiento contable real de castigo — antes el estado
/// existía pero nada lo llevaba ahí ni dejaba rastro del evento.
/// </summary>
public class PrestamoCastigado
{
    public Guid Id { get; set; }

    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public decimal SaldoTransferido { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public Guid? IdComprobante { get; set; }
}
