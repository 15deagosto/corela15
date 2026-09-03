using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Proveedor real — verificado contra CONTABILIDAD.PROVEEDOR (621 filas
/// reales). A diferencia de otras entidades del core, no se fuerza a
/// vincular una Persona real (`IdPersona` nullable, mismo criterio que
/// Softbank): buena parte de los 621 proveedores reales son empresas
/// nacionales grandes (CNT, CNEL, distribuidoras...) que este core nunca
/// modeló como Persona — obligarlo hubiera sido alta masiva de datos
/// fuera del alcance real de un proveedor de compras.
/// </summary>
public class Proveedor
{
    public Guid Id { get; set; }

    public Guid? IdPersona { get; set; }
    public Persona? Persona { get; set; }

    public int IdTipoIdentificacion { get; set; }
    public TipoIdentificacion TipoIdentificacion { get; set; } = null!;
    public string Identificacion { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    public bool EsContribuyenteEspecial { get; set; }
    public bool ObligadoLlevarContabilidad { get; set; }

    public bool Activo { get; set; } = true;
}

/// <summary>
/// Tipo de comprobante real del proveedor (catálogo oficial SRI, Tabla
/// de "Tipos de comprobante" del Anexo Transaccional Simplificado — ATS)
/// — verificado contra CONTABILIDAD.ATS_TIPOCOMPROBANTE, sembrado solo
/// con los 7 códigos que tienen uso real en CONTABILIDAD.COMPRAS de esta
/// cooperativa (01 Factura, 02 Nota o boleta de venta, 03 Liquidación de
/// compra, 04 Nota de crédito, 20/41/47 — casos especiales de reembolso/
/// instituciones del Estado).
/// </summary>
public class TipoComprobanteCompra
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public enum EstadoCompra
{
    Procesada,
    Anulada,
    Reversada,
}

/// <summary>
/// Registro real de compra/factura de proveedor — verificado contra
/// CONTABILIDAD.COMPRAS (3.029 filas reales) + CONTABILIDAD.COMPRAS_DETALLE
/// (4.679 líneas reales). Cierra el "motor Compras/SRI" documentado como
/// pendiente desde Cuentas por Pagar y Proveeduría — no modela el ciclo
/// completo de cumplimiento tributario SRI (retenciones electrónicas,
/// ATS de envío, imágenes/documentación adjunta — las tablas reales de
/// esas extensiones tienen 0 filas en Softbank, sin caso de uso real
/// detrás), solo el registro contable real de la factura de compra con
/// sus campos de identificación tributaria (sustento, comprobante,
/// establecimiento-puntoEmisión-secuencial-autorización) y el pasivo
/// real con el proveedor.
/// </summary>
public class Compra
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public Guid IdProveedor { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    /// <summary>
    /// Sustento tributario real (Tabla 5 SRI — catálogo público oficial,
    /// no modelado como tabla en Softbank, hardcodeado como constante
    /// igual que otros catálogos pequeños de un solo consumidor ya
    /// usados en el proyecto): 01 Crédito tributario, 02 Costo o gasto,
    /// 04 Costo o gasto por reembolso — los 3 códigos con uso real en
    /// CONTABILIDAD.COMPRAS de esta cooperativa.
    /// </summary>
    public string CodigoSustento { get; set; } = string.Empty;

    public string CodigoTipoComprobante { get; set; } = string.Empty;
    public TipoComprobanteCompra TipoComprobante { get; set; } = null!;

    public string Establecimiento { get; set; } = string.Empty;
    public string PuntoEmision { get; set; } = string.Empty;
    public string Secuencial { get; set; } = string.Empty;
    public string Autorizacion { get; set; } = string.Empty;

    public DateOnly FechaEmision { get; set; }
    public string Concepto { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal MontoIva { get; set; }
    public decimal MontoRetencion { get; set; }
    public decimal Total { get; set; }

    public decimal MontoInicial { get; set; }
    public decimal Saldo { get; set; }

    public EstadoCompra Estado { get; set; } = EstadoCompra.Procesada;

    public DateOnly FechaRegistro { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public Guid? IdComprobante { get; set; }
    public Guid? IdComprobanteReverso { get; set; }

    public List<CompraDetalle> Detalle { get; set; } = [];
}

public class CompraDetalle
{
    public Guid Id { get; set; }

    public Guid IdCompra { get; set; }
    public Compra Compra { get; set; } = null!;

    public Guid IdCuentaContable { get; set; }
    public CuentaContable CuentaContable { get; set; } = null!;

    public string Detalle { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal PorcentajeIva { get; set; }

    public decimal Subtotal { get; set; }
    public decimal MontoIva { get; set; }
    public decimal Total { get; set; }
}
