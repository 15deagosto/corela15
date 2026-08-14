namespace Corela15.Domain.Ahorros;

/// <summary>
/// Bitácora del devengo diario de interés — un registro por cuenta por
/// día. Además de auditoría real (se puede reconstruir exactamente qué se
/// devengó y cuándo), el índice único (Fecha, IdCuenta) es lo que hace que
/// correr el devengo dos veces el mismo día sea seguro por diseño: la
/// segunda corrida simplemente no encuentra cuentas pendientes de ese día.
/// </summary>
public class DevengoInteresLog
{
    public Guid Id { get; set; }

    public DateOnly Fecha { get; set; }

    public Guid IdCuenta { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public decimal SaldoBase { get; set; }
    public decimal TasaAnualAplicada { get; set; }
    public decimal MontoDevengado { get; set; }
}
