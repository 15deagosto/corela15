using Corela15.Application.Documentos;
using Corela15.Domain.Documentos;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IAccesoDocumentalService"/> — administra el ACL real por
/// carpeta de la Biblioteca de Documentos, con herencia real (una carpeta
/// sin filas propias hereda de la carpeta padre más cercana que sí tenga
/// filas; en cuanto tiene filas propias, esas son las únicas que aplican
/// para ella y sus descendientes).
/// </summary>
public class AccesoDocumentalService(Corela15DbContext db) : IAccesoDocumentalService
{
    public async Task<IReadOnlyList<CarpetaAccesoItem>> ListarAsync(Guid idCarpeta, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var carpeta = await db.Carpetas.FirstOrDefaultAsync(c => c.Id == idCarpeta && c.Activa, cancellationToken)
            ?? throw new CarpetaInvalidaException();
        if (!contexto.TieneEscritura(carpeta.Id)) throw new AccesoDocumentalDenegadoException(carpeta.Nombre);

        var lista = await db.CarpetaAccesos
            .Include(a => a.Usuario)
            .Where(a => a.IdCarpeta == idCarpeta)
            .OrderBy(a => a.Usuario.NombreUsuario)
            .ToListAsync(cancellationToken);

        return lista.Select(a => new CarpetaAccesoItem(a.Id, a.IdUsuario, a.Usuario.NombreUsuario, a.IdCarpeta, carpeta.Nombre, a.NivelAcceso.ToString(), a.CreadoEn, a.CreadoPor)).ToList();
    }

    public async Task<IReadOnlyList<UsuarioParaAccesoItem>> BuscarUsuariosAsync(string q, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2) return [];

        return await db.Usuarios
            .Where(u => u.Activo && (EF.Functions.ILike(u.NombreUsuario, $"%{q}%") || (u.NombreCompleto != null && EF.Functions.ILike(u.NombreCompleto, $"%{q}%"))))
            .OrderBy(u => u.NombreUsuario)
            .Take(15)
            .Select(u => new UsuarioParaAccesoItem(u.Id, u.NombreUsuario))
            .ToListAsync(cancellationToken);
    }

    public async Task OtorgarAsync(Guid idCarpeta, OtorgarAccesoCarpetaRequest request, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var carpeta = await db.Carpetas.FirstOrDefaultAsync(c => c.Id == idCarpeta && c.Activa, cancellationToken)
            ?? throw new CarpetaInvalidaException();
        if (!contexto.TieneEscritura(carpeta.Id)) throw new AccesoDocumentalDenegadoException(carpeta.Nombre);

        if (!Enum.TryParse<NivelAccesoDocumental>(request.NivelAcceso, ignoreCase: true, out var nivel))
            throw new NivelAccesoInvalidoException(request.NivelAcceso);

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == request.IdUsuario && u.Activo, cancellationToken)
            ?? throw new UsuarioAccesoInvalidoException();

        var existente = await db.CarpetaAccesos
            .FirstOrDefaultAsync(a => a.IdUsuario == usuario.Id && a.IdCarpeta == idCarpeta, cancellationToken);

        if (existente is not null)
        {
            // Ya tenía acceso a esta carpeta -- se actualiza el nivel (ej.
            // Lectura -> Escritura), nunca se duplica la fila.
            existente.NivelAcceso = nivel;
        }
        else
        {
            db.CarpetaAccesos.Add(new CarpetaAcceso
            {
                Id = Guid.NewGuid(),
                IdUsuario = usuario.Id,
                IdCarpeta = idCarpeta,
                NivelAcceso = nivel,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = request.RegistradoPor,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task QuitarAsync(Guid id, ContextoAccesoDocumental contexto, CancellationToken cancellationToken = default)
    {
        var acceso = await db.CarpetaAccesos.Include(a => a.Carpeta).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (acceso is null) return;
        if (!contexto.TieneEscritura(acceso.IdCarpeta)) throw new AccesoDocumentalDenegadoException(acceso.Carpeta.Nombre);

        db.CarpetaAccesos.Remove(acceso);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> ResolverAccesoEfectivoAsync(Guid idUsuario, CancellationToken cancellationToken = default)
    {
        var carpetas = await db.Carpetas
            .Select(c => new { c.Id, c.IdCarpetaPadre })
            .ToListAsync(cancellationToken);
        var padrePorId = carpetas.ToDictionary(c => c.Id, c => c.IdCarpetaPadre);

        // Carpetas que tienen AL MENOS una fila de ACL propia (de
        // cualquier usuario) -- el punto real donde se corta la herencia.
        var carpetasConAclPropio = (await db.CarpetaAccesos
            .Select(a => a.IdCarpeta)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();

        // Para cada carpeta, la carpeta "autoritativa" real: ella misma si
        // tiene ACL propio, si no la primera ancestro (subiendo) que sí lo
        // tenga, si ninguna la tiene -- null (nadie, salvo VeTodo, la ve).
        var autoritativaPorCarpeta = new Dictionary<Guid, Guid?>();
        foreach (var carpetaId in padrePorId.Keys)
        {
            if (autoritativaPorCarpeta.ContainsKey(carpetaId)) continue;
            ResolverAutoritativa(carpetaId, padrePorId, carpetasConAclPropio, autoritativaPorCarpeta);
        }

        var accesosDelUsuario = await db.CarpetaAccesos
            .Where(a => a.IdUsuario == idUsuario)
            .Select(a => new { a.IdCarpeta, a.NivelAcceso })
            .ToListAsync(cancellationToken);
        var nivelPorCarpetaAutoritativa = accesosDelUsuario.ToDictionary(a => a.IdCarpeta, a => a.NivelAcceso.ToString());

        var resultado = new Dictionary<Guid, string>();
        foreach (var (carpetaId, autoritativaId) in autoritativaPorCarpeta)
        {
            if (autoritativaId is Guid autId && nivelPorCarpetaAutoritativa.TryGetValue(autId, out var nivel))
                resultado[carpetaId] = nivel;
        }
        return resultado;
    }

    /// <summary>Resuelve recursivamente hacia la raíz, memoizando en el diccionario compartido para no recorrer la misma cadena dos veces.</summary>
    private static Guid? ResolverAutoritativa(Guid carpetaId, IReadOnlyDictionary<Guid, Guid?> padrePorId, HashSet<Guid> carpetasConAclPropio, Dictionary<Guid, Guid?> memo)
    {
        if (memo.TryGetValue(carpetaId, out var yaResuelto)) return yaResuelto;

        if (carpetasConAclPropio.Contains(carpetaId))
        {
            memo[carpetaId] = carpetaId;
            return carpetaId;
        }

        var padreId = padrePorId.TryGetValue(carpetaId, out var p) ? p : null;
        Guid? autoritativa = padreId is Guid padre
            ? ResolverAutoritativa(padre, padrePorId, carpetasConAclPropio, memo)
            : null;

        memo[carpetaId] = autoritativa;
        return autoritativa;
    }
}
