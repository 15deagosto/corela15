namespace Corela15.Domain.General;

/// <summary>Los 7 niveles reales de la jerarquía CIIU — verificado contra GENERAL.TIPO_ACTIVIDAD (Sección→División→Grupo→Clase→Subclase→Actividad, más "Actividad no económica").</summary>
public class TipoActividad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
