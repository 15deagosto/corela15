namespace Corela15.Domain.Marketing;

public enum EstadoRifa
{
    Activa = 1,
    Cerrada = 2,
    Sorteada = 3
}

/// <summary>Rifas/promociones a socios — verificado contra MARKETING.RIFA.</summary>
public class Rifa
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaSorteo { get; set; }
    public EstadoRifa Estado { get; set; } = EstadoRifa.Activa;

    public List<RifaPremio> Premios { get; set; } = new();
}
