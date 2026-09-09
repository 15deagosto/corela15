using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Corela15.Infrastructure.Reporteria.Semantic;

/// <summary>
/// Carga y expone los datasets definidos como recursos embebidos
/// (Reporteria/Semantic/models/*.json, portados del proyecto SIGA) -- es la
/// única fuente de verdad sobre qué SQL puede generar el módulo de
/// Reportería Gerencial. Embebidos (no archivos sueltos en disco) por el
/// mismo motivo que <c>CatalogoCucCompleto.sql</c>: la ruta de un archivo
/// suelto depende del directorio de trabajo real al arrancar, que varía
/// entre `dotnet run` local y el contenedor Docker de producción; un
/// recurso embebido siempre viaja dentro del ensamblado.
/// </summary>
public sealed class SemanticModelStore
{
    private readonly Dictionary<string, Dataset> _datasets = new(StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public SemanticModelStore(ILogger<SemanticModelStore> log)
    {
        var assembly = typeof(SemanticModelStore).Assembly;
        var prefix = "Corela15.Infrastructure.Reporteria.Semantic.models.";
        var nombres = assembly.GetManifestResourceNames().Where(n => n.StartsWith(prefix) && n.EndsWith(".json"));

        foreach (var nombre in nombres)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(nombre)
                    ?? throw new InvalidOperationException($"No se pudo abrir el recurso embebido {nombre}.");
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                var ds = JsonSerializer.Deserialize<Dataset>(json, JsonOpts);
                if (ds is null) continue;
                Validate(ds);
                _datasets[ds.Id] = ds;
                log.LogInformation("Dataset de Reportería Gerencial cargado: {Id} ({D} dims, {M} métricas, {F} filtros)",
                    ds.Id, ds.Dimensions.Length, ds.Measures.Length, ds.Filters.Length);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error cargando el modelo semántico embebido {Nombre}", nombre);
                throw;
            }
        }
    }

    /// <summary>
    /// Verifica en el arranque que ninguna expresión del modelo contenga
    /// verbos de escritura. Falla temprano y ruidosamente si algo está mal.
    /// </summary>
    private static void Validate(Dataset ds)
    {
        IEnumerable<string> All()
        {
            yield return ds.BaseSql;
            if (ds.BaseWhere is not null) yield return ds.BaseWhere;
            foreach (var d in ds.Dimensions) yield return d.Sql;
            foreach (var m in ds.Measures) yield return m.Sql;
            foreach (var f in ds.Filters)
            {
                yield return f.Sql;
                if (f.CatalogSql is not null) yield return f.CatalogSql;
            }
        }

        foreach (var sql in All())
            Reporteria.Security.SqlGuard.AssertNoWriteVerbs(sql, $"modelo semántico '{ds.Id}'");

        var dupDim = ds.Dimensions.GroupBy(x => x.Id).FirstOrDefault(g => g.Count() > 1);
        if (dupDim is not null) throw new InvalidOperationException($"Dimensión duplicada: {dupDim.Key}");

        var dupMea = ds.Measures.GroupBy(x => x.Id).FirstOrDefault(g => g.Count() > 1);
        if (dupMea is not null) throw new InvalidOperationException($"Métrica duplicada: {dupMea.Key}");
    }

    public Dataset? Get(string id) => _datasets.GetValueOrDefault(id);

    public IEnumerable<Dataset> All() => _datasets.Values;

    /// <summary>Datasets visibles según los códigos de dataset otorgados al usuario (claim `dataset` del JWT).</summary>
    public IEnumerable<DatasetInfo> Describe(IReadOnlyCollection<string> datasetsOtorgados) =>
        _datasets.Values
            .Where(d => datasetsOtorgados.Contains(d.Id, StringComparer.OrdinalIgnoreCase))
            .Select(d => new DatasetInfo(
                d.Id, d.Label, d.Description, d.SupportsSnapshot,
                d.Dimensions.Select(x => new ItemInfo(x.Id, x.Label, x.Group)).ToArray(),
                d.Measures.Select(x => new MeasureInfo(x.Id, x.Label, x.Format, x.Description, x.HigherIsWorse)).ToArray(),
                d.Filters.Select(x => new FilterInfo(x.Id, x.Label, x.Type, x.CatalogSql is not null)).ToArray()));
}
