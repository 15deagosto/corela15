using Corela15.Domain.Contabilidad;

namespace Corela15.Domain.Proveeduria;

/// <summary>
/// Verificado contra PROVEEDURIA.TIPO_ARTICULO (4 filas reales: OI
/// Obsequios de inversiones, P Publicidad, SO Suministros de oficina,
/// UAL Útiles de aseo y limpieza) — cada tipo real trae su propia cuenta
/// contable de activo (bodega) y de gasto (al consumirse), verificadas
/// contra el CUC oficial ya sembrado (los códigos reales de Softbank de
/// 8 dígitos no existen en el catálogo oficial de 6 — se mapearon por
/// semántica a la cuenta oficial más cercana, nunca inventada).
/// </summary>
public class TipoArticulo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Detalle { get; set; }

    public Guid IdCuentaContableActivo { get; set; }
    public CuentaContable CuentaContableActivo { get; set; } = null!;

    public Guid IdCuentaContableGasto { get; set; }
    public CuentaContable CuentaContableGasto { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
