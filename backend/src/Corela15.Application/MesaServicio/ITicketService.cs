using Corela15.Application.Common;

namespace Corela15.Application.MesaServicio;

/// <summary>
/// Mesa de servicio — control de incidencias real exigido por la SEPS.
/// Disponible para todo usuario autenticado (ver AuthService.LoginAsync,
/// el menú `mesa-servicio` se agrega siempre, sin pasar por `rol_menu`).
/// Un segundo menú, `mesa-servicio-agente` (ese sí vía `rol_menu` normal,
/// otorgado explícitamente a quien resuelve tickets), gatea quién puede
/// tomar/reasignar/cambiar el estado de un ticket — cualquier usuario
/// puede reportar, solo un agente puede trabajarlo.
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Registra el ticket, calcula la fecha límite de SLA a partir de la
    /// prioridad elegida, y deja la primera entrada de bitácora ("(vacío)"
    /// → estado inicial) — mismo patrón ya usado en AvanceRiesgo/Hallazgo.
    /// </summary>
    Task<TicketDto> CrearAsync(CrearTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>Agrega un comentario de seguimiento — no cambia el estado.</summary>
    Task<TicketDto> ComentarAsync(Guid idTicket, ComentarTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transiciona el estado (código destino debe existir y estar activo),
    /// deja bitácora, y reasigna si se pasa un nuevo usuario asignado. Si
    /// el destino es un estado final (Resuelto/Cerrado/Cancelado), fija
    /// FechaCierre. Solo la ejecuta un agente (Menu:mesa-servicio-agente,
    /// validado en el controller) — la primera vez que esto corre sobre un
    /// ticket, fija FechaPrimeraRespuesta.
    /// </summary>
    Task<TicketDto> CambiarEstadoAsync(Guid idTicket, CambiarEstadoTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>Reasignación explícita a cualquier agente — solo un agente puede ejecutarla.</summary>
    Task<TicketDto> AsignarAsync(Guid idTicket, AsignarTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Un agente se autoasigna un ticket sin dueño — no requiere elegir de
    /// una lista. Rechaza tomar un ticket que ya tiene otro agente asignado
    /// (para eso existe Reasignar, un cambio explícito, no un "robo" silencioso).
    /// </summary>
    Task<TicketDto> TomarAsync(Guid idTicket, TomarTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// El reportante original califica la atención (1-5 + comentario
    /// opcional) una sola vez, solo cuando el ticket ya está
    /// Resuelto/Cerrado — nunca antes, nunca dos veces.
    /// </summary>
    Task<TicketDto> CalificarAsync(Guid idTicket, CalificarTicketRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketListItemDto>> ListarAsync(ListarTicketsFiltro filtro, CancellationToken cancellationToken = default);

    Task<TicketDetalleDto> ObtenerAsync(Guid idTicket, CancellationToken cancellationToken = default);

    /// <summary>Usuarios reales con permiso de agente (menú `mesa-servicio-agente` vía rol, incluye roles temporales vigentes) — la única lista que debe verse al asignar/reasignar.</summary>
    Task<IReadOnlyList<AgenteDto>> ListarAgentesAsync(CancellationToken cancellationToken = default);
}

public record CrearTicketRequest(
    string Titulo,
    string Descripcion,
    string CodigoCategoria,
    string CodigoPrioridad,
    int IdAgencia,
    Guid? IdUsuarioAsignado,
    string RegistradoPor);

public record ComentarTicketRequest(string Comentario, string RegistradoPor);

public record CambiarEstadoTicketRequest(string CodigoEstado, string? Comentario, string RegistradoPor);

public record AsignarTicketRequest(Guid? IdUsuarioAsignado, string RegistradoPor);

public record TomarTicketRequest(Guid IdUsuarioAgente, string RegistradoPor);

public record CalificarTicketRequest(int Calificacion, string? Comentario, string SolicitadoPor);

public record ListarTicketsFiltro(string? CodigoEstado, Guid? IdUsuarioAsignado, string? CreadoPor);

public record AgenteDto(Guid Id, string NombreUsuario);

public record TicketDto(
    Guid Id, string Numero, string Titulo, string Descripcion,
    string CodigoCategoria, string Categoria,
    string CodigoPrioridad, string Prioridad,
    string CodigoEstado, string Estado,
    string Agencia, string? UsuarioAsignado,
    DateTimeOffset FechaLimiteSla, bool VencidoSla, DateTimeOffset? FechaCierre,
    DateTimeOffset? FechaPrimeraRespuesta,
    double? TiempoPrimeraRespuestaHoras, double? TiempoResolucionHoras,
    int? Calificacion, string? ComentarioCalificacion,
    DateTimeOffset CreadoEn, string CreadoPor);

public record TicketListItemDto(
    Guid Id, string Numero, string Titulo,
    string Categoria, string Prioridad, string CodigoEstado, string Estado,
    string Agencia, string? UsuarioAsignado,
    DateTimeOffset FechaLimiteSla, bool VencidoSla,
    int? Calificacion,
    DateTimeOffset CreadoEn, string CreadoPor);

public record TicketComentarioDto(string Comentario, string RegistradoPor, DateTimeOffset Fecha);

public record TicketEtapaHistDto(string EstadoAnterior, string EstadoNuevo, string? Comentario, string RegistradoPor, DateTimeOffset Fecha, double? HorasEnEstadoAnterior);

public record TicketDetalleDto(
    TicketDto Ticket,
    IReadOnlyList<TicketComentarioDto> Comentarios,
    IReadOnlyList<TicketEtapaHistDto> Bitacora);

public class CategoriaIncidenciaInvalidaException(string codigo)
    : ReglaDeNegocioException($"La categoría de incidencia '{codigo}' no existe o no está activa");

public class PrioridadTicketInvalidaException(string codigo)
    : ReglaDeNegocioException($"La prioridad '{codigo}' no existe o no está activa");

public class EstadoTicketInvalidoException(string codigo)
    : ReglaDeNegocioException($"El estado '{codigo}' no existe o no está activo");

public class AgenciaInvalidaParaTicketException(int idAgencia)
    : ReglaDeNegocioException($"La agencia {idAgencia} no existe o no está activa");

public class UsuarioAsignadoInvalidoException(Guid idUsuario)
    : ReglaDeNegocioException($"El usuario {idUsuario} no existe, no está activo, o no tiene permiso de agente de mesa de servicio");

public class TicketNoExisteException(Guid idTicket)
    : ReglaDeNegocioException($"El ticket {idTicket} no existe");

public class TicketYaCerradoException(string numero)
    : ReglaDeNegocioException($"El ticket {numero} ya está cerrado — no admite más cambios");

public class TicketYaAsignadoException(string numero, string usuarioActual)
    : ReglaDeNegocioException($"El ticket {numero} ya está asignado a {usuarioActual} — usá \"Reasignar\" si querés cambiarlo, \"Tomar\" es solo para tickets sin dueño");

public class TicketNoCalificableException(string numero)
    : ReglaDeNegocioException($"El ticket {numero} todavía no está resuelto — no se puede calificar hasta que la mesa de servicio lo marque Resuelto o Cerrado");

public class TicketYaCalificadoException(string numero)
    : ReglaDeNegocioException($"El ticket {numero} ya fue calificado — la calificación es una sola vez");

public class SoloCreadorPuedeCalificarException()
    : ReglaDeNegocioException("Solo quien reportó el ticket puede calificar la atención");

public class CalificacionInvalidaException(int valor)
    : SolicitudInvalidaException($"La calificación debe ser un número entre 1 y 5 (recibido: {valor})");
