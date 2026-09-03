namespace Corela15.Domain.Sujeto;

/// <summary>
/// Representante legal de una persona (natural o jurídica) — verificado
/// contra SUJETO.REPRESENTANTE (778 filas reales). El representante
/// también es una Persona real del sistema. `EjerceControl` es el campo
/// real de Softbank para marcar cuando el representante ejerce control
/// efectivo (relevante para beneficiario final, requisito de la norma
/// LA/FT); `Principal` marca cuál es el representante principal cuando
/// hay más de uno.
/// </summary>
public class Representante
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public Guid IdPersonaRepresentante { get; set; }
    public Persona PersonaRepresentante { get; set; } = null!;

    public bool Principal { get; set; }
    public bool EjerceControl { get; set; }
    public bool Activo { get; set; } = true;
}
