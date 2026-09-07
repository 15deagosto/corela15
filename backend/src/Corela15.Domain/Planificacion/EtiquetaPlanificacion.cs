namespace Corela15.Domain.Planificacion;

/// <summary>
/// Categoría/etiqueta real de un bloque de la planificación semanal (ej.
/// "Soporte y continuidad del negocio", "Infraestructura y respaldos") --
/// con un color real (hex) para pintar el bloque en la grilla visual,
/// igual que el ejemplo real que ya usaba el usuario en su PDF manual.
/// Editable desde Configuración -- cada área puede necesitar sus propias
/// categorías con el tiempo, no es un catálogo cerrado.
/// </summary>
public class EtiquetaPlanificacion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#6b7280";
    public bool Activo { get; set; } = true;
}
