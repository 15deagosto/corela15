namespace Corela15.Domain.Cumplimiento;

/// <summary>Catálogo real de estados del hallazgo — verificado contra CUMPLIMIENTO.ESTADO_HALLAZGO. El código "111" de la fuente real (nombre corrupto, un dato con nombre de persona en vez de estado, ACTIVO=false) se excluyó deliberadamente, mismo criterio que la anomalía "671" del CUC.</summary>
public class EstadoHallazgo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
