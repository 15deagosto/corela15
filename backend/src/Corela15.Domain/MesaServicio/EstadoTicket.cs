namespace Corela15.Domain.MesaServicio;

/// <summary>
/// Catálogo fijo del ciclo de vida real de un ticket — no se crea ni se
/// borra desde la app (mismo criterio que las 9 categorías de riesgo de
/// cartera u otros catálogos de estado fijos del proyecto): Abierto →
/// EnProgreso → EsperandoUsuario/Resuelto → Cerrado, con Cancelado como
/// salida alternativa desde cualquier punto antes de Cerrado.
/// </summary>
public class EstadoTicket
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
