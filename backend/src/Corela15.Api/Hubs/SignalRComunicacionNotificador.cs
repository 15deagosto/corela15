using System.Security.Claims;
using Corela15.Application.Comunicacion;
using Microsoft.AspNetCore.SignalR;

namespace Corela15.Api.Hubs;

/// <summary>
/// Implementación real de <see cref="IComunicacionNotificador"/> — vive
/// en Api (no en Infrastructure) porque necesita conocer el tipo del Hub;
/// Application/Infrastructure nunca referencian SignalR directamente.
/// </summary>
public class SignalRComunicacionNotificador(IHubContext<ComunicacionHub> hubContext) : IComunicacionNotificador
{
    public Task NotificarMensajeNuevoAsync(Guid idCanal, MensajeDto mensaje, CancellationToken cancellationToken = default) =>
        hubContext.Clients.Group(ComunicacionHub.NombreGrupo(idCanal)).SendAsync("mensajeNuevo", mensaje, cancellationToken);

    public Task NotificarAgregadoACanalAsync(Guid idUsuario, CanalDto canal, CancellationToken cancellationToken = default) =>
        hubContext.Clients.User(idUsuario.ToString()).SendAsync("agregadoACanal", canal, cancellationToken);
}

/// <summary>
/// Resuelve el "user id" real de SignalR (usado por `Clients.User(...)`)
/// desde el mismo claim que el resto del backend ya usa para identificar
/// al usuario del JWT (`sub`, con `ClaimTypes.NameIdentifier` como
/// alternativa) — sin esto, `Clients.User` no encontraría a nadie porque
/// el proveedor por defecto de SignalR solo mira `ClaimTypes.NameIdentifier`.
/// </summary>
public class ClaimsUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? connection.User.FindFirst("sub")?.Value;
}
