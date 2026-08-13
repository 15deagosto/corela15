namespace Corela15.Domain.Riesgo;

/// <summary>
/// El resultado de cruzar impacto × probabilidad — verificado contra
/// RIESGOOPERATIVO.NIVELRIESGO. Patrón genérico y reutilizable para
/// cualquier tipo de riesgo (operativo, liquidez...), no solo operativo —
/// confirmado en la investigación original.
/// </summary>
public class NivelRiesgo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }
    public decimal RangoInicio { get; set; }
    public decimal RangoFin { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; } = true;
}
