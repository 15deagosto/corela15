namespace Corela15.Application.Contabilidad;

public record LineaMovimientoRequest(Guid IdCuentaContable, decimal Debito, decimal Credito, string? Descripcion);

public record RegistrarComprobanteContableRequest(
    DateOnly Fecha,
    int IdTipoComprobante,
    int IdAgencia,
    string? Descripcion,
    string RegistradoPor,
    IReadOnlyList<LineaMovimientoRequest> Lineas);

public record ComprobanteContableRegistradoResult(Guid Id, long Numero);
