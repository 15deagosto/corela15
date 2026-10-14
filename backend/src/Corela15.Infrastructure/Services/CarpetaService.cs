using Corela15.Application.Documentos;
using Corela15.Domain.Documentos;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>Ver <see cref="ICarpetaService"/> — árbol real de carpetas de la Biblioteca de Documentos.</summary>
public class CarpetaService(Corela15DbContext db) : ICarpetaService
{
    public async Task<IReadOnlyList<CarpetaItem>> ListarAsync(ContextoAccesoDocumental contexto, Guid idUsuarioActual, string nombreUsuarioActual, CancellationToken cancellationToken = default)
    {
        var carpetas = await db.Carpetas.Where(c => c.Activa).ToListAsync(cancellationToken);
        var origen = await ResolverOrigenCompartidoAsync(carpetas, idUsuarioActual, nombreUsuarioActual, cancellationToken);

        CarpetaItem Item(Carpeta c, string nivel, bool escribe)
        {
            var (compartida, por) = origen.TryGetValue(c.Id, out var o) ? o : (false, null);
            return new CarpetaItem(c.Id, c.Nombre, c.IdCarpetaPadre, c.Activa, nivel, escribe, escribe, compartida, por);
        }

        if (contexto.VeTodo)
            return carpetas.Select(c => Item(c, "Escritura", true)).ToList();

        var resultado = new List<CarpetaItem>();
        foreach (var c in carpetas)
        {
            if (!contexto.AccesoPorCarpeta.TryGetValue(c.Id, out var nivel)) continue;
            resultado.Add(Item(c, nivel, nivel == "Escritura"));
        }
        return resultado;
    }

    /// <summary>
    /// Marca qué carpetas le compartió OTRA persona al usuario actual (pedido
    /// explícito: "que se identifique que es una carpeta que le compartieron
    /// y quién se la compartió"). El dueño real de un árbol es el dueño de
    /// su carpeta raíz: la primera fila de ACL que se creó en ella (la de su
    /// carpeta personal, ver <see cref="AsegurarCarpetaPersonalAsync"/>).
    /// Quien la compartió es el que creó la fila de acceso del usuario en la
    /// carpeta que hoy define su acceso; si esa fila la creó él mismo o una
    /// migración, se muestra al dueño del árbol.
    /// </summary>
    private async Task<Dictionary<Guid, (bool Compartida, string? Por)>> ResolverOrigenCompartidoAsync(
        List<Carpeta> carpetas, Guid idUsuarioActual, string nombreUsuarioActual, CancellationToken cancellationToken)
    {
        var accesos = await db.CarpetaAccesos
            .Select(a => new { a.IdCarpeta, a.IdUsuario, a.CreadoEn, a.CreadoPor })
            .ToListAsync(cancellationToken);
        var porCarpeta = accesos.GroupBy(a => a.IdCarpeta).ToDictionary(g => g.Key, g => g.ToList());
        var padrePorId = carpetas.ToDictionary(c => c.Id, c => c.IdCarpetaPadre);

        var usuarios = await db.Usuarios
            .Select(u => new { u.Id, u.NombreUsuario, Nombre = u.Persona != null ? u.Persona.Nombre : u.NombreCompleto })
            .ToListAsync(cancellationToken);
        var nombrePorId = usuarios.ToDictionary(u => u.Id, u => u.Nombre ?? u.NombreUsuario);
        var nombrePorUsuario = usuarios
            .GroupBy(u => u.NombreUsuario, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Nombre ?? g.First().NombreUsuario, StringComparer.OrdinalIgnoreCase);

        Guid Raiz(Guid id)
        {
            var visitados = new HashSet<Guid>();
            while (padrePorId.TryGetValue(id, out var padre) && padre is Guid p && padrePorId.ContainsKey(p) && visitados.Add(id))
                id = p;
            return id;
        }

        Guid? Autoritativa(Guid id)
        {
            Guid? actual = id;
            var visitados = new HashSet<Guid>();
            while (actual is Guid a && visitados.Add(a))
            {
                if (porCarpeta.ContainsKey(a)) return a;
                actual = padrePorId.TryGetValue(a, out var p) ? p : null;
            }
            return null;
        }

        var resultado = new Dictionary<Guid, (bool, string?)>();
        foreach (var c in carpetas)
        {
            var raiz = Raiz(c.Id);
            Guid? dueno = porCarpeta.TryGetValue(raiz, out var aclRaiz)
                ? aclRaiz.OrderBy(a => a.CreadoEn).First().IdUsuario
                : null;

            var aut = Autoritativa(c.Id);
            var miFila = aut is Guid idAut ? porCarpeta[idAut].FirstOrDefault(a => a.IdUsuario == idUsuarioActual) : null;
            var otorgante = miFila?.CreadoPor;
            var otorganteValido = otorgante is not null
                && !string.Equals(otorgante, nombreUsuarioActual, StringComparison.OrdinalIgnoreCase)
                && nombrePorUsuario.ContainsKey(otorgante);

            var compartida = dueno is Guid d ? d != idUsuarioActual : otorganteValido;
            if (!compartida) continue;

            // Sin una fila propia (ej. gerencia documental que ve todo) se
            // muestra solo al dueño -- nunca se inventa un otorgante.
            string? por = dueno is Guid idDueno ? nombrePorId.GetValueOrDefault(idDueno) : null;
            if (otorganteValido)
            {
                var nombreOtorgante = nombrePorUsuario[otorgante!];
                // Si el que otorgó es distinto del dueño (ej. alguien con
                // escritura volvió a compartir), se muestran ambos.
                por = por is null || por == nombreOtorgante ? nombreOtorgante : $"{nombreOtorgante} (carpeta de {por})";
            }
            resultado[c.Id] = (true, por);
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

    public async Task<Guid> AsegurarCarpetaPersonalAsync(Guid idUsuario, string nombreVisible, string creadoPor, CancellationToken cancellationToken = default)
    {
        // Carpeta personal real como raíz propia (sin agrupador -- se
        // simplificó a pedido explícito del usuario: nada de un catálogo
        // fijo compartido por defecto, cada quien ve solo la suya, y la
        // comparte él mismo si quiere, mismo mecanismo de siempre).
        var personal = await db.Carpetas.FirstOrDefaultAsync(
            c => c.Activa && c.IdCarpetaPadre == null && c.Nombre == nombreVisible, cancellationToken);
        if (personal is null)
        {
            personal = new Carpeta
            {
                Id = Guid.NewGuid(),
                Nombre = nombreVisible,
                IdCarpetaPadre = null,
                Activa = true,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = creadoPor,
            };
            db.Carpetas.Add(personal);
            await db.SaveChangesAsync(cancellationToken);
        }

        var accesoPersonal = await db.CarpetaAccesos.FirstOrDefaultAsync(
            a => a.IdCarpeta == personal.Id && a.IdUsuario == idUsuario, cancellationToken);
        if (accesoPersonal is null)
        {
            db.CarpetaAccesos.Add(new CarpetaAcceso
            {
                Id = Guid.NewGuid(),
                IdCarpeta = personal.Id,
                IdUsuario = idUsuario,
                NivelAcceso = NivelAccesoDocumental.Escritura,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = creadoPor,
            });
        }
        else if (accesoPersonal.NivelAcceso != NivelAccesoDocumental.Escritura)
        {
            accesoPersonal.NivelAcceso = NivelAccesoDocumental.Escritura;
        }

        await db.SaveChangesAsync(cancellationToken);
        return personal.Id;
    }
}
