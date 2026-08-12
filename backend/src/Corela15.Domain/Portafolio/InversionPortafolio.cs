using Corela15.Domain.General;

namespace Corela15.Domain.Portafolio;

public enum EstadoInversionPortafolio
{
    Vigente = 1,
    Vencida = 2
}

/// <summary>
/// Portafolio de inversiones propias de la cooperativa (dónde coloca su
/// liquidez excedente — no confundir con los DPF de los socios, Nivel 3) —
/// grupo CUC 13. Espejo simplificado de PORTAFOLIO.INVERSIONPORTAFOLIO.
/// </summary>
public class InversionPortafolio
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public string Institucion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal Tasa { get; set; }
    public DateOnly FechaInversion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public EstadoInversionPortafolio Estado { get; set; } = EstadoInversionPortafolio.Vigente;
}
