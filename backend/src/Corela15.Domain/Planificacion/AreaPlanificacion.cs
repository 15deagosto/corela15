using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Planificacion;

/// <summary>
/// "Espacio" de planificación semanal: el dueño de un plan, de sus
/// etiquetas, sus KPIs y de las disposiciones de gerencia que se le
/// asignan. Originalmente era un catálogo de áreas/departamentos (TI,
/// Contabilidad...), pero por decisión explícita del usuario (oct-2026)
/// la planificación pasó a ser **personal**: cada usuario con acceso al
/// módulo tiene su propio espacio, creado automáticamente la primera vez
/// que se necesita (<see cref="IdUsuario"/> no nulo). Las filas de
/// departamento previas (IdUsuario nulo) quedan inactivas, conservadas
/// solo como historia de los planes que se registraron con ellas.
/// </summary>
public class AreaPlanificacion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    /// <summary>Dueño real del espacio personal. Nulo solo en las áreas de departamento heredadas.</summary>
    public Guid? IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }
}
