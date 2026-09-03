namespace Corela15.Domain.Colocacion;

/// <summary>
/// Catálogo real de tipos de rubro — verificado contra COLOCACION.TIPO_RUBRO
/// (12 filas reales: AHO/CAP/CAS/DIF/GAJ/INT/MOR/OTR/PRE/SEG/SME/SVE). Cada
/// rubro individual (46 filas reales en COLOCACION.RUBRO, ej. "Capital",
/// "Gastos Judiciales", "Citaciones"...) referencia uno de estos 12 tipos —
/// las banderas de identificación real (EsCapital/EsTasa/EsMora/...) viven
/// acá, en el tipo, no en cada rubro individual.
/// </summary>
public class TipoRubro
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsCapital { get; set; }
    public bool EsTasa { get; set; }
    public bool EsMora { get; set; }
    public bool EsSeguro { get; set; }
    public bool EsCastigo { get; set; }
    public bool EsAdicional { get; set; }
    public bool EsJudicial { get; set; }
    public bool EsPrejudicial { get; set; }
    public bool EsMedico { get; set; }
    public bool EsAhorros { get; set; }
    public bool EsSeguroVehicular { get; set; }
    public bool Activo { get; set; } = true;
}
