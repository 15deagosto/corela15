namespace Corela15.Domain.Inversion;

/// <summary>Vínculo origen→destino de cada renovación de un DPF.</summary>
public class DepositoRenovacion
{
    public Guid Id { get; set; }

    public Guid IdDepositoOrigen { get; set; }
    public Deposito DepositoOrigen { get; set; } = null!;

    public Guid IdDepositoDestino { get; set; }
    public Deposito DepositoDestino { get; set; } = null!;

    public decimal Valor { get; set; }
    public decimal ValorIncremento { get; set; }
    public DateOnly FechaRenovacion { get; set; }
}
