using Corela15.Application.Contabilidad;
using Corela15.Application.CuentasPorCobrar;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Motor real de compras/facturación de proveedores — verificado contra
/// CONTABILIDAD.COMPRAS (3.029 filas reales). Ver CLAUDE.md, "Motor de
/// Compras (factura de proveedor con cumplimiento tributario SRI)".
/// </summary>
public class ComprasService(Corela15DbContext db, IComprobanteContableService comprobantes) : IComprasService
{
    private const string CodigoCuentaIva = "199005"; // Impuesto al valor agregado – IVA
    private const string CodigoCuentaRetenciones = "250405"; // Retenciones fiscales
    private const string CodigoCuentaProveedores = "2506"; // Proveedores
    private const string CodigoCuentaCaja = "1101";
    private const int IdTipoComprobanteDiario = 3; // 'DIA'

    public async Task<CompraRegistradaResult> RegistrarAsync(
        RegistrarCompraRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Detalle.Count == 0)
        {
            throw new CompraSinDetalleException();
        }

        var proveedorValido = await db.Proveedores.AnyAsync(p => p.Id == request.IdProveedor && p.Activo, cancellationToken);
        if (!proveedorValido)
        {
            throw new ProveedorInvalidoException(request.IdProveedor);
        }

        var tipoComprobanteValido = await db.TiposComprobanteCompra
            .AnyAsync(t => t.Codigo == request.CodigoTipoComprobante && t.Activo, cancellationToken);
        if (!tipoComprobanteValido)
        {
            throw new TipoComprobanteCompraInvalidoException(request.CodigoTipoComprobante);
        }

        var lineas = new List<CompraDetalle>();
        decimal subtotalTotal = 0, ivaTotal = 0;
        foreach (var linea in request.Detalle)
        {
            if (linea.Cantidad <= 0 || linea.ValorUnitario <= 0)
            {
                throw new LineaCompraInvalidaException($"Cantidad y valor unitario de '{linea.Detalle}' deben ser mayores a cero");
            }

            var cuentaValida = await db.CuentasContables
                .AnyAsync(c => c.Id == linea.IdCuentaContable && c.Activa && c.EsMayor, cancellationToken);
            if (!cuentaValida)
            {
                throw new LineaCompraInvalidaException($"La cuenta contable de la línea '{linea.Detalle}' no existe, está inactiva, o no es una cuenta de detalle");
            }

            var subtotalLinea = Math.Round(linea.Cantidad * linea.ValorUnitario, 2);
            var ivaLinea = Math.Round(subtotalLinea * linea.PorcentajeIva, 2);
            subtotalTotal += subtotalLinea;
            ivaTotal += ivaLinea;

            lineas.Add(new CompraDetalle
            {
                Id = Guid.NewGuid(),
                IdCuentaContable = linea.IdCuentaContable,
                Detalle = linea.Detalle,
                Cantidad = linea.Cantidad,
                ValorUnitario = linea.ValorUnitario,
                PorcentajeIva = linea.PorcentajeIva,
                Subtotal = subtotalLinea,
                MontoIva = ivaLinea,
                Total = subtotalLinea + ivaLinea,
            });
        }

        var total = subtotalTotal + ivaTotal;
        if (request.MontoRetencion < 0 || request.MontoRetencion > total)
        {
            throw new RetencionCompraInvalidaException(request.MontoRetencion, total);
        }

        var (idCuentaIva, idCuentaRetenciones, idCuentaProveedores) = await ResolverCuentasAsync(cancellationToken);

        var numero = "COM" + ((await db.Compras.CountAsync(cancellationToken)) + 1).ToString().PadLeft(9, '0');
        var fechaRegistro = DateOnly.FromDateTime(DateTime.UtcNow);
        var saldoInicial = total - request.MontoRetencion;

        var compra = new Compra
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            IdProveedor = request.IdProveedor,
            IdAgencia = request.IdAgencia,
            CodigoSustento = request.CodigoSustento,
            CodigoTipoComprobante = request.CodigoTipoComprobante,
            Establecimiento = request.Establecimiento,
            PuntoEmision = request.PuntoEmision,
            Secuencial = request.Secuencial,
            Autorizacion = request.Autorizacion,
            FechaEmision = request.FechaEmision,
            Concepto = request.Concepto,
            Subtotal = subtotalTotal,
            MontoIva = ivaTotal,
            MontoRetencion = request.MontoRetencion,
            Total = total,
            MontoInicial = saldoInicial,
            Saldo = saldoInicial,
            Estado = EstadoCompra.Procesada,
            FechaRegistro = fechaRegistro,
            RegistradoPor = request.RegistradoPor,
            Detalle = lineas,
        };

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var descripcion = $"Compra {numero} — {request.Concepto}";
        var lineasComprobante = new List<LineaMovimientoRequest>();
        foreach (var linea in lineas)
        {
            lineasComprobante.Add(new LineaMovimientoRequest(linea.IdCuentaContable, linea.Subtotal, 0, linea.Detalle));
        }
        if (ivaTotal > 0)
        {
            lineasComprobante.Add(new LineaMovimientoRequest(idCuentaIva, ivaTotal, 0, descripcion));
        }
        if (request.MontoRetencion > 0)
        {
            lineasComprobante.Add(new LineaMovimientoRequest(idCuentaRetenciones, 0, request.MontoRetencion, descripcion));
        }
        lineasComprobante.Add(new LineaMovimientoRequest(idCuentaProveedores, 0, saldoInicial, descripcion));

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                fechaRegistro, IdTipoComprobanteDiario, request.IdAgencia, descripcion, request.RegistradoPor, lineasComprobante),
            cancellationToken);

        compra.IdComprobante = resultadoComprobante.Id;
        db.Compras.Add(compra);
        await db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new CompraRegistradaResult(compra.Id, numero, subtotalTotal, ivaTotal, total, saldoInicial, resultadoComprobante.Id);
    }

    public async Task<PagoCompraRegistradoResult> PagarAsync(
        PagarCompraRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Monto <= 0)
        {
            throw new LineaCompraInvalidaException("El monto del pago debe ser mayor a cero");
        }

        var formaCancelacion = await db.FormasCancelacion
            .FirstOrDefaultAsync(f => f.Codigo == request.CodigoFormaCancelacion && f.Activo, cancellationToken);
        if (formaCancelacion is null)
        {
            throw new FormaCancelacionInvalidaException(request.CodigoFormaCancelacion);
        }

        var compra = await db.Compras.FirstOrDefaultAsync(c => c.Id == request.IdCompra, cancellationToken);
        if (compra is null || compra.Estado != EstadoCompra.Procesada)
        {
            throw new CompraInvalidaException(request.IdCompra);
        }

        if (request.Monto > compra.Saldo)
        {
            throw new PagoCompraExcedeSaldoException(request.Monto, compra.Saldo);
        }

        var (_, _, idCuentaProveedores) = await ResolverCuentasAsync(cancellationToken);
        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Falta la cuenta contable {CodigoCuentaCaja}.");

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        compra.Saldo -= request.Monto;
        var cancelada = compra.Saldo == 0;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Corela15.Application.Common.ConflictoConcurrenciaException($"La compra {compra.Numero}");
        }

        var descripcion = $"Pago compra {compra.Numero} ({formaCancelacion.Nombre})";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, compra.IdAgencia, descripcion, request.RegistradoPor,
                [
                    new LineaMovimientoRequest(idCuentaProveedores, request.Monto, 0, descripcion),
                    new LineaMovimientoRequest(idCuentaCaja, 0, request.Monto, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new PagoCompraRegistradoResult(compra.Saldo, cancelada, resultadoComprobante.Id);
    }

    public async Task<CompraAnuladaResult> AnularAsync(AnularCompraRequest request, CancellationToken cancellationToken = default)
    {
        var compra = await db.Compras.Include(c => c.Detalle)
            .FirstOrDefaultAsync(c => c.Id == request.IdCompra, cancellationToken);
        if (compra is null || compra.Estado != EstadoCompra.Procesada)
        {
            throw new CompraInvalidaException(request.IdCompra);
        }

        if (compra.Saldo != compra.MontoInicial)
        {
            throw new CompraConPagosException(request.IdCompra);
        }

        var (idCuentaIva, idCuentaRetenciones, idCuentaProveedores) = await ResolverCuentasAsync(cancellationToken);

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        compra.Estado = EstadoCompra.Anulada;
        compra.Saldo = 0;

        var descripcion = $"Anulación compra {compra.Numero} ({request.Motivo})";
        var lineasComprobante = new List<LineaMovimientoRequest>();
        foreach (var linea in compra.Detalle)
        {
            lineasComprobante.Add(new LineaMovimientoRequest(linea.IdCuentaContable, 0, linea.Subtotal, descripcion));
        }
        if (compra.MontoIva > 0)
        {
            lineasComprobante.Add(new LineaMovimientoRequest(idCuentaIva, 0, compra.MontoIva, descripcion));
        }
        if (compra.MontoRetencion > 0)
        {
            lineasComprobante.Add(new LineaMovimientoRequest(idCuentaRetenciones, compra.MontoRetencion, 0, descripcion));
        }
        lineasComprobante.Add(new LineaMovimientoRequest(idCuentaProveedores, compra.MontoInicial, 0, descripcion));

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, compra.IdAgencia, descripcion, request.RegistradoPor,
                lineasComprobante),
            cancellationToken);

        compra.IdComprobanteReverso = resultadoComprobante.Id;
        await db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new CompraAnuladaResult(resultadoComprobante.Id);
    }

    private async Task<(Guid IdCuentaIva, Guid IdCuentaRetenciones, Guid IdCuentaProveedores)> ResolverCuentasAsync(
        CancellationToken cancellationToken)
    {
        var idCuentaIva = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaIva).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaRetenciones = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaRetenciones).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaProveedores = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaProveedores).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaIva is null || idCuentaRetenciones is null || idCuentaProveedores is null)
        {
            throw new InvalidOperationException(
                $"Faltan las cuentas contables {CodigoCuentaIva}/{CodigoCuentaRetenciones}/{CodigoCuentaProveedores}.");
        }

        return (idCuentaIva.Value, idCuentaRetenciones.Value, idCuentaProveedores.Value);
    }
}
