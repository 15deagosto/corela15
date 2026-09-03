using Corela15.Application.Ahorros;
using Corela15.Application.Cajas;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Cajas;
using Corela15.Domain.Clientes;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CuentaAhorroService(Corela15DbContext db, IComprobanteContableService comprobantes)
    : ICuentaAhorroService, IAutorizacionTransaccionService
{
    private const string CodigoCuentaCaja = "1101";
    private const string CodigoCuentaDepositos = "2101";
    private const string CodigoCuentaAportesSocios = "3103";
    private const int IdTipoComprobanteDiario = 3; // 'DIA', sembrado en Nivel1_MotorContable

    /// <summary>
    /// Bug real encontrado y corregido: la apertura de cuenta acreditaba
    /// siempre `2101` Depósitos de ahorro a la vista sin importar el
    /// producto — correcto para AHV/AHI (son depósitos, un pasivo real),
    /// pero contablemente incorrecto para Certificados de Aportación (es
    /// capital social/patrimonio en el CUC real, cuenta `3103` Aportes de
    /// socios, grupo 31 Capital social — verificado contra el catálogo
    /// oficial ya sembrado). Encontrado al construir el reporte S01
    /// (Socios SEPS), que sí lee el saldo correcto del balde "Disponible"
    /// — el bug era solo contable, no de saldo del socio.
    /// </summary>
    private static string CodigoCuentaDestinoApertura(string codigoTipoCuenta) =>
        codigoTipoCuenta == "CERT" ? CodigoCuentaAportesSocios : CodigoCuentaDepositos;

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
            var codigoCuentaDestino = CodigoCuentaDestinoApertura(tipoCuenta.Codigo);

            var idCuentaCaja = await db.CuentasContables
                .Where(c => c.Codigo == CodigoCuentaCaja).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);
            var idCuentaDestino = await db.CuentasContables
                .Where(c => c.Codigo == codigoCuentaDestino).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(cancellationToken);

            if (idCuentaCaja is null || idCuentaDestino is null)
            {
                throw new InvalidOperationException(
                    $"Faltan las cuentas contables {CodigoCuentaCaja}/{codigoCuentaDestino} — correr Contabilidad_SeedCuentasCajaYDepositos.");
            }

            var descripcionDestino = codigoCuentaDestino == CodigoCuentaAportesSocios
                ? $"Aporte de capital — cuenta {numero}"
                : $"Depósito inicial cuenta {numero}";

            var resultadoComprobante = await comprobantes.RegistrarAsync(
                new RegistrarComprobanteContableRequest(
                    DateOnly.FromDateTime(DateTime.UtcNow),
                    IdTipoComprobanteDiario,
                    request.IdAgencia,
                    $"Apertura cuenta {numero} - depósito inicial",
                    request.RegistradoPor,
                    [
                        new LineaMovimientoRequest(idCuentaCaja.Value, request.MontoInicial, 0, "Ingreso de efectivo por apertura"),
                        new LineaMovimientoRequest(idCuentaDestino.Value, 0, request.MontoInicial, descripcionDestino),
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

        // Cualquier titular marcado PEP, o con una alerta de lista de
        // control sin resolver (ver AlertaListaControl.cs — Sentenciados/
        // PEP/ONU/OFAC/etc.), pone la transacción en espera de
        // autorización de un supervisor — verificado contra el criterio
        // real CAJAS.AUTORIZACION_TRANSACCION.PERTENECELISTADECONTROL.
        var idsPersonaTitulares = await db.CuentasClientes
            .Where(cc => cc.IdCuenta == cuenta.Id)
            .Join(db.Clientes, cc => cc.IdCliente, c => c.Id, (cc, c) => c.IdPersona)
            .ToListAsync(cancellationToken);

        var algunTitularEsPep = await db.PersonasNaturales
            .Where(pn => idsPersonaTitulares.Contains(pn.IdPersona))
            .AnyAsync(pn => pn.EsPep, cancellationToken);

        var algunTitularTieneAlertaSinResolver = await db.AlertasListaControl
            .AnyAsync(a => idsPersonaTitulares.Contains(a.IdPersona) && !a.Resuelta, cancellationToken);

        if (algunTitularEsPep || algunTitularTieneAlertaSinResolver)
        {
            var motivo = algunTitularTieneAlertaSinResolver ? "alerta de lista de control sin resolver" : "titular PEP";
            var autorizacion = new AutorizacionTransaccion
            {
                Id = Guid.NewGuid(),
                IdCuenta = cuenta.Id,
                CodigoTipoTransaccion = request.CodigoTipoTransaccion,
                Monto = request.Monto,
                Detalle = $"{tipoTransaccion.Nombre} - cuenta {cuenta.Numero} ({motivo})",
                Procesado = false,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = request.RegistradoPor,
            };
            db.AutorizacionesTransaccion.Add(autorizacion);
            await db.SaveChangesAsync(cancellationToken);

            return new MovimientoCuentaRegistradoResult(null, null, null, autorizacion.Id);
        }

        var resultado = await EjecutarMovimientoAsync(cuenta, tipoTransaccion, request.Monto, request.RegistradoPor, cancellationToken);
        return new MovimientoCuentaRegistradoResult(resultado.IdMovimiento, resultado.SaldoResultante, resultado.IdComprobanteContable, null);
    }

    public async Task<IReadOnlyList<AutorizacionPendienteDetalle>> ListarPendientesAsync(CancellationToken cancellationToken = default)
    {
        return await db.AutorizacionesTransaccion
            .Include(a => a.Cuenta)
            .Where(a => !a.Procesado)
            .OrderBy(a => a.CreadoEn)
            .Select(a => new AutorizacionPendienteDetalle(
                a.Id, a.Cuenta.Numero,
                db.CuentasClientes.Where(cc => cc.IdCuenta == a.IdCuenta && cc.Principal)
                    .Select(cc => cc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                a.CodigoTipoTransaccion, a.Monto, a.Detalle, a.CreadoEn, a.CreadoPor))
            .ToListAsync(cancellationToken);
    }

    public async Task<MovimientoCuentaRegistradoResult> AprobarAsync(
        Guid idAutorizacion, string autorizadoPor, CancellationToken cancellationToken = default)
    {
        var autorizacion = await db.AutorizacionesTransaccion
            .Include(a => a.Cuenta).ThenInclude(c => c.TipoCuenta)
            .FirstOrDefaultAsync(a => a.Id == idAutorizacion && !a.Procesado, cancellationToken);
        if (autorizacion is null)
        {
            throw new AutorizacionInvalidaException(idAutorizacion);
        }

        var tipoTransaccion = await db.TiposTransaccion
            .FirstOrDefaultAsync(t => t.Codigo == autorizacion.CodigoTipoTransaccion, cancellationToken);
        if (tipoTransaccion is null || !tipoTransaccion.Activo)
        {
            throw new TipoTransaccionInvalidoException(autorizacion.CodigoTipoTransaccion);
        }

        var resultado = await EjecutarMovimientoAsync(
            autorizacion.Cuenta, tipoTransaccion, autorizacion.Monto, autorizacion.CreadoPor, cancellationToken);

        autorizacion.Procesado = true;
        autorizacion.Autorizada = true;
        autorizacion.AutorizadoPor = autorizadoPor;
        autorizacion.FechaAutorizacion = DateTimeOffset.UtcNow;
        autorizacion.ModificadoEn = DateTimeOffset.UtcNow;
        autorizacion.ModificadoPor = autorizadoPor;
        await db.SaveChangesAsync(cancellationToken);

        return new MovimientoCuentaRegistradoResult(resultado.IdMovimiento, resultado.SaldoResultante, resultado.IdComprobanteContable, null);
    }

    public async Task RechazarAsync(
        Guid idAutorizacion, string comentario, string autorizadoPor, CancellationToken cancellationToken = default)
    {
        var autorizacion = await db.AutorizacionesTransaccion
            .FirstOrDefaultAsync(a => a.Id == idAutorizacion && !a.Procesado, cancellationToken);
        if (autorizacion is null)
        {
            throw new AutorizacionInvalidaException(idAutorizacion);
        }

        autorizacion.Procesado = true;
        autorizacion.Autorizada = false;
        autorizacion.AutorizadoPor = autorizadoPor;
        autorizacion.FechaAutorizacion = DateTimeOffset.UtcNow;
        autorizacion.ComentarioRechazo = comentario;
        autorizacion.ModificadoEn = DateTimeOffset.UtcNow;
        autorizacion.ModificadoPor = autorizadoPor;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// El movimiento real: saldo, asiento contable y bitácora, todo en una
    /// sola transacción. Compartido por el camino directo (sin PEP de por
    /// medio) y por AprobarAsync (una vez que el supervisor autoriza).
    /// </summary>
    private async Task<(Guid IdMovimiento, decimal SaldoResultante, Guid IdComprobanteContable)> EjecutarMovimientoAsync(
        Cuenta cuenta, Domain.Contabilidad.TipoTransaccion tipoTransaccion, decimal monto, string registradoPor,
        CancellationToken cancellationToken)
    {
        var itemDisponible = await db.CuentasItemSaldo
            .Include(x => x.ItemSaldo)
            .FirstOrDefaultAsync(x => x.IdCuenta == cuenta.Id && x.ItemSaldo.Codigo == "DISP", cancellationToken);
        if (itemDisponible is null)
        {
            throw new InvalidOperationException($"La cuenta {cuenta.Numero} no tiene ítem de saldo 'Disponible'.");
        }

        var saldoNuevo = itemDisponible.Saldo + monto * tipoTransaccion.SignoSaldoCuenta;

        await using var transaccion = await db.Database.BeginTransactionAsync(cancellationToken);

        itemDisponible.Saldo = saldoNuevo;
        itemDisponible.ModificadoEn = DateTimeOffset.UtcNow;
        itemDisponible.ModificadoPor = registradoPor;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"La cuenta {cuenta.Numero}");
        }

        // Override real por producto (ver TipoTransaccionCuentaProducto.cs) —
        // sin esto, DEP-EFEC/RET-EFEC siempre usarían las cuentas fijas del
        // tipo de transacción (1101/2101), contablemente incorrecto para
        // Certificados de Aportación (capital social real, cuenta 3103).
        var overrideProducto = await db.TiposTransaccionCuentaProducto
            .FirstOrDefaultAsync(
                o => o.IdTipoTransaccion == tipoTransaccion.Id && o.IdTipoCuenta == cuenta.IdTipoCuenta && o.Activo,
                cancellationToken);
        var idCuentaDebito = overrideProducto?.IdCuentaContableDebito ?? tipoTransaccion.IdCuentaContableDebito;
        var idCuentaCredito = overrideProducto?.IdCuentaContableCredito ?? tipoTransaccion.IdCuentaContableCredito;

        var resultadoComprobante = await comprobantes.RegistrarAsync(
            new RegistrarComprobanteContableRequest(
                DateOnly.FromDateTime(DateTime.UtcNow),
                tipoTransaccion.IdTipoComprobante,
                cuenta.IdAgencia,
                $"{tipoTransaccion.Nombre} - cuenta {cuenta.Numero}",
                registradoPor,
                [
                    new LineaMovimientoRequest(idCuentaDebito, monto, 0, tipoTransaccion.Nombre),
                    new LineaMovimientoRequest(idCuentaCredito, 0, monto, tipoTransaccion.Nombre),
                ]),
            cancellationToken);

        var movimiento = new CuentaMovimiento
        {
            Id = Guid.NewGuid(),
            IdCuenta = cuenta.Id,
            Tipo = tipoTransaccion.SignoSaldoCuenta > 0 ? TipoMovimientoCuenta.Deposito : TipoMovimientoCuenta.Retiro,
            Monto = monto,
            SaldoResultante = saldoNuevo,
            IdComprobanteContable = resultadoComprobante.Id,
            FechaHora = DateTimeOffset.UtcNow,
            RegistradoPor = registradoPor,
        };
        db.CuentasMovimientos.Add(movimiento);
        await db.SaveChangesAsync(cancellationToken);

        await transaccion.CommitAsync(cancellationToken);

        return (movimiento.Id, saldoNuevo, resultadoComprobante.Id);
    }
}
