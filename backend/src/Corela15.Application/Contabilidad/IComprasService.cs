using Corela15.Application.Common;

namespace Corela15.Application.Contabilidad;

public interface IComprasService
{
    /// <summary>
    /// Registra una compra/factura de proveedor real: por cada línea de
    /// detalle, débito a la cuenta contable elegida (gasto o activo,
    /// según lo que se compró) por el subtotal; débito consolidado a
    /// `199005` IVA (crédito tributario) por el IVA total; crédito a
    /// `250405` Retenciones fiscales por el valor retenido (si aplica);
    /// crédito a `2506` Proveedores por el neto a pagar. El proveedor
    /// queda como una obligación real (`Saldo`/`MontoInicial`), pagable
    /// después vía `PagarAsync`.
    /// </summary>
    Task<CompraRegistradaResult> RegistrarAsync(RegistrarCompraRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pago (total o parcial) al proveedor: débito `2506` Proveedores /
    /// crédito Caja (u otra forma de cancelación real del catálogo ya
    /// compartido con Cuentas por Pagar). Cancela la compra automáticamente
    /// al llegar el saldo a cero.
    /// </summary>
    Task<PagoCompraRegistradoResult> PagarAsync(PagarCompraRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Anula una compra registrada por error — solo si no se le ha
    /// pagado nada todavía. Reversa exacta del asiento de registro,
    /// línea por línea.
    /// </summary>
    Task<CompraAnuladaResult> AnularAsync(AnularCompraRequest request, CancellationToken cancellationToken = default);
}

public record DetalleCompraRequest(
    Guid IdCuentaContable, string Detalle, decimal Cantidad, decimal ValorUnitario, decimal PorcentajeIva);

public record RegistrarCompraRequest(
    Guid IdProveedor, int IdAgencia, string CodigoSustento, string CodigoTipoComprobante,
    string Establecimiento, string PuntoEmision, string Secuencial, string Autorizacion,
    DateOnly FechaEmision, string Concepto, decimal MontoRetencion,
    IReadOnlyList<DetalleCompraRequest> Detalle, string RegistradoPor);

public record CompraRegistradaResult(Guid Id, string Numero, decimal Subtotal, decimal MontoIva, decimal Total, decimal Saldo, Guid IdComprobante);

public record PagarCompraRequest(Guid IdCompra, decimal Monto, string CodigoFormaCancelacion, string RegistradoPor);

public record PagoCompraRegistradoResult(decimal SaldoRestante, bool Cancelada, Guid IdComprobante);

public record AnularCompraRequest(Guid IdCompra, string Motivo, string RegistradoPor);

public record CompraAnuladaResult(Guid IdComprobanteReverso);

public class ProveedorInvalidoException(Guid idProveedor)
    : ReglaDeNegocioException($"El proveedor {idProveedor} no existe o está inactivo");

public class TipoComprobanteCompraInvalidoException(string codigo)
    : ReglaDeNegocioException($"El tipo de comprobante '{codigo}' no existe o está inactivo");

public class CompraSinDetalleException()
    : SolicitudInvalidaException("La compra debe tener al menos una línea de detalle");

public class LineaCompraInvalidaException(string motivo)
    : SolicitudInvalidaException(motivo);

public class RetencionCompraInvalidaException(decimal retencion, decimal total)
    : SolicitudInvalidaException($"La retención ({retencion:0.00}) no puede ser mayor al total de la compra ({total:0.00})");

public class CompraInvalidaException(Guid idCompra)
    : ReglaDeNegocioException($"La compra {idCompra} no existe o no está vigente");

public class PagoCompraExcedeSaldoException(decimal monto, decimal saldo)
    : ReglaDeNegocioException($"El pago ({monto:0.00}) excede el saldo pendiente de la compra ({saldo:0.00})");

public class CompraConPagosException(Guid idCompra)
    : ReglaDeNegocioException($"La compra {idCompra} ya tiene pagos registrados — no se puede anular, use el flujo de reverso");
