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
///
/// Cada plan lleva, además de los bloques de horario, KPIs reales por
/// área (meta vs. real de la semana, ver Indicador/PlanSemanalIndicador)
/// y un estado de revisión de gerencia (EstadoAprobacionPlan) separado
/// del envío -- gerencia aprueba o devuelve con observaciones un plan
/// ya enviado; si el área lo reguarda después, vuelve a Pendiente solo.
/// </summary>
public interface IPlanificacionService
{
    /// <summary>
    /// Espacio de planificación personal del usuario actual -- se crea solo
    /// la primera vez (nunca hace falta que un administrador lo asigne). La
    /// planificación es por persona, no por departamento (decisión explícita
    /// del usuario, oct-2026): el "área" de las demás operaciones es este
    /// espacio personal.
    /// </summary>
    Task<EspacioPlanificacionDto> ObtenerMiEspacioAsync(string usuarioActual, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sin gerencia: solo el espacio propio. Con gerencia: el de cada
    /// persona que tiene acceso real al módulo (crea los que falten), para
    /// filtrar el historial y asignar disposiciones por persona.
    /// </summary>
    Task<IReadOnlyList<EspacioPlanificacionDto>> ListarEspaciosAsync(bool esGerencia, string usuarioActual, CancellationToken cancellationToken = default);

    /// <summary>Rechaza (422) si el espacio no es el del usuario actual -- etiquetas y KPIs solo los crea/edita el dueño.</summary>
    Task ValidarDuenoEspacioAsync(string codigoArea, string usuarioActual, CancellationToken cancellationToken = default);

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
    /// Solo gerencia -- aprueba el plan tal como está hoy. Exige que el
    /// área ya lo haya enviado (aprobar un borrador no tiene sentido real).
    /// </summary>
    Task<PlanSemanalDto> AprobarAsync(Guid idPlan, string usuarioActual, CancellationToken cancellationToken = default);

    /// <summary>
    /// Solo gerencia -- devuelve el plan con observaciones (comentario
    /// obligatorio, se guarda también como NotaGerencia general). Mismo
    /// requisito de envío previo que AprobarAsync.
    /// </summary>
    Task<PlanSemanalDto> SolicitarCambiosAsync(Guid idPlan, string usuarioActual, string comentario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Combo editable real para KPIs, mismo patrón que
    /// ObtenerOCrearEtiquetaAsync: si ya existe un indicador con ese
    /// nombre en el área, lo devuelve; si no, lo crea en el momento.
    /// </summary>
    Task<IndicadorDto> ObtenerOCrearIndicadorAsync(string codigoArea, string nombre, string? unidad, CancellationToken cancellationToken = default);

    /// <summary>Solo gerencia -- crea una disposición real con plazo, asignada a una o más áreas.</summary>
    Task<DisposicionGerenciaDto> CrearDisposicionAsync(CrearDisposicionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Solo gerencia -- cierra una disposición (ya no se puede seguir etiquetando bloques nuevos con ella).</summary>
    Task CerrarDisposicionAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gerencia sin filtro ve todas; gerencia con `codigoArea` ve todas las
    /// de esa área (cualquier estado, para poder calificar cumplimiento);
    /// un área normal ve solo las que tiene asignadas, abiertas, para
    /// poder etiquetar bloques -- ver DisposicionGerenciaArea.
    /// </summary>
    Task<IReadOnlyList<DisposicionGerenciaDto>> ListarDisposicionesAsync(bool esGerencia, string? codigoArea, CancellationToken cancellationToken = default);

    /// <summary>Solo gerencia -- califica si esta área concreta cumplió o no la disposición (independiente de las demás áreas asignadas).</summary>
    Task<DisposicionGerenciaDto> CalificarCumplimientoAsync(Guid idDisposicion, string codigoArea, string cumplimiento, string? comentario, string calificadoPor, CancellationToken cancellationToken = default);

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
    Task<EtiquetaDto> ActualizarEtiquetaAsync(string codigo, string nombre, string colorHex, string usuarioActual, CancellationToken cancellationToken = default);
}

public record EspacioPlanificacionDto(string Codigo, string Nombre, string NombreUsuario);

public record EtiquetaDto(string Codigo, string Nombre, string ColorHex);

public record BloqueRequest(
    int DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string CodigoEtiqueta,
    string Descripcion,
    Guid? IdDisposicion);

public record IndicadorRequest(int IdIndicador, decimal? Meta, decimal? Real, string? Comentario);

public record GuardarPlanSemanalRequest(
    string CodigoArea,
    DateOnly FechaInicioSemana,
    string NombreResponsable,
    string CargoResponsable,
    IReadOnlyList<BloqueRequest> Bloques,
    IReadOnlyList<IndicadorRequest> Indicadores,
    string RegistradoPor);

public record ListarPlanesFiltro(string? CodigoArea, DateOnly? Desde, DateOnly? Hasta, string UsuarioActual, bool EsGerencia);

public record BloqueDto(Guid Id, int DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, string CodigoEtiqueta, string Etiqueta, string ColorHex, string Descripcion, string? NotaGerencia, Guid? IdDisposicion, string? TituloDisposicion);

public record CrearDisposicionRequest(string Titulo, string? Descripcion, DateOnly FechaLimite, IReadOnlyList<string> CodigosArea, string CreadoPor);

public record DisposicionAreaDto(string CodigoArea, string NombreArea, string Cumplimiento, string? ComentarioCumplimiento, string? CalificadoPor, DateTimeOffset? FechaCalificacion, decimal HorasDedicadas);

public record DisposicionGerenciaDto(Guid Id, string Titulo, string? Descripcion, DateOnly FechaLimite, string Estado, IReadOnlyList<DisposicionAreaDto> Areas, decimal HorasDedicadas, DateTimeOffset CreadoEn, string CreadoPor);

public record IndicadorDto(int Id, string CodigoArea, string Nombre, string? Unidad);

public record IndicadorSemanalDto(Guid Id, int IdIndicador, string Nombre, string? Unidad, decimal? Meta, decimal? Real, string? Comentario);

public record PlanSemanalDto(
    Guid Id, string CodigoArea, string Area, DateOnly FechaInicioSemana,
    string NombreResponsable, string CargoResponsable,
    IReadOnlyList<BloqueDto> Bloques,
    IReadOnlyList<IndicadorSemanalDto> Indicadores,
    bool Enviada, DateTimeOffset? FechaEnvio, bool EnviadaFueraDeTiempo, string? EnviadaPor,
    string? NotaGerencia, string EstadoAprobacion, string? AprobadoPor, DateTimeOffset? FechaAprobacion,
    bool Bloqueada, DateTimeOffset FechaLimiteEnvio,
    DateTimeOffset CreadoEn, string CreadoPor, DateTimeOffset? ModificadoEn);

public record PlanSemanalListItemDto(
    Guid Id, string CodigoArea, string Area, DateOnly FechaInicioSemana,
    string NombreResponsable, int CantidadBloques,
    bool Enviada, bool EnviadaFueraDeTiempo, bool Bloqueada,
    string EstadoAprobacion,
    DateTimeOffset CreadoEn, string CreadoPor);

public class AreaPlanificacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La planificación '{codigo}' no existe o no está activa");

public class EtiquetaPlanificacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La etiqueta '{codigo}' no existe o no está activa");

public class NombreEtiquetaDuplicadoException(string nombre)
    : ReglaDeNegocioException($"Ya tenés una etiqueta llamada '{nombre}'");

public class BloqueHorarioInvalidoException(string detalle)
    : SolicitudInvalidaException(detalle);

public class PlanSemanalNoExisteException(Guid id)
    : ReglaDeNegocioException($"El plan semanal {id} no existe");

public class PlanSemanalAjenoException()
    : ReglaDeNegocioException("Solo podés ver/editar tu propia planificación");

public class PlanSemanalBloqueadoException()
    : ReglaDeNegocioException("Ya pasó el horario límite de envío (viernes 5:00 pm) y esta semana ya fue enviada -- no se puede modificar");

public class PlanSemanalConNotaGerenciaException()
    : ReglaDeNegocioException("Esta semana ya tiene comentarios de gerencia sobre bloques puntuales -- coordiná con gerencia antes de reconstruir la planificación completa");

public class UsuarioSinAreaPlanificacionException()
    : ReglaDeNegocioException("Tu usuario no tiene una planificación personal todavía -- volvé a abrir el módulo de Planificación");

public class IndicadorPlanificacionInvalidoException(int idIndicador)
    : ReglaDeNegocioException($"El indicador {idIndicador} no existe, no está activo, o no es tuyo");

public class PlanSemanalNoEnviadoException()
    : ReglaDeNegocioException("Gerencia solo puede aprobar u observar un plan que ya fue enviado");

public class ComentarioObligatorioException()
    : SolicitudInvalidaException("El comentario es obligatorio para solicitar cambios");

public class DisposicionGerenciaInvalidaException(Guid id)
    : ReglaDeNegocioException($"La disposición {id} no existe, está cerrada, o no está asignada a esta persona");

public class DisposicionSinAreasException()
    : SolicitudInvalidaException("Una disposición de gerencia necesita al menos una persona destinataria");

public class DisposicionAreaInvalidaException()
    : ReglaDeNegocioException("Esta disposición no está asignada a esa persona");

public class CumplimientoInvalidoException(string valor)
    : SolicitudInvalidaException($"'{valor}' no es un valor válido de cumplimiento (Cumplio/NoCumplio/SinEvaluar)");
