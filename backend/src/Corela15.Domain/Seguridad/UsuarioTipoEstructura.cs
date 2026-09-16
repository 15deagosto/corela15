namespace Corela15.Domain.Seguridad;

/// <summary>Otorgamiento directo de una estructura regulatoria (ej. OF01) a una persona puntual -- mismo patrón exacto que UsuarioMenu/UsuarioDatasetReporteria/UsuarioOpcion (suma, nunca resta lo que ya da el rol). Cierra el pendiente ya documentado desde "Estructuras Financieras" — hasta ahora ese segundo nivel de permiso solo se podía otorgar por rol.</summary>
public class UsuarioTipoEstructura
{
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string CodigoTipoEstructura { get; set; } = string.Empty;
    public TipoEstructura TipoEstructura { get; set; } = null!;

    public bool Activo { get; set; } = true;

    /// <summary>Exclusión real, mismo patrón exacto que <see cref="UsuarioMenu.Excluido"/> -- bloquea esta estructura para esta persona aunque el rol la otorgue, con prioridad absoluta.</summary>
    public bool Excluido { get; set; } = false;
}
