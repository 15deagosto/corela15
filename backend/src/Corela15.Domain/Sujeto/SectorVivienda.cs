namespace Corela15.Domain.Sujeto;

/// <summary>Catálogo real — verificado contra SUJETO.SECTORVIVIENDA (5 filas).</summary>
public class SectorVivienda
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
