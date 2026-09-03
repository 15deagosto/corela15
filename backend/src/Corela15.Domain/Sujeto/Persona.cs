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
    public General.ActividadEconomica? ActividadEconomica { get; set; }

    /// <summary>
    /// Provincia de domicilio (Tabla 05 SEPS/INEC) — el campo geográfico
    /// real que exige la estructura D01 (Depósitos) y C01 (Cartera de
    /// Créditos), documentado como pendiente en ambos hasta ahora. Solo
    /// provincia por ahora, ver Provincia.cs para la razón de no tener
    /// todavía cantón/parroquia.
    /// </summary>
    public string? CodigoProvinciaDomicilio { get; set; }
    public Provincia? ProvinciaDomicilio { get; set; }

    public decimal? Activos { get; set; }
    public decimal? Pasivos { get; set; }
    public decimal? Ingresos { get; set; }
    public decimal? Egresos { get; set; }

    public string? NumeroCasa { get; set; }
    public string? Barrio { get; set; }
    public string? CallePrincipal { get; set; }

    // Verificados contra SUJETO.PERSONA — datos reales de domicilio y
    // referencia de contacto que este core no capturaba.
    public string? CalleSecundaria { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Referencia { get; set; }
    public string? ParentescoServicioBasico { get; set; }

    public PersonaNatural? PersonaNatural { get; set; }
    public PersonaJuridica? PersonaJuridica { get; set; }
}
