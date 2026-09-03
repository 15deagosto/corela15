using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Proveeduria;

/// <summary>Verificado contra PROVEEDURIA.ESTADOPEDIDO (3 códigos reales).</summary>
public enum EstadoSolicitudPedido
{
    Ingresada = 1,
    Procesada = 2,
    Anulada = 3,
}

/// <summary>
/// Solicitud real de pedido de suministros a una bodega — verificado
/// contra PROVEEDURIA.SOLICITUD_PEDIDO (41 filas reales) + _ARTICULO
/// (448 líneas reales) + _ETAPA (89 filas, bitácora real de transición).
/// </summary>
public class SolicitudPedido
{
    public Guid Id { get; set; }

    public Guid IdBodega { get; set; }
    public Bodega Bodega { get; set; } = null!;

    public Guid IdUsuarioSolicitante { get; set; }
    public Usuario UsuarioSolicitante { get; set; } = null!;

    public string? Detalle { get; set; }
    public EstadoSolicitudPedido Estado { get; set; } = EstadoSolicitudPedido.Ingresada;

    public DateTimeOffset FechaSistema { get; set; }
    public DateTimeOffset? FechaProceso { get; set; }
}

/// <summary>Línea real de artículo solicitado.</summary>
public class SolicitudPedidoArticulo
{
    public Guid Id { get; set; }

    public Guid IdSolicitud { get; set; }
    public SolicitudPedido Solicitud { get; set; } = null!;

    public string CodigoArticulo { get; set; } = string.Empty;
    public Articulo Articulo { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Detalle { get; set; }
}

/// <summary>Bitácora inmutable de transición de estado — nunca se edita ni se borra.</summary>
public class SolicitudPedidoEtapa
{
    public Guid Id { get; set; }

    public Guid IdSolicitud { get; set; }
    public SolicitudPedido Solicitud { get; set; } = null!;

    public EstadoSolicitudPedido Estado { get; set; }
    public string? Comentario { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset FechaSistema { get; set; }
}
