using Corela15.Domain.Ahorros;

namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Override real de cuenta contable por producto para un TipoTransaccion —
/// cierra el bug documentado en la sección "Servicios Financieros" de
/// CLAUDE.md: el motor `DEP-EFEC`/`RET-EFEC` siempre debitaba/acreditaba
/// las cuentas fijas de `TipoTransaccion` (`1101`/`2101`) sin importar el
/// producto de la cuenta afectada — correcto para Ahorro a la Vista/
/// Infantil (son depósitos, pasivo real), incorrecto para Certificados de
/// Aportación (`CERT`, capital social real del CUC, cuenta `3103`). El
/// mismo bug que ya se corrigió en `CuentaAhorroService.AbrirAsync`
/// (constante `CodigoCuentaDestinoApertura`), ahora resuelto de forma
/// configurable — sin este catálogo, cada producto nuevo con reglas
/// contables distintas habría exigido tocar código C# de nuevo.
///
/// Si no existe una fila activa para (TipoTransaccion, TipoCuenta), el
/// motor usa las cuentas por defecto de `TipoTransaccion` — este catálogo
/// es solo la excepción, no reemplaza el caso general.
/// </summary>
public class TipoTransaccionCuentaProducto
{
    public int Id { get; set; }

    public int IdTipoTransaccion { get; set; }
    public TipoTransaccion TipoTransaccion { get; set; } = null!;

    public int IdTipoCuenta { get; set; }
    public TipoCuenta TipoCuenta { get; set; } = null!;

    public Guid IdCuentaContableDebito { get; set; }
    public CuentaContable CuentaContableDebito { get; set; } = null!;

    public Guid IdCuentaContableCredito { get; set; }
    public CuentaContable CuentaContableCredito { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
