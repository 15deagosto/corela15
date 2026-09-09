namespace Corela15.Domain.Seguridad;

/// <summary>N:M rol↔dataset de reportería -- qué datasets puede consultar cada rol dentro del módulo. Clave compuesta (IdRol, CodigoDataset).</summary>
public class RolDatasetReporteria
{
    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public string CodigoDataset { get; set; } = string.Empty;
    public DatasetReporteria Dataset { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
