namespace Corela15.Domain.Nomina;

/// <summary>
/// Rol de pagos de un empleado en particular — verificado contra NOMINA.
/// ROLPAGOS_EMPLEADO (que solo guarda INGRESOS/EGRESOS totales, igual que
/// `Ingresos`/`Egresos`/`Total` acá). El detalle línea por línea de abajo
/// (horas extra, bonificaciones, décimos, préstamos IESS...) no existe
/// como columnas reales en Softbank -- la cooperativa lo lleva aparte en
/// un Excel mensual ("Rol &lt;mes&gt; &lt;año&gt;.xlsx", hojas
/// ADMINISTRATIVOS/NEGOCIOS-OPERATIVOS/SERVICIOS PROFESIONALES) que SÍ
/// tiene esta granularidad real, pedida explícitamente para no perderla
/// al traer el rol al core. Cada campo nuevo es aditivo y corresponde
/// 1:1 a una columna real de ese Excel (comentado con su letra de
/// columna) -- nunca se inventó ningún concepto que no estuviera ya en
/// el documento real que usa Contabilidad/Gerencia para aprobar el rol.
/// </summary>
public class RolPagosEmpleado
{
    public Guid Id { get; set; }

    public Guid IdRolPagos { get; set; }
    public RolPagos RolPagos { get; set; } = null!;

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    /// <summary>Total Ingresos (columna AF del Excel real).</summary>
    public decimal Ingresos { get; set; }
    /// <summary>Total Egresos (columna AQ del Excel real).</summary>
    public decimal Egresos { get; set; }
    /// <summary>Líquido a recibir (columna AR del Excel real).</summary>
    public decimal Total { get; set; }
    public int DiasLaborados { get; set; }
    public bool Anulado { get; set; }

    // --- Datos base del período (columnas I/J del Excel) ---
    public decimal? SueldoAfiliado { get; set; }
    public decimal? SueldoProporcional { get; set; }

    // --- Horas suplementarias y extraordinarias (K/L/M/N) ---
    public decimal HorasSuplementarias50 { get; set; }
    public decimal HorasSuplementarias100 { get; set; }
    public decimal HorasExtraordinarias50 { get; set; }
    public decimal HorasExtraordinarias100 { get; set; }

    // --- Otros ingresos (P/Q/R/S) ---
    public decimal Movilizacion { get; set; }
    public decimal Bonificaciones { get; set; }
    public decimal ComponenteSalarial { get; set; }
    public decimal Comisiones { get; set; }

    // --- Fondos de reserva (T/V/W) — Acumula = "SI/NO" real del Excel,
    // coincide con Empleado.RecibeFondosReserva/EmpleadoDatosAdicionales.
    // PagoFondosReservaRol, que ya existían de una ronda anterior.
    public bool FondosReservaAcumula { get; set; }
    public decimal FondosReservaValorAcumulado { get; set; }
    public decimal FondosReservaValorMensual { get; set; }

    // --- Décimo Tercero (X/Z/AA) — Acumula coincide con EmpleadoDatosAdicionales.PagoDecimoMensual. ---
    public bool DecimoTerceroAcumula { get; set; }
    public decimal DecimoTerceroValorAcumulado { get; set; }
    public decimal DecimoTerceroValorMensual { get; set; }

    // --- Décimo Cuarto (AB/AD/AE) ---
    public bool DecimoCuartoAcumula { get; set; }
    public decimal DecimoCuartoValorAcumulado { get; set; }
    public decimal DecimoCuartoValorMensual { get; set; }

    // --- Egresos (AG..AP) ---
    /// <summary>Aporte individual IESS 9.45% (AG).</summary>
    public decimal AporteIndividualIess { get; set; }
    public decimal Sanciones { get; set; }
    public decimal Atrasos { get; set; }
    public decimal Descuentos { get; set; }
    public decimal AnticipoSueldo { get; set; }
    public decimal SubsidioIess { get; set; }
    public decimal ActasFiniquito { get; set; }
    public decimal PrestamoQuirografario { get; set; }
    public decimal PrestamoHipotecario { get; set; }
    public decimal RetencionRenta { get; set; }
}
