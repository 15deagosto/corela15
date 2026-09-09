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

    /// <summary>
    /// Exclusión real: bloquea este menú para esta persona puntual aunque
    /// su rol se lo otorgue -- la única forma real de "restar" en un
    /// modelo que por diseño solo suma (rol ∪ directo). Tiene prioridad
    /// absoluta sobre cualquier otorgamiento (rol o directo): si
    /// Excluido=true, <see cref="Activo"/> se ignora en el cálculo final
    /// de AuthService.EmitirTokenAsync -- seguridad primero, nunca al
    /// revés. Nunca aplica a "mesa-servicio" (universal por requisito
    /// SEPS, sin excepción para nadie).
    /// </summary>
    public bool Excluido { get; set; } = false;
}
