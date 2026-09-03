using Corela15.Domain.Common;
using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.Seguridad;

public class Usuario : AuditableEntity
{
    public Guid Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;

    public Guid? IdPersona { get; set; }
    public Persona? Persona { get; set; }

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

    public List<UsuarioRol> UsuarioRoles { get; set; } = new();
}
