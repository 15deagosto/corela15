namespace Corela15.Application.Colocacion;

public record ScoreCrediticioResult(
    Guid Id, Guid IdCliente, DateTimeOffset Fecha, int Puntaje, string Categoria,
    decimal? RatioIngresoEgreso, decimal? RatioEndeudamiento,
    bool TienePrestamoCastigado, int PrestamosCancelados, bool EsPep);

public interface IScoreCrediticioService
{
    /// <summary>
    /// Calcula una calificación crediticia simplificada a partir de datos
    /// reales ya existentes en el dominio (Persona.Ingresos/Egresos/
    /// Activos/Pasivos, historial real de préstamos del cliente,
    /// PersonaNatural.EsPep) — no el motor histórico completo de Softbank
    /// (fuera de alcance por complejidad), pero tampoco un número
    /// inventado: cada componente del puntaje sale de una columna real.
    /// Guarda un snapshot cada vez que se calcula.
    /// </summary>
    Task<ScoreCrediticioResult> CalcularAsync(Guid idCliente, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScoreCrediticioResult>> HistorialAsync(Guid idCliente, CancellationToken cancellationToken = default);
}
