using Corela15.Domain.Common;
using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.Clientes;

public enum EstadoCliente
{
    Activo = 1,
    Inactivo = 2,
    Suspendido = 3
}

/// <summary>
/// La persona SE VUELVE socio acá. Decisión de diseño explícita (a diferencia
/// de Softbank, ver 02-arquitectura-datos-40-modulos.md 1.2): una Persona
/// puede tener varios Cliente en el tiempo (reingresos), pero nunca dos
/// Cliente ACTIVOS simultáneos — se aplica como índice único filtrado
/// en la configuración de EF Core, no solo como regla de aplicación.
/// </summary>
public class Cliente : AuditableEntity
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public Guid? IdUsuarioOficial { get; set; }

    public EstadoCliente Estado { get; set; } = EstadoCliente.Activo;
}
