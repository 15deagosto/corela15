using Corela15.Application.Nomina;
using Corela15.Domain.Nomina;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CalculoImpuestoRentaService(Corela15DbContext db) : ICalculoImpuestoRentaService
{
    // Tasa real de aporte personal al IESS (Ecuador, en relación de
    // dependencia) — deducible antes de aplicar la tabla del SRI. Dato
    // público, no vive en ninguna tabla de Softbank, mismo criterio ya
    // usado con la tasa patronal 11.15%.
    private const decimal TasaAportePersonalIess = 0.0945m;

    public async Task<CalculoImpuestoRentaResult> CalcularAsync(Guid idEmpleado, CancellationToken cancellationToken = default)
    {
        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == idEmpleado, cancellationToken)
            ?? throw new EmpleadoSinIngresoParaRentaException(idEmpleado);

        var ultimoIngresoMensual = await db.RolesPagosEmpleado
            .Where(rpe => rpe.IdEmpleado == idEmpleado && !rpe.Anulado)
            .OrderByDescending(rpe => rpe.RolPagos.Periodo)
            .Select(rpe => (decimal?)rpe.Ingresos)
            .FirstOrDefaultAsync(cancellationToken);

        if (ultimoIngresoMensual is null)
        {
            throw new EmpleadoSinIngresoParaRentaException(idEmpleado);
        }

        var ingresoAnual = Math.Round(ultimoIngresoMensual.Value * 12m, 2);
        var aportePersonal = Math.Round(ingresoAnual * TasaAportePersonalIess, 2);
        var baseImponible = ingresoAnual - aportePersonal;

        var tramo = await db.TramosImpuestoRenta
            .Where(t => baseImponible >= t.FraccionBasica && baseImponible < t.ExcesoHasta)
            .OrderByDescending(t => t.FraccionBasica)
            .FirstOrDefaultAsync(cancellationToken);
        tramo ??= await db.TramosImpuestoRenta.OrderByDescending(t => t.FraccionBasica).FirstAsync(cancellationToken);

        var impuestoCausado = Math.Round(
            tramo.ImpuestoFraccionBasica + (baseImponible - tramo.FraccionBasica) * (tramo.PorcentajeExcedente / 100m), 2);
        var retencionMensual = Math.Round(impuestoCausado / 12m, 2);

        var calculo = new CalculoImpuestoRenta
        {
            Id = Guid.NewGuid(),
            IdEmpleado = idEmpleado,
            Anio = DateTime.UtcNow.Year,
            IngresoAnualProyectado = ingresoAnual,
            BaseImponible = baseImponible,
            ImpuestoCausadoAnual = impuestoCausado,
            RetencionMensual = retencionMensual,
            FechaCalculo = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        db.CalculosImpuestoRenta.Add(calculo);
        await db.SaveChangesAsync(cancellationToken);

        return new CalculoImpuestoRentaResult(
            calculo.Id, idEmpleado, calculo.Anio, ingresoAnual, aportePersonal, baseImponible, impuestoCausado,
            retencionMensual, calculo.FechaCalculo);
    }

    public async Task<IReadOnlyList<CalculoImpuestoRentaResult>> HistorialAsync(
        Guid idEmpleado, CancellationToken cancellationToken = default)
    {
        return await db.CalculosImpuestoRenta
            .Where(c => c.IdEmpleado == idEmpleado)
            .OrderByDescending(c => c.FechaCalculo)
            .Select(c => new CalculoImpuestoRentaResult(
                c.Id, c.IdEmpleado, c.Anio, c.IngresoAnualProyectado,
                Math.Round(c.IngresoAnualProyectado * TasaAportePersonalIess, 2),
                c.BaseImponible, c.ImpuestoCausadoAnual, c.RetencionMensual, c.FechaCalculo))
            .ToListAsync(cancellationToken);
    }
}
