namespace Corela15.Application.Ahorros;

public record AbrirCuentaAhorroRequest(
    Guid IdCliente,
    int IdTipoCuenta,
    int IdAgencia,
    decimal MontoInicial,
    string RegistradoPor);

public record CuentaAhorroAbiertaResult(Guid IdCuenta, string Numero, Guid? IdComprobanteContable);
