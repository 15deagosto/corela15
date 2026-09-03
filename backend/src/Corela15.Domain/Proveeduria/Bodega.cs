using Corela15.Domain.General;
using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Proveeduria;

/// <summary>
/// Bodega de suministros por agencia — verificado contra PROVEEDURIA.
/// BODEGAPROVEEDURIA (5 filas reales). El responsable real referencia un
/// usuario por código de texto en Softbank — acá FK real a
/// `seguridad.usuario`, mismo criterio de integridad ya aplicado en todo
/// el proyecto (nunca texto libre cuando existe la entidad real).
/// </summary>
public class Bodega
{
    public Guid Id { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public Guid IdUsuarioResponsable { get; set; }
    public Usuario UsuarioResponsable { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
