namespace Corela15.Infrastructure.Reporteria.Semantic;

/// <summary>
/// Un dataset del modelo semántico. Define el universo cerrado de dimensiones,
/// métricas y filtros que un usuario puede combinar. El compilador de consultas
/// SOLO puede emitir expresiones SQL declaradas aquí. Portado del proyecto SIGA.
/// </summary>
public sealed class Dataset
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public string? Description { get; init; }

    /// <summary>Roles del modelo original de SIGA (Gerencia/Negocios/TI) -- ya
    /// no se usa para autorizar en Corela15 (eso lo resuelve el claim real
    /// `dataset` del JWT, ver <c>seguridad.rol_dataset_reporteria</c>); se
    /// conserva el campo solo por compatibilidad con los JSON ya escritos.</summary>
    public string[] Roles { get; init; } = [];

    /// <summary>Si admite consulta histórica con FOR SYSTEM_TIME AS OF.</summary>
    public bool SupportsSnapshot { get; init; }

    /// <summary>Bloque FROM/JOIN. El marcador {{T}} se reemplaza por la cláusula temporal.</summary>
    public required string BaseSql { get; init; }

    /// <summary>Predicado siempre aplicado (reglas de negocio no negociables).</summary>
    public string? BaseWhere { get; init; }

    public Dimension[] Dimensions { get; init; } = [];
    public Measure[] Measures { get; init; } = [];
    public FilterDef[] Filters { get; init; } = [];
}

public sealed class Dimension
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Sql { get; init; }
    public string? Group { get; init; }
}

public sealed class Measure
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Sql { get; init; }
    /// <summary>money | pct | int | num</summary>
    public string Format { get; init; } = "num";
    public string? Description { get; init; }
    /// <summary>Si es true, un valor alto es malo (para semáforos en la UI).</summary>
    public bool HigherIsWorse { get; init; }
}

public sealed class FilterDef
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Sql { get; init; }
    /// <summary>multi | single | dateFrom | dateTo</summary>
    public string Type { get; init; } = "multi";
    /// <summary>Consulta que llena el desplegable de opciones. Solo SELECT.</summary>
    public string? CatalogSql { get; init; }
}

/// <summary>Descripción que se envía al frontend (sin exponer el SQL).</summary>
public sealed record DatasetInfo(
    string Id, string Label, string? Description, bool SupportsSnapshot,
    ItemInfo[] Dimensions, MeasureInfo[] Measures, FilterInfo[] Filters);

public sealed record ItemInfo(string Id, string Label, string? Group);
public sealed record MeasureInfo(string Id, string Label, string Format, string? Description, bool HigherIsWorse);
public sealed record FilterInfo(string Id, string Label, string Type, bool HasCatalog);
