using Corela15.Domain.Common;

namespace Corela15.Domain.Ahorros;

/// <summary>
/// Saldo real por cuenta × ítem. El campo <see cref="AcreditaPrestamo"/> es
/// una de las tres configuraciones independientes del auto-débito de cuota
/// de préstamo por SPI (junto con TipoCuenta.PermiteDebitoPrestamo acá, y
/// COLOCACION.PRESTAMO.DEBITOSPI que vivirá en Nivel 3) — el aprendizaje real
/// de la investigación original es que estas tres deben validarse como una
/// sola fuente de verdad cuando se construya el caso de uso de auto-débito
/// en Nivel 3, no quedar como flags independientes que un proceso puede
/// ignorar (fue exactamente el bug encontrado en producción).
/// </summary>
public class CuentaItemSaldo : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid IdCuenta { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public int IdItemSaldo { get; set; }
    public ItemSaldo ItemSaldo { get; set; } = null!;

    public decimal Saldo { get; set; }
    public bool AcreditaPrestamo { get; set; }
}
