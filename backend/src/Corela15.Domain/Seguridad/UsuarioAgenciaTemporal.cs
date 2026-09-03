using Corela15.Domain.General;

namespace Corela15.Domain.Seguridad;

/// <summary>Reasignación temporal de agencia (cubre a un cajero de otra sucursal, por ejemplo) — verificado contra SEGURIDAD.USUARIO_AGENCIA_TEMPORAL.</summary>
public class UsuarioAgenciaTemporal
{
    public Guid Id { get; set; }

    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int IdAgenciaOrigen { get; set; }
    public Agencia AgenciaOrigen { get; set; } = null!;

    public int IdAgenciaActual { get; set; }
    public Agencia AgenciaActual { get; set; } = null!;

    public DateTimeOffset FechaCaducidad { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
