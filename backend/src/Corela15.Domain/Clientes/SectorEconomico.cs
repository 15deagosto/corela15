namespace Corela15.Domain.Clientes;

/// <summary>Catálogo real — verificado contra CLIENTES.SECTOR_ECONOMICO (6 filas).</summary>
public class SectorEconomico
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}
