namespace Corela15.Domain.Proveeduria;

/// <summary>
/// Catálogo real de artículos de bodega — verificado contra PROVEEDURIA.
/// ARTICULO (114 filas reales, código propio como clave natural real, no
/// inventado — mismo criterio que otros catálogos de esta sesión que sí
/// usan un ID autogenerado cuando la fuente no tiene código propio).
/// </summary>
public class Articulo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public string CodigoTipoArticulo { get; set; } = string.Empty;
    public TipoArticulo TipoArticulo { get; set; } = null!;

    public string? Marca { get; set; }

    /// <summary>Unidad de medida real (UNIDAD/CAJA/GALON/ROLLO/PAQUETE/MILLAR/PLIEGO/LITROS...).</summary>
    public string Multiplo { get; set; } = "UNIDAD";

    public bool Activo { get; set; } = true;
}
