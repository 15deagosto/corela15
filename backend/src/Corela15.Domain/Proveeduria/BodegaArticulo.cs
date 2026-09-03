namespace Corela15.Domain.Proveeduria;

/// <summary>
/// Existencia real de un artículo en una bodega (kardex) — verificado
/// contra PROVEEDURIA.BODEGA_ARTICULO (218 filas reales). Fila viva por
/// (Bodega, Articulo) — el historial de movimientos vive en
/// <see cref="BodegaArticuloMovimiento"/>, no acá.
/// </summary>
public class BodegaArticulo
{
    public Guid Id { get; set; }

    public Guid IdBodega { get; set; }
    public Bodega Bodega { get; set; } = null!;

    public string CodigoArticulo { get; set; } = string.Empty;
    public Articulo Articulo { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}

/// <summary>
/// Movimiento real de kardex (ingreso o baja) — verificado contra
/// PROVEEDURIA.BODEGA_ARTICULO_MOVIMIENTO_TRANSACCION_DETALLE (452 filas
/// reales). Bitácora inmutable — nunca se edita ni se borra.
/// </summary>
public class BodegaArticuloMovimiento
{
    public Guid Id { get; set; }

    public Guid IdBodegaArticulo { get; set; }
    public BodegaArticulo BodegaArticulo { get; set; } = null!;

    public int Cantidad { get; set; }
    public decimal Valor { get; set; }
    public int SaldoResultante { get; set; }

    /// <summary>true = salida/consumo (baja), false = ingreso (compra/reposición).</summary>
    public bool EsBajaArticulo { get; set; }

    public string? Comentario { get; set; }
    public DateTimeOffset FechaSistema { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}
