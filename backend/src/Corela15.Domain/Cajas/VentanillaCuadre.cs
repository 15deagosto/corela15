namespace Corela15.Domain.Cajas;

/// <summary>
/// El cuadre de caja diario por cajero — control operativo obligatorio,
/// verificado contra CAJAS.VENTANILLA_CUADRE (20 columnas en Softbank,
/// varias son flags booleanos redundantes con el signo de las diferencias —
/// acá se simplifican a los montos, que son la fuente de verdad real).
/// </summary>
public class VentanillaCuadre
{
    public Guid Id { get; set; }

    public Guid IdVentanilla { get; set; }
    public Ventanilla Ventanilla { get; set; } = null!;

    public DateOnly FechaProceso { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalCheque { get; set; }
    public decimal Total { get; set; }
    public decimal DiferenciaEfectivo { get; set; }
    public decimal DiferenciaCheque { get; set; }
    public bool EstaCuadrado { get; set; }
    public bool Aprobada { get; set; }
    public bool Activa { get; set; } = true;
}
