namespace Corela15.Domain.Riesgo;

/// <summary>Nivel más alto de la jerarquía de procesos — verificado contra RIESGOOPERATIVO.MACROPROCESO.</summary>
public class MacroProceso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
