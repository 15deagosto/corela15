namespace Corela15.Domain.Cajas;

/// <summary>
/// Bitácora real de cada movimiento de efectivo por transacción —
/// verificado contra CAJAS.VENTANILLA_ITEMCAJA_MOVIMIENTOTRANSACCION (la
/// tabla real, sin la granularidad por denominación de
/// `_MOVIMIENTOTRANSACCION_EFECTIVO`, 2,2M filas en Softbank —
/// deliberadamente fuera de alcance, ver nota en Ventanilla.cs: acá
/// interesa el saldo real de la ventanilla para el cuadre, no reconstruir
/// cuántos billetes de $20 exactos se movieron en cada transacción, algo
/// que ningún caso de uso de este core necesita hoy). A diferencia de
/// Softbank (que referencia `IDMOVIMIENTOTRANSACCIONDETALLE`, un ID
/// interno opaco), acá referencia directo el `ComprobanteContable` real
/// que generó el movimiento — trazabilidad completa sin cruzar tablas.
/// Nunca se borra, mismo criterio de auditoría del resto del core.
/// </summary>
public class VentanillaItemCajaMovimiento
{
    public Guid Id { get; set; }

    public Guid IdVentanillaItemCaja { get; set; }
    public VentanillaItemCaja VentanillaItemCaja { get; set; } = null!;

    /// <summary>Positivo = entra efectivo a la ventanilla (depósito, pago de cuota...); negativo = sale (retiro, desembolso...).</summary>
    public decimal Valor { get; set; }

    /// <summary>Saldo de VentanillaItemCaja inmediatamente después de este movimiento.</summary>
    public decimal SaldoResultante { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public Guid? IdComprobanteContable { get; set; }
    public Corela15.Domain.Contabilidad.ComprobanteContable? ComprobanteContable { get; set; }

    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
}
