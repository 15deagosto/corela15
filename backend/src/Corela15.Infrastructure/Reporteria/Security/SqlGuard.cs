using System.Text.RegularExpressions;

namespace Corela15.Infrastructure.Reporteria.Security;

/// <summary>
/// Última barrera antes de enviar SQL al motor de Reportería Gerencial
/// (Softbank, solo lectura). Portado tal cual del proyecto SIGA -- ver
/// <see cref="Corela15.Application.Obligacion.IObligacionSyncService"/>
/// para la otra excepción real ya existente a la "regla de oro" del
/// proyecto (el backend nunca escribe en Softbank; acá tampoco lee más
/// allá de SELECT). Defensa en profundidad: el compilador ya solo emite
/// expresiones del modelo semántico, esto garantiza que ningún camino --
/// presente o futuro -- pueda enviar una escritura.
/// </summary>
public static partial class SqlGuard
{
    private static readonly string[] WriteVerbs =
    [
        "INSERT", "UPDATE", "DELETE", "MERGE", "TRUNCATE", "DROP", "ALTER",
        "CREATE", "GRANT", "REVOKE", "DENY", "BACKUP", "RESTORE", "SHUTDOWN",
        "RECONFIGURE", "KILL", "OPENROWSET", "OPENQUERY", "BULK",
    ];

    [GeneratedRegex(@"\b(sp_|xp_)\w+", RegexOptions.IgnoreCase)]
    private static partial Regex ProcRegex();

    [GeneratedRegex(@"\bEXEC(UTE)?\b", RegexOptions.IgnoreCase)]
    private static partial Regex ExecRegex();

    /// <summary>
    /// Lanza si el fragmento contiene verbos de escritura.
    /// Usado al validar el modelo semántico en el arranque.
    /// </summary>
    public static void AssertNoWriteVerbs(string sql, string context)
    {
        foreach (var verb in WriteVerbs)
            if (Regex.IsMatch(sql, $@"\b{verb}\b", RegexOptions.IgnoreCase))
                throw new InvalidOperationException(
                    $"Verbo de escritura '{verb}' detectado en {context}. Reportería Gerencial es de solo lectura.");

        if (ProcRegex().IsMatch(sql))
            throw new InvalidOperationException($"Llamada a procedimiento detectada en {context}.");

        if (ExecRegex().IsMatch(sql))
            throw new InvalidOperationException($"EXEC detectado en {context}.");
    }

    /// <summary>
    /// Verifica una consulta completa antes de ejecutarla.
    /// Debe empezar por SET TRANSACTION ISOLATION y no contener escrituras.
    /// </summary>
    public static void AssertQuerySafe(string sql)
    {
        var trimmed = sql.TrimStart();

        if (!trimmed.StartsWith("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED",
                                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Toda consulta debe iniciar con SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED.");

        AssertNoWriteVerbs(sql, "consulta compilada");

        // Un solo lote: no permitimos apilar sentencias.
        if (sql.Contains("GO\n", StringComparison.OrdinalIgnoreCase) ||
            sql.Contains("GO\r", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Separador de lote no permitido.");

        // Debe haber exactamente un SELECT principal.
        var selects = Regex.Matches(sql, @"\bSELECT\b", RegexOptions.IgnoreCase).Count;
        if (selects == 0)
            throw new InvalidOperationException("La consulta no contiene SELECT.");
    }
}
