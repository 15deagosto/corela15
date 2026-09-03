using Corela15.Application.Cajas;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Cajas;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Motor real de recaudación de servicios de terceros (cobro de
/// institución externa) — verificado contra CAJAS.PAGO_EXTERNO_TRANSACCION
/// (ver CLAUDE.md, "Cajas: chequera/caja chica/pago externo — gaps
/// operativos reales"). No modela la pasarela/switch de pago externo en
/// sí (CAJAS.PAGO_EXTERNO real trae credenciales de un gateway externo,
/// URL/usuario/clave — fuera de alcance sin una integración real), solo
/// el registro real del cobro recibido en ventanilla y su asiento.
/// </summary>
public class PagoExternoService(Corela15DbContext db, IComprobanteContableService comprobantes) : IPagoExternoService
{
    private const string CodigoCuentaCaja = "1101";
    private const string CodigoCuentaRecaudaciones = "2303"; // Recaudaciones para el sector público
    private const string CodigoCuentaComision = "5290"; // Comisiones ganadas — Otras
    private const int IdTipoComprobanteDiario = 3; // 'DIA'

    public async Task<PagoExternoRegistradoResult> RegistrarAsync(
        RegistrarPagoExternoRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Valor <= 0)
        {
            throw new ValorPagoExternoInvalidoException(request.Valor);
        }

        if (request.Comision < 0 || request.Comision > request.Valor)
        {
            throw new ComisionPagoExternoInvalidaException(request.Comision, request.Valor);
        }

        var productoValido = await db.PagoExternoProductos
            .AnyAsync(p => p.Id == request.IdProducto && p.Activo, cancellationToken);
        if (!productoValido)
        {
            throw new PagoExternoProductoInvalidoException(request.IdProducto);
        }

        var (idCuentaCaja, idCuentaRecaudaciones, idCuentaComision) = await ResolverCuentasAsync(cancellationToken);

        var transaccion = new PagoExternoTransaccion
        {
            Id = Guid.NewGuid(),
            IdProducto = request.IdProducto,
            Referencia = request.Referencia,
            Documento = request.Documento,
            Valor = request.Valor,
            Comision = request.Comision,
            IdAgencia = request.IdAgencia,
            FechaProceso = DateTimeOffset.UtcNow,
            RegistradoPor = request.RegistradoPor,
        };

        await using var dbTransaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var descripcion = $"Pago externo — {request.Referencia}";
        var lineas = new List<LineaMovimientoRequest>
        {
            new(idCuentaCaja, request.Valor, 0, descripcion),
            new(idCuentaRecaudaciones, 0, request.Valor - request.Comision, descripcion),
        };
        if (request.Comision > 0)
        {
            lineas.Add(new LineaMovimientoRequest(idCuentaComision, 0, request.Comision, descripcion));
        }

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, request.IdAgencia, descripcion,
                request.RegistradoPor, lineas),
            cancellationToken);

        transaccion.IdComprobante = resultadoComprobante.Id;
        db.PagoExternoTransacciones.Add(transaccion);
        await db.SaveChangesAsync(cancellationToken);

        await dbTransaccion.CommitAsync(cancellationToken);

        return new PagoExternoRegistradoResult(transaccion.Id, resultadoComprobante.Id);
    }

    public async Task<PagoExternoReversadoResult> ReversarAsync(
        ReversarPagoExternoRequest request, CancellationToken cancellationToken = default)
    {
        var transaccion = await db.PagoExternoTransacciones
            .FirstOrDefaultAsync(t => t.Id == request.IdTransaccion, cancellationToken);
        if (transaccion is null || transaccion.Reversada)
        {
            throw new PagoExternoTransaccionInvalidaException(request.IdTransaccion);
        }

        var (idCuentaCaja, idCuentaRecaudaciones, idCuentaComision) = await ResolverCuentasAsync(cancellationToken);

        await using var dbTransaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        transaccion.Reversada = true;
        transaccion.FechaReverso = DateTimeOffset.UtcNow;
        transaccion.ReversadaPor = request.RegistradoPor;

        var descripcion = $"Reverso pago externo — {transaccion.Referencia}";
        var lineas = new List<LineaMovimientoRequest>
        {
            new(idCuentaRecaudaciones, transaccion.Valor - transaccion.Comision, 0, descripcion),
            new(idCuentaCaja, 0, transaccion.Valor, descripcion),
        };
        if (transaccion.Comision > 0)
        {
            lineas.Insert(1, new LineaMovimientoRequest(idCuentaComision, transaccion.Comision, 0, descripcion));
        }

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, transaccion.IdAgencia, descripcion,
                request.RegistradoPor, lineas),
            cancellationToken);

        transaccion.IdComprobanteReverso = resultadoComprobante.Id;
        await db.SaveChangesAsync(cancellationToken);

        await dbTransaccion.CommitAsync(cancellationToken);

        return new PagoExternoReversadoResult(resultadoComprobante.Id);
    }

    private async Task<(Guid IdCuentaCaja, Guid IdCuentaRecaudaciones, Guid IdCuentaComision)> ResolverCuentasAsync(
        CancellationToken cancellationToken)
    {
        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaRecaudaciones = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaRecaudaciones).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaComision = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaComision).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaCaja is null || idCuentaRecaudaciones is null || idCuentaComision is null)
        {
            throw new InvalidOperationException(
                $"Faltan las cuentas contables {CodigoCuentaCaja}/{CodigoCuentaRecaudaciones}/{CodigoCuentaComision}.");
        }

        return (idCuentaCaja.Value, idCuentaRecaudaciones.Value, idCuentaComision.Value);
    }
}
