using Corela15.Application.Common;

namespace Corela15.Application.Nomina;

/// <summary>
/// Cálculo real de retención en la fuente por Impuesto a la Renta
/// (relación de dependencia) — usa la tabla oficial de tramos del SRI
/// ya sembrada (<c>nomina.tramo_impuesto_renta</c>, período fiscal 2023
/// vigente, verificada contra NOMINA.PERIODO_FISCAL_IMPUESTORENTA).
/// Puramente informativo: no genera ningún asiento ni retiene nada de
/// verdad — es la proyección que RRHH necesita para saber cuánto
/// retener cada mes, mismo criterio que <c>GastoCobranzaService</c>
/// (estimación real, sin efecto contable).
/// </summary>
public interface ICalculoImpuestoRentaService
{
    Task<CalculoImpuestoRentaResult> CalcularAsync(Guid idEmpleado, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CalculoImpuestoRentaResult>> HistorialAsync(Guid idEmpleado, CancellationToken cancellationToken = default);
}

public record CalculoImpuestoRentaResult(
    Guid Id, Guid IdEmpleado, int Anio, decimal IngresoAnualProyectado, decimal AportePersonalIess,
    decimal BaseImponible, decimal ImpuestoCausadoAnual, decimal RetencionMensual, DateOnly FechaCalculo);

public class EmpleadoSinIngresoParaRentaException(Guid idEmpleado)
    : ReglaDeNegocioException($"El empleado {idEmpleado} no tiene ningún rol de pagos procesado — no hay base real para proyectar el ingreso anual");
