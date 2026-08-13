namespace Corela15.Application.Ahorros;

public interface ICuentaAhorroService
{
    /// <summary>
    /// Abre una cuenta de ahorro, crea sus ítems de saldo según el producto
    /// (TipoCuentaItemSaldo) y, si hay depósito inicial, registra el
    /// comprobante contable correspondiente (débito Caja / crédito
    /// Depósitos a la vista) vía IComprobanteContableService — la
    /// integración real entre Ahorros y Contabilidad.
    /// </summary>
    Task<CuentaAhorroAbiertaResult> AbrirAsync(
        AbrirCuentaAhorroRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un depósito o retiro (según el TipoTransaccion) sobre una
    /// cuenta ya abierta: valida saldo mínimo del producto y saldo
    /// suficiente, actualiza el ítem de saldo "Disponible", genera el
    /// asiento contable configurado en TipoTransaccion, y deja el
    /// movimiento en CuentaMovimiento para trazabilidad.
    /// </summary>
    Task<MovimientoCuentaRegistradoResult> RegistrarMovimientoAsync(
        RegistrarMovimientoCuentaRequest request, CancellationToken cancellationToken = default);
}

public class CuentaInvalidaException(Guid idCuenta)
    : Exception($"La cuenta {idCuenta} no existe o no está activa");

public class TipoTransaccionInvalidoException(string codigo)
    : Exception($"El tipo de transacción '{codigo}' no existe o está inactivo");

public class SaldoInsuficienteException(decimal saldoActual, decimal saldoMinimo, decimal montoSolicitado)
    : Exception(
        $"Saldo insuficiente: disponible {saldoActual:0.00}, mínimo exigido {saldoMinimo:0.00}, se pidió retirar {montoSolicitado:0.00}");

public class TipoCuentaInvalidoException(int idTipoCuenta)
    : Exception($"El tipo de cuenta {idTipoCuenta} no existe o está inactivo");

public class ClienteInvalidoException(Guid idCliente)
    : Exception($"El cliente {idCliente} no existe o no está activo");
