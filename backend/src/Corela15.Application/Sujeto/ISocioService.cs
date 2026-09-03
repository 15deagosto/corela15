using Corela15.Application.Common;

namespace Corela15.Application.Sujeto;

public record CrearSocioRequest(
    bool EsPersonaNatural,
    string Identificacion,
    int IdTipoIdentificacion,
    string? Email,
    int IdAgencia,
    // Persona natural
    string? PrimerNombre,
    string? SegundoNombre,
    string? ApellidoPaterno,
    string? ApellidoMaterno,
    DateOnly? FechaNacimiento,
    bool? EsMasculino,
    // Persona jurídica
    string? RazonSocial,
    DateOnly? FechaCreacion,
    // Datos socioeconómicos reales (persona natural) — todos opcionales,
    // catálogos configurables desde Configuración (ver CLAUDE.md).
    string? CodigoEstadoCivil,
    string? CodigoEducacion,
    string? CodigoVivienda,
    string? CodigoSectorVivienda,
    string? CodigoNacionalidad,
    string? CodigoProfesion,
    int? IdActividadEconomica,
    // Datos de vinculación real (cliente) — también opcionales/configurables.
    string? CodigoCausaVinculacion,
    string? CodigoCalificacionInterna,
    string? CodigoSectorEconomico,
    string RegistradoPor);

public record ActualizarSocioRequest(
    string? Email,
    decimal? Activos,
    decimal? Pasivos,
    decimal? Ingresos,
    decimal? Egresos,
    // Persona natural
    string? PrimerNombre,
    string? SegundoNombre,
    string? ApellidoPaterno,
    string? ApellidoMaterno,
    bool? EsPep,
    // Persona jurídica
    string? RazonSocial,
    // Datos socioeconómicos reales (persona natural)
    string? CodigoEstadoCivil,
    string? CodigoEducacion,
    string? CodigoVivienda,
    string? CodigoSectorVivienda,
    string? CodigoNacionalidad,
    string? CodigoProfesion,
    int? IdActividadEconomica,
    // Datos socioeconómicos adicionales reales (persona natural) —
    // verificados contra SUJETO.PERSONA_NATURAL, sin caso de uso hasta ahora.
    bool? CobraBonoDesarrolloHumano,
    bool? EsSeparacionDeBienes,
    bool? TieneDiscapacidad,
    bool? TieneCargasFamiliares,
    int? NumeroCargasFamiliares,
    // Domicilio/contacto real (Persona) — verificados contra SUJETO.PERSONA.
    string? CallePrincipal,
    string? NumeroCasa,
    string? Barrio,
    string? CodigoProvinciaDomicilio,
    string? CalleSecundaria,
    string? CodigoPostal,
    string? Referencia,
    string? ParentescoServicioBasico,
    // Datos de vinculación real (cliente)
    string? CodigoCausaVinculacion,
    string? CodigoCalificacionInterna,
    string? CodigoSectorEconomico,
    Guid? IdUsuarioOficial,
    bool? EsExento,
    string RegistradoPor);

public record SocioCreadoResult(Guid IdCliente, Guid IdPersona, string Numero, string Nombre);

public interface ISocioService
{
    /// <summary>
    /// Crea un socio nuevo: Persona (+ PersonaNatural o PersonaJuridica,
    /// mutuamente excluyentes) y su Cliente activo — cierra el gap real de
    /// que este core no tenía ninguna forma de registrar un socio nuevo,
    /// solo los 5 sembrados por Nivel0_SeedDatosPrueba. Numeración de
    /// Cliente.Numero por conteo+1 (mismo patrón que Cuenta/Prestamo).
    /// </summary>
    Task<SocioCreadoResult> CrearAsync(CrearSocioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edita los datos básicos ya capturados de un socio existente —
    /// contacto, datos financieros (Activos/Pasivos/Ingresos/Egresos, los
    /// mismos que usan ScoreCrediticioService/PerfilLavadoActivosService),
    /// y nombre/EsPep para persona natural o razón social para jurídica.
    /// Antes solo se podían tocar por SQL directo — ver notas de prueba de
    /// ScoreCrediticio/PerfilLavado en CLAUDE.md.
    /// </summary>
    Task ActualizarAsync(Guid idPersona, ActualizarSocioRequest request, CancellationToken cancellationToken = default);

    /// <summary>Activa/inactiva/suspende el Cliente de un socio (Cliente.Estado).</summary>
    Task CambiarEstadoClienteAsync(Guid idCliente, string nuevoEstado, string registradoPor, CancellationToken cancellationToken = default);
}

public class TipoIdentificacionInvalidoException(int id)
    : ReglaDeNegocioException($"El tipo de identificación {id} no existe");

public class AgenciaInvalidaException(int id)
    : ReglaDeNegocioException($"La agencia {id} no existe o no está activa");

public class DatosPersonaNaturalIncompletosException()
    : SolicitudInvalidaException("Para una persona natural son obligatorios PrimerNombre, ApellidoPaterno y FechaNacimiento");

public class DatosPersonaJuridicaIncompletosException()
    : SolicitudInvalidaException("Para una persona jurídica son obligatorios RazonSocial y FechaCreacion");

public class ClienteNoExisteException(Guid idCliente)
    : ReglaDeNegocioException($"El cliente {idCliente} no existe");

public class EstadoClienteInvalidoException(string estado)
    : SolicitudInvalidaException($"Estado de cliente inválido: '{estado}'");

public class CatalogoSocioInvalidoException(string catalogo, string codigo)
    : ReglaDeNegocioException($"El código '{codigo}' no existe en el catálogo de {catalogo}");

public class UsuarioOficialInvalidoException(Guid idUsuario)
    : ReglaDeNegocioException($"El usuario {idUsuario} no existe o no está activo");
