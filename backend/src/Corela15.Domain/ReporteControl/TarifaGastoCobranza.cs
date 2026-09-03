namespace Corela15.Domain.ReporteControl;

/// <summary>
/// Tarifario real de gasto de cobranza extrajudicial — verificado contra
/// REPORTECONTROL.F01_TARIFARIO_GASTOCOBRANZA (24 filas reales,
/// escalonado por rango de cuota y rango de días de mora — código
/// `SFM057` en adelante). A diferencia del tarifario general, este SÍ se
/// conecta a un cálculo real: `IGastoCobranzaService.EstimarAsync`
/// reutiliza el mismo `IMoraCarteraService` ya construido (días de mora
/// reales por préstamo) para estimar cuánto gasto de cobranza le
/// correspondería a cada préstamo vencido, sin cobrarlo todavía (solo
/// informativo — cobrarlo de verdad requeriría la misma infraestructura
/// de cobro de servicios ausente, ver TarifaServicioFinanciero.cs).
/// </summary>
public class TarifaGastoCobranza
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public decimal CuotaInicio { get; set; }
    public decimal CuotaFinal { get; set; }
    public int DiaMoraInicio { get; set; }
    public int DiaMoraFin { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
