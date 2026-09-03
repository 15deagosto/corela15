namespace Corela15.Domain.Colocacion;

/// <summary>
/// Catálogo de rubros aplicables a un préstamo — verificado contra
/// COLOCACION.RUBRO (46 filas reales). No tiene un código propio (a
/// diferencia de un diseño previo de esta sesión, corregido tras
/// verificar la fuente real): cada rubro es un nombre individual
/// (ej. "Capital", "Gastos Judiciales", "Notificaciones") clasificado por
/// su <see cref="TipoRubro"/> compartido — "identificar la cuota de
/// capital" se resuelve por <c>TipoRubro.EsCapital</c>, no por comparar un
/// código de texto contra "CAP".
/// </summary>
public class Rubro
{
    public int Id { get; set; }

    public string CodigoTipoRubro { get; set; } = string.Empty;
    public TipoRubro TipoRubro { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Rubro con cálculo diario adicional (ej. seguro desgravamen) — COLOCACION.RUBRO.ESCALCULOADICIONAL.</summary>
    public bool EsCalculoAdicional { get; set; }

    /// <summary>Genera un asiento de adjudicación al desembolsar (ej. Capital) — COLOCACION.RUBRO.GENERAADJUDICACION.</summary>
    public bool GeneraAdjudicacion { get; set; }

    /// <summary>
    /// Al cargarse manualmente a un préstamo, genera una cuenta por cobrar
    /// real (ver <see cref="PrestamoRubroCuentaPorCobrar"/>) — verificado
    /// contra COLOCACION.RUBRO.ESCUENTAPORCOBRAR y contra
    /// COLOCACION.RUBRO_CUENTAPORCOBRAR (7 rubros reales configurados así:
    /// Certificado de Gravamen, Notificaciones, Inicio/Demanda Judicial,
    /// Cobranzas, Gastos, Gastos Judiciales).
    /// </summary>
    public bool EsCuentaPorCobrar { get; set; }

    public int OrdenDeCobro { get; set; }
    public bool Activo { get; set; } = true;

    /// <summary>Genera factura al cobrarse — COLOCACION.RUBRO.SEFACTURA.</summary>
    public bool SeFactura { get; set; }

    /// <summary>Tarifa de impuesto aplicable (IVA sobre el rubro, ej. gastos de cobranza) — COLOCACION.RUBRO.TARIFAIMPUESTO.</summary>
    public decimal TarifaImpuesto { get; set; }

    /// <summary>Rubro adicional a la cuota normal — COLOCACION.RUBRO.ESADICIONAL.</summary>
    public bool EsAdicional { get; set; }

    /// <summary>Puede condonarse (perdonarse) total o parcialmente — COLOCACION.RUBRO.ESCONDONACION.</summary>
    public bool EsCondonacion { get; set; }
}
