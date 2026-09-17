using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Comunicacion;

/// <summary>
/// Un mensaje real dentro de un canal (grupo o directo) — nunca se edita
/// ni se borra, mismo criterio de auditoría de todo el core (ver
/// `accion_ingreso_usuario`/`sesion_usuario`). El texto se guarda tal
/// cual (React escapa al renderizar, así que no hace falta sanitizar en
/// el servidor para evitar XSS) — nunca se interpreta como HTML/Markdown.
/// Puede llevar un adjunto real (imagen o archivo) — reusa la misma
/// abstracción de almacenamiento que la Biblioteca de Documentos
/// (<see cref="Corela15.Application.Documentos.IDocumentoStorageService"/>,
/// mismo punto de montaje NAS), un mensaje siempre tiene texto o adjunto
/// (nunca ninguno de los dos), nunca ambos vacíos.
/// </summary>
public class Mensaje
{
    public Guid Id { get; set; }

    public Guid IdCanal { get; set; }
    public Canal Canal { get; set; } = null!;

    public Guid IdUsuarioRemitente { get; set; }
    public Usuario UsuarioRemitente { get; set; } = null!;

    public string? Texto { get; set; }

    /// <summary>Ruta relativa dentro de la raíz configurada del storage real — nunca expuesta directo al frontend, se resuelve siempre vía el endpoint de descarga.</summary>
    public string? RutaAdjunto { get; set; }
    public string? NombreArchivoAdjunto { get; set; }
    public string? ContentTypeAdjunto { get; set; }
    public long? TamanoBytesAdjunto { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
}
