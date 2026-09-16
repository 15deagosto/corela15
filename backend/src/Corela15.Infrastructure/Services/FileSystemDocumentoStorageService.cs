using Corela15.Application.Documentos;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Implementación real de <see cref="IDocumentoStorageService"/> — escribe
/// directo sobre un filesystem, cuya raíz viene de
/// <c>Documentos__RutaAlmacenamiento</c> (obligatoria, sin default
/// silencioso: si falta, el módulo debe fallar ruidosamente al arrancar en
/// vez de escribir en cualquier carpeta del contenedor). En producción esa
/// raíz es el punto de montaje del NAS de la cooperativa (CIFS/SMB, ver
/// docker-compose.prod.yml) — este servicio nunca sabe ni le importa si es
/// un disco local o una carpeta de red montada, es exactamente el mismo
/// código en los dos casos.
/// </summary>
public class FileSystemDocumentoStorageService : IDocumentoStorageService
{
    private readonly string _raiz;

    public FileSystemDocumentoStorageService()
    {
        _raiz = Environment.GetEnvironmentVariable("Documentos__RutaAlmacenamiento")
            ?? throw new InvalidOperationException(
                "Falta Documentos__RutaAlmacenamiento -- sin esta variable, la Biblioteca de " +
                "Documentos no sabe dónde guardar los archivos reales (NAS en producción, carpeta " +
                "local en desarrollo) y no debe arrancar guardando en cualquier lado.");
    }

    public async Task<string> GuardarAsync(Stream contenido, string nombreOriginal, CancellationToken cancellationToken = default)
    {
        var hoy = DateTime.UtcNow;
        var carpetaRelativa = Path.Combine(hoy.Year.ToString(), hoy.Month.ToString("00"));
        var carpetaAbsoluta = Path.Combine(_raiz, carpetaRelativa);
        Directory.CreateDirectory(carpetaAbsoluta);

        // Nombre único real (nunca solo el original: dos usuarios pueden
        // subir "Manual.pdf" el mismo mes) — el nombre original se conserva
        // aparte en Documento.NombreArchivoOriginal para mostrar/descargar.
        var nombreArchivo = $"{Guid.NewGuid():N}_{SanitizarNombre(nombreOriginal)}";
        var rutaRelativa = Path.Combine(carpetaRelativa, nombreArchivo).Replace('\\', '/');
        var rutaAbsoluta = Path.Combine(carpetaAbsoluta, nombreArchivo);

        await using var destino = File.Create(rutaAbsoluta);
        await contenido.CopyToAsync(destino, cancellationToken);

        return rutaRelativa;
    }

    public Task<Stream> AbrirAsync(string rutaRelativa, CancellationToken cancellationToken = default)
    {
        var rutaAbsoluta = Path.Combine(_raiz, rutaRelativa.Replace('/', Path.DirectorySeparatorChar));
        Stream stream = File.OpenRead(rutaAbsoluta);
        return Task.FromResult(stream);
    }

    private static string SanitizarNombre(string nombre)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var limpio = new string(nombre.Select(c => invalidos.Contains(c) ? '_' : c).ToArray());
        return limpio.Length > 150 ? limpio[..150] : limpio;
    }
}
