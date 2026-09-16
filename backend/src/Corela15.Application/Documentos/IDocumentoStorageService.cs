namespace Corela15.Application.Documentos;

/// <summary>
/// Abstracción real de dónde vive físicamente el archivo — la
/// implementación real (<c>FileSystemDocumentoStorageService</c>, en
/// Infrastructure) escribe sobre una ruta configurada por variable de
/// entorno (<c>Documentos__RutaAlmacenamiento</c>), que en producción es
/// el punto de montaje del NAS de la cooperativa (CIFS/SMB, ver
/// docker-compose.prod.yml) y en desarrollo local es una carpeta común
/// del disco. Nunca se referencia un path físico fuera de acá — el
/// dominio y el servicio de negocio solo conocen la ruta relativa
/// guardada en <see cref="Corela15.Domain.Documentos.Documento.RutaAlmacenamiento"/>.
/// </summary>
public interface IDocumentoStorageService
{
    /// <summary>Guarda el contenido bajo una ruta real única (carpeta año/mes + nombre único) y devuelve esa ruta relativa.</summary>
    Task<string> GuardarAsync(Stream contenido, string nombreOriginal, CancellationToken cancellationToken = default);

    /// <summary>Abre el archivo real para lectura (descarga) — el caller es responsable de cerrar el stream.</summary>
    Task<Stream> AbrirAsync(string rutaRelativa, CancellationToken cancellationToken = default);
}
