using Corela15.Domain.Riesgo;

namespace Corela15.Domain.Auditoria;

public enum EstadoSeguimiento
{
    Abierto = 1,
    EnProceso = 2,
    Cerrado = 3
}

/// <summary>
/// Hallazgo de auditoría interna y su seguimiento — verificado contra
/// AUDITORIA.SEGUIMIENTO. Reusa <see cref="Riesgo.NivelRiesgo"/> de Nivel 8:
/// AUDITORIA.NIVEL_IMPACTO/NIVEL_PROBABILIDAD/NIVEL_RIESGO en Softbank son
/// el mismo patrón matriz impacto×probabilidad que RIESGOOPERATIVO, solo que
/// duplicado en dos esquemas — acá se declara una sola vez y se comparte,
/// que es justo el problema que este proyecto busca no heredar.
/// </summary>
public class Seguimiento
{
    public Guid Id { get; set; }

    public int IdAreaAuditoria { get; set; }
    public AreaAuditoria AreaAuditoria { get; set; } = null!;

    public string Descripcion { get; set; } = string.Empty;

    public int IdNivelRiesgo { get; set; }
    public NivelRiesgo NivelRiesgo { get; set; } = null!;

    public DateOnly FechaIdentificacion { get; set; }
    public EstadoSeguimiento Estado { get; set; } = EstadoSeguimiento.Abierto;
}
