using Corela15.Domain.Clientes;
using Corela15.Domain.Common;
using Corela15.Domain.General;

namespace Corela15.Domain.Credito;

public enum EstadoSolicitud
{
    EnAnalisis = 1,
    Aprobada = 2,
    Rechazada = 3,
    Desembolsada = 4
}

/// <summary>
/// La solicitud, antes de aprobarse — verificado contra CREDITO.SOLICITUD_PRESTAMO
/// (columna por columna, solo lectura). El scoring/comité de crédito (motor de
/// reglas configurable en Softbank: SOLICITUD_PRESTAMO_CALIFICACION,
/// COMITE_CREDITO_MAESTRO) queda fuera del alcance inicial: se agrega cuando
/// haga falta ese detalle, sin bloquear el resto del módulo.
/// </summary>
public class SolicitudPrestamo : AuditableEntity
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int IdTipoPrestamo { get; set; }
    public TipoPrestamo TipoPrestamo { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public decimal MontoSolicitado { get; set; }
    public decimal? MontoAprobado { get; set; }
    public int Cuotas { get; set; }
    public DateOnly FechaSolicitud { get; set; }
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.EnAnalisis;
}
