namespace Corela15.Domain.Colocacion;

/// <summary>
/// Catálogo de rubros de cuota (capital, interés, mora, seguro...) —
/// verificado contra COLOCACION.RUBRO.
/// </summary>
public class Rubro
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsCuentaPorCobrar { get; set; }
    public int OrdenDeCobro { get; set; }
    public bool Activo { get; set; } = true;
}
