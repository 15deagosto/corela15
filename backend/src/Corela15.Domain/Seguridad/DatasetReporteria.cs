namespace Corela15.Domain.Seguridad;

/// <summary>
/// Catálogo real de datasets del módulo "Reportería Gerencial" (motor
/// semántico portado de SIGA, ver <c>Corela15.Infrastructure.Reporteria</c>)
/// -- segundo nivel de permiso, más fino que el menú, mismo patrón exacto
/// que <see cref="TipoEstructura"/>: un rol puede tener el módulo entero y
/// aun así solo ver ciertos datasets. "nominal" es un dataset más en este
/// catálogo, pero nunca se otorga por defecto (contiene identificación de
/// socios) -- requiere aprobación explícita, documentado en la migración
/// de siembra.
/// </summary>
public class DatasetReporteria
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
