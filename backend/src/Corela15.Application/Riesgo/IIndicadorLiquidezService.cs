namespace Corela15.Application.Riesgo;

public record IndicadorLiquidezResult(
    Guid Id, DateTimeOffset Fecha, decimal FondosDisponibles, decimal DepositosCortoPlazo,
    decimal Coeficiente, decimal MinimoRegulatorio, bool CumpleMinimo);

public record ParametroLiquidezResult(decimal MinimoRegulatorio, DateTimeOffset ActualizadoEn, string ActualizadoPor);

public interface IIndicadorLiquidezService
{
    /// <summary>
    /// Calcula el coeficiente de liquidez real (fórmula SEPS: fondos
    /// disponibles / depósitos a corto plazo) desde los saldos contables
    /// actuales — grupo 11 (Fondos disponibles) sobre grupo 21
    /// (Obligaciones con el público: ahorro a la vista + DPF, sin
    /// distinguir plazo remanente todavía — simplificación documentada).
    /// Guarda un snapshot histórico cada vez que se corre, nunca
    /// sobreescribe el anterior.
    /// </summary>
    Task<IndicadorLiquidezResult> CalcularAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IndicadorLiquidezResult>> ListarHistoricoAsync(CancellationToken cancellationToken = default);

    Task<ParametroLiquidezResult> ObtenerParametroAsync(CancellationToken cancellationToken = default);

    Task<ParametroLiquidezResult> ActualizarParametroAsync(decimal minimoRegulatorio, string registradoPor, CancellationToken cancellationToken = default);
}
