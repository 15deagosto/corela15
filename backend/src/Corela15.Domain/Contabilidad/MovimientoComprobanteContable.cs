namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Una línea del asiento: débito o crédito a una cuenta, nunca ambos
/// (se aplica como CHECK en la configuración de EF Core).
/// </summary>
public class MovimientoComprobanteContable
{
    public Guid Id { get; set; }

    public Guid IdComprobante { get; set; }
    public ComprobanteContable Comprobante { get; set; } = null!;

    public Guid IdCuentaContable { get; set; }
    public CuentaContable CuentaContable { get; set; } = null!;

    public int NumeroLinea { get; set; }
    public decimal Debito { get; set; }
    public decimal Credito { get; set; }
    public string? Descripcion { get; set; }
}
