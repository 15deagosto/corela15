namespace Corela15.Domain.Riesgo;

/// <summary>Escala de impacto — verificado contra RIESGOOPERATIVO.IMPACTO.</summary>
public class NivelImpacto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public bool Activo { get; set; } = true;
}
