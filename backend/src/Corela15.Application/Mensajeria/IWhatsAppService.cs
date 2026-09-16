using Corela15.Application.Common;

namespace Corela15.Application.Mensajeria;

/// <summary>
/// Envío saliente de WhatsApp real vía WhatsApp Business Platform (Meta
/// Cloud API) — pensado para el caso real de una cajera sin acceso a
/// WhatsApp que necesita avisarle algo puntual a un socio (confirmar un
/// dato, pedir que se acerque, recordar un pago) usando un número real de
/// la cooperativa. Ver <see cref="Corela15.Domain.Mensajeria.MensajeWhatsapp"/>
/// para la explicación completa de por qué el texto libre viaja como
/// variable de una plantilla pre-aprobada, y la limitación real de
/// "solo envío" (sin bandeja de respuestas en esta ronda).
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Normaliza el número (Ecuador, 593 por defecto si no trae código de
    /// país), llama a <see cref="IWhatsAppCloudApiClient"/>, y SIEMPRE deja
    /// constancia en la bitácora — éxito o fallo — antes de decidir si
    /// lanza <see cref="WhatsAppEnvioFallidoException"/> (mismo criterio
    /// que AuthService.LoginAsync con accion_ingreso_usuario: el intento
    /// se audita siempre, no solo cuando sale bien).
    /// </summary>
    Task<MensajeWhatsappDto> EnviarAsync(EnviarMensajeWhatsappRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MensajeWhatsappDto>> ListarAsync(ListarMensajesWhatsappFiltro filtro, CancellationToken cancellationToken = default);
}

public record EnviarMensajeWhatsappRequest(
    string NumeroDestino,
    Guid? IdPersonaDestino,
    string Texto,
    int IdAgencia,
    string EnviadoPor);

public record ListarMensajesWhatsappFiltro(int? Dias);

public record MensajeWhatsappDto(
    Guid Id,
    string NumeroDestino,
    Guid? IdPersonaDestino,
    string? NombrePersonaDestino,
    string Texto,
    string Estado,
    string? IdMensajeExterno,
    string? DetalleError,
    string Agencia,
    string EnviadoPor,
    DateTimeOffset CreadoEn);

public class NumeroWhatsappInvalidoException(string numero)
    : SolicitudInvalidaException($"El número '{numero}' no es un número de WhatsApp válido (debe tener entre 9 y 15 dígitos, con o sin código de país)");

public class TextoMensajeWhatsappInvalidoException()
    : SolicitudInvalidaException("El mensaje no puede estar vacío ni superar los 1000 caracteres");

public class PersonaDestinoWhatsappInvalidaException(Guid idPersona)
    : ReglaDeNegocioException($"La persona {idPersona} no existe");

public class AgenciaInvalidaParaWhatsappException(int idAgencia)
    : ReglaDeNegocioException($"La agencia {idAgencia} no existe o no está activa");

/// <summary>
/// El módulo no tiene credenciales reales de Meta cargadas todavía
/// (`WhatsApp__PhoneNumberId`/`WhatsApp__AccessToken` en el entorno) —
/// mensaje explícito para no confundir esto con un error transitorio de
/// red.
/// </summary>
public class WhatsAppNoConfiguradoException()
    : ReglaDeNegocioException("El módulo de WhatsApp todavía no está conectado a Meta (faltan las credenciales reales) — contactar a TI antes de usarlo");

/// <summary>Meta rechazó o no pudo entregar el mensaje — el detalle real que trae Meta, nunca un genérico.</summary>
public class WhatsAppEnvioFallidoException(string detalle)
    : ReglaDeNegocioException($"WhatsApp no pudo enviar el mensaje: {detalle}");
