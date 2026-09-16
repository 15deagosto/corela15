using System.Security.Claims;
using Corela15.Application.Comunicacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Corela15.Api.Hubs;

/// <summary>
/// Transporte en tiempo real de Comunicación interna — WebSocket propio
/// del core (ASP.NET Core SignalR, sin ningún servicio externo, mismo
/// criterio de "usar mi app" que motivó reemplazar el módulo de
/// WhatsApp). Autenticado con el mismo JWT de siempre (ver Program.cs,
/// `OnMessageReceived` lo extrae de la query string porque un WebSocket
/// no puede llevar el header Authorization en el handshake — mismo
/// patrón documentado oficialmente para SignalR + JWT).
///
/// Un usuario conectado se suma automáticamente al grupo real de cada
/// canal donde ya es miembro (`OnConnectedAsync`) — los mensajes nuevos
/// llegan solo a quien realmente pertenece a ese canal, nunca a todos los
/// conectados (autorización real, no solo transporte).
/// </summary>
[Authorize(Policy = "Menu:comunicacion-interna")]
public class ComunicacionHub(IComunicacionService service) : Hub
{
    public static string NombreGrupo(Guid idCanal) => $"canal-{idCanal}";

    public override async Task OnConnectedAsync()
    {
        var idUsuario = ObtenerIdUsuario();
        var canales = await service.ListarIdsCanalesDelUsuarioAsync(idUsuario);
        foreach (var idCanal in canales)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, NombreGrupo(idCanal));
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// El cliente lo invoca justo después de crear/unirse a un canal
    /// nuevo mientras ya está conectado — sin esto, tendría que
    /// reconectar para empezar a recibir mensajes de ese canal en vivo.
    /// Valida membresía real antes de sumarlo al grupo, nunca confía en
    /// lo que el cliente dice ser miembro.
    /// </summary>
    public async Task UnirseACanal(Guid idCanal)
    {
        var idUsuario = ObtenerIdUsuario();
        var canales = await service.ListarIdsCanalesDelUsuarioAsync(idUsuario);
        if (canales.Contains(idCanal))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, NombreGrupo(idCanal));
        }
    }

    private Guid ObtenerIdUsuario()
    {
        var valor = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;
        return Guid.Parse(valor!);
    }
}
