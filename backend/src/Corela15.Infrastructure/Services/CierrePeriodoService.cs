using Corela15.Application.Contabilidad;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CierrePeriodoService(Corela15DbContext db) : ICierrePeriodoService
{
    public async Task<PeriodoCerradoResult> CerrarAsync(
        CerrarPeriodoRequest request, CancellationToken cancellationToken = default)
    {
        var periodo = new DateOnly(request.Periodo.Year, request.Periodo.Month, 1);

        var existente = await db.PeriodosContables.FirstOrDefaultAsync(p => p.Periodo == periodo, cancellationToken);
        if (existente is not null && existente.Cerrado)
        {
            throw new PeriodoYaCerradoException(periodo);
        }

        var fechaCierre = DateTimeOffset.UtcNow;

        if (existente is null)
        {
            db.PeriodosContables.Add(new PeriodoContable
            {
                Id = Guid.NewGuid(),
                Periodo = periodo,
                Cerrado = true,
                FechaCierre = fechaCierre,
                CerradoPor = request.RegistradoPor,
            });
        }
        else
        {
            existente.Cerrado = true;
            existente.FechaCierre = fechaCierre;
            existente.CerradoPor = request.RegistradoPor;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new PeriodoCerradoResult(periodo, fechaCierre);
    }

    public async Task<IReadOnlyList<PeriodoContableListItem>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await db.PeriodosContables
            .OrderByDescending(p => p.Periodo)
            .Select(p => new PeriodoContableListItem(p.Periodo, p.Cerrado, p.FechaCierre, p.CerradoPor))
            .ToListAsync(cancellationToken);
    }
}
