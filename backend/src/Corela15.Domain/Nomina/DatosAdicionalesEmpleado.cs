namespace Corela15.Domain.Nomina;

/// <summary>
/// Catálogo real de tipos de contrato — verificado contra NOMINA.
/// TIPO_CONTRATO (6 filas: AP/CI/TC/TF/TI/TP).
/// </summary>
public class TipoContrato
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsTiempoCompleto { get; set; }
    public bool EsTiempoParcial { get; set; }
    public bool TieneFechaSalida { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Histórico real de contratos del empleado — verificado contra NOMINA.
/// EMPLEADO_TIPOCONTRATO (221 filas reales). Simplificado a una sola
/// tabla con `FechaSalida` nullable en vez de replicar
/// `EMPLEADO_TIPOCONTRATO_DESACTIVACIONCONTRATO`/`_FECHASALIDA` (tablas
/// separadas reales, mismo dato) — mismo criterio de simplificación de
/// esquema (no de dato) ya aplicado a `HorarioAccesoUsuario`.
/// </summary>
public class EmpleadoContrato
{
    public Guid Id { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public string CodigoTipoContrato { get; set; } = string.Empty;
    public TipoContrato TipoContrato { get; set; } = null!;

    public int NumeroContrato { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public DateOnly? FechaSalida { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Datos adicionales reales del empleado — verificado contra NOMINA.
/// EMPLEADO_DATOS_IESS/_DATOS_MINISTERIOLABORAL/_INFORMACIONADICIONAL
/// (126 filas cada una, 1:1 real con Empleado). Se modela como fila
/// única aditiva embebida en el propio `Empleado` (mismo criterio ya
/// usado para `Persona.CodigoProvinciaDomicilio` — un campo real que
/// existía suelto sin catálogo, acá 3 grupos de datos reales sin ningún
/// caso de uso todavía, cerrados en un solo movimiento).
/// </summary>
public class EmpleadoDatosAdicionales
{
    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    // NOMINA.EMPLEADO_DATOS_IESS
    public string? CodigoIess { get; set; }
    public DateOnly? FechaIngresoIess { get; set; }
    public DateOnly? FechaSalidaIess { get; set; }

    // NOMINA.EMPLEADO_DATOS_MINISTERIOLABORAL
    public DateOnly? FechaIngresoMinisterioLaboral { get; set; }
    public DateOnly? FechaSalidaMinisterioLaboral { get; set; }
    public int? NumeroCargasFamiliares { get; set; }

    // NOMINA.EMPLEADO_INFORMACIONADICIONAL
    public bool PagoDecimoMensual { get; set; }
    public bool PagoFondosReservaRol { get; set; }
    public bool ExtensionConyugal { get; set; }
}
