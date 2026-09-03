using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Cumplimiento;

/// <summary>
/// Hallazgo de auditoría/cumplimiento con ciclo de vida real — verificado
/// contra CUMPLIMIENTO.HALLAZGO. Distinto de riesgo.evento_riesgo (matriz
/// impacto×probabilidad de un riesgo operativo) y de auditoria.seguimiento
/// (área+seguimiento genérico ya sembrado en Nivel 8 periféricos) — este
/// modela el hallazgo específico con asignación a un auditado y su
/// respuesta, el gap real señalado en 03-sbk-wpf-decompilado.md.
/// </summary>
public class Hallazgo
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;

    public Guid IdUsuarioReporta { get; set; }
    public Usuario UsuarioReporta { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoHallazgo Estado { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}
