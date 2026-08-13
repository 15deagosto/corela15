namespace Corela15.Domain.Marketing;

/// <summary>Premios de una rifa — verificado contra MARKETING.RIFA_PREMIO.</summary>
public class RifaPremio
{
    public Guid Id { get; set; }

    public Guid IdRifa { get; set; }
    public Rifa Rifa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public decimal? ValorReferencial { get; set; }
}
