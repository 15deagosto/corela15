namespace Corela15.Domain.Riesgo;

/// <summary>
/// Snapshot histórico del coeficiente de liquidez — fórmula real SEPS
/// (fondos disponibles / depósitos a corto plazo, confirmado por
/// investigación: "Fondos Disponibles... en relación a Depósitos a Corto
/// Plazo"). Se calcula sobre saldo_contable real (grupo 11 vs grupo 21),
/// nunca inventado. Un registro por corrida, no se sobreescribe — permite
/// ver la evolución del indicador en el tiempo.
/// </summary>
public class IndicadorLiquidez
{
    public Guid Id { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public decimal FondosDisponibles { get; set; }
    public decimal DepositosCortoPlazo { get; set; }
    public decimal Coeficiente { get; set; }
    public decimal MinimoRegulatorio { get; set; }
    public bool CumpleMinimo { get; set; }
}
