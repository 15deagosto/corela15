namespace Corela15.Domain.Seguridad;

/// <summary>N:M rol↔menú — qué módulos puede usar cada rol. Clave compuesta (IdRol, IdMenu).</summary>
public class RolMenu
{
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public int IdMenu { get; set; }
    public Menu Menu { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
