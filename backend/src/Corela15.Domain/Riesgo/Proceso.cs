namespace Corela15.Domain.Riesgo;

/// <summary>
/// Un proceso de la cooperativa (dentro de un macroproceso) — verificado
/// contra RIESGOOPERATIVO.PROCESO. El siguiente nivel de la jerarquía
/// (Subproceso → Actividad) queda fuera de alcance inicial: se agrega
/// cuando el registro de riesgo lo necesite a ese nivel de detalle.
/// </summary>
public class Proceso
{
    public int Id { get; set; }

    public int IdMacroProceso { get; set; }
    public MacroProceso MacroProceso { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public bool Critico { get; set; }
    public bool Activo { get; set; } = true;
}
