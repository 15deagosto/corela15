namespace Corela15.Domain.Ahorros;

/// <summary>Catálogo de productos de ahorro (Ahorro Vista, Certificados, Ahorro Infantil...).</summary>
public class TipoCuenta
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Saldo mínimo general del producto — Softbank solo valida un mínimo
    /// cuando la cuenta tiene débito de préstamo asociado
    /// (<see cref="SaldoMinimoConPrestamo"/>); en la práctica real de una
    /// cooperativa, la mayoría de productos de ahorro exige un mínimo
    /// siempre (ej. aporte de encaje), no solo cuando hay préstamo. Falencia
    /// corregida acá: se valida en todo retiro, tenga o no préstamo.
    /// </summary>
    public decimal SaldoMinimo { get; set; }

    /// <summary>Si este producto admite débito automático de cuota de préstamo.</summary>
    public bool PermiteDebitoPrestamo { get; set; }

    /// <summary>Saldo mínimo adicional exigido cuando la cuenta tiene débito de préstamo activo.</summary>
    public decimal? SaldoMinimoConPrestamo { get; set; }

    public bool Activo { get; set; } = true;
}
