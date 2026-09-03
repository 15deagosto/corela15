using Corela15.Domain.Contabilidad;

namespace Corela15.Domain.ActivoFijo;

/// <summary>
/// Categoría real de activo fijo (Escritorio, Computador, Vehículo...) —
/// verificado contra ACTIVOFIJO.ESTRUCTURA (86 filas reales, jerarquía de
/// 3 niveles; solo el nivel 3 —hoja, 73 filas activas— se asigna a un
/// activo real, es el único nivel con `PROCESACONTABILIDAD=true` en
/// ACTIVOFIJO.ESTRUCTURA_NIVEL). Los niveles 1/2 (agrupación visual, sin
/// depreciación propia) no se modelaron — sin caso de uso real de reporte
/// agrupado todavía, simplificación consciente documentada en CLAUDE.md.
///
/// Los códigos de cuenta contable reales de Softbank para esta tabla
/// (ej. "18051001", 8 dígitos) son un sub-mayor interno que no existe en
/// el Catálogo Único de Cuentas oficial ya sembrado (máximo 6 dígitos) —
/// cada categoría se mapeó a la cuenta oficial real más cercana por
/// semántica (ej. "Aire Acondicionado"→1805 Muebles y enseres), nunca
/// inventando una cuenta de 8 dígitos sin respaldo en el catálogo oficial.
/// </summary>
public class Estructura
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public bool SeDeprecia { get; set; }
    public decimal PorcentajeDepreciacionAnual { get; set; }
    public bool EsBienIntangible { get; set; }

    public Guid IdCuentaContableActivo { get; set; }
    public CuentaContable CuentaContableActivo { get; set; } = null!;

    public Guid? IdCuentaContableDeprecia { get; set; }
    public CuentaContable? CuentaContableDeprecia { get; set; }

    public Guid? IdCuentaContableGasto { get; set; }
    public CuentaContable? CuentaContableGasto { get; set; }

    public bool Activo { get; set; } = true;
}
