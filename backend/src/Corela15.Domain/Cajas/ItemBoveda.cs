namespace Corela15.Domain.Cajas;

/// <summary>Catálogo real de contenedores de valor en bóveda — verificado contra CAJAS.ITEMBOVEDA (2 filas reales: EFE, CHE).</summary>
public class ItemBoveda
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
