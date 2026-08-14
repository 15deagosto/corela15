using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ComprobanteContableService(Corela15DbContext db) : IComprobanteContableService
{
    private const int MaxReintentosPorConcurrencia = 3;

    public async Task<ComprobanteContableRegistradoResult> RegistrarAsync(
        RegistrarComprobanteContableRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lineas.Count < 2)
        {
            throw new ComprobanteLineasInsuficientesException();
        }

        var totalDebitos = request.Lineas.Sum(l => l.Debito);
        var totalCreditos = request.Lineas.Sum(l => l.Credito);
        if (totalDebitos != totalCreditos)
        {
            throw new ComprobanteDesbalanceadoException(totalDebitos, totalCreditos);
        }

        var periodoSolicitado = new DateOnly(request.Fecha.Year, request.Fecha.Month, 1);
        var periodoCerrado = await db.PeriodosContables
            .AnyAsync(p => p.Periodo == periodoSolicitado && p.Cerrado, cancellationToken);
        if (periodoCerrado)
        {
            throw new PeriodoContableCerradoException(periodoSolicitado);
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

        // Atomicidad real con el que llama: si ya hay una transacción ambiente
        // (ej. CuentaAhorroService abriendo una cuenta con depósito inicial),
        // este método NO abre la suya propia — se suma a la del caller, así
        // que si el comprobante falla, la operación de dominio también se
        // revierte. Si se llama de forma standalone (ej. el endpoint
        // POST /api/contabilidad/comprobantes), sí gestiona su propia
        // transacción como antes.
        var transaccionPropia = db.Database.CurrentTransaction is null;
        var transaccion = transaccionPropia
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

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

        // saldo_contable es el punto de mayor contención del sistema —
        // toda cuenta/período puede recibir comprobantes concurrentes
        // legítimos (dos depósitos distintos a la misma hora). Con xmin
        // como token de concurrencia, un choque real ya no se pierde en
        // silencio: se reintenta desde cero (soltando por completo el
        // tracking de las filas de saldo tocadas, sean nuevas o existentes,
        // para no aplicar el delta dos veces sobre una fila que nunca
        // llegó a fallar) hasta MaxReintentosPorConcurrencia veces antes
        // de rendirse con 409.
        for (var intento = 1; ; intento++)
        {
            await ActualizarSaldosAsync(request, cuentas, cancellationToken);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                break;
            }
            catch (DbUpdateConcurrencyException) when (intento < MaxReintentosPorConcurrencia)
            {
                foreach (var entry in db.ChangeTracker.Entries<SaldoContable>().ToList())
                {
                    entry.State = EntityState.Detached;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictoConcurrenciaException("El saldo contable de una de las cuentas afectadas");
            }
        }

        if (transaccion is not null)
        {
            await transaccion.CommitAsync(cancellationToken);
            await transaccion.DisposeAsync();
        }

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
            // Sin duplicados dentro de una misma pasada (GroupBy por
            // cuenta) y siempre destrackeado por completo entre
            // reintentos (ver RegistrarAsync) — cada pasada consulta la
            // fila real más reciente, nunca reutiliza estado a medio
            // aplicar de un intento anterior.
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
