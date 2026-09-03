using Corela15.Application.Common;

namespace Corela15.Application.Obligacion;

/// <summary>
/// Captura real de obligaciones financieras (deuda de la cooperativa con
/// terceros) para alimentar la estructura OF01 (SEPS, Manual Técnico v1.0,
/// 13/03/2026). Las validaciones reproducen los "controles de validación"
/// reales del §4 del manual — nunca inventadas, cada una citada en el
/// código de la implementación.
/// </summary>
public interface IObligacionFinancieraService
{
    Task<ObligacionFinancieraDto> RegistrarAsync(RegistrarObligacionFinancieraRequest request, CancellationToken cancellationToken = default);

    Task<ObligacionFinancieraDto> ActualizarAsync(Guid id, ActualizarObligacionFinancieraRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ObligacionFinancieraDto>> ListarAsync(CancellationToken cancellationToken = default);
}

public record RegistrarObligacionFinancieraRequest(
    int IdAgencia,
    string TipoIdentificacionAcreedor,
    string IdentificacionAcreedor,
    string CodigoPaisAcreedor,
    string NumeroObligacion,
    string DestinoLineaCredito,
    decimal MontoLineaCredito,
    decimal MontoPorUtilizar,
    string CodigoEstado,
    decimal Saldo,
    Guid IdCuentaContable,
    decimal TasaInteres,
    decimal InteresesPorPagar,
    bool PagaComision,
    decimal? TasaInteresComision,
    decimal? ValorComision,
    DateOnly FechaConcesion,
    DateOnly FechaVencimiento,
    string CodigoPeriodicidadPago,
    bool TienePeriodoGracia,
    int? NumeroPeriodosGracia,
    string CodigoClase,
    decimal? ValorVencido,
    string? CodigoFormaCancelacion,
    string? NumeroObligacionAnterior,
    string RegistradoPor);

public record ActualizarObligacionFinancieraRequest(
    decimal MontoPorUtilizar,
    string CodigoEstado,
    decimal Saldo,
    decimal TasaInteres,
    decimal InteresesPorPagar,
    bool PagaComision,
    decimal? TasaInteresComision,
    decimal? ValorComision,
    bool TienePeriodoGracia,
    int? NumeroPeriodosGracia,
    decimal? ValorVencido,
    string? CodigoFormaCancelacion,
    string ModificadoPor);

public record ObligacionFinancieraDto(
    Guid Id,
    string Agencia,
    string TipoIdentificacionAcreedor,
    string IdentificacionAcreedor,
    string CodigoPaisAcreedor,
    string PaisAcreedor,
    string NumeroObligacion,
    string DestinoLineaCredito,
    decimal MontoLineaCredito,
    decimal MontoPorUtilizar,
    string CodigoEstado,
    string Estado,
    decimal Saldo,
    string CodigoCuentaContable,
    string NombreCuentaContable,
    decimal TasaInteres,
    decimal InteresesPorPagar,
    bool PagaComision,
    decimal? TasaInteresComision,
    decimal? ValorComision,
    DateOnly FechaConcesion,
    DateOnly FechaVencimiento,
    string CodigoPeriodicidadPago,
    string PeriodicidadPago,
    bool TienePeriodoGracia,
    int? NumeroPeriodosGracia,
    string CodigoClase,
    string Clase,
    decimal? ValorVencido,
    string? CodigoFormaCancelacion,
    string? FormaCancelacion,
    string? NumeroObligacionAnterior);

public class TipoIdentificacionAcreedorInvalidoException(string tipo)
    : SolicitudInvalidaException($"Tipo de identificación de acreedor '{tipo}' inválido — OF01 solo admite 'R' (RUC) o 'X' (extranjero)");

public class PaisAcreedorInvalidoException(string codigo)
    : ReglaDeNegocioException($"El país acreedor '{codigo}' no existe o no está activo");

public class EstadoObligacionInvalidoException(string codigo)
    : ReglaDeNegocioException($"El estado de obligación '{codigo}' no existe o no está activo");

public class PeriodicidadPagoInvalidaException(string codigo)
    : ReglaDeNegocioException($"La periodicidad de pago '{codigo}' no existe o no está activa");

public class ClaseObligacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La clase de obligación '{codigo}' no existe o no está activa");

public class FormaCancelacionObligacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La forma de cancelación '{codigo}' no existe o no está activa");

public class CuentaContableObligacionInvalidaException(Guid id)
    : ReglaDeNegocioException($"La cuenta contable {id} no existe, no está activa, o no es de detalle del grupo 26 (Obligaciones Financieras)");

public class MontoPorUtilizarExcedeLineaException(decimal montoPorUtilizar, decimal montoLinea)
    : ReglaDeNegocioException($"El monto por utilizar ({montoPorUtilizar:0.00}) no puede exceder el monto de la línea de crédito ({montoLinea:0.00})");

public class SaldoObligacionCanceladaException()
    : ReglaDeNegocioException("Una obligación en estado Cancelada (CN) debe tener saldo exacto en cero");

public class ObligacionDuplicadaException(string numeroObligacion)
    : ReglaDeNegocioException($"Ya existe una obligación activa con el número '{numeroObligacion}' para el mismo acreedor y cuenta contable");

public class DatosComisionIncompletosException()
    : SolicitudInvalidaException("Si la obligación paga comisión, la tasa y el valor de la comisión son obligatorios");

public class DatosPeriodoGraciaIncompletosException()
    : SolicitudInvalidaException("Si la obligación tiene período de gracia, el número de períodos es obligatorio");

public class ValorVencidoRequeridoException()
    : SolicitudInvalidaException("Una obligación en estado Vencida (VN) requiere el valor vencido");

public class FormaCancelacionRequeridaException()
    : SolicitudInvalidaException("Una obligación en estado Cancelada (CN) requiere la forma de cancelación");

public class ObligacionFinancieraNoExisteException(Guid id)
    : ReglaDeNegocioException($"La obligación financiera {id} no existe");
