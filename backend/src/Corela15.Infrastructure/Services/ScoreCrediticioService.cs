using Corela15.Application.Colocacion;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Credito;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ScoreCrediticioService(Corela15DbContext db) : IScoreCrediticioService
{
    public async Task<ScoreCrediticioResult> CalcularAsync(Guid idCliente, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural)
            .FirstOrDefaultAsync(c => c.Id == idCliente, cancellationToken);
        if (cliente is null)
        {
            throw new Corela15.Application.Ahorros.ClienteInvalidoException(idCliente);
        }

        var persona = cliente.Persona;
        var puntaje = 50;

        decimal? ratioIngresoEgreso = null;
        if (persona.Ingresos is > 0)
        {
            ratioIngresoEgreso = (persona.Ingresos.Value - (persona.Egresos ?? 0)) / persona.Ingresos.Value;
            puntaje += ratioIngresoEgreso switch
            {
                >= 0.5m => 30,
                >= 0.3m => 20,
                >= 0.1m => 10,
                >= 0m => 0,
                _ => -20,
            };
        }

        decimal? ratioEndeudamiento = null;
        if (persona.Activos is > 0)
        {
            ratioEndeudamiento = (persona.Pasivos ?? 0) / persona.Activos.Value;
            puntaje += ratioEndeudamiento switch
            {
                < 0.3m => 10,
                < 0.6m => 0,
                _ => -15,
            };
        }

        var prestamosDelCliente = await db.PrestamosClientes
            .Where(pc => pc.IdCliente == idCliente)
            .Select(pc => pc.Prestamo.Estado)
            .ToListAsync(cancellationToken);

        var tienePrestamoCastigado = prestamosDelCliente.Contains(EstadoPrestamo.Castigado);
        var prestamosCancelados = prestamosDelCliente.Count(e => e == EstadoPrestamo.Cancelado);

        if (tienePrestamoCastigado)
        {
            puntaje -= 40;
        }
        puntaje += Math.Min(prestamosCancelados * 10, 20);

        puntaje = Math.Clamp(puntaje, 0, 100);

        var categoria = puntaje switch
        {
            >= 70 => CategoriaScore.RiesgoBajo,
            >= 40 => CategoriaScore.RiesgoMedio,
            _ => CategoriaScore.RiesgoAlto,
        };

        var esPep = persona.PersonaNatural?.EsPep ?? false;

        var score = new ScoreCrediticio
        {
            Id = Guid.NewGuid(),
            IdCliente = idCliente,
            Fecha = DateTimeOffset.UtcNow,
            Puntaje = puntaje,
            Categoria = categoria,
            RatioIngresoEgreso = ratioIngresoEgreso,
            RatioEndeudamiento = ratioEndeudamiento,
            TienePrestamoCastigado = tienePrestamoCastigado,
            PrestamosCancelados = prestamosCancelados,
            EsPep = esPep,
        };
        db.ScoresCrediticios.Add(score);
        await db.SaveChangesAsync(cancellationToken);

        return Mapear(score);
    }

    public async Task<IReadOnlyList<ScoreCrediticioResult>> HistorialAsync(Guid idCliente, CancellationToken cancellationToken = default)
    {
        return await db.ScoresCrediticios
            .Where(s => s.IdCliente == idCliente)
            .OrderByDescending(s => s.Fecha)
            .Select(s => new ScoreCrediticioResult(
                s.Id, s.IdCliente, s.Fecha, s.Puntaje, s.Categoria.ToString(),
                s.RatioIngresoEgreso, s.RatioEndeudamiento, s.TienePrestamoCastigado, s.PrestamosCancelados, s.EsPep))
            .ToListAsync(cancellationToken);
    }

    private static ScoreCrediticioResult Mapear(ScoreCrediticio s) => new(
        s.Id, s.IdCliente, s.Fecha, s.Puntaje, s.Categoria.ToString(),
        s.RatioIngresoEgreso, s.RatioEndeudamiento, s.TienePrestamoCastigado, s.PrestamosCancelados, s.EsPep);
}
