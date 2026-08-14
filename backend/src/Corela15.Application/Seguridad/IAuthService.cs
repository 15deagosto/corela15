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

    /// <summary>Revoca la sesión (token) actual — logout real, no solo del lado del cliente.</summary>
    Task LogoutAsync(Guid idSesion, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>Historial de sesiones (tokens emitidos) de un usuario, más recientes primero.</summary>
    Task<IReadOnlyList<SesionUsuarioResult>> ListarSesionesAsync(Guid idUsuario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca TODAS las sesiones activas de un usuario de una sola vez —
    /// el caso real de "cerrar todos los accesos" cuando alguien deja la
    /// cooperativa o se sospecha de una cuenta comprometida, sin esperar a
    /// que cada token expire solo.
    /// </summary>
    Task RevocarTodasLasSesionesAsync(Guid idUsuario, string registradoPor, CancellationToken cancellationToken = default);
}

public class CredencialesInvalidasException() : NoAutenticadoException("Usuario o contraseña incorrectos");

public class UsuarioNoHabilitadoException()
    : NoAutenticadoException("El usuario está inactivo, bloqueado, o no puede ingresar al sistema");
