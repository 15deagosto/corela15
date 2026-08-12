namespace Corela15.Domain.Ahorros;

/// <summary>Qué ítems de saldo aplican a cada producto (TipoCuenta).</summary>
public class TipoCuentaItemSaldo
{
    public int IdTipoCuenta { get; set; }
    public TipoCuenta TipoCuenta { get; set; } = null!;

    public int IdItemSaldo { get; set; }
    public ItemSaldo ItemSaldo { get; set; } = null!;
}
