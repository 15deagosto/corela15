using Corela15.Application.Ahorros;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Application.CuentasPorCobrar;
using Corela15.Domain.Colocacion;
using Corela15.Domain.CuentasPorCobrar;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CuentaPorCobrarService(Corela15DbContext db, IComprobanteContableService comprobantes) : ICuentaPorCobrarService
{
    private const string CodigoTipoTransaccionRegistro = "REG-CXC";
    private const string CodigoTipoTransaccionAbono = "ABONO-CXC";

    public async Task<CuentaPorCobrarRegistradaResult> RegistrarAsync(
        RegistrarCuentaPorCobrarRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MontoInicial <= 0)
        {
            throw new MontoCuentaPorCobrarInvalidoException(request.MontoInicial);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionRegistro, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionRegistro);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var fechaCreacion = DateOnly.FromDateTime(DateTime.UtcNow);
        var cuentaPorCobrar = new CuentaPorCobrar
        {
            Id = Guid.NewGuid(),
            Concepto = request.Concepto,
            IdAgencia = request.IdAgencia,
            IdPersona = request.IdPersona,
            Cuotas = request.Cuotas,
            MontoInicial = request.MontoInicial,
            Saldo = request.MontoInicial,
            FechaCreacion = fechaCreacion,
            FechaVencimiento = request.FechaVencimiento,
            Estado = EstadoCuentaPorCobrar.Vigente,
        };
        db.CuentasPorCobrar.Add(cuentaPorCobrar);
        await db.SaveChangesAsync(cancellationToken);

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                fechaCreacion,
                tipoTransaccion.IdTipoComprobante,
                request.IdAgencia,
                $"Registro CxC — {request.Concepto}",
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, request.MontoInicial, 0, request.Concepto),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, request.MontoInicial, request.Concepto),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new CuentaPorCobrarRegistradaResult(cuentaPorCobrar.Id, resultadoComprobante.Id);
    }

    public async Task<AbonoCuentaPorCobrarRegistradoResult> AbonarAsync(
        AbonarCuentaPorCobrarRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Monto <= 0)
        {
            throw new MontoCuentaPorCobrarInvalidoException(request.Monto);
        }

        var cuentaPorCobrar = await db.CuentasPorCobrar
            .FirstOrDefaultAsync(c => c.Id == request.IdCuentaPorCobrar, cancellationToken);
        if (cuentaPorCobrar is null || cuentaPorCobrar.Estado != EstadoCuentaPorCobrar.Vigente)
        {
            throw new CuentaPorCobrarInvalidaException(request.IdCuentaPorCobrar);
        }

        if (request.Monto > cuentaPorCobrar.Saldo)
        {
            throw new AbonoExcedeSaldoException(request.Monto, cuentaPorCobrar.Saldo);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionAbono, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionAbono);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        cuentaPorCobrar.Saldo -= request.Monto;
        var cancelada = cuentaPorCobrar.Saldo == 0;
        if (cancelada)
        {
            cuentaPorCobrar.Estado = EstadoCuentaPorCobrar.Cancelada;
        }

        // Si esta CxC nació de un rubro manual cargado a un préstamo (ver
        // PrestamoService.CargarRubroManualAsync), el abono también debe
        // reflejarse en el PrestamoRubro real — sin esto, la cartera de
        // Créditos seguiría mostrando el cargo como pendiente aunque ya se
        // cobró desde Tesorería. Mismo criterio real de Softbank: ambas
        // vistas (Colocación y CxC) del mismo cargo real.
        var prestamoRubro = await db.PrestamosRubrosCuentasPorCobrar
            .Where(b => b.IdCuentaPorCobrar == cuentaPorCobrar.Id)
            .Select(b => b.PrestamoRubro)
            .FirstOrDefaultAsync(cancellationToken);
        if (prestamoRubro is not null)
        {
            prestamoRubro.Cobrado += request.Monto;
            prestamoRubro.Estado = cancelada ? "C" : "P";
            if (cancelada) prestamoRubro.FechaCobro = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"La cuenta por cobrar {cuentaPorCobrar.Concepto}");
        }

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow),
                tipoTransaccion.IdTipoComprobante,
                cuentaPorCobrar.IdAgencia,
                $"Abono CxC — {cuentaPorCobrar.Concepto}",
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, request.Monto, 0, cuentaPorCobrar.Concepto),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, request.Monto, cuentaPorCobrar.Concepto),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new AbonoCuentaPorCobrarRegistradoResult(cuentaPorCobrar.Saldo, cancelada, resultadoComprobante.Id);
    }
}
