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

    // Datos socioeconómicos reales de SUJETO.PERSONA_NATURAL, sin caso de uso
    // hasta la sección "CRUD real de Socios y Usuarios y roles" — verificados
    // contra los catálogos reales (ver CLAUDE.md), no inventados.
    public string? CodigoEstadoCivil { get; set; }
    public EstadoCivil? EstadoCivil { get; set; }
    public string? CodigoEducacion { get; set; }
    public Educacion? Educacion { get; set; }
    public string? CodigoVivienda { get; set; }
    public Vivienda? Vivienda { get; set; }
    public string? CodigoSectorVivienda { get; set; }
    public SectorVivienda? SectorVivienda { get; set; }
    public string? CodigoNacionalidad { get; set; }
    public Nacionalidad? Nacionalidad { get; set; }
    public string? CodigoProfesion { get; set; }
    public Profesion? Profesion { get; set; }

    // Verificados contra SUJETO.PERSONA_NATURAL — datos reales de política
    // social/protección que Softbank captura y este core no tenía.
    public bool CobraBonoDesarrolloHumano { get; set; }
    public bool EsSeparacionDeBienes { get; set; }
    public bool TieneDiscapacidad { get; set; }
    public bool TieneCargasFamiliares { get; set; }
    public int NumeroCargasFamiliares { get; set; }
}
