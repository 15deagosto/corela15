namespace Corela15.Application.Inversion;

public record AbrirDepositoRequest(
    Guid IdCliente,
    int IdAgencia,
    decimal Monto,
    int PlazoDias,
    bool EsPersonaJuridica,
    string RegistradoPor);

public record DepositoAbiertoResult(Guid IdDeposito, string Codigo, decimal Tasa, DateOnly FechaVencimiento, Guid IdComprobanteContable);

public record CancelarDepositoRequest(Guid IdDeposito, string RegistradoPor);

public record DepositoCanceladoResult(decimal ValorDevuelto, Guid IdComprobanteContable);
