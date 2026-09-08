using Corela15.Application.Common;

namespace Corela15.Application.Planificacion;

/// <summary>
/// Planificación semanal real de cada área -- reemplaza el PDF manual que
/// cada responsable armaba por separado. Solo quien tiene el permiso real
/// de planificación (Menu:planificacion, otorgado por rol -- no universal)
/// puede crear/editar, y ve/edita únicamente el área a la que está
/// vinculado (Usuario.CodigoAreaPlanificacion -- jefe y asistente de la
/// misma área comparten el mismo plan, sin importar quién lo cargó),
/// salvo que además tenga el permiso de gerencia (Menu:planificacion-
/// gerencia), que da visibilidad de solo lectura + notas sobre todas las
/// áreas.
///
/// Envío real con corte los viernes 17:00 (hora Ecuador, UTC-5):
/// - Antes del corte: editable siempre, esté enviada o no.
/// - Después del corte, si ya estaba enviada: bloqueada por completo.
/// - Después del corte, si nunca se envió: se permite un único envío
///   tardío, marcado explícitamente como fuera de tiempo.
/// </summary>
public interface IPlanificacionService
{
    /// <summary>
    /// Crea o actualiza (upsert real, por Área+Semana) el plan completo de
    /// una semana -- reemplaza todos los bloques existentes por los que
    /// vengan en el request. Rechazado si el plan ya está bloqueado (ver
    /// resumen de la interfaz). Si algún bloque existente tiene una nota de
    /// gerencia, solo se rechaza el reguardado (para no perderla en
    /// silencio) una vez pasado el corte real -- antes del corte, un
    /// comentario de gerencia suele ser un pedido de cambio, así que el
    /// área puede reguardar libremente para aplicarlo.
    /// </summary>
    Task<PlanSemanalDto> GuardarAsync(GuardarPlanSemanalRequest request, CancellationToken cancellationToken = default);

    /// <summary>Marca el plan como enviado -- ver la lógica real de corte/tardanza en el resumen de la interfaz.</summary>
    Task<PlanSemanalDto> EnviarAsync(Guid idPlan, string usuarioActual, bool esGerencia, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlanSemanalListItemDto>> ListarAsync(ListarPlanesFiltro filtro, CancellationToken cancellationToken = default);

    Task<PlanSemanalDto> ObtenerAsync(Guid idPlan, string usuarioActual, bool esGerencia, CancellationToken cancellationToken = default);

    /// <summary>Solo gerencia -- nota general sobre toda la semana.</summary>
    Task<PlanSemanalDto> GuardarNotaGerenciaAsync(Guid idPlan, string? nota, CancellationToken cancellationToken = default);

    /// <summary>Solo gerencia -- nota puntual sobre un bloque (una hora específica).</summary>
    Task<PlanSemanalDto> GuardarNotaBloqueAsync(Guid idBloque, string? nota, CancellationToken cancellationToken = default);

    /// <summary>
    /// Combo editable real: si ya existe una etiqueta con ese nombre en
    /// esa área, la devuelve tal cual (mismo color de siempre). Si no
    /// existe, la crea en el momento -- código autogenerado real, color
    /// asignado de una paleta rotativa según cuántas etiquetas ya tiene
    /// el área, sin que el usuario tenga que ir a Configuración a mano.
    /// </summary>
    Task<EtiquetaDto> ObtenerOCrearEtiquetaAsync(string codigoArea, string nombre, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edición real de una etiqueta ya existente (nombre + color) desde el
    /// propio combo del formulario, sin tener que ir a Configuración --
    /// mismo criterio de autonomía por área ya establecido con la
    /// creación sobre la marcha. Rechaza dejarla con el mismo nombre que
    /// otra etiqueta ya real de la misma área.
    /// </summary>
    Task<EtiquetaDto> ActualizarEtiquetaAsync(string codigo, string nombre, string colorHex, CancellationToken cancellationToken = default);
}

public record EtiquetaDto(string Codigo, string Nombre, string ColorHex);

public record BloqueRequest(
    int DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string CodigoEtiqueta,
    string Descripcion);

public record GuardarPlanSemanalRequest(
    string CodigoArea,
    DateOnly FechaInicioSemana,
    string NombreResponsable,
    string CargoResponsable,
    IReadOnlyList<BloqueRequest> Bloques,
    string RegistradoPor);

public record ListarPlanesFiltro(string? CodigoArea, DateOnly? Desde, DateOnly? Hasta, string UsuarioActual, bool EsGerencia);

public record BloqueDto(Guid Id, int DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, string CodigoEtiqueta, string Etiqueta, string ColorHex, string Descripcion, string? NotaGerencia);

public record PlanSemanalDto(
    Guid Id, string CodigoArea, string Area, DateOnly FechaInicioSemana,
    string NombreResponsable, string CargoResponsable,
    IReadOnlyList<BloqueDto> Bloques,
    bool Enviada, DateTimeOffset? FechaEnvio, bool EnviadaFueraDeTiempo, string? EnviadaPor,
    string? NotaGerencia, bool Bloqueada, DateTimeOffset FechaLimiteEnvio,
    DateTimeOffset CreadoEn, string CreadoPor, DateTimeOffset? ModificadoEn);

public record PlanSemanalListItemDto(
    Guid Id, string CodigoArea, string Area, DateOnly FechaInicioSemana,
    string NombreResponsable, int CantidadBloques,
    bool Enviada, bool EnviadaFueraDeTiempo, bool Bloqueada,
    DateTimeOffset CreadoEn, string CreadoPor);

public class AreaPlanificacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"El área '{codigo}' no existe o no está activa");

public class EtiquetaPlanificacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La etiqueta '{codigo}' no existe o no está activa");

public class NombreEtiquetaDuplicadoException(string nombre)
    : ReglaDeNegocioException($"Ya existe una etiqueta llamada '{nombre}' en esta área");

public class BloqueHorarioInvalidoException(string detalle)
    : SolicitudInvalidaException(detalle);

public class PlanSemanalNoExisteException(Guid id)
    : ReglaDeNegocioException($"El plan semanal {id} no existe");

public class PlanSemanalAjenoException()
    : ReglaDeNegocioException("Solo podés ver/editar la planificación de tu propia área");

public class PlanSemanalBloqueadoException()
    : ReglaDeNegocioException("Ya pasó el horario límite de envío (viernes 5:00 pm) y esta semana ya fue enviada -- no se puede modificar");

public class PlanSemanalConNotaGerenciaException()
    : ReglaDeNegocioException("Esta semana ya tiene comentarios de gerencia sobre bloques puntuales -- coordiná con gerencia antes de reconstruir la planificación completa");

public class UsuarioSinAreaPlanificacionException()
    : ReglaDeNegocioException("Tu usuario no tiene un área de planificación asignada -- pedile a un administrador que te la configure en Usuarios y roles");
