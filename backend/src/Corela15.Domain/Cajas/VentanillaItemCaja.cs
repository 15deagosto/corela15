using Corela15.Domain.General;

namespace Corela15.Domain.Cajas;

/// <summary>
/// Saldo real, en vivo, de un ítem de caja (Efectivo, Cheque...) dentro de
/// una sesión de ventanilla — verificado contra CAJAS.VENTANILLA_ITEMCAJA.
/// Cierra el gap real documentado desde Nivel 5: "el cuadre no compara
/// contra un monto esperado calculado". `Saldo` se actualiza en vivo con
/// cada movimiento real de efectivo (ver VentanillaItemCajaMovimiento,
/// enganchado en ComprobanteContableService — cualquier comprobante que
/// toque la cuenta `1101` Caja bajo la ventanilla abierta del usuario que
/// lo registra genera un movimiento acá), así que al cerrar la ventanilla
/// existe un saldo esperado real contra el cual comparar el conteo físico
/// declarado (`SaldoCuadre`).
/// </summary>
public class VentanillaItemCaja
{
    public Guid Id { get; set; }

    public Guid IdVentanilla { get; set; }
    public Ventanilla Ventanilla { get; set; } = null!;

    public int IdItemCaja { get; set; }
    public ItemCaja ItemCaja { get; set; } = null!;

    public int IdMoneda { get; set; }
    public Moneda Moneda { get; set; } = null!;

    /// <summary>Saldo en vivo — se actualiza con cada VentanillaItemCajaMovimiento real.</summary>
    public decimal Saldo { get; set; }

    /// <summary>Conteo físico declarado al cerrar — null mientras la ventanilla sigue abierta.</summary>
    public decimal? SaldoCuadre { get; set; }
}
