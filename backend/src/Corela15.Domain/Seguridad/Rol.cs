namespace Corela15.Domain.Seguridad;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }

    /// <summary>Días de vigencia de la contraseña para usuarios de este rol — verificado contra SEGURIDAD.PERFIL_USUARIO.DIASCAMBIOCLAVE (real, no un valor fijo global).</summary>
    public int DiasCambioClave { get; set; } = 30;

    /// <summary>Ve el consolidado de cliente (multi-agencia) — verificado contra SEGURIDAD.ROL.PERMITECONSOLIDADOCLIENTE.</summary>
    public bool PermiteConsolidadoCliente { get; set; }

    public List<UsuarioRol> UsuarioRoles { get; set; } = new();
}
