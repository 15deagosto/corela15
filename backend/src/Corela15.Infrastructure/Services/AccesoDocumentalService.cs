using Corela15.Application.Documentos;
using Corela15.Domain.Documentos;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>Ver <see cref="IAccesoDocumentalService"/> — administra el ACL real por área de la Biblioteca de Documentos.</summary>
public class AccesoDocumentalService(Corela15DbContext db) : IAccesoDocumentalService
{
    public async Task<IReadOnlyList<AreaAccesoItem>> ListarAsync(string? area, CancellationToken cancellationToken = default)
    {
        var query = db.AreaAccesosDocumentales.Include(a => a.Usuario).AsQueryable();
        if (!string.IsNullOrWhiteSpace(area))
        {
            var areaEnum = ParsearArea(area);
            query = query.Where(a => a.Area == areaEnum);
        }

        var lista = await query.OrderBy(a => a.Area).ThenBy(a => a.Usuario.NombreUsuario).ToListAsync(cancellationToken);
        return lista.Select(a => new AreaAccesoItem(a.Id, a.IdUsuario, a.Usuario.NombreUsuario, a.Area.ToString(), a.NivelAcceso.ToString(), a.CreadoEn, a.CreadoPor)).ToList();
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

    public async Task OtorgarAsync(OtorgarAccesoAreaRequest request, CancellationToken cancellationToken = default)
    {
        var area = ParsearArea(request.Area);
        if (!Enum.TryParse<NivelAccesoDocumental>(request.NivelAcceso, ignoreCase: true, out var nivel))
            throw new NivelAccesoInvalidoException(request.NivelAcceso);

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == request.IdUsuario && u.Activo, cancellationToken)
            ?? throw new UsuarioAccesoInvalidoException();

        var existente = await db.AreaAccesosDocumentales
            .FirstOrDefaultAsync(a => a.IdUsuario == usuario.Id && a.Area == area, cancellationToken);

        if (existente is not null)
        {
            // Ya tenía acceso a esta área -- se actualiza el nivel (ej.
            // Lectura -> Escritura), nunca se duplica la fila.
            existente.NivelAcceso = nivel;
        }
        else
        {
            db.AreaAccesosDocumentales.Add(new AreaAccesoUsuario
            {
                Id = Guid.NewGuid(),
                IdUsuario = usuario.Id,
                Area = area,
                NivelAcceso = nivel,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = request.RegistradoPor,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task QuitarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var acceso = await db.AreaAccesosDocumentales.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (acceso is null) return;
        db.AreaAccesosDocumentales.Remove(acceso);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, string>> ObtenerAccesoPorAreaAsync(Guid idUsuario, CancellationToken cancellationToken = default)
    {
        var accesos = await db.AreaAccesosDocumentales
            .Where(a => a.IdUsuario == idUsuario)
            .Select(a => new { a.Area, a.NivelAcceso })
            .ToListAsync(cancellationToken);

        return accesos.ToDictionary(a => a.Area.ToString(), a => a.NivelAcceso.ToString());
    }

    private static AreaDocumental ParsearArea(string valor) =>
        Enum.TryParse<AreaDocumental>(valor, ignoreCase: true, out var area)
            ? area
            : throw new ValorCatalogoDocumentoInvalidoException("área", valor);
}
