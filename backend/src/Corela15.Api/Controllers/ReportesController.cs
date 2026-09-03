using Corela15.Domain.Contabilidad;
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

public record EstadoFinancieroCabecera(
    string CodigoEstructura, string Ruc, DateOnly FechaCorte, int NumeroTotalRegistros, decimal ValorCuadre);

public record EstadoFinancieroDetalleItem(string CodigoCuentaContable, string NombreCuentaContable, decimal SaldoCuentaContable);

public record EstadoFinancieroResult(
    EstadoFinancieroCabecera Cabecera, IReadOnlyList<EstadoFinancieroDetalleItem> Detalle, IReadOnlyList<string> Advertencias);

public record MayorAuxiliarLinea(DateOnly Fecha, long NumeroComprobante, string Descripcion, decimal Debito, decimal Credito, decimal SaldoCorriente);

public record MayorAuxiliarResult(
    string CodigoCuenta, string NombreCuenta, DateOnly Desde, DateOnly Hasta, decimal SaldoInicial,
    IReadOnlyList<MayorAuxiliarLinea> Lineas, decimal SaldoFinal, decimal TotalDebitos, decimal TotalCreditos);

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

    // Grupos CUC 62/63/72/73 (contrapartidas de cuentas de orden/contingentes)
    // — excluidos de B11/B13 por instrucción explícita del manual técnico.
    private static readonly string[] GruposExcluidos = ["62", "63", "72", "73"];

    // Cuentas/subcuentas que el manual autoriza a reportar en negativo (el
    // resto debe ser positivo salvo elemento 3, grupos 35/36 y 3502/3504) —
    // Manual Técnico de Estructuras de Datos "Estados Financieros" v10.0,
    // sección 4.1, cuadro de "Saldo de cuenta contable".
    private static readonly HashSet<string> CuentasNegativasPermitidas =
    [
        "1399", "139905", "139910", "1499", "149905", "149910", "149915", "149920",
        "149940", "149945", "149950", "149955", "149980", "149985", "149987", "149989",
        "1699", "169905", "169910", "169915", "169920", "170599", "170699", "1799",
        "179910", "1899", "189905", "189910", "189915", "189920", "189925", "189930",
        "189940", "190499", "190599", "1999", "199905", "199910", "199990", "3602", "3604",
    ];

    /// <summary>
    /// Genera la estructura B11 (mensual) o B13 (diaria) tal como la define
    /// el "Manual Técnico de Estructuras de Datos - Estados Financieros"
    /// v10.0 de SEPS (fuente real, provista por el usuario — no la web
    /// pública, cuyos PDF vienen como imagen no legible). Campo a campo:
    /// cabecera (código de estructura, RUC, fecha de corte, número de
    /// registros, valor de cuadre) + detalle (código/nombre/saldo por
    /// cuenta). B13 solo puede generarse "a hoy": nuestro modelo de saldos
    /// es mensual, no diario, así que no existe una foto exacta de un día
    /// pasado arbitrario — limitación real, documentada en vez de fingida.
    /// **No reemplaza la validación de la SEPS**: el número de registros no
    /// va a coincidir con el oficial (1.192 para COAC) porque el catálogo
    /// de cuentas sembrado en este core es un subconjunto operativo, no el
    /// CUC completo — queda como advertencia explícita, no oculta.
    /// </summary>
    private async Task<EstadoFinancieroResult> GenerarEstadoFinancieroAsync(
        string codigoEstructura, DateOnly fechaCorte, CancellationToken cancellationToken)
    {
        var periodo = new DateOnly(fechaCorte.Year, fechaCorte.Month, 1);

        var empresa = await db.Empresas.FirstOrDefaultAsync(cancellationToken);
        var ruc = empresa?.Ruc ?? string.Empty;

        var saldosLeaf = await db.SaldosContables
            .Where(s => s.Periodo <= periodo)
            .GroupBy(s => s.IdCuentaContable)
            .Select(g => new { IdCuentaContable = g.Key, SaldoAcumulado = g.Sum(s => s.SaldoFinal) })
            .ToDictionaryAsync(x => x.IdCuentaContable, x => x.SaldoAcumulado, cancellationToken);

        var todasLasCuentas = await db.CuentasContables
            .Where(c => c.Activa)
            .OrderBy(c => c.Codigo)
            .ToListAsync(cancellationToken);

        // El "cuadre jerárquico" que el manual exige (elemento = suma de sus
        // grupos, grupo = suma de sus cuentas, cuenta = suma de sus
        // subcuentas) solo es posible de validar del lado de SEPS si las
        // filas de agrupación TAMBIÉN van en el archivo — no solo las hojas.
        // Confirmado con el conteo real: 1.194 cuentas activas − 3 de los
        // grupos excluidos (62/63/73) = 1.191, prácticamente exacto contra
        // el 1.192 esperado del manual (la única cuenta de diferencia es el
        // código anómalo de 3 dígitos "671" que se excluyó de la siembra
        // por no encajar en ningún nivel válido de la jerarquía). El saldo
        // de una cuenta de agrupación es la suma de TODOS sus descendientes
        // hoja, calculado bottom-up con memoización sobre el árbol real de
        // IdCuentaPadre — no una lectura directa de saldo_contable, que solo
        // existe para las cuentas de detalle donde se postea de verdad.
        var porId = todasLasCuentas.ToDictionary(c => c.Id);
        var hijosPorPadre = todasLasCuentas
            .Where(c => c.IdCuentaPadre.HasValue)
            .GroupBy(c => c.IdCuentaPadre!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        var saldoCalculado = new Dictionary<Guid, decimal>();

        decimal CalcularSaldo(CuentaContable cuenta)
        {
            if (saldoCalculado.TryGetValue(cuenta.Id, out var yaCalculado))
            {
                return yaCalculado;
            }

            decimal resultado;
            if (cuenta.EsMayor)
            {
                resultado = saldosLeaf.GetValueOrDefault(cuenta.Id, 0m);
            }
            else
            {
                var hijos = hijosPorPadre.GetValueOrDefault(cuenta.Id, []);
                resultado = hijos.Sum(h => CalcularSaldo(h));
            }

            saldoCalculado[cuenta.Id] = resultado;
            return resultado;
        }

        var detalle = new List<EstadoFinancieroDetalleItem>();
        var advertencias = new List<string>();

        foreach (var cuenta in todasLasCuentas)
        {
            if (GruposExcluidos.Any(g => cuenta.Codigo.StartsWith(g)))
            {
                continue;
            }

            var saldo = CalcularSaldo(cuenta);

            // El control de saldo positivo/negativo del manual aplica al
            // nivel de detalle real (donde se postea) — las cuentas de
            // agrupación heredan el signo que resulte de sus hijas sin
            // validarlas aparte, para no duplicar la misma advertencia en
            // cada nivel de la jerarquía.
            if (cuenta.EsMayor)
            {
                var puedeSerNegativo = cuenta.Grupo == GrupoCuc.Patrimonio
                    || cuenta.Codigo.StartsWith("35") || cuenta.Codigo.StartsWith("36")
                    || cuenta.Codigo is "3502" or "3504"
                    || CuentasNegativasPermitidas.Contains(cuenta.Codigo);

                if (saldo < 0 && !puedeSerNegativo)
                {
                    advertencias.Add(
                        $"Cuenta {cuenta.Codigo} ({cuenta.Nombre}) tiene saldo negativo ({saldo:0.00}) pero el manual no la autoriza a reportarse en negativo.");
                }
            }

            detalle.Add(new EstadoFinancieroDetalleItem(cuenta.Codigo, cuenta.Nombre, saldo));
        }

        var valorCuadre = detalle.Sum(d => d.SaldoCuentaContable);

        const int registrosEsperadosCoac = 1192;
        if (detalle.Count != registrosEsperadosCoac)
        {
            advertencias.Add(
                $"Número de registros ({detalle.Count}) no coincide exacto con el esperado oficial para COAC " +
                $"({registrosEsperadosCoac}, Manual Técnico v10.0) — diferencia de {Math.Abs(detalle.Count - registrosEsperadosCoac)}. " +
                "El código '671' del catálogo oficial (3 dígitos, no encaja en la jerarquía elemento/grupo/cuenta/subcuenta) " +
                "se excluyó de la siembra en vez de adivinar su posición — ver CLAUDE.md.");
        }

        if (string.IsNullOrEmpty(ruc))
        {
            advertencias.Add("La empresa no tiene RUC configurado (Configuración > Empresa) — campo obligatorio de la cabecera.");
        }

        var cabecera = new EstadoFinancieroCabecera(codigoEstructura, ruc, fechaCorte, detalle.Count, valorCuadre);
        return new EstadoFinancieroResult(cabecera, detalle, advertencias);
    }

    // Mayor auxiliar — reporte real verificado contra el catálogo de
    // Softbank (Contabilidad.LibroAuxilar, SEGURIDAD.MENU_REPORTE, ver
    // 06-catalogo-reportes-softbank.md): historial de movimientos de UNA
    // cuenta contable en un rango de fechas, con saldo corriente — a
    // diferencia del Balance de Comprobación (agregado por período
    // completo), este es el detalle línea por línea de una cuenta
    // específica, el reporte que un contador usa para auditar una cuenta
    // puntual.
    [HttpGet("mayor-auxiliar")]
    public async Task<ActionResult<MayorAuxiliarResult>> MayorAuxiliar(
        [FromQuery] Guid idCuenta, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var cuenta = await db.CuentasContables.FirstOrDefaultAsync(c => c.Id == idCuenta, cancellationToken);
        if (cuenta is null)
        {
            return NotFound();
        }

        var esDeudora = cuenta.Naturaleza == NaturalezaCuenta.Deudora;

        var movimientosPrevios = await db.MovimientosComprobanteContable
            .Include(m => m.Comprobante)
            .Where(m => m.IdCuentaContable == idCuenta && m.Comprobante.Fecha < desde)
            .ToListAsync(cancellationToken);
        var saldoInicial = esDeudora
            ? movimientosPrevios.Sum(m => m.Debito - m.Credito)
            : movimientosPrevios.Sum(m => m.Credito - m.Debito);

        var movimientosRango = await db.MovimientosComprobanteContable
            .Include(m => m.Comprobante)
            .Where(m => m.IdCuentaContable == idCuenta && m.Comprobante.Fecha >= desde && m.Comprobante.Fecha <= hasta)
            .OrderBy(m => m.Comprobante.Fecha).ThenBy(m => m.Comprobante.Numero)
            .Select(m => new
            {
                m.Comprobante.Fecha,
                m.Comprobante.Numero,
                Descripcion = m.Descripcion ?? m.Comprobante.Descripcion ?? string.Empty,
                m.Debito,
                m.Credito,
            })
            .ToListAsync(cancellationToken);

        var saldoCorriente = saldoInicial;
        var lineas = new List<MayorAuxiliarLinea>();
        foreach (var m in movimientosRango)
        {
            saldoCorriente += esDeudora ? m.Debito - m.Credito : m.Credito - m.Debito;
            lineas.Add(new MayorAuxiliarLinea(m.Fecha, m.Numero, m.Descripcion, m.Debito, m.Credito, saldoCorriente));
        }

        return Ok(new MayorAuxiliarResult(
            cuenta.Codigo, cuenta.Nombre, desde, hasta, saldoInicial, lineas, saldoCorriente,
            lineas.Sum(l => l.Debito), lineas.Sum(l => l.Credito)));
    }

    [HttpGet("b11")]
    public async Task<ActionResult<EstadoFinancieroResult>> B11([FromQuery] DateOnly periodo, CancellationToken cancellationToken)
    {
        // Fecha de corte real = último día del mes reportado.
        var fechaCorte = new DateOnly(periodo.Year, periodo.Month, DateTime.DaysInMonth(periodo.Year, periodo.Month));
        return Ok(await GenerarEstadoFinancieroAsync("B11", fechaCorte, cancellationToken));
    }

    [HttpGet("b13")]
    public async Task<ActionResult<EstadoFinancieroResult>> B13(CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await GenerarEstadoFinancieroAsync("B13", hoy, cancellationToken));
    }
}
