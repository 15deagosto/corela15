using Corela15.Domain.Clientes;
using Corela15.Domain.Common;
using Corela15.Domain.FlujoTrabajo;
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

    // Comité de Crédito: EnAnalisis→Aprobada/Rechazada era un estado
    // definido en el enum desde el diseño original pero nunca usado —
    // DesembolsarAsync desembolsaba directo desde EnAnalisis. Estos tres
    // campos son el rastro real de auditoría de esa decisión.
    public string? AprobadoPor { get; set; }
    public DateTimeOffset? FechaAprobacion { get; set; }
    public string? ComentarioAprobacion { get; set; }

    // Motor de aprobaciones genérico (ver Domain/FlujoTrabajo) — la etapa
    // real donde quedó la decisión del Comité (Etapa "COMITE DE CREDITO"),
    // usada para resolver qué GrupoContable (aprobadores por agencia+monto)
    // puede aprobarla. Nunca reemplaza el enum EstadoSolicitud existente,
    // es información adicional real, no un cambio de máquina de estados.
    public int? IdEtapaActual { get; set; }
    public Etapa? EtapaActual { get; set; }

    // Convenio real bajo el que se solicita el crédito (empleador/sindicato
    // con acuerdo de descuento vía rol de pagos, ej. "SINDICATO DE
    // TRABAJADORES DE LA DIRECCIÓN PROVINCIAL DE SALUD DE COTOPAXI",
    // verificado contra CREDITO.SOLICITUD_PRESTAMO_TIPOCONVENIO) — nullable,
    // la mayoría de solicitudes no tienen convenio. Se copia a
    // Prestamo.CodigoTipoConvenio al desembolsar, mismo patrón que los
    // garantes.
    public string? CodigoTipoConvenio { get; set; }
    public TipoConvenio? TipoConvenio { get; set; }
}
