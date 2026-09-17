using Corela15.Application.Common;

namespace Corela15.Application.Comunicacion;

/// <summary>
/// Comunicación interna real — canales de grupo (ej. "Cajas", "Balcón de
/// Servicio", o cualquiera que un usuario cree) y mensajes directos 1:1,
/// todo dentro del propio core (sin depender de WhatsApp ni de ningún
/// servicio externo — decisión explícita del usuario tras evaluar el
/// módulo de WhatsApp: "no quiero gastar y mejor quiero usar mi app").
/// El envío en tiempo real (push a quien tiene el canal abierto) lo hace
/// <see cref="IComunicacionNotificador"/> — esta interfaz nunca conoce
/// SignalR directamente, solo dispara la notificación después de guardar.
/// </summary>
public interface IComunicacionService
{
    Task<CanalDto> CrearCanalAsync(CrearCanalRequest request, CancellationToken cancellationToken = default);

    /// <summary>Busca la conversación directa real entre dos usuarios (única por par, ver Canal.ClaveDirecta) o la crea si es la primera vez que se escriben.</summary>
    Task<CanalDto> ObtenerOCrearDirectoAsync(Guid idUsuarioA, Guid idUsuarioB, CancellationToken cancellationToken = default);

    Task AgregarMiembroAsync(Guid idCanal, Guid idUsuarioNuevo, string ejecutadoPor, CancellationToken cancellationToken = default);

    Task SalirDelCanalAsync(Guid idCanal, Guid idUsuario, CancellationToken cancellationToken = default);

    /// <summary>Un mensaje siempre lleva texto o adjunto (nunca ninguno de los dos) — `archivo` null si es un mensaje de solo texto.</summary>
    Task<MensajeDto> EnviarMensajeAsync(Guid idCanal, Guid idUsuarioRemitente, string? texto, ArchivoAdjuntoEntrada? archivo, CancellationToken cancellationToken = default);

    /// <summary>Valida que el usuario sea miembro real del canal del mensaje antes de abrir el adjunto — mismo criterio "deny-by-default" que el resto del core, nunca se confía en el Id del mensaje solo.</summary>
    Task<DescargaAdjuntoResult> DescargarAdjuntoAsync(Guid idMensaje, Guid idUsuario, CancellationToken cancellationToken = default);

    /// <summary>Canales (grupo y directos) donde el usuario es miembro activo, con el último mensaje y cuántos no leyó — ordenados por actividad reciente.</summary>
    Task<IReadOnlyList<CanalListItemDto>> ListarCanalesAsync(Guid idUsuario, CancellationToken cancellationToken = default);

    /// <summary>Historial paginado hacia atrás — `antesDe` es el Id del mensaje más antiguo ya cargado, null para traer los más recientes.</summary>
    Task<IReadOnlyList<MensajeDto>> ListarMensajesAsync(Guid idCanal, Guid idUsuario, Guid? antesDe, CancellationToken cancellationToken = default);

    Task MarcarLeidoAsync(Guid idCanal, Guid idUsuario, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UsuarioParaChatDto>> BuscarUsuariosAsync(string? q, Guid idUsuarioExcluir, CancellationToken cancellationToken = default);

    /// <summary>Ids de canal donde el usuario es miembro activo — usado por el Hub al conectar para unirlo a sus grupos reales de SignalR.</summary>
    Task<IReadOnlyList<Guid>> ListarIdsCanalesDelUsuarioAsync(Guid idUsuario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Todos los canales de grupo activos (nunca directos), marcando cuáles
    /// ya integra el usuario — autoservicio real: un canal creado por otra
    /// persona (ej. uno nuevo, o uno sembrado por migración como "Cajas")
    /// se descubre y se une con un clic, sin depender de que alguien lo
    /// agregue a mano.
    /// </summary>
    Task<IReadOnlyList<CanalDescubribleDto>> ListarCanalesDescubriblesAsync(Guid idUsuario, CancellationToken cancellationToken = default);
}

/// <summary>
/// Puerto hacia el transporte en tiempo real — la implementación real
/// (`Corela15.Api.Hubs.SignalRComunicacionNotificador`) vive en Api porque
/// necesita conocer el tipo del Hub; Application/Infrastructure nunca
/// referencian SignalR directamente.
/// </summary>
public interface IComunicacionNotificador
{
    Task NotificarMensajeNuevoAsync(Guid idCanal, MensajeDto mensaje, CancellationToken cancellationToken = default);

    /// <summary>Avisa al usuario recién agregado (si tiene una conexión activa) que se sume al grupo real del canal, y le manda el canal completo para que aparezca en su lista sin recargar.</summary>
    Task NotificarAgregadoACanalAsync(Guid idUsuario, CanalDto canal, CancellationToken cancellationToken = default);
}

public record CrearCanalRequest(string Nombre, string? Descripcion, IReadOnlyList<Guid> IdsMiembrosIniciales, Guid CreadoPor);

public record CanalDto(Guid Id, string? Nombre, string? Descripcion, bool EsDirecto, DateTimeOffset CreadoEn);

public record CanalListItemDto(
    Guid Id, string Nombre, bool EsDirecto,
    string? UltimoMensajeTexto, string? UltimoMensajeAutor, DateTimeOffset? UltimoMensajeFecha,
    int NoLeidos);

public record MensajeDto(
    Guid Id, Guid IdCanal, Guid IdUsuarioRemitente, string NombreRemitente, string? Texto, DateTimeOffset CreadoEn,
    string? NombreArchivoAdjunto, string? ContentTypeAdjunto, long? TamanoBytesAdjunto);

public record ArchivoAdjuntoEntrada(Stream Contenido, string NombreOriginal, string ContentType, long TamanoBytes);

public record DescargaAdjuntoResult(Stream Contenido, string NombreArchivo, string ContentType);

public record UsuarioParaChatDto(Guid Id, string Nombre);

public record CanalDescubribleDto(Guid Id, string Nombre, string? Descripcion, bool YaSoyMiembro);

public class CanalNoExisteException(Guid idCanal) : ReglaDeNegocioException($"El canal {idCanal} no existe");

public class UsuarioInvalidoParaChatException(Guid idUsuario) : ReglaDeNegocioException($"El usuario {idUsuario} no existe o no está activo");

public class NoEsMiembroDelCanalException() : ReglaDeNegocioException("No sos miembro de este canal");

public class NombreCanalInvalidoException() : SolicitudInvalidaException("El nombre del canal no puede estar vacío");

public class TextoMensajeInvalidoException() : SolicitudInvalidaException("El texto del mensaje no puede superar los 2000 caracteres");

public class MensajeVacioException() : SolicitudInvalidaException("El mensaje necesita texto o un archivo adjunto");

public class NoSePuedeSalirDeConversacionDirectaException() : ReglaDeNegocioException("Esta operación solo aplica a canales de grupo, no a una conversación directa");

public class ExtensionAdjuntoNoPermitidaException(string extension)
    : SolicitudInvalidaException($"Extensión '{extension}' no permitida — solo imágenes (jpg, jpeg, png, gif, webp) y documentos (pdf, doc, docx, xls, xlsx, txt, ppt, pptx, csv)");

public class ArchivoAdjuntoDemasiadoGrandeException(int maxMb) : SolicitudInvalidaException($"El adjunto no puede pesar más de {maxMb}MB");

public class MensajeSinAdjuntoException() : ReglaDeNegocioException("Este mensaje no tiene ningún archivo adjunto");
