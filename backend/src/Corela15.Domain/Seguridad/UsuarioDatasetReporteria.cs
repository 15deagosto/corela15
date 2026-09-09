namespace Corela15.Domain.Seguridad;

/// <summary>Mismo patrón que <see cref="UsuarioMenu"/>, para datasets de Reportería Gerencial -- otorgamiento directo a un usuario puntual, además de lo que ya le dan sus roles.</summary>
public class UsuarioDatasetReporteria
{
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string CodigoDataset { get; set; } = string.Empty;
    public DatasetReporteria Dataset { get; set; } = null!;

    public bool Activo { get; set; } = true;

    /// <summary>Exclusión real, mismo patrón exacto que <see cref="UsuarioMenu.Excluido"/> -- bloquea este dataset para esta persona aunque el rol lo otorgue, con prioridad absoluta.</summary>
    public bool Excluido { get; set; } = false;
}
