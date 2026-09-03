using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Riesgo;

/// <summary>
/// Plan de acción real sobre un evento de riesgo — verificado contra
/// RIESGOOPERATIVO.AVANCERIESGO. Cierra el gap documentado desde Nivel 8:
/// "Los planes de acción y etapas de avance quedan fuera de alcance".
/// Deliberadamente distinto de Softbank en un punto: acá el responsable
/// es un IdUsuarioResponsable real (Usuario ya trae su propia agencia),
/// no texto libre nombres/agencia/cargo/departamento — mismo criterio ya
/// aplicado en AlertaListaControl (nunca texto libre cuando existe una
/// entidad real del dominio).
/// </summary>
public class AvanceRiesgo
{
    public Guid Id { get; set; }

    public Guid IdEventoRiesgo { get; set; }
    public EventoRiesgo EventoRiesgo { get; set; } = null!;

    public Guid IdUsuarioResponsable { get; set; }
    public Usuario UsuarioResponsable { get; set; } = null!;

    public DateTimeOffset? HoraInicio { get; set; }
    public DateTimeOffset? HoraFin { get; set; }

    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoAvanceRiesgo Estado { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}
