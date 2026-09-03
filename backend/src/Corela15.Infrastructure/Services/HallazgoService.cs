using Corela15.Application.Cumplimiento;
using Corela15.Domain.Cumplimiento;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class HallazgoService(Corela15DbContext db) : IHallazgoService
{
    private const string CodigoEstadoInicial = "IN";

    public async Task<HallazgoCreadoResult> CrearAsync(
        CrearHallazgoRequest request, CancellationToken cancellationToken = default)
    {
        var reportaActivo = await db.Usuarios.AnyAsync(u => u.Id == request.IdUsuarioReporta && u.Activo, cancellationToken);
        if (!reportaActivo)
        {
            throw new UsuarioReportaInvalidoException(request.IdUsuarioReporta);
        }

        var asignadosActivos = await db.Usuarios
            .Where(u => request.IdsUsuarioAsignado.Contains(u.Id) && u.Activo)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
        foreach (var idAsignado in request.IdsUsuarioAsignado)
        {
            if (!asignadosActivos.Contains(idAsignado))
            {
                throw new UsuarioAsignadoInvalidoException(idAsignado);
            }
        }

        var ahora = DateTimeOffset.UtcNow;

        var hallazgo = new Hallazgo
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Detalle = request.Detalle,
            IdUsuarioReporta = request.IdUsuarioReporta,
            CodigoEstado = CodigoEstadoInicial,
            Fecha = DateOnly.FromDateTime(ahora.UtcDateTime),
            CreadoEn = ahora,
            CreadoPor = request.RegistradoPor,
        };
        db.Hallazgos.Add(hallazgo);

        foreach (var idAsignado in request.IdsUsuarioAsignado)
        {
            db.HallazgosUsuario.Add(new HallazgoUsuario
            {
                Id = Guid.NewGuid(),
                IdHallazgo = hallazgo.Id,
                IdUsuario = idAsignado,
                EstadoRespondido = false,
                Activo = true,
            });
        }

        db.HallazgosEtapa.Add(new HallazgoEtapa
        {
            Id = Guid.NewGuid(),
            IdHallazgo = hallazgo.Id,
            CodigoEstado = CodigoEstadoInicial,
            Comentario = "Hallazgo registrado",
            RegistradoPor = request.RegistradoPor,
            Fecha = ahora,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new HallazgoCreadoResult(hallazgo.Id);
    }

    public async Task ResponderAsync(ResponderHallazgoRequest request, CancellationToken cancellationToken = default)
    {
        var asignacion = await db.HallazgosUsuario
            .FirstOrDefaultAsync(h => h.Id == request.IdHallazgoUsuario && h.Activo, cancellationToken);
        if (asignacion is null)
        {
            throw new HallazgoUsuarioInvalidoException(request.IdHallazgoUsuario);
        }

        asignacion.EstadoRespondido = true;

        db.HallazgosUsuarioRespuesta.Add(new HallazgoUsuarioRespuesta
        {
            Id = Guid.NewGuid(),
            IdHallazgoUsuario = asignacion.Id,
            Respuesta = request.Respuesta,
            Fecha = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarEstadoAsync(CambiarEstadoHallazgoRequest request, CancellationToken cancellationToken = default)
    {
        var hallazgo = await db.Hallazgos.FirstOrDefaultAsync(h => h.Id == request.IdHallazgo, cancellationToken);
        if (hallazgo is null)
        {
            throw new HallazgoInvalidoException(request.IdHallazgo);
        }

        var estadoNuevoActivo = await db.EstadosHallazgo
            .AnyAsync(e => e.Codigo == request.CodigoEstadoNuevo && e.Activo, cancellationToken);
        if (!estadoNuevoActivo)
        {
            throw new EstadoHallazgoInvalidoException(request.CodigoEstadoNuevo);
        }

        hallazgo.CodigoEstado = request.CodigoEstadoNuevo;

        db.HallazgosEtapa.Add(new HallazgoEtapa
        {
            Id = Guid.NewGuid(),
            IdHallazgo = hallazgo.Id,
            CodigoEstado = request.CodigoEstadoNuevo,
            Comentario = request.Comentario,
            RegistradoPor = request.RegistradoPor,
            Fecha = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
