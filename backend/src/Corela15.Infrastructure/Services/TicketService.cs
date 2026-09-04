using Corela15.Application.MesaServicio;
using Corela15.Domain.MesaServicio;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class TicketService(Corela15DbContext db) : ITicketService
{
    // "Resuelto" deliberadamente NO es final — el ticket puede reabrirse si
    // el usuario no queda conforme, o pasar a "Cerrado" tras confirmación.
    // Solo Cerrado/Cancelado bloquean cambios posteriores.
    private static readonly string[] EstadosFinales = ["CERRADO", "CANCELADO"];
    private static readonly string[] EstadosQueFijanCierre = ["RESUELTO", "CERRADO", "CANCELADO"];
    private static readonly string[] EstadosCalificables = ["RESUELTO", "CERRADO"];
    private const string CodigoMenuAgente = "mesa-servicio-agente";

    public async Task<TicketDto> CrearAsync(CrearTicketRequest request, CancellationToken cancellationToken = default)
    {
        var categoria = await db.CategoriasIncidencia
            .FirstOrDefaultAsync(c => c.Codigo == request.CodigoCategoria && c.Activo, cancellationToken)
            ?? throw new CategoriaIncidenciaInvalidaException(request.CodigoCategoria);

        var prioridad = await db.PrioridadesTicket
            .FirstOrDefaultAsync(p => p.Codigo == request.CodigoPrioridad && p.Activo, cancellationToken)
            ?? throw new PrioridadTicketInvalidaException(request.CodigoPrioridad);

        var agencia = await db.Agencias
            .FirstOrDefaultAsync(a => a.Id == request.IdAgencia && a.Activa, cancellationToken)
            ?? throw new AgenciaInvalidaParaTicketException(request.IdAgencia);

        if (request.IdUsuarioAsignado is not null)
            await ValidarEsAgenteAsync(request.IdUsuarioAsignado.Value, cancellationToken);

        var estadoInicial = await db.EstadosTicket.FirstOrDefaultAsync(e => e.Codigo == "ABIERTO" && e.Activo, cancellationToken)
            ?? throw new EstadoTicketInvalidoException("ABIERTO");

        var total = await db.Tickets.CountAsync(cancellationToken);
        var numero = $"TCK-{DateTimeOffset.UtcNow.Year}-{(total + 1).ToString().PadLeft(6, '0')}";
        var ahora = DateTimeOffset.UtcNow;

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            CodigoCategoria = categoria.Codigo,
            CodigoPrioridad = prioridad.Codigo,
            CodigoEstado = estadoInicial.Codigo,
            IdAgencia = agencia.Id,
            IdUsuarioAsignado = request.IdUsuarioAsignado,
            FechaLimiteSla = ahora.AddHours(prioridad.HorasSla),
            CreadoEn = ahora,
            CreadoPor = request.RegistradoPor,
        };
        db.Tickets.Add(ticket);

        db.TicketEtapaHist.Add(new TicketEtapaHist
        {
            Id = Guid.NewGuid(),
            IdTicket = ticket.Id,
            CodigoEstadoAnterior = string.Empty,
            CodigoEstadoNuevo = estadoInicial.Codigo,
            Comentario = "Ticket creado",
            RegistradoPor = request.RegistradoPor,
            Fecha = ahora,
        });

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> ComentarAsync(Guid idTicket, ComentarTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == idTicket, cancellationToken)
            ?? throw new TicketNoExisteException(idTicket);

        db.TicketComentarios.Add(new TicketComentario
        {
            Id = Guid.NewGuid(),
            IdTicket = ticket.Id,
            Comentario = request.Comentario,
            RegistradoPor = request.RegistradoPor,
            Fecha = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> CambiarEstadoAsync(Guid idTicket, CambiarEstadoTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == idTicket, cancellationToken)
            ?? throw new TicketNoExisteException(idTicket);

        if (EstadosFinales.Contains(ticket.CodigoEstado))
            throw new TicketYaCerradoException(ticket.Numero);

        var estadoNuevo = await db.EstadosTicket.FirstOrDefaultAsync(e => e.Codigo == request.CodigoEstado && e.Activo, cancellationToken)
            ?? throw new EstadoTicketInvalidoException(request.CodigoEstado);

        var estadoAnterior = ticket.CodigoEstado;
        var ahora = DateTimeOffset.UtcNow;

        ticket.CodigoEstado = estadoNuevo.Codigo;
        ticket.ModificadoEn = ahora;
        ticket.ModificadoPor = request.RegistradoPor;
        ticket.FechaPrimeraRespuesta ??= ahora;
        // FechaCierre se fija al llegar a Resuelto/Cerrado/Cancelado, y se
        // limpia si se reabre desde ahí hacia cualquier otro estado.
        ticket.FechaCierre = EstadosQueFijanCierre.Contains(estadoNuevo.Codigo) ? ahora : null;
        // Un ticket reabierto (vuelve a un estado no final) pierde una
        // calificación previa — el usuario va a calificar de nuevo cuando
        // realmente se resuelva, no la primera atención a medias.
        if (!EstadosQueFijanCierre.Contains(estadoNuevo.Codigo) && ticket.Calificacion is not null)
        {
            ticket.Calificacion = null;
            ticket.ComentarioCalificacion = null;
            ticket.FechaCalificacion = null;
        }

        db.TicketEtapaHist.Add(new TicketEtapaHist
        {
            Id = Guid.NewGuid(),
            IdTicket = ticket.Id,
            CodigoEstadoAnterior = estadoAnterior,
            CodigoEstadoNuevo = estadoNuevo.Codigo,
            Comentario = request.Comentario,
            RegistradoPor = request.RegistradoPor,
            Fecha = ahora,
        });

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> AsignarAsync(Guid idTicket, AsignarTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == idTicket, cancellationToken)
            ?? throw new TicketNoExisteException(idTicket);

        if (request.IdUsuarioAsignado is not null)
            await ValidarEsAgenteAsync(request.IdUsuarioAsignado.Value, cancellationToken);

        var ahora = DateTimeOffset.UtcNow;
        ticket.IdUsuarioAsignado = request.IdUsuarioAsignado;
        ticket.ModificadoEn = ahora;
        ticket.ModificadoPor = request.RegistradoPor;
        ticket.FechaPrimeraRespuesta ??= ahora;

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> TomarAsync(Guid idTicket, TomarTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await db.Tickets.Include(t => t.UsuarioAsignado)
            .FirstOrDefaultAsync(t => t.Id == idTicket, cancellationToken)
            ?? throw new TicketNoExisteException(idTicket);

        if (EstadosFinales.Contains(ticket.CodigoEstado))
            throw new TicketYaCerradoException(ticket.Numero);

        if (ticket.IdUsuarioAsignado is not null && ticket.IdUsuarioAsignado != request.IdUsuarioAgente)
            throw new TicketYaAsignadoException(ticket.Numero, ticket.UsuarioAsignado?.NombreUsuario ?? ticket.IdUsuarioAsignado.Value.ToString());

        await ValidarEsAgenteAsync(request.IdUsuarioAgente, cancellationToken);

        var ahora = DateTimeOffset.UtcNow;
        ticket.IdUsuarioAsignado = request.IdUsuarioAgente;
        ticket.ModificadoEn = ahora;
        ticket.ModificadoPor = request.RegistradoPor;
        ticket.FechaPrimeraRespuesta ??= ahora;

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> CalificarAsync(Guid idTicket, CalificarTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.Id == idTicket, cancellationToken)
            ?? throw new TicketNoExisteException(idTicket);

        if (request.Calificacion is < 1 or > 5)
            throw new CalificacionInvalidaException(request.Calificacion);

        if (!string.Equals(ticket.CreadoPor, request.SolicitadoPor, StringComparison.OrdinalIgnoreCase))
            throw new SoloCreadorPuedeCalificarException();

        if (!EstadosCalificables.Contains(ticket.CodigoEstado))
            throw new TicketNoCalificableException(ticket.Numero);

        if (ticket.Calificacion is not null)
            throw new TicketYaCalificadoException(ticket.Numero);

        ticket.Calificacion = request.Calificacion;
        ticket.ComentarioCalificacion = request.Comentario;
        ticket.FechaCalificacion = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(ticket.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<TicketListItemDto>> ListarAsync(ListarTicketsFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = db.Tickets
            .Include(t => t.Categoria).Include(t => t.Prioridad).Include(t => t.Estado)
            .Include(t => t.Agencia).Include(t => t.UsuarioAsignado)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.CodigoEstado)) query = query.Where(t => t.CodigoEstado == filtro.CodigoEstado);
        if (filtro.IdUsuarioAsignado is not null) query = query.Where(t => t.IdUsuarioAsignado == filtro.IdUsuarioAsignado);
        if (!string.IsNullOrWhiteSpace(filtro.CreadoPor)) query = query.Where(t => t.CreadoPor == filtro.CreadoPor);

        var ahora = DateTimeOffset.UtcNow;

        return await query
            .OrderByDescending(t => t.CreadoEn)
            .Select(t => new TicketListItemDto(
                t.Id, t.Numero, t.Titulo,
                t.Categoria.Nombre, t.Prioridad.Nombre, t.CodigoEstado, t.Estado.Nombre,
                t.Agencia.Nombre, t.UsuarioAsignado == null ? null : t.UsuarioAsignado.NombreUsuario,
                t.FechaLimiteSla, t.FechaCierre == null && t.FechaLimiteSla < ahora,
                t.Calificacion,
                t.CreadoEn, t.CreadoPor))
            .ToListAsync(cancellationToken);
    }

    public async Task<TicketDetalleDto> ObtenerAsync(Guid idTicket, CancellationToken cancellationToken = default)
    {
        var ticket = await MapearDtoAsync(idTicket, cancellationToken);

        var comentarios = await db.TicketComentarios
            .Where(c => c.IdTicket == idTicket)
            .OrderBy(c => c.Fecha)
            .Select(c => new TicketComentarioDto(c.Comentario, c.RegistradoPor, c.Fecha))
            .ToListAsync(cancellationToken);

        var etapas = await db.TicketEtapaHist
            .Where(h => h.IdTicket == idTicket)
            .OrderBy(h => h.Fecha)
            .ToListAsync(cancellationToken);

        // Tiempo real que pasó en el estado anterior antes de esta transición
        // — se calcula contra la fecha de la fila previa de la misma
        // bitácora, nunca un valor aparte que se pueda desincronizar.
        var bitacora = new List<TicketEtapaHistDto>();
        for (var i = 0; i < etapas.Count; i++)
        {
            double? horas = i == 0 ? null : (etapas[i].Fecha - etapas[i - 1].Fecha).TotalHours;
            bitacora.Add(new TicketEtapaHistDto(etapas[i].CodigoEstadoAnterior, etapas[i].CodigoEstadoNuevo, etapas[i].Comentario, etapas[i].RegistradoPor, etapas[i].Fecha, horas));
        }

        return new TicketDetalleDto(ticket, comentarios, bitacora);
    }

    public async Task<IReadOnlyList<AgenteDto>> ListarAgentesAsync(CancellationToken cancellationToken = default)
    {
        var ahora = DateTimeOffset.UtcNow;

        var idsRolConPermiso = db.RolesMenu
            .Where(rm => rm.Activo && rm.Menu.Activo && rm.Menu.Codigo == CodigoMenuAgente)
            .Select(rm => rm.IdRol);

        var porRolPermanente = db.Usuarios.Where(u => u.Activo)
            .Where(u => u.UsuarioRoles.Any(ur => ur.Activo && idsRolConPermiso.Contains(ur.IdRol)));

        var porRolTemporal = db.Usuarios.Where(u => u.Activo)
            .Where(u => db.UsuariosRolTemporal.Any(urt =>
                urt.IdUsuario == u.Id && urt.Activo && urt.FechaCaducidad > ahora && idsRolConPermiso.Contains(urt.IdRol)));

        return await porRolPermanente.Union(porRolTemporal)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new AgenteDto(u.Id, u.NombreUsuario))
            .ToListAsync(cancellationToken);
    }

    private async Task ValidarEsAgenteAsync(Guid idUsuario, CancellationToken cancellationToken)
    {
        var agentes = await ListarAgentesAsync(cancellationToken);
        if (!agentes.Any(a => a.Id == idUsuario))
            throw new UsuarioAsignadoInvalidoException(idUsuario);
    }

    private async Task<TicketDto> MapearDtoAsync(Guid idTicket, CancellationToken cancellationToken)
    {
        // Se materializa la entidad y se calculan las horas en memoria en vez
        // de proyectar la resta de fechas en el SQL — Npgsql no siempre
        // traduce bien aritmética de DateTimeOffset dentro de un Select, y
        // este cálculo es liviano (un solo ticket), sin costo real de traer
        // de más.
        var t = await db.Tickets
            .Include(x => x.Categoria).Include(x => x.Prioridad).Include(x => x.Estado)
            .Include(x => x.Agencia).Include(x => x.UsuarioAsignado)
            .FirstAsync(x => x.Id == idTicket, cancellationToken);

        var ahora = DateTimeOffset.UtcNow;
        return new TicketDto(
            t.Id, t.Numero, t.Titulo, t.Descripcion,
            t.CodigoCategoria, t.Categoria.Nombre,
            t.CodigoPrioridad, t.Prioridad.Nombre,
            t.CodigoEstado, t.Estado.Nombre,
            t.Agencia.Nombre, t.UsuarioAsignado?.NombreUsuario,
            t.FechaLimiteSla, t.FechaCierre == null && t.FechaLimiteSla < ahora, t.FechaCierre,
            t.FechaPrimeraRespuesta,
            t.FechaPrimeraRespuesta is null ? null : (t.FechaPrimeraRespuesta.Value - t.CreadoEn).TotalHours,
            t.FechaCierre is null ? null : (t.FechaCierre.Value - t.CreadoEn).TotalHours,
            t.Calificacion, t.ComentarioCalificacion,
            t.CreadoEn, t.CreadoPor);
    }
}
