using Corela15.Application.Ahorros;
using Corela15.Application.Colocacion;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Application.FlujoTrabajo;
using Corela15.Domain.Clientes;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Credito;
using Corela15.Domain.CuentasPorCobrar;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class PrestamoService(
    Corela15DbContext db, IComprobanteContableService comprobantes, IFlujoTrabajoService flujoTrabajo) : IPrestamoService
{
    // IDs reales de flujotrabajo.etapa para el TipoEtapa "SOLICITUD CRÉDITO"
    // (verificado contra Softbank, ver migración FlujoTrabajo_MotorAprobaciones).
    private const int IdEtapaComiteCredito = 8;
    private const int IdEtapaNegada = 26;

    private const string CodigoTipoTransaccionDesembolso = "DESEMB-EFEC";
    private const string CodigoCuentaCaja = "1101";
    private const string CodigoCuentaCartera = "1401";
    private const string CodigoCuentaInteresesGanados = "5101";
    private const string CodigoCuentaInteresMora = "510450"; // real, sembrada desde el CUC oficial completo
    private const string CodigoCuentaProvisionIncobrables = "1499"; // real, sembrada desde el CUC oficial completo
    private const decimal TasaMoraMaximaAnual = 0.10m; // techo real BCE, "Norma para tasas de interés por mora"
    private const int IdTipoComprobanteDiario = 3; // 'DIA'

    // Códigos reales de COLOCACION.ESTADO_PRESTAMORUBRO (A/C/E/P/V) — antes
    // este motor usaba texto libre "Pendiente"/"Pagado" inventado, sin
    // relación con el catálogo real. Solo P (Pendiente) y C (Cancelado) se
    // usan hoy — A/E/V quedan reservados para cuando se necesiten (ver
    // CLAUDE.md, sección "Motor de rubros real").
    private const string CodigoEstadoPendiente = "P";
    private const string CodigoEstadoCancelado = "C";

    private const string CodigoTipoTransaccionRegistroCxC = "REG-CXC"; // reusado para el cargo de rubro manual

    public async Task<SolicitudPrestamoCreadaResult> SolicitarAsync(
        SolicitarPrestamoRequest request, CancellationToken cancellationToken = default)
    {
        var tipoPrestamo = await db.TiposPrestamo.FirstOrDefaultAsync(t => t.Id == request.IdTipoPrestamo, cancellationToken);
        if (tipoPrestamo is null || !tipoPrestamo.Activo)
        {
            throw new TipoPrestamoInvalidoException(request.IdTipoPrestamo);
        }

        if (request.MontoSolicitado < tipoPrestamo.MontoMinimo || request.MontoSolicitado > tipoPrestamo.MontoMaximo)
        {
            throw new MontoFueraDeRangoException(request.MontoSolicitado, tipoPrestamo.MontoMinimo, tipoPrestamo.MontoMaximo);
        }

        // Techo regulatorio BCE: se valida contra el techo VIGENTE a hoy,
        // no contra el que existía cuando se sembró/configuró el producto
        // — si el BCE baja el techo de un segmento, un producto que era
        // válido puede dejar de serlo, y acá se detecta en cada solicitud
        // nueva, no solo al crear el producto.
        var techoVigente = await db.TasasTechoBce
            .Where(t => t.Segmento == tipoPrestamo.SegmentoBce && t.Activo && t.FechaVigenciaDesde <= DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderByDescending(t => t.FechaVigenciaDesde)
            .FirstOrDefaultAsync(cancellationToken);
        if (techoVigente is null)
        {
            throw new SinTechoBceConfiguradoException(tipoPrestamo.SegmentoBce);
        }
        if (tipoPrestamo.TasaAnual > techoVigente.TasaMaxima)
        {
            throw new TasaExcedeTechoBceException(tipoPrestamo.TasaAnual, techoVigente.TasaMaxima, tipoPrestamo.SegmentoBce);
        }

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.IdCliente, cancellationToken);
        if (cliente is null || cliente.Estado != EstadoCliente.Activo)
        {
            throw new ClienteInvalidoException(request.IdCliente);
        }

        // Convenio real (empleador/sindicato con descuento vía rol de
        // pagos) — opcional, verificado contra el catálogo real (solo 2
        // códigos con uso real hoy, ver migración Credito_TipoConvenio).
        if (request.CodigoTipoConvenio is not null)
        {
            var convenioValido = await db.TiposConvenio
                .AnyAsync(c => c.Codigo == request.CodigoTipoConvenio && c.Activo, cancellationToken);
            if (!convenioValido)
            {
                throw new TipoConvenioInvalidoException(request.CodigoTipoConvenio);
            }
        }

        var ultimoNumero = await db.SolicitudesPrestamo.CountAsync(cancellationToken);
        var numero = (ultimoNumero + 1).ToString().PadLeft(10, '0');

        var solicitud = new SolicitudPrestamo
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            IdCliente = request.IdCliente,
            IdTipoPrestamo = request.IdTipoPrestamo,
            IdAgencia = request.IdAgencia,
            MontoSolicitado = request.MontoSolicitado,
            Cuotas = request.Cuotas,
            FechaSolicitud = DateOnly.FromDateTime(DateTime.UtcNow),
            Estado = EstadoSolicitud.EnAnalisis,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
            CodigoTipoConvenio = request.CodigoTipoConvenio,
        };
        db.SolicitudesPrestamo.Add(solicitud);
        await db.SaveChangesAsync(cancellationToken);

        return new SolicitudPrestamoCreadaResult(solicitud.Id, numero);
    }

    public async Task<SolicitudAprobadaResult> AprobarSolicitudAsync(
        AprobarSolicitudRequest request, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesPrestamo.Include(s => s.TipoPrestamo)
            .FirstOrDefaultAsync(s => s.Id == request.IdSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitud.EnAnalisis)
        {
            throw new SolicitudPrestamoNoEnAnalisisException(request.IdSolicitud);
        }

        if (request.MontoAprobado < solicitud.TipoPrestamo.MontoMinimo || request.MontoAprobado > solicitud.TipoPrestamo.MontoMaximo)
        {
            throw new MontoFueraDeRangoException(request.MontoAprobado, solicitud.TipoPrestamo.MontoMinimo, solicitud.TipoPrestamo.MontoMaximo);
        }

        // Motor real de aprobaciones (ver Domain/FlujoTrabajo): valida que
        // quien aprueba pertenezca al grupo de aprobadores reales del
        // Comité de Crédito para esta agencia y este monto — antes
        // cualquier usuario con acceso al menú de Créditos podía aprobar
        // cualquier monto, gap real cerrado acá.
        var etapaRegistrada = await flujoTrabajo.ValidarYRegistrarAsync(
            IdEtapaComiteCredito, solicitud.IdAgencia, request.MontoAprobado, request.RegistradoPor, cancellationToken);

        var idEtapaAnterior = solicitud.IdEtapaActual;
        solicitud.Estado = EstadoSolicitud.Aprobada;
        solicitud.MontoAprobado = request.MontoAprobado;
        solicitud.AprobadoPor = request.RegistradoPor;
        solicitud.FechaAprobacion = DateTimeOffset.UtcNow;
        solicitud.ComentarioAprobacion = request.Comentario;
        solicitud.IdEtapaActual = etapaRegistrada.IdEtapa;
        solicitud.ModificadoEn = DateTimeOffset.UtcNow;
        solicitud.ModificadoPor = request.RegistradoPor;

        db.SolicitudesPrestamoEtapaHist.Add(new SolicitudPrestamoEtapaHist
        {
            Id = Guid.NewGuid(),
            IdSolicitudPrestamo = solicitud.Id,
            IdEtapaAnterior = idEtapaAnterior,
            IdEtapa = etapaRegistrada.IdEtapa,
            Comentario = request.Comentario,
            RegistradoPor = request.RegistradoPor,
            Fecha = DateTimeOffset.UtcNow,
            EsRetorno = false,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new SolicitudAprobadaResult(solicitud.Id, request.MontoAprobado);
    }

    public async Task RechazarSolicitudAsync(
        RechazarSolicitudRequest request, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesPrestamo.FirstOrDefaultAsync(s => s.Id == request.IdSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitud.EnAnalisis)
        {
            throw new SolicitudPrestamoNoEnAnalisisException(request.IdSolicitud);
        }

        await flujoTrabajo.ValidarYRegistrarAsync(
            IdEtapaComiteCredito, solicitud.IdAgencia, solicitud.MontoSolicitado, request.RegistradoPor, cancellationToken);

        var idEtapaAnterior = solicitud.IdEtapaActual;
        solicitud.Estado = EstadoSolicitud.Rechazada;
        solicitud.AprobadoPor = request.RegistradoPor;
        solicitud.FechaAprobacion = DateTimeOffset.UtcNow;
        solicitud.ComentarioAprobacion = request.Comentario;
        solicitud.IdEtapaActual = IdEtapaNegada;
        solicitud.ModificadoEn = DateTimeOffset.UtcNow;
        solicitud.ModificadoPor = request.RegistradoPor;

        db.SolicitudesPrestamoEtapaHist.Add(new SolicitudPrestamoEtapaHist
        {
            Id = Guid.NewGuid(),
            IdSolicitudPrestamo = solicitud.Id,
            IdEtapaAnterior = idEtapaAnterior,
            IdEtapa = IdEtapaNegada,
            Comentario = request.Comentario,
            RegistradoPor = request.RegistradoPor,
            Fecha = DateTimeOffset.UtcNow,
            EsRetorno = true,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PrestamoDesembolsadoResult> DesembolsarAsync(
        DesembolsarPrestamoRequest request, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesPrestamo.Include(s => s.TipoPrestamo).ThenInclude(t => t.TipoSeguro)
            .FirstOrDefaultAsync(s => s.Id == request.IdSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitud.Aprobada || solicitud.MontoAprobado is null)
        {
            throw new SolicitudPrestamoInvalidaException(request.IdSolicitud);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionDesembolso, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionDesembolso);
        }

        var rubroCapital = await db.Rubros.FirstAsync(r => r.TipoRubro.EsCapital, cancellationToken);
        var rubroInteres = await db.Rubros.FirstAsync(r => r.TipoRubro.EsTasa, cancellationToken);

        // Seguro de desgravamen flat (ver TipoSeguro.cs) — solo se genera
        // si el producto tiene un tipo de seguro configurado. `null` es
        // el caso real y legítimo de "este producto no cobra seguro", no
        // un error.
        var tipoSeguro = solicitud.TipoPrestamo.TipoSeguro;
        var rubroSeguro = tipoSeguro is not null
            ? await db.Rubros.FirstAsync(r => r.CodigoTipoRubro == "SEG", cancellationToken)
            : null;

        // Una sola transacción: préstamo, tabla de amortización, asiento
        // contable y cambio de estado de la solicitud, todo junto o nada
        // (mismo patrón que CuentaAhorroService — ver CLAUDE.md).
        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var ultimoNumero = await db.Prestamos.CountAsync(cancellationToken);
        var numero = (ultimoNumero + 1).ToString().PadLeft(10, '0');
        var fechaAdjudicacion = DateOnly.FromDateTime(DateTime.UtcNow);

        var prestamo = new Prestamo
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            IdTipoPrestamo = solicitud.IdTipoPrestamo,
            IdAgencia = solicitud.IdAgencia,
            Cuotas = solicitud.Cuotas,
            DeudaInicial = solicitud.MontoAprobado.Value,
            Saldo = solicitud.MontoAprobado.Value,
            Tasa = solicitud.TipoPrestamo.TasaAnual,
            Tea = solicitud.TipoPrestamo.TasaAnual,
            FechaAdjudicacion = fechaAdjudicacion,
            FechaVencimiento = fechaAdjudicacion.AddMonths(solicitud.Cuotas),
            DebitoSpi = false,
            Estado = EstadoPrestamo.Vigente,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
            CodigoUsuarioAsesor = solicitud.CreadoPor,
            CodigoTipoConvenio = solicitud.CodigoTipoConvenio,
        };
        db.Prestamos.Add(prestamo);

        db.PrestamosClientes.Add(new PrestamoCliente
        {
            IdPrestamo = prestamo.Id,
            IdCliente = solicitud.IdCliente,
            Principal = true,
        });

        // Garantes registrados durante el análisis pasan al préstamo real,
        // igual que Softbank copia SOLICITUD_PRESTAMO_GARANTIAPERSONAL →
        // PRESTAMO_GARANTIAPERSONAL en el desembolso.
        var garantesSolicitud = await db.SolicitudesPrestamoGarantia
            .Where(g => g.IdSolicitudPrestamo == solicitud.Id && g.Activo)
            .ToListAsync(cancellationToken);
        foreach (var garante in garantesSolicitud)
        {
            db.PrestamosGarantias.Add(new PrestamoGarantia
            {
                Id = Guid.NewGuid(),
                IdPrestamo = prestamo.Id,
                IdClienteGarante = garante.IdClienteGarante,
                CodigoEstadoGarantia = "A",
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = request.RegistradoPor,
            });
        }

        foreach (var cuota in GenerarTablaAmortizacion(prestamo.DeudaInicial, prestamo.Tasa, prestamo.Cuotas, fechaAdjudicacion))
        {
            db.PrestamosRubros.Add(new PrestamoRubro
            {
                Id = Guid.NewGuid(),
                IdPrestamo = prestamo.Id,
                IdRubro = rubroCapital.Id,
                NumeroCuota = cuota.NumeroCuota,
                FechaInicio = cuota.FechaInicio,
                FechaFin = cuota.FechaFin,
                Proyectado = cuota.Capital,
                Calculado = 0,
                Cobrado = 0,
                Estado = CodigoEstadoPendiente,
            });
            db.PrestamosRubros.Add(new PrestamoRubro
            {
                Id = Guid.NewGuid(),
                IdPrestamo = prestamo.Id,
                IdRubro = rubroInteres.Id,
                NumeroCuota = cuota.NumeroCuota,
                FechaInicio = cuota.FechaInicio,
                FechaFin = cuota.FechaFin,
                Proyectado = cuota.Interes,
                Calculado = 0,
                Cobrado = 0,
                Estado = CodigoEstadoPendiente,
            });

            if (rubroSeguro is not null)
            {
                db.PrestamosRubros.Add(new PrestamoRubro
                {
                    Id = Guid.NewGuid(),
                    IdPrestamo = prestamo.Id,
                    IdRubro = rubroSeguro.Id,
                    NumeroCuota = cuota.NumeroCuota,
                    FechaInicio = cuota.FechaInicio,
                    FechaFin = cuota.FechaFin,
                    Proyectado = tipoSeguro!.ValorMensual,
                    Calculado = 0,
                    Cobrado = 0,
                    Estado = CodigoEstadoPendiente,
                });
            }
        }

        solicitud.Estado = EstadoSolicitud.Desembolsada;
        solicitud.ModificadoEn = DateTimeOffset.UtcNow;
        solicitud.ModificadoPor = request.RegistradoPor;

        await db.SaveChangesAsync(cancellationToken);

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                fechaAdjudicacion,
                tipoTransaccion.IdTipoComprobante,
                prestamo.IdAgencia,
                $"Desembolso préstamo {numero}",
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, prestamo.DeudaInicial, 0, $"Desembolso préstamo {numero}"),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, prestamo.DeudaInicial, $"Entrega de efectivo préstamo {numero}"),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new PrestamoDesembolsadoResult(prestamo.Id, numero, resultadoComprobante.Id);
    }

    public async Task<PagoCuotaRegistradoResult> PagarCuotaAsync(
        PagarCuotaRequest request, CancellationToken cancellationToken = default)
    {
        var prestamo = await db.Prestamos.FirstOrDefaultAsync(p => p.Id == request.IdPrestamo, cancellationToken);
        if (prestamo is null || prestamo.Estado != EstadoPrestamo.Vigente)
        {
            throw new PrestamoInvalidoException(request.IdPrestamo);
        }

        var rubrosCuota = await db.PrestamosRubros
            .Include(r => r.Rubro).ThenInclude(r => r.TipoRubro)
            .Where(r => r.IdPrestamo == prestamo.Id && r.Estado == CodigoEstadoPendiente)
            .OrderBy(r => r.NumeroCuota)
            .ToListAsync(cancellationToken);

        // La próxima cuota a pagar se define por el rubro de Capital
        // pendiente, nunca por "cualquier rubro pendiente" — un rubro que
        // nunca se cobra automáticamente (ej. Seguro Desgravamen, ver
        // "Consolidado Seguro Desgravamen" en CLAUDE.md) puede quedar
        // pendiente indefinidamente en una cuota ya pagada en capital e
        // interés; usar el mínimo genérico dejaba la cuota "atascada" ahí
        // y la siguiente llamada a PagarCuotaAsync fallaba (bug real
        // encontrado al construir el reporte de abonos por convenio,
        // corregido en el mismo turno).
        var rubrosCapitalPendientes = rubrosCuota.Where(r => r.Rubro.TipoRubro.EsCapital).ToList();
        if (rubrosCapitalPendientes.Count == 0)
        {
            throw new PrestamoSinCuotasPendientesException(request.IdPrestamo);
        }
        var proximoNumeroCuota = rubrosCapitalPendientes.Select(r => r.NumeroCuota).Min();
        var rubrosDeLaCuota = rubrosCuota.Where(r => r.NumeroCuota == proximoNumeroCuota).ToList();

        var rubroCapital = rubrosDeLaCuota.First(r => r.Rubro.TipoRubro.EsCapital);
        var rubroInteres = rubrosDeLaCuota.First(r => r.Rubro.TipoRubro.EsTasa);

        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaCartera = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCartera).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaIntereses = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaInteresesGanados).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaInteresMora = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaInteresMora).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);

        if (idCuentaCaja is null || idCuentaCartera is null || idCuentaIntereses is null || idCuentaInteresMora is null)
        {
            throw new InvalidOperationException(
                $"Faltan cuentas contables {CodigoCuentaCaja}/{CodigoCuentaCartera}/{CodigoCuentaInteresesGanados}/{CodigoCuentaInteresMora}.");
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var diasMoraCuota = hoy > rubroCapital.FechaFin ? hoy.DayNumber - rubroCapital.FechaFin.DayNumber : 0;
        var interesMora = diasMoraCuota > 0
            ? Math.Round(rubroCapital.Proyectado * TasaMoraMaximaAnual * diasMoraCuota / 365m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        // Una sola transacción: rubros pagados, saldo del préstamo (y su
        // posible cancelación) y asiento contable, todo junto o nada.
        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        rubroCapital.Cobrado = rubroCapital.Proyectado;
        rubroCapital.Estado = CodigoEstadoCancelado;
        rubroCapital.FechaCobro = hoy;
        rubroInteres.Cobrado = rubroInteres.Proyectado;
        rubroInteres.Estado = CodigoEstadoCancelado;
        rubroInteres.FechaCobro = hoy;

        prestamo.Saldo -= rubroCapital.Proyectado;
        var quedanCuotasPendientes = await db.PrestamosRubros
            .AnyAsync(r => r.IdPrestamo == prestamo.Id && r.Estado == CodigoEstadoPendiente && r.NumeroCuota != proximoNumeroCuota,
                cancellationToken);
        if (!quedanCuotasPendientes)
        {
            prestamo.Estado = EstadoPrestamo.Cancelado;

            // La garantía se libera cuando la deuda se salda por completo
            // — mismo comportamiento real observado en Softbank.
            var garantiasDelPrestamo = await db.PrestamosGarantias
                .Where(g => g.IdPrestamo == prestamo.Id && g.CodigoEstadoGarantia == "A")
                .ToListAsync(cancellationToken);
            foreach (var garantia in garantiasDelPrestamo)
            {
                garantia.CodigoEstadoGarantia = "L";
                garantia.ModificadoEn = DateTimeOffset.UtcNow;
                garantia.ModificadoPor = request.RegistradoPor;
            }
        }
        prestamo.ModificadoEn = DateTimeOffset.UtcNow;
        prestamo.ModificadoPor = request.RegistradoPor;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El préstamo {prestamo.Numero}");
        }

        var montoTotal = rubroCapital.Proyectado + rubroInteres.Proyectado + interesMora;
        var lineas = new List<LineaMovimientoRequest>
        {
            new(idCuentaCaja.Value, montoTotal, 0, $"Pago cuota {proximoNumeroCuota} préstamo {prestamo.Numero}"),
        };
        if (rubroCapital.Proyectado > 0)
        {
            lineas.Add(new LineaMovimientoRequest(idCuentaCartera.Value, 0, rubroCapital.Proyectado, "Abono a capital"));
        }
        if (rubroInteres.Proyectado > 0)
        {
            lineas.Add(new LineaMovimientoRequest(idCuentaIntereses.Value, 0, rubroInteres.Proyectado, "Interés cobrado"));
        }
        if (interesMora > 0)
        {
            lineas.Add(new LineaMovimientoRequest(idCuentaInteresMora.Value, 0, interesMora, $"Interés de mora ({diasMoraCuota} días)"));
        }

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow),
                IdTipoComprobanteDiario,
                prestamo.IdAgencia,
                $"Pago cuota {proximoNumeroCuota} préstamo {prestamo.Numero}",
                request.RegistradoPor,
                lineas),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new PagoCuotaRegistradoResult(
            proximoNumeroCuota, rubroCapital.Proyectado, rubroInteres.Proyectado, prestamo.Saldo,
            prestamo.Estado == EstadoPrestamo.Cancelado, resultadoComprobante.Id, diasMoraCuota, interesMora);
    }

    public async Task<RubroManualCargadoResult> CargarRubroManualAsync(
        CargarRubroManualRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Monto <= 0)
        {
            throw new MontoRubroManualInvalidoException(request.Monto);
        }

        var prestamo = await db.Prestamos.FirstOrDefaultAsync(p => p.Id == request.IdPrestamo, cancellationToken);
        if (prestamo is null || prestamo.Estado != EstadoPrestamo.Vigente)
        {
            throw new PrestamoInvalidoException(request.IdPrestamo);
        }

        var rubro = await db.Rubros.Include(r => r.TipoRubro)
            .FirstOrDefaultAsync(r => r.Id == request.IdRubro && r.Activo, cancellationToken);
        if (rubro is null)
        {
            throw new RubroInvalidoParaCargoManualException(request.IdRubro);
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var proximoNumeroCuota = await db.PrestamosRubros
            .Where(r => r.IdPrestamo == prestamo.Id)
            .Select(r => (int?)r.NumeroCuota)
            .MaxAsync(cancellationToken) ?? 0;
        proximoNumeroCuota++;

        var prestamoRubro = new PrestamoRubro
        {
            Id = Guid.NewGuid(),
            IdPrestamo = prestamo.Id,
            IdRubro = rubro.Id,
            NumeroCuota = proximoNumeroCuota,
            FechaInicio = hoy,
            FechaFin = hoy,
            Proyectado = request.Monto,
            Calculado = 0,
            Cobrado = 0,
            Estado = CodigoEstadoPendiente,
        };
        db.PrestamosRubros.Add(prestamoRubro);
        await db.SaveChangesAsync(cancellationToken);

        Guid? idCuentaPorCobrar = null;
        Guid? idComprobante = null;

        if (rubro.EsCuentaPorCobrar)
        {
            var idPersona = await db.PrestamosClientes
                .Where(pc => pc.IdPrestamo == prestamo.Id)
                .OrderByDescending(pc => pc.Principal)
                .Select(pc => (Guid?)pc.Cliente.IdPersona)
                .FirstOrDefaultAsync(cancellationToken);
            if (idPersona is null)
            {
                throw new InvalidOperationException($"El préstamo {prestamo.Numero} no tiene un socio asociado.");
            }

            var tipoTransaccionCxC = await db.TiposTransaccion
                .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionRegistroCxC, cancellationToken);
            if (tipoTransaccionCxC is null || !tipoTransaccionCxC.Activo)
            {
                throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionRegistroCxC);
            }

            var concepto = $"Cargo de rubro manual — {rubro.Nombre} — Préstamo #{prestamo.Numero}: {request.Detalle}";
            var cuentaPorCobrar = new CuentaPorCobrar
            {
                Id = Guid.NewGuid(),
                Concepto = concepto,
                IdAgencia = prestamo.IdAgencia,
                IdPersona = idPersona.Value,
                Cuotas = 1,
                MontoInicial = request.Monto,
                Saldo = request.Monto,
                FechaCreacion = hoy,
                FechaVencimiento = hoy.AddDays(30),
                Estado = EstadoCuentaPorCobrar.Vigente,
            };
            db.CuentasPorCobrar.Add(cuentaPorCobrar);
            await db.SaveChangesAsync(cancellationToken);

            db.PrestamosRubrosCuentasPorCobrar.Add(new PrestamoRubroCuentaPorCobrar
            {
                IdPrestamoRubro = prestamoRubro.Id,
                IdCuentaPorCobrar = cuentaPorCobrar.Id,
            });
            await db.SaveChangesAsync(cancellationToken);

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    hoy,
                    tipoTransaccionCxC.IdTipoComprobante,
                    prestamo.IdAgencia,
                    concepto,
                    request.RegistradoPor,
                    [
                        new LineaMovimientoRequest(tipoTransaccionCxC.IdCuentaContableDebito, request.Monto, 0, concepto),
                        new LineaMovimientoRequest(tipoTransaccionCxC.IdCuentaContableCredito, 0, request.Monto, concepto),
                    ]),
                cancellationToken);

            idCuentaPorCobrar = cuentaPorCobrar.Id;
            idComprobante = resultadoComprobante.Id;
        }

        await transaccion.CommitAsync(cancellationToken);

        return new RubroManualCargadoResult(
            prestamoRubro.Id, rubro.Nombre, request.Monto, hoy, prestamoRubro.Estado, idCuentaPorCobrar, idComprobante);
    }

    public async Task<IReadOnlyList<RubroManualCargadoResult>> ListarRubrosManualesAsync(
        Guid idPrestamo, CancellationToken cancellationToken = default)
    {
        var manuales = await db.PrestamosRubros
            .Include(r => r.Rubro).ThenInclude(r => r.TipoRubro)
            .Where(r => r.IdPrestamo == idPrestamo && !r.Rubro.TipoRubro.EsCapital && !r.Rubro.TipoRubro.EsTasa)
            .OrderByDescending(r => r.FechaInicio)
            .Select(r => new { r.Id, r.Rubro.Nombre, r.Proyectado, r.FechaInicio, r.Estado })
            .ToListAsync(cancellationToken);

        var idsPrestamoRubro = manuales.Select(m => m.Id).ToList();
        var bridges = await db.PrestamosRubrosCuentasPorCobrar
            .Where(b => idsPrestamoRubro.Contains(b.IdPrestamoRubro))
            .ToDictionaryAsync(b => b.IdPrestamoRubro, b => b.IdCuentaPorCobrar, cancellationToken);

        return manuales
            .Select(m => new RubroManualCargadoResult(
                m.Id, m.Nombre, m.Proyectado, m.FechaInicio, m.Estado,
                bridges.TryGetValue(m.Id, out var idCxC) ? idCxC : null, null))
            .ToList();
    }

    private record CuotaAmortizacion(int NumeroCuota, DateOnly FechaInicio, DateOnly FechaFin, decimal Capital, decimal Interes);

    /// <summary>
    /// Sistema francés (cuota fija): la más usada en cooperativas de ahorro
    /// y crédito ecuatorianas para consumo/microcrédito. El redondeo del
    /// último período absorbe el residuo de centavos para que la suma de
    /// capital cuadre exacto con la deuda inicial.
    /// </summary>
    private static List<CuotaAmortizacion> GenerarTablaAmortizacion(
        decimal monto, decimal tasaAnual, int cuotas, DateOnly fechaInicio)
    {
        var tasaMensual = tasaAnual / 12m;
        var cuotaFija = tasaMensual == 0
            ? monto / cuotas
            : monto * (tasaMensual / (1 - Pow(1 + tasaMensual, -cuotas)));

        var resultado = new List<CuotaAmortizacion>();
        var saldo = monto;
        var inicioPeriodo = fechaInicio;

        for (var i = 1; i <= cuotas; i++)
        {
            var finPeriodo = fechaInicio.AddMonths(i);
            var interes = Math.Round(saldo * tasaMensual, 2);
            var capital = i == cuotas ? saldo : Math.Round(cuotaFija - interes, 2);
            saldo -= capital;

            resultado.Add(new CuotaAmortizacion(i, inicioPeriodo, finPeriodo, capital, interes));
            inicioPeriodo = finPeriodo;
        }

        return resultado;
    }

    private static decimal Pow(decimal valorBase, int exponente)
    {
        var resultado = 1m;
        var potenciaPositiva = Math.Abs(exponente);
        for (var i = 0; i < potenciaPositiva; i++)
        {
            resultado *= valorBase;
        }

        return exponente < 0 ? 1m / resultado : resultado;
    }

    public async Task<DiferimientoCuotaResult> DiferirCuotasAsync(
        DiferirCuotasRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DiasDiferidos <= 0)
        {
            throw new DiasDiferidosInvalidosException(request.DiasDiferidos);
        }

        var prestamo = await db.Prestamos.FirstOrDefaultAsync(p => p.Id == request.IdPrestamo, cancellationToken);
        if (prestamo is null || prestamo.Estado != EstadoPrestamo.Vigente)
        {
            throw new PrestamoInvalidoException(request.IdPrestamo);
        }

        var cuotasCapitalPendientes = await db.PrestamosRubros
            .Include(r => r.Rubro).ThenInclude(r => r.TipoRubro)
            .Where(r => r.IdPrestamo == prestamo.Id && r.Estado == CodigoEstadoPendiente && r.Rubro.TipoRubro.EsCapital)
            .ToListAsync(cancellationToken);

        foreach (var rubro in cuotasCapitalPendientes)
        {
            rubro.FechaFin = rubro.FechaFin.AddDays(request.DiasDiferidos);
        }

        var diferimiento = new DiferimientoCuota
        {
            Id = Guid.NewGuid(),
            IdPrestamo = prestamo.Id,
            DiasDiferidos = request.DiasDiferidos,
            Comentario = request.Comentario,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow),
            RegistradoPor = request.RegistradoPor,
        };
        db.DiferimientosCuota.Add(diferimiento);
        await db.SaveChangesAsync(cancellationToken);

        return new DiferimientoCuotaResult(
            diferimiento.Id, diferimiento.DiasDiferidos, diferimiento.Comentario, diferimiento.Fecha, diferimiento.RegistradoPor);
    }

    public async Task<IReadOnlyList<DiferimientoCuotaResult>> ListarDiferimientosAsync(
        Guid idPrestamo, CancellationToken cancellationToken = default)
    {
        return await db.DiferimientosCuota
            .Where(d => d.IdPrestamo == idPrestamo)
            .OrderByDescending(d => d.Fecha)
            .Select(d => new DiferimientoCuotaResult(d.Id, d.DiasDiferidos, d.Comentario, d.Fecha, d.RegistradoPor))
            .ToListAsync(cancellationToken);
    }

    public async Task<PrestamoCastigadoResult> CastigarAsync(
        CastigarPrestamoRequest request, CancellationToken cancellationToken = default)
    {
        var prestamo = await db.Prestamos.FirstOrDefaultAsync(p => p.Id == request.IdPrestamo, cancellationToken);
        if (prestamo is null || prestamo.Estado == EstadoPrestamo.Cancelado)
        {
            throw new PrestamoInvalidoException(request.IdPrestamo);
        }

        if (prestamo.Estado == EstadoPrestamo.Castigado)
        {
            throw new PrestamoYaCastigadoException(request.IdPrestamo);
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var saldoTransferido = prestamo.Saldo;

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var idCuentaProvision = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaProvisionIncobrables).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaCartera = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCartera).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);

        if (idCuentaProvision is null || idCuentaCartera is null)
        {
            throw new InvalidOperationException("Faltan las cuentas contables reales de provisión/cartera para el castigo.");
        }

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                hoy, IdTipoComprobanteDiario, prestamo.IdAgencia,
                $"Castigo de cartera — Préstamo #{prestamo.Numero}: {request.Comentario}", request.RegistradoPor,
                [
                    new LineaMovimientoRequest(idCuentaProvision.Value, saldoTransferido, 0, "Castigo de cartera"),
                    new LineaMovimientoRequest(idCuentaCartera.Value, 0, saldoTransferido, "Castigo de cartera"),
                ]),
            cancellationToken);

        prestamo.Estado = EstadoPrestamo.Castigado;

        var castigo = new PrestamoCastigado
        {
            Id = Guid.NewGuid(),
            IdPrestamo = prestamo.Id,
            SaldoTransferido = saldoTransferido,
            Comentario = request.Comentario,
            Fecha = hoy,
            RegistradoPor = request.RegistradoPor,
            IdComprobante = resultadoComprobante.Id,
        };
        db.PrestamosCastigados.Add(castigo);
        await db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new PrestamoCastigadoResult(castigo.Id, saldoTransferido, hoy, resultadoComprobante.Id);
    }
}
