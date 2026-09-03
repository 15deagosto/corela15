using Corela15.Application.Colocacion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Job programático real del auto-débito de cuota por SPI — antes solo se
/// ejecutaba manual desde la pantalla (pendiente documentado en
/// CLAUDE.md). Corre una vez al día a la hora fija configurada
/// (`HoraEjecucionUtc`, por defecto 06:00 UTC ≈ 01:00 hora Ecuador,
/// horario bancario antes de que abran las agencias) — `IAutoDebitoSpiService.
/// EjecutarAsync` ya es idempotente por diseño (índice único Fecha+
/// IdPrestamo en `AutoDebitoSpiLog`), así que si el proceso se reinicia y
/// esta corrida ya se hizo hoy, correrla de nuevo no duplica nada.
/// El botón manual en `Creditos.tsx` se mantiene para corridas fuera de
/// horario (ej. un préstamo activado después de la corrida del día).
/// </summary>
public class AutoDebitoSpiBackgroundService(
    IServiceScopeFactory scopeFactory, ILogger<AutoDebitoSpiBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan HoraEjecucionUtc = TimeSpan.FromHours(6);
    private const string RegistradoPorSistema = "sistema:auto-debito-spi-job";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var espera = CalcularProximaEspera(DateTime.UtcNow);
            try
            {
                await Task.Delay(espera, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var servicio = scope.ServiceProvider.GetRequiredService<IAutoDebitoSpiService>();
                var resultado = await servicio.EjecutarAsync(RegistradoPorSistema, stoppingToken);
                logger.LogInformation(
                    "Auto-débito SPI (job programático) {Fecha}: {Debitados} debitados, {Omitidos} omitidos, {Total:0.00} total.",
                    resultado.Fecha, resultado.Debitados, resultado.Omitidos, resultado.TotalDebitado);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Nunca tumbar el proceso completo por un fallo del job — se
                // reintenta en la próxima corrida programada, y el botón
                // manual sigue disponible mientras tanto.
                logger.LogError(ex, "Falló la corrida programada del auto-débito SPI.");
            }
        }
    }

    internal static TimeSpan CalcularProximaEspera(DateTime ahoraUtc)
    {
        var proximaCorrida = ahoraUtc.Date + HoraEjecucionUtc;
        if (proximaCorrida <= ahoraUtc)
        {
            proximaCorrida = proximaCorrida.AddDays(1);
        }

        return proximaCorrida - ahoraUtc;
    }
}
