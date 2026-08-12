namespace Corela15.Domain.Sujeto;

/// <summary>Extensión 1:1 de Persona. Mutuamente excluyente con PersonaJuridica.</summary>
public class PersonaNatural
{
    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public bool EsMasculino { get; set; }

    /// <summary>Persona expuesta políticamente — cumplimiento/UAF, ver Nivel 4.</summary>
    public bool EsPep { get; set; }
}
