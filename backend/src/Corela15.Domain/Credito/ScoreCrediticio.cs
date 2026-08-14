using Corela15.Domain.Clientes;

namespace Corela15.Domain.Credito;

public enum CategoriaScore
{
    RiesgoBajo = 1,
    RiesgoMedio = 2,
    RiesgoAlto = 3
}

/// <summary>
/// Snapshot de calificación crediticia — modelo simplificado propio, NO el
/// motor histórico completo de Softbank (SOLICITUD_PRESTAMO_CALIFICACION,
/// fuera de alcance por complejidad, ver CLAUDE.md). Calculado sobre
/// variables reales ya existentes en el dominio (Persona.Ingresos/Egresos/
/// Activos/Pasivos, historial real de Prestamo del cliente,
/// PersonaNatural.EsPep) — nunca inventa una tabla de datos nueva para
/// alimentar el score. Un registro por cálculo, no se sobreescribe.
/// </summary>
public class ScoreCrediticio
{
    public Guid Id { get; set; }

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateTimeOffset Fecha { get; set; }
    public int Puntaje { get; set; }
    public CategoriaScore Categoria { get; set; }

    public decimal? RatioIngresoEgreso { get; set; }
    public decimal? RatioEndeudamiento { get; set; }
    public bool TienePrestamoCastigado { get; set; }
    public int PrestamosCancelados { get; set; }
    public bool EsPep { get; set; }
}
