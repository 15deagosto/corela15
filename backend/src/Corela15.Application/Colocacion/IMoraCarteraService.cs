namespace Corela15.Application.Colocacion;

public record PrestamoMoraResult(Guid IdPrestamo, string Numero, decimal Saldo, int DiasMora);

public interface IMoraCarteraService
{
    /// <summary>
    /// Calcula los días de mora reales de cada préstamo vigente — hoy
    /// menos la fecha de vencimiento de la cuota de capital impaga más
    /// antigua (PrestamoRubro.FechaFin), 0 si no tiene ninguna vencida.
    /// Única fuente de verdad para "días de mora" en todo el sistema:
    /// antes este cálculo vivía duplicado e implícito solo dentro de
    /// ProvisionCarteraService, y Cobranza no tenía ninguna forma de saber
    /// qué préstamo estaba realmente vencido — su selector mostraba todos
    /// los vigentes por igual. Ambos consumidores (provisión y cobranza)
    /// llaman a este mismo cálculo ahora.
    /// </summary>
    Task<IReadOnlyList<PrestamoMoraResult>> CalcularAsync(CancellationToken cancellationToken = default);
}
