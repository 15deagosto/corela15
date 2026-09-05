namespace Corela15.Application.Obligacion;

/// <summary>
/// Única excepción real y acotada a la "regla de oro" del proyecto
/// (el backend nunca se conecta a Softbank, solo a Postgres): por
/// decisión explícita del usuario, el módulo de Estructuras Financieras
/// necesita poder sincronizar datos reales de OBLIGACION_FINANCIERA
/// desde Softbank en el momento, sin depender de correr una herramienta
/// externa por consola (ver el histórico `sync-of01`, cuyo mapeo real
/// campo por campo se reutiliza acá tal cual, verificado en su momento
/// contra Softbank en vivo).
///
/// Alcance deliberadamente angosto: solo esta interfaz, solo consumida
/// por <c>ObligacionesFinancierasController</c> (protegido por
/// `Menu:estructuras-financieras` + `Estructura:OF01`), y la cadena de
/// conexión de Softbank vive en su propia variable de entorno
/// (`Softbank__ConnectionStringRo`), separada por completo de
/// `ConnectionStrings__Core` -- nunca se mezclan ni se comparten.
/// </summary>
public interface IObligacionSyncService
{
    Task<SincronizacionObligacionesResult> SincronizarAsync(CancellationToken cancellationToken = default);
}

public record SincronizacionObligacionesResult(
    int EncontradasEnSoftbank,
    int Sincronizadas,
    int Omitidas,
    IReadOnlyList<string> Detalle);
