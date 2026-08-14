using Corela15.Application.Colocacion;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ProvisionCarteraService(Corela15DbContext db, IComprobanteContableService comprobantes) : IProvisionCarteraService
{
    private const string CodigoCuentaProvision = "1499";
    private const string CodigoTipoTransaccionProvision = "PROV-CART";
    private const int IdAgenciaDefault = 1; // Matriz — mismo patrón usado en el resto del core

    public async Task<ProvisionCarteraCalculadaResult> EjecutarCalculoAsync(
        string registradoPor, CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var categorias = await db.CategoriasRiesgoCartera
            .Where(c => c.Activo)
            .OrderBy(c => c.DiasMoraInicio)
            .ToListAsync(cancellationToken);

        var prestamosVigentes = await db.Prestamos
            .Where(p => p.Estado == EstadoPrestamo.Vigente)
            .Select(p => new { p.Id, p.Saldo })
            .ToListAsync(cancellationToken);

        var acumuladoPorCategoria = new Dictionary<string, (int Cantidad, decimal Saldo, decimal Provision)>();
        var totalRequerido = 0m;

        foreach (var prestamo in prestamosVigentes)
        {
            var cuotaVencidaMasAntigua = await db.PrestamosRubros
                .Where(r => r.IdPrestamo == prestamo.Id && r.Rubro.Codigo == "CAP"
                    && r.Estado == "Pendiente" && r.FechaFin < hoy)
                .OrderBy(r => r.FechaFin)
                .Select(r => (DateOnly?)r.FechaFin)
                .FirstOrDefaultAsync(cancellationToken);

            var diasMora = cuotaVencidaMasAntigua is null ? 0 : hoy.DayNumber - cuotaVencidaMasAntigua.Value.DayNumber;

            var categoria = categorias.FirstOrDefault(c => diasMora >= c.DiasMoraInicio && diasMora <= c.DiasMoraFin)
                ?? categorias[^1];

            var provisionPrestamo = prestamo.Saldo * categoria.PorcentajeProvision;
            totalRequerido += provisionPrestamo;

            var acumulado = acumuladoPorCategoria.TryGetValue(categoria.Codigo, out var existente)
                ? existente
                : (Cantidad: 0, Saldo: 0m, Provision: 0m);
            acumuladoPorCategoria[categoria.Codigo] =
                (acumulado.Cantidad + 1, acumulado.Saldo + prestamo.Saldo, acumulado.Provision + provisionPrestamo);
        }

        var periodo = new DateOnly(hoy.Year, hoy.Month, 1);
        var idCuentaProvision = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaProvision).Select(c => c.Id).FirstAsync(cancellationToken);
        var provisionAcumuladaAnterior = await db.SaldosContables
            .Where(s => s.IdCuentaContable == idCuentaProvision && s.Periodo == periodo)
            .Select(s => (decimal?)s.SaldoFinal)
            .FirstOrDefaultAsync(cancellationToken) ?? 0m;

        var incremento = totalRequerido - provisionAcumuladaAnterior;

        Guid? idComprobante = null;
        if (incremento > 0)
        {
            var tipoTransaccion = await db.TiposTransaccion
                .FirstAsync(t => t.Codigo == CodigoTipoTransaccionProvision, cancellationToken);

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    hoy,
                    tipoTransaccion.IdTipoComprobante,
                    IdAgenciaDefault,
                    $"Constitución de provisión de cartera — {hoy:yyyy-MM}",
                    registradoPor,
                    [
                        new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, incremento, 0, "Provisión de cartera"),
                        new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, incremento, "Provisión de cartera"),
                    ]),
                cancellationToken);

            idComprobante = resultadoComprobante.Id;
        }

        var detalle = categorias
            .Where(c => acumuladoPorCategoria.ContainsKey(c.Codigo))
            .Select(c =>
            {
                var a = acumuladoPorCategoria[c.Codigo];
                return new DetalleCategoriaProvision(c.Codigo, c.Nombre, a.Cantidad, a.Saldo, a.Provision);
            })
            .ToList();

        return new ProvisionCarteraCalculadaResult(
            totalRequerido, provisionAcumuladaAnterior, Math.Max(incremento, 0), idComprobante, detalle);
    }
}
