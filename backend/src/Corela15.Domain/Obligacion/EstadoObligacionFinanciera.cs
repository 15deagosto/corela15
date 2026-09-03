namespace Corela15.Domain.Obligacion;

/// <summary>Tabla 81 real (SEPS, Manual OF01): NV/VG/VN/CN. Catálogo fijo, no se crea ni se borra desde la app.</summary>
public class EstadoObligacionFinanciera
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>Tabla 11 real (SEPS): periodicidad de pago. Catálogo fijo.</summary>
public class PeriodicidadPago
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>Tabla 85 real (SEPS, OF01): clase de la obligación financiera. Catálogo fijo.</summary>
public class ClaseObligacionFinanciera
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>Tabla 86 real (SEPS, OF01): forma de cancelación. Distinta de contabilidad.forma_cancelacion (esa es de Tesorería/CxP, otro catálogo real con otros códigos). Catálogo fijo.</summary>
public class FormaCancelacionObligacion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
