namespace Corela15.Domain.Seguridad;

/// <summary>Asignación temporal de rol con vencimiento real — verificado contra SEGURIDAD.USUARIO_ROL_TEMPORAL (33 filas reales: cubre licencia/vacaciones/reemplazo).</summary>
public class UsuarioRolTemporal
{
    public Guid Id { get; set; }

    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public DateTimeOffset FechaCaducidad { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
