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

    /// <summary>
    /// Paga la próxima cuota pendiente (la de menor número aún no pagada):
    /// marca capital e interés de esa cuota como cobrados, reduce
    /// Prestamo.Saldo, y registra el asiento (débito Caja / crédito Cartera
    /// de créditos por el capital / crédito Intereses ganados por el
    /// interés). Si la cuota está vencida (hoy después de su fecha de
    /// vencimiento), cobra además interés de mora real sobre el capital
    /// vencido — `capital × 10% anual × días de mora / 365`, el techo real
    /// vigente según la "Norma para tasas de interés por mora" del BCE
    /// (escala de hasta 10%, calculada solo sobre el capital vencido, desde
    /// la fecha de no pago hasta la fecha de cumplimiento — se usa el techo
    /// fijo del 10% como simplificación consciente: la norma real gradúa el
    /// porcentaje según el perfil de riesgo/comportamiento de pago del
    /// socio, algo que este core no modela todavía). Si era la última
    /// cuota, el préstamo pasa a Cancelado. Todo en una sola transacción.
    /// </summary>
    Task<PagoCuotaRegistradoResult> PagarCuotaAsync(
        PagarCuotaRequest request, CancellationToken cancellationToken = default);
}

public class SolicitudPrestamoInvalidaException(Guid idSolicitud)
    : ReglaDeNegocioException($"La solicitud {idSolicitud} no existe o no está en análisis");

public class TipoPrestamoInvalidoException(int idTipoPrestamo)
    : ReglaDeNegocioException($"El tipo de préstamo {idTipoPrestamo} no existe o está inactivo");

public class MontoFueraDeRangoException(decimal monto, decimal montoMinimo, decimal montoMaximo)
    : ReglaDeNegocioException(
        $"El monto {monto:0.00} está fuera del rango permitido para el producto ({montoMinimo:0.00} - {montoMaximo:0.00})");

public class PrestamoInvalidoException(Guid idPrestamo)
    : ReglaDeNegocioException($"El préstamo {idPrestamo} no existe o no está vigente");

public class PrestamoSinCuotasPendientesException(Guid idPrestamo)
    : ReglaDeNegocioException($"El préstamo {idPrestamo} no tiene cuotas pendientes de pago");

public class TasaExcedeTechoBceException(decimal tasaAnual, decimal tasaMaxima, string segmento)
    : ReglaDeNegocioException(
        $"La tasa del producto ({tasaAnual:0.00%}) excede el techo BCE vigente para {segmento} ({tasaMaxima:0.00%})");

public class SinTechoBceConfiguradoException(string segmento)
    : ReglaDeNegocioException($"No hay una tasa techo BCE vigente configurada para el segmento '{segmento}'");
