using Corela15.Application.Colocacion;
using Corela15.Domain.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class MoraCarteraService(Corela15DbContext db) : IMoraCarteraService
{
    public async Task<IReadOnlyList<PrestamoMoraResult>> CalcularAsync(CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var prestamosVigentes = await db.Prestamos
            .Where(p => p.Estado == EstadoPrestamo.Vigente)
            .Select(p => new { p.Id, p.Numero, p.Saldo })
            .ToListAsync(cancellationToken);

        var resultado = new List<PrestamoMoraResult>(prestamosVigentes.Count);

        foreach (var prestamo in prestamosVigentes)
        {
            var cuotaVencidaMasAntigua = await db.PrestamosRubros
                .Where(r => r.IdPrestamo == prestamo.Id && r.Rubro.Codigo == "CAP"
                    && r.Estado == "Pendiente" && r.FechaFin < hoy)
                .OrderBy(r => r.FechaFin)
                .Select(r => (DateOnly?)r.FechaFin)
                .FirstOrDefaultAsync(cancellationToken);

            var diasMora = cuotaVencidaMasAntigua is null ? 0 : hoy.DayNumber - cuotaVencidaMasAntigua.Value.DayNumber;

            resultado.Add(new PrestamoMoraResult(prestamo.Id, prestamo.Numero, prestamo.Saldo, diasMora));
        }

        return resultado;
    }
}
