namespace Corela15.Domain.Seguridad;

/// <summary>N:M rol↔tipo de estructura — qué estructuras puede generar cada rol, dentro del módulo. Clave compuesta (IdRol, CodigoTipoEstructura).</summary>
public class RolTipoEstructura
{
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public string CodigoTipoEstructura { get; set; } = string.Empty;
    public TipoEstructura TipoEstructura { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
