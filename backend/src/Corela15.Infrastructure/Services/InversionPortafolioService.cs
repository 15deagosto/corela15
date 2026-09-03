using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Application.Portafolio;
using Corela15.Domain.Portafolio;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Motor de inversiones propias de la cooperativa — económicamente el
/// espejo de <see cref="DepositoService"/> (Nivel 3) del lado activo: acá
/// la cooperativa coloca capital en otra institución en vez de recibirlo
/// de un socio. Mismas fórmulas de interés/renovación, cuentas reales del
/// grupo 13 (Inversiones) en vez del 21 (Depósitos).
/// </summary>
public class InversionPortafolioService(Corela15DbContext db, IComprobanteContableService comprobantes) : IInversionPortafolioService
{
    private const string CodigoCuentaCaja = "1101";
    private const string CodigoCuentaIngresoInversion = "510315"; // Intereses de inversiones mantenidas hasta el vencimiento
    private const int IdTipoComprobanteDiario = 3; // 'DIA'

    // Cuentas reales del grupo 13 (Mantenidas hasta su vencimiento) por
    // plazo — verificadas contra el CUC oficial. Sector financiero
    // popular y solidario (COAC/Caja Central, tipos 003/006) vs sector
    // privado (bancos/mutualistas/sociedades, 001/002/004/005) son series
    // distintas del mismo grupo, la clasificación real que exige el CUC.
    private static string ResolverCuentaInversion(int diasPlazo, bool esSectorFinancieroPopular)
    {
        // 1305xx real (Mantenidas hasta su vencimiento): serie 50-70 para
        // sector financiero popular y solidario, serie 05-25 para sector
        // privado — mismo grupo 13, sufijo distinto según el CUC oficial.
        var sufijo = (esSectorFinancieroPopular, diasPlazo) switch
        {
            (true, <= 30) => "50",
            (true, <= 90) => "55",
            (true, <= 180) => "60",
            (true, <= 360) => "65",
            (true, _) => "70",
            (false, <= 30) => "05",
            (false, <= 90) => "10",
            (false, <= 180) => "15",
            (false, <= 360) => "20",
            (false, _) => "25",
        };
        return "1305" + sufijo;
    }

    private static bool EsSectorFinancieroPopular(string codigoTipoInstitucion) =>
        codigoTipoInstitucion is "003" or "006";

    public async Task<InversionAbiertaResult> AbrirAsync(
        AbrirInversionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ValorNominal <= 0)
        {
            throw new MontoInversionInvalidoException(request.ValorNominal);
        }

        var institucion = await db.Instituciones.Include(i => i.TipoInstitucion)
            .FirstOrDefaultAsync(i => i.Codigo == request.CodigoInstitucion && i.Activa, cancellationToken);
        if (institucion is null)
        {
            throw new InstitucionInvalidaException(request.CodigoInstitucion);
        }

        var diasPlazo = request.FechaVencimiento.DayNumber - request.FechaCompra.DayNumber;
        var codigoCuentaInversion = ResolverCuentaInversion(diasPlazo, EsSectorFinancieroPopular(institucion.CodigoTipoInstitucion));

        var idCuentaInversion = await db.CuentasContables
            .Where(c => c.Codigo == codigoCuentaInversion).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaInversion is null || idCuentaCaja is null)
        {
            throw new InvalidOperationException($"Faltan las cuentas contables {codigoCuentaInversion}/{CodigoCuentaCaja}.");
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var inversion = new InversionPortafolio
        {
            Id = Guid.NewGuid(),
            Documento = request.Documento,
            IdAgencia = request.IdAgencia,
            CodigoInstitucion = request.CodigoInstitucion,
            ValorNominal = request.ValorNominal,
            Tasa = request.Tasa,
            FechaCompra = request.FechaCompra,
            FechaVencimiento = request.FechaVencimiento,
            Estado = EstadoInversionPortafolio.Activa,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.InversionesPortafolio.Add(inversion);
        await db.SaveChangesAsync(cancellationToken);

        var descripcion = $"Apertura inversión {request.Documento} — {institucion.Nombre}";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                request.FechaCompra, IdTipoComprobanteDiario, request.IdAgencia, descripcion, request.RegistradoPor,
                [
                    new LineaMovimientoRequest(idCuentaInversion.Value, request.ValorNominal, 0, descripcion),
                    new LineaMovimientoRequest(idCuentaCaja.Value, 0, request.ValorNominal, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new InversionAbiertaResult(inversion.Id, resultadoComprobante.Id);
    }

    public async Task<InversionCanceladaResult> CancelarAsync(
        CancelarInversionRequest request, CancellationToken cancellationToken = default)
    {
        var inversion = await db.InversionesPortafolio.Include(i => i.Institucion).ThenInclude(inst => inst.TipoInstitucion)
            .FirstOrDefaultAsync(i => i.Id == request.IdInversion, cancellationToken);
        if (inversion is null || inversion.Estado != EstadoInversionPortafolio.Activa)
        {
            throw new InversionInvalidaException(request.IdInversion);
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var plazoTotal = inversion.FechaVencimiento.DayNumber - inversion.FechaCompra.DayNumber;
        var diasTranscurridos = Math.Clamp(hoy.DayNumber - inversion.FechaCompra.DayNumber, 0, plazoTotal);
        var interesGanado = Math.Round(inversion.ValorNominal * inversion.Tasa * diasTranscurridos / 365m, 2, MidpointRounding.AwayFromZero);

        var codigoCuentaInversion = ResolverCuentaInversion(plazoTotal, EsSectorFinancieroPopular(inversion.Institucion.CodigoTipoInstitucion));
        var idCuentaInversion = await db.CuentasContables
            .Where(c => c.Codigo == codigoCuentaInversion).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaInversion is null || idCuentaCaja is null)
        {
            throw new InvalidOperationException($"Faltan las cuentas contables {codigoCuentaInversion}/{CodigoCuentaCaja}.");
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        inversion.Estado = EstadoInversionPortafolio.Cancelada;
        inversion.ModificadoEn = DateTimeOffset.UtcNow;
        inversion.ModificadoPor = request.RegistradoPor;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"La inversión {inversion.Documento}");
        }

        var descripcion = $"Cancelación inversión {inversion.Documento}";
        var lineas = new List<LineaMovimientoRequest>
        {
            new(idCuentaCaja.Value, inversion.ValorNominal, 0, descripcion),
            new(idCuentaInversion.Value, 0, inversion.ValorNominal, descripcion),
        };
        if (interesGanado > 0)
        {
            var idCuentaIngreso = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaIngresoInversion).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            if (idCuentaIngreso is null)
            {
                throw new InvalidOperationException($"Falta la cuenta contable {CodigoCuentaIngresoInversion}.");
            }

            lineas.Add(new LineaMovimientoRequest(idCuentaCaja.Value, interesGanado, 0, $"Interés ganado ({diasTranscurridos} días)"));
            lineas.Add(new LineaMovimientoRequest(idCuentaIngreso.Value, 0, interesGanado, $"Interés ganado inversión {inversion.Documento}"));
        }

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(hoy, IdTipoComprobanteDiario, inversion.IdAgencia, descripcion, request.RegistradoPor, lineas),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new InversionCanceladaResult(inversion.ValorNominal, interesGanado, resultadoComprobante.Id);
    }

    public async Task<InversionRenovadaResult> RenovarAsync(
        RenovarInversionRequest request, CancellationToken cancellationToken = default)
    {
        var origen = await db.InversionesPortafolio.Include(i => i.Institucion).ThenInclude(inst => inst.TipoInstitucion)
            .FirstOrDefaultAsync(i => i.Id == request.IdInversion, cancellationToken);
        if (origen is null || origen.Estado != EstadoInversionPortafolio.Activa)
        {
            throw new InversionInvalidaException(request.IdInversion);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var destino = new InversionPortafolio
        {
            Id = Guid.NewGuid(),
            Documento = request.DocumentoNuevo,
            IdAgencia = origen.IdAgencia,
            CodigoInstitucion = origen.CodigoInstitucion,
            ValorNominal = origen.ValorNominal,
            Tasa = origen.Tasa,
            FechaCompra = DateOnly.FromDateTime(DateTime.UtcNow),
            FechaVencimiento = request.FechaVencimientoNueva,
            Estado = EstadoInversionPortafolio.Activa,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.InversionesPortafolio.Add(destino);

        origen.Estado = EstadoInversionPortafolio.Cancelada;
        origen.ModificadoEn = DateTimeOffset.UtcNow;
        origen.ModificadoPor = request.RegistradoPor;

        db.InversionesRenovacion.Add(new InversionRenovacion
        {
            Id = Guid.NewGuid(),
            IdInversionOrigen = origen.Id,
            IdInversionDestino = destino.Id,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"La inversión {origen.Documento}");
        }

        // El origen deja de correr interés al renovarse — se liquida en
        // efectivo el interés devengado hasta hoy, mismo criterio ya
        // aplicado en DepositoService.RenovarAsync.
        var fechaRenovacion = destino.FechaCompra;
        var plazoOrigen = origen.FechaVencimiento.DayNumber - origen.FechaCompra.DayNumber;
        var diasTranscurridos = Math.Clamp(fechaRenovacion.DayNumber - origen.FechaCompra.DayNumber, 0, plazoOrigen);
        var interesGanado = Math.Round(origen.ValorNominal * origen.Tasa * diasTranscurridos / 365m, 2, MidpointRounding.AwayFromZero);

        Guid? idComprobante = null;
        if (interesGanado > 0)
        {
            var idCuentaIngreso = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaIngresoInversion).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var idCuentaCaja = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            if (idCuentaIngreso is null || idCuentaCaja is null)
            {
                throw new InvalidOperationException($"Faltan las cuentas contables {CodigoCuentaIngresoInversion}/{CodigoCuentaCaja}.");
            }

            var descripcion = $"Interés ganado inversión {origen.Documento} al renovar → {request.DocumentoNuevo}";
            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    fechaRenovacion, IdTipoComprobanteDiario, origen.IdAgencia, descripcion, request.RegistradoPor,
                    [
                        new LineaMovimientoRequest(idCuentaCaja.Value, interesGanado, 0, descripcion),
                        new LineaMovimientoRequest(idCuentaIngreso.Value, 0, interesGanado, descripcion),
                    ]),
                cancellationToken);

            idComprobante = resultadoComprobante.Id;
        }

        await transaccion.CommitAsync(cancellationToken);

        return new InversionRenovadaResult(destino.Id, destino.Documento, destino.ValorNominal, destino.Tasa, interesGanado, idComprobante);
    }
}
