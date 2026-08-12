namespace Corela15.Domain.Inversion;

public enum TipoPersonaTasa
{
    Natural = 1,
    Juridica = 2,
    Ambas = 3
}

/// <summary>
/// Tablero de tasas oficial para DPF, por rango de saldo/plazo/tipo de
/// persona — espejo simplificado de INVERSION.ITEMPLAZO_TASA (+ _DETALLE).
/// Toda renovación o apertura debe leer la tasa vigente de acá, nunca
/// hardcodear un valor (ver aprendizaje documentado en Deposito).
/// </summary>
public class ItemPlazoTasa
{
    public Guid Id { get; set; }

    public int PlazoDiasMin { get; set; }
    public int PlazoDiasMax { get; set; }
    public decimal MontoMin { get; set; }
    public decimal? MontoMax { get; set; }
    public TipoPersonaTasa TipoPersona { get; set; } = TipoPersonaTasa.Ambas;

    public decimal Tasa { get; set; }
    public DateOnly FechaVigenciaDesde { get; set; }
    public bool Activo { get; set; } = true;
}
