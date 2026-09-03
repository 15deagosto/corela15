using Corela15.Application.Common;

namespace Corela15.Application.Colocacion;

public interface IPrestamoService
{
    /// <summary>Registra la solicitud, antes del desembolso — todavía no mueve dinero ni genera asiento.</summary>
    Task<SolicitudPrestamoCreadaResult> SolicitarAsync(
        SolicitarPrestamoRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Comité de Crédito: aprueba la solicitud (EnAnalisis→Aprobada) con el
    /// monto que autoriza — puede ser distinto al solicitado, siempre
    /// dentro del rango del producto. Solo después de este paso se puede
    /// desembolsar; antes, DesembolsarAsync no distinguía este estado.
    /// </summary>
    Task<SolicitudAprobadaResult> AprobarSolicitudAsync(
        AprobarSolicitudRequest request, CancellationToken cancellationToken = default);

    /// <summary>Comité de Crédito: rechaza la solicitud (EnAnalisis→Rechazada), con motivo obligatorio.</summary>
    Task RechazarSolicitudAsync(
        RechazarSolicitudRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Desembolsa una solicitud ya aprobada por Comité: crea el Prestamo, la tabla de
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

    /// <summary>
    /// Carga un rubro manual (ej. Gastos Judiciales, Notificaciones,
    /// Certificado de Gravamen) a un préstamo vigente — verificado contra
    /// el motor real de Softbank (FINANCIERO.TRANSACCION_COLOCACION id=63
    /// "CUENTAS POR COBRAR RUBROS CARTERA", código PRA). Se agrega como
    /// una fila más de PrestamoRubro (fuera de la tabla de amortización
    /// normal, en el próximo número de cuota disponible) y, si el rubro
    /// está marcado <c>EsCuentaPorCobrar</c> (7 rubros reales lo tienen:
    /// Certificado de Gravamen, Notificaciones, Inicio/Demanda Judicial,
    /// Cobranzas, Gastos, Gastos Judiciales), genera automáticamente una
    /// cuenta por cobrar real vinculada (mismo motor REG-CXC ya construido
    /// para Tesorería — nunca un asiento inventado), quedando registrado el
    /// vínculo préstamo↔rubro↔CxC en PrestamoRubroCuentaPorCobrar (espejo de
    /// COLOCACION.PRESTAMO_RUBRO_CUENTAPORCOBRAR real). Si el rubro no
    /// genera CxC, solo queda el cargo informativo en PrestamoRubro, sin
    /// asiento.
    /// </summary>
    Task<RubroManualCargadoResult> CargarRubroManualAsync(
        CargarRubroManualRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lista los rubros manuales cargados a un préstamo, con su CxC vinculada si aplica.</summary>
    Task<IReadOnlyList<RubroManualCargadoResult>> ListarRubrosManualesAsync(
        Guid idPrestamo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Diferimiento de cuotas real (período de gracia) — verificado
    /// contra COLOCACION.PRESTAMO_CUOTADIFERIDA_AGREGADA. Desplaza la
    /// fecha de vencimiento de todas las cuotas de capital todavía
    /// pendientes por la cantidad de días indicada, y deja constancia
    /// del evento. No genera ningún asiento (no mueve dinero, solo
    /// reprograma fechas).
    /// </summary>
    Task<DiferimientoCuotaResult> DiferirCuotasAsync(
        DiferirCuotasRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lista el historial de diferimientos de un préstamo.</summary>
    Task<IReadOnlyList<DiferimientoCuotaResult>> ListarDiferimientosAsync(
        Guid idPrestamo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Castigo formal real de cartera — verificado contra COLOCACION.
    /// PRESTAMO_CASTIGADO. Marca el préstamo como Castigado, registra el
    /// evento con el saldo transferido, y genera el asiento contable real
    /// (débito `1499` Provisión para créditos incobrables / crédito
    /// `1401` Cartera de créditos, reversando la cartera contra la
    /// provisión ya constituida — partida doble estándar de castigo).
    /// </summary>
    Task<PrestamoCastigadoResult> CastigarAsync(
        CastigarPrestamoRequest request, CancellationToken cancellationToken = default);
}

public record DiferirCuotasRequest(Guid IdPrestamo, int DiasDiferidos, string Comentario, string RegistradoPor);

public record DiferimientoCuotaResult(Guid Id, int DiasDiferidos, string Comentario, DateOnly Fecha, string RegistradoPor);

public record CastigarPrestamoRequest(Guid IdPrestamo, string Comentario, string RegistradoPor);

public record PrestamoCastigadoResult(Guid Id, decimal SaldoTransferido, DateOnly Fecha, Guid IdComprobante);

public class DiasDiferidosInvalidosException(int dias)
    : SolicitudInvalidaException($"Los días a diferir ({dias}) deben ser mayores a cero");

public class PrestamoYaCastigadoException(Guid idPrestamo)
    : ReglaDeNegocioException($"El préstamo {idPrestamo} ya fue castigado");

public record CargarRubroManualRequest(Guid IdPrestamo, int IdRubro, decimal Monto, string Detalle, string RegistradoPor);

public record RubroManualCargadoResult(
    Guid IdPrestamoRubro, string NombreRubro, decimal Monto, DateOnly Fecha, string Estado,
    Guid? IdCuentaPorCobrar, Guid? IdComprobante);

public class RubroInvalidoParaCargoManualException(int idRubro)
    : ReglaDeNegocioException($"El rubro {idRubro} no existe, está inactivo, o no es un rubro manual real (COLOCACION.RUBRO)");

public class MontoRubroManualInvalidoException(decimal monto)
    : SolicitudInvalidaException($"El monto {monto:0.00} debe ser mayor a cero");

public class SolicitudPrestamoInvalidaException(Guid idSolicitud)
    : ReglaDeNegocioException($"La solicitud {idSolicitud} no existe o no está aprobada por Comité de Crédito");

public class SolicitudPrestamoNoEnAnalisisException(Guid idSolicitud)
    : ReglaDeNegocioException($"La solicitud {idSolicitud} no existe o ya no está en análisis (ya fue aprobada/rechazada/desembolsada)");

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

public class TipoConvenioInvalidoException(string codigoTipoConvenio)
    : ReglaDeNegocioException($"El convenio '{codigoTipoConvenio}' no existe o está inactivo");
