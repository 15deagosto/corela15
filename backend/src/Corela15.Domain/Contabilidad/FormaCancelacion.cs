namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Forma de cancelación real (cómo se liquida una cuenta por cobrar/pagar
/// u otro movimiento) — verificado contra CONTABILIDAD.FORMA_CANCELACION
/// (6 filas reales: Efectivo/Cheque/Causal/Acreditación a cuenta/Factura
/// servicios profesionales/Transferencia bancaria, esta última ACTIVO=false
/// en la fuente real). Catálogo transversal — lo consumen CuentasPorCobrar,
/// CuentasPorPagar, y potencialmente Nómina/Compras cuando se construyan.
/// </summary>
public class FormaCancelacion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsEfectivo { get; set; }
    public bool EsCheque { get; set; }
    public bool EsTransferencia { get; set; }
    public bool EsCausal { get; set; }
    public bool EsCuenta { get; set; }
    public bool Activo { get; set; } = true;
}
