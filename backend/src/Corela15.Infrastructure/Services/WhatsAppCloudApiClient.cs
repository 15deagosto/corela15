using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Corela15.Application.Mensajeria;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Cliente real de WhatsApp Business Platform (Meta Cloud API,
/// <c>https://graph.facebook.com</c>) — implementación concreta de
/// <see cref="IWhatsAppCloudApiClient"/>. Credenciales SIEMPRE por
/// variable de entorno (mismo criterio que Jwt__Secret/Softbank__ConnectionStringRo
/// en el resto del proyecto — ver .env.core), nunca hardcodeadas:
///
/// - <c>WhatsApp__PhoneNumberId</c>: el "Phone Number ID" real que Meta
///   asigna al número de la cooperativa dentro de Meta Business Manager
///   (NO es el número de teléfono en sí).
/// - <c>WhatsApp__AccessToken</c>: token de acceso real (de sistema, de
///   larga duración — un token de usuario de prueba de Meta expira en
///   24h y rompería el módulo al día siguiente).
/// - <c>WhatsApp__TemplateName</c> (opcional, default "aviso_cooperativa"):
///   el nombre exacto de la plantilla de utilidad genérica de UN solo
///   parámetro de texto ya aprobada por Meta — ver MensajeWhatsapp.cs
///   para la explicación de por qué se usa una plantilla en vez de texto
///   libre directo.
/// - <c>WhatsApp__TemplateLanguage</c> (opcional, default "es").
/// - <c>WhatsApp__ApiVersion</c> (opcional, default "v21.0").
///
/// Mientras esas credenciales no existan (cuenta de Meta Business Manager
/// todavía sin verificar), el cliente rechaza con
/// <see cref="WhatsAppNoConfiguradoException"/> en vez de fallar con un
/// error de red confuso — el módulo queda "dormido" a propósito hasta que
/// TI cargue las credenciales reales.
/// </summary>
public class WhatsAppCloudApiClient(IHttpClientFactory httpClientFactory) : IWhatsAppCloudApiClient
{
    public async Task<ResultadoEnvioWhatsapp> EnviarPlantillaAsync(
        string numero, string texto, CancellationToken cancellationToken = default)
    {
        var phoneNumberId = Environment.GetEnvironmentVariable("WhatsApp__PhoneNumberId");
        var accessToken = Environment.GetEnvironmentVariable("WhatsApp__AccessToken");
        if (string.IsNullOrWhiteSpace(phoneNumberId) || string.IsNullOrWhiteSpace(accessToken))
        {
            throw new WhatsAppNoConfiguradoException();
        }

        var apiVersion = Environment.GetEnvironmentVariable("WhatsApp__ApiVersion") ?? "v21.0";
        var templateName = Environment.GetEnvironmentVariable("WhatsApp__TemplateName") ?? "aviso_cooperativa";
        var templateLanguage = Environment.GetEnvironmentVariable("WhatsApp__TemplateLanguage") ?? "es";

        var http = httpClientFactory.CreateClient("whatsapp-cloud-api");
        http.BaseAddress = new Uri("https://graph.facebook.com/");
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = numero,
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = templateLanguage },
                components = new object[]
                {
                    new
                    {
                        type = "body",
                        parameters = new object[] { new { type = "text", text = texto } },
                    },
                },
            },
        };

        HttpResponseMessage respuesta;
        try
        {
            respuesta = await http.PostAsJsonAsync($"{apiVersion}/{phoneNumberId}/messages", payload, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Falla de red real (sin internet, DNS, timeout) — se trata
            // igual que un rechazo de Meta: se audita como fallido, nunca
            // tumba el request del usuario con un 500.
            return new ResultadoEnvioWhatsapp(false, null, $"No se pudo contactar a WhatsApp: {ex.Message}");
        }

        var cuerpo = await respuesta.Content.ReadAsStringAsync(cancellationToken);

        if (respuesta.IsSuccessStatusCode)
        {
            var ok = JsonSerializer.Deserialize<RespuestaExitosaMeta>(cuerpo, JsonOpciones);
            var wamid = ok?.Messages?.FirstOrDefault()?.Id;
            return new ResultadoEnvioWhatsapp(true, wamid, null);
        }

        var error = JsonSerializer.Deserialize<RespuestaErrorMeta>(cuerpo, JsonOpciones);
        var detalle = error?.Error?.Message ?? $"HTTP {(int)respuesta.StatusCode}";
        return new ResultadoEnvioWhatsapp(false, null, detalle);
    }

    private static readonly JsonSerializerOptions JsonOpciones = new(JsonSerializerDefaults.Web);

    private record RespuestaExitosaMeta([property: JsonPropertyName("messages")] List<MensajeMeta>? Messages);
    private record MensajeMeta([property: JsonPropertyName("id")] string Id);
    private record RespuestaErrorMeta([property: JsonPropertyName("error")] ErrorMeta? Error);
    private record ErrorMeta(
        [property: JsonPropertyName("message")] string? Message,
        [property: JsonPropertyName("code")] int? Code);
}
