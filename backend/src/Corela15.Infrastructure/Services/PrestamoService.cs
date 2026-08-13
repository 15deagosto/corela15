using Corela15.Application.Ahorros;
using Corela15.Application.Colocacion;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Clientes;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Credito;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class PrestamoService(Corela15DbContext db, IComprobanteContableService comprobantes) : IPrestamoService
{
    private const string CodigoTipoTransaccionDesembolso = "DESEMB-EFEC";
    private const string CodigoCuentaCaja = "1101";
    private const string CodigoCuentaCartera = "1401";
    private const string CodigoCuentaInteresesGanados = "5101";
    private const int IdTipoComprobanteDiario = 3; // 'DIA'

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

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.IdCliente, cancellationToken);
        if (cliente is null || cliente.Estado != EstadoCliente.Activo)
        {
            throw new ClienteInvalidoException(request.IdCliente);
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
        };
        db.SolicitudesPrestamo.Add(solicitud);
        await db.SaveChangesAsync(cancellationToken);

        return new SolicitudPrestamoCreadaResult(solicitud.Id, numero);
    }

    public async Task<PrestamoDesembolsadoResult> DesembolsarAsync(
        DesembolsarPrestamoRequest request, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesPrestamo.Include(s => s.TipoPrestamo)
            .FirstOrDefaultAsync(s => s.Id == request.IdSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitud.EnAnalisis)
        {
            throw new SolicitudPrestamoInvalidaException(request.IdSolicitud);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionDesembolso, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionDesembolso);
        }

        var rubroCapital = await db.Rubros.FirstAsync(r => r.Codigo == "CAP", cancellationToken);
        var rubroInteres = await db.Rubros.FirstAsync(r => r.Codigo == "INT", cancellationToken);

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
            DeudaInicial = solicitud.MontoSolicitado,
            Saldo = solicitud.MontoSolicitado,
            Tasa = solicitud.TipoPrestamo.TasaAnual,
            Tea = solicitud.TipoPrestamo.TasaAnual,
            FechaAdjudicacion = fechaAdjudicacion,
            FechaVencimiento = fechaAdjudicacion.AddMonths(solicitud.Cuotas),
            DebitoSpi = false,
            Estado = EstadoPrestamo.Vigente,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.Prestamos.Add(prestamo);

        db.PrestamosClientes.Add(new PrestamoCliente
        {
            IdPrestamo = prestamo.Id,
            IdCliente = solicitud.IdCliente,
            Principal = true,
        });

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
                Estado = "Pendiente",
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
                Estado = "Pendiente",
            });
        }

        solicitud.Estado = EstadoSolicitud.Desembolsada;
        solicitud.MontoAprobado = solicitud.MontoSolicitado;
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
            .Include(r => r.Rubro)
            .Where(r => r.IdPrestamo == prestamo.Id && r.Estado == "Pendiente")
            .OrderBy(r => r.NumeroCuota)
            .ToListAsync(cancellationToken);

        var proximoNumeroCuota = rubrosCuota.Select(r => r.NumeroCuota).DefaultIfEmpty(0).Min();
        var rubrosDeLaCuota = rubrosCuota.Where(r => r.NumeroCuota == proximoNumeroCuota).ToList();
        if (rubrosDeLaCuota.Count == 0)
        {
            throw new PrestamoSinCuotasPendientesException(request.IdPrestamo);
        }

        var rubroCapital = rubrosDeLaCuota.First(r => r.Rubro.Codigo == "CAP");
        var rubroInteres = rubrosDeLaCuota.First(r => r.Rubro.Codigo == "INT");

        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaCartera = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCartera).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaIntereses = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaInteresesGanados).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);

        if (idCuentaCaja is null || idCuentaCartera is null || idCuentaIntereses is null)
        {
            throw new InvalidOperationException(
                $"Faltan cuentas contables {CodigoCuentaCaja}/{CodigoCuentaCartera}/{CodigoCuentaInteresesGanados}.");
        }

        // Una sola transacción: rubros pagados, saldo del préstamo (y su
        // posible cancelación) y asiento contable, todo junto o nada.
        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        rubroCapital.Cobrado = rubroCapital.Proyectado;
        rubroCapital.Estado = "Pagado";
        rubroInteres.Cobrado = rubroInteres.Proyectado;
        rubroInteres.Estado = "Pagado";

        prestamo.Saldo -= rubroCapital.Proyectado;
        var quedanCuotasPendientes = await db.PrestamosRubros
            .AnyAsync(r => r.IdPrestamo == prestamo.Id && r.Estado == "Pendiente" && r.NumeroCuota != proximoNumeroCuota,
                cancellationToken);
        if (!quedanCuotasPendientes)
        {
            prestamo.Estado = EstadoPrestamo.Cancelado;
        }
        prestamo.ModificadoEn = DateTimeOffset.UtcNow;
        prestamo.ModificadoPor = request.RegistradoPor;

        await db.SaveChangesAsync(cancellationToken);

        var montoTotal = rubroCapital.Proyectado + rubroInteres.Proyectado;
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
            prestamo.Estado == EstadoPrestamo.Cancelado, resultadoComprobante.Id);
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
}
