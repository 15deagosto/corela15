using Corela15.Application.LavadoActivos;
using Corela15.Domain.LavadoActivos;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ListaControlService(Corela15DbContext db) : IListaControlService
{
    public async Task<AlertaListaControlResult> RegistrarAlertaAsync(
        RegistrarAlertaListaControlRequest request, CancellationToken cancellationToken = default)
    {
        var personaExiste = await db.Personas.AnyAsync(p => p.Id == request.IdPersona, cancellationToken);
        if (!personaExiste)
        {
            throw new PersonaInvalidaException(request.IdPersona);
        }

        var tipoActivo = await db.TiposListaControl
            .AnyAsync(t => t.Codigo == request.CodigoTipoListaControl && t.Activo, cancellationToken);
        if (!tipoActivo)
        {
            throw new TipoListaControlInvalidoException(request.CodigoTipoListaControl);
        }

        var alerta = new AlertaListaControl
        {
            Id = Guid.NewGuid(),
            IdPersona = request.IdPersona,
            CodigoTipoListaControl = request.CodigoTipoListaControl,
            Detalle = request.Detalle,
            FechaDeteccion = DateOnly.FromDateTime(DateTime.UtcNow),
            Resuelta = false,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.AlertasListaControl.Add(alerta);
        await db.SaveChangesAsync(cancellationToken);

        return new AlertaListaControlResult(alerta.Id);
    }

    public async Task ResolverAlertaAsync(
        ResolverAlertaListaControlRequest request, CancellationToken cancellationToken = default)
    {
        var alerta = await db.AlertasListaControl
            .FirstOrDefaultAsync(a => a.Id == request.IdAlerta && !a.Resuelta, cancellationToken);
        if (alerta is null)
        {
            throw new AlertaListaControlInvalidaException(request.IdAlerta);
        }

        alerta.Resuelta = true;
        alerta.ComentarioResolucion = request.Comentario;
        alerta.ResueltoPor = request.ResueltoPor;
        alerta.FechaResolucion = DateTimeOffset.UtcNow;
        alerta.ModificadoEn = DateTimeOffset.UtcNow;
        alerta.ModificadoPor = request.ResueltoPor;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AlertaListaControlDetalle>> ListarAsync(
        bool soloNoResueltas, CancellationToken cancellationToken = default)
    {
        var query = db.AlertasListaControl
            .Include(a => a.Persona)
            .Include(a => a.TipoListaControl)
            .AsQueryable();

        if (soloNoResueltas)
        {
            query = query.Where(a => !a.Resuelta);
        }

        return await query
            .OrderByDescending(a => a.FechaDeteccion)
            .Select(a => new AlertaListaControlDetalle(
                a.Id, a.Persona.Nombre, a.Persona.Identificacion, a.CodigoTipoListaControl, a.TipoListaControl.Nombre,
                a.Detalle, a.FechaDeteccion, a.Resuelta, a.ComentarioResolucion, a.ResueltoPor, a.CreadoPor))
            .ToListAsync(cancellationToken);
    }
}
