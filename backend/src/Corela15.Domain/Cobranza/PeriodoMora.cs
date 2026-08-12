namespace Corela15.Domain.Cobranza;

/// <summary>
/// Tramos de días de mora con su gestión correspondiente — verificado
/// contra COBRANZA.PERIODO_MORA. Coincide con el flujo documentado en el
/// manual de Cobranzas: Gestión Preventiva → Gestión Cobranza → Comité Mora
/// I → Comité Mora II/III → Judicial.
/// </summary>
public class PeriodoMora
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int DiasInicio { get; set; }
    public int DiasFin { get; set; }
    public bool Activo { get; set; } = true;
}
