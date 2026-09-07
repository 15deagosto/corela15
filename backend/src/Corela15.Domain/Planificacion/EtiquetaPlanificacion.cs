namespace Corela15.Domain.Planificacion;

/// <summary>
/// Categoría/etiqueta real de un bloque de la planificación semanal (ej.
/// "Soporte y continuidad del negocio", "Infraestructura y respaldos") --
/// con un color real (hex) para pintar el bloque en la grilla visual,
/// igual que el ejemplo real que ya usaba el usuario en su PDF manual.
///
/// Real por área, no un catálogo global compartido -- distintas áreas
/// necesitan distintas categorías (lo que aplica a TI no necesariamente
/// aplica a Contabilidad o Cobranzas). Se puede seguir editando desde
/// Configuración, pero también se crea "sobre la marcha" cuando alguien
/// escribe una categoría nueva al planificar su semana (ver
/// IPlanificacionService.ObtenerOCrearEtiquetaAsync) -- el combo del
/// frontend queda editable, no un <select> cerrado.
/// </summary>
public class EtiquetaPlanificacion
{
    public string Codigo { get; set; } = string.Empty;
    public string CodigoArea { get; set; } = string.Empty;
    public AreaPlanificacion Area { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#6b7280";
    public bool Activo { get; set; } = true;
}
