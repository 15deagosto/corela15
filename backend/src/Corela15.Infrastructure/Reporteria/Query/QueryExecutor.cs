using System.Diagnostics;
using Corela15.Infrastructure.Reporteria.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Corela15.Infrastructure.Reporteria.Query;

/// <summary>
/// Ejecuta consultas compiladas contra Softbank. Solo lectura, con timeout
/// y verificación previa de seguridad. Portado de SIGA. Segunda excepción
/// real y acotada a la "regla de oro" del proyecto -- ver
/// <see cref="Corela15.Application.Obligacion.IObligacionSyncService"/>
/// para la primera. La cadena de conexión (`Softbank__ConnectionStringRo`
/// o, en su defecto, `ConnectionStrings__Softbank` -- el nombre que ya
/// usaba SIGA en el `.env` reservado en la raíz del repo) es de solo
/// lectura (`db_datareader`), nunca la misma que `ConnectionStrings__Core`.
/// </summary>
public sealed class QueryExecutor(IConfiguration config, ILogger<QueryExecutor> log)
{
    private readonly string _cs = config["Softbank:ConnectionStringRo"]
        ?? config.GetConnectionString("Softbank")
        ?? throw new InvalidOperationException(
            "Falta Softbank__ConnectionStringRo o ConnectionStrings__Softbank (revisar .env en la raíz del repo).");
    private readonly int _timeout = config.GetValue("Reporteria:CommandTimeoutSeconds", 30);

    public async Task<QueryResult> ExecuteAsync(CompiledQuery q, CancellationToken ct = default)
    {
        SqlGuard.AssertQuerySafe(q.Sql);

        var sw = Stopwatch.StartNew();
        var rows = new List<Dictionary<string, object?>>();

        await using var cn = new SqlConnection(_cs);
        await cn.OpenAsync(ct);

        await using var cmd = new SqlCommand(q.Sql, cn) { CommandTimeout = _timeout };
        foreach (var (k, v) in q.Parameters)
            cmd.Parameters.AddWithValue(k, v);

        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>(rd.FieldCount);
            for (var i = 0; i < rd.FieldCount; i++)
                row[rd.GetName(i)] = await rd.IsDBNullAsync(i, ct) ? null : rd.GetValue(i);
            rows.Add(row);
        }

        sw.Stop();
        log.LogInformation("Reportería Gerencial: consulta ejecutada en {Ms} ms, {Filas} filas", sw.ElapsedMilliseconds, rows.Count);

        return new QueryResult(q.Columns, rows, sw.ElapsedMilliseconds, Truncated: false);
    }

    /// <summary>Llena las opciones de un filtro. Solo catálogos pequeños.</summary>
    public async Task<List<CatalogItem>> CatalogAsync(string sql, CancellationToken ct = default)
    {
        SqlGuard.AssertNoWriteVerbs(sql, "catálogo de filtro");

        var items = new List<CatalogItem>();
        await using var cn = new SqlConnection(_cs);
        await cn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, cn) { CommandTimeout = _timeout };
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            var value = rd.GetValue(0)?.ToString() ?? "";
            var label = rd.FieldCount > 1 ? rd.GetValue(1)?.ToString() ?? value : value;
            items.Add(new CatalogItem(value, label));
        }
        return items;
    }
}

public sealed record CatalogItem(string Value, string Label);
