namespace Corela15.Domain.Seguridad;

/// <summary>
/// Catálogo real de estructuras/reportes regulatorios que el módulo
/// "Estructuras y Procesos Financieros" puede generar (OF01 primero, con
/// espacio para sumar B11/B13/S01/ROTEF/etc. si se migran acá después) —
/// segundo nivel de permiso, más fino que el menú: un rol puede tener
/// acceso al módulo entero y aun así solo poder generar ciertas
/// estructuras (ver <see cref="RolTipoEstructura"/>), justo lo que permite
/// crear un usuario genérico que solo vea/genere una estructura puntual.
/// </summary>
public class TipoEstructura
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
