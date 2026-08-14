using Corela15.Application.Common;

namespace Corela15.Application.Contabilidad;

public record CerrarPeriodoRequest(DateOnly Periodo, string RegistradoPor);

public record PeriodoCerradoResult(DateOnly Periodo, DateTimeOffset FechaCierre);

public record PeriodoContableListItem(DateOnly Periodo, bool Cerrado, DateTimeOffset? FechaCierre, string? CerradoPor);

public interface ICierrePeriodoService
{
    /// <summary>
    /// Cierra un período contable (mes calendario) — a partir de ese
    /// momento, ComprobanteContableService.RegistrarAsync rechaza
    /// cualquier comprobante nuevo con fecha dentro de ese período. No
    /// implementa cierre de resultados (utilidad del ejercicio →
    /// patrimonio, típico del cierre anual) — solo el bloqueo operativo
    /// del período, que es lo que impide contabilizar retroactivo.
    /// </summary>
    Task<PeriodoCerradoResult> CerrarAsync(CerrarPeriodoRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PeriodoContableListItem>> ListarAsync(CancellationToken cancellationToken = default);
}

public class PeriodoYaCerradoException(DateOnly periodo)
    : ReglaDeNegocioException($"El período {periodo:yyyy-MM} ya está cerrado");

public class PeriodoContableCerradoException(DateOnly periodo)
    : ReglaDeNegocioException($"El período {periodo:yyyy-MM} está cerrado — no se pueden registrar comprobantes con fecha dentro de ese período");
