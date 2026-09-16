using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Comunicacion;

/// <summary>
/// Membresía real de un usuario en un canal — único por (IdCanal,
/// IdUsuario). <see cref="FechaUltimaLectura"/> es la marca de "leído
/// hasta acá" (los mensajes del canal con CreadoEn posterior son los no
/// leídos, sin necesidad de una fila por mensaje). <see cref="Activo"/>
/// permite "salir" de un canal de grupo sin perder el historial que ya
/// se generó mientras era miembro (nunca se borra la fila).
/// </summary>
public class CanalMiembro
{
    public Guid Id { get; set; }

    public Guid IdCanal { get; set; }
    public Canal Canal { get; set; } = null!;

    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTimeOffset? FechaUltimaLectura { get; set; }
    public bool Activo { get; set; } = true;

    public DateTimeOffset CreadoEn { get; set; }
}
