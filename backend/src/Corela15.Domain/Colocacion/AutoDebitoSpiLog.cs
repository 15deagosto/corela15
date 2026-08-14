namespace Corela15.Domain.Colocacion;

/// <summary>
/// Bitácora del auto-débito de cuota por SPI — un registro por préstamo por
/// día, se procese o se omita, y en ambos casos con el motivo explícito.
/// Es la corrección directa del incidente real documentado en
/// 01-contexto-origen.md y en <see cref="Prestamo.DebitoSpi"/>: el proceso
/// original no dejaba rastro de por qué un préstamo sí o no fue debitado,
/// lo que hizo casi imposible diagnosticar la inconsistencia sin cruzar el
/// historial temporal nativo de SQL Server a mano. Acá cada corrida queda
/// visible, y el índice único (Fecha, IdPrestamo) además hace que correr el
/// batch dos veces el mismo día sea seguro por diseño — mismo patrón que
/// DevengoInteresLog.
/// </summary>
public class AutoDebitoSpiLog
{
    public Guid Id { get; set; }

    public DateOnly Fecha { get; set; }

    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public int? NumeroCuota { get; set; }
    public Guid? IdCuenta { get; set; }

    public bool Debitado { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}
