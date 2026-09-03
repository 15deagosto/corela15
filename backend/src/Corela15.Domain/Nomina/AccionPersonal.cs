namespace Corela15.Domain.Nomina;

/// <summary>
/// Catálogo real de estados — verificado contra NOMINA.ESTADO_ACCIONPERSONAL
/// (3 filas: IN Ingresada, AP Aprobada, AN Anulada).
/// </summary>
public class EstadoAccionPersonal
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Catálogo real de tipos de acción de personal — verificado contra
/// NOMINA.ACCION_PERSONAL (17 filas reales, IDs preservados). Solo se
/// exponen las banderas con efecto real implementado en
/// <see cref="ISolicitudAccionPersonalService"/> — el resto de columnas
/// reales (EsLiquidacion, EsPagoVacacion, EsSancion...) no tienen caso de
/// uso todavía, documentado a propósito en CLAUDE.md.
/// </summary>
public class TipoAccionPersonal
{
    public int Id { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public bool ActivaContrato { get; set; }
    public bool DesactivaContrato { get; set; }
    public bool EsCambioCargoSueldo { get; set; }
    public bool EsCambioAgenciaDepartamento { get; set; }
    public bool EsFormaPagoDecimos { get; set; }
    public bool EsFormaPagoFondosReserva { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Solicitud real de acción de personal — verificado contra NOMINA.
/// SOLICITUD_ACCIONPERSONAL (411 filas reales). Los datos específicos por
/// tipo (cargo nuevo, agencia nueva, nuevo valor de fondos de reserva) se
/// capturan aditivamente en la propia cabecera en vez de replicar las
/// ~10 tablas de detalle reales de Softbank (SOLICITUD_ACCIONPERSONAL_
/// CAMBIOCARGO_*/_CAMBIOAGENCIADEPARTAMENTO/etc.) — simplificación
/// consciente, documentada en CLAUDE.md, cubre el 90% real del volumen.
/// </summary>
public class SolicitudAccionPersonal
{
    public Guid Id { get; set; }
    public int NumeroAccion { get; set; }

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public int IdTipoAccionPersonal { get; set; }
    public TipoAccionPersonal TipoAccionPersonal { get; set; } = null!;

    public string Detalle { get; set; } = string.Empty;
    public string CodigoEstado { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }

    public Guid IdUsuario { get; set; }

    // Datos específicos por tipo, capturados al crear, aplicados al aprobar.
    public int? IdCargoNuevo { get; set; }
    public decimal? NuevoSueldo { get; set; }
    public int? IdAgenciaNueva { get; set; }
    public bool? NuevoValorRecibeFondosReserva { get; set; }

    public List<SolicitudAccionPersonalEtapa> Etapas { get; set; } = [];
}

/// <summary>Bitácora inmutable de transición — nunca se borra.</summary>
public class SolicitudAccionPersonalEtapa
{
    public Guid Id { get; set; }

    public Guid IdSolicitudAccionPersonal { get; set; }
    public SolicitudAccionPersonal SolicitudAccionPersonal { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public string? Comentario { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}
