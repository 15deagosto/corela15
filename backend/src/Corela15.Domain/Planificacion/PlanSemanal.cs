namespace Corela15.Domain.Planificacion;

/// <summary>
/// Planificación semanal de un área real -- una fila por área x semana
/// (fecha del lunes de esa semana). Reemplaza el PDF manual que cada
/// responsable armaba por separado: ahora vive en un solo lugar real,
/// con historial completo (nunca se sobreescribe la semana anterior,
/// cada semana es su propia fila).
/// </summary>
public class PlanSemanal
{
    public Guid Id { get; set; }

    public string CodigoArea { get; set; } = string.Empty;
    public AreaPlanificacion Area { get; set; } = null!;

    /// <summary>Fecha del lunes de la semana real que cubre este plan.</summary>
    public DateOnly FechaInicioSemana { get; set; }

    /// <summary>Nombre real del responsable que firma el plan -- puede no coincidir con quien lo digitó (CreadoPor).</summary>
    public string NombreResponsable { get; set; } = string.Empty;
    public string CargoResponsable { get; set; } = string.Empty;

    public ICollection<PlanSemanalBloque> Bloques { get; set; } = new List<PlanSemanalBloque>();

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
