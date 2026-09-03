using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Application.Proveeduria;
using Corela15.Domain.Proveeduria;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Motor real de bodega de suministros — verificado contra el esquema
/// PROVEEDURIA completo (ver CLAUDE.md, "Proveeduria — bodega de
/// suministros real"). Ciclo: ingreso de stock por compra → solicitud de
/// pedido → procesamiento (baja de kardex + asiento contable real).
/// </summary>
public class SolicitudPedidoService(Corela15DbContext db, IComprobanteContableService comprobantes) : ISolicitudPedidoService
{
    private const string CodigoCuentaCaja = "1101";
    private const int IdTipoComprobanteDiario = 3; // 'DIA'

    public async Task<SolicitudPedidoCreadaResult> CrearAsync(
        CrearSolicitudPedidoRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lineas.Count == 0)
        {
            throw new SolicitudPedidoSinLineasException();
        }

        var bodega = await db.Bodegas.FirstOrDefaultAsync(b => b.Id == request.IdBodega && b.Activo, cancellationToken);
        if (bodega is null)
        {
            throw new BodegaInvalidaException(request.IdBodega);
        }

        foreach (var linea in request.Lineas)
        {
            if (linea.Cantidad <= 0)
            {
                throw new CantidadInvalidaException(linea.Cantidad);
            }

            var articuloValido = await db.Articulos.AnyAsync(a => a.Codigo == linea.CodigoArticulo && a.Activo, cancellationToken);
            if (!articuloValido)
            {
                throw new ArticuloInvalidoException(linea.CodigoArticulo);
            }
        }

        var solicitud = new SolicitudPedido
        {
            Id = Guid.NewGuid(),
            IdBodega = request.IdBodega,
            IdUsuarioSolicitante = request.IdUsuarioSolicitante,
            Detalle = request.Detalle,
            Estado = EstadoSolicitudPedido.Ingresada,
            FechaSistema = DateTimeOffset.UtcNow,
        };
        db.SolicitudesPedido.Add(solicitud);

        foreach (var linea in request.Lineas)
        {
            var precioActual = await db.BodegasArticulo
                .Where(ba => ba.IdBodega == request.IdBodega && ba.CodigoArticulo == linea.CodigoArticulo)
                .Select(ba => (decimal?)ba.PrecioUnitario).FirstOrDefaultAsync(cancellationToken) ?? 0m;

            db.SolicitudesPedidoArticulo.Add(new SolicitudPedidoArticulo
            {
                Id = Guid.NewGuid(),
                IdSolicitud = solicitud.Id,
                CodigoArticulo = linea.CodigoArticulo,
                Cantidad = linea.Cantidad,
                PrecioUnitario = precioActual,
                Detalle = linea.Detalle,
            });
        }

        db.SolicitudesPedidoEtapa.Add(new SolicitudPedidoEtapa
        {
            Id = Guid.NewGuid(),
            IdSolicitud = solicitud.Id,
            Estado = EstadoSolicitudPedido.Ingresada,
            RegistradoPor = request.RegistradoPor,
            FechaSistema = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new SolicitudPedidoCreadaResult(solicitud.Id);
    }

    public async Task<ProcesarSolicitudPedidoResult> ProcesarAsync(
        Guid idSolicitud, string registradoPor, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesPedido
            .Include(s => s.Bodega)
            .FirstOrDefaultAsync(s => s.Id == idSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitudPedido.Ingresada)
        {
            throw new SolicitudPedidoInvalidaException(idSolicitud, "Ingresada");
        }

        var lineas = await db.SolicitudesPedidoArticulo
            .Include(l => l.Articulo).ThenInclude(a => a.TipoArticulo)
            .Where(l => l.IdSolicitud == idSolicitud)
            .ToListAsync(cancellationToken);

        // Validación completa de stock ANTES de escribir nada — un
        // procesamiento parcial dejaría el kardex inconsistente con lo
        // que la solicitud realmente pidió.
        var stocks = new Dictionary<string, BodegaArticulo>();
        foreach (var linea in lineas)
        {
            var stock = await db.BodegasArticulo
                .FirstOrDefaultAsync(ba => ba.IdBodega == solicitud.IdBodega && ba.CodigoArticulo == linea.CodigoArticulo, cancellationToken);
            if (stock is null || stock.Cantidad < linea.Cantidad)
            {
                throw new StockInsuficienteException(linea.CodigoArticulo, stock?.Cantidad ?? 0, linea.Cantidad);
            }
            stocks[linea.CodigoArticulo] = stock;
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var acumuladoPorCuenta = new Dictionary<(Guid Gasto, Guid Activo), decimal>();
        var valorTotal = 0m;

        foreach (var linea in lineas)
        {
            var stock = stocks[linea.CodigoArticulo];
            var valorLinea = linea.Cantidad * stock.PrecioUnitario;

            stock.Cantidad -= linea.Cantidad;
            stock.ValorTotal = stock.Cantidad * stock.PrecioUnitario;

            db.BodegasArticuloMovimiento.Add(new BodegaArticuloMovimiento
            {
                Id = Guid.NewGuid(),
                IdBodegaArticulo = stock.Id,
                Cantidad = linea.Cantidad,
                Valor = valorLinea,
                SaldoResultante = stock.Cantidad,
                EsBajaArticulo = true,
                Comentario = $"Solicitud de pedido {idSolicitud}",
                FechaSistema = DateTimeOffset.UtcNow,
                RegistradoPor = registradoPor,
            });

            var clave = (linea.Articulo.TipoArticulo.IdCuentaContableGasto, linea.Articulo.TipoArticulo.IdCuentaContableActivo);
            acumuladoPorCuenta[clave] = acumuladoPorCuenta.GetValueOrDefault(clave) + valorLinea;
            valorTotal += valorLinea;
        }

        solicitud.Estado = EstadoSolicitudPedido.Procesada;
        solicitud.FechaProceso = DateTimeOffset.UtcNow;

        db.SolicitudesPedidoEtapa.Add(new SolicitudPedidoEtapa
        {
            Id = Guid.NewGuid(),
            IdSolicitud = solicitud.Id,
            Estado = EstadoSolicitudPedido.Procesada,
            RegistradoPor = registradoPor,
            FechaSistema = DateTimeOffset.UtcNow,
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El kardex de la bodega {solicitud.IdBodega}");
        }

        Guid? idComprobante = null;
        if (valorTotal > 0)
        {
            var descripcion = $"Consumo de bodega — Solicitud de pedido {idSolicitud}";
            var lineasComprobante = new List<LineaMovimientoRequest>();
            foreach (var ((idGasto, idActivo), monto) in acumuladoPorCuenta)
            {
                lineasComprobante.Add(new LineaMovimientoRequest(idGasto, monto, 0, descripcion));
                lineasComprobante.Add(new LineaMovimientoRequest(idActivo, 0, monto, descripcion));
            }

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, solicitud.Bodega.IdAgencia,
                    descripcion, registradoPor, lineasComprobante),
                cancellationToken);

            idComprobante = resultadoComprobante.Id;
        }

        await transaccion.CommitAsync(cancellationToken);

        return new ProcesarSolicitudPedidoResult(idComprobante, valorTotal);
    }

    public async Task AnularAsync(
        Guid idSolicitud, string registradoPor, string? comentario, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesPedido.FirstOrDefaultAsync(s => s.Id == idSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitudPedido.Ingresada)
        {
            throw new SolicitudPedidoInvalidaException(idSolicitud, "Ingresada");
        }

        solicitud.Estado = EstadoSolicitudPedido.Anulada;

        db.SolicitudesPedidoEtapa.Add(new SolicitudPedidoEtapa
        {
            Id = Guid.NewGuid(),
            IdSolicitud = solicitud.Id,
            Estado = EstadoSolicitudPedido.Anulada,
            Comentario = comentario,
            RegistradoPor = registradoPor,
            FechaSistema = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<StockIngresadoResult> IngresarStockAsync(
        IngresarStockRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Cantidad <= 0)
        {
            throw new CantidadInvalidaException(request.Cantidad);
        }

        var bodega = await db.Bodegas.FirstOrDefaultAsync(b => b.Id == request.IdBodega && b.Activo, cancellationToken);
        if (bodega is null)
        {
            throw new BodegaInvalidaException(request.IdBodega);
        }

        var articulo = await db.Articulos.Include(a => a.TipoArticulo)
            .FirstOrDefaultAsync(a => a.Codigo == request.CodigoArticulo && a.Activo, cancellationToken);
        if (articulo is null)
        {
            throw new ArticuloInvalidoException(request.CodigoArticulo);
        }

        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaCaja is null)
        {
            throw new InvalidOperationException($"Falta la cuenta contable {CodigoCuentaCaja}.");
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var stock = await db.BodegasArticulo
            .FirstOrDefaultAsync(ba => ba.IdBodega == request.IdBodega && ba.CodigoArticulo == request.CodigoArticulo, cancellationToken);

        var valorIngreso = request.Cantidad * request.PrecioUnitario;

        if (stock is null)
        {
            stock = new BodegaArticulo
            {
                Id = Guid.NewGuid(),
                IdBodega = request.IdBodega,
                CodigoArticulo = request.CodigoArticulo,
                Cantidad = request.Cantidad,
                PrecioUnitario = request.PrecioUnitario,
                ValorTotal = valorIngreso,
            };
            db.BodegasArticulo.Add(stock);
        }
        else
        {
            // Costo promedio ponderado real — mismo criterio contable
            // estándar que un kardex de inventario.
            var nuevaCantidad = stock.Cantidad + request.Cantidad;
            var nuevoValorTotal = stock.ValorTotal + valorIngreso;
            stock.Cantidad = nuevaCantidad;
            stock.ValorTotal = nuevoValorTotal;
            stock.PrecioUnitario = nuevaCantidad > 0 ? Math.Round(nuevoValorTotal / nuevaCantidad, 4) : 0m;
        }

        db.BodegasArticuloMovimiento.Add(new BodegaArticuloMovimiento
        {
            Id = Guid.NewGuid(),
            IdBodegaArticulo = stock.Id,
            Cantidad = request.Cantidad,
            Valor = valorIngreso,
            SaldoResultante = stock.Cantidad,
            EsBajaArticulo = false,
            Comentario = "Ingreso de stock",
            FechaSistema = DateTimeOffset.UtcNow,
            RegistradoPor = request.RegistradoPor,
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El kardex del artículo {request.CodigoArticulo}");
        }

        var descripcion = $"Ingreso de stock — {articulo.Nombre}";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, bodega.IdAgencia, descripcion, request.RegistradoPor,
                [
                    new LineaMovimientoRequest(articulo.TipoArticulo.IdCuentaContableActivo, valorIngreso, 0, descripcion),
                    new LineaMovimientoRequest(idCuentaCaja.Value, 0, valorIngreso, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new StockIngresadoResult(resultadoComprobante.Id, stock.Cantidad, stock.ValorTotal);
    }
}
