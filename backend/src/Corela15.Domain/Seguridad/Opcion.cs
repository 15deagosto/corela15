namespace Corela15.Domain.Seguridad;

/// <summary>
/// Tercer nivel de permiso, el más fino de los tres (Menú → Estructura/
/// Dataset (por módulo específico) → Opción (genérico, cualquier módulo)):
/// un reporte puntual, una pantalla puntual o una acción puntual dentro de
/// un módulo. Pedido explícito del usuario: a veces una persona necesita
/// UN SOLO reporte o UNA SOLA opción de un módulo, no el módulo entero —
/// dárselo por menú sería de más, un riesgo real de sobre-otorgar acceso.
///
/// A diferencia de TipoEstructura/DatasetReporteria (una tabla propia por
/// módulo), esta es genérica y reusable por cualquier módulo — evita crear
/// una tabla nueva cada vez que un módulo necesite este nivel. Se siembra
/// a mano, módulo por módulo, solo con opciones reales que valga la pena
/// gatear (nunca se replica automáticamente cada endpoint — eso fue
/// evaluado y descartado explícitamente para SEGURIDAD.DIRECCION/
/// MENU_DIRECCION de Softbank, plomería de pantallas del cliente viejo,
/// no un permiso de negocio real).
/// </summary>
public class Opcion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Menú al que pertenece, solo para agrupar en la pantalla de administración -- no se valida en el gate real (el claim "opcion" ya es suficiente).</summary>
    public string CodigoMenu { get; set; } = string.Empty;
    public Menu Menu { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
