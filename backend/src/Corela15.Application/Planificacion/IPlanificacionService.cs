using Corela15.Application.Common;

namespace Corela15.Application.Planificacion;

/// <summary>
/// Planificación semanal real de cada área -- reemplaza el PDF manual que
/// cada responsable armaba por separado. Solo quien tiene el permiso real
/// de planificación (Menu:planificacion, otorgado por rol -- no universal)
/// puede crear/editar, y únicamente ve/edita lo que él mismo reportó,
/// salvo que además tenga el permiso de gerencia (Menu:planificacion-
/// gerencia), que da visibilidad de solo lectura sobre todas las áreas.
/// </summary>
public interface IPlanificacionService
{
    /// <summary>
    /// Crea o actualiza (upsert real, por Área+Semana) el plan completo de
    /// una semana -- reemplaza todos los bloques existentes por los que
    /// vengan en el request, en una sola operación, para que editar una
    /// semana completa sea una sola llamada, no N llamadas por bloque.
    /// </summary>
    Task<PlanSemanalDto> GuardarAsync(GuardarPlanSemanalRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlanSemanalListItemDto>> ListarAsync(ListarPlanesFiltro filtro, CancellationToken cancellationToken = default);

    Task<PlanSemanalDto> ObtenerAsync(Guid idPlan, string usuarioActual, bool esGerencia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Combo editable real: si ya existe una etiqueta con ese nombre en
    /// esa área, la devuelve tal cual (mismo color de siempre). Si no
    /// existe, la crea en el momento -- código autogenerado real, color
    /// asignado de una paleta rotativa según cuántas etiquetas ya tiene
    /// el área, sin que el usuario tenga que ir a Configuración a mano.
    /// </summary>
    Task<EtiquetaDto> ObtenerOCrearEtiquetaAsync(string codigoArea, string nombre, CancellationToken cancellationToken = default);
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

public record BloqueDto(Guid Id, int DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, string CodigoEtiqueta, string Etiqueta, string ColorHex, string Descripcion);

public record PlanSemanalDto(
    Guid Id, string CodigoArea, string Area, DateOnly FechaInicioSemana,
    string NombreResponsable, string CargoResponsable,
    IReadOnlyList<BloqueDto> Bloques,
    DateTimeOffset CreadoEn, string CreadoPor, DateTimeOffset? ModificadoEn);

public record PlanSemanalListItemDto(
    Guid Id, string CodigoArea, string Area, DateOnly FechaInicioSemana,
    string NombreResponsable, int CantidadBloques, DateTimeOffset CreadoEn, string CreadoPor);

public class AreaPlanificacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"El área '{codigo}' no existe o no está activa");

public class EtiquetaPlanificacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La etiqueta '{codigo}' no existe o no está activa");

public class BloqueHorarioInvalidoException(string detalle)
    : SolicitudInvalidaException(detalle);

public class PlanSemanalNoExisteException(Guid id)
    : ReglaDeNegocioException($"El plan semanal {id} no existe");

public class PlanSemanalAjenoException()
    : ReglaDeNegocioException("Solo podés ver/editar la planificación que vos mismo reportaste");
