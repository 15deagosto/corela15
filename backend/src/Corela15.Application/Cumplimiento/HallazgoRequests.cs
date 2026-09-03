namespace Corela15.Application.Cumplimiento;

public record CrearHallazgoRequest(
    string Nombre, string Detalle, Guid IdUsuarioReporta, IReadOnlyList<Guid> IdsUsuarioAsignado, string RegistradoPor);

public record HallazgoCreadoResult(Guid IdHallazgo);

public record ResponderHallazgoRequest(Guid IdHallazgoUsuario, string Respuesta);

public record CambiarEstadoHallazgoRequest(Guid IdHallazgo, string CodigoEstadoNuevo, string Comentario, string RegistradoPor);
