namespace Corela15.Domain.Sujeto;

/// <summary>Catálogo real — verificado contra SUJETO.PROFESION (864 filas reales, 861 activas sembradas).</summary>
public class Profesion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
