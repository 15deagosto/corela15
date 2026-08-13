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
        await transaccion.CommitAsync(cancellationToken);

        // Nota: la apertura de cuenta y el registro contable son dos
        // transacciones separadas (ComprobanteContableService abre la suya
        // propia) — si el comprobante falla acá, la cuenta ya quedó creada
        // sin su asiento. Aceptable por ahora (no hay saga/outbox todavía);
        // revisar si esto se vuelve un problema real en producción.
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

        return new CuentaAhorroAbiertaResult(cuenta.Id, numero, idComprobante);
    }
}
