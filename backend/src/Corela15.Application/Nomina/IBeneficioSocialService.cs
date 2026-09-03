using Corela15.Application.Common;

namespace Corela15.Application.Nomina;

/// <summary>
/// Décimo Tercero, Décimo Cuarto, Fondos de Reserva y Provisión de
/// Vacaciones — los 4 beneficios sociales obligatorios en Ecuador,
/// verificados contra NOMINA.EMPLEADO_DECIMOTERCERO/_DECIMOCUARTO/
/// _FONDOSRESERVA/_PROVISION_VACACION (205/218/29/3.223 filas reales).
/// Devengo mensual idempotente (mismo patrón que
/// <c>DevengoInteresService</c>), liquidación real por empleado.
/// </summary>
public interface IBeneficioSocialService
{
    Task<DevengoBeneficiosResult> EjecutarDevengoAsync(string registradoPor, CancellationToken cancellationToken = default);

    Task<PagoBeneficioResult> PagarDecimoTerceroAsync(Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default);
    Task<PagoBeneficioResult> PagarDecimoCuartoAsync(Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default);
    Task<PagoBeneficioResult> PagarFondosReservaAsync(Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default);
    Task<PagoBeneficioResult> PagarVacacionesAsync(Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aporte Patronal al IESS — mismo devengo mensual idempotente que
    /// los 4 beneficios sociales, pero es un gasto real de la cooperativa
    /// (nunca se le paga al empleado, se liquida contra el IESS).
    /// </summary>
    Task<PagoBeneficioResult> PagarAportePatronalAsync(Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default);
}

public record DevengoBeneficiosResult(
    DateOnly Fecha, int EmpleadosProcesados, decimal TotalDecimoTercero, decimal TotalDecimoCuarto,
    decimal TotalFondosReserva, decimal TotalProvisionVacaciones, decimal TotalAportePatronal, IReadOnlyList<Guid> IdsComprobante);

public record PagoBeneficioResult(Guid IdEmpleado, string Beneficio, decimal MontoPagado, Guid IdComprobante);

public class EmpleadoSinBeneficioException(Guid idEmpleado, string beneficio)
    : ReglaDeNegocioException($"El empleado {idEmpleado} no tiene un registro de {beneficio} todavía (aún no se ejecutó el devengo)");

public class BeneficioSinSaldoException(Guid idEmpleado, string beneficio)
    : ReglaDeNegocioException($"El empleado {idEmpleado} no tiene saldo acumulado de {beneficio} pendiente de pago");
