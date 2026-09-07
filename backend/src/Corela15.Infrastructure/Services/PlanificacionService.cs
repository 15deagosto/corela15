using Corela15.Application.Planificacion;
using Corela15.Domain.Planificacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class PlanificacionService(Corela15DbContext db) : IPlanificacionService
{
    public async Task<PlanSemanalDto> GuardarAsync(GuardarPlanSemanalRequest request, CancellationToken cancellationToken = default)
    {
        var area = await db.AreasPlanificacion.FirstOrDefaultAsync(a => a.Codigo == request.CodigoArea && a.Activo, cancellationToken)
            ?? throw new AreaPlanificacionInvalidaException(request.CodigoArea);

        if (request.Bloques.Count == 0)
            throw new BloqueHorarioInvalidoException("La planificación necesita al menos un bloque de actividad.");

        foreach (var b in request.Bloques)
        {
            if (b.HoraFin <= b.HoraInicio)
                throw new BloqueHorarioInvalidoException($"El bloque de {b.HoraInicio:HH\\:mm} a {b.HoraFin:HH\\:mm} tiene la hora fin antes o igual a la hora inicio.");
            if (b.DiaSemana < 1 || b.DiaSemana > 6)
                throw new BloqueHorarioInvalidoException("El día de la semana debe estar entre lunes (1) y sábado (6).");

            var etiquetaValida = await db.EtiquetasPlanificacion.AnyAsync(e => e.Codigo == b.CodigoEtiqueta && e.Activo, cancellationToken);
            if (!etiquetaValida)
                throw new EtiquetaPlanificacionInvalidaException(b.CodigoEtiqueta);
        }

        var existente = await db.PlanesSemanales
            .Include(p => p.Bloques)
            .FirstOrDefaultAsync(p => p.CodigoArea == request.CodigoArea && p.FechaInicioSemana == request.FechaInicioSemana, cancellationToken);

        PlanSemanal plan;
        if (existente is null)
        {
            plan = new PlanSemanal
            {
                Id = Guid.NewGuid(),
                CodigoArea = request.CodigoArea,
                FechaInicioSemana = request.FechaInicioSemana,
                NombreResponsable = request.NombreResponsable,
                CargoResponsable = request.CargoResponsable,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = request.RegistradoPor,
            };
            db.PlanesSemanales.Add(plan);
        }
        else
        {
            if (existente.CreadoPor != request.RegistradoPor)
                throw new PlanSemanalAjenoException();

            plan = existente;
            plan.NombreResponsable = request.NombreResponsable;
            plan.CargoResponsable = request.CargoResponsable;
            plan.ModificadoEn = DateTimeOffset.UtcNow;
            plan.ModificadoPor = request.RegistradoPor;
            db.PlanesSemanalesBloques.RemoveRange(existente.Bloques);
        }

        foreach (var b in request.Bloques)
        {
            db.PlanesSemanalesBloques.Add(new PlanSemanalBloque
            {
                Id = Guid.NewGuid(),
                IdPlanSemanal = plan.Id,
                DiaSemana = b.DiaSemana,
                HoraInicio = b.HoraInicio,
                HoraFin = b.HoraFin,
                CodigoEtiqueta = b.CodigoEtiqueta,
                Descripcion = b.Descripcion,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(plan.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<PlanSemanalListItemDto>> ListarAsync(ListarPlanesFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = db.PlanesSemanales.Include(p => p.Area).Include(p => p.Bloques).AsQueryable();

        // Sin permiso de gerencia, solo ve lo que él mismo reportó -- nunca
        // opcional del lado del cliente, mismo criterio real ya aplicado
        // en Mesa de Servicio tras el hallazgo de privacidad de esa ronda.
        if (!filtro.EsGerencia)
            query = query.Where(p => p.CreadoPor == filtro.UsuarioActual);

        if (!string.IsNullOrWhiteSpace(filtro.CodigoArea))
            query = query.Where(p => p.CodigoArea == filtro.CodigoArea);
        if (filtro.Desde is not null)
            query = query.Where(p => p.FechaInicioSemana >= filtro.Desde);
        if (filtro.Hasta is not null)
            query = query.Where(p => p.FechaInicioSemana <= filtro.Hasta);

        return await query
            .OrderByDescending(p => p.FechaInicioSemana)
            .Select(p => new PlanSemanalListItemDto(
                p.Id, p.CodigoArea, p.Area.Nombre, p.FechaInicioSemana,
                p.NombreResponsable, p.Bloques.Count, p.CreadoEn, p.CreadoPor))
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanSemanalDto> ObtenerAsync(Guid idPlan, string usuarioActual, bool esGerencia, CancellationToken cancellationToken = default)
    {
        var plan = await db.PlanesSemanales.FirstOrDefaultAsync(p => p.Id == idPlan, cancellationToken)
            ?? throw new PlanSemanalNoExisteException(idPlan);

        if (!esGerencia && plan.CreadoPor != usuarioActual)
            throw new PlanSemanalAjenoException();

        return await MapearDtoAsync(idPlan, cancellationToken);
    }

    private async Task<PlanSemanalDto> MapearDtoAsync(Guid idPlan, CancellationToken cancellationToken)
    {
        var plan = await db.PlanesSemanales
            .Include(p => p.Area)
            .Include(p => p.Bloques).ThenInclude(b => b.Etiqueta)
            .FirstAsync(p => p.Id == idPlan, cancellationToken);

        var bloques = plan.Bloques
            .OrderBy(b => b.DiaSemana).ThenBy(b => b.HoraInicio)
            .Select(b => new BloqueDto(b.Id, b.DiaSemana, b.HoraInicio, b.HoraFin, b.CodigoEtiqueta, b.Etiqueta.Nombre, b.Etiqueta.ColorHex, b.Descripcion))
            .ToList();

        return new PlanSemanalDto(
            plan.Id, plan.CodigoArea, plan.Area.Nombre, plan.FechaInicioSemana,
            plan.NombreResponsable, plan.CargoResponsable,
            bloques, plan.CreadoEn, plan.CreadoPor, plan.ModificadoEn);
    }
}
