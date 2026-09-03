using Corela15.Application.Common;

namespace Corela15.Application.Cajas;

public interface IPagoExternoService
{
    /// <summary>
    /// Registra el cobro real de un servicio de terceros (agua, luz,
    /// IESS, telefonía...) recibido en efectivo en ventanilla — débito
    /// Caja / crédito 2303 Recaudaciones para el sector público por el
    /// neto a remitir al proveedor / crédito 5290 Otras (Comisiones
    /// ganadas) solo si la operación cobra comisión propia (en la
    /// actividad real de esta cooperativa, ninguna la cobra hoy —
    /// verificado contra Softbank, ver CLAUDE.md). El movimiento de
    /// efectivo se enlaza automáticamente a la ventanilla del cajero
    /// (mismo mecanismo centralizado ya usado por todo el core, vía
    /// ComprobanteContableService).
    /// </summary>
    Task<PagoExternoRegistradoResult> RegistrarAsync(
        RegistrarPagoExternoRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reversa un cobro registrado por error — reversa exacta del
    /// asiento (crédito Caja / débito 2303 y 5290 según corresponda).
    /// Solo procede sobre una transacción no reversada todavía.
    /// </summary>
    Task<PagoExternoReversadoResult> ReversarAsync(
        ReversarPagoExternoRequest request, CancellationToken cancellationToken = default);
}

public record RegistrarPagoExternoRequest(
    int IdProducto, string Referencia, string? Documento, decimal Valor, decimal Comision, int IdAgencia, string RegistradoPor);

public record PagoExternoRegistradoResult(Guid Id, Guid IdComprobante);

public record ReversarPagoExternoRequest(Guid IdTransaccion, string RegistradoPor);

public record PagoExternoReversadoResult(Guid IdComprobanteReverso);

public class PagoExternoProductoInvalidoException(int idProducto)
    : ReglaDeNegocioException($"El producto de pago externo {idProducto} no existe o está inactivo");

public class ValorPagoExternoInvalidoException(decimal valor)
    : SolicitudInvalidaException($"El valor {valor:0.00} debe ser mayor a cero");

public class ComisionPagoExternoInvalidaException(decimal comision, decimal valor)
    : SolicitudInvalidaException($"La comisión ({comision:0.00}) no puede ser mayor al valor total cobrado ({valor:0.00})");

public class PagoExternoTransaccionInvalidaException(Guid idTransaccion)
    : ReglaDeNegocioException($"La transacción {idTransaccion} no existe o ya fue reversada");
