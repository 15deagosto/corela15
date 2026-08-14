namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Estado de cierre de un período contable (mes calendario). Mientras no
/// exista una fila para un período, se asume abierto — solo un cierre
/// explícito lo bloquea. Un período cerrado impide registrar cualquier
/// comprobante con fecha dentro de él (ComprobanteContableService.RegistrarAsync
/// valida esto en cada registro) — nadie puede contabilizar retroactivo
/// sobre un mes ya cerrado y reportado. No implementa el cierre de
/// resultados (utilidad del ejercicio → patrimonio, típico del cierre
/// anual) — eso queda fuera de alcance, ver CLAUDE.md.
/// </summary>
public class PeriodoContable
{
    public Guid Id { get; set; }
    public DateOnly Periodo { get; set; }
    public bool Cerrado { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }
    public string? CerradoPor { get; set; }
}
