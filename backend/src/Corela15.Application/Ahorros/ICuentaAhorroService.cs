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
}

public class TipoCuentaInvalidoException(int idTipoCuenta)
    : Exception($"El tipo de cuenta {idTipoCuenta} no existe o está inactivo");

public class ClienteInvalidoException(Guid idCliente)
    : Exception($"El cliente {idCliente} no existe o no está activo");
