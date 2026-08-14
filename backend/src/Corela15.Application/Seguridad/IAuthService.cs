using Corela15.Application.Common;

namespace Corela15.Application.Seguridad;

public interface IAuthService
{
    /// <summary>
    /// Valida usuario/contraseña (BCrypt), registra el intento en
    /// `seguridad.accion_ingreso_usuario` (exitoso o no — se audita siempre,
    /// no solo el éxito), y si es válido emite un JWT con los roles y los
    /// códigos de menú permitidos como claims.
    /// </summary>
    Task<LoginResult> LoginAsync(LoginRequest request, string? direccionIp, CancellationToken cancellationToken = default);
}

public class CredencialesInvalidasException() : NoAutenticadoException("Usuario o contraseña incorrectos");

public class UsuarioNoHabilitadoException()
    : NoAutenticadoException("El usuario está inactivo, bloqueado, o no puede ingresar al sistema");
