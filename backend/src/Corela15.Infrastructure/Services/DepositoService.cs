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

        // Nota: devuelve el capital nominal — el cálculo de interés devengado
        // a la fecha de corte (proporcional si se cancela antes del
        // vencimiento) queda pendiente, ver CLAUDE.md.
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow),
                tipoTransaccion.IdTipoComprobante,
                deposito.IdAgencia,
                $"Cancelación DPF {deposito.Codigo}",
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, deposito.Monto, 0, $"Cancelación DPF {deposito.Codigo}"),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, deposito.Monto, $"Devolución capital DPF {deposito.Codigo}"),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new DepositoCanceladoResult(deposito.Monto, resultadoComprobante.Id);
    }
}
