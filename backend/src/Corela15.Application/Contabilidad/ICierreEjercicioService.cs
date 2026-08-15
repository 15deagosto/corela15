using Corela15.Application.Common;

namespace Corela15.Application.Contabilidad;

public record CerrarEjercicioRequest(int Anio, string RegistradoPor);

public record CierreEjercicioResult(
    int Anio, DateTimeOffset FechaCierre, decimal TotalIngresos, decimal TotalGastos,
    decimal Utilidad, Guid IdComprobanteContable);

public record CierreEjercicioListItem(
    int Anio, DateTimeOffset FechaCierre, decimal TotalIngresos, decimal TotalGastos,
    decimal Utilidad, string CerradoPor);

public interface ICierreEjercicioService
{
    /// <summary>
    /// Liquida el resultado del año: suma el saldo acumulado de todas las
    /// cuentas de Ingresos (grupo 5) y Gastos (grupo 4) a través de los
    /// períodos del año, genera un comprobante que zera cada una de esas
    /// cuentas (débito a Ingresos, crédito a Gastos) y traslada la
    /// diferencia a patrimonio: crédito `3603` Utilidad del ejercicio si
    /// es positiva, débito `3604` (Pérdida del ejercicio) si es negativa.
    /// No implementa el cierre de resultados acumulados de años
    /// anteriores (`3601`/`3602`) — solo la liquidación del ejercicio que
    /// se cierra, documentado a propósito como fuera de alcance.
    /// </summary>
    Task<CierreEjercicioResult> CerrarAsync(CerrarEjercicioRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CierreEjercicioListItem>> ListarAsync(CancellationToken cancellationToken = default);
}

public class EjercicioYaCerradoException(int anio)
    : ReglaDeNegocioException($"El ejercicio {anio} ya fue cerrado");

public class EjercicioSinMovimientosException(int anio)
    : ReglaDeNegocioException($"El ejercicio {anio} no tiene movimientos de ingresos o gastos que liquidar");
