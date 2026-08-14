namespace Corela15.Domain.Seguridad;

/// <summary>
/// Una fila por JWT emitido (Id = claim "jti" del token) — lo que hace
/// posible revocar un token antes de que expire solo, algo que un JWT
/// puro no permite por diseño (es válido hasta su fecha de expiración,
/// sin importar qué pase con la cuenta después de emitirlo). Sin esto, un
/// usuario deshabilitado o un token filtrado seguían siendo válidos hasta
/// las 8h de `Jwt__ExpiryMinutes` — gap real documentado desde la
/// implementación de autenticación (ver CLAUDE.md, sección
/// "Revocación de tokens").
/// </summary>
public class SesionUsuario
{
    public Guid Id { get; set; }

    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTimeOffset EmitidaEn { get; set; }
    public DateTimeOffset ExpiraEn { get; set; }
    public string? DireccionIp { get; set; }

    public bool Revocada { get; set; }
    public DateTimeOffset? RevocadaEn { get; set; }
    public string? RevocadaPor { get; set; }
}
