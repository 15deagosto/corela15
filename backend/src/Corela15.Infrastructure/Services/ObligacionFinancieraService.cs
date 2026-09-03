using Corela15.Application.Obligacion;
using Corela15.Domain.Obligacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ObligacionFinancieraService(Corela15DbContext db) : IObligacionFinancieraService
{
    public async Task<ObligacionFinancieraDto> RegistrarAsync(RegistrarObligacionFinancieraRequest request, CancellationToken cancellationToken = default)
    {
        // Campo 1 (Tabla 02, restringido para OF01): solo R (RUC, persona
        // jurídica) o X (extranjero) — el resto del catálogo general de
        // tipos de identificación no aplica a un acreedor institucional.
        if (request.TipoIdentificacionAcreedor is not ("R" or "X"))
            throw new TipoIdentificacionAcreedorInvalidoException(request.TipoIdentificacionAcreedor);

        var pais = await db.Nacionalidades
            .FirstOrDefaultAsync(n => n.Codigo == request.CodigoPaisAcreedor && n.Activo, cancellationToken);
        if (pais is null) throw new PaisAcreedorInvalidoException(request.CodigoPaisAcreedor);

        var estado = await db.Set<EstadoObligacionFinanciera>()
            .FirstOrDefaultAsync(e => e.Codigo == request.CodigoEstado && e.Activo, cancellationToken);
        if (estado is null) throw new EstadoObligacionInvalidoException(request.CodigoEstado);

        var periodicidad = await db.Set<PeriodicidadPago>()
            .FirstOrDefaultAsync(p => p.Codigo == request.CodigoPeriodicidadPago && p.Activo, cancellationToken);
        if (periodicidad is null) throw new PeriodicidadPagoInvalidaException(request.CodigoPeriodicidadPago);

        var clase = await db.Set<ClaseObligacionFinanciera>()
            .FirstOrDefaultAsync(c => c.Codigo == request.CodigoClase && c.Activo, cancellationToken);
        if (clase is null) throw new ClaseObligacionInvalidaException(request.CodigoClase);

        FormaCancelacionObligacion? formaCancelacion = null;
        if (request.CodigoFormaCancelacion is not null)
        {
            formaCancelacion = await db.Set<FormaCancelacionObligacion>()
                .FirstOrDefaultAsync(f => f.Codigo == request.CodigoFormaCancelacion && f.Activo, cancellationToken);
            if (formaCancelacion is null) throw new FormaCancelacionObligacionInvalidaException(request.CodigoFormaCancelacion);
        }

        // Campos 10-11 (Tabla 82): cuenta de detalle real, grupo 26
        // (Obligaciones Financieras) del CUC ya sembrado.
        var cuenta = await db.CuentasContables
            .FirstOrDefaultAsync(c => c.Id == request.IdCuentaContable && c.Activa && c.EsMayor && c.Codigo.StartsWith("26"), cancellationToken);
        if (cuenta is null) throw new CuentaContableObligacionInvalidaException(request.IdCuentaContable);

        ValidarReglasNegocio(
            request.MontoLineaCredito, request.MontoPorUtilizar, request.CodigoEstado, request.Saldo,
            request.PagaComision, request.TasaInteresComision, request.ValorComision,
            request.TienePeriodoGracia, request.NumeroPeriodosGracia,
            request.ValorVencido, request.CodigoFormaCancelacion);

        // Control real de duplicados (§4): mismo acreedor + número de
        // obligación + cuenta contable, todavía activo (no Cancelada).
        var yaExiste = await db.ObligacionesFinancieras.AnyAsync(o =>
            o.TipoIdentificacionAcreedor == request.TipoIdentificacionAcreedor &&
            o.IdentificacionAcreedor == request.IdentificacionAcreedor &&
            o.NumeroObligacion == request.NumeroObligacion &&
            o.IdCuentaContable == request.IdCuentaContable &&
            o.CodigoEstado != "CN", cancellationToken);
        if (yaExiste) throw new ObligacionDuplicadaException(request.NumeroObligacion);

        var obligacion = new ObligacionFinanciera
        {
            Id = Guid.NewGuid(),
            IdAgencia = request.IdAgencia,
            TipoIdentificacionAcreedor = request.TipoIdentificacionAcreedor,
            IdentificacionAcreedor = request.IdentificacionAcreedor,
            CodigoPaisAcreedor = request.CodigoPaisAcreedor,
            NumeroObligacion = request.NumeroObligacion,
            DestinoLineaCredito = request.DestinoLineaCredito,
            MontoLineaCredito = request.MontoLineaCredito,
            MontoPorUtilizar = request.MontoPorUtilizar,
            CodigoEstado = request.CodigoEstado,
            Saldo = request.Saldo,
            IdCuentaContable = request.IdCuentaContable,
            TasaInteres = request.TasaInteres,
            InteresesPorPagar = request.InteresesPorPagar,
            PagaComision = request.PagaComision,
            TasaInteresComision = request.TasaInteresComision,
            ValorComision = request.ValorComision,
            FechaConcesion = request.FechaConcesion,
            FechaVencimiento = request.FechaVencimiento,
            CodigoPeriodicidadPago = request.CodigoPeriodicidadPago,
            TienePeriodoGracia = request.TienePeriodoGracia,
            NumeroPeriodosGracia = request.NumeroPeriodosGracia,
            CodigoClase = request.CodigoClase,
            ValorVencido = request.ValorVencido,
            CodigoFormaCancelacion = request.CodigoFormaCancelacion,
            NumeroObligacionAnterior = request.NumeroObligacionAnterior,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };

        db.ObligacionesFinancieras.Add(obligacion);
        await db.SaveChangesAsync(cancellationToken);

        return await MapearAsync(obligacion.Id, cancellationToken);
    }

    public async Task<ObligacionFinancieraDto> ActualizarAsync(Guid id, ActualizarObligacionFinancieraRequest request, CancellationToken cancellationToken = default)
    {
        var obligacion = await db.ObligacionesFinancieras.FirstOrDefaultAsync(o => o.Id == id, cancellationToken)
            ?? throw new ObligacionFinancieraNoExisteException(id);

        var estado = await db.Set<EstadoObligacionFinanciera>()
            .FirstOrDefaultAsync(e => e.Codigo == request.CodigoEstado && e.Activo, cancellationToken);
        if (estado is null) throw new EstadoObligacionInvalidoException(request.CodigoEstado);

        FormaCancelacionObligacion? formaCancelacion = null;
        if (request.CodigoFormaCancelacion is not null)
        {
            formaCancelacion = await db.Set<FormaCancelacionObligacion>()
                .FirstOrDefaultAsync(f => f.Codigo == request.CodigoFormaCancelacion && f.Activo, cancellationToken);
            if (formaCancelacion is null) throw new FormaCancelacionObligacionInvalidaException(request.CodigoFormaCancelacion);
        }

        ValidarReglasNegocio(
            obligacion.MontoLineaCredito, request.MontoPorUtilizar, request.CodigoEstado, request.Saldo,
            request.PagaComision, request.TasaInteresComision, request.ValorComision,
            request.TienePeriodoGracia, request.NumeroPeriodosGracia,
            request.ValorVencido, request.CodigoFormaCancelacion);

        obligacion.MontoPorUtilizar = request.MontoPorUtilizar;
        obligacion.CodigoEstado = request.CodigoEstado;
        obligacion.Saldo = request.Saldo;
        obligacion.TasaInteres = request.TasaInteres;
        obligacion.InteresesPorPagar = request.InteresesPorPagar;
        obligacion.PagaComision = request.PagaComision;
        obligacion.TasaInteresComision = request.TasaInteresComision;
        obligacion.ValorComision = request.ValorComision;
        obligacion.TienePeriodoGracia = request.TienePeriodoGracia;
        obligacion.NumeroPeriodosGracia = request.NumeroPeriodosGracia;
        obligacion.ValorVencido = request.ValorVencido;
        obligacion.CodigoFormaCancelacion = request.CodigoFormaCancelacion;
        obligacion.ModificadoEn = DateTimeOffset.UtcNow;
        obligacion.ModificadoPor = request.ModificadoPor;

        await db.SaveChangesAsync(cancellationToken);

        return await MapearAsync(obligacion.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<ObligacionFinancieraDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await db.ObligacionesFinancieras
            .Include(o => o.Agencia)
            .Include(o => o.PaisAcreedor)
            .Include(o => o.Estado)
            .Include(o => o.CuentaContable)
            .Include(o => o.PeriodicidadPago)
            .Include(o => o.Clase)
            .Include(o => o.FormaCancelacion)
            .OrderByDescending(o => o.CreadoEn)
            .Select(o => new ObligacionFinancieraDto(
                o.Id, o.Agencia.Nombre,
                o.TipoIdentificacionAcreedor, o.IdentificacionAcreedor, o.CodigoPaisAcreedor, o.PaisAcreedor.Nombre,
                o.NumeroObligacion, o.DestinoLineaCredito, o.MontoLineaCredito, o.MontoPorUtilizar,
                o.CodigoEstado, o.Estado.Nombre, o.Saldo,
                o.CuentaContable.Codigo, o.CuentaContable.Nombre,
                o.TasaInteres, o.InteresesPorPagar, o.PagaComision, o.TasaInteresComision, o.ValorComision,
                o.FechaConcesion, o.FechaVencimiento, o.CodigoPeriodicidadPago, o.PeriodicidadPago.Nombre,
                o.TienePeriodoGracia, o.NumeroPeriodosGracia,
                o.CodigoClase, o.Clase.Nombre, o.ValorVencido,
                o.CodigoFormaCancelacion, o.FormaCancelacion == null ? null : o.FormaCancelacion.Nombre,
                o.NumeroObligacionAnterior))
            .ToListAsync(cancellationToken);
    }

    private async Task<ObligacionFinancieraDto> MapearAsync(Guid id, CancellationToken cancellationToken)
    {
        return (await ListarInternalAsync(id, cancellationToken))!;
    }

    private async Task<ObligacionFinancieraDto?> ListarInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.ObligacionesFinancieras
            .Include(o => o.Agencia)
            .Include(o => o.PaisAcreedor)
            .Include(o => o.Estado)
            .Include(o => o.CuentaContable)
            .Include(o => o.PeriodicidadPago)
            .Include(o => o.Clase)
            .Include(o => o.FormaCancelacion)
            .Where(o => o.Id == id)
            .Select(o => new ObligacionFinancieraDto(
                o.Id, o.Agencia.Nombre,
                o.TipoIdentificacionAcreedor, o.IdentificacionAcreedor, o.CodigoPaisAcreedor, o.PaisAcreedor.Nombre,
                o.NumeroObligacion, o.DestinoLineaCredito, o.MontoLineaCredito, o.MontoPorUtilizar,
                o.CodigoEstado, o.Estado.Nombre, o.Saldo,
                o.CuentaContable.Codigo, o.CuentaContable.Nombre,
                o.TasaInteres, o.InteresesPorPagar, o.PagaComision, o.TasaInteresComision, o.ValorComision,
                o.FechaConcesion, o.FechaVencimiento, o.CodigoPeriodicidadPago, o.PeriodicidadPago.Nombre,
                o.TienePeriodoGracia, o.NumeroPeriodosGracia,
                o.CodigoClase, o.Clase.Nombre, o.ValorVencido,
                o.CodigoFormaCancelacion, o.FormaCancelacion == null ? null : o.FormaCancelacion.Nombre,
                o.NumeroObligacionAnterior))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Reglas de negocio reales del §4 del manual OF01: monto por utilizar
    /// nunca excede la línea de crédito; una obligación Cancelada (CN)
    /// siempre tiene saldo cero; los campos condicionales (comisión,
    /// período de gracia, valor vencido, forma de cancelación) son
    /// obligatorios exactamente cuando su bandera/estado los activa.
    /// </summary>
    private static void ValidarReglasNegocio(
        decimal montoLineaCredito, decimal montoPorUtilizar, string codigoEstado, decimal saldo,
        bool pagaComision, decimal? tasaComision, decimal? valorComision,
        bool tienePeriodoGracia, int? numeroPeriodosGracia,
        decimal? valorVencido, string? codigoFormaCancelacion)
    {
        if (montoPorUtilizar > montoLineaCredito)
            throw new MontoPorUtilizarExcedeLineaException(montoPorUtilizar, montoLineaCredito);

        if (codigoEstado == "CN" && saldo != 0m)
            throw new SaldoObligacionCanceladaException();

        if (pagaComision && (tasaComision is null || valorComision is null))
            throw new DatosComisionIncompletosException();

        if (tienePeriodoGracia && numeroPeriodosGracia is null)
            throw new DatosPeriodoGraciaIncompletosException();

        if (codigoEstado == "VN" && valorVencido is null)
            throw new ValorVencidoRequeridoException();

        if (codigoEstado == "CN" && codigoFormaCancelacion is null)
            throw new FormaCancelacionRequeridaException();
    }
}
