using Corela15.Domain.General;

namespace Corela15.Domain.Cajas;

/// <summary>
/// Saldo vivo de bóveda por ítem×moneda — verificado contra CAJAS.
/// BOVEDA_ITEMBOVEDA. Sin enganche automático todavía (a diferencia de
/// VentanillaItemCaja, que sí se actualiza solo desde ComprobanteContableService):
/// no existe un caso de uso real de transferencia caja↔bóveda en este core,
/// así que el saldo se mantiene por ajuste manual administrativo hasta que
/// ese flujo se construya — mismo criterio de alcance ya documentado en
/// Boveda.cs.
/// </summary>
public class BovedaItemBoveda
{
    public Guid Id { get; set; }

    public Guid IdBoveda { get; set; }
    public Boveda Boveda { get; set; } = null!;

    public int IdItemBoveda { get; set; }
    public ItemBoveda ItemBoveda { get; set; } = null!;

    public int IdMoneda { get; set; }
    public Moneda Moneda { get; set; } = null!;

    public decimal Saldo { get; set; }
}
