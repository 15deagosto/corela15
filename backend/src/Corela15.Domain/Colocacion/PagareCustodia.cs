namespace Corela15.Domain.Colocacion;

/// <summary>
/// Catálogo real de estados — verificado contra COLOCACION.ESTADO_
/// CUSTODIO_PAGARE (2 filas: E Entregado, R Receptado).
/// </summary>
public class EstadoCustodioPagare
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Custodia física real del pagaré (título valor) de un préstamo —
/// verificado contra COLOCACION.CUSTODIO_PAGARE_INVENTARIO (2.845 filas
/// reales) — trazabilidad de dónde está guardado físicamente el pagaré
/// de cada préstamo (bóveda, ubicación) y si ya fue receptado o sigue
/// entregado. Simplificación consciente de esquema: la cabecera real
/// `CUSTODIO_PAGARE` (solo 5 filas — un "lote" de procesamiento masivo,
/// no un patrón operativo repetido) no se modeló aparte; cada registro
/// de custodia referencia directo el préstamo, con su propia bitácora
/// de movimientos.
/// </summary>
public class PagareCustodia
{
    public Guid Id { get; set; }

    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoCustodioPagare Estado { get; set; } = null!;

    public string Ubicacion { get; set; } = string.Empty;
    public DateOnly FechaActualizacion { get; set; }

    public List<PagareCustodiaMovimiento> Movimientos { get; set; } = [];
}

/// <summary>Bitácora inmutable de movimiento de custodia — nunca se borra.</summary>
public class PagareCustodiaMovimiento
{
    public Guid Id { get; set; }

    public Guid IdPagareCustodia { get; set; }
    public PagareCustodia PagareCustodia { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public bool EsRecepcion { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}
