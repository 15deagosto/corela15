using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Comunicacion;

/// <summary>
/// Un mensaje real dentro de un canal (grupo o directo) — nunca se edita
/// ni se borra, mismo criterio de auditoría de todo el core (ver
/// `accion_ingreso_usuario`/`sesion_usuario`). El texto se guarda tal
/// cual (React escapa al renderizar, así que no hace falta sanitizar en
/// el servidor para evitar XSS) — nunca se interpreta como HTML/Markdown.
/// </summary>
public class Mensaje
{
    public Guid Id { get; set; }

    public Guid IdCanal { get; set; }
    public Canal Canal { get; set; } = null!;

    public Guid IdUsuarioRemitente { get; set; }
    public Usuario UsuarioRemitente { get; set; } = null!;

    public string Texto { get; set; } = string.Empty;

    public DateTimeOffset CreadoEn { get; set; }
}
