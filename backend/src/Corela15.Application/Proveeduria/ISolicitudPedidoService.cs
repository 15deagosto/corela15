using Corela15.Application.Common;

namespace Corela15.Application.Proveeduria;

public interface ISolicitudPedidoService
{
    /// <summary>Ingresa una solicitud de pedido de suministros a una bodega, con sus líneas — sin efecto de stock todavía.</summary>
    Task<SolicitudPedidoCreadaResult> CrearAsync(CrearSolicitudPedidoRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Procesa una solicitud ingresada: por cada línea, da de baja el
    /// stock real de la bodega (kardex) y consolida un asiento contable
    /// por tipo de artículo tocado (débito gasto / crédito 190615
    /// Proveeduría). Si la bodega no tiene stock suficiente de algún
    /// artículo, se rechaza completa — sin procesamiento parcial.
    /// </summary>
    Task<ProcesarSolicitudPedidoResult> ProcesarAsync(Guid idSolicitud, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>Anula una solicitud ingresada, sin efecto de stock (nunca se procesó).</summary>
    Task AnularAsync(Guid idSolicitud, string registradoPor, string? comentario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingresa stock real a una bodega (compra/reposición) — débito 190615
    /// Proveeduría / crédito Caja. Actualiza cantidad y valor promedio
    /// ponderado del kardex.
    /// </summary>
    Task<StockIngresadoResult> IngresarStockAsync(IngresarStockRequest request, CancellationToken cancellationToken = default);
}

public class BodegaInvalidaException(Guid idBodega)
    : ReglaDeNegocioException($"La bodega {idBodega} no existe o está inactiva");

public class ArticuloInvalidoException(string codigoArticulo)
    : ReglaDeNegocioException($"El artículo '{codigoArticulo}' no existe o está inactivo");

public class SolicitudPedidoInvalidaException(Guid idSolicitud, string estadoEsperado)
    : ReglaDeNegocioException($"La solicitud {idSolicitud} no existe o no está en estado '{estadoEsperado}'");

public class SolicitudPedidoSinLineasException()
    : SolicitudInvalidaException("La solicitud de pedido debe tener al menos una línea de artículo");

public class StockInsuficienteException(string codigoArticulo, int disponible, int solicitado)
    : ReglaDeNegocioException(
        $"Stock insuficiente del artículo '{codigoArticulo}': disponible {disponible}, solicitado {solicitado}");

public class CantidadInvalidaException(int cantidad)
    : SolicitudInvalidaException($"La cantidad {cantidad} debe ser mayor a cero");
