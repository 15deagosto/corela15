using Corela15.Domain.Common;

namespace Corela15.Domain.Cajas;

/// <summary>
/// Transacción de una cuenta de ahorro puesta en espera porque algún
/// titular está marcado PEP (persona expuesta políticamente) —
/// verificado contra CAJAS.AUTORIZACION_TRANSACCION: el campo real
/// PERTENECELISTADECONTROL es lo que dispara esta cola de autorización
/// en Softbank, no un umbral de monto (los valores reales de la
/// cooperativa van de $0.01 a $700). Este core usa PersonaNatural.EsPep
/// como el único criterio de "lista de control" ya modelado y real — no
/// existe todavía una lista de sentenciados/PLA propia (ver
/// LavadoActivos.CalificacionCliente, que tampoco tiene ese flag), así
/// que el criterio se documenta explícitamente como una simplificación,
/// no una réplica completa de la lista de control real de Softbank.
/// La transacción no se ejecuta (no toca saldo ni genera comprobante)
/// hasta que un supervisor la aprueba — recién ahí se corre la misma
/// lógica que un movimiento normal.
/// </summary>
public class AutorizacionTransaccion : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid IdCuenta { get; set; }
    public Ahorros.Cuenta Cuenta { get; set; } = null!;

    public string CodigoTipoTransaccion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Detalle { get; set; } = string.Empty;

    public bool Procesado { get; set; }
    public bool? Autorizada { get; set; }
    public string? AutorizadoPor { get; set; }
    public DateTimeOffset? FechaAutorizacion { get; set; }
    public string? ComentarioRechazo { get; set; }
}
