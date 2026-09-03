namespace Corela15.Domain.ActivoFijo;

/// <summary>Verificado contra ACTIVOFIJO.MOTIVO_BAJA (12 filas reales).</summary>
public class MotivoBaja
{
    public int Id { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public bool EsDonacion { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>Verificado contra ACTIVOFIJO.ESTADO_SOLICITUDACTIVOBAJA (5 códigos reales).</summary>
public enum EstadoSolicitudActivoBaja
{
    Ingresado = 1,
    Verificado = 2,
    Autorizado = 3,
    Procesado = 4,
    Anulado = 5,
}

/// <summary>
/// Solicitud de baja de un activo — verificado en estructura contra
/// ACTIVOFIJO.SOLICITUD_ACTIVO_BAJA (0 filas reales en la Softbank de esta
/// cooperativa hoy, pero la estructura/catálogos de motivo y estado sí
/// están sembrados y activos — se construye igual, mismo criterio ya
/// aplicado a ObligacionFinanciera en Nivel 7: la norma/proceso real lo
/// exige aunque todavía no haya una baja real registrada).
/// </summary>
public class SolicitudActivoBaja
{
    public Guid Id { get; set; }

    public Guid IdActivo { get; set; }
    public Activo Activo { get; set; } = null!;

    public int IdMotivoBaja { get; set; }
    public MotivoBaja MotivoBaja { get; set; } = null!;

    public string? Detalle { get; set; }
    public DateOnly FechaSolicitud { get; set; }
    public EstadoSolicitudActivoBaja Estado { get; set; } = EstadoSolicitudActivoBaja.Ingresado;

    public Guid? IdComprobante { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
