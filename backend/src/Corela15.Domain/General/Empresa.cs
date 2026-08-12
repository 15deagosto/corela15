namespace Corela15.Domain.General;

/// <summary>La cooperativa misma. Una sola fila en operación normal.</summary>
public class Empresa
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
    public int IdMoneda { get; set; }
    public Moneda Moneda { get; set; } = null!;
    public int IdPais { get; set; }
    public Pais Pais { get; set; } = null!;
}
