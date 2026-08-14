using Corela15.Application.Ahorros;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Ahorros;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class DevengoInteresService(Corela15DbContext db, IComprobanteContableService comprobantes) : IDevengoInteresService
{
    private const string CodigoTipoTransaccion = "DEVENGO-INT-AHO";
    private const string CodigoItemSaldoInteres = "INT";
    private const string CodigoItemSaldoDisponible = "DISP";
    private const int IdAgenciaDefault = 1;

    public async Task<DevengoInteresEjecutadoResult> EjecutarDevengoDiarioAsync(
        string registradoPor, CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var cuentasElegibles = await db.Cuentas
            .Include(c => c.TipoCuenta)
            .Where(c => c.Estado == EstadoCuenta.Activa && c.TipoCuenta.TasaInteresAnual > 0)
            .Where(c => !db.DevengosInteresLog.Any(d => d.Fecha == hoy && d.IdCuenta == c.Id))
            .Select(c => new { c.Id, c.Numero, c.IdAgencia, TasaAnual = c.TipoCuenta.TasaInteresAnual })
            .ToListAsync(cancellationToken);

        var totalDevengado = 0m;
        var cuentasProcesadas = 0;

        foreach (var cuenta in cuentasElegibles)
        {
            var itemDisponible = await db.CuentasItemSaldo
                .Include(x => x.ItemSaldo)
                .FirstOrDefaultAsync(x => x.IdCuenta == cuenta.Id && x.ItemSaldo.Codigo == CodigoItemSaldoDisponible, cancellationToken);
            if (itemDisponible is null || itemDisponible.Saldo <= 0)
            {
                continue;
            }

            var interesDelDia = Math.Round(itemDisponible.Saldo * cuenta.TasaAnual / 365m, 2, MidpointRounding.AwayFromZero);
            if (interesDelDia <= 0)
            {
                continue;
            }

            var itemInteres = await db.CuentasItemSaldo
                .Include(x => x.ItemSaldo)
                .FirstOrDefaultAsync(x => x.IdCuenta == cuenta.Id && x.ItemSaldo.Codigo == CodigoItemSaldoInteres, cancellationToken);
            if (itemInteres is null)
            {
                continue; // el producto no tiene el balde INT configurado — no debería pasar con los productos sembrados, pero no se inventa uno acá
            }

            itemInteres.Saldo += interesDelDia;
            itemInteres.ModificadoEn = DateTimeOffset.UtcNow;
            itemInteres.ModificadoPor = registradoPor;

            db.DevengosInteresLog.Add(new DevengoInteresLog
            {
                Id = Guid.NewGuid(),
                Fecha = hoy,
                IdCuenta = cuenta.Id,
                SaldoBase = itemDisponible.Saldo,
                TasaAnualAplicada = cuenta.TasaAnual,
                MontoDevengado = interesDelDia,
            });

            totalDevengado += interesDelDia;
            cuentasProcesadas++;
        }

        await db.SaveChangesAsync(cancellationToken);

        Guid? idComprobante = null;
        if (totalDevengado > 0)
        {
            var tipoTransaccion = await db.TiposTransaccion
                .FirstAsync(t => t.Codigo == CodigoTipoTransaccion, cancellationToken);

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    hoy,
                    tipoTransaccion.IdTipoComprobante,
                    IdAgenciaDefault,
                    $"Devengo de interés sobre ahorros — {hoy:yyyy-MM-dd}",
                    registradoPor,
                    [
                        new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, totalDevengado, 0, "Devengo de interés"),
                        new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, totalDevengado, "Devengo de interés"),
                    ]),
                cancellationToken);

            idComprobante = resultadoComprobante.Id;
        }

        return new DevengoInteresEjecutadoResult(hoy, cuentasProcesadas, totalDevengado, idComprobante);
    }
}
