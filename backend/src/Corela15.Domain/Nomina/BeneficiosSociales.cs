namespace Corela15.Domain.Nomina;

/// <summary>
/// Parámetro real único (Salario Básico Unificado vigente) — necesario
/// para el cálculo real de Décimo Cuarto (1 SBU/año, fijo, independiente
/// del sueldo de cada empleado). Mismo criterio ya usado para
/// `riesgo.parametro_liquidez`: valor de referencia real, a actualizar
/// cada vez que el Ministerio de Trabajo publique el SBU del año.
/// </summary>
public class ParametroNomina
{
    public int Id { get; set; }
    public decimal SalarioBasicoUnificado { get; set; }
}

/// <summary>
/// Acumulación real de Décimo Tercero (bono navideño) — verificado
/// contra NOMINA.EMPLEADO_DECIMOTERCERO (205 filas reales). Simplificado
/// a una sola fila "corriente" por empleado que acumula indefinidamente
/// hasta liquidarse (en vez del esquema real de filas por período
/// rotando cada año) — mismo criterio de simplificación ya aplicado a
/// `DevengoInteresService`/`ActivoService` (acumular en una fila viva,
/// no reconstruir el historial completo de Softbank).
/// </summary>
public class EmpleadoDecimoTercero
{
    public Guid Id { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public decimal Proyectado { get; set; }
    public decimal Acumulado { get; set; }
    public decimal Pagado { get; set; }

    public DateOnly? UltimoDevengo { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Acumulación real de Décimo Cuarto (bono escolar, 1 SBU/año fijo) —
/// verificado contra NOMINA.EMPLEADO_DECIMOCUARTO (218 filas reales).
/// La variación real por región (`IDREGIONNATURAL`, pago en agosto
/// Sierra/Amazonía vs. marzo Costa/Insular) no se modeló — esta
/// cooperativa opera en Cotopaxi (Sierra), simplificación consciente
/// documentada en CLAUDE.md.
/// </summary>
public class EmpleadoDecimoCuarto
{
    public Guid Id { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public decimal Proyectado { get; set; }
    public decimal Acumulado { get; set; }
    public decimal Pagado { get; set; }

    public DateOnly? UltimoDevengo { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Acumulación real de Fondos de Reserva — verificado contra NOMINA.
/// EMPLEADO_FONDOSRESERVA (29 filas reales, con 1.117 movimientos reales
/// asociados). Solo aplica a empleados con `Empleado.RecibeFondosReserva`
/// y más de un año de antigüedad real (regla laboral ecuatoriana).
/// </summary>
public class EmpleadoFondosReserva
{
    public Guid Id { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public decimal Proyectado { get; set; }
    public decimal Acumulado { get; set; }
    public decimal Pagado { get; set; }

    public DateOnly? UltimoDevengo { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Provisión real de vacaciones — verificado contra NOMINA.EMPLEADO_
/// PROVISION_VACACION (3.223 filas reales) + _DETALLE (2.548, detalle
/// mensual con último sueldo/días/valor anterior/valor actual — mismo
/// patrón cabecera+detalle ya usado en `DepreciacionAgencia`).
/// </summary>
public class EmpleadoProvisionVacacion
{
    public Guid Id { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public decimal Acumulado { get; set; }
    public decimal Pagado { get; set; }

    public DateOnly? UltimoDevengo { get; set; }
    public bool Activo { get; set; } = true;
}

public class EmpleadoProvisionVacacionDetalle
{
    public Guid Id { get; set; }

    public Guid IdProvisionVacacion { get; set; }
    public EmpleadoProvisionVacacion ProvisionVacacion { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public decimal UltimoSueldo { get; set; }
    public decimal Dias { get; set; }
    public decimal ValorAnteriorProvision { get; set; }
    public decimal ValorActualProvision { get; set; }
    public decimal ValorAProvisionar { get; set; }
}

/// <summary>
/// Aporte Patronal al IESS — verificado contra NOMINA.EMPLEADO_
/// APORTEPATRONAL (122 filas reales) + _MOVIMIENTOAFECTACION (2.380).
/// Mismo patrón de acumulación mensual que los 4 beneficios sociales,
/// pero es un GASTO real de la cooperativa (no un beneficio que se le
/// paga al empleado) — se liquida contra el IESS, nunca contra el
/// empleado. Tasa real 11.15% (tasa vigente de aporte patronal IESS
/// Ecuador, sector privado — dato público, no está en ninguna tabla de
/// Softbank, mismo criterio ya usado con el 10% de interés de mora BCE).
/// </summary>
public class EmpleadoAportePatronal
{
    public Guid Id { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public decimal Proyectado { get; set; }
    public decimal Acumulado { get; set; }
    public decimal Pagado { get; set; }

    public DateOnly? UltimoDevengo { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Tramo real de la tabla de Impuesto a la Renta para personas naturales
/// en relación de dependencia — verificado contra NOMINA.PERIODO_FISCAL_
/// IMPUESTORENTA, período fiscal 2023 (vigente, 10 tramos reales, hasta
/// el 37% de excedente). El SRI no ha publicado una tabla nueva desde
/// entonces (los rangos se ajustan por inflación cada año, pero la
/// estructura de 10 tramos sigue siendo la vigente) — actualizar esta
/// siembra cuando el SRI publique una tabla nueva, mismo criterio que
/// `TasaTechoBce`/`ParametroNomina`.
/// </summary>
public class TramoImpuestoRenta
{
    public int Id { get; set; }
    public decimal FraccionBasica { get; set; }
    public decimal ExcesoHasta { get; set; }
    public decimal ImpuestoFraccionBasica { get; set; }
    public decimal PorcentajeExcedente { get; set; }
}

/// <summary>
/// Snapshot histórico del cálculo de retención en la fuente por Impuesto
/// a la Renta (relación de dependencia) — nunca se sobreescribe, mismo
/// criterio que `ScoreCrediticio`. Verificado contra la tabla real de
/// tramos NOMINA.PERIODO_FISCAL_IMPUESTORENTA (período fiscal 2023,
/// vigente — 10 tramos reales con el 37% de excedente máximo,
/// confirmados fila por fila contra Softbank).
/// </summary>
public class CalculoImpuestoRenta
{
    public Guid Id { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public int Anio { get; set; }
    public decimal IngresoAnualProyectado { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal ImpuestoCausadoAnual { get; set; }
    public decimal RetencionMensual { get; set; }
    public DateOnly FechaCalculo { get; set; }
}
