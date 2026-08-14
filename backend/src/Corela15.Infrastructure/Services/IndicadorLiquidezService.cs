using Corela15.Application.Riesgo;
using Corela15.Domain.Riesgo;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class IndicadorLiquidezService(Corela15DbContext db) : IIndicadorLiquidezService
{
    public async Task<IndicadorLiquidezResult> CalcularAsync(CancellationToken cancellationToken = default)
    {
        var periodo = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var fondosDisponibles = await db.SaldosContables
            .Where(s => s.Periodo == periodo && s.CuentaContable.Codigo.StartsWith("11"))
            .SumAsync(s => (decimal?)s.SaldoFinal, cancellationToken) ?? 0m;

        var depositosCortoPlazo = await db.SaldosContables
            .Where(s => s.Periodo == periodo && s.CuentaContable.Codigo.StartsWith("21"))
            .SumAsync(s => (decimal?)s.SaldoFinal, cancellationToken) ?? 0m;

        var coeficiente = depositosCortoPlazo > 0 ? fondosDisponibles / depositosCortoPlazo : 0m;

        var parametro = await ObtenerOCrearParametroAsync(cancellationToken);

        var indicador = new IndicadorLiquidez
        {
            Id = Guid.NewGuid(),
            Fecha = DateTimeOffset.UtcNow,
            FondosDisponibles = fondosDisponibles,
            DepositosCortoPlazo = depositosCortoPlazo,
            Coeficiente = coeficiente,
            MinimoRegulatorio = parametro.MinimoRegulatorio,
            CumpleMinimo = coeficiente >= parametro.MinimoRegulatorio,
        };
        db.IndicadoresLiquidez.Add(indicador);
        await db.SaveChangesAsync(cancellationToken);

        return Mapear(indicador);
    }

    public async Task<IReadOnlyList<IndicadorLiquidezResult>> ListarHistoricoAsync(CancellationToken cancellationToken = default)
    {
        return await db.IndicadoresLiquidez
            .OrderByDescending(i => i.Fecha)
            .Take(50)
            .Select(i => new IndicadorLiquidezResult(
                i.Id, i.Fecha, i.FondosDisponibles, i.DepositosCortoPlazo, i.Coeficiente, i.MinimoRegulatorio, i.CumpleMinimo))
            .ToListAsync(cancellationToken);
    }

    public async Task<ParametroLiquidezResult> ObtenerParametroAsync(CancellationToken cancellationToken = default)
    {
        var parametro = await ObtenerOCrearParametroAsync(cancellationToken);
        return new ParametroLiquidezResult(parametro.MinimoRegulatorio, parametro.ActualizadoEn, parametro.ActualizadoPor);
    }

    public async Task<ParametroLiquidezResult> ActualizarParametroAsync(
        decimal minimoRegulatorio, string registradoPor, CancellationToken cancellationToken = default)
    {
        var parametro = await ObtenerOCrearParametroAsync(cancellationToken);
        parametro.MinimoRegulatorio = minimoRegulatorio;
        parametro.ActualizadoEn = DateTimeOffset.UtcNow;
        parametro.ActualizadoPor = registradoPor;
        await db.SaveChangesAsync(cancellationToken);

        return new ParametroLiquidezResult(parametro.MinimoRegulatorio, parametro.ActualizadoEn, parametro.ActualizadoPor);
    }

    private async Task<ParametroLiquidez> ObtenerOCrearParametroAsync(CancellationToken cancellationToken)
    {
        var parametro = await db.ParametrosLiquidez.FirstOrDefaultAsync(cancellationToken);
        if (parametro is null)
        {
            parametro = new ParametroLiquidez
            {
                MinimoRegulatorio = 0.25m,
                ActualizadoEn = DateTimeOffset.UtcNow,
                ActualizadoPor = "sistema",
            };
            db.ParametrosLiquidez.Add(parametro);
            await db.SaveChangesAsync(cancellationToken);
        }
        return parametro;
    }

    private static IndicadorLiquidezResult Mapear(IndicadorLiquidez i) => new(
        i.Id, i.Fecha, i.FondosDisponibles, i.DepositosCortoPlazo, i.Coeficiente, i.MinimoRegulatorio, i.CumpleMinimo);
}
