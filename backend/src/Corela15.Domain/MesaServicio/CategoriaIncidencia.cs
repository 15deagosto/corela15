namespace Corela15.Domain.MesaServicio;

/// <summary>
/// Catálogo real de categorías de incidencia — control de incidencias
/// exigido por la SEPS (registro y clasificación de eventos que afectan
/// la operación). Catálogo simple, editable desde Configuración (código
/// único, sin invariante de negocio más allá de eso), mismo patrón que
/// otros catálogos simples del proyecto.
/// </summary>
public class CategoriaIncidencia
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
