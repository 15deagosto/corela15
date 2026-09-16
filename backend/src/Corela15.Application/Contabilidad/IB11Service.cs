namespace Corela15.Application.Contabilidad;

public record B11CuentaDetalle(string Codigo, string Nombre, decimal Total);

public record B11Result(
    string CodigoEstructura, string Ruc, DateOnly FechaCorte, int NumeroRegistros, decimal ValorCuadre,
    IReadOnlyList<B11CuentaDetalle> Detalle, IReadOnlyList<string> Advertencias);

/// <summary>
/// Estructura B11 (Balance de Comprobación, SEPS) real — a diferencia del
/// balance ya construido sobre el ledger propio de Corela15
/// (<c>ReportesController.GenerarEstadoFinancieroAsync</c>, que usa
/// <c>saldo_contable</c> propio, todavía casi sin datos reales), este
/// servicio calcula B11 en vivo desde el ledger real de Softbank
/// (<c>CONTABILIDAD.SALDOCONTABLE</c>/<c>CUENTACONTABLE</c>), verificado
/// campo por campo contra el archivo real de referencia (SEPS, corte
/// 31/08/2026, ver <c>07-verificacion-estructuras-seps-agosto2026.md</c>):
/// agencia CSD (consolidado, ya sumado, nunca sumar las 4 agencias),
/// fórmula SALDOINICIAL+TOTALDEBITO-TOTALCREDITO, y la eliminación real
/// de transferencias internas (cuentas 1908/2908 y todos sus
/// descendientes forzados a 0.00, confirmado byte a byte contra el XML
/// real). Segunda excepción real, acotada, a la regla de oro del
/// proyecto (misma <c>Softbank__ConnectionStringRo</c> ya usada por
/// <see cref="Obligacion.IObligacionSyncService"/>).
/// </summary>
public interface IB11Service
{
    Task<B11Result> GenerarAsync(DateOnly fechaCorte, CancellationToken cancellationToken = default);
}
