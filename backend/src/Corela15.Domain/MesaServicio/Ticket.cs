using Corela15.Domain.General;
using Corela15.Domain.Seguridad;

namespace Corela15.Domain.MesaServicio;

/// <summary>
/// Mesa de servicio — control de incidencias real exigido por la SEPS
/// (registro, clasificación, y seguimiento de incidentes hasta su
/// resolución). Disponible para TODOS los usuarios del sistema por
/// diseño (ver AuthService.LoginAsync: el menú `mesa-servicio` se agrega
/// siempre al token, sin pasar por `rol_menu` como el resto de módulos)
/// — es infraestructura transversal, no un módulo de negocio con
/// permiso segmentado.
/// </summary>
public class Ticket
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public string CodigoCategoria { get; set; } = string.Empty;
    public CategoriaIncidencia Categoria { get; set; } = null!;

    public string CodigoPrioridad { get; set; } = string.Empty;
    public PrioridadTicket Prioridad { get; set; } = null!;

    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoTicket Estado { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public Guid? IdUsuarioAsignado { get; set; }
    public Usuario? UsuarioAsignado { get; set; }

    public DateTimeOffset FechaLimiteSla { get; set; }
    public DateTimeOffset? FechaCierre { get; set; }

    /// <summary>
    /// Momento en que un AGENTE (con el menú `mesa-servicio-agente`) tomó la
    /// primera acción real sobre el ticket — tomar, reasignar, o cambiar de
    /// estado. Nunca se fija por un comentario del propio reportante. Mide
    /// el SLA de primera respuesta, distinto del SLA de resolución
    /// (FechaLimiteSla/FechaCierre).
    /// </summary>
    public DateTimeOffset? FechaPrimeraRespuesta { get; set; }

    /// <summary>Calificación real del reportante, 1-5, solo una vez, solo con el ticket Resuelto/Cerrado.</summary>
    public int? Calificacion { get; set; }
    public string? ComentarioCalificacion { get; set; }
    public DateTimeOffset? FechaCalificacion { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
