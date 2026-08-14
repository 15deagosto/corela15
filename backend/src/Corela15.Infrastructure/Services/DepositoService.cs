using Corela15.Application.Ahorros;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Application.Inversion;
using Corela15.Domain.Clientes;
using Corela15.Domain.Inversion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class DepositoService(Corela15DbContext db, IComprobanteContableService comprobantes) : IDepositoService
{
    private const string CodigoTipoTransaccionApertura = "APER-DPF";
    private const string CodigoTipoTransaccionCancelacion = "CANC-DPF";
    private const string CodigoCuentaIntereses = "4101";
    private const string CodigoCuentaCaja = "1101";

    public async Task<DepositoAbiertoResult> AbrirAsync(
        AbrirDepositoRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.IdCliente, cancellationToken);
        if (cliente is null || cliente.Estado != EstadoCliente.Activo)
        {
            throw new ClienteInvalidoParaDepositoException(request.IdCliente);
        }

        var tipoPersona = request.EsPersonaJuridica ? TipoPersonaTasa.Juridica : TipoPersonaTasa.Natural;
        var itemTasa = await db.ItemsPlazoTasa
            .Where(t => t.Activo
                && t.PlazoDiasMin <= request.PlazoDias && t.PlazoDiasMax >= request.PlazoDias
                && t.MontoMin <= request.Monto && (t.MontoMax == null || t.MontoMax >= request.Monto)
                && (t.TipoPersona == TipoPersonaTasa.Ambas || t.TipoPersona == tipoPersona))
            .OrderByDescending(t => t.FechaVigenciaDesde)
            .FirstOrDefaultAsync(cancellationToken);
        if (itemTasa is null)
        {
            throw new SinTasaVigenteException(request.PlazoDias, request.Monto);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionApertura, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionApertura);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var ultimoNumero = await db.Depositos.CountAsync(cancellationToken);
        var codigo = "DPF" + (ultimoNumero + 1).ToString().PadLeft(9, '0');
        var fechaCreacion = DateOnly.FromDateTime(DateTime.UtcNow);

        var deposito = new Deposito
        {
            Id = Guid.NewGuid(),
            Codigo = codigo,
            IdAgencia = request.IdAgencia,
            Monto = request.Monto,
            Tasa = itemTasa.Tasa,
            VariacionTasa = 0,
            PlazoDias = request.PlazoDias,
            PagoPeriodicoInteres = false,
            FechaCreacion = fechaCreacion,
            FechaVencimiento = fechaCreacion.AddDays(request.PlazoDias),
            Estado = EstadoDeposito.Vigente,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.Depositos.Add(deposito);

        db.DepositosClientes.Add(new DepositoCliente
        {
            IdDeposito = deposito.Id,
            IdCliente = request.IdCliente,
            Principal = true,
            Activo = true,
        });

        await db.SaveChangesAsync(cancellationToken);

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                fechaCreacion,
                tipoTransaccion.IdTipoComprobante,
                request.IdAgencia,
                $"Apertura DPF {codigo}",
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, request.Monto, 0, $"Apertura DPF {codigo}"),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, request.Monto, $"Captación DPF {codigo}"),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new DepositoAbiertoResult(deposito.Id, codigo, itemTasa.Tasa, deposito.FechaVencimiento, resultadoComprobante.Id);
    }

    public async Task<DepositoCanceladoResult> CancelarAsync(
        CancelarDepositoRequest request, CancellationToken cancellationToken = default)
    {
        var deposito = await db.Depositos.FirstOrDefaultAsync(d => d.Id == request.IdDeposito, cancellationToken);
        if (deposito is null || deposito.Estado != EstadoDeposito.Vigente)
        {
            throw new DepositoInvalidoException(request.IdDeposito);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionCancelacion, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionCancelacion);
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var diasTranscurridos = Math.Clamp(hoy.DayNumber - deposito.FechaCreacion.DayNumber, 0, deposito.PlazoDias);
        var interesDevengado = Math.Round(deposito.Monto * deposito.Tasa * diasTranscurridos / 365m, 2, MidpointRounding.AwayFromZero);

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        deposito.Estado = EstadoDeposito.Cancelado;
        deposito.ModificadoEn = DateTimeOffset.UtcNow;
        deposito.ModificadoPor = request.RegistradoPor;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El depósito {deposito.Codigo}");
        }

        var lineas = new List<LineaMovimientoRequest>
        {
            new(tipoTransaccion.IdCuentaContableDebito, deposito.Monto, 0, $"Cancelación DPF {deposito.Codigo}"),
            new(tipoTransaccion.IdCuentaContableCredito, 0, deposito.Monto, $"Devolución capital DPF {deposito.Codigo}"),
        };
        if (interesDevengado > 0)
        {
            var idCuentaIntereses = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaIntereses).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var idCuentaCaja = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            if (idCuentaIntereses is null || idCuentaCaja is null)
            {
                throw new InvalidOperationException($"Faltan las cuentas contables {CodigoCuentaIntereses}/{CodigoCuentaCaja}.");
            }

            lineas.Add(new LineaMovimientoRequest(idCuentaIntereses.Value, interesDevengado, 0, $"Interés devengado DPF {deposito.Codigo} ({diasTranscurridos} días)"));
            lineas.Add(new LineaMovimientoRequest(idCuentaCaja.Value, 0, interesDevengado, $"Pago de interés DPF {deposito.Codigo}"));
        }

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                hoy,
                tipoTransaccion.IdTipoComprobante,
                deposito.IdAgencia,
                $"Cancelación DPF {deposito.Codigo}",
                request.RegistradoPor,
                lineas.ToArray()),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new DepositoCanceladoResult(deposito.Monto, interesDevengado, resultadoComprobante.Id);
    }

    public async Task<DepositoRenovadoResult> RenovarAsync(
        RenovarDepositoRequest request, CancellationToken cancellationToken = default)
    {
        var origen = await db.Depositos.FirstOrDefaultAsync(d => d.Id == request.IdDepositoOrigen, cancellationToken);
        if (origen is null || origen.Estado != EstadoDeposito.Vigente)
        {
            throw new DepositoInvalidoException(request.IdDepositoOrigen);
        }

        if (request.IncrementoCapital < 0)
        {
            throw new IncrementoCapitalInvalidoException(request.IncrementoCapital);
        }

        var montoNuevo = origen.Monto + request.IncrementoCapital;

        // Tasa vigente EN ESTE MOMENTO, nunca copiada del depósito origen —
        // ver el aprendizaje documentado en IDepositoService.RenovarAsync.
        var tipoPersona = request.EsPersonaJuridica ? TipoPersonaTasa.Juridica : TipoPersonaTasa.Natural;
        var itemTasa = await db.ItemsPlazoTasa
            .Where(t => t.Activo
                && t.PlazoDiasMin <= request.PlazoDias && t.PlazoDiasMax >= request.PlazoDias
                && t.MontoMin <= montoNuevo && (t.MontoMax == null || t.MontoMax >= montoNuevo)
                && (t.TipoPersona == TipoPersonaTasa.Ambas || t.TipoPersona == tipoPersona))
            .OrderByDescending(t => t.FechaVigenciaDesde)
            .FirstOrDefaultAsync(cancellationToken);
        if (itemTasa is null)
        {
            throw new SinTasaVigenteException(request.PlazoDias, montoNuevo);
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var ultimoNumero = await db.Depositos.CountAsync(cancellationToken);
        var codigoDestino = "DPF" + (ultimoNumero + 1).ToString().PadLeft(9, '0');
        var fechaRenovacion = DateOnly.FromDateTime(DateTime.UtcNow);

        var destino = new Deposito
        {
            Id = Guid.NewGuid(),
            Codigo = codigoDestino,
            IdAgencia = origen.IdAgencia,
            Monto = montoNuevo,
            Tasa = itemTasa.Tasa,
            VariacionTasa = 0,
            PlazoDias = request.PlazoDias,
            PagoPeriodicoInteres = origen.PagoPeriodicoInteres,
            FechaCreacion = fechaRenovacion,
            FechaVencimiento = fechaRenovacion.AddDays(request.PlazoDias),
            Estado = EstadoDeposito.Vigente,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.Depositos.Add(destino);

        var titulares = await db.DepositosClientes
            .Where(dc => dc.IdDeposito == origen.Id && dc.Activo)
            .ToListAsync(cancellationToken);
        foreach (var titular in titulares)
        {
            db.DepositosClientes.Add(new DepositoCliente
            {
                IdDeposito = destino.Id,
                IdCliente = titular.IdCliente,
                Principal = titular.Principal,
                Activo = true,
            });
        }

        origen.Estado = EstadoDeposito.Renovado;
        origen.ModificadoEn = DateTimeOffset.UtcNow;
        origen.ModificadoPor = request.RegistradoPor;

        db.DepositosRenovaciones.Add(new DepositoRenovacion
        {
            Id = Guid.NewGuid(),
            IdDepositoOrigen = origen.Id,
            IdDepositoDestino = destino.Id,
            Valor = montoNuevo,
            ValorIncremento = request.IncrementoCapital,
            FechaRenovacion = fechaRenovacion,
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El depósito {origen.Codigo}");
        }

        Guid? idComprobante = null;
        if (request.IncrementoCapital > 0)
        {
            var tipoTransaccion = await db.TiposTransaccion
                .FirstOrDefaultAsync(t => t.Codigo == CodigoTipoTransaccionApertura, cancellationToken);
            if (tipoTransaccion is null || !tipoTransaccion.Activo)
            {
                throw new TipoTransaccionInvalidoException(CodigoTipoTransaccionApertura);
            }

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    fechaRenovacion,
                    tipoTransaccion.IdTipoComprobante,
                    origen.IdAgencia,
                    $"Incremento de capital — renovación DPF {origen.Codigo} → {codigoDestino}",
                    request.RegistradoPor,
                    [
                        new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, request.IncrementoCapital, 0, "Incremento de capital en renovación"),
                        new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, request.IncrementoCapital, "Incremento de capital en renovación"),
                    ]),
                cancellationToken);

            idComprobante = resultadoComprobante.Id;
        }

        await transaccion.CommitAsync(cancellationToken);

        return new DepositoRenovadoResult(destino.Id, codigoDestino, montoNuevo, itemTasa.Tasa, destino.FechaVencimiento, idComprobante);
    }
}
