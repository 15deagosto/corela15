namespace Corela15.Application.CuentasPorCobrar;

public record RegistrarCuentaPorPagarRequest(
    string Concepto, int IdAgencia, Guid IdPersona, int Cuotas, decimal MontoInicial,
    DateOnly FechaVencimiento, string RegistradoPor);

public record CuentaPorPagarRegistradaResult(Guid IdCuentaPorPagar, Guid IdComprobante);

public record PagarCuentaPorPagarRequest(
    Guid IdCuentaPorPagar, decimal Monto, string CodigoFormaCancelacion, string RegistradoPor);

public record PagoCuentaPorPagarRegistradoResult(decimal SaldoResultante, bool Cancelada, Guid IdComprobante);

public record AnularCuentaPorPagarRequest(Guid IdCuentaPorPagar, string Motivo, string RegistradoPor);

public record CuentaPorPagarAnuladaResult(Guid IdComprobante);
