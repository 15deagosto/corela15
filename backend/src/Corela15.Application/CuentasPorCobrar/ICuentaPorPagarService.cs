using Corela15.Application.Common;

namespace Corela15.Application.CuentasPorCobrar;

public interface ICuentaPorPagarService
{
    /// <summary>Registra una cuenta por pagar interna y su asiento (débito 450790 Otros gastos / crédito 259090 Otras cuentas por pagar).</summary>
    Task<CuentaPorPagarRegistradaResult> RegistrarAsync(
        RegistrarCuentaPorPagarRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un pago (débito 259090 / crédito Caja), reduce el saldo, y
    /// marca la cuenta Cancelada automáticamente cuando el saldo llega a
    /// cero — mismo patrón que AbonarAsync de CuentaPorCobrar. Exige la
    /// forma de cancelación real (verificada contra CONTABILIDAD.
    /// FORMA_CANCELACION) — queda como referencia en la descripción del
    /// comprobante; no redirige la cuenta contable de destino según la
    /// forma (ej. "Acreditación a cuenta" no acredita automáticamente una
    /// cuenta de ahorro del socio) — eso sería un caso de uso mayor sin
    /// demanda real confirmada todavía, documentado a propósito.
    /// </summary>
    Task<PagoCuentaPorPagarRegistradoResult> PagarAsync(
        PagarCuentaPorPagarRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Anula una cuenta por pagar registrada por error — verificado contra
    /// CUENTASPORCOBRAR.ESTADO_CUENTAPORPAGAR (código real "N" Anulado,
    /// distinto de "Cancelado" que implica pago real). Solo procede si no
    /// se ha pagado nada todavía (Saldo == MontoInicial) — anular una
    /// cuenta con pagos parciales ya reales no tiene sentido, eso se
    /// resuelve pagando el resto o quedaría un hueco contable.
    /// </summary>
    Task<CuentaPorPagarAnuladaResult> AnularAsync(AnularCuentaPorPagarRequest request, CancellationToken cancellationToken = default);
}

public class MontoCuentaPorPagarInvalidoException(decimal monto)
    : SolicitudInvalidaException($"El monto {monto:0.00} debe ser mayor a cero");

public class CuentaPorPagarInvalidaException(Guid idCuentaPorPagar)
    : ReglaDeNegocioException($"La cuenta por pagar {idCuentaPorPagar} no existe o ya no está vigente");

public class PagoCuentaPorPagarExcedeSaldoException(decimal monto, decimal saldo)
    : ReglaDeNegocioException($"El pago {monto:0.00} excede el saldo pendiente {saldo:0.00}");

public class FormaCancelacionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La forma de cancelación '{codigo}' no existe o no está activa");

public class CuentaPorPagarConPagosException(Guid idCuentaPorPagar)
    : ReglaDeNegocioException($"La cuenta por pagar {idCuentaPorPagar} ya tiene pagos registrados, no se puede anular");
