using Corela15.Application.Riesgo;
using Corela15.Domain.Riesgo;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class AvanceRiesgoService(Corela15DbContext db) : IAvanceRiesgoService
{
    private const string CodigoEstadoInicial = "PRE";

    public async Task<AvanceRiesgoCreadoResult> CrearAsync(
        CrearAvanceRiesgoRequest request, CancellationToken cancellationToken = default)
    {
        var eventoActivo = await db.EventosRiesgo
            .AnyAsync(e => e.Id == request.IdEventoRiesgo && e.Activo, cancellationToken);
        if (!eventoActivo)
        {
            throw new EventoRiesgoInvalidoParaAvanceException(request.IdEventoRiesgo);
        }

        var responsableActivo = await db.Usuarios
            .AnyAsync(u => u.Id == request.IdUsuarioResponsable && u.Activo, cancellationToken);
        if (!responsableActivo)
        {
            throw new UsuarioResponsableInvalidoException(request.IdUsuarioResponsable);
        }

        var ahora = DateTimeOffset.UtcNow;

        var avance = new AvanceRiesgo
        {
            Id = Guid.NewGuid(),
            IdEventoRiesgo = request.IdEventoRiesgo,
            IdUsuarioResponsable = request.IdUsuarioResponsable,
            HoraInicio = request.HoraInicio,
            HoraFin = request.HoraFin,
            CodigoEstado = CodigoEstadoInicial,
            Fecha = DateOnly.FromDateTime(ahora.UtcDateTime),
            CreadoEn = ahora,
            CreadoPor = request.RegistradoPor,
        };
        db.AvancesRiesgo.Add(avance);

        var detalle = new AvanceRiesgoDetalle
        {
            Id = Guid.NewGuid(),
            IdAvanceRiesgo = avance.Id,
            EventoDetectado = request.EventoDetectado,
            PosibleCausa = request.PosibleCausa,
            Tratamiento = request.Tratamiento,
            Inconvenientes = request.Inconvenientes,
            AccionesSugeridas = request.AccionesSugeridas,
        };
        db.AvancesRiesgoDetalle.Add(detalle);

        db.AvancesRiesgoEtapa.Add(new AvanceRiesgoEtapa
        {
            Id = Guid.NewGuid(),
            IdAvanceRiesgoDetalle = detalle.Id,
            CodigoEstado = CodigoEstadoInicial,
            Comentario = "Plan de acción creado",
            RegistradoPor = request.RegistradoPor,
            Fecha = ahora,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AvanceRiesgoCreadoResult(avance.Id, detalle.Id);
    }

    public async Task CambiarEstadoAsync(
        CambiarEstadoAvanceRiesgoRequest request, CancellationToken cancellationToken = default)
    {
        var detalle = await db.AvancesRiesgoDetalle
            .FirstOrDefaultAsync(d => d.Id == request.IdAvanceRiesgoDetalle, cancellationToken);
        if (detalle is null)
        {
            throw new AvanceRiesgoDetalleInvalidoException(request.IdAvanceRiesgoDetalle);
        }

        var estadoNuevoActivo = await db.EstadosAvanceRiesgo
            .AnyAsync(e => e.Codigo == request.CodigoEstadoNuevo && e.Activo, cancellationToken);
        if (!estadoNuevoActivo)
        {
            throw new EstadoAvanceRiesgoInvalidoException(request.CodigoEstadoNuevo);
        }

        var avance = await db.AvancesRiesgo.FirstAsync(a => a.Id == detalle.IdAvanceRiesgo, cancellationToken);
        avance.CodigoEstado = request.CodigoEstadoNuevo;

        db.AvancesRiesgoEtapa.Add(new AvanceRiesgoEtapa
        {
            Id = Guid.NewGuid(),
            IdAvanceRiesgoDetalle = detalle.Id,
            CodigoEstado = request.CodigoEstadoNuevo,
            Comentario = request.Comentario,
            RegistradoPor = request.RegistradoPor,
            Fecha = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
