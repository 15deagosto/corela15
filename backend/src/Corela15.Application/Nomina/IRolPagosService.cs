using Corela15.Application.Common;

namespace Corela15.Application.Nomina;

public interface IRolPagosService
{
    /// <summary>
    /// Genera el rol de pagos de un período: crea una línea por empleado con
    /// los ingresos/egresos/días laborados digitados (siguiendo el diseño
    /// real de Softbank — NOMINA.ROLPAGOS_EMPLEADO.INGRESOS se digita por
    /// período, no se deriva de un sueldo base fijo por cargo, verificado
    /// que esa tabla no existe). Rechaza período+tipo duplicado y empleados
    /// inactivos o inexistentes.
    /// </summary>
    Task<RolPagosGeneradoResult> GenerarAsync(GenerarRolPagosRequest request, CancellationToken cancellationToken = default);
}

public class RolPagosDuplicadoException(DateOnly periodo, string tipo)
    : ReglaDeNegocioException($"Ya existe un rol de pagos {tipo} para el período {periodo:yyyy-MM-dd}");

public class EmpleadoInvalidoException(Guid idEmpleado)
    : ReglaDeNegocioException($"El empleado {idEmpleado} no existe o no está activo");

public class TipoRolPagosInvalidoException(string tipo)
    : SolicitudInvalidaException($"Tipo de rol de pagos inválido: {tipo} (use Mensual o Quincenal)");

public class RolPagosSinLineasException()
    : SolicitudInvalidaException("El rol de pagos debe incluir al menos un empleado");
