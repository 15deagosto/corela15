namespace Corela15.Infrastructure.Reporteria.Query;

/// <summary>
/// Petición del frontend. Nunca contiene SQL: solo identificadores que el
/// compilador valida contra el modelo semántico. Portado de SIGA.
/// </summary>
public sealed record QueryRequest
{
    public required string Dataset { get; init; }
    public string[] Dimensions { get; init; } = [];
    public string[] Measures { get; init; } = [];
    public FilterValue[] Filters { get; init; } = [];

    /// <summary>Fecha para consulta histórica (FOR SYSTEM_TIME AS OF). Null = estado actual.</summary>
    public DateTime? Snapshot { get; init; }

    public string? OrderBy { get; init; }
    public bool OrderDesc { get; init; } = true;
    public int Limit { get; init; } = 5000;
}

public sealed record FilterValue(string Id, string[] Values);

public sealed record CompiledQuery(
    string Sql,
    IReadOnlyDictionary<string, object> Parameters,
    ColumnMeta[] Columns);

public sealed record ColumnMeta(string Id, string Label, string Kind, string Format);

public sealed record QueryResult(
    ColumnMeta[] Columns,
    IReadOnlyList<Dictionary<string, object?>> Rows,
    long ElapsedMs,
    bool Truncated);

public sealed class QueryCompilerException(string message) : Exception(message);
