using Corela15.Application.Colocacion;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class AutoDebitoSpiService(Corela15DbContext db, IComprobanteContableService comprobantes) : IAutoDebitoSpiService
{
    private const string CodigoItemSaldoDisponible = "DISP";
    private const string CodigoCuentaDepositos = "2101";
    private const string CodigoCuentaCartera = "1401";
    private const string CodigoCuentaInteresesGanados = "5101";
    private const int IdAgenciaDefault = 1;
    private const int IdTipoComprobanteDiario = 3; // 'DIA' — el asiento tiene 3 líneas, no cabe en el motor TipoTransaccion (1 débito/1 crédito), mismo criterio que PagarCuotaAsync.

    public async Task ConfigurarAsync(
        Guid idPrestamo, bool activar, string registradoPor, CancellationToken cancellationToken = default)
    {
        var prestamo = await db.Prestamos.FirstOrDefaultAsync(p => p.Id == idPrestamo, cancellationToken);
        if (prestamo is null || prestamo.Estado != EstadoPrestamo.Vigente)
        {
            throw new PrestamoInvalidoParaDebitoSpiException(idPrestamo);
        }

        prestamo.DebitoSpi = activar;
        prestamo.ModificadoEn = DateTimeOffset.UtcNow;
        prestamo.ModificadoPor = registradoPor;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AutoDebitoSpiEjecutadoResult> EjecutarAsync(
        string registradoPor, CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var prestamosElegibles = await db.Prestamos
            .Where(p => p.Estado == EstadoPrestamo.Vigente && p.DebitoSpi)
            .Where(p => !db.AutoDebitosSpiLog.Any(l => l.Fecha == hoy && l.IdPrestamo == p.Id))
            .ToListAsync(cancellationToken);

        var detalles = new List<AutoDebitoSpiDetalle>();
        var totalCapital = 0m;
        var totalInteres = 0m;

        foreach (var prestamo in prestamosElegibles)
        {
            var rubrosCuota = await db.PrestamosRubros
                .Include(r => r.Rubro)
                .Where(r => r.IdPrestamo == prestamo.Id && r.Estado == "Pendiente")
                .OrderBy(r => r.NumeroCuota)
                .ToListAsync(cancellationToken);
            var proximoNumeroCuota = rubrosCuota.Select(r => r.NumeroCuota).DefaultIfEmpty(0).Min();
            var rubrosDeLaCuota = rubrosCuota.Where(r => r.NumeroCuota == proximoNumeroCuota).ToList();
            var rubroCapital = rubrosDeLaCuota.FirstOrDefault(r => r.Rubro.Codigo == "CAP");
            var rubroInteres = rubrosDeLaCuota.FirstOrDefault(r => r.Rubro.Codigo == "INT");

            if (rubroCapital is null || rubroInteres is null)
            {
                Registrar(prestamo, hoy, null, null, false, "Sin cuotas pendientes de pago", 0);
                detalles.Add(new AutoDebitoSpiDetalle(prestamo.Numero, null, null, false, "Sin cuotas pendientes de pago", 0));
                continue;
            }

            var montoCuota = rubroCapital.Proyectado + rubroInteres.Proyectado;

            var idCliente = await db.PrestamosClientes
                .Where(pc => pc.IdPrestamo == prestamo.Id)
                .OrderByDescending(pc => pc.Principal)
                .Select(pc => (Guid?)pc.IdCliente)
                .FirstOrDefaultAsync(cancellationToken);
            if (idCliente is null)
            {
                Registrar(prestamo, hoy, proximoNumeroCuota, null, false, "El préstamo no tiene un socio asociado", montoCuota);
                detalles.Add(new AutoDebitoSpiDetalle(prestamo.Numero, null, proximoNumeroCuota, false, "El préstamo no tiene un socio asociado", montoCuota));
                continue;
            }

            // Cruce de las tres configuraciones como una sola fuente de
            // verdad — el bug real del incidente original: el proceso
            // legado solo miraba Prestamo.DebitoSpi (ya filtrado arriba) y
            // nunca cruzaba con las otras dos.
            var cuentaElegible = await db.CuentasClientes
                .Include(cc => cc.Cuenta).ThenInclude(c => c.TipoCuenta)
                .Where(cc => cc.IdCliente == idCliente
                    && cc.Cuenta.Estado == EstadoCuenta.Activa
                    && cc.Cuenta.TipoCuenta.PermiteDebitoPrestamo)
                .Select(cc => cc.Cuenta)
                .FirstOrDefaultAsync(cancellationToken);
            if (cuentaElegible is null)
            {
                Registrar(prestamo, hoy, proximoNumeroCuota, null, false,
                    "Ninguna cuenta activa del socio tiene TipoCuenta.PermiteDebitoPrestamo=true", montoCuota);
                detalles.Add(new AutoDebitoSpiDetalle(prestamo.Numero, null, proximoNumeroCuota, false,
                    "Ninguna cuenta activa del socio tiene TipoCuenta.PermiteDebitoPrestamo=true", montoCuota));
                continue;
            }

            var itemDisponible = await db.CuentasItemSaldo
                .Include(x => x.ItemSaldo)
                .FirstOrDefaultAsync(x => x.IdCuenta == cuentaElegible.Id && x.ItemSaldo.Codigo == CodigoItemSaldoDisponible, cancellationToken);
            if (itemDisponible is null || !itemDisponible.AcreditaPrestamo)
            {
                Registrar(prestamo, hoy, proximoNumeroCuota, cuentaElegible.Id, false,
                    $"La cuenta {cuentaElegible.Numero} no tiene CuentaItemSaldo.AcreditaPrestamo=true", montoCuota);
                detalles.Add(new AutoDebitoSpiDetalle(prestamo.Numero, cuentaElegible.Numero, proximoNumeroCuota, false,
                    $"La cuenta {cuentaElegible.Numero} no tiene CuentaItemSaldo.AcreditaPrestamo=true", montoCuota));
                continue;
            }

            var saldoMinimoExigido = cuentaElegible.TipoCuenta.SaldoMinimoConPrestamo ?? cuentaElegible.TipoCuenta.SaldoMinimo;
            var saldoResultante = itemDisponible.Saldo - montoCuota;
            if (saldoResultante < saldoMinimoExigido)
            {
                var motivo = $"Saldo insuficiente en cuenta {cuentaElegible.Numero} ({itemDisponible.Saldo:0.00}) para cubrir la cuota ({montoCuota:0.00}) manteniendo el mínimo ({saldoMinimoExigido:0.00})";
                Registrar(prestamo, hoy, proximoNumeroCuota, cuentaElegible.Id, false, motivo, montoCuota);
                detalles.Add(new AutoDebitoSpiDetalle(prestamo.Numero, cuentaElegible.Numero, proximoNumeroCuota, false, motivo, montoCuota));
                continue;
            }

            itemDisponible.Saldo = saldoResultante;
            itemDisponible.ModificadoEn = DateTimeOffset.UtcNow;
            itemDisponible.ModificadoPor = registradoPor;

            rubroCapital.Cobrado = rubroCapital.Proyectado;
            rubroCapital.Estado = "Pagado";
            rubroInteres.Cobrado = rubroInteres.Proyectado;
            rubroInteres.Estado = "Pagado";

            prestamo.Saldo -= rubroCapital.Proyectado;
            var quedanPendientes = await db.PrestamosRubros
                .AnyAsync(r => r.IdPrestamo == prestamo.Id && r.Estado == "Pendiente" && r.NumeroCuota != proximoNumeroCuota,
                    cancellationToken);
            if (!quedanPendientes)
            {
                prestamo.Estado = EstadoPrestamo.Cancelado;
            }
            prestamo.ModificadoEn = DateTimeOffset.UtcNow;
            prestamo.ModificadoPor = registradoPor;

            Registrar(prestamo, hoy, proximoNumeroCuota, cuentaElegible.Id, true, "Débito aplicado", montoCuota);
            detalles.Add(new AutoDebitoSpiDetalle(prestamo.Numero, cuentaElegible.Numero, proximoNumeroCuota, true, "Débito aplicado", montoCuota));

            totalCapital += rubroCapital.Proyectado;
            totalInteres += rubroInteres.Proyectado;
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Corela15.Application.Common.ConflictoConcurrenciaException("Una o más cuentas/préstamos del lote de auto-débito SPI");
        }

        Guid? idComprobante = null;
        var totalDebitado = totalCapital + totalInteres;
        if (totalDebitado > 0)
        {
            var idCuentaDepositos = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaDepositos).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var idCuentaCartera = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaCartera).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var idCuentaIntereses = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaInteresesGanados).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            if (idCuentaDepositos is null || idCuentaCartera is null || idCuentaIntereses is null)
            {
                throw new InvalidOperationException(
                    $"Faltan cuentas contables {CodigoCuentaDepositos}/{CodigoCuentaCartera}/{CodigoCuentaInteresesGanados}.");
            }

            var lineas = new List<LineaMovimientoRequest>
            {
                new(idCuentaDepositos.Value, totalDebitado, 0, $"Auto-débito SPI de cuotas — {hoy:yyyy-MM-dd}"),
            };
            if (totalCapital > 0)
            {
                lineas.Add(new LineaMovimientoRequest(idCuentaCartera.Value, 0, totalCapital, "Abono a capital"));
            }
            if (totalInteres > 0)
            {
                lineas.Add(new LineaMovimientoRequest(idCuentaIntereses.Value, 0, totalInteres, "Interés cobrado"));
            }

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    hoy, IdTipoComprobanteDiario, IdAgenciaDefault,
                    $"Auto-débito SPI de cuotas — {hoy:yyyy-MM-dd}", registradoPor, lineas),
                cancellationToken);

            idComprobante = resultadoComprobante.Id;
        }

        return new AutoDebitoSpiEjecutadoResult(
            hoy, detalles.Count(d => d.Debitado), detalles.Count(d => !d.Debitado), totalDebitado, idComprobante, detalles);
    }

    public async Task<IReadOnlyList<AutoDebitoSpiDetalle>> HistorialAsync(CancellationToken cancellationToken = default)
    {
        return await db.AutoDebitosSpiLog
            .Include(l => l.Prestamo)
            .OrderByDescending(l => l.Fecha).ThenBy(l => l.Prestamo.Numero)
            .Select(l => new AutoDebitoSpiDetalle(l.Prestamo.Numero, null, l.NumeroCuota, l.Debitado, l.Motivo, l.Monto))
            .ToListAsync(cancellationToken);
    }

    private void Registrar(
        Prestamo prestamo, DateOnly fecha, int? numeroCuota, Guid? idCuenta, bool debitado, string motivo, decimal monto)
    {
        db.AutoDebitosSpiLog.Add(new AutoDebitoSpiLog
        {
            Id = Guid.NewGuid(),
            Fecha = fecha,
            IdPrestamo = prestamo.Id,
            NumeroCuota = numeroCuota,
            IdCuenta = idCuenta,
            Debitado = debitado,
            Motivo = motivo,
            Monto = monto,
        });
    }
}
