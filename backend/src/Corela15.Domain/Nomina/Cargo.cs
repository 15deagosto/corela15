namespace Corela15.Domain.Nomina;

/// <summary>
/// Catálogo real de cargos de la cooperativa — verificado contra
/// NOMINA.CARGO (90 filas reales, IDs preservados idénticos a la
/// fuente). `SeProrrateaSueldo`/`EsCargoExterno` son banderas reales sin
/// caso de uso todavía en Corela15 — se exponen para cuando se necesiten
/// (prorrateo entre agencias, cargos de servicios externos).
/// </summary>
public class Cargo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool SeProrrateaSueldo { get; set; }
    public bool EsCargoExterno { get; set; }
    public bool Activo { get; set; } = true;
}
