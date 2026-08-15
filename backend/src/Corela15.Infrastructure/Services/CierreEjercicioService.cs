using Corela15.Application.Contabilidad;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CierreEjercicioService(Corela15DbContext db, IComprobanteContableService comprobantes) : ICierreEjercicioService
{
    private const string CodigoCuentaUtilidad = "3603";
    private const string CodigoCuentaPerdida = "3604";
    private const int IdTipoComprobanteCierre = 5; // 'CIE'
    private const int IdAgenciaDefault = 1;

    public async Task<CierreEjercicioResult> CerrarAsync(
        CerrarEjercicioRequest request, CancellationToken cancellationToken = default)
    {
        if (await db.CierresEjercicio.AnyAsync(c => c.Anio == request.Anio, cancellationToken))
        {
            throw new EjercicioYaCerradoException(request.Anio);
        }

        var inicioAnio = new DateOnly(request.Anio, 1, 1);
        var finAnio = new DateOnly(request.Anio, 12, 1);

        var saldosPorCuenta = await db.SaldosContables
            .Where(s => s.Periodo >= inicioAnio && s.Periodo <= finAnio)
            .Include(s => s.CuentaContable)
            .Where(s => s.CuentaContable.Grupo == GrupoCuc.Ingresos || s.CuentaContable.Grupo == GrupoCuc.Gastos)
            .GroupBy(s => new { s.IdCuentaContable, s.CuentaContable.Codigo, s.CuentaContable.Nombre, s.CuentaContable.Grupo })
            .Select(g => new
            {
                g.Key.IdCuentaContable,
                g.Key.Codigo,
                g.Key.Nombre,
                g.Key.Grupo,
                SaldoAcumulado = g.Sum(x => x.SaldoFinal),
            })
            .ToListAsync(cancellationToken);

        var lineas = new List<LineaMovimientoRequest>();
        decimal totalIngresos = 0;
        decimal totalGastos = 0;

        foreach (var cuenta in saldosPorCuenta.Where(c => c.SaldoAcumulado != 0))
        {
            if (cuenta.Grupo == GrupoCuc.Ingresos)
            {
                // Naturaleza Acreedora: el saldo positivo es un crédito
                // acumulado — se debita para dejarlo en cero.
                lineas.Add(new LineaMovimientoRequest(cuenta.IdCuentaContable, cuenta.SaldoAcumulado, 0, $"Cierre {request.Anio} — {cuenta.Codigo} {cuenta.Nombre}"));
                totalIngresos += cuenta.SaldoAcumulado;
            }
            else
            {
                // Naturaleza Deudora: el saldo positivo es un débito
                // acumulado — se acredita para dejarlo en cero.
                lineas.Add(new LineaMovimientoRequest(cuenta.IdCuentaContable, 0, cuenta.SaldoAcumulado, $"Cierre {request.Anio} — {cuenta.Codigo} {cuenta.Nombre}"));
                totalGastos += cuenta.SaldoAcumulado;
            }
        }

        if (lineas.Count == 0)
        {
            throw new EjercicioSinMovimientosException(request.Anio);
        }

        var utilidad = totalIngresos - totalGastos;

        if (utilidad > 0)
        {
            var idCuentaUtilidad = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaUtilidad).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            if (idCuentaUtilidad is null)
            {
                throw new InvalidOperationException($"Falta la cuenta contable {CodigoCuentaUtilidad}.");
            }
            lineas.Add(new LineaMovimientoRequest(idCuentaUtilidad.Value, 0, utilidad, $"Utilidad del ejercicio {request.Anio}"));
        }
        else if (utilidad < 0)
        {
            var idCuentaPerdida = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaPerdida).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            if (idCuentaPerdida is null)
            {
                throw new InvalidOperationException($"Falta la cuenta contable {CodigoCuentaPerdida}.");
            }
            lineas.Add(new LineaMovimientoRequest(idCuentaPerdida.Value, -utilidad, 0, $"Pérdida del ejercicio {request.Anio}"));
        }

        // Una sola transacción: el comprobante de cierre y el registro de
        // CierreEjercicio se confirman juntos o no se confirma ninguno —
        // mismo patrón que CuentaAhorroService (ver CLAUDE.md).
        // ComprobanteContableService detecta esta transacción ambiente y
        // no abre la suya propia.
        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var fechaCierreComprobante = new DateOnly(request.Anio, 12, 31);
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                fechaCierreComprobante,
                IdTipoComprobanteCierre,
                IdAgenciaDefault,
                $"Cierre de resultados del ejercicio {request.Anio}",
                request.RegistradoPor,
                lineas),
            cancellationToken);

        var fechaCierre = DateTimeOffset.UtcNow;
        db.CierresEjercicio.Add(new CierreEjercicio
        {
            Id = Guid.NewGuid(),
            Anio = request.Anio,
            FechaCierre = fechaCierre,
            TotalIngresos = totalIngresos,
            TotalGastos = totalGastos,
            Utilidad = utilidad,
            IdComprobanteContable = resultadoComprobante.Id,
            CerradoPor = request.RegistradoPor,
        });
        await db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new CierreEjercicioResult(request.Anio, fechaCierre, totalIngresos, totalGastos, utilidad, resultadoComprobante.Id);
    }

    public async Task<IReadOnlyList<CierreEjercicioListItem>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await db.CierresEjercicio
            .OrderByDescending(c => c.Anio)
            .Select(c => new CierreEjercicioListItem(
                c.Anio, c.FechaCierre, c.TotalIngresos, c.TotalGastos, c.Utilidad, c.CerradoPor))
            .ToListAsync(cancellationToken);
    }
}
