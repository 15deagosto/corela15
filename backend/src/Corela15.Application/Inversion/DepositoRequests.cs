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

public record RenovarDepositoRequest(
    Guid IdDepositoOrigen,
    int PlazoDias,
    decimal IncrementoCapital,
    bool EsPersonaJuridica,
    string RegistradoPor);

public record DepositoRenovadoResult(
    Guid IdDepositoDestino, string CodigoDestino, decimal MontoNuevo, decimal TasaAplicada,
    DateOnly FechaVencimiento, Guid? IdComprobanteContable);
