using Corela15.Application.Cobranza;
using Corela15.Application.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class GastoCobranzaService(Corela15DbContext db, IMoraCarteraService moraCarteraService) : IGastoCobranzaService
{
    public async Task<IReadOnlyList<GastoCobranzaEstimado>> EstimarAsync(CancellationToken cancellationToken = default)
    {
        var moras = (await moraCarteraService.CalcularAsync(cancellationToken))
            .Where(m => m.DiasMora > 0)
            .ToList();

        if (moras.Count == 0) return [];

        var tarifas = await db.TarifasGastoCobranza.AsNoTracking().ToListAsync(cancellationToken);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var resultado = new List<GastoCobranzaEstimado>(moras.Count);

        foreach (var mora in moras)
        {
            var numeroCuotaVencida = await db.PrestamosRubros
                .Where(r => r.IdPrestamo == mora.IdPrestamo && r.Rubro.TipoRubro.EsCapital
                    && r.Estado == "P" && r.FechaFin < hoy)
                .OrderBy(r => r.FechaFin)
                .Select(r => (int?)r.NumeroCuota)
                .FirstOrDefaultAsync(cancellationToken);

            var montoCuota = numeroCuotaVencida is null
                ? 0m
                : await db.PrestamosRubros
                    .Where(r => r.IdPrestamo == mora.IdPrestamo && r.NumeroCuota == numeroCuotaVencida)
                    .SumAsync(r => r.Proyectado, cancellationToken);

            var tarifa = tarifas.FirstOrDefault(t =>
                montoCuota >= t.CuotaInicio && montoCuota <= t.CuotaFinal &&
                mora.DiasMora >= t.DiaMoraInicio && mora.DiasMora <= t.DiaMoraFin);

            resultado.Add(new GastoCobranzaEstimado(
                mora.IdPrestamo, mora.Numero, mora.Saldo, mora.DiasMora,
                montoCuota, tarifa?.Codigo, tarifa?.Valor));
        }

        return resultado;
    }
}
