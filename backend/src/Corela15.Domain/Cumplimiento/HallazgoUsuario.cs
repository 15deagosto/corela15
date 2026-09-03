using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Cumplimiento;

/// <summary>Auditado asignado a un hallazgo — verificado contra CUMPLIMIENTO.HALLAZGO_USUARIO.</summary>
public class HallazgoUsuario
{
    public Guid Id { get; set; }

    public Guid IdHallazgo { get; set; }
    public Hallazgo Hallazgo { get; set; } = null!;

    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public bool EstadoRespondido { get; set; }
    public bool Activo { get; set; } = true;
}
