namespace Corela15.Domain.Cajas;

/// <summary>
/// Catálogo real de recaudación de servicios de terceros (agua, luz,
/// IESS, telefonía...) — verificado contra CAJAS.PAGO_EXTERNO_PRODUCTO
/// (629 filas activas reales, el catálogo completo de un switch
/// agregador nacional). Sembrado solo con los 29 productos que tienen
/// actividad transaccional real de esta cooperativa (679 transacciones
/// reales en CAJAS.PAGO_EXTERNO_TRANSACCION) — el resto del catálogo
/// nacional (cientos de billers nunca usados por esta coop) queda fuera
/// a propósito, mismo criterio ya aplicado a Actividad Económica/
/// Profesión: sembrar el subconjunto real usado, no el universo
/// completo del estándar externo.
/// </summary>
public class PagoExternoProducto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TituloReferencia { get; set; } = string.Empty;
    public bool RequiereDatosFactura { get; set; }
    public bool TieneComision { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Registro real de un cobro de servicio de terceros en ventanilla —
/// verificado contra CAJAS.PAGO_EXTERNO_TRANSACCION. A diferencia de
/// Softbank (que integra en vivo contra un switch externo — Facilito/
/// Cisnergia, con URL/usuario/clave/token reales de la pasarela en
/// CAJAS.PAGO_EXTERNO, 1 fila real), este core no modela esa integración
/// (no hay conexión real a un switch de pagos desde este proyecto,
/// inventarla sería estructura sin backend real detrás — mismo criterio
/// ya aplicado en otras rondas) — el cajero registra el cobro real
/// recibido en efectivo, tal como quedaría confirmado por el switch.
/// </summary>
public class PagoExternoTransaccion
{
    public Guid Id { get; set; }

    public int IdProducto { get; set; }
    public PagoExternoProducto Producto { get; set; } = null!;

    public string Referencia { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public decimal Valor { get; set; }
    public decimal Comision { get; set; }

    public int IdAgencia { get; set; }

    public DateTimeOffset FechaProceso { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;

    public bool Reversada { get; set; }
    public DateTimeOffset? FechaReverso { get; set; }
    public string? ReversadaPor { get; set; }

    public Guid? IdComprobante { get; set; }
    public Guid? IdComprobanteReverso { get; set; }
}
