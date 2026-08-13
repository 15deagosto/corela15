namespace Corela15.Domain.Riesgo;

/// <summary>Escala de probabilidad — verificado contra RIESGOOPERATIVO.PROBABILIDAD.</summary>
public class NivelProbabilidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public bool Activo { get; set; } = true;
}
