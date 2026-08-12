using Corela15.Domain.Common;
using Corela15.Domain.General;

namespace Corela15.Domain.Sujeto;

/// <summary>
/// Núcleo de identidad, común a natural/jurídica.
/// Espejo de SUJETO.PERSONA en Softbank (ver 02-arquitectura-datos-40-modulos.md, 1.1).
/// </summary>
public class Persona : AuditableEntity
{
    public Guid Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public int IdTipoIdentificacion { get; set; }
    public TipoIdentificacion TipoIdentificacion { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? IdPais { get; set; }
    public Pais? Pais { get; set; }
    public int? IdActividadEconomica { get; set; }

    public decimal? Activos { get; set; }
    public decimal? Pasivos { get; set; }
    public decimal? Ingresos { get; set; }
    public decimal? Egresos { get; set; }

    public string? NumeroCasa { get; set; }
    public string? Barrio { get; set; }
    public string? CallePrincipal { get; set; }

    public PersonaNatural? PersonaNatural { get; set; }
    public PersonaJuridica? PersonaJuridica { get; set; }
}
