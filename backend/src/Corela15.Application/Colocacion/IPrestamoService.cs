using Corela15.Application.Common;

namespace Corela15.Application.Colocacion;

public interface IPrestamoService
{
    /// <summary>Registra la solicitud, antes del desembolso — todavía no mueve dinero ni genera asiento.</summary>
    Task<SolicitudPrestamoCreadaResult> SolicitarAsync(
        SolicitarPrestamoRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aprueba la solicitud y desembolsa: crea el Prestamo, la tabla de
    /// amortización (sistema francés, sobre la tasa del producto) en
    /// PrestamoRubro, y registra el asiento contable (débito Cartera de
    /// créditos / crédito Caja) vía TipoTransaccion — todo en una sola
    /// transacción atómica (ver patrón documentado en CLAUDE.md).
    /// </summary>
    Task<PrestamoDesembolsadoResult> DesembolsarAsync(
        DesembolsarPrestamoRequest request, CancellationToken cancellationToken = default);
}

public class SolicitudPrestamoInvalidaException(Guid idSolicitud)
    : ReglaDeNegocioException($"La solicitud {idSolicitud} no existe o no está en análisis");

public class TipoPrestamoInvalidoException(int idTipoPrestamo)
    : ReglaDeNegocioException($"El tipo de préstamo {idTipoPrestamo} no existe o está inactivo");

public class MontoFueraDeRangoException(decimal monto, decimal montoMinimo, decimal montoMaximo)
    : ReglaDeNegocioException(
        $"El monto {monto:0.00} está fuera del rango permitido para el producto ({montoMinimo:0.00} - {montoMaximo:0.00})");
