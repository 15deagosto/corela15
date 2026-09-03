using Corela15.Application.Common;

namespace Corela15.Application.Nomina;

public record CrearSolicitudAccionPersonalRequest(
    Guid IdEmpleado, int IdTipoAccionPersonal, string Detalle,
    int? IdCargoNuevo, decimal? NuevoSueldo, int? IdAgenciaNueva, bool? NuevoValorRecibeFondosReserva);

public record SolicitudAccionPersonalCreadaResult(Guid Id, int NumeroAccion);

public interface ISolicitudAccionPersonalService
{
    /// <summary>
    /// Ingresa una solicitud de acción de personal (estado Ingresada) —
    /// valida el empleado y el tipo, y que los datos específicos del tipo
    /// elegido (cargo nuevo, agencia nueva, nuevo valor de fondos de
    /// reserva) estén presentes cuando el tipo los requiere.
    /// </summary>
    Task<SolicitudAccionPersonalCreadaResult> CrearAsync(
        CrearSolicitudAccionPersonalRequest request, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aprueba la solicitud y aplica su efecto real sobre el empleado
    /// (activar/desactivar contrato, cambiar cargo, cambiar agencia, o
    /// cambiar si recibe fondos de reserva) — solo procede desde
    /// Ingresada.
    /// </summary>
    Task AprobarAsync(Guid id, string? comentario, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>Anula la solicitud — solo procede desde Ingresada (antes de tener efecto real).</summary>
    Task AnularAsync(Guid id, string motivo, string registradoPor, CancellationToken cancellationToken = default);
}

public class EmpleadoInvalidoParaAccionException(Guid idEmpleado)
    : ReglaDeNegocioException($"El empleado {idEmpleado} no existe");

public class TipoAccionPersonalInvalidoException(int idTipo)
    : ReglaDeNegocioException($"El tipo de acción de personal {idTipo} no existe o no está activo");

public class DatosAccionPersonalIncompletosException(string mensaje) : SolicitudInvalidaException(mensaje);

public class SolicitudAccionPersonalInexistenteException(Guid id)
    : ReglaDeNegocioException($"La solicitud de acción de personal {id} no existe");

public class SolicitudAccionPersonalNoIngresadaException(Guid id)
    : ReglaDeNegocioException($"La solicitud {id} ya fue procesada — solo se puede aprobar o anular mientras está Ingresada");
