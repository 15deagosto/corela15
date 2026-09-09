namespace Corela15.Domain.Reporteria;

/// <summary>
/// Bitácora real de cada consulta al motor de Reportería Gerencial --
/// nunca se borra, mismo criterio de auditoría de todo el core (ver
/// `seguridad.accion_ingreso_usuario`/`sesion_usuario`). Portado de
/// `Siga.Api.AuditService`, con el mismo principio: si registrar la
/// auditoría falla, la consulta igual se devuelve al usuario -- un
/// problema de auditoría nunca debe dejar sin servicio a Gerencia.
/// </summary>
public class AuditoriaConsultaReporteria
{
    public long Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Dataset { get; set; } = string.Empty;
    public string Dimensiones { get; set; } = string.Empty;
    public string Metricas { get; set; } = string.Empty;
    public string Filtros { get; set; } = string.Empty;
    public DateTime? Snapshot { get; set; }
    public long DuracionMs { get; set; }
    public int Filas { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset FechaHora { get; set; }
}
