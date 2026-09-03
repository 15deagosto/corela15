namespace Corela15.Domain.Cajas;

/// <summary>Catálogo real de "contenedores de valor" en ventanilla — verificado contra CAJAS.ITEMCAJA (4 filas reales).</summary>
public class ItemCaja
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
