namespace Corela15.Domain.Colocacion;

/// <summary>
/// Matriz de calificación de riesgo de cartera (Norma para la Gestión del
/// Riesgo de Crédito en las COAC y Asociaciones Mutualistas, Art. 44) —
/// 9 categorías (A1-A3 riesgo normal, B1-B2 riesgo potencial, C1-C2
/// deficiente, D dudoso recaudo, E pérdida), cada una con un % mínimo de
/// provisión sobre el saldo. Distinta de ClasificacionCartera (que
/// clasifica el BALDE de presentación del balance — por vencer/NDI/
/// vencida — no el % de provisión regulatorio; son dos clasificaciones
/// relacionadas pero no la misma cosa en la norma real).
/// </summary>
public class CategoriaRiesgoCartera
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int DiasMoraInicio { get; set; }
    public int DiasMoraFin { get; set; }
    public decimal PorcentajeProvision { get; set; }
    public bool Activo { get; set; } = true;
}
