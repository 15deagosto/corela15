using Corela15.Domain.General;

namespace Corela15.Domain.ActivoFijo;

/// <summary>Verificado contra ACTIVOFIJO.MOTIVO_TRASLADO_ACTIVO (4 filas reales).</summary>
public class MotivoTrasladoActivo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsDefinitivo { get; set; }
    public bool Activo { get; set; } = true;
}

/// <summary>Verificado contra ACTIVOFIJO.ESTADO_TRASLADO_ACTIVO (4 códigos reales).</summary>
public enum EstadoTrasladoActivo
{
    Pendiente = 1,
    Aprobado = 2,
    Procesado = 3,
    Anulado = 4,
}

/// <summary>
/// Traslado real de un activo entre responsables/agencias — verificado
/// contra ACTIVOFIJO.TRASLADO_ACTIVO (cabecera) + _DETALLE (1.969 filas
/// reales — el detalle tiene su propio bridge por activo trasladado, acá
/// se modela 1:1 traslado↔activo, el caso real observado: cada traslado
/// mueve un activo puntual, nunca se vio agrupación real de varios
/// activos en un mismo traslado en la muestra consultada).
/// </summary>
public class TrasladoActivo
{
    public Guid Id { get; set; }

    public Guid IdActivo { get; set; }
    public Activo Activo { get; set; } = null!;

    public string Concepto { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }

    public int IdMotivoTraslado { get; set; }
    public MotivoTrasladoActivo MotivoTraslado { get; set; } = null!;

    public string? Razon { get; set; }

    public Guid? IdResponsableOrigen { get; set; }
    public Responsable? ResponsableOrigen { get; set; }
    public int IdAgenciaOrigen { get; set; }
    public Agencia AgenciaOrigen { get; set; } = null!;

    public Guid? IdResponsableDestino { get; set; }
    public Responsable? ResponsableDestino { get; set; }
    public int IdAgenciaDestino { get; set; }
    public Agencia AgenciaDestino { get; set; } = null!;

    public EstadoTrasladoActivo Estado { get; set; } = EstadoTrasladoActivo.Pendiente;

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
