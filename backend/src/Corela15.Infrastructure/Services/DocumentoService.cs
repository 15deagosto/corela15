using Corela15.Application.Documentos;
using Corela15.Domain.Documentos;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IDocumentoService"/> — portado real de CredVault
/// (apps.documents.Document/DocumentViewSet), nativo del core ahora. Cada
/// método real de lectura/escritura recibe un
/// <see cref="ContextoAccesoDocumental"/> ya resuelto (rol ADMINISTRADOR
/// + menú `biblioteca-documentos-gerencia` + ACL real por carpeta, ver
/// CarpetaAcceso) y lo aplica como deny-by-default real — sin acceso
/// explícito a una carpeta, el usuario no ve ni puede tocar sus documentos.
/// </summary>
public class DocumentoService(Corela15DbContext db, IDocumentoStorageService storage) : IDocumentoService
{
    private const int MaxMb = 20;
    private static readonly HashSet<string> ExtensionesPermitidas =
        new(StringComparer.OrdinalIgnoreCase) { "pdf", "doc", "docx", "xls", "xlsx", "txt", "ppt", "pptx" };

    public async Task<Guid> CrearAsync(CrearDocumentoRequest request, Stream archivo, string nombreArchivoOriginal, string contentType, long tamanoBytes, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo)) throw new TituloDocumentoRequeridoException();
        var carpeta = await db.Carpetas.FirstOrDefaultAsync(c => c.Id == request.IdCarpeta && c.Activa, cancellationToken)
            ?? throw new CarpetaInvalidaException();
        var area = ParsearEnum<AreaDocumental>("área", request.Area);
        var tipo = ParsearEnum<TipoDocumento>("tipo", request.Tipo);
        if (!contexto.TieneEscritura(carpeta.Id)) throw new AccesoDocumentalDenegadoException(carpeta.Nombre);
        ValidarArchivo(nombreArchivoOriginal, tamanoBytes);

        var ruta = await storage.GuardarAsync(archivo, nombreArchivoOriginal, cancellationToken);

        var documento = new Documento
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo.Trim(),
            IdCarpeta = carpeta.Id,
            Area = area,
            Tipo = tipo,
            Version = string.IsNullOrWhiteSpace(request.Version) ? "1.0" : request.Version.Trim(),
            Estado = EstadoDocumento.Borrador,
            NombreArchivoOriginal = nombreArchivoOriginal,
            RutaAlmacenamiento = ruta,
            TamanoBytes = tamanoBytes,
            ContentType = contentType,
            InstanciaAprobacion = request.InstanciaAprobacion,
            InstanciaRevision = request.InstanciaRevision,
            FechaAprobacion = request.FechaAprobacion,
            ProximaRevision = request.ProximaRevision,
            Notas = request.Notas,
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };

        db.Documentos.Add(documento);
        await db.SaveChangesAsync(cancellationToken);
        return documento.Id;
    }

    public async Task ActualizarAsync(Guid id, ActualizarDocumentoRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var documento = await BuscarConEscrituraAsync(id, contexto, cancellationToken);
        if (string.IsNullOrWhiteSpace(request.Titulo)) throw new TituloDocumentoRequeridoException();
        var area = ParsearEnum<AreaDocumental>("área", request.Area);
        var tipo = ParsearEnum<TipoDocumento>("tipo", request.Tipo);
        var estado = ParsearEnum<EstadoDocumento>("estado", request.Estado);

        // Si además intenta MOVER el documento a otra carpeta, necesita
        // Escritura también sobre la carpeta destino — moverlo a una
        // carpeta ajena sin permiso ahí sería una forma real de eludir el ACL.
        if (request.IdCarpeta != documento.IdCarpeta)
        {
            var carpetaDestino = await db.Carpetas.FirstOrDefaultAsync(c => c.Id == request.IdCarpeta && c.Activa, cancellationToken)
                ?? throw new CarpetaInvalidaException();
            if (!contexto.TieneEscritura(carpetaDestino.Id)) throw new AccesoDocumentalDenegadoException(carpetaDestino.Nombre);
            documento.IdCarpeta = carpetaDestino.Id;
        }

        documento.Titulo = request.Titulo.Trim();
        documento.Area = area;
        documento.Tipo = tipo;
        documento.Estado = estado;
        documento.InstanciaAprobacion = request.InstanciaAprobacion;
        documento.InstanciaRevision = request.InstanciaRevision;
        documento.FechaAprobacion = request.FechaAprobacion;
        documento.ProximaRevision = request.ProximaRevision;
        documento.Notas = request.Notas;
        documento.ModificadoEn = DateTimeOffset.UtcNow;
        documento.ModificadoPor = request.ModificadoPor;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SubirNuevaVersionAsync(Guid id, SubirNuevaVersionRequest request, Stream archivo, string nombreArchivoOriginal, string contentType, long tamanoBytes, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var documento = await BuscarConEscrituraAsync(id, contexto, cancellationToken);
        ValidarArchivo(nombreArchivoOriginal, tamanoBytes);

        // El archivo anterior se queda en el NAS tal cual — nunca se borra
        // (mismo criterio "nunca DELETE real" de todo el core), solo deja
        // de ser el que este documento referencia.
        var ruta = await storage.GuardarAsync(archivo, nombreArchivoOriginal, cancellationToken);

        documento.NombreArchivoOriginal = nombreArchivoOriginal;
        documento.RutaAlmacenamiento = ruta;
        documento.TamanoBytes = tamanoBytes;
        documento.ContentType = contentType;
        documento.Version = string.IsNullOrWhiteSpace(request.Version) ? documento.Version : request.Version.Trim();
        documento.ModificadoEn = DateTimeOffset.UtcNow;
        documento.ModificadoPor = request.ModificadoPor;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DesactivarAsync(Guid id, string modificadoPor, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var documento = await BuscarConEscrituraAsync(id, contexto, cancellationToken);
        documento.Activo = false;
        documento.ModificadoEn = DateTimeOffset.UtcNow;
        documento.ModificadoPor = modificadoPor;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentoListItem>> ListarAsync(ListarDocumentosFiltro filtro, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        // Si pide una carpeta puntual sin lectura ahí, no es un error --
        // simplemente no hay nada visible para mostrar (mismo criterio
        // "deny-by-default silencioso" que una carpeta compartida real).
        if (filtro.IdCarpeta is Guid idCarpetaFiltro && !contexto.TieneLectura(idCarpetaFiltro))
            return [];

        var query = db.Documentos.Include(d => d.Carpeta).Where(d => d.Activo).AsQueryable();

        if (!contexto.VeTodo)
        {
            var carpetasVisibles = contexto.AccesoPorCarpeta.Keys.ToList();
            query = query.Where(d => carpetasVisibles.Contains(d.IdCarpeta));
        }

        if (filtro.IdCarpeta is Guid idCarpeta)
        {
            query = query.Where(d => d.IdCarpeta == idCarpeta);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Area))
        {
            var area = ParsearEnum<AreaDocumental>("área", filtro.Area);
            query = query.Where(d => d.Area == area);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Tipo))
        {
            var tipo = ParsearEnum<TipoDocumento>("tipo", filtro.Tipo);
            query = query.Where(d => d.Tipo == tipo);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Estado))
        {
            var estado = ParsearEnum<EstadoDocumento>("estado", filtro.Estado);
            query = query.Where(d => d.Estado == estado);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var b = filtro.Busqueda.Trim();
            query = query.Where(d => EF.Functions.ILike(d.Titulo, $"%{b}%") || (d.Notas != null && EF.Functions.ILike(d.Notas, $"%{b}%")));
        }

        var lista = await query.OrderByDescending(d => d.ModificadoEn ?? d.CreadoEn).ToListAsync(cancellationToken);
        return lista.Select(Proyectar).ToList();
    }

    public async Task<DocumentoListItem?> ObtenerAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var documento = await db.Documentos.Include(d => d.Carpeta).FirstOrDefaultAsync(d => d.Id == id && d.Activo, cancellationToken);
        // Sin lectura sobre la carpeta -- se trata igual que "no existe", nunca
        // se distingue (no hay que confirmarle a nadie que un documento
        // ajeno existe).
        if (documento is null || !contexto.TieneLectura(documento.IdCarpeta)) return null;
        return Proyectar(documento);
    }

    public async Task<DescargaDocumentoResult> DescargarAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var documento = await db.Documentos.FirstOrDefaultAsync(d => d.Id == id && d.Activo, cancellationToken);
        if (documento is null || !contexto.TieneLectura(documento.IdCarpeta)) throw new DocumentoInvalidoException();

        var contenido = await storage.AbrirAsync(documento.RutaAlmacenamiento, cancellationToken);
        return new DescargaDocumentoResult(contenido, documento.NombreArchivoOriginal, documento.ContentType);
    }

    public async Task<MatrizDocumentalResult> ObtenerMatrizAsync(ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        if (!contexto.VeTodo) throw new AccesoDocumentalDenegadoException("todas (matriz de cobertura)");

        // Siempre sobre el universo completo de áreas×tipos, nunca sobre
        // filtros de un listado — mismo criterio real de CredVault
        // (matriz de cobertura documental). El área sigue siendo el
        // metadato de clasificación real para esta vista, aunque el
        // acceso real hoy lo decide la carpeta, no el área.
        var datos = await db.Documentos.Where(d => d.Activo)
            .Select(d => new { d.Area, d.Tipo, d.Estado })
            .ToListAsync(cancellationToken);

        var filas = new List<MatrizFilaResult>();
        foreach (AreaDocumental area in Enum.GetValues<AreaDocumental>())
        {
            var celdas = new Dictionary<string, MatrizCeldaResult>();
            foreach (TipoDocumento tipo in Enum.GetValues<TipoDocumento>())
            {
                var delCelda = datos.Where(d => d.Area == area && d.Tipo == tipo).ToList();
                celdas[tipo.ToString()] = new MatrizCeldaResult(delCelda.Count, delCelda.Count(d => d.Estado == EstadoDocumento.Vigente));
            }
            filas.Add(new MatrizFilaResult(area.ToString(), celdas));
        }

        return new MatrizDocumentalResult(filas);
    }

    private async Task<Documento> BuscarConEscrituraAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.Include(d => d.Carpeta).FirstOrDefaultAsync(d => d.Id == id && d.Activo, cancellationToken)
            ?? throw new DocumentoInvalidoException();
        if (!contexto.TieneEscritura(documento.IdCarpeta)) throw new AccesoDocumentalDenegadoException(documento.Carpeta.Nombre);
        return documento;
    }

    private static void ValidarArchivo(string nombreOriginal, long tamanoBytes)
    {
        if (string.IsNullOrWhiteSpace(nombreOriginal)) throw new ArchivoDocumentoRequeridoException();

        var extension = Path.GetExtension(nombreOriginal).TrimStart('.').ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension)) throw new ExtensionNoPermitidaException(extension);

        if (tamanoBytes > MaxMb * 1024L * 1024L) throw new ArchivoDemasiadoGrandeException(MaxMb);
    }

    private static TEnum ParsearEnum<TEnum>(string campo, string valor) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(valor, ignoreCase: true, out var resultado)
            ? resultado
            : throw new ValorCatalogoDocumentoInvalidoException(campo, valor);

    private static DocumentoListItem Proyectar(Documento d) => new(
        d.Id, d.IdCarpeta, d.Carpeta.Nombre, d.Area.ToString(), d.Tipo.ToString(), d.Titulo, d.Version, d.Estado.ToString(),
        d.NombreArchivoOriginal, d.TamanoBytes,
        d.InstanciaAprobacion, d.InstanciaRevision, d.FechaAprobacion, d.ProximaRevision, d.Notas,
        d.CreadoPor, d.CreadoEn, d.ModificadoEn);
}
