namespace Corela15.Domain.Reporteria;

/// <summary>Un tablero marcado como favorito por un usuario. Clave compuesta (Usuario, IdTablero).</summary>
public class FavoritoTablero
{
    public string Usuario { get; set; } = string.Empty;
    public int IdTablero { get; set; }
    public Tablero Tablero { get; set; } = null!;
    public DateTimeOffset AgregadoEn { get; set; }
}
