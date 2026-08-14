namespace Corela15.Domain.Riesgo;

/// <summary>
/// Fila única con el mínimo regulatorio del coeficiente de liquidez.
/// Sembrado en 25% como valor de referencia conservador para una
/// cooperativa Segmento 2 — el porcentaje exacto de la norma real (Nota
/// Técnica de la Norma para la Administración de Riesgo de Liquidez,
/// SEPS) no se pudo verificar columna por columna porque el PDF oficial
/// no se pudo extraer programáticamente (mismo problema que con la
/// matriz de provisiones) — editable acá mientras tanto, no hardcodeado
/// en el motor de cálculo.
/// </summary>
public class ParametroLiquidez
{
    public int Id { get; set; }
    public decimal MinimoRegulatorio { get; set; }
    public DateTimeOffset ActualizadoEn { get; set; }
    public string ActualizadoPor { get; set; } = string.Empty;
}
