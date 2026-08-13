using Corela15.Domain.Contabilidad;

namespace Corela15.Domain.Ahorros;

public enum TipoMovimientoCuenta
{
    Deposito = 1,
    Retiro = 2
}

/// <summary>
/// Bitácora de depósitos/retiros sobre una cuenta de ahorro — espejo
/// simplificado de AHORROS.CUENTA_MOVIMIENTOTRANSACCIONDETALLE, pero
/// registrando el movimiento completo (no solo un snapshot de saldo) y con
/// trazabilidad directa al comprobante contable que generó, para no
/// depender de cruzar tablas para auditar un movimiento.
/// </summary>
public class CuentaMovimiento
{
    public Guid Id { get; set; }

    public Guid IdCuenta { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public TipoMovimientoCuenta Tipo { get; set; }
    public decimal Monto { get; set; }
    public decimal SaldoResultante { get; set; }

    public Guid? IdComprobanteContable { get; set; }
    public ComprobanteContable? ComprobanteContable { get; set; }

    public DateTimeOffset FechaHora { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}
