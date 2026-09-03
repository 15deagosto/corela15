using Corela15.Application.Common;

namespace Corela15.Application.Riesgo;

public interface IAvanceRiesgoService
{
    /// <summary>
    /// Crea el plan de acción sobre un evento de riesgo ya registrado —
    /// nace en estado "PRE" (Preingresada, verificado como el primer
    /// estado real de la bitácora de Softbank) y registra la primera
    /// entrada de la bitácora automáticamente.
    /// </summary>
    Task<AvanceRiesgoCreadoResult> CrearAsync(
        CrearAvanceRiesgoRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transiciona el estado del plan, dejando bitácora — el código
    /// destino debe existir y estar activo en riesgo.estado_avance_riesgo.
    /// Sin máquina de estados estricta (no verificable contra Softbank más
    /// allá del catálogo), cualquier código activo es un destino válido.
    /// </summary>
    Task CambiarEstadoAsync(CambiarEstadoAvanceRiesgoRequest request, CancellationToken cancellationToken = default);
}

public class EventoRiesgoInvalidoParaAvanceException(Guid idEventoRiesgo)
    : ReglaDeNegocioException($"El evento de riesgo {idEventoRiesgo} no existe o no está activo");

public class UsuarioResponsableInvalidoException(Guid idUsuarioResponsable)
    : ReglaDeNegocioException($"El usuario responsable {idUsuarioResponsable} no existe o no está activo");

public class EstadoAvanceRiesgoInvalidoException(string codigoEstado)
    : ReglaDeNegocioException($"El estado '{codigoEstado}' no existe o no está activo");

public class AvanceRiesgoDetalleInvalidoException(Guid idAvanceRiesgoDetalle)
    : ReglaDeNegocioException($"El plan de acción {idAvanceRiesgoDetalle} no existe");
