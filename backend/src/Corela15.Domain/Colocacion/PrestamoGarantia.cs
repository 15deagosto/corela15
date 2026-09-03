using Corela15.Domain.Clientes;
using Corela15.Domain.Common;
using Corela15.Domain.Credito;

namespace Corela15.Domain.Colocacion;

/// <summary>
/// Garante/codeudor real de un préstamo ya desembolsado — verificado
/// contra COLOCACION.PRESTAMO_GARANTIAPERSONAL. Se crea copiando los
/// garantes activos de SolicitudPrestamoGarantia en el momento del
/// desembolso (mismo patrón que MontoAprobado→DeudaInicial); su estado
/// pasa a Levantada ('L') cuando el préstamo se cancela por completo —
/// la garantía se libera cuando la deuda se salda, el mismo
/// comportamiento real observado en los datos de Softbank.
/// </summary>
public class PrestamoGarantia : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public Guid IdClienteGarante { get; set; }
    public Cliente ClienteGarante { get; set; } = null!;

    public string CodigoEstadoGarantia { get; set; } = "A";
    public EstadoGarantia EstadoGarantia { get; set; } = null!;
}
