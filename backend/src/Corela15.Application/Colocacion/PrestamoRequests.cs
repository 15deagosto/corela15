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
