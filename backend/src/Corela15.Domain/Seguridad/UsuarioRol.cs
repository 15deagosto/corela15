namespace Corela15.Domain.Seguridad;

/// <summary>N:M usuario↔rol. Un usuario puede tener varios roles activos a la vez.</summary>
public class UsuarioRol
{
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public bool Activo { get; set; } = true;
    public DateTimeOffset AsignadoEn { get; set; }
}
