using Corela15.Domain.Clientes;

namespace Corela15.Domain.LavadoActivos;

/// <summary>
/// Calificación de riesgo de lavado de activos por cliente — verificado
/// contra LAVADOACTIVOS.CALIFICACIONCLIENTE (15 columnas estadísticas en
/// Softbank; acá se modela el resumen del perfil, no el detalle completo
/// del cálculo — CALIFICACIONCLIENTE_TRANSACCION/DETALLE, 6.3M y 4M filas,
/// son el motor de cálculo transaccional, fuera de alcance inicial: se
/// agrega cuando se construya el motor de scoring real).
/// Responde a la Ley Orgánica de Prevención, Detección y Erradicación del
/// Delito de Lavado de Activos y a la obligación de reportar a la UAF.
/// </summary>
public class CalificacionCliente
{
    public Guid Id { get; set; }

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public decimal? Patrimonio { get; set; }
    public decimal? IngresoMensual { get; set; }
    public decimal PerfilComportamiento { get; set; }
    public decimal PerfilTransaccional { get; set; }
    public decimal TotalPerfil { get; set; }
}
