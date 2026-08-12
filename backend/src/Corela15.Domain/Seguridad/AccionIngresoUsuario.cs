namespace Corela15.Domain.Seguridad;

/// <summary>
/// Log de inicios de sesión. Patrón bueno a copiar de Softbank (ver
/// 02-arquitectura-datos-40-modulos.md 1.3): a diferencia de LOGAUDITORIA
/// (declarada pero vacía siempre), esta tabla sí se llena desde el día uno.
/// </summary>
public class AccionIngresoUsuario
{
    public long Id { get; set; }
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public DateTimeOffset FechaHora { get; set; }
    public bool Exitoso { get; set; }
    public string? DireccionIp { get; set; }
    public string? Detalle { get; set; }
}
