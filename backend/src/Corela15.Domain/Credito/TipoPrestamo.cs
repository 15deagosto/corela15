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
    public bool Activo { get; set; } = true;
}
