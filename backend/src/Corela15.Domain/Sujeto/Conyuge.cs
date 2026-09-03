namespace Corela15.Domain.Sujeto;

/// <summary>
/// Cónyuge de una persona natural — verificado contra SUJETO.CONYUGE
/// (4.763 filas reales). El cónyuge también es una Persona real del
/// sistema (misma tabla `sujeto.persona`, igual que en Softbank) — no un
/// campo de texto libre. Requisito real de la Norma de Prevención de
/// Lavado de Activos: el cónyuge de un socio con garantía personal (ver
/// SolicitudPrestamoGarantia) puede necesitar dar su consentimiento si es
/// casado, dato que Softbank sí modela (PRESTAMO_GARANTIAPERSONAL_
/// ESTADOCIVIL) pero que este core no cubre — ver la nota de pendiente en
/// SolicitudPrestamoGarantia.cs, sigue sin resolverse.
/// </summary>
public class Conyuge
{
    public Guid Id { get; set; }

    public Guid IdPersonaNatural { get; set; }
    public PersonaNatural PersonaNatural { get; set; } = null!;

    public Guid IdPersonaConyuge { get; set; }
    public Persona PersonaConyuge { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
