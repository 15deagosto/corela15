namespace Corela15.Domain.Auditoria;

/// <summary>Áreas/procesos sujetos a auditoría interna — verificado contra AUDITORIA.AREA_AUDITORIA.</summary>
public class AreaAuditoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
