using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Comunicacion;

/// <summary>
/// Un mensaje real dentro de un canal (grupo o directo) — nunca se edita
/// ni se borra, mismo criterio de auditoría de todo el core (ver
/// `accion_ingreso_usuario`/`sesion_usuario`). El texto se guarda tal
/// cual (React escapa al renderizar, así que no hace falta sanitizar en
/// el servidor para evitar XSS) — nunca se interpreta como HTML/Markdown.
/// Puede llevar hasta 3 adjuntos reales (imágenes y/o archivos, ver
/// <see cref="MensajeAdjunto"/>) — un mensaje siempre tiene texto o al
/// menos un adjunto (nunca ambos vacíos).
/// </summary>
public class Mensaje
{
    public Guid Id { get; set; }

    public Guid IdCanal { get; set; }
    public Canal Canal { get; set; } = null!;

    public Guid IdUsuarioRemitente { get; set; }
    public Usuario UsuarioRemitente { get; set; } = null!;

    public string? Texto { get; set; }

    public ICollection<MensajeAdjunto> Adjuntos { get; set; } = new List<MensajeAdjunto>();

    public DateTimeOffset CreadoEn { get; set; }
}

/// <summary>
/// Un adjunto real de un mensaje — hasta 3 por mensaje (imágenes reales
/// pegadas con Ctrl+V o archivos elegidos, mismo límite que el chequeo
/// real ya usado en otras apps de mensajería para no saturar un solo
/// mensaje). Reusa la misma abstracción de almacenamiento que la
/// Biblioteca de Documentos
/// (<see cref="Corela15.Application.Documentos.IDocumentoStorageService"/>,
/// mismo punto de montaje NAS) — nunca duplica el archivo real al
/// reenviar un mensaje (ver <see cref="Corela15.Application.Comunicacion.IComunicacionService.ReenviarMensajeAsync"/>).
/// </summary>
public class MensajeAdjunto
{
    public Guid Id { get; set; }

    public Guid IdMensaje { get; set; }
    public Mensaje Mensaje { get; set; } = null!;

    /// <summary>Ruta relativa dentro de la raíz configurada del storage real — nunca expuesta directo al frontend, se resuelve siempre vía el endpoint de descarga.</summary>
    public string RutaAdjunto { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long TamanoBytes { get; set; }

    public int Orden { get; set; }
}
