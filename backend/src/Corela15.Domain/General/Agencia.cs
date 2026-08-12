namespace Corela15.Domain.General;

public class Agencia
{
    public int Id { get; set; }
    public int IdEmpresa { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsOperativa { get; set; }
    public bool Activa { get; set; } = true;
}
