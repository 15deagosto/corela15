namespace Corela15.Application.Colocacion;

public record DetalleCategoriaProvision(
    string Codigo, string Nombre, int CantidadPrestamos, decimal SaldoCartera, decimal ProvisionRequerida);

public record ProvisionCarteraCalculadaResult(
    decimal ProvisionRequeridaTotal,
    decimal ProvisionAcumuladaAnterior,
    decimal IncrementoRegistrado,
    Guid? IdComprobanteContable,
    IReadOnlyList<DetalleCategoriaProvision> Detalle);

public interface IProvisionCarteraService
{
    /// <summary>
    /// Calcula la provisión de cartera requerida (Norma para la Gestión
    /// del Riesgo de Crédito en las COAC, Art. 44): para cada préstamo
    /// vigente, calcula días de mora reales (hoy - fecha de vencimiento
    /// de la cuota de capital impaga más antigua), lo clasifica en su
    /// categoría de riesgo (A1-E) y aplica el % de provisión de esa
    /// categoría sobre el saldo. Solo registra el asiento (débito 4402
    /// Gasto / crédito 1499 Provisión) cuando la provisión requerida
    /// total es MAYOR que la ya acumulada — un incremento. Si diera
    /// menor (la cartera mejoró), no reversa automáticamente: una
    /// reducción de provisión requiere revisión manual explícita, no es
    /// una decisión que deba tomar un batch solo. Diseñado para correrse
    /// periódicamente (mensual como mínimo), no por préstamo individual.
    /// </summary>
    Task<ProvisionCarteraCalculadaResult> EjecutarCalculoAsync(string registradoPor, CancellationToken cancellationToken = default);
}
