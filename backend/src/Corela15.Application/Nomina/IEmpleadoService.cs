using Corela15.Application.Common;

namespace Corela15.Application.Nomina;

public record CrearEmpleadoRequest(
    Guid? IdPersona, int IdAgencia, int IdCargo, DateOnly FechaIngreso, bool RecibeFondosReserva, decimal? SueldoInicial,
    // Datos para crear una Persona nueva — solo se usan cuando IdPersona es
    // null (alta de un empleado cuya persona todavía no existe en el
    // sistema). Mutuamente excluyente con IdPersona.
    int? IdTipoIdentificacion, string? Identificacion, string? PrimerNombre, string? SegundoNombre,
    string? ApellidoPaterno, string? ApellidoMaterno, DateOnly? FechaNacimiento, bool? EsMasculino, string? Email);

public record ActualizarEmpleadoRequest(int IdAgencia, int IdCargo, bool RecibeFondosReserva, string Estado);

public record EmpleadoCreadoResult(Guid IdEmpleado, Guid IdPersona, string Nombre);

public interface IEmpleadoService
{
    /// <summary>
    /// Da de alta a un empleado nuevo — vincula una Persona ya existente en
    /// el sistema si <c>IdPersona</c> viene informado, o crea una Persona
    /// (+ PersonaNatural) nueva a partir de los datos básicos si no — mismo
    /// criterio dual ya usado en Socios (buscar existente o crear nueva).
    /// </summary>
    Task<EmpleadoCreadoResult> CrearAsync(CrearEmpleadoRequest request, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edita agencia, cargo, si recibe fondos de reserva, y el estado
    /// (Activo/Vacaciones/LicenciaSinSueldo/Desvinculado) de un empleado
    /// ya existente.
    /// </summary>
    Task ActualizarAsync(Guid idEmpleado, ActualizarEmpleadoRequest request, CancellationToken cancellationToken = default);
}

public class PersonaInvalidaParaEmpleadoException(Guid idPersona)
    : ReglaDeNegocioException($"La persona {idPersona} no existe");

public class PersonaYaEsEmpleadoException(Guid idPersona)
    : ReglaDeNegocioException($"La persona {idPersona} ya tiene un registro de empleado");

public class DatosPersonaEmpleadoIncompletosException()
    : SolicitudInvalidaException(
        "Para crear un empleado se necesita vincular una persona existente (IdPersona) o los datos básicos de una " +
        "persona nueva: tipo y número de identificación, primer nombre, apellido paterno y fecha de nacimiento.");

public class TipoIdentificacionInvalidoParaEmpleadoException(int idTipoIdentificacion)
    : ReglaDeNegocioException($"El tipo de identificación {idTipoIdentificacion} no existe");

public class IdentificacionDuplicadaParaEmpleadoException(string identificacion)
    : ReglaDeNegocioException($"Ya existe una persona con la identificación '{identificacion}'");

public class CargoInvalidoException(int idCargo)
    : ReglaDeNegocioException($"El cargo {idCargo} no existe o no está activo");

public class AgenciaInvalidaParaEmpleadoException(int idAgencia)
    : ReglaDeNegocioException($"La agencia {idAgencia} no existe o no está activa");

public class EmpleadoInexistenteException(Guid idEmpleado)
    : ReglaDeNegocioException($"El empleado {idEmpleado} no existe");

public class EstadoEmpleadoInvalidoException(string estado)
    : SolicitudInvalidaException($"Estado de empleado inválido: '{estado}' (use Activo, Vacaciones, LicenciaSinSueldo o Desvinculado)");
