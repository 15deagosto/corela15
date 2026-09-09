namespace Corela15.Domain.Seguridad;

/// <summary>
/// Otorgamiento de un menú directo a un usuario puntual, además de lo que
/// ya le da su(s) rol(es) -- nunca resta lo que el rol otorga, solo suma.
/// Cierra el pedido real de poder darle acceso a un módulo a una sola
/// persona sin tener que crear (o tocar) un rol para eso. Mismo criterio
/// de unión ya usado para roles temporales/permanentes en
/// AuthService.EmitirTokenAsync.
/// </summary>
public class UsuarioMenu
{
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int IdMenu { get; set; }
    public Menu Menu { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
