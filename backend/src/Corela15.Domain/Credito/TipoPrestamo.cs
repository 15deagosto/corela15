namespace Corela15.Domain.Credito;

/// <summary>
/// Catálogo de productos de crédito — verificado contra CREDITO.TIPO_PRESTAMO
/// (60 filas en Softbank). Campos de negocio reales confirmados que valen la
/// pena modelar desde ahora (evitan un producto de crédito inválido): rango
/// de monto y plazo permitido.
/// </summary>
public class TipoPrestamo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal MontoMinimo { get; set; }
    public decimal MontoMaximo { get; set; }
    public int PlazoMinimoDias { get; set; }
    public int PlazoMaximoDias { get; set; }

    /// <summary>Tasa nominal anual del producto — no verificada como columna
    /// directa en TIPO_PRESTAMO (Softbank la maneja vía tablero de tasas
    /// aparte), pero un producto de crédito real de cooperativa siempre
    /// tiene una tasa base configurada, no la inventa cada oficial al
    /// desembolsar. Necesaria para calcular la tabla de amortización.</summary>
    public decimal TasaAnual { get; set; }

    /// <summary>
    /// Segmento de crédito BCE al que pertenece este producto (debe
    /// coincidir con TasaTechoBce.Segmento) — determina contra qué techo
    /// regulatorio se valida TasaAnual. No es un campo libre: los
    /// segmentos reales que publica la Junta de Política y Regulación
    /// Monetaria y Financiera son un catálogo cerrado (Consumo
    /// Prioritario/Ordinario, Microcrédito Minorista/Acumulación
    /// Simple/Acumulación Ampliada, Productivo Corporativo/Empresarial/
    /// PYMES, Vivienda, Vivienda de Interés Público).
    /// </summary>
    public string SegmentoBce { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
