using System.Text;
using Corela15.Infrastructure.Reporteria.Semantic;
using Microsoft.Extensions.Configuration;
// GetValue<T>() es un método de extensión de Microsoft.Extensions.Configuration.Binder.

namespace Corela15.Infrastructure.Reporteria.Query;

/// <summary>
/// Compila una QueryRequest en SQL parametrizado. Portado de SIGA, con un
/// solo cambio real: la autorización por dataset ya no viene de un rol
/// fijo del JSON (`Dataset.Roles`, concepto propio de SIGA) sino del claim
/// real `dataset` del JWT de Corela15 (`seguridad.rol_dataset_reporteria`)
/// -- el llamador (controller) pasa los códigos de dataset que el usuario
/// tiene otorgados.
///
/// Garantías (sin cambios frente a SIGA):
///   - Solo emite expresiones declaradas en el modelo semántico.
///   - Cualquier identificador desconocido aborta la compilación.
///   - Los valores de filtro van siempre como parámetros, nunca concatenados.
/// </summary>
public sealed class QueryCompiler(SemanticModelStore store, IConfiguration config)
{
    private readonly int _maxRows = config.GetValue("Reporteria:MaxRows", 50_000);

    public CompiledQuery Compile(QueryRequest req, IReadOnlyCollection<string> datasetsOtorgados)
    {
        var ds = store.Get(req.Dataset)
            ?? throw new QueryCompilerException($"Dataset desconocido: {req.Dataset}");

        if (!datasetsOtorgados.Contains(ds.Id, StringComparer.OrdinalIgnoreCase))
            throw new QueryCompilerException($"No tiene acceso al dataset '{ds.Id}'.");

        if (req.Dimensions.Length == 0 && req.Measures.Length == 0)
            throw new QueryCompilerException("Se requiere al menos una dimensión o una métrica.");

        if (req.Dimensions.Length > 8)
            throw new QueryCompilerException("Máximo 8 dimensiones por consulta.");

        // --- whitelist estricta ---
        var dims = req.Dimensions
            .Select(id => ds.Dimensions.FirstOrDefault(d => d.Id == id)
                ?? throw new QueryCompilerException($"Dimensión no permitida: {id}"))
            .ToArray();

        var meas = req.Measures
            .Select(id => ds.Measures.FirstOrDefault(m => m.Id == id)
                ?? throw new QueryCompilerException($"Métrica no permitida: {id}"))
            .ToArray();

        var pars = new Dictionary<string, object>();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(ds.BaseWhere))
            where.Add($"({ds.BaseWhere})");

        var pi = 0;
        foreach (var f in req.Filters)
        {
            var def = ds.Filters.FirstOrDefault(x => x.Id == f.Id)
                ?? throw new QueryCompilerException($"Filtro no permitido: {f.Id}");

            if (f.Values.Length == 0) continue;

            switch (def.Type)
            {
                case "dateFrom":
                    if (!DateTime.TryParse(f.Values[0], out var df))
                        throw new QueryCompilerException($"Fecha inválida en filtro {f.Id}");
                    pars[$"@p{pi}"] = df;
                    where.Add($"{def.Sql} >= @p{pi++}");
                    break;

                case "dateTo":
                    if (!DateTime.TryParse(f.Values[0], out var dt))
                        throw new QueryCompilerException($"Fecha inválida en filtro {f.Id}");
                    pars[$"@p{pi}"] = dt;
                    where.Add($"{def.Sql} <= @p{pi++}");
                    break;

                default: // multi | single
                    if (f.Values.Length > 500)
                        throw new QueryCompilerException($"Demasiados valores en el filtro {f.Id} (máximo 500).");
                    var names = new List<string>(f.Values.Length);
                    foreach (var v in f.Values)
                    {
                        var n = $"@p{pi++}";
                        pars[n] = v;   // SIEMPRE parámetro
                        names.Add(n);
                    }
                    where.Add($"{def.Sql} IN ({string.Join(", ", names)})");
                    break;
            }
        }

        // --- foto temporal ---
        var temporal = string.Empty;
        if (req.Snapshot is { } snap)
        {
            if (!ds.SupportsSnapshot)
                throw new QueryCompilerException($"El dataset '{ds.Id}' no admite consulta histórica.");
            if (snap.Date > DateTime.Now.Date)
                throw new QueryCompilerException("La fecha de corte no puede ser futura.");

            // El core cierra el día con un proceso batch nocturno: "corte al
            // 01/07" en su reportería significa el estado YA PROCESADO al
            // final de ese día -- ver siga/CLAUDE.md, verificado contra
            // ÍNDICE DE MOROSIDAD (corte 01/07/2026).
            pars["@snapshot"] = snap.Date.AddDays(1);
            temporal = "FOR SYSTEM_TIME AS OF @snapshot";
        }

        var top = Math.Clamp(req.Limit, 1, _maxRows);

        var select = new List<string>(dims.Length + meas.Length);
        var cols = new List<ColumnMeta>(dims.Length + meas.Length);

        foreach (var d in dims)
        {
            select.Add($"{d.Sql} AS [{d.Id}]");
            cols.Add(new ColumnMeta(d.Id, d.Label, "dimension", "text"));
        }
        foreach (var m in meas)
        {
            select.Add($"{m.Sql} AS [{m.Id}]");
            cols.Add(new ColumnMeta(m.Id, m.Label, "measure", m.Format));
        }

        var sb = new StringBuilder();
        sb.AppendLine("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;");
        sb.AppendLine($"SELECT TOP ({top})");
        sb.AppendLine("       " + string.Join(",\n       ", select));
        sb.AppendLine(ds.BaseSql.Replace("{{T}}", temporal));

        if (where.Count > 0)
            sb.AppendLine("WHERE " + string.Join("\n  AND ", where));

        if (dims.Length > 0)
            sb.AppendLine("GROUP BY " + string.Join(", ", dims.Select(d => d.Sql)));

        if (req.OrderBy is { } ob)
        {
            var valido = dims.Any(d => d.Id == ob) || meas.Any(m => m.Id == ob);
            if (!valido) throw new QueryCompilerException($"Orden no permitido: {ob}");
            sb.AppendLine($"ORDER BY [{ob}] {(req.OrderDesc ? "DESC" : "ASC")}");
        }
        else if (meas.Length > 0)
        {
            sb.AppendLine($"ORDER BY [{meas[0].Id}] DESC");
        }
        else if (dims.Length > 0)
        {
            sb.AppendLine($"ORDER BY [{dims[0].Id}] ASC");
        }

        return new CompiledQuery(sb.ToString(), pars, cols.ToArray());
    }
}
