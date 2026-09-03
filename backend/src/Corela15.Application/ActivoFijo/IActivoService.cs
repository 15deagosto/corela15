using Corela15.Application.Common;

namespace Corela15.Application.ActivoFijo;

public interface IActivoService
{
    /// <summary>Registra un activo fijo nuevo, con responsable inicial opcional.</summary>
    Task<ActivoRegistradoResult> RegistrarAsync(RegistrarActivoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reasigna el responsable de custodia vigente de un activo (desactiva la asignación anterior).</summary>
    Task AsignarResponsableAsync(AsignarResponsableRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Corre la depreciación mensual real de todos los activos vigentes que
    /// deprecian, agrupados por agencia — un comprobante por agencia con
    /// una línea débito/crédito por cada par (cuenta gasto, cuenta
    /// depreciación acumulada) real de su Estructura. Idempotente por
    /// Agencia+Mes (índice único en DepreciacionAgencia), mismo patrón que
    /// DevengoInteresService.
    /// </summary>
    Task<DepreciacionEjecutadaResult> EjecutarDepreciacionAsync(string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>Crea un traslado de activo, pendiente de procesar.</summary>
    Task<TrasladoCreadoResult> CrearTrasladoAsync(CrearTrasladoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Procesa (ejecuta) un traslado pendiente: reasigna agencia/responsable del activo.</summary>
    Task ProcesarTrasladoAsync(Guid idTraslado, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>Anula un traslado pendiente sin ejecutarlo.</summary>
    Task AnularTrasladoAsync(Guid idTraslado, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>Ingresa una solicitud de baja de un activo vigente.</summary>
    Task<SolicitudBajaCreadaResult> SolicitarBajaAsync(SolicitarBajaRequest request, CancellationToken cancellationToken = default);

    /// <summary>Autoriza una solicitud de baja ya ingresada.</summary>
    Task AutorizarBajaAsync(Guid idSolicitud, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Procesa una baja autorizada: revierte la depreciación acumulada,
    /// reconoce la pérdida en libros (débito 4701 Pérdida en venta de
    /// bienes) y saca el activo de la cuenta de propiedades y equipo —
    /// marca el activo Baja y la solicitud Procesada.
    /// </summary>
    Task<BajaProcesadaResult> ProcesarBajaAsync(Guid idSolicitud, string registradoPor, CancellationToken cancellationToken = default);
}

public class EstructuraInvalidaException(int idEstructura)
    : ReglaDeNegocioException($"La categoría de activo {idEstructura} no existe o está inactiva");

public class ResponsableInvalidoException(Guid idResponsable)
    : ReglaDeNegocioException($"El responsable {idResponsable} no existe o está inactivo");

public class ActivoInvalidoException(Guid idActivo)
    : ReglaDeNegocioException($"El activo {idActivo} no existe o no está vigente");

public class TrasladoActivoInvalidoException(Guid idTraslado)
    : ReglaDeNegocioException($"El traslado {idTraslado} no existe o ya no está pendiente");

public class MotivoTrasladoInvalidoException(int idMotivo)
    : ReglaDeNegocioException($"El motivo de traslado {idMotivo} no existe o está inactivo");

public class MotivoBajaInvalidoException(int idMotivo)
    : ReglaDeNegocioException($"El motivo de baja {idMotivo} no existe o está inactivo");

public class SolicitudActivoBajaInvalidaException(Guid idSolicitud)
    : ReglaDeNegocioException($"La solicitud de baja {idSolicitud} no existe o no está en el estado esperado para esta operación");

public class CodigoActivoDuplicadoException(string codigo)
    : ReglaDeNegocioException($"Ya existe un activo registrado con el código '{codigo}'");

public class ValorActivoInvalidoException(decimal valor)
    : SolicitudInvalidaException($"El valor del activo ({valor:0.00}) debe ser mayor a cero");

public class FechaCompraFuturaException(DateOnly fechaCompra)
    : SolicitudInvalidaException($"La fecha de compra ({fechaCompra:yyyy-MM-dd}) no puede ser futura");

public class AgenciaInvalidaException(int idAgencia)
    : ReglaDeNegocioException($"La agencia {idAgencia} no existe o no está activa");
