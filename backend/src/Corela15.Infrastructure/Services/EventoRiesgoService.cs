using Corela15.Application.Riesgo;
using Corela15.Domain.Riesgo;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class EventoRiesgoService(Corela15DbContext db) : IEventoRiesgoService
{
    public async Task<EventoRiesgoRegistradoResult> RegistrarAsync(
        RegistrarEventoRiesgoRequest request, CancellationToken cancellationToken = default)
    {
        var procesoActivo = await db.Procesos
            .AnyAsync(p => p.Id == request.IdProceso && p.Activo, cancellationToken);
        if (!procesoActivo)
        {
            throw new ProcesoInvalidoException(request.IdProceso);
        }

        var nivelImpacto = await db.NivelesImpacto
            .FirstOrDefaultAsync(n => n.Id == request.IdNivelImpacto && n.Activo, cancellationToken);
        if (nivelImpacto is null)
        {
            throw new NivelImpactoInvalidoException(request.IdNivelImpacto);
        }

        var nivelProbabilidad = await db.NivelesProbabilidad
            .FirstOrDefaultAsync(n => n.Id == request.IdNivelProbabilidad && n.Activo, cancellationToken);
        if (nivelProbabilidad is null)
        {
            throw new NivelProbabilidadInvalidoException(request.IdNivelProbabilidad);
        }

        var puntaje = nivelImpacto.Nivel * nivelProbabilidad.Nivel;

        var nivelRiesgo = await db.NivelesRiesgo
            .FirstOrDefaultAsync(
                n => n.Activo && n.RangoInicio <= puntaje && n.RangoFin >= puntaje, cancellationToken);
        if (nivelRiesgo is null)
        {
            throw new SinNivelRiesgoParaPuntajeException(puntaje);
        }

        var evento = new EventoRiesgo
        {
            Id = Guid.NewGuid(),
            IdProceso = request.IdProceso,
            Descripcion = request.Descripcion,
            IdNivelImpacto = request.IdNivelImpacto,
            IdNivelProbabilidad = request.IdNivelProbabilidad,
            IdNivelRiesgo = nivelRiesgo.Id,
            FechaIdentificacion = DateOnly.FromDateTime(DateTime.UtcNow),
            Activo = true,
        };
        db.EventosRiesgo.Add(evento);
        await db.SaveChangesAsync(cancellationToken);

        return new EventoRiesgoRegistradoResult(evento.Id, puntaje, nivelRiesgo.Nombre, nivelRiesgo.Color ?? "#999999");
    }
}
