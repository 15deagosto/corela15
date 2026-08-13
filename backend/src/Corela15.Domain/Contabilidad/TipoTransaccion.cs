namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Motor contable configurable: qué transacción dispara qué asiento, como
/// una sola fila legible, en vez de la cadena de tres tablas cruzadas que
/// usa Softbank (FINANCIERO.TRANSACCION → CONTABILIDAD.GENERADOR_CONTABLE →
/// CONTABILIDAD.CAUSAL) — la propia investigación original documentó que
/// entender qué transacción dispara qué asiento ahí requería cruzar las tres
/// tablas a mano (ver 02-arquitectura-datos-40-modulos.md, Nivel 1.3,
/// aprendizaje #3: "el generador contable debe ser explícito y trazable").
///
/// Cualquier módulo que necesite generar un asiento automático (depósito,
/// retiro, desembolso de préstamo, pago de cuota...) declara acá qué
/// TipoTransaccion usa, y de qué cuenta a qué cuenta mueve el dinero —
/// visible en una consulta simple, no reconstruible solo con ingeniería
/// inversa de datos.
/// </summary>
public class TipoTransaccion
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public Guid IdCuentaContableDebito { get; set; }
    public CuentaContable CuentaContableDebito { get; set; } = null!;

    public Guid IdCuentaContableCredito { get; set; }
    public CuentaContable CuentaContableCredito { get; set; } = null!;

    public int IdTipoComprobante { get; set; }
    public TipoComprobanteContable TipoComprobante { get; set; } = null!;

    /// <summary>
    /// +1 si esta transacción suma al saldo disponible de la cuenta afectada
    /// (ej. depósito), -1 si resta (ej. retiro). Explícito y configurable a
    /// propósito — no se infiere del lado débito/crédito porque eso depende
    /// de la naturaleza contable de la cuenta (una de ahorro es pasivo, un
    /// desembolso de préstamo futuro sería activo, etc.), y esa inferencia
    /// implícita es justo el tipo de cosa que hace ilegible un motor contable.
    /// </summary>
    public int SignoSaldoCuenta { get; set; }

    public bool Activo { get; set; } = true;
}
