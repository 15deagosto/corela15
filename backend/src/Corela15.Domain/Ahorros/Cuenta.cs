using Corela15.Domain.Common;
using Corela15.Domain.General;

namespace Corela15.Domain.Ahorros;

public enum EstadoCuenta
{
    Activa = 1,
    Bloqueada = 2,
    Cerrada = 3
}

/// <summary>
/// Cuenta de ahorros — grupo CUC 21 (Obligaciones con el público), subcuenta
/// 2101 depósitos a la vista. Espejo de AHORROS.CUENTA (que en Softbank ya
/// es temporal nativa — el único caso bueno a copiar de Nivel 0/1). Acá el
/// versionado real es el mismo patrón trigger+historico de cuenta_contable
/// (Nivel1_MotorContable), aplicado también a cuenta_item_saldo.
/// </summary>
public class Cuenta : AuditableEntity
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public int IdTipoCuenta { get; set; }
    public TipoCuenta TipoCuenta { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public DateOnly FechaApertura { get; set; }
    public EstadoCuenta Estado { get; set; } = EstadoCuenta.Activa;
}
