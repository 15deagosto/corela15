namespace Corela15.Application.Mensajeria;

/// <summary>
/// Puerto hacia WhatsApp Business Platform (Meta Cloud API) — la
/// implementación real (<c>WhatsAppCloudApiClient</c>, Infrastructure)
/// hace el HTTP real contra Meta; esta interfaz vive en Application para
/// que <c>WhatsAppService</c> no dependa de detalles de transporte.
/// </summary>
public interface IWhatsAppCloudApiClient
{
    /// <summary>
    /// Envía la plantilla de utilidad genérica ya aprobada con `texto` como
    /// su única variable — nunca un mensaje de texto libre directo (Meta lo
    /// rechazaría fuera de la ventana de 24h). `numero` ya viene normalizado
    /// en formato E.164 sin "+".
    /// </summary>
    Task<ResultadoEnvioWhatsapp> EnviarPlantillaAsync(string numero, string texto, CancellationToken cancellationToken = default);
}

public record ResultadoEnvioWhatsapp(bool Exitoso, string? IdMensajeExterno, string? DetalleError);
