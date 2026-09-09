namespace Corela15.Domain.Seguridad;

/// <summary>N:M rol↔opción -- mismo patrón exacto que RolTipoEstructura/RolDatasetReporteria.</summary>
public class RolOpcion
{
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public string CodigoOpcion { get; set; } = string.Empty;
    public Opcion Opcion { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
