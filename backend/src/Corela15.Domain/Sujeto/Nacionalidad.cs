namespace Corela15.Domain.Sujeto;

/// <summary>
/// Catálogo real de nacionalidades — verificado contra SUJETO.NACIONALIDAD
/// (213 filas, lista ISO de países). Sembradas las 212 activas.
/// </summary>
public class Nacionalidad
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
