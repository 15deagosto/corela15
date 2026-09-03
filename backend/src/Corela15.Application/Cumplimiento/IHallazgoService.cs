using Corela15.Application.Common;

namespace Corela15.Application.Cumplimiento;

public interface IHallazgoService
{
    /// <summary>
    /// Registra un hallazgo de auditoría/cumplimiento, asignándolo a uno o
    /// más usuarios auditados — nace en estado "IN" (Ingresada, verificado
    /// como el primer estado real observado en Softbank).
    /// </summary>
    Task<HallazgoCreadoResult> CrearAsync(CrearHallazgoRequest request, CancellationToken cancellationToken = default);

    /// <summary>El auditado responde al hallazgo asignado — marca HallazgoUsuario.EstadoRespondido.</summary>
    Task ResponderAsync(ResponderHallazgoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Transiciona el estado del hallazgo, dejando bitácora.</summary>
    Task CambiarEstadoAsync(CambiarEstadoHallazgoRequest request, CancellationToken cancellationToken = default);
}

public class UsuarioReportaInvalidoException(Guid idUsuario)
    : ReglaDeNegocioException($"El usuario {idUsuario} no existe o no está activo");

public class UsuarioAsignadoInvalidoException(Guid idUsuario)
    : ReglaDeNegocioException($"El usuario asignado {idUsuario} no existe o no está activo");

public class HallazgoInvalidoException(Guid idHallazgo)
    : ReglaDeNegocioException($"El hallazgo {idHallazgo} no existe");

public class HallazgoUsuarioInvalidoException(Guid idHallazgoUsuario)
    : ReglaDeNegocioException($"La asignación {idHallazgoUsuario} no existe o no está activa");

public class EstadoHallazgoInvalidoException(string codigoEstado)
    : ReglaDeNegocioException($"El estado '{codigoEstado}' no existe o no está activo");
