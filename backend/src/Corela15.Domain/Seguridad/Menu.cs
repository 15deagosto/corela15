namespace Corela15.Domain.Seguridad;

/// <summary>
/// Un ítem del menú del sistema — el código coincide con el `slug` de cada
/// módulo en `frontend/src/modules.ts`. Verificado contra el patrón real de
/// Softbank (MENU/ROL_MENU, pendiente en Nivel 0 hasta ahora): controla qué
/// módulos ve y puede usar cada rol.
/// </summary>
public class Menu
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}
