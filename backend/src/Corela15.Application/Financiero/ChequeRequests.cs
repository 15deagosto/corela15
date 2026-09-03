namespace Corela15.Application.Financiero;

public record RegistrarChequeRequest(
    int IdBanco, string CuentaCorriente, string NumeroCheque, decimal Valor,
    Guid IdCuenta, int IdAgencia, string RegistradoPor);

public record ChequeRegistradoResult(Guid IdCheque);

public record DepositarChequeRequest(Guid IdCheque, string RegistradoPor);

public record ChequeDepositadoResult(Guid IdComprobante);

public record EfectivizarChequeRequest(Guid IdCheque, string RegistradoPor);

public record ChequeEfectivizadoResult(Guid IdComprobante);

public record ProtestarChequeRequest(Guid IdCheque, string? Documento, string RegistradoPor);

public record ChequeProtestadoResult(Guid IdComprobante);
