using Corela15.Application.ActivoFijo;
using Corela15.Application.Contabilidad;
using Corela15.Domain.ActivoFijo;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ActivoService(Corela15DbContext db, IComprobanteContableService comprobantes) : IActivoService
{
    private const int IdTipoComprobanteDiario = 3; // 'DIA'
    private const string CodigoCuentaPerdidaBajaActivos = "4701"; // Pérdida en venta de bienes, ya sembrada en el CUC oficial

    public async Task<ActivoRegistradoResult> RegistrarAsync(
        RegistrarActivoRequest request, CancellationToken cancellationToken = default)
    {
        var estructura = await db.EstructurasActivoFijo
            .FirstOrDefaultAsync(e => e.Id == request.IdEstructura && e.Activo, cancellationToken);
        if (estructura is null)
        {
            throw new EstructuraInvalidaException(request.IdEstructura);
        }

        if (request.Valor <= 0)
        {
            throw new ValorActivoInvalidoException(request.Valor);
        }

        if (request.FechaCompra > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new FechaCompraFuturaException(request.FechaCompra);
        }

        // Código físico real (verificado contra ACTIVOFIJO.ACTIVO.
        // CODIGOHOMOLOGADO en Softbank — sin duplicados entre valores no
        // nulos en producción) — opcional, pero si se captura debe ser
        // único, igual que en la fuente real.
        if (!string.IsNullOrWhiteSpace(request.Codigo)
            && await db.Activos.AnyAsync(a => a.Codigo == request.Codigo, cancellationToken))
        {
            throw new CodigoActivoDuplicadoException(request.Codigo);
        }

        if (request.IdResponsableInicial is not null)
        {
            var responsableValido = await db.ResponsablesActivoFijo
                .AnyAsync(r => r.Id == request.IdResponsableInicial && r.Activo, cancellationToken);
            if (!responsableValido)
            {
                throw new ResponsableInvalidoException(request.IdResponsableInicial.Value);
            }
        }

        var activo = new Activo
        {
            Id = Guid.NewGuid(),
            Codigo = request.Codigo,
            IdEstructura = request.IdEstructura,
            IdAgencia = request.IdAgencia,
            Detalle = request.Detalle,
            FechaCompra = request.FechaCompra,
            Valor = request.Valor,
            Marca = request.Marca,
            Modelo = request.Modelo,
            Serie = request.Serie,
            EsVehiculo = request.EsVehiculo,
            EsBienDeControl = request.EsBienDeControl,
            EsBienIntangible = estructura.EsBienIntangible,
            Asegurado = request.Asegurado,
            Color = request.Color,
            Motor = request.Motor,
            Chasis = request.Chasis,
            Placa = request.Placa,
            Cilindraje = request.Cilindraje,
            AnioMatriculacion = request.AnioMatriculacion,
            AnioVehiculo = request.AnioVehiculo,
            Condicion = CondicionActivo.Bueno,
            Estado = EstadoActivo.Activo,
            FechaInicioCalculo = request.FechaCompra,
            DepreciacionAcumulada = 0,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.Activos.Add(activo);

        if (request.IdResponsableInicial is not null)
        {
            db.ActivosResponsables.Add(new ActivoResponsable
            {
                Id = Guid.NewGuid(),
                IdActivo = activo.Id,
                IdResponsable = request.IdResponsableInicial.Value,
                FechaAsignacion = DateOnly.FromDateTime(DateTime.UtcNow),
                Activa = true,
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ActivoRegistradoResult(activo.Id, activo.Codigo);
    }

    public async Task AsignarResponsableAsync(
        AsignarResponsableRequest request, CancellationToken cancellationToken = default)
    {
        var activo = await db.Activos.FirstOrDefaultAsync(a => a.Id == request.IdActivo, cancellationToken);
        if (activo is null || activo.Estado != EstadoActivo.Activo)
        {
            throw new ActivoInvalidoException(request.IdActivo);
        }

        var responsable = await db.ResponsablesActivoFijo
            .FirstOrDefaultAsync(r => r.Id == request.IdResponsable && r.Activo, cancellationToken);
        if (responsable is null)
        {
            throw new ResponsableInvalidoException(request.IdResponsable);
        }

        var asignacionAnterior = await db.ActivosResponsables
            .Where(ar => ar.IdActivo == activo.Id && ar.Activa)
            .ToListAsync(cancellationToken);
        foreach (var anterior in asignacionAnterior)
        {
            anterior.Activa = false;
        }

        db.ActivosResponsables.Add(new ActivoResponsable
        {
            Id = Guid.NewGuid(),
            IdActivo = activo.Id,
            IdResponsable = request.IdResponsable,
            FechaAsignacion = DateOnly.FromDateTime(DateTime.UtcNow),
            Activa = true,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<DepreciacionEjecutadaResult> EjecutarDepreciacionAsync(
        string registradoPor, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow;
        var fecha = DateOnly.FromDateTime(new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month)));

        var agenciasConActivos = await db.Activos
            .Where(a => a.Estado == EstadoActivo.Activo && a.Estructura.SeDeprecia)
            .Select(a => a.IdAgencia)
            .Distinct()
            .Where(idAgencia => !db.DepreciacionesAgencia.Any(d => d.IdAgencia == idAgencia && d.Fecha == fecha))
            .ToListAsync(cancellationToken);

        var idsComprobante = new List<Guid>();
        var totalActivos = 0;
        var totalDepreciado = 0m;

        foreach (var idAgencia in agenciasConActivos)
        {
            var activos = await db.Activos
                .Include(a => a.Estructura)
                .Where(a => a.IdAgencia == idAgencia && a.Estado == EstadoActivo.Activo && a.Estructura.SeDeprecia
                    && a.DepreciacionAcumulada < a.Valor)
                .ToListAsync(cancellationToken);

            if (activos.Count == 0) continue;

            var cabecera = new DepreciacionAgencia
            {
                Id = Guid.NewGuid(),
                IdAgencia = idAgencia,
                Fecha = fecha,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = registradoPor,
            };
            db.DepreciacionesAgencia.Add(cabecera);

            var acumuladoPorCuenta = new Dictionary<(Guid Gasto, Guid Deprecia), decimal>();

            foreach (var activo in activos)
            {
                var depreciacionMensual = Math.Round(activo.Valor * (activo.Estructura.PorcentajeDepreciacionAnual / 100m) / 12m, 2);
                var maximoRestante = activo.Valor - activo.DepreciacionAcumulada;
                if (depreciacionMensual > maximoRestante) depreciacionMensual = maximoRestante;
                if (depreciacionMensual <= 0) continue;

                activo.DepreciacionAcumulada += depreciacionMensual;
                var saldoLibros = activo.Valor - activo.DepreciacionAcumulada;

                db.DepreciacionesAgenciaDetalle.Add(new DepreciacionAgenciaDetalle
                {
                    Id = Guid.NewGuid(),
                    IdDepreciacionAgencia = cabecera.Id,
                    IdActivo = activo.Id,
                    DepreciacionPeriodo = depreciacionMensual,
                    DepreciacionAcumulada = activo.DepreciacionAcumulada,
                    SaldoLibros = saldoLibros,
                    DepreciadoTotal = saldoLibros <= 0,
                });

                if (activo.Estructura.IdCuentaContableGasto is Guid idGasto && activo.Estructura.IdCuentaContableDeprecia is Guid idDeprecia)
                {
                    var clave = (idGasto, idDeprecia);
                    acumuladoPorCuenta[clave] = acumuladoPorCuenta.GetValueOrDefault(clave) + depreciacionMensual;
                }

                totalActivos++;
                totalDepreciado += depreciacionMensual;
            }

            await db.SaveChangesAsync(cancellationToken);

            if (acumuladoPorCuenta.Count > 0)
            {
                var lineas = new List<LineaMovimientoRequest>();
                foreach (var ((idGasto, idDeprecia), monto) in acumuladoPorCuenta)
                {
                    lineas.Add(new LineaMovimientoRequest(idGasto, monto, 0, $"Depreciación {fecha:yyyy-MM}"));
                    lineas.Add(new LineaMovimientoRequest(idDeprecia, 0, monto, $"Depreciación {fecha:yyyy-MM}"));
                }

                var resultadoComprobante = await comprobantes.RegistrarAsync(
                    new RegistrarComprobanteContableRequest(
                        fecha, IdTipoComprobanteDiario, idAgencia,
                        $"Depreciación de activos fijos — {fecha:yyyy-MM}", registradoPor, lineas),
                    cancellationToken);

                idsComprobante.Add(resultadoComprobante.Id);
            }
        }

        return new DepreciacionEjecutadaResult(fecha, agenciasConActivos.Count, totalActivos, totalDepreciado, idsComprobante);
    }

    public async Task<TrasladoCreadoResult> CrearTrasladoAsync(
        CrearTrasladoRequest request, CancellationToken cancellationToken = default)
    {
        var activo = await db.Activos.FirstOrDefaultAsync(a => a.Id == request.IdActivo, cancellationToken);
        if (activo is null || activo.Estado != EstadoActivo.Activo)
        {
            throw new ActivoInvalidoException(request.IdActivo);
        }

        var motivoValido = await db.MotivosTrasladoActivo.AnyAsync(m => m.Id == request.IdMotivoTraslado && m.Activo, cancellationToken);
        if (!motivoValido)
        {
            throw new MotivoTrasladoInvalidoException(request.IdMotivoTraslado);
        }

        var agenciaDestinoValida = await db.Agencias.AnyAsync(a => a.Id == request.IdAgenciaDestino && a.Activa, cancellationToken);
        if (!agenciaDestinoValida)
        {
            throw new AgenciaInvalidaException(request.IdAgenciaDestino);
        }

        if (request.IdResponsableDestino is not null)
        {
            var responsableDestinoValido = await db.ResponsablesActivoFijo
                .AnyAsync(r => r.Id == request.IdResponsableDestino && r.Activo, cancellationToken);
            if (!responsableDestinoValido)
            {
                throw new ResponsableInvalidoException(request.IdResponsableDestino.Value);
            }
        }

        var responsableActual = await db.ActivosResponsables
            .Where(ar => ar.IdActivo == activo.Id && ar.Activa)
            .Select(ar => (Guid?)ar.IdResponsable)
            .FirstOrDefaultAsync(cancellationToken);

        var traslado = new TrasladoActivo
        {
            Id = Guid.NewGuid(),
            IdActivo = activo.Id,
            Concepto = request.Concepto,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow),
            IdMotivoTraslado = request.IdMotivoTraslado,
            Razon = request.Razon,
            IdResponsableOrigen = responsableActual,
            IdAgenciaOrigen = activo.IdAgencia,
            IdResponsableDestino = request.IdResponsableDestino,
            IdAgenciaDestino = request.IdAgenciaDestino,
            Estado = EstadoTrasladoActivo.Pendiente,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.TrasladosActivo.Add(traslado);
        await db.SaveChangesAsync(cancellationToken);

        return new TrasladoCreadoResult(traslado.Id);
    }

    public async Task ProcesarTrasladoAsync(Guid idTraslado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var traslado = await db.TrasladosActivo
            .Include(t => t.Activo)
            .FirstOrDefaultAsync(t => t.Id == idTraslado, cancellationToken);
        if (traslado is null || traslado.Estado != EstadoTrasladoActivo.Pendiente)
        {
            throw new TrasladoActivoInvalidoException(idTraslado);
        }

        traslado.Activo.IdAgencia = traslado.IdAgenciaDestino;
        traslado.Activo.ModificadoEn = DateTimeOffset.UtcNow;
        traslado.Activo.ModificadoPor = registradoPor;

        if (traslado.IdResponsableDestino is Guid idResponsableDestino)
        {
            var asignacionesAnteriores = await db.ActivosResponsables
                .Where(ar => ar.IdActivo == traslado.IdActivo && ar.Activa)
                .ToListAsync(cancellationToken);
            foreach (var anterior in asignacionesAnteriores)
            {
                anterior.Activa = false;
            }

            db.ActivosResponsables.Add(new ActivoResponsable
            {
                Id = Guid.NewGuid(),
                IdActivo = traslado.IdActivo,
                IdResponsable = idResponsableDestino,
                FechaAsignacion = DateOnly.FromDateTime(DateTime.UtcNow),
                Activa = true,
            });
        }

        traslado.Estado = EstadoTrasladoActivo.Procesado;
        traslado.ModificadoEn = DateTimeOffset.UtcNow;
        traslado.ModificadoPor = registradoPor;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AnularTrasladoAsync(Guid idTraslado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var traslado = await db.TrasladosActivo.FirstOrDefaultAsync(t => t.Id == idTraslado, cancellationToken);
        if (traslado is null || traslado.Estado != EstadoTrasladoActivo.Pendiente)
        {
            throw new TrasladoActivoInvalidoException(idTraslado);
        }

        traslado.Estado = EstadoTrasladoActivo.Anulado;
        traslado.ModificadoEn = DateTimeOffset.UtcNow;
        traslado.ModificadoPor = registradoPor;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<SolicitudBajaCreadaResult> SolicitarBajaAsync(
        SolicitarBajaRequest request, CancellationToken cancellationToken = default)
    {
        var activo = await db.Activos.FirstOrDefaultAsync(a => a.Id == request.IdActivo, cancellationToken);
        if (activo is null || activo.Estado != EstadoActivo.Activo)
        {
            throw new ActivoInvalidoException(request.IdActivo);
        }

        var motivoValido = await db.MotivosBajaActivo.AnyAsync(m => m.Id == request.IdMotivoBaja && m.Activo, cancellationToken);
        if (!motivoValido)
        {
            throw new MotivoBajaInvalidoException(request.IdMotivoBaja);
        }

        var solicitud = new SolicitudActivoBaja
        {
            Id = Guid.NewGuid(),
            IdActivo = activo.Id,
            IdMotivoBaja = request.IdMotivoBaja,
            Detalle = request.Detalle,
            FechaSolicitud = DateOnly.FromDateTime(DateTime.UtcNow),
            Estado = EstadoSolicitudActivoBaja.Ingresado,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.SolicitudesActivoBaja.Add(solicitud);
        await db.SaveChangesAsync(cancellationToken);

        return new SolicitudBajaCreadaResult(solicitud.Id);
    }

    public async Task AutorizarBajaAsync(Guid idSolicitud, string registradoPor, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesActivoBaja.FirstOrDefaultAsync(s => s.Id == idSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitudActivoBaja.Ingresado)
        {
            throw new SolicitudActivoBajaInvalidaException(idSolicitud);
        }

        solicitud.Estado = EstadoSolicitudActivoBaja.Autorizado;
        solicitud.ModificadoEn = DateTimeOffset.UtcNow;
        solicitud.ModificadoPor = registradoPor;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<BajaProcesadaResult> ProcesarBajaAsync(
        Guid idSolicitud, string registradoPor, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesActivoBaja
            .Include(s => s.Activo).ThenInclude(a => a.Estructura)
            .FirstOrDefaultAsync(s => s.Id == idSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitudActivoBaja.Autorizado)
        {
            throw new SolicitudActivoBajaInvalidaException(idSolicitud);
        }

        var idCuentaPerdida = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaPerdidaBajaActivos).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaPerdida is null)
        {
            throw new InvalidOperationException($"Falta la cuenta contable {CodigoCuentaPerdidaBajaActivos}.");
        }

        var activo = solicitud.Activo;
        var depreciacionAcumulada = activo.DepreciacionAcumulada;
        var valorLibros = activo.Valor - depreciacionAcumulada;

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        activo.Estado = EstadoActivo.Baja;
        activo.ModificadoEn = DateTimeOffset.UtcNow;
        activo.ModificadoPor = registradoPor;

        solicitud.Estado = EstadoSolicitudActivoBaja.Procesado;
        solicitud.ModificadoEn = DateTimeOffset.UtcNow;
        solicitud.ModificadoPor = registradoPor;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Application.Common.ConflictoConcurrenciaException($"El activo {activo.Detalle}");
        }

        var descripcion = $"Baja de activo — {activo.Detalle}";
        var lineas = new List<LineaMovimientoRequest>();
        if (depreciacionAcumulada > 0 && activo.Estructura.IdCuentaContableDeprecia is Guid idDeprecia)
        {
            lineas.Add(new LineaMovimientoRequest(idDeprecia, depreciacionAcumulada, 0, descripcion));
        }
        if (valorLibros > 0)
        {
            lineas.Add(new LineaMovimientoRequest(idCuentaPerdida.Value, valorLibros, 0, descripcion));
        }
        lineas.Add(new LineaMovimientoRequest(activo.Estructura.IdCuentaContableActivo, 0, activo.Valor, descripcion));

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, activo.IdAgencia,
                descripcion, registradoPor, lineas),
            cancellationToken);

        solicitud.IdComprobante = resultadoComprobante.Id;
        await db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new BajaProcesadaResult(resultadoComprobante.Id, valorLibros, depreciacionAcumulada);
    }
}
