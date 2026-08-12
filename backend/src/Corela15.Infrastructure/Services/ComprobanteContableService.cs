using Corela15.Application.Contabilidad;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ComprobanteContableService(Corela15DbContext db) : IComprobanteContableService
{
    public async Task<ComprobanteContableRegistradoResult> RegistrarAsync(
        RegistrarComprobanteContableRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lineas.Count < 2)
        {
            throw new ArgumentException("Un comprobante necesita al menos dos líneas (débito y crédito).");
        }

        var totalDebitos = request.Lineas.Sum(l => l.Debito);
        var totalCreditos = request.Lineas.Sum(l => l.Credito);
        if (totalDebitos != totalCreditos)
        {
            throw new ComprobanteDesbalanceadoException(totalDebitos, totalCreditos);
        }

        var idsCuenta = request.Lineas.Select(l => l.IdCuentaContable).Distinct().ToList();
        var cuentas = await db.CuentasContables
            .Where(c => idsCuenta.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        foreach (var idCuenta in idsCuenta)
        {
            if (!cuentas.TryGetValue(idCuenta, out var cuenta) || !cuenta.Activa || !cuenta.EsMayor)
            {
                throw new CuentaContableInvalidaException(idCuenta);
            }
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        // Numeración por tipo de comprobante (talonario propio por tipo, ver
        // ContabilidadConfigurations.cs). Bajo concurrencia alta esto podría
        // colisionar — aceptable por ahora, revisar si se vuelve un problema real.
        var ultimoNumero = await db.ComprobantesContables
            .Where(c => c.IdTipoComprobante == request.IdTipoComprobante)
            .OrderByDescending(c => c.Numero)
            .Select(c => (long?)c.Numero)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

        var comprobante = new ComprobanteContable
        {
            Id = Guid.NewGuid(),
            Numero = ultimoNumero + 1,
            Fecha = request.Fecha,
            IdTipoComprobante = request.IdTipoComprobante,
            IdAgencia = request.IdAgencia,
            Descripcion = request.Descripcion,
            Estado = EstadoComprobante.Registrado,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };

        var numeroLinea = 1;
        foreach (var linea in request.Lineas)
        {
            comprobante.Movimientos.Add(new MovimientoComprobanteContable
            {
                Id = Guid.NewGuid(),
                IdComprobante = comprobante.Id,
                IdCuentaContable = linea.IdCuentaContable,
                NumeroLinea = numeroLinea++,
                Debito = linea.Debito,
                Credito = linea.Credito,
                Descripcion = linea.Descripcion,
            });
        }

        db.ComprobantesContables.Add(comprobante);

        await ActualizarSaldosAsync(request, cuentas, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);

        return new ComprobanteContableRegistradoResult(comprobante.Id, comprobante.Numero);
    }

    private async Task ActualizarSaldosAsync(
        RegistrarComprobanteContableRequest request,
        Dictionary<Guid, CuentaContable> cuentas,
        CancellationToken cancellationToken)
    {
        var periodo = new DateOnly(request.Fecha.Year, request.Fecha.Month, 1);

        var totalesPorCuenta = request.Lineas
            .GroupBy(l => l.IdCuentaContable)
            .Select(g => new { IdCuentaContable = g.Key, Debitos = g.Sum(l => l.Debito), Creditos = g.Sum(l => l.Credito) });

        foreach (var t in totalesPorCuenta)
        {
            var saldo = await db.SaldosContables.FirstOrDefaultAsync(
                s => s.IdCuentaContable == t.IdCuentaContable && s.Periodo == periodo, cancellationToken);

            if (saldo is null)
            {
                saldo = new SaldoContable
                {
                    Id = Guid.NewGuid(),
                    IdCuentaContable = t.IdCuentaContable,
                    Periodo = periodo,
                };
                db.SaldosContables.Add(saldo);
            }

            saldo.TotalDebitos += t.Debitos;
            saldo.TotalCreditos += t.Creditos;

            var naturaleza = cuentas[t.IdCuentaContable].Naturaleza;
            saldo.SaldoFinal = naturaleza == NaturalezaCuenta.Deudora
                ? saldo.TotalDebitos - saldo.TotalCreditos
                : saldo.TotalCreditos - saldo.TotalDebitos;
        }
    }
}
