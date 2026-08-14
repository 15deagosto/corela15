using Corela15.Application.Common;

namespace Corela15.Application.Contabilidad;

public record CrearCuentaContableRequest(
    string Codigo, string Nombre, string Grupo, string Naturaleza, Guid? IdCuentaPadre, bool EsMayor, string RegistradoPor);

public record CuentaContableCreadaResult(Guid Id, string Codigo);

public record ActualizarCuentaContableRequest(Guid Id, string Nombre, bool Activa, string RegistradoPor);

public interface ICuentaContableAdminService
{
    /// <summary>
    /// Crea una subcuenta nueva del plan de cuentas. El versionado real
    /// (cuenta_contable_historico) lo dispara el trigger de base de datos
    /// automáticamente — este servicio no necesita saber que existe.
    /// </summary>
    Task<CuentaContableCreadaResult> CrearAsync(CrearCuentaContableRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza nombre y estado activo — código, grupo, naturaleza y
    /// es_mayor son inmutables después de creada (cambiarlos rompería la
    /// integridad de los asientos ya registrados contra esa cuenta).
    /// Desactivar una cuenta con saldo distinto de cero, o referenciada
    /// por un tipo de transacción activo, se rechaza — no es un catálogo
    /// simple, tiene invariantes reales.
    /// </summary>
    Task ActualizarAsync(ActualizarCuentaContableRequest request, CancellationToken cancellationToken = default);
}

public class CuentaContablePadreInvalidaException(Guid idCuentaPadre)
    : ReglaDeNegocioException($"La cuenta padre {idCuentaPadre} no existe o es una cuenta de detalle (no puede tener subcuentas)");

public class CuentaContableNoExisteException(Guid id)
    : ReglaDeNegocioException($"La cuenta contable {id} no existe");

public class CuentaContableConSaldoException(string codigo)
    : ReglaDeNegocioException($"La cuenta {codigo} tiene saldo distinto de cero — no se puede desactivar");

public class CuentaContableEnUsoException(string codigo)
    : ReglaDeNegocioException($"La cuenta {codigo} está en uso por un tipo de transacción activo — no se puede desactivar");
