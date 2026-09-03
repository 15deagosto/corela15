namespace Corela15.Domain.Portafolio;

/// <summary>
/// Cadena real de renovación origen→destino — verificado contra
/// PORTAFOLIO.INVERSION_RENOVACION (16 filas reales, bridge simple),
/// mismo patrón que inversion.deposito_renovacion (Nivel 3).
/// </summary>
public class InversionRenovacion
{
    public Guid Id { get; set; }

    public Guid IdInversionOrigen { get; set; }
    public InversionPortafolio InversionOrigen { get; set; } = null!;

    public Guid IdInversionDestino { get; set; }
    public InversionPortafolio InversionDestino { get; set; } = null!;

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}
