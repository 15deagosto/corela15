namespace Corela15.Application.Riesgo;

public record CrearAvanceRiesgoRequest(
    Guid IdEventoRiesgo, Guid IdUsuarioResponsable, DateTimeOffset? HoraInicio, DateTimeOffset? HoraFin,
    string EventoDetectado, string PosibleCausa, string Tratamiento, string Inconvenientes, string AccionesSugeridas,
    string RegistradoPor);

public record AvanceRiesgoCreadoResult(Guid IdAvanceRiesgo, Guid IdAvanceRiesgoDetalle);

public record CambiarEstadoAvanceRiesgoRequest(Guid IdAvanceRiesgoDetalle, string CodigoEstadoNuevo, string Comentario, string RegistradoPor);
