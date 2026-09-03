using Corela15.Domain.FlujoTrabajo;

namespace Corela15.Domain.Credito;

/// <summary>
/// Bitácora real de transiciones de etapa de una solicitud — verificado
/// contra CREDITO.SOLICITUD_PRESTAMO_ETAPA_HIST (13.912 filas reales en
/// Softbank, la tabla que de verdad reconstruye "quién decidió qué y
/// cuándo" para un crédito). Nunca se borra, mismo criterio de auditoría
/// del resto del core.
/// </summary>
public class SolicitudPrestamoEtapaHist
{
    public Guid Id { get; set; }
    public Guid IdSolicitudPrestamo { get; set; }
    public SolicitudPrestamo SolicitudPrestamo { get; set; } = null!;

    public int? IdEtapaAnterior { get; set; }
    public int IdEtapa { get; set; }
    public Etapa Etapa { get; set; } = null!;

    public string? Comentario { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
    public bool EsRetorno { get; set; }
}
