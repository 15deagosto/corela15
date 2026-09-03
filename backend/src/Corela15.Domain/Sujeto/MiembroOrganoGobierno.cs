namespace Corela15.Domain.Sujeto;

/// <summary>
/// Miembro de un órgano de gobierno real y obligatorio en una COAC
/// ecuatoriana — verificado contra SUJETO.CONSEJOVIGILANCIA (16 filas
/// reales). El nombre de la tabla real es engañoso: cada fila no es
/// "un consejo de vigilancia", es una Persona con hasta 3 membresías
/// simultáneas reales (Asamblea General / Consejo de Administración /
/// Consejo de Vigilancia), cada una con su propio período de inicio y
/// fin — confirmado con los datos reales (todas las filas de muestra
/// tienen `ESCONSEJOADMINISTRACION=true` con fechas propias y también
/// fechas de Asamblea/Vigilancia, aunque esas banderas específicas
/// estén en `false` en esa fila — se preservan las 3 banderas + 3
/// pares de fecha tal cual la fuente, sin colapsar a una sola.
/// `CONSEJOVIGILANCIA_VINCULADO` (parientes por consanguinidad, control
/// de partes vinculadas) tiene 0 filas reales — no se modeló, sin caso
/// de uso todavía.
/// </summary>
public class MiembroOrganoGobierno
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public bool EsAsambleaGeneral { get; set; }
    public DateOnly? FechaIniciaAsambleaGeneral { get; set; }
    public DateOnly? FechaTerminaAsambleaGeneral { get; set; }

    public bool EsConsejoAdministracion { get; set; }
    public DateOnly? FechaIniciaConsejoAdministracion { get; set; }
    public DateOnly? FechaTerminaConsejoAdministracion { get; set; }

    public bool EsConsejoVigilancia { get; set; }
    public DateOnly? FechaIniciaConsejoVigilancia { get; set; }
    public DateOnly? FechaTerminaConsejoVigilancia { get; set; }

    public bool Activo { get; set; } = true;
    public string RegistradoPor { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }
}
