namespace Corela15.Domain.Sujeto;

/// <summary>Catálogo real — verificado contra SUJETO.ESTADO_CIVIL (7 filas).</summary>
public class EstadoCivil
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
