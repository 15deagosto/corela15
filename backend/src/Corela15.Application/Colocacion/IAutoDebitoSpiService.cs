using Corela15.Application.Common;

namespace Corela15.Application.Colocacion;

public record AutoDebitoSpiDetalle(
    string NumeroPrestamo, string? NumeroCuenta, int? NumeroCuota, bool Debitado, string Motivo, decimal Monto);

public record AutoDebitoSpiEjecutadoResult(
    DateOnly Fecha, int Debitados, int Omitidos, decimal TotalDebitado,
    Guid? IdComprobanteContable, IReadOnlyList<AutoDebitoSpiDetalle> Detalles);

public interface IAutoDebitoSpiService
{
    /// <summary>
    /// Activa/desactiva el auto-débito de cuota por SPI de un préstamo
    /// (Prestamo.DebitoSpi). No mueve dinero por sí solo — solo habilita
    /// que EjecutarAsync lo considere en la próxima corrida.
    /// </summary>
    Task ConfigurarAsync(Guid idPrestamo, bool activar, string registradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Corre el batch diario: para cada préstamo vigente con DebitoSpi=true
    /// que todavía no se procesó hoy, valida las TRES configuraciones como
    /// una sola fuente de verdad (Prestamo.DebitoSpi,
    /// TipoCuenta.PermiteDebitoPrestamo, CuentaItemSaldo.AcreditaPrestamo) —
    /// exactamente el cruce que el incidente real documentado en
    /// 01-contexto-origen.md encontró que el proceso original ignoraba — y
    /// si las tres se cumplen y hay saldo suficiente, debita la próxima
    /// cuota pendiente de la cuenta del socio. Registra en
    /// AutoDebitoSpiLog cada préstamo procesado, se debite o se omita, con
    /// el motivo explícito.
    /// </summary>
    Task<AutoDebitoSpiEjecutadoResult> EjecutarAsync(string registradoPor, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutoDebitoSpiDetalle>> HistorialAsync(CancellationToken cancellationToken = default);
}

public class PrestamoInvalidoParaDebitoSpiException(Guid idPrestamo)
    : ReglaDeNegocioException($"El préstamo {idPrestamo} no existe o no está vigente");
