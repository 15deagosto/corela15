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

    /// <summary>
    /// Cambio de clave personal (real, del propio usuario autenticado) —
    /// verifica la contraseña actual, valida la nueva (mínimo 8
    /// caracteres, distinta de la actual), la hashea con BCrypt, y
    /// registra la acción en `seguridad.accion_usuario` (código
    /// `CambioClavePersonal`, mismo catálogo real que Softbank). Por
    /// seguridad, revoca todas las DEMÁS sesiones activas del usuario —
    /// un cambio de clave real (ej. porque se sospechaba de la cuenta) no
    /// debería dejar sesiones viejas con la clave anterior todavía
    /// vigentes en otro dispositivo.
    /// </summary>
    Task CambiarClaveAsync(Guid idUsuario, Guid idSesionActual, CambiarClaveRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bloquea/desbloquea el acceso de un usuario (`Usuario.TieneBloqueo`,
    /// ya validado en LoginAsync pero sin pantalla para activarlo hasta
    /// ahora). Al bloquear, revoca también todas sus sesiones activas —
    /// bloquear a alguien y dejarlo con una sesión vigente sería un
    /// bloqueo sin efecto real.
    /// </summary>
    Task ConfigurarBloqueoAsync(Guid idUsuario, bool bloquear, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Alta de usuario nuevo — cierra el gap real de que este core no tenía
    /// ninguna forma de crear un usuario fuera de una migración de seed.
    /// Contraseña inicial hasheada con BCrypt (mismo criterio que
    /// CambiarClaveAsync, mínimo 8 caracteres). Registra la acción en
    /// `seguridad.accion_usuario` con el código real `CreacionSujeto`
    /// (verificado contra SEGURIDAD.ACCION de Softbank).
    /// </summary>
    Task<Guid> CrearUsuarioAsync(CrearUsuarioRequest request, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edita agencia, vínculo con Persona, y si puede ingresar al sistema.
    /// Registra la acción con el código real `CambioSujeto`.
    /// </summary>
    Task ActualizarUsuarioAsync(Guid idUsuario, ActualizarUsuarioRequest request, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reseteo de contraseña por un administrador (distinto de
    /// CambiarClaveAsync, que es self-service) — revoca todas las sesiones
    /// activas del usuario afectado, mismo criterio que ConfigurarBloqueoAsync.
    /// No existe en el catálogo real un código específico para "reseteo de
    /// un solo usuario por un admin" — se usa el más cercano de los 6 reales,
    /// `CambioClaveEnLote` (aunque acá se aplica a un usuario a la vez, no en
    /// lote), documentado a propósito en vez de inventar un código nuevo.
    /// </summary>
    Task ResetearClaveAsync(Guid idUsuario, string claveNueva, string registradoPor, CancellationToken cancellationToken = default);
}

public record CrearUsuarioRequest(
    string NombreUsuario, string ContrasenaInicial, int IdAgencia, Guid? IdPersona,
    bool UsaDispositivoMovil = false, bool PermiteRiesgoOperativo = false,
    bool PermiteConsultaEmpleados = false, bool ValidaIp = false, bool CambiaClave = true, int? DiasCambioClave = null);

public record ActualizarUsuarioRequest(
    int IdAgencia, Guid? IdPersona, bool PuedeIngresarSistema,
    bool UsaDispositivoMovil, bool PermiteRiesgoOperativo,
    bool PermiteConsultaEmpleados, bool ValidaIp, bool CambiaClave, int? DiasCambioClave);

public class NombreUsuarioDuplicadoException(string nombreUsuario)
    : ReglaDeNegocioException($"Ya existe un usuario con el nombre '{nombreUsuario}'");

public class AgenciaInvalidaException(int idAgencia)
    : ReglaDeNegocioException($"La agencia {idAgencia} no existe o no está activa");

public class CredencialesInvalidasException() : NoAutenticadoException("Usuario o contraseña incorrectos");

public class UsuarioNoHabilitadoException()
    : NoAutenticadoException("El usuario está inactivo, bloqueado, o no puede ingresar al sistema");

public class ContrasenaActualIncorrectaException()
    : ReglaDeNegocioException("La contraseña actual no es correcta");

public class ContrasenaNuevaInvalidaException(string motivo)
    : SolicitudInvalidaException(motivo);

public class UsuarioNoExisteException(Guid idUsuario)
    : ReglaDeNegocioException($"El usuario {idUsuario} no existe");

public class FueraDeHorarioException()
    : NoAutenticadoException("El usuario no tiene autorizado el ingreso en este horario");
