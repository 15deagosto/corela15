namespace Corela15.Domain.Ahorros;

/// <summary>Catálogo de productos de ahorro (Ahorro Vista, Certificados, Ahorro Infantil...).</summary>
public class TipoCuenta
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Si este producto admite débito automático de cuota de préstamo.</summary>
    public bool PermiteDebitoPrestamo { get; set; }

    /// <summary>Saldo mínimo exigido cuando la cuenta tiene débito de préstamo activo.</summary>
    public decimal? SaldoMinimoConPrestamo { get; set; }

    public bool Activo { get; set; } = true;
}
