namespace Corela15.Domain.Cumplimiento;

/// <summary>Bitácora real de transición de estado del hallazgo — verificado contra CUMPLIMIENTO.HALLAZGO_ETAPA. Nunca se borra.</summary>
public class HallazgoEtapa
{
    public Guid Id { get; set; }

    public Guid IdHallazgo { get; set; }
    public Hallazgo Hallazgo { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoHallazgo Estado { get; set; } = null!;

    public string Comentario { get; set; } = string.Empty;
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
}
