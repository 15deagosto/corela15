namespace Corela15.Application.Portafolio;

public record AbrirInversionRequest(
    string Documento, int IdAgencia, string CodigoInstitucion, decimal ValorNominal, decimal Tasa,
    DateOnly FechaCompra, DateOnly FechaVencimiento, string RegistradoPor);

public record InversionAbiertaResult(Guid IdInversion, Guid IdComprobante);

public record CancelarInversionRequest(Guid IdInversion, string RegistradoPor);

public record InversionCanceladaResult(decimal ValorDevuelto, decimal InteresGanado, Guid IdComprobante);

public record RenovarInversionRequest(
    Guid IdInversion, string DocumentoNuevo, DateOnly FechaVencimientoNueva, string RegistradoPor);

public record InversionRenovadaResult(
    Guid IdInversionNueva, string DocumentoNuevo, decimal ValorNominal, decimal Tasa, decimal InteresGanado, Guid? IdComprobante);
