using Corela15.Application.LavadoActivos;
using Corela15.Domain.LavadoActivos;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class PerfilLavadoActivosService(Corela15DbContext db) : IPerfilLavadoActivosService
{
    public async Task<PerfilLavadoActivosResult> CalcularAsync(Guid idCliente, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes.Include(c => c.Persona).FirstOrDefaultAsync(c => c.Id == idCliente, cancellationToken);
        if (cliente is null)
        {
            throw new ClienteInvalidoParaPerfilLavadoException(idCliente);
        }

        decimal? patrimonio = cliente.Persona.Activos.HasValue && cliente.Persona.Pasivos.HasValue
            ? cliente.Persona.Activos.Value - cliente.Persona.Pasivos.Value
            : null;
        var ingresoMensual = cliente.Persona.Ingresos;

        var bandaPatrimonio = patrimonio.HasValue
            ? await db.RangosPatrimonioLavado
                .Where(r => r.Activo && r.ValorInicial <= patrimonio.Value && r.ValorFinal >= patrimonio.Value)
                .Select(r => (decimal?)r.Valor)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var bandaIngreso = ingresoMensual.HasValue
            ? await db.RangosIngresoLavado
                .Where(r => r.Activo && r.ValorInicial <= ingresoMensual.Value && r.ValorFinal >= ingresoMensual.Value)
                .Select(r => (decimal?)r.Valor)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        // Promedio simple de las dos bandas disponibles — ver
        // CalificacionCliente.cs para por qué no es el score ponderado de
        // 5 grupos real de Softbank (los otros 4 grupos no son
        // calculables con datos que este core capture hoy).
        decimal? totalPerfil = null;
        if (bandaPatrimonio.HasValue && bandaIngreso.HasValue)
        {
            totalPerfil = (bandaPatrimonio.Value + bandaIngreso.Value) / 2m;
        }
        else if (bandaPatrimonio.HasValue || bandaIngreso.HasValue)
        {
            totalPerfil = bandaPatrimonio ?? bandaIngreso;
        }

        var categoria = totalPerfil.HasValue
            ? (Domain.LavadoActivos.CategoriaRiesgoLavado)Math.Clamp((int)Math.Round(totalPerfil.Value, MidpointRounding.AwayFromZero), 1, 4)
            : (Domain.LavadoActivos.CategoriaRiesgoLavado?)null;

        var calificacion = new CalificacionCliente
        {
            Id = Guid.NewGuid(),
            IdCliente = idCliente,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow),
            Patrimonio = patrimonio,
            IngresoMensual = ingresoMensual,
            BandaPatrimonio = bandaPatrimonio,
            BandaIngreso = bandaIngreso,
            TotalPerfil = totalPerfil,
            Categoria = categoria,
        };
        db.CalificacionesCliente.Add(calificacion);
        await db.SaveChangesAsync(cancellationToken);

        return new PerfilLavadoActivosResult(
            calificacion.Id, idCliente, calificacion.Fecha, patrimonio, ingresoMensual,
            bandaPatrimonio, bandaIngreso, totalPerfil, categoria);
    }

    public async Task<IReadOnlyList<PerfilLavadoActivosResult>> HistorialAsync(Guid idCliente, CancellationToken cancellationToken = default)
    {
        return await db.CalificacionesCliente
            .Where(c => c.IdCliente == idCliente)
            .OrderByDescending(c => c.Fecha)
            .Select(c => new PerfilLavadoActivosResult(
                c.Id, c.IdCliente, c.Fecha, c.Patrimonio, c.IngresoMensual,
                c.BandaPatrimonio, c.BandaIngreso, c.TotalPerfil, c.Categoria))
            .ToListAsync(cancellationToken);
    }
}
