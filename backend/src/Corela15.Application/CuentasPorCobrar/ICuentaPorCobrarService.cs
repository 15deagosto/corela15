using Corela15.Application.Common;

namespace Corela15.Application.CuentasPorCobrar;

public interface ICuentaPorCobrarService
{
    /// <summary>Registra una cuenta por cobrar interna y su asiento (débito 1601 / crédito 5601).</summary>
    Task<CuentaPorCobrarRegistradaResult> RegistrarAsync(
        RegistrarCuentaPorCobrarRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un abono (débito Caja / crédito 1601), reduce el saldo, y
    /// marca la cuenta Cancelada automáticamente cuando el saldo llega a
    /// cero — mismo patrón que el pago de cuota de préstamo (Nivel 3).
    /// </summary>
    Task<AbonoCuentaPorCobrarRegistradoResult> AbonarAsync(
        AbonarCuentaPorCobrarRequest request, CancellationToken cancellationToken = default);
}

public class MontoCuentaPorCobrarInvalidoException(decimal monto)
    : SolicitudInvalidaException($"El monto {monto:0.00} debe ser mayor a cero");

public class CuentaPorCobrarInvalidaException(Guid idCuentaPorCobrar)
    : ReglaDeNegocioException($"La cuenta por cobrar {idCuentaPorCobrar} no existe o ya no está vigente");

public class AbonoExcedeSaldoException(decimal monto, decimal saldo)
    : ReglaDeNegocioException($"El abono {monto:0.00} excede el saldo pendiente {saldo:0.00}");
