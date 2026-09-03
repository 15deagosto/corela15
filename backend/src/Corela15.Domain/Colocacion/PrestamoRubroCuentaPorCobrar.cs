using Corela15.Domain.CuentasPorCobrar;

namespace Corela15.Domain.Colocacion;

/// <summary>
/// Vínculo real entre un rubro manual cargado a un préstamo y la cuenta
/// por cobrar que generó — verificado contra
/// COLOCACION.PRESTAMO_RUBRO_CUENTAPORCOBRAR (1.363 filas reales, bridge
/// 1:1). Sin esta tabla, un cargo de "Gastos Judiciales" a un préstamo no
/// tendría forma real de saber qué CxC le corresponde — en Softbank real
/// solo queda una referencia de texto libre dentro del concepto de la CxC
/// (confirmado: ni CUENTAPORCOBRAR ni PRESTAMO_RUBRO tienen FK directa
/// entre sí, la relación vive únicamente en esta tabla puente).
/// </summary>
public class PrestamoRubroCuentaPorCobrar
{
    public Guid IdPrestamoRubro { get; set; }
    public PrestamoRubro PrestamoRubro { get; set; } = null!;

    public Guid IdCuentaPorCobrar { get; set; }
    public CuentaPorCobrar CuentaPorCobrar { get; set; } = null!;
}
