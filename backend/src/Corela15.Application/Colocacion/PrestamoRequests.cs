namespace Corela15.Application.Colocacion;

public record SolicitarPrestamoRequest(
    Guid IdCliente,
    int IdTipoPrestamo,
    int IdAgencia,
    decimal MontoSolicitado,
    int Cuotas,
    string RegistradoPor);

public record SolicitudPrestamoCreadaResult(Guid IdSolicitud, string Numero);

public record DesembolsarPrestamoRequest(Guid IdSolicitud, string RegistradoPor);

public record PrestamoDesembolsadoResult(Guid IdPrestamo, string Numero, Guid IdComprobanteContable);

public record PagarCuotaRequest(Guid IdPrestamo, string RegistradoPor);

public record PagoCuotaRegistradoResult(
    int NumeroCuota, decimal MontoCapital, decimal MontoInteres, decimal SaldoResultante,
    bool PrestamoCancelado, Guid IdComprobanteContable, int DiasMoraCuota, decimal MontoInteresMora);
