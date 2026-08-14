namespace Corela15.Domain.Credito;

/// <summary>
/// Tasa de interés activa efectiva máxima por segmento, publicada
/// mensualmente por la Junta de Política y Regulación Monetaria y
/// Financiera (BCE) — ningún producto de crédito puede tener una tasa
/// nominal anual por encima de su techo vigente. Configurable con
/// vigencia real (los techos cambian mes a mes) en vez de un valor fijo
/// hardcodeado — la validación siempre usa el techo vigente a la fecha,
/// no el que existía cuando se sembró el producto.
/// </summary>
public class TasaTechoBce
{
    public int Id { get; set; }
    public string Segmento { get; set; } = string.Empty;
    public decimal TasaMaxima { get; set; }
    public DateOnly FechaVigenciaDesde { get; set; }
    public bool Activo { get; set; } = true;
}
