namespace Corela15.Domain.Sujeto;

/// <summary>
/// Catálogo real de canales de recepción — verificado contra SUJETO.
/// PERSONA_RECLAMO_CANAL (3 filas: Presencial/Telefónico/Web).
/// </summary>
public class CanalReclamo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Catálogo real de estados — verificado contra SUJETO.PERSONA_RECLAMO_
/// ESTADO (2 filas: 1 En trámite, 2 Resuelto).
/// </summary>
public class EstadoReclamo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Catálogo real de tipo de resolución — verificado contra SUJETO.
/// PERSONA_RECLAMO_TIPORESOLUCION (4 filas).
/// </summary>
public class TipoResolucionReclamo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Catálogo real de tipo de producto — verificado contra SUJETO.
/// TIPO_PRODUCTO_RECLAMO (3 filas: Ahorros/Préstamo/Inversiones).
/// </summary>
public class TipoProductoReclamo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Catálogo real de concepto (grupo) del reclamo — verificado contra
/// SUJETO.PERSONA_RECLAMO_CONCEPTO (3 filas: CC Cartera de crédito,
/// OP Obligaciones con el público, TC Tarjetas de crédito).
/// </summary>
public class ConceptoReclamo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Catálogo real detallado de motivo del reclamo — verificado contra
/// SUJETO.PERSONA_RECLAMO_CONCEPTO_DETALLE (26 filas reales, catálogo
/// oficial SEPS de tipificación de reclamos por cobros indebidos).
/// </summary>
public class ConceptoReclamoDetalle
{
    public string Codigo { get; set; } = string.Empty;
    public string CodigoConcepto { get; set; } = string.Empty;
    public ConceptoReclamo Concepto { get; set; } = null!;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Reclamo real de un socio/cliente — canal formal de reclamos exigido
/// por la Ley Orgánica de Defensa del Consumidor en Ecuador. Verificado
/// contra SUJETO.PERSONA_RECLAMO (estructura real vigente, con los
/// catálogos SEPS ya sembrados — a diferencia de SUJETO.RECLAMO, un
/// formato legacy de "escrito de reclamo" con solo 1 fila real y sin
/// relación a los catálogos oficiales).
/// </summary>
public class Reclamo
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public string CodigoCanalRecepcion { get; set; } = string.Empty;
    public CanalReclamo CanalRecepcion { get; set; } = null!;

    public DateOnly FechaRecepcion { get; set; }

    public int IdTipoProducto { get; set; }
    public TipoProductoReclamo TipoProducto { get; set; } = null!;

    public string CodigoConceptoDetalle { get; set; } = string.Empty;
    public ConceptoReclamoDetalle ConceptoDetalle { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoReclamo Estado { get; set; } = null!;

    public string Descripcion { get; set; } = string.Empty;
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }

    public ReclamoRespuesta? Respuesta { get; set; }
}

/// <summary>
/// Respuesta real del reclamo — verificado contra SUJETO.PERSONA_
/// RECLAMO_RESPUESTA (monto restituido + interés sobre el monto, cuando
/// el reclamo resulta favorable o parcialmente favorable al socio).
/// </summary>
public class ReclamoRespuesta
{
    public Guid Id { get; set; }

    public Guid IdReclamo { get; set; }
    public Reclamo Reclamo { get; set; } = null!;

    public string CodigoTipoResolucion { get; set; } = string.Empty;
    public TipoResolucionReclamo TipoResolucion { get; set; } = null!;

    public decimal? MontoRestituido { get; set; }
    public decimal? InteresSobreMonto { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }
}
