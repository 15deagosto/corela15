using Corela15.Application.Ahorros;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Application.CuentasPorCobrar;
using Corela15.Domain.CuentasPorCobrar;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CuentaPorPagarService(Corela15DbContext db, IComprobanteContableService comprobantes) : ICuentaPorPagarService
{
    private const string CodigoTipoTransaccionRegistro = "REG-CXP";
    private const string CodigoTipoTransaccionPago = "PAGO-CXP";
    private const string CodigoTipoTransaccionAnulacion = "ANULA-CXP";

    public async Task<CuentaPorPagarRegistradaResult> RegistrarAsync(
        RegistrarCuentaPorPagarRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MontoInicial <= 0)
        {
            throw new MontoCuentaPorPagarInvalidoException(request.MontoInicial);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionRegistro, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionRegistro);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var fechaCreacion = DateOnly.FromDateTime(DateTime.UtcNow);
        var cuentaPorPagar = new CuentaPorPagar
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
            Estado = EstadoCuentaPorPagar.Vigente,
        };
        db.CuentasPorPagar.Add(cuentaPorPagar);
        await db.SaveChangesAsync(cancellationToken);

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                fechaCreacion,
                tipoTransaccion.IdTipoComprobante,
                request.IdAgencia,
                $"Registro CxP — {request.Concepto}",
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, request.MontoInicial, 0, request.Concepto),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, request.MontoInicial, request.Concepto),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new CuentaPorPagarRegistradaResult(cuentaPorPagar.Id, resultadoComprobante.Id);
    }

    public async Task<PagoCuentaPorPagarRegistradoResult> PagarAsync(
        PagarCuentaPorPagarRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Monto <= 0)
        {
            throw new MontoCuentaPorPagarInvalidoException(request.Monto);
        }

        var formaCancelacion = await db.FormasCancelacion
            .FirstOrDefaultAsync(f => f.Codigo == request.CodigoFormaCancelacion && f.Activo, cancellationToken);
        if (formaCancelacion is null)
        {
            throw new FormaCancelacionInvalidaException(request.CodigoFormaCancelacion);
        }

        var cuentaPorPagar = await db.CuentasPorPagar
            .FirstOrDefaultAsync(c => c.Id == request.IdCuentaPorPagar, cancellationToken);
        if (cuentaPorPagar is null || cuentaPorPagar.Estado != EstadoCuentaPorPagar.Vigente)
        {
            throw new CuentaPorPagarInvalidaException(request.IdCuentaPorPagar);
        }

        if (request.Monto > cuentaPorPagar.Saldo)
        {
            throw new PagoCuentaPorPagarExcedeSaldoException(request.Monto, cuentaPorPagar.Saldo);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionPago, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionPago);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        cuentaPorPagar.Saldo -= request.Monto;
        var cancelada = cuentaPorPagar.Saldo == 0;
        if (cancelada)
        {
            cuentaPorPagar.Estado = EstadoCuentaPorPagar.Cancelada;
        }
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"La cuenta por pagar {cuentaPorPagar.Concepto}");
        }

        var descripcion = $"Pago CxP ({formaCancelacion.Nombre}) — {cuentaPorPagar.Concepto}";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow),
                tipoTransaccion.IdTipoComprobante,
                cuentaPorPagar.IdAgencia,
                descripcion,
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, request.Monto, 0, descripcion),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, request.Monto, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new PagoCuentaPorPagarRegistradoResult(cuentaPorPagar.Saldo, cancelada, resultadoComprobante.Id);
    }

    public async Task<CuentaPorPagarAnuladaResult> AnularAsync(AnularCuentaPorPagarRequest request, CancellationToken cancellationToken = default)
    {
        var cuentaPorPagar = await db.CuentasPorPagar
            .FirstOrDefaultAsync(c => c.Id == request.IdCuentaPorPagar, cancellationToken);
        if (cuentaPorPagar is null || cuentaPorPagar.Estado != EstadoCuentaPorPagar.Vigente)
        {
            throw new CuentaPorPagarInvalidaException(request.IdCuentaPorPagar);
        }

        if (cuentaPorPagar.Saldo != cuentaPorPagar.MontoInicial)
        {
            throw new CuentaPorPagarConPagosException(request.IdCuentaPorPagar);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionAnulacion, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionAnulacion);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        // Reversa real del asiento de registro (REG-CXP) — sin esto, el
        // pasivo/gasto quedaría posteado para siempre pese a que la cuenta
        // nunca correspondió a una obligación real.
        cuentaPorPagar.Estado = EstadoCuentaPorPagar.Anulada;
        cuentaPorPagar.Saldo = 0;
        await db.SaveChangesAsync(cancellationToken);

        var descripcion = $"Anulación CxP — {cuentaPorPagar.Concepto} ({request.Motivo})";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow),
                tipoTransaccion.IdTipoComprobante,
                cuentaPorPagar.IdAgencia,
                descripcion,
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, cuentaPorPagar.MontoInicial, 0, descripcion),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, cuentaPorPagar.MontoInicial, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new CuentaPorPagarAnuladaResult(resultadoComprobante.Id);
    }
}
