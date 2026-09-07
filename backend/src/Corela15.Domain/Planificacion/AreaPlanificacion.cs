namespace Corela15.Domain.Planificacion;

/// <summary>
/// Catálogo simple de áreas/departamentos de la cooperativa -- solo para
/// que cada quien clasifique su propia planificación semanal bajo el área
/// real a la que pertenece (TI, Contabilidad, Cobranzas, etc.). Editable
/// desde Configuración, sin invariante de negocio más allá de código único.
/// </summary>
public class AreaPlanificacion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
