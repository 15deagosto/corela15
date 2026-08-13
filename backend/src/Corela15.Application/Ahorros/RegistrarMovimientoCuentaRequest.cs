namespace Corela15.Application.Ahorros;

public record RegistrarMovimientoCuentaRequest(
    Guid IdCuenta,
    string CodigoTipoTransaccion,
    decimal Monto,
    string RegistradoPor);

public record MovimientoCuentaRegistradoResult(Guid IdMovimiento, decimal SaldoResultante, Guid IdComprobanteContable);
