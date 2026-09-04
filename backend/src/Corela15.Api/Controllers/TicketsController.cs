using Corela15.Application.MesaServicio;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record CrearTicketBody(
    string Titulo, string Descripcion, string CodigoCategoria, string CodigoPrioridad,
    int IdAgencia, Guid? IdUsuarioAsignado);

public record ComentarTicketBody(string Comentario);

public record CambiarEstadoTicketBody(string CodigoEstado, string? Comentario);

public record AsignarTicketBody(Guid? IdUsuarioAsignado);

public record CalificarTicketBody(int Calificacion, string? Comentario);

/// <summary>
/// Mesa de servicio — control de incidencias real exigido por la SEPS.
/// A diferencia de todo otro controller del proyecto, el permiso de menú
/// (`Menu:mesa-servicio`) lo tiene TODO usuario autenticado por diseño
/// (ver AuthService.LoginAsync) — no hace falta que ningún rol lo otorgue
/// explícitamente vía rol_menu. Eso alcanza para reportar, comentar y
/// calificar. Trabajar un ticket (tomarlo, reasignarlo, cambiar su estado)
/// exige además `Menu:mesa-servicio-agente` — ese sí es un menú normal,
/// otorgado explícitamente desde Configuración → Roles → Permisos a quien
/// realmente resuelve incidencias. Los dos `[Authorize]` (clase + método)
/// se combinan con AND, así que esas acciones exigen ambos permisos.
/// </summary>
[ApiController]
[Route("api/mesa-servicio")]
[Authorize(Policy = "Menu:mesa-servicio")]
public class TicketsController(ITicketService service, Corela15DbContext db) : ControllerBase
{
    [HttpGet("tickets")]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> Listar(
        [FromQuery] string? codigoEstado, [FromQuery] bool soloMios, [FromQuery] bool misTickets,
        CancellationToken cancellationToken)
    {
        // Dos filtros reales distintos, nunca confundidos: "asignados a mí"
        // (soloMios — un agente ve lo que le toca resolver) vs "creados por
        // mí" (misTickets — cualquier usuario, sea agente o no, ve solo lo
        // que él mismo reportó). Antes un usuario sin permiso de agente no
        // tenía forma real de filtrar a "mis reportes" — veía la lista
        // completa de toda la cooperativa mezclada.
        Guid? idUsuarioAsignado = null;
        if (soloMios)
        {
            var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            if (Guid.TryParse(idUsuario, out var parsed)) idUsuarioAsignado = parsed;
        }
        var creadoPor = misTickets ? User.Identity!.Name! : null;

        var resultado = await service.ListarAsync(new ListarTicketsFiltro(codigoEstado, idUsuarioAsignado, creadoPor), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("tickets/{id:guid}")]
    public async Task<ActionResult<TicketDetalleDto>> Obtener(Guid id, CancellationToken cancellationToken)
        => Ok(await service.ObtenerAsync(id, cancellationToken));

    [HttpPost("tickets")]
    public async Task<ActionResult<TicketDto>> Crear(CrearTicketBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.CrearAsync(new CrearTicketRequest(
            body.Titulo, body.Descripcion, body.CodigoCategoria, body.CodigoPrioridad,
            body.IdAgencia, body.IdUsuarioAsignado, User.Identity!.Name!), cancellationToken);
        return Created($"/api/mesa-servicio/tickets/{resultado.Id}", resultado);
    }

    [HttpPost("tickets/{id:guid}/comentarios")]
    public async Task<ActionResult<TicketDto>> Comentar(Guid id, ComentarTicketBody body, CancellationToken cancellationToken)
        => Ok(await service.ComentarAsync(id, new ComentarTicketRequest(body.Comentario, User.Identity!.Name!), cancellationToken));

    [HttpPost("tickets/{id:guid}/estado")]
    [Authorize(Policy = "Menu:mesa-servicio-agente")]
    public async Task<ActionResult<TicketDto>> CambiarEstado(Guid id, CambiarEstadoTicketBody body, CancellationToken cancellationToken)
        => Ok(await service.CambiarEstadoAsync(id, new CambiarEstadoTicketRequest(body.CodigoEstado, body.Comentario, User.Identity!.Name!), cancellationToken));

    [HttpPost("tickets/{id:guid}/asignar")]
    [Authorize(Policy = "Menu:mesa-servicio-agente")]
    public async Task<ActionResult<TicketDto>> Asignar(Guid id, AsignarTicketBody body, CancellationToken cancellationToken)
        => Ok(await service.AsignarAsync(id, new AsignarTicketRequest(body.IdUsuarioAsignado, User.Identity!.Name!), cancellationToken));

    /// <summary>Autoasignación real — el agente se pone el ticket a sí mismo, sin elegir de una lista.</summary>
    [HttpPost("tickets/{id:guid}/tomar")]
    [Authorize(Policy = "Menu:mesa-servicio-agente")]
    public async Task<ActionResult<TicketDto>> Tomar(Guid id, CancellationToken cancellationToken)
    {
        var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(idUsuario, out var idAgente)) return Unauthorized();
        return Ok(await service.TomarAsync(id, new TomarTicketRequest(idAgente, User.Identity!.Name!), cancellationToken));
    }

    /// <summary>Solo el reportante original puede calificar, y solo con el ticket Resuelto/Cerrado — validado en el servicio, no aquí.</summary>
    [HttpPost("tickets/{id:guid}/calificar")]
    public async Task<ActionResult<TicketDto>> Calificar(Guid id, CalificarTicketBody body, CancellationToken cancellationToken)
        => Ok(await service.CalificarAsync(id, new CalificarTicketRequest(body.Calificacion, body.Comentario, User.Identity!.Name!), cancellationToken));

    [HttpGet("categorias")]
    public async Task<ActionResult<IReadOnlyList<object>>> Categorias(CancellationToken cancellationToken)
        => Ok(await db.CategoriasIncidencia.Where(c => c.Activo).OrderBy(c => c.Nombre)
            .Select(c => new { c.Codigo, c.Nombre }).ToListAsync(cancellationToken));

    [HttpGet("prioridades")]
    public async Task<ActionResult<IReadOnlyList<object>>> Prioridades(CancellationToken cancellationToken)
        => Ok(await db.PrioridadesTicket.Where(p => p.Activo).OrderBy(p => p.HorasSla)
            .Select(p => new { p.Codigo, p.Nombre, p.HorasSla }).ToListAsync(cancellationToken));

    [HttpGet("estados")]
    public async Task<ActionResult<IReadOnlyList<object>>> Estados(CancellationToken cancellationToken)
        => Ok(await db.EstadosTicket.Where(e => e.Activo)
            .Select(e => new { e.Codigo, e.Nombre }).ToListAsync(cancellationToken));

    /// <summary>Solo usuarios con permiso real de agente (Menu:mesa-servicio-agente) — la única lista válida para asignar/reasignar un ticket.</summary>
    [HttpGet("agentes")]
    public async Task<ActionResult<IReadOnlyList<AgenteDto>>> Agentes(CancellationToken cancellationToken)
        => Ok(await service.ListarAgentesAsync(cancellationToken));
}
