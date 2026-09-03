using Corela15.Domain.Clientes;
using Corela15.Domain.Common;

namespace Corela15.Domain.Credito;

/// <summary>
/// Garante/codeudor de una solicitud, antes de aprobarse — verificado
/// contra CREDITO.SOLICITUD_PRESTAMO_GARANTIAPERSONAL. Confirmado con la
/// actividad real de la cooperativa (COLOCACION.PRESTAMO_GARANTIAPERSONAL,
/// 1307 filas) que la garantía real y casi exclusiva de este segmento es
/// personal/quirografaria — hipotecaria tiene apenas 45 filas, prendaria
/// y pagaré 0 — por eso solo se modela este tipo, no los cuatro del
/// catálogo completo CREDITO.TIPO_GARANTIA.
/// </summary>
public class SolicitudPrestamoGarantia : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid IdSolicitudPrestamo { get; set; }
    public SolicitudPrestamo SolicitudPrestamo { get; set; } = null!;

    public Guid IdClienteGarante { get; set; }
    public Cliente ClienteGarante { get; set; } = null!;

    public string? Detalle { get; set; }
    public bool Activo { get; set; } = true;
}
