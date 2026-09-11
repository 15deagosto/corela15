using Corela15.Application.MesaServicio;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record CrearTicketBody(
    string Titulo, string Descripcion, string CodigoCategoria, string CodigoPrioridad,
    int IdAgencia, Guid? IdUsuarioAsignado, bool EsProactivo = false);

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
        // que él mismo reportó).
        //
        // Corrección real de seguridad: `misTickets` era un filtro opcional
        // controlado por el cliente -- cualquier usuario autenticado (el
        // menú `mesa-servicio` es universal, ver AuthService.LoginAsync)
        // podía llamar a este endpoint sin ese parámetro y ver los tickets
        // de TODA la cooperativa, no solo los propios. Un usuario sin el
        // permiso real de agente (`Menu:mesa-servicio-agente`) ahora queda
        // forzado a `creadoPor = él mismo` sin excepción, sin importar qué
        // pida el query string -- solo un agente puede pedir la lista
        // completa.
        var esAgente = User.HasClaim("menu", "mesa-servicio-agente");

        Guid? idUsuarioAsignado = null;
        if (esAgente && soloMios)
        {
            var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            if (Guid.TryParse(idUsuario, out var parsed)) idUsuarioAsignado = parsed;
        }
        var creadoPor = esAgente ? (misTickets ? User.Identity!.Name! : null) : User.Identity!.Name!;

        var resultado = await service.ListarAsync(new ListarTicketsFiltro(codigoEstado, idUsuarioAsignado, creadoPor), cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Mismo criterio de seguridad que <see cref="Listar"/>: un usuario sin
    /// permiso de agente solo puede ver el detalle de un ticket que él
    /// mismo reportó -- antes cualquiera con el ID (Guid) de un ticket
    /// ajeno podía leer sus comentarios/bitácora sin restricción.
    /// </summary>
    [HttpGet("tickets/{id:guid}")]
    public async Task<ActionResult<TicketDetalleDto>> Obtener(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await service.ObtenerAsync(id, cancellationToken);
        var esAgente = User.HasClaim("menu", "mesa-servicio-agente");
        if (!esAgente && resultado.Ticket.CreadoPor != User.Identity!.Name)
            return NotFound();
        return Ok(resultado);
    }

    [HttpPost("tickets")]
    public async Task<ActionResult<TicketDto>> Crear(CrearTicketBody body, CancellationToken cancellationToken)
    {
        // "Proactivo" (mantenimiento que TI inicia por su cuenta, sin que
        // nadie lo pida) nunca se confía del cliente -- solo un agente real
        // puede marcarlo así; un usuario reportando un problema real nunca
        // es "proactivo" por definición, sin importar qué mande el body.
        var esAgente = User.HasClaim("menu", "mesa-servicio-agente");
        var resultado = await service.CrearAsync(new CrearTicketRequest(
            body.Titulo, body.Descripcion, body.CodigoCategoria, body.CodigoPrioridad,
            body.IdAgencia, body.IdUsuarioAsignado, User.Identity!.Name!,
            esAgente && body.EsProactivo), cancellationToken);
        return Created($"/api/mesa-servicio/tickets/{resultado.Id}", resultado);
    }

    /// <summary>Mismo criterio de seguridad que <see cref="Listar"/>/<see cref="Obtener"/>: un no-agente solo puede comentar en un ticket que él mismo reportó.</summary>
    [HttpPost("tickets/{id:guid}/comentarios")]
    public async Task<ActionResult<TicketDto>> Comentar(Guid id, ComentarTicketBody body, CancellationToken cancellationToken)
    {
        var esAgente = User.HasClaim("menu", "mesa-servicio-agente");
        if (!esAgente)
        {
            var creadoPor = await db.Tickets.Where(t => t.Id == id).Select(t => t.CreadoPor).FirstOrDefaultAsync(cancellationToken);
            if (creadoPor != User.Identity!.Name) return NotFound();
        }
        return Ok(await service.ComentarAsync(id, new ComentarTicketRequest(body.Comentario, User.Identity!.Name!), cancellationToken));
    }

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
