using Corela15.Domain.Clientes;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Common;

namespace Corela15.Domain.Cobranza;

/// <summary>
/// Cada gestión de cobro registrada sobre un préstamo — versión simplificada
/// de COBRANZA.GESTION_PRESTAMO_COBRANZA (que en Softbank vive detrás de una
/// estructura de lotes/asignación de trabajo por usuario, PRESTAMO_GESTIONAR
/// + PRESTAMO_GESTIONAR_DETALLE; acá se simplifica a un registro directo por
/// préstamo, sin heredar esa complejidad de asignación de trabajo).
/// </summary>
public class GestionPrestamoCobranza : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Si el contactado es el deudor principal (false = codeudor/referencia).</summary>
    public bool EsDeudor { get; set; }

    public int IdAccionGestion { get; set; }
    public AccionGestion AccionGestion { get; set; } = null!;

    public bool TieneCompromisoPago { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Observacion { get; set; }
}
