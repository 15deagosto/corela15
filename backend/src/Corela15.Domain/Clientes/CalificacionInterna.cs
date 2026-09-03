namespace Corela15.Domain.Clientes;

/// <summary>Catálogo real — verificado contra CLIENTES.CALIFICACION_INTERNA (3 filas).</summary>
public class CalificacionInterna
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}
