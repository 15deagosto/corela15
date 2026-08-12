namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Saldo acumulado por cuenta y período (mes). Lo que consulta cualquier
/// reporte financiero, para no sumar movimientos cada vez — espejo de
/// CONTABILIDAD.SALDOCONTABLE. Se recalcula/actualiza al registrar
/// comprobantes, no se edita a mano.
/// </summary>
public class SaldoContable
{
    public Guid Id { get; set; }

    public Guid IdCuentaContable { get; set; }
    public CuentaContable CuentaContable { get; set; } = null!;

    /// <summary>Primer día del mes del período (ej. 2026-08-01 para agosto 2026).</summary>
    public DateOnly Periodo { get; set; }

    public decimal TotalDebitos { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal SaldoFinal { get; set; }
}
