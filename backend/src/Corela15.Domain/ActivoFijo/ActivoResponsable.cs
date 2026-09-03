namespace Corela15.Domain.ActivoFijo;

/// <summary>
/// Asignación de custodia de un activo a un responsable — verificado
/// contra ACTIVOFIJO.ACTIVO_RESPONSABLE (402 filas reales, bridge simple
/// IdActivo/IdResponsable). Se agregó `FechaAsignacion`/`Activa` (no
/// existen en la fuente real) para poder responder "quién es el
/// responsable actual" sin ambigüedad — la fuente real no distingue
/// asignaciones históricas de la vigente en esta misma tabla.
/// </summary>
public class ActivoResponsable
{
    public Guid Id { get; set; }

    public Guid IdActivo { get; set; }
    public Activo Activo { get; set; } = null!;

    public Guid IdResponsable { get; set; }
    public Responsable Responsable { get; set; } = null!;

    public DateOnly FechaAsignacion { get; set; }
    public bool Activa { get; set; } = true;
}
