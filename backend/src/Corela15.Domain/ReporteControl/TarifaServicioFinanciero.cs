namespace Corela15.Domain.ReporteControl;

/// <summary>
/// Tarifario real de servicios financieros — verificado contra
/// REPORTECONTROL.F01_TARIFARIO (21 filas reales, códigos SFB*/SFM* de la
/// estructura F01 "Servicios Financieros" de la SEPS). Sembrado como
/// catálogo de referencia real — el reporte F01 completo (número de
/// transacciones + ingreso total por servicio) no se implementó: exige
/// cobrar estas tarifas en cada transacción real (apertura de cuenta,
/// retiro en cajero, emisión de tarjeta...), una capacidad de cobro de
/// servicios que este core no tiene en ningún flujo todavía — construirla
/// sería una funcionalidad de negocio nueva, no la lectura de un reporte
/// sobre datos que ya existen.
/// </summary>
public class TarifaServicioFinanciero
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Tarifa { get; set; }
    public bool Activo { get; set; } = true;
}
