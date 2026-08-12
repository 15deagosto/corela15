namespace Corela15.Domain.Ahorros;

/// <summary>Catálogo de los "baldes" de saldo dentro de una cuenta (disponible, encaje, bloqueo, interés...).</summary>
public class ItemSaldo
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
