namespace Corela15.Domain.Contabilidad;

/// <summary>Catálogo de tipos de comprobante (ingreso, egreso, diario...).</summary>
public class TipoComprobanteContable
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
