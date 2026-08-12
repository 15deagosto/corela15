namespace Corela15.Domain.Proveeduria;

/// <summary>Catálogo de artículos de bodega — verificado contra PROVEEDURIA.ARTICULO.</summary>
public class Articulo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public bool Activo { get; set; } = true;
}
