namespace Corela15.Domain.Credito;

public class TipoConvenio
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public int IdAgencia { get; set; }
    public General.Agencia Agencia { get; set; } = null!;

    public bool EsCooperativa { get; set; }
    public decimal ValorAhorro { get; set; }
    public bool Activo { get; set; } = true;
}
