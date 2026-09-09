namespace Corela15.Domain.Seguridad;

/// <summary>Otorgamiento directo de una opción a una persona puntual -- mismo patrón exacto que UsuarioMenu/UsuarioDatasetReporteria (suma, nunca resta lo que ya da el rol).</summary>
public class UsuarioOpcion
{
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string CodigoOpcion { get; set; } = string.Empty;
    public Opcion Opcion { get; set; } = null!;

    public bool Activo { get; set; } = true;

    /// <summary>Exclusión real, mismo patrón exacto que <see cref="UsuarioMenu.Excluido"/> -- bloquea esta opción para esta persona aunque el rol la otorgue, con prioridad absoluta.</summary>
    public bool Excluido { get; set; } = false;
}
