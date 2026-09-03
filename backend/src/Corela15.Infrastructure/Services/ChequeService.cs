using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Application.Financiero;
using Corela15.Domain.Financiero;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Motor real de cheques de terceros recibidos en depósito — primer caso
/// de uso del módulo `Financiero` (ver CLAUDE.md, "Financiero — cheques
/// como instrumento de pago real"). Verificado contra FINANCIERO.CHEQUE.
/// </summary>
public class ChequeService(Corela15DbContext db, IComprobanteContableService comprobantes) : IChequeService
{
    private const string CodigoCuentaCanje = "110401"; // Efectos de cobro inmediato
    private const string CodigoCuentaDepositos = "2101"; // Depósitos de ahorro a la vista
    private const string CodigoCuentaCaja = "1101";
    private const string CodigoItemSaldoBloqueado = "BLOQ";
    private const string CodigoItemSaldoDisponible = "DISP";
    private const int IdTipoComprobanteDiario = 3; // 'DIA'

    public async Task<ChequeRegistradoResult> RegistrarAsync(
        RegistrarChequeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Valor <= 0)
        {
            throw new MontoChequeInvalidoException(request.Valor);
        }

        var bancoValido = await db.Bancos.AnyAsync(b => b.Id == request.IdBanco && b.Activo, cancellationToken);
        if (!bancoValido)
        {
            throw new BancoInvalidoException(request.IdBanco);
        }

        var cheque = new Cheque
        {
            Id = Guid.NewGuid(),
            IdBanco = request.IdBanco,
            CuentaCorriente = request.CuentaCorriente,
            NumeroCheque = request.NumeroCheque,
            Valor = request.Valor,
            IdCuenta = request.IdCuenta,
            IdAgencia = request.IdAgencia,
            FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow),
            Estado = EstadoCheque.Ingresado,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.Cheques.Add(cheque);
        await db.SaveChangesAsync(cancellationToken);

        return new ChequeRegistradoResult(cheque.Id);
    }

    public async Task<ChequeDepositadoResult> DepositarAsync(
        DepositarChequeRequest request, CancellationToken cancellationToken = default)
    {
        var cheque = await db.Cheques.FirstOrDefaultAsync(c => c.Id == request.IdCheque, cancellationToken);
        if (cheque is null || cheque.Estado != EstadoCheque.Ingresado)
        {
            throw new ChequeInvalidoException(request.IdCheque, "Ingresado");
        }

        var (idCuentaCanje, idCuentaDepositos) = await ResolverCuentasAsync(cancellationToken);

        var itemBloqueado = await db.CuentasItemSaldo.Include(x => x.ItemSaldo)
            .FirstOrDefaultAsync(x => x.IdCuenta == cheque.IdCuenta && x.ItemSaldo.Codigo == CodigoItemSaldoBloqueado, cancellationToken);
        if (itemBloqueado is null)
        {
            throw new InvalidOperationException($"La cuenta {cheque.IdCuenta} no tiene el balde Bloqueado configurado.");
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        cheque.Estado = EstadoCheque.DepositadoEnBanco;
        cheque.ModificadoEn = DateTimeOffset.UtcNow;
        cheque.ModificadoPor = request.RegistradoPor;

        itemBloqueado.Saldo += cheque.Valor;
        itemBloqueado.ModificadoEn = DateTimeOffset.UtcNow;
        itemBloqueado.ModificadoPor = request.RegistradoPor;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El cheque {cheque.NumeroCheque}");
        }

        var descripcion = $"Depósito cheque {cheque.NumeroCheque}";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, cheque.IdAgencia, descripcion, request.RegistradoPor,
                [
                    new LineaMovimientoRequest(idCuentaCanje, cheque.Valor, 0, descripcion),
                    new LineaMovimientoRequest(idCuentaDepositos, 0, cheque.Valor, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new ChequeDepositadoResult(resultadoComprobante.Id);
    }

    public async Task<ChequeEfectivizadoResult> EfectivizarAsync(
        EfectivizarChequeRequest request, CancellationToken cancellationToken = default)
    {
        var cheque = await db.Cheques.FirstOrDefaultAsync(c => c.Id == request.IdCheque, cancellationToken);
        if (cheque is null || cheque.Estado != EstadoCheque.DepositadoEnBanco)
        {
            throw new ChequeInvalidoException(request.IdCheque, "DepositadoEnBanco");
        }

        var idCuentaCanje = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCanje).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaCaja = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaCanje is null || idCuentaCaja is null)
        {
            throw new InvalidOperationException($"Faltan las cuentas contables {CodigoCuentaCanje}/{CodigoCuentaCaja}.");
        }

        var itemBloqueado = await db.CuentasItemSaldo.Include(x => x.ItemSaldo)
            .FirstOrDefaultAsync(x => x.IdCuenta == cheque.IdCuenta && x.ItemSaldo.Codigo == CodigoItemSaldoBloqueado, cancellationToken);
        var itemDisponible = await db.CuentasItemSaldo.Include(x => x.ItemSaldo)
            .FirstOrDefaultAsync(x => x.IdCuenta == cheque.IdCuenta && x.ItemSaldo.Codigo == CodigoItemSaldoDisponible, cancellationToken);
        if (itemBloqueado is null || itemDisponible is null)
        {
            throw new InvalidOperationException($"La cuenta {cheque.IdCuenta} no tiene los baldes Bloqueado/Disponible configurados.");
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        cheque.Estado = EstadoCheque.Efectivizado;
        cheque.ModificadoEn = DateTimeOffset.UtcNow;
        cheque.ModificadoPor = request.RegistradoPor;

        itemBloqueado.Saldo -= cheque.Valor;
        itemDisponible.Saldo += cheque.Valor;
        itemBloqueado.ModificadoEn = itemDisponible.ModificadoEn = DateTimeOffset.UtcNow;
        itemBloqueado.ModificadoPor = itemDisponible.ModificadoPor = request.RegistradoPor;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El cheque {cheque.NumeroCheque}");
        }

        var descripcion = $"Efectivización cheque {cheque.NumeroCheque}";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, cheque.IdAgencia, descripcion, request.RegistradoPor,
                [
                    new LineaMovimientoRequest(idCuentaCaja.Value, cheque.Valor, 0, descripcion),
                    new LineaMovimientoRequest(idCuentaCanje.Value, 0, cheque.Valor, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new ChequeEfectivizadoResult(resultadoComprobante.Id);
    }

    public async Task<ChequeProtestadoResult> ProtestarAsync(
        ProtestarChequeRequest request, CancellationToken cancellationToken = default)
    {
        var cheque = await db.Cheques.FirstOrDefaultAsync(c => c.Id == request.IdCheque, cancellationToken);
        if (cheque is null || cheque.Estado != EstadoCheque.DepositadoEnBanco)
        {
            throw new ChequeInvalidoException(request.IdCheque, "DepositadoEnBanco");
        }

        var (idCuentaCanje, idCuentaDepositos) = await ResolverCuentasAsync(cancellationToken);

        var itemBloqueado = await db.CuentasItemSaldo.Include(x => x.ItemSaldo)
            .FirstOrDefaultAsync(x => x.IdCuenta == cheque.IdCuenta && x.ItemSaldo.Codigo == CodigoItemSaldoBloqueado, cancellationToken);
        if (itemBloqueado is null)
        {
            throw new InvalidOperationException($"La cuenta {cheque.IdCuenta} no tiene el balde Bloqueado configurado.");
        }

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        cheque.Estado = EstadoCheque.Protestado;
        cheque.ModificadoEn = DateTimeOffset.UtcNow;
        cheque.ModificadoPor = request.RegistradoPor;

        itemBloqueado.Saldo -= cheque.Valor;
        itemBloqueado.ModificadoEn = DateTimeOffset.UtcNow;
        itemBloqueado.ModificadoPor = request.RegistradoPor;

        db.ChequesProtesto.Add(new ChequeProtesto
        {
            Id = Guid.NewGuid(),
            IdCheque = cheque.Id,
            Documento = request.Documento,
            FechaProceso = DateTimeOffset.UtcNow,
            RegistradoPor = request.RegistradoPor,
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"El cheque {cheque.NumeroCheque}");
        }

        var descripcion = $"Protesto cheque {cheque.NumeroCheque}";
        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow), IdTipoComprobanteDiario, cheque.IdAgencia, descripcion, request.RegistradoPor,
                [
                    new LineaMovimientoRequest(idCuentaDepositos, cheque.Valor, 0, descripcion),
                    new LineaMovimientoRequest(idCuentaCanje, 0, cheque.Valor, descripcion),
                ]),
            cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new ChequeProtestadoResult(resultadoComprobante.Id);
    }

    private async Task<(Guid IdCuentaCanje, Guid IdCuentaDepositos)> ResolverCuentasAsync(CancellationToken cancellationToken)
    {
        var idCuentaCanje = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaCanje).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        var idCuentaDepositos = await db.CuentasContables
            .Where(c => c.Codigo == CodigoCuentaDepositos).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
        if (idCuentaCanje is null || idCuentaDepositos is null)
        {
            throw new InvalidOperationException($"Faltan las cuentas contables {CodigoCuentaCanje}/{CodigoCuentaDepositos}.");
        }

        return (idCuentaCanje.Value, idCuentaDepositos.Value);
    }
}
