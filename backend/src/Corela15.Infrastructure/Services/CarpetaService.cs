using Corela15.Application.Documentos;
using Corela15.Domain.Documentos;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>Ver <see cref="ICarpetaService"/> — árbol real de carpetas de la Biblioteca de Documentos.</summary>
public class CarpetaService(Corela15DbContext db) : ICarpetaService
{
    public async Task<IReadOnlyList<CarpetaItem>> ListarAsync(ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var carpetas = await db.Carpetas.Where(c => c.Activa).ToListAsync(cancellationToken);

        if (contexto.VeTodo)
            return carpetas.Select(c => new CarpetaItem(c.Id, c.Nombre, c.IdCarpetaPadre, c.Activa, "Escritura", true, true)).ToList();

        var resultado = new List<CarpetaItem>();
        foreach (var c in carpetas)
        {
            if (!contexto.AccesoPorCarpeta.TryGetValue(c.Id, out var nivel)) continue;
            var puedeEscribir = nivel == "Escritura";
            resultado.Add(new CarpetaItem(c.Id, c.Nombre, c.IdCarpetaPadre, c.Activa, nivel, puedeEscribir, puedeEscribir));
        }
        return resultado;
    }

    public async Task<Guid> CrearAsync(CrearCarpetaRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre)) throw new NombreCarpetaRequeridoException();

        if (request.IdCarpetaPadre is null)
        {
            // Crear una carpeta raíz nueva (sin padre) exige ver todo -- no
            // hay ninguna carpeta ancestro de la que heredar el permiso de
            // "puedo crear acá", así que solo Administración/Gerencia
            // documental puede abrir una raíz nueva.
            if (!contexto.VeTodo) throw new AccesoDocumentalDenegadoException("raíz");
        }
        else
        {
            var padre = await db.Carpetas.FirstOrDefaultAsync(c => c.Id == request.IdCarpetaPadre && c.Activa, cancellationToken)
                ?? throw new CarpetaInvalidaException();
            if (!contexto.TieneEscritura(padre.Id)) throw new AccesoDocumentalDenegadoException(padre.Nombre);
        }

        var carpeta = new Carpeta
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            IdCarpetaPadre = request.IdCarpetaPadre,
            Activa = true,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.CreadoPor,
        };
        db.Carpetas.Add(carpeta);
        await db.SaveChangesAsync(cancellationToken);
        return carpeta.Id;
    }

    public async Task RenombrarAsync(Guid id, RenombrarCarpetaRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre)) throw new NombreCarpetaRequeridoException();
        var carpeta = await BuscarConEscrituraAsync(id, contexto, cancellationToken);
        carpeta.Nombre = request.Nombre.Trim();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DesactivarAsync(Guid id, string modificadoPor, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var carpeta = await BuscarConEscrituraAsync(id, contexto, cancellationToken);

        var tieneContenido = await db.Documentos.AnyAsync(d => d.IdCarpeta == id && d.Activo, cancellationToken)
            || await db.Carpetas.AnyAsync(c => c.IdCarpetaPadre == id && c.Activa, cancellationToken);
        if (tieneContenido) throw new CarpetaConContenidoException();

        carpeta.Activa = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Carpeta> BuscarConEscrituraAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken)
    {
        var carpeta = await db.Carpetas.FirstOrDefaultAsync(c => c.Id == id && c.Activa, cancellationToken)
            ?? throw new CarpetaInvalidaException();
        if (!contexto.TieneEscritura(carpeta.Id)) throw new AccesoDocumentalDenegadoException(carpeta.Nombre);
        return carpeta;
    }
}
