using Corela15.Application.Contabilidad;
using Corela15.Application.Nomina;
using Corela15.Domain.Nomina;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class BeneficioSocialService(Corela15DbContext db, IComprobanteContableService comprobantes) : IBeneficioSocialService
{
    private const int IdTipoComprobanteDiario = 3; // 'DIA'
    private const string CodigoGastoBeneficios = "450110"; // Beneficios sociales (décimos + provisión vacaciones)
    private const string CodigoPasivoBeneficios = "250310"; // Beneficios Sociales
    private const string CodigoGastoFondosReserva = "450135"; // Fondo de reserva IESS
    private const string CodigoPasivoFondosReserva = "250320"; // Fondo de reserva IESS
    private const string CodigoGastoAportePatronal = "450120"; // Aportes al IESS (gasto)
    private const string CodigoPasivoAportePatronal = "250315"; // Aportes al IESS (pasivo)
    private const string CodigoCaja = "1101"; // Caja General

    // Tasa real vigente de aporte patronal al IESS (Ecuador, sector
    // privado) — dato público, no vive en ninguna tabla de Softbank,
    // mismo criterio ya usado con el 10% de interés de mora BCE.
    private const decimal TasaAportePatronal = 0.1115m;

    public async Task<DevengoBeneficiosResult> EjecutarDevengoAsync(
        string registradoPor, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow;
        var fecha = DateOnly.FromDateTime(new DateTime(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month)));
        var primerDiaMes = new DateOnly(hoy.Year, hoy.Month, 1);

        var parametro = await db.ParametrosNomina.FirstOrDefaultAsync(cancellationToken);
        var sbu = parametro?.SalarioBasicoUnificado ?? 470m;

        var empleados = await db.Empleados
            .Where(e => e.Estado == EstadoEmpleado.Activo)
            .ToListAsync(cancellationToken);

        var idsComprobante = new List<Guid>();
        var totalDecimoTercero = 0m;
        var totalDecimoCuarto = 0m;
        var totalFondosReserva = 0m;
        var totalProvisionVacaciones = 0m;
        var procesados = 0;

        var totalAportePatronal = 0m;

        var acumuladoBeneficios = 0m; // 450110 débito / 250310 crédito
        var acumuladoFondosReserva = 0m; // 450135 débito / 250320 crédito
        var acumuladoAportePatronal = 0m; // 450120 débito / 250315 crédito

        foreach (var empleado in empleados)
        {
            var ultimoIngreso = await db.RolesPagosEmpleado
                .Where(rpe => rpe.IdEmpleado == empleado.Id && !rpe.Anulado)
                .OrderByDescending(rpe => rpe.RolPagos.Periodo)
                .Select(rpe => (decimal?)rpe.Ingresos)
                .FirstOrDefaultAsync(cancellationToken);

            if (ultimoIngreso is null)
            {
                continue; // sin ningún rol de pagos procesado todavía, no hay base real para devengar
            }

            var decimoTercero = await db.EmpleadosDecimoTercero.FirstOrDefaultAsync(d => d.IdEmpleado == empleado.Id, cancellationToken);
            if (decimoTercero is null)
            {
                decimoTercero = new EmpleadoDecimoTercero { Id = Guid.NewGuid(), IdEmpleado = empleado.Id };
                db.EmpleadosDecimoTercero.Add(decimoTercero);
            }

            var decimoCuarto = await db.EmpleadosDecimoCuarto.FirstOrDefaultAsync(d => d.IdEmpleado == empleado.Id, cancellationToken);
            if (decimoCuarto is null)
            {
                decimoCuarto = new EmpleadoDecimoCuarto { Id = Guid.NewGuid(), IdEmpleado = empleado.Id };
                db.EmpleadosDecimoCuarto.Add(decimoCuarto);
            }

            var provisionVacacion = await db.EmpleadosProvisionVacacion.FirstOrDefaultAsync(d => d.IdEmpleado == empleado.Id, cancellationToken);
            if (provisionVacacion is null)
            {
                provisionVacacion = new EmpleadoProvisionVacacion { Id = Guid.NewGuid(), IdEmpleado = empleado.Id };
                db.EmpleadosProvisionVacacion.Add(provisionVacacion);
            }

            var yaDevengoEsteMes = decimoTercero.UltimoDevengo is not null && decimoTercero.UltimoDevengo >= primerDiaMes;
            if (yaDevengoEsteMes)
            {
                continue;
            }

            var ingreso = ultimoIngreso.Value;

            var cuotaDecimoTercero = Math.Round(ingreso / 12m, 2);
            decimoTercero.Proyectado = ingreso;
            decimoTercero.Acumulado += cuotaDecimoTercero;
            decimoTercero.UltimoDevengo = fecha;

            var cuotaDecimoCuarto = Math.Round(sbu / 12m, 2);
            decimoCuarto.Proyectado = sbu;
            decimoCuarto.Acumulado += cuotaDecimoCuarto;
            decimoCuarto.UltimoDevengo = fecha;

            var cuotaVacaciones = Math.Round(ingreso / 24m, 2);
            var valorAnterior = provisionVacacion.Acumulado;
            provisionVacacion.Acumulado += cuotaVacaciones;
            provisionVacacion.UltimoDevengo = fecha;
            db.EmpleadosProvisionVacacionDetalle.Add(new EmpleadoProvisionVacacionDetalle
            {
                Id = Guid.NewGuid(),
                IdProvisionVacacion = provisionVacacion.Id,
                Fecha = fecha,
                UltimoSueldo = ingreso,
                Dias = 1.25m, // 15 días/año ÷ 12 meses
                ValorAnteriorProvision = valorAnterior,
                ValorActualProvision = provisionVacacion.Acumulado,
                ValorAProvisionar = cuotaVacaciones,
            });

            acumuladoBeneficios += cuotaDecimoTercero + cuotaDecimoCuarto + cuotaVacaciones;

            totalDecimoTercero += cuotaDecimoTercero;
            totalDecimoCuarto += cuotaDecimoCuarto;
            totalProvisionVacaciones += cuotaVacaciones;

            if (empleado.RecibeFondosReserva && empleado.FechaIngreso <= fecha.AddYears(-1))
            {
                var fondosReserva = await db.EmpleadosFondosReserva.FirstOrDefaultAsync(d => d.IdEmpleado == empleado.Id, cancellationToken);
                if (fondosReserva is null)
                {
                    fondosReserva = new EmpleadoFondosReserva { Id = Guid.NewGuid(), IdEmpleado = empleado.Id };
                    db.EmpleadosFondosReserva.Add(fondosReserva);
                }

                var cuotaFondos = Math.Round(ingreso / 12m, 2);
                fondosReserva.Proyectado = ingreso;
                fondosReserva.Acumulado += cuotaFondos;
                fondosReserva.UltimoDevengo = fecha;

                acumuladoFondosReserva += cuotaFondos;
                totalFondosReserva += cuotaFondos;
            }

            var aportePatronal = await db.EmpleadosAportePatronal.FirstOrDefaultAsync(d => d.IdEmpleado == empleado.Id, cancellationToken);
            if (aportePatronal is null)
            {
                aportePatronal = new EmpleadoAportePatronal { Id = Guid.NewGuid(), IdEmpleado = empleado.Id };
                db.EmpleadosAportePatronal.Add(aportePatronal);
            }

            var cuotaAportePatronal = Math.Round(ingreso * TasaAportePatronal, 2);
            aportePatronal.Proyectado = ingreso;
            aportePatronal.Acumulado += cuotaAportePatronal;
            aportePatronal.UltimoDevengo = fecha;

            acumuladoAportePatronal += cuotaAportePatronal;
            totalAportePatronal += cuotaAportePatronal;

            procesados++;
        }

        await db.SaveChangesAsync(cancellationToken);

        if (acumuladoBeneficios > 0)
        {
            var idGasto = await IdCuenta(CodigoGastoBeneficios, cancellationToken);
            var idPasivo = await IdCuenta(CodigoPasivoBeneficios, cancellationToken);
            var resultado = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    fecha, IdTipoComprobanteDiario, 1,
                    $"Devengo de décimos y provisión de vacaciones — {fecha:yyyy-MM}", registradoPor,
                    [
                        new LineaMovimientoRequest(idGasto, acumuladoBeneficios, 0, $"Beneficios sociales {fecha:yyyy-MM}"),
                        new LineaMovimientoRequest(idPasivo, 0, acumuladoBeneficios, $"Beneficios sociales {fecha:yyyy-MM}"),
                    ]),
                cancellationToken);
            idsComprobante.Add(resultado.Id);
        }

        if (acumuladoFondosReserva > 0)
        {
            var idGasto = await IdCuenta(CodigoGastoFondosReserva, cancellationToken);
            var idPasivo = await IdCuenta(CodigoPasivoFondosReserva, cancellationToken);
            var resultado = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    fecha, IdTipoComprobanteDiario, 1,
                    $"Devengo de fondos de reserva — {fecha:yyyy-MM}", registradoPor,
                    [
                        new LineaMovimientoRequest(idGasto, acumuladoFondosReserva, 0, $"Fondos de reserva {fecha:yyyy-MM}"),
                        new LineaMovimientoRequest(idPasivo, 0, acumuladoFondosReserva, $"Fondos de reserva {fecha:yyyy-MM}"),
                    ]),
                cancellationToken);
            idsComprobante.Add(resultado.Id);
        }

        if (acumuladoAportePatronal > 0)
        {
            var idGasto = await IdCuenta(CodigoGastoAportePatronal, cancellationToken);
            var idPasivo = await IdCuenta(CodigoPasivoAportePatronal, cancellationToken);
            var resultado = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    fecha, IdTipoComprobanteDiario, 1,
                    $"Devengo de aporte patronal IESS — {fecha:yyyy-MM}", registradoPor,
                    [
                        new LineaMovimientoRequest(idGasto, acumuladoAportePatronal, 0, $"Aporte patronal {fecha:yyyy-MM}"),
                        new LineaMovimientoRequest(idPasivo, 0, acumuladoAportePatronal, $"Aporte patronal {fecha:yyyy-MM}"),
                    ]),
                cancellationToken);
            idsComprobante.Add(resultado.Id);
        }

        return new DevengoBeneficiosResult(
            fecha, procesados, totalDecimoTercero, totalDecimoCuarto, totalFondosReserva, totalProvisionVacaciones,
            totalAportePatronal, idsComprobante);
    }

    public async Task<PagoBeneficioResult> PagarDecimoTerceroAsync(
        Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var registro = await db.EmpleadosDecimoTercero.FirstOrDefaultAsync(d => d.IdEmpleado == idEmpleado, cancellationToken);
        if (registro is null) throw new EmpleadoSinBeneficioException(idEmpleado, "Décimo Tercero");
        return await LiquidarAsync(registro, r => r.Acumulado, r => r.Pagado, (r, v) => r.Pagado = v,
            "Décimo Tercero", CodigoPasivoBeneficios, registradoPor, cancellationToken);
    }

    public async Task<PagoBeneficioResult> PagarDecimoCuartoAsync(
        Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var registro = await db.EmpleadosDecimoCuarto.FirstOrDefaultAsync(d => d.IdEmpleado == idEmpleado, cancellationToken);
        if (registro is null) throw new EmpleadoSinBeneficioException(idEmpleado, "Décimo Cuarto");
        return await LiquidarAsync(registro, r => r.Acumulado, r => r.Pagado, (r, v) => r.Pagado = v,
            "Décimo Cuarto", CodigoPasivoBeneficios, registradoPor, cancellationToken);
    }

    public async Task<PagoBeneficioResult> PagarFondosReservaAsync(
        Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var registro = await db.EmpleadosFondosReserva.FirstOrDefaultAsync(d => d.IdEmpleado == idEmpleado, cancellationToken);
        if (registro is null) throw new EmpleadoSinBeneficioException(idEmpleado, "Fondos de Reserva");
        return await LiquidarAsync(registro, r => r.Acumulado, r => r.Pagado, (r, v) => r.Pagado = v,
            "Fondos de Reserva", CodigoPasivoFondosReserva, registradoPor, cancellationToken);
    }

    public async Task<PagoBeneficioResult> PagarVacacionesAsync(
        Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var registro = await db.EmpleadosProvisionVacacion.FirstOrDefaultAsync(d => d.IdEmpleado == idEmpleado, cancellationToken);
        if (registro is null) throw new EmpleadoSinBeneficioException(idEmpleado, "Provisión de Vacaciones");
        return await LiquidarAsync(registro, r => r.Acumulado, r => r.Pagado, (r, v) => r.Pagado = v,
            "Provisión de Vacaciones", CodigoPasivoBeneficios, registradoPor, cancellationToken);
    }

    public async Task<PagoBeneficioResult> PagarAportePatronalAsync(
        Guid idEmpleado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var registro = await db.EmpleadosAportePatronal.FirstOrDefaultAsync(d => d.IdEmpleado == idEmpleado, cancellationToken);
        if (registro is null) throw new EmpleadoSinBeneficioException(idEmpleado, "Aporte Patronal");
        return await LiquidarAsync(registro, r => r.Acumulado, r => r.Pagado, (r, v) => r.Pagado = v,
            "Aporte Patronal", CodigoPasivoAportePatronal, registradoPor, cancellationToken);
    }

    private async Task<PagoBeneficioResult> LiquidarAsync<T>(
        T registro, Func<T, decimal> acumulado, Func<T, decimal> pagado, Action<T, decimal> setPagado,
        string nombreBeneficio, string codigoCuentaPasivo, string registradoPor, CancellationToken cancellationToken)
        where T : class
    {
        var pendiente = acumulado(registro) - pagado(registro);
        if (pendiente <= 0)
        {
            var idEmpleado = registro switch
            {
                EmpleadoDecimoTercero d => d.IdEmpleado,
                EmpleadoDecimoCuarto d => d.IdEmpleado,
                EmpleadoFondosReserva d => d.IdEmpleado,
                EmpleadoProvisionVacacion d => d.IdEmpleado,
                EmpleadoAportePatronal d => d.IdEmpleado,
                _ => Guid.Empty,
            };
            throw new BeneficioSinSaldoException(idEmpleado, nombreBeneficio);
        }

        setPagado(registro, acumulado(registro));
        await db.SaveChangesAsync(cancellationToken);

        var idPasivo = await IdCuenta(codigoCuentaPasivo, cancellationToken);
        var idCaja = await IdCuenta(CodigoCaja, cancellationToken);
        var fecha = DateOnly.FromDateTime(DateTime.UtcNow);

        var resultado = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                fecha, IdTipoComprobanteDiario, 1, $"Pago de {nombreBeneficio}", registradoPor,
                [
                    new LineaMovimientoRequest(idPasivo, pendiente, 0, $"Pago de {nombreBeneficio}"),
                    new LineaMovimientoRequest(idCaja, 0, pendiente, $"Pago de {nombreBeneficio}"),
                ]),
            cancellationToken);

        var idEmpleadoFinal = registro switch
        {
            EmpleadoDecimoTercero d => d.IdEmpleado,
            EmpleadoDecimoCuarto d => d.IdEmpleado,
            EmpleadoFondosReserva d => d.IdEmpleado,
            EmpleadoProvisionVacacion d => d.IdEmpleado,
            EmpleadoAportePatronal d => d.IdEmpleado,
            _ => Guid.Empty,
        };

        return new PagoBeneficioResult(idEmpleadoFinal, nombreBeneficio, pendiente, resultado.Id);
    }

    private async Task<Guid> IdCuenta(string codigo, CancellationToken cancellationToken)
    {
        var id = await db.CuentasContables.Where(c => c.Codigo == codigo).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        return id ?? throw new InvalidOperationException($"Falta la cuenta contable {codigo}.");
    }
}
