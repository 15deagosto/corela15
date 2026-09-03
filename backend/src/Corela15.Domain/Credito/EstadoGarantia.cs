namespace Corela15.Domain.Credito;

/// <summary>
/// Catálogo real de estados de garantía — verificado contra
/// CREDITO.ESTADO_GARANTIA (5 códigos exactos): A Abierta, C Cerrada,
/// L Levantada (deuda saldada, garantía liberada), N Anulada,
/// P Por constituir.
/// </summary>
public class EstadoGarantia
{
    public string Codigo { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
