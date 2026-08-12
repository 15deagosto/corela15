namespace Corela15.Domain.Sujeto;

/// <summary>Extensión 1:1 de Persona. Mutuamente excluyente con PersonaNatural.</summary>
public class PersonaJuridica
{
    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public string RazonSocial { get; set; } = string.Empty;
    public DateOnly FechaCreacion { get; set; }
    public bool EsGrupo { get; set; }
    public bool EsInstitucionBancaria { get; set; }
    public bool EsPublica { get; set; }
    public int? PaisConstitucion { get; set; }
}
