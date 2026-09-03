using Corela15.Application.Common;

namespace Corela15.Application.Sujeto;

public record TelefonoDetalle(Guid Id, string Telefono, bool EsTelefonoMovil, bool EsPrincipal, bool NotificacionSms);

public record AgregarTelefonoRequest(
    Guid IdPersona, string Telefono, bool EsTelefonoMovil, bool EsPrincipal, bool NotificacionSms, string RegistradoPor);

public record ConyugeDetalle(Guid Id, Guid IdPersonaConyuge, string NombreConyuge, string IdentificacionConyuge);

public record AgregarConyugeRequest(Guid IdPersonaNatural, Guid IdPersonaConyuge, string RegistradoPor);

public record RepresentanteDetalle(
    Guid Id, Guid IdPersonaRepresentante, string NombreRepresentante, string IdentificacionRepresentante,
    bool Principal, bool EjerceControl);

public record AgregarRepresentanteRequest(
    Guid IdPersona, Guid IdPersonaRepresentante, bool Principal, bool EjerceControl, string RegistradoPor);

public record RegistroAgregadoResult(Guid Id);

public interface IDatosPersonaService
{
    Task<IReadOnlyList<TelefonoDetalle>> ListarTelefonosAsync(Guid idPersona, CancellationToken cancellationToken = default);
    Task<RegistroAgregadoResult> AgregarTelefonoAsync(AgregarTelefonoRequest request, CancellationToken cancellationToken = default);
    Task QuitarTelefonoAsync(Guid idTelefono, string registradoPor, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConyugeDetalle>> ListarConyugesAsync(Guid idPersonaNatural, CancellationToken cancellationToken = default);
    Task<RegistroAgregadoResult> AgregarConyugeAsync(AgregarConyugeRequest request, CancellationToken cancellationToken = default);
    Task QuitarConyugeAsync(Guid idConyuge, string registradoPor, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RepresentanteDetalle>> ListarRepresentantesAsync(Guid idPersona, CancellationToken cancellationToken = default);
    Task<RegistroAgregadoResult> AgregarRepresentanteAsync(AgregarRepresentanteRequest request, CancellationToken cancellationToken = default);
    Task QuitarRepresentanteAsync(Guid idRepresentante, string registradoPor, CancellationToken cancellationToken = default);
}

public class PersonaNoExisteException(Guid idPersona)
    : ReglaDeNegocioException($"La persona {idPersona} no existe");

public class PersonaNaturalNoExisteException(Guid idPersona)
    : ReglaDeNegocioException($"La persona {idPersona} no existe o no es una persona natural");

public class ConyugeEsLaMismaPersonaException(Guid idPersona)
    : ReglaDeNegocioException($"El cónyuge no puede ser la misma persona ({idPersona})");

public class YaTieneConyugeActivoException(Guid idPersonaNatural)
    : ReglaDeNegocioException($"La persona {idPersonaNatural} ya tiene un cónyuge activo registrado — quitarlo antes de registrar uno nuevo");

public class RepresentanteEsLaMismaPersonaException(Guid idPersona)
    : ReglaDeNegocioException($"El representante no puede ser la misma persona ({idPersona})");

public class RegistroNoExisteException(Guid id)
    : ReglaDeNegocioException($"El registro {id} no existe o ya fue quitado");
