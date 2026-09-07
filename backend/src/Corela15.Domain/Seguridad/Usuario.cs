using Corela15.Domain.Common;
using Corela15.Domain.General;
using Corela15.Domain.Planificacion;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.Seguridad;

public class Usuario : AuditableEntity
{
    public Guid Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;

    public Guid? IdPersona { get; set; }
    public Persona? Persona { get; set; }

    // Nombre real y correo — verificados contra SEGURIDAD.USUARIO.NOMBRE/
    // EMAIL de Softbank (un solo campo de nombre completo, no separado en
    // nombres/apellidos — mismo criterio ahí, se respeta acá). Existen
    // aparte de Persona/IdPersona a propósito: Softbank no exige una
    // identificación (cédula) real para dar de alta un usuario del
    // sistema, así que no hay dato verificable para armar una Persona
    // completa — nunca se inventa una identificación. Si más adelante
    // este usuario también es una Persona real del sistema (socio,
    // empleado con ficha), IdPersona se vincula aparte y
    // Persona.Nombre tiene prioridad de visualización.
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }

    // Código real de usuario en Softbank (SEGURIDAD.USUARIO.USUARIO — ahí
    // es la clave primaria real, no un ID numérico separado) — se guarda
    // aparte de NombreUsuario a propósito, para trazabilidad real: hoy
    // ambos valen lo mismo (se migró usando el código real tal cual),
    // pero si algún día se renombra el usuario acá, este campo sigue
    // apuntando al origen real para cualquier migración futura de datos
    // adicionales de Softbank que se necesite cruzar por este código.
    // Null para usuarios creados nativos en Corela15 (nunca existieron en
    // Softbank).
    public string? CodigoUsuarioSoftbank { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public bool PuedeIngresarSistema { get; set; } = true;
    public bool TieneBloqueo { get; set; }
    public bool Activo { get; set; } = true;

    // Toggles reales verificados contra SEGURIDAD.USUARIO — sin pantalla
    // hasta ahora, solo NombreUsuario/agencia/persona/bloqueo se exponían.
    public bool CambiaClave { get; set; } = true;
    public int? DiasCambioClave { get; set; }
    public bool UsaDispositivoMovil { get; set; }
    public bool PermiteRiesgoOperativo { get; set; }
    public bool PermiteConsultaEmpleados { get; set; }
    public bool ValidaIp { get; set; }

    // Área real de planificación del usuario (jefe o asistente) -- quien
    // tenga el mismo área ve/edita el mismo plan semanal, sin importar
    // quién lo cargó originalmente (ver PlanificacionService). Nullable:
    // la mayoría de usuarios no tienen nada que ver con este módulo.
    public string? CodigoAreaPlanificacion { get; set; }
    public AreaPlanificacion? AreaPlanificacion { get; set; }

    public List<UsuarioRol> UsuarioRoles { get; set; } = new();
}
