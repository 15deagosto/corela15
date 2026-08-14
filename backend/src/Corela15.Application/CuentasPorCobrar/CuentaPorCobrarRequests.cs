namespace Corela15.Application.CuentasPorCobrar;

public record RegistrarCuentaPorCobrarRequest(
    string Concepto, int IdAgencia, Guid IdPersona, int Cuotas, decimal MontoInicial,
    DateOnly FechaVencimiento, string RegistradoPor);

public record CuentaPorCobrarRegistradaResult(Guid IdCuentaPorCobrar, Guid IdComprobante);

public record AbonarCuentaPorCobrarRequest(Guid IdCuentaPorCobrar, decimal Monto, string RegistradoPor);

public record AbonoCuentaPorCobrarRegistradoResult(decimal SaldoResultante, bool Cancelada, Guid IdComprobante);
