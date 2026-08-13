using Corela15.Application.Ahorros;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Clientes;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CuentaAhorroService(Corela15DbContext db, IComprobanteContableService comprobantes) : ICuentaAhorroService
{
    private const string CodigoCuentaCaja = "1101";
    private const string CodigoCuentaDepositos = "2101";
    private const int IdTipoComprobanteDiario = 3; // 'DIA', sembrado en Nivel1_MotorContable

    public async Task<CuentaAhorroAbiertaResult> AbrirAsync(
        AbrirCuentaAhorroRequest request, CancellationToken cancellationToken = default)
    {
        var tipoCuenta = await db.TiposCuenta
            .FirstOrDefaultAsync(t => t.Id == request.IdTipoCuenta, cancellationToken);
        if (tipoCuenta is null || !tipoCuenta.Activo)
        {
            throw new TipoCuentaInvalidoException(request.IdTipoCuenta);
        }

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.IdCliente, cancellationToken);
        if (cliente is null || cliente.Estado != EstadoCliente.Activo)
        {
            throw new ClienteInvalidoException(request.IdCliente);
        }

        // Una sola transacción para toda la operación (apertura + asiento
        // contable): si el comprobante falla, la cuenta tampoco queda creada.
        // ComprobanteContableService detecta esta transacción ambiente y no
        // abre la suya propia (ver ComprobanteContableService.RegistrarAsync).
        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        var ultimoNumero = await db.Cuentas.CountAsync(cancellationToken);
        var numero = (ultimoNumero + 1).ToString().PadLeft(10, '0');

        var cuenta = new Cuenta
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            IdTipoCuenta = request.IdTipoCuenta,
            IdAgencia = request.IdAgencia,
            FechaApertura = DateOnly.FromDateTime(DateTime.UtcNow),
            Estado = EstadoCuenta.Activa,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.Cuentas.Add(cuenta);

        db.CuentasClientes.Add(new CuentaCliente
        {
            IdCuenta = cuenta.Id,
            IdCliente = request.IdCliente,
            Principal = true,
        });

        var itemsDelProducto = await db.TiposCuentaItemSaldo
            .Where(x => x.IdTipoCuenta == request.IdTipoCuenta)
            .Select(x => x.IdItemSaldo)
            .ToListAsync(cancellationToken);

        var idItemDisponible = await db.ItemsSaldo
            .Where(i => i.Codigo == "DISP")
            .Select(i => (int?)i.Id)
            .FirstOrDefaultAsync(cancellationToken);

        foreach (var idItem in itemsDelProducto)
        {
            db.CuentasItemSaldo.Add(new CuentaItemSaldo
            {
                Id = Guid.NewGuid(),
                IdCuenta = cuenta.Id,
                IdItemSaldo = idItem,
                Saldo = idItem == idItemDisponible ? request.MontoInicial : 0,
                AcreditaPrestamo = false,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = request.RegistradoPor,
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        Guid? idComprobante = null;
        if (request.MontoInicial > 0)
        {
            var idCuentaCaja = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var idCuentaDepositos = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaDepositos).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);

            if (idCuentaCaja is null || idCuentaDepositos is null)
            {
                throw new InvalidOperationException(
                    $"Faltan las cuentas contables {CodigoCuentaCaja}/{CodigoCuentaDepositos} — correr Contabilidad_SeedCuentasCajaYDepositos.");
            }

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    DateOnly.FromDateTime(DateTime.UtcNow),
                    IdTipoComprobanteDiario,
                    request.IdAgencia,
                    $"Apertura cuenta {numero} - depósito inicial",
                    request.RegistradoPor,
                    [
                        new LineaMovimientoRequest(idCuentaCaja.Value, request.MontoInicial, 0, "Ingreso de efectivo por apertura"),
                        new LineaMovimientoRequest(idCuentaDepositos.Value, 0, request.MontoInicial, $"Depósito inicial cuenta {numero}"),
                    ]),
                cancellationToken);

            idComprobante = resultadoComprobante.Id;
        }

        await transaccion.CommitAsync(cancellationToken);

        return new CuentaAhorroAbiertaResult(cuenta.Id, numero, idComprobante);
    }

    public async Task<MovimientoCuentaRegistradoResult> RegistrarMovimientoAsync(
        RegistrarMovimientoCuentaRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Monto <= 0)
        {
            throw new MontoInvalidoException(request.Monto);
        }

        var cuenta = await db.Cuentas.Include(c => c.TipoCuenta)
            .FirstOrDefaultAsync(c => c.Id == request.IdCuenta, cancellationToken);
        if (cuenta is null || cuenta.Estado != EstadoCuenta.Activa)
        {
            throw new CuentaInvalidaException(request.IdCuenta);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == request.CodigoTipoTransaccion, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(request.CodigoTipoTransaccion);
        }

        var itemDisponible = await db.CuentasItemSaldo
            .Include(x => x.ItemSaldo)
            .FirstOrDefaultAsync(x => x.IdCuenta == cuenta.Id && x.ItemSaldo.Codigo == "DISP", cancellationToken);
        if (itemDisponible is null)
        {
            throw new InvalidOperationException($"La cuenta {cuenta.Numero} no tiene ítem de saldo 'Disponible'.");
        }

        var saldoNuevo = itemDisponible.Saldo + request.Monto * tipoTransaccion.SignoSaldoCuenta;
        if (saldoNuevo < cuenta.TipoCuenta.SaldoMinimo)
        {
            throw new SaldoInsuficienteException(itemDisponible.Saldo, cuenta.TipoCuenta.SaldoMinimo, request.Monto);
        }

        // Una sola transacción: saldo, asiento contable y bitácora se
        // confirman juntos o no se confirma ninguno.
        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        itemDisponible.Saldo = saldoNuevo;
        itemDisponible.ModificadoEn = DateTimeOffset.UtcNow;
        itemDisponible.ModificadoPor = request.RegistradoPor;
        await db.SaveChangesAsync(cancellationToken);

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow),
                tipoTransaccion.IdTipoComprobante,
                cuenta.IdAgencia,
                $"{tipoTransaccion.Nombre} - cuenta {cuenta.Numero}",
                request.RegistradoPor,
                [
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableDebito, request.Monto, 0, tipoTransaccion.Nombre),
                    new LineaMovimientoRequest(tipoTransaccion.IdCuentaContableCredito, 0, request.Monto, tipoTransaccion.Nombre),
                ]),
            cancellationToken);

        var movimiento = new CuentaMovimiento
        {
            Id = Guid.NewGuid(),
            IdCuenta = cuenta.Id,
            Tipo = tipoTransaccion.SignoSaldoCuenta > 0 ? TipoMovimientoCuenta.Deposito : TipoMovimientoCuenta.Retiro,
            Monto = request.Monto,
            SaldoResultante = saldoNuevo,
            IdComprobanteContable = resultadoComprobante.Id,
            FechaHora = DateTimeOffset.UtcNow,
            RegistradoPor = request.RegistradoPor,
        };
        db.CuentasMovimientos.Add(movimiento);
        await db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return new MovimientoCuentaRegistradoResult(movimiento.Id, saldoNuevo, resultadoComprobante.Id);
    }
}
