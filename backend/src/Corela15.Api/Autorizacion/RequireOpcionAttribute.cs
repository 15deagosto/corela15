namespace Corela15.Api.Autorizacion;

/// <summary>
/// Tercer nivel de permiso (ver Opcion.cs): marca un endpoint puntual
/// (un reporte, una acción sensible) como exigiendo, además del menú del
/// módulo, un código de opción real otorgado por rol o directo a la
/// persona (claim "opcion" del JWT) -- ver OpcionFilter. A diferencia de
/// las policies "Menu:*"/"Estructura:*"/"Dataset:*" (que se declaran en
/// Program.cs contra un arreglo fijo de códigos conocidos al arrancar),
/// esto no requiere tocar Program.cs nunca: un código de opción nuevo
/// solo necesita existir en `seguridad.opcion` y usarse acá.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireOpcionAttribute(string codigo) : Attribute
{
    public string Codigo { get; } = codigo;
}
