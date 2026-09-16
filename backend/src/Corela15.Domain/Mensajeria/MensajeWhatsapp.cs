using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.Mensajeria;

/// <summary>
/// Estado real del envío contra WhatsApp Business Platform (Meta Cloud
/// API) — nunca "en cola"/"pendiente": la llamada a Meta es síncrona
/// (ver WhatsAppCloudApiClient), así que el registro nace ya resuelto.
/// </summary>
public enum EstadoEnvioWhatsapp
{
    Enviado = 1,
    Fallido = 2,
}

/// <summary>
/// Bitácora real, inmutable, de cada mensaje de WhatsApp enviado desde el
/// core — nunca se borra, mismo criterio de auditoría de todo el proyecto
/// (ver `accion_ingreso_usuario`/`sesion_usuario`). Un solo sentido: salida
/// únicamente. Meta exige que un negocio inicie el contacto con una
/// PLANTILLA pre-aprobada (no texto libre) salvo que el destinatario le
/// haya escrito primero en las últimas 24h — por eso `Texto` nunca se manda
/// como mensaje de texto libre a la API: viaja como la única variable de
/// una plantilla de utilidad genérica ya aprobada
/// (`WhatsAppCloudApiClient`/`WhatsApp:TemplateName`), lo que en la
/// práctica le da a la cajera un cuadro de texto libre real sin violar la
/// política de Meta.
///
/// Limitación real, documentada a propósito: este módulo es de SOLO
/// ENVÍO. Si el socio responde, esa respuesta cae en la app de WhatsApp
/// normal del número usado (la cajera no tiene acceso a esa app) — no hay
/// bandeja de entrada ni webhook de respuestas en esta ronda. Sirve para
/// avisos salientes (confirmar un dato, pedir que se acerque, recordar un
/// pago), no para sostener una conversación de ida y vuelta.
/// </summary>
public class MensajeWhatsapp
{
    public Guid Id { get; set; }

    /// <summary>Formato E.164 sin "+" (ej. 593987654321) — normalizado en el servicio antes de guardar, nunca como lo tipeó la cajera.</summary>
    public string NumeroDestino { get; set; } = string.Empty;

    /// <summary>Socio/persona real vinculada al número, cuando se eligió desde el buscador — null si se tipeó un número suelto (ej. un proveedor sin registrar).</summary>
    public Guid? IdPersonaDestino { get; set; }
    public Persona? PersonaDestino { get; set; }

    public string Texto { get; set; } = string.Empty;

    public EstadoEnvioWhatsapp Estado { get; set; }

    /// <summary>wamid real que devuelve Meta cuando el envío fue exitoso.</summary>
    public string? IdMensajeExterno { get; set; }

    /// <summary>Código/mensaje de error real devuelto por Meta cuando el envío falló — nunca un genérico, para que la cajera (o TI) sepa qué pasó.</summary>
    public string? DetalleError { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public string EnviadoPor { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }
}
