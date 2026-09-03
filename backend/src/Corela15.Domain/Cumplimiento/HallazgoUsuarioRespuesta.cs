namespace Corela15.Domain.Cumplimiento;

/// <summary>Respuesta real del auditado — verificado contra CUMPLIMIENTO.HALLAZGO_USUARIO_RESPUESTA. Nunca se borra, mismo criterio de auditoría del resto del proyecto.</summary>
public class HallazgoUsuarioRespuesta
{
    public Guid Id { get; set; }

    public Guid IdHallazgoUsuario { get; set; }
    public HallazgoUsuario HallazgoUsuario { get; set; } = null!;

    public string Respuesta { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
}
