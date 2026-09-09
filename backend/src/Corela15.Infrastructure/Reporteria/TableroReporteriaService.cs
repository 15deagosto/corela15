using System.Text.Json;
using Corela15.Application.Reporteria;
using Corela15.Domain.Reporteria;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Corela15.Infrastructure.Reporteria;

public sealed record TableroInput(string Nombre, string? Descripcion, string Definicion, bool EsPublico, string[] RolesPermitidos);
public sealed record PermisosTableroInput(bool EsPublico, string[] RolesPermitidos);

public sealed record TableroDto(
    int Id, string Nombre, string? Descripcion, string Definicion, string Propietario,
    bool EsPublico, string[] RolesPermitidos, bool EsPredefinido, DateTimeOffset CreadoEn, DateTimeOffset ModificadoEn);

public sealed record TableroResumenDto(
    int Id, string Nombre, string? Descripcion, string Propietario, bool EsPublico,
    string[] RolesPermitidos, bool EsPredefinido, DateTimeOffset ModificadoEn,
    bool EsMio, bool EsFavorito, string[] Datasets);

/// <summary>
/// Persistencia real de tableros guardados/favoritos/auditoría del módulo
/// Reportería Gerencial -- portado del `TableroStore`/`AuditService` de
/// SIGA, con dos diferencias reales: vive en el propio Postgres de
/// Corela15 (no un SQLite local aparte), y la visibilidad se resuelve
/// contra los roles reales del usuario (`ClaimTypes.Role` del JWT), no un
/// solo rol fijo (Gerencia/Negocios/TI) como en SIGA -- un usuario de
/// Corela15 puede tener varios roles a la vez.
/// </summary>
public sealed class TableroReporteriaService(Corela15DbContext db, ILogger<TableroReporteriaService> log)
{
    private static string[] ExtraerDatasets(string definicionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(definicionJson);
            if (!doc.RootElement.TryGetProperty("widgets", out var widgets) || widgets.ValueKind != JsonValueKind.Array)
                return [];

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var w in widgets.EnumerateArray())
                if (w.TryGetProperty("dataset", out var ds) && ds.ValueKind == JsonValueKind.String)
                    set.Add(ds.GetString()!);
            return [.. set];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool EsVisible(Tablero t, string usuario, IReadOnlyCollection<string> roles) =>
        t.Propietario == usuario ||
        (t.EsPublico && (t.RolesPermitidos.Length == 0 || t.RolesPermitidos.Any(roles.Contains)));

    private static TableroResumenDto AResumen(Tablero t, string usuario, bool esFavorito) =>
        new(t.Id, t.Nombre, t.Descripcion, t.Propietario, t.EsPublico, t.RolesPermitidos, t.EsPredefinido,
            t.ModificadoEn, t.Propietario == usuario, esFavorito, ExtraerDatasets(t.Definicion));

    private static TableroDto ADto(Tablero t) =>
        new(t.Id, t.Nombre, t.Descripcion, t.Definicion, t.Propietario, t.EsPublico, t.RolesPermitidos,
            t.EsPredefinido, t.CreadoEn, t.ModificadoEn);

    public async Task<IReadOnlyList<TableroResumenDto>> ListarAsync(string usuario, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        var favoritos = await db.FavoritosTablero.Where(f => f.Usuario == usuario).Select(f => f.IdTablero).ToListAsync(ct);
        var todos = await db.TablerosReporteria.OrderByDescending(t => t.ModificadoEn).ToListAsync(ct);

        return todos
            .Where(t => EsVisible(t, usuario, roles))
            .Select(t => AResumen(t, usuario, favoritos.Contains(t.Id)))
            .OrderByDescending(t => t.EsFavorito)
            .ThenByDescending(t => t.ModificadoEn)
            .ToArray();
    }

    /// <summary>Panel de administración: todos los tableros, sin filtrar por visibilidad. El caller ya validó que el usuario puede administrar.</summary>
    public async Task<IReadOnlyList<TableroResumenDto>> ListarTodosAsync(string usuario, CancellationToken ct)
    {
        var favoritos = await db.FavoritosTablero.Where(f => f.Usuario == usuario).Select(f => f.IdTablero).ToListAsync(ct);
        var todos = await db.TablerosReporteria.OrderByDescending(t => t.ModificadoEn).ToListAsync(ct);
        return todos.Select(t => AResumen(t, usuario, favoritos.Contains(t.Id))).ToArray();
    }

    public async Task<TableroDto?> ObtenerAsync(int id, string usuario, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        var t = await db.TablerosReporteria.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null || !EsVisible(t, usuario, roles)) return null;
        return ADto(t);
    }

    public async Task<TableroDto> CrearAsync(TableroInput input, string usuario, CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var t = new Tablero
        {
            Nombre = input.Nombre,
            Descripcion = input.Descripcion,
            Definicion = input.Definicion,
            Propietario = usuario,
            EsPublico = input.EsPublico,
            RolesPermitidos = input.RolesPermitidos,
            EsPredefinido = false,
            CreadoEn = ahora,
            ModificadoEn = ahora,
        };
        db.TablerosReporteria.Add(t);
        await db.SaveChangesAsync(ct);
        return ADto(t);
    }

    public async Task<TableroDto?> ActualizarAsync(int id, TableroInput input, string usuario, CancellationToken ct)
    {
        var t = await db.TablerosReporteria.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return null;
        if (t.Propietario != usuario) throw new TableroSinPermisoException();

        t.Nombre = input.Nombre;
        t.Descripcion = input.Descripcion;
        t.Definicion = input.Definicion;
        t.EsPublico = input.EsPublico;
        t.RolesPermitidos = input.RolesPermitidos;
        t.ModificadoEn = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return ADto(t);
    }

    /// <summary>Panel de administración: cambia visibilidad de cualquier tablero, sin importar el dueño. El caller ya validó que el usuario puede administrar.</summary>
    public async Task<bool> ActualizarPermisosAsync(int id, PermisosTableroInput input, CancellationToken ct)
    {
        var t = await db.TablerosReporteria.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return false;
        t.EsPublico = input.EsPublico;
        t.RolesPermitidos = input.RolesPermitidos;
        t.ModificadoEn = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> BorrarAsync(int id, string usuario, CancellationToken ct)
    {
        var t = await db.TablerosReporteria.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t is null) return false;
        if (t.Propietario != usuario) throw new TableroSinPermisoException();

        db.TablerosReporteria.Remove(t); // favorito_tablero cae en cascada por FK
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task MarcarFavoritoAsync(int idTablero, string usuario, CancellationToken ct)
    {
        var yaExiste = await db.FavoritosTablero.AnyAsync(f => f.Usuario == usuario && f.IdTablero == idTablero, ct);
        if (yaExiste) return;
        db.FavoritosTablero.Add(new FavoritoTablero { Usuario = usuario, IdTablero = idTablero, AgregadoEn = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(ct);
    }

    public async Task QuitarFavoritoAsync(int idTablero, string usuario, CancellationToken ct)
    {
        var f = await db.FavoritosTablero.FirstOrDefaultAsync(x => x.Usuario == usuario && x.IdTablero == idTablero, ct);
        if (f is null) return;
        db.FavoritosTablero.Remove(f);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Nunca lanza -- si la auditoría falla, la consulta ya se le devolvió
    /// al usuario y no debe verse afectada (mismo criterio que
    /// `Siga.Api.AuditService`). El fallo queda en el log del servidor.
    /// </summary>
    public async Task RegistrarAuditoriaAsync(
        string usuario, string dataset, string[] dimensiones, string[] metricas, string filtrosJson,
        DateTime? snapshot, long duracionMs, int filas, string? error, CancellationToken ct)
    {
        try
        {
            db.AuditoriaConsultaReporteria.Add(new AuditoriaConsultaReporteria
            {
                Usuario = usuario,
                Dataset = dataset,
                Dimensiones = string.Join(",", dimensiones),
                Metricas = string.Join(",", metricas),
                Filtros = filtrosJson,
                Snapshot = snapshot,
                DuracionMs = duracionMs,
                Filas = filas,
                Error = error,
                FechaHora = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "No se pudo registrar la auditoría de Reportería Gerencial para {Usuario}", usuario);
        }
    }
}
