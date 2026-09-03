namespace Corela15.Application.Proveeduria;

public record LineaSolicitudPedidoRequest(string CodigoArticulo, int Cantidad, string? Detalle);

public record CrearSolicitudPedidoRequest(
    Guid IdBodega, Guid IdUsuarioSolicitante, string? Detalle,
    IReadOnlyList<LineaSolicitudPedidoRequest> Lineas, string RegistradoPor);

public record SolicitudPedidoCreadaResult(Guid IdSolicitud);

public record ProcesarSolicitudPedidoResult(Guid? IdComprobante, decimal ValorTotal);

public record IngresarStockRequest(
    Guid IdBodega, string CodigoArticulo, int Cantidad, decimal PrecioUnitario, string RegistradoPor);

public record StockIngresadoResult(Guid IdComprobante, int CantidadResultante, decimal ValorTotalResultante);
