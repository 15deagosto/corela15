using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record BalanceComprobacionLinea(
    string Codigo, string Nombre, string Grupo,
    decimal SaldoInicial, decimal Debitos, decimal Creditos, decimal SaldoFinal);

public record BalanceComprobacionResult(
    DateOnly Periodo, IReadOnlyList<BalanceComprobacionLinea> Lineas,
    decimal TotalDebitos, decimal TotalCreditos, bool Cuadrado);

/// <summary>
/// Reportes derivados 100% de datos ya existentes (saldo_contable), sin
/// estructura de ningún formulario regulatorio específico — ver la
/// advertencia explícita en CLAUDE.md sobre reporte_regulatorio: la
/// estructura de detalle de cada reporte SEPS/BCE (B13, D01, etc.) exige
/// verificación contra la norma oficial, que no se pudo completar (PDFs
/// binarios/no legibles). El Balance de Comprobación es un reporte
/// contable estándar, no un formulario regulatorio codificado, así que no
/// depende de esa verificación pendiente.
/// </summary>
[ApiController]
[Route("api/contabilidad/reportes")]
[Authorize(Policy = "Menu:contabilidad")]
public class ReportesController(Corela15DbContext db) : ControllerBase
{
    [HttpGet("periodos")]
    public async Task<ActionResult<IReadOnlyList<DateOnly>>> Periodos(CancellationToken cancellationToken)
    {
        var periodos = await db.SaldosContables
            .Select(s => s.Periodo)
            .Distinct()
            .OrderByDescending(p => p)
            .ToListAsync(cancellationToken);

        return Ok(periodos);
    }

    [HttpGet("balance-comprobacion")]
    public async Task<ActionResult<BalanceComprobacionResult>> BalanceComprobacion(
        [FromQuery] DateOnly periodo, CancellationToken cancellationToken)
    {
        var saldosAnteriores = await db.SaldosContables
            .Where(s => s.Periodo < periodo)
            .GroupBy(s => s.IdCuentaContable)
            .Select(g => new { IdCuentaContable = g.Key, SaldoInicial = g.Sum(s => s.SaldoFinal) })
            .ToDictionaryAsync(x => x.IdCuentaContable, x => x.SaldoInicial, cancellationToken);

        var saldosDelPeriodo = await db.SaldosContables
            .Where(s => s.Periodo == periodo)
            .ToDictionaryAsync(s => s.IdCuentaContable, cancellationToken);

        var cuentas = await db.CuentasContables
            .Where(c => c.EsMayor)
            .OrderBy(c => c.Codigo)
            .ToListAsync(cancellationToken);

        var lineas = new List<BalanceComprobacionLinea>();
        foreach (var cuenta in cuentas)
        {
            var saldoInicial = saldosAnteriores.GetValueOrDefault(cuenta.Id, 0m);
            saldosDelPeriodo.TryGetValue(cuenta.Id, out var saldoPeriodo);
            var debitos = saldoPeriodo?.TotalDebitos ?? 0m;
            var creditos = saldoPeriodo?.TotalCreditos ?? 0m;
            var movimientoNeto = saldoPeriodo?.SaldoFinal ?? 0m;
            var saldoFinal = saldoInicial + movimientoNeto;

            if (saldoInicial == 0m && debitos == 0m && creditos == 0m && saldoFinal == 0m)
            {
                continue;
            }

            lineas.Add(new BalanceComprobacionLinea(
                cuenta.Codigo, cuenta.Nombre, cuenta.Grupo.ToString(), saldoInicial, debitos, creditos, saldoFinal));
        }

        var totalDebitos = lineas.Sum(l => l.Debitos);
        var totalCreditos = lineas.Sum(l => l.Creditos);

        return Ok(new BalanceComprobacionResult(
            periodo, lineas, totalDebitos, totalCreditos, totalDebitos == totalCreditos));
    }
}
