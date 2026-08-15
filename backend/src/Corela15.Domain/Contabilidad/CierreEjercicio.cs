namespace Corela15.Domain.Contabilidad;

/// <summary>
/// Cierre de resultados del ejercicio (utilidad/pérdida del año →
/// patrimonio) — un registro por año, nunca se reabre. Distinto de
/// <see cref="PeriodoContable"/>: ese bloquea contabilizar retroactivo
/// mes a mes; esto es la liquidación anual real de ingresos y gastos
/// contra `36` Resultados (cuentas `3603`/`3604`, sembradas para este
/// caso de uso).
/// </summary>
public class CierreEjercicio
{
    public Guid Id { get; set; }

    public int Anio { get; set; }
    public DateTimeOffset FechaCierre { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal Utilidad { get; set; }

    public Guid IdComprobanteContable { get; set; }
    public ComprobanteContable ComprobanteContable { get; set; } = null!;

    public string CerradoPor { get; set; } = string.Empty;
}
