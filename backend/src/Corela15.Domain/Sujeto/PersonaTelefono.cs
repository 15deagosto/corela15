namespace Corela15.Domain.Sujeto;

/// <summary>
/// Teléfono de una persona — verificado contra SUJETO.PERSONA_TELEFONO
/// (14.559 filas reales, una persona puede tener varios). `NotificacionSms`
/// es el campo real que decide si ese número recibe alertas del sistema
/// (mismo concepto que ya usa `AutorizacionTransaccion`/`AlertaListaControl`
/// para saber a quién avisar, aunque el envío de SMS en sí queda fuera de
/// alcance).
/// </summary>
public class PersonaTelefono
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public string Telefono { get; set; } = string.Empty;
    public bool EsTelefonoMovil { get; set; }
    public bool EsPrincipal { get; set; }
    public bool NotificacionSms { get; set; }
    public bool Activo { get; set; } = true;
}
