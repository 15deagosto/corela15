namespace Corela15.Application.Cobranza;

public record GastoCobranzaEstimado(
    Guid IdPrestamo,
    string Numero,
    decimal Saldo,
    int DiasMora,
    decimal MontoCuotaVencida,
    string? CodigoTarifa,
    decimal? GastoEstimado);

public interface IGastoCobranzaService
{
    /// <summary>
    /// Estima el gasto de cobranza extrajudicial que le correspondería a cada
    /// préstamo vencido, según el tarifario real
    /// REPORTECONTROL.F01_TARIFARIO_GASTOCOBRANZA (sembrado en
    /// reportecontrol.tarifa_gasto_cobranza) — escalonado por rango de monto
    /// de la cuota vencida más antigua × rango de días de mora. Reusa
    /// IMoraCarteraService.CalcularAsync() como única fuente de días de mora,
    /// no duplica ese cálculo. Puramente informativo: no cobra nada, no
    /// genera ningún asiento ni movimiento — no existe todavía infraestructura
    /// de cobro de servicios en este core (ver TarifaServicioFinanciero.cs).
    /// Si un préstamo en mora no cae en ningún rango sembrado (caso teórico,
    /// los rangos reales cubren de $0 a más de $1000), GastoEstimado queda
    /// null en vez de inventar un valor.
    /// </summary>
    Task<IReadOnlyList<GastoCobranzaEstimado>> EstimarAsync(CancellationToken cancellationToken = default);
}
