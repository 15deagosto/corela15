using Corela15.Application.Contabilidad;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IB11Service"/> — segunda excepción real, acotada, a la
/// regla de oro del proyecto, misma credencial de solo lectura
/// (<c>Softbank__ConnectionStringRo</c>) ya usada por
/// <see cref="ObligacionSyncService"/>.
/// </summary>
public class B11Service(Corela15DbContext db) : IB11Service
{
    // Mismas listas exactas ya verificadas y probadas en
    // ReportesController.GenerarEstadoFinancieroAsync (B11/B13 sobre el
    // ledger propio) — duplicadas acá a propósito (Application/
    // Infrastructure no pueden referenciar un controller de Api), nunca
    // reinventadas.
    private static readonly string[] GruposExcluidos = ["62", "63", "72", "73"];

    private static readonly HashSet<string> CuentasNegativasPermitidas =
    [
        "1399", "139905", "139910", "1499", "149905", "149910", "149915", "149920",
        "149940", "149945", "149950", "149955", "149980", "149985", "149987", "149989",
        "1699", "169905", "169910", "169915", "169920", "170599", "170699", "1799",
        "179910", "1899", "189905", "189910", "189915", "189920", "189925", "189930",
        "189940", "190499", "190599", "1999", "199905", "199910", "199990", "3602", "3604",
    ];

    private static bool EsElementoAcreedor(GrupoCuc elemento) =>
        elemento is GrupoCuc.Pasivo or GrupoCuc.Patrimonio or GrupoCuc.Ingresos;

    public async Task<B11Result> GenerarAsync(DateOnly fechaCorte, CancellationToken cancellationToken = default)
    {
        var connStringSoftbank = Environment.GetEnvironmentVariable("Softbank__ConnectionStringRo")
            ?? throw new InvalidOperationException(
                "Falta Softbank__ConnectionStringRo -- sin esta variable, el módulo de " +
                "Estructuras Financieras no puede generar B11 real desde Softbank (a propósito, " +
                "es la única conexión real de todo el backend a esa base, junto con OF01).");

        var empresa = await db.Empresas.FirstOrDefaultAsync(cancellationToken);
        var ruc = empresa?.Ruc ?? string.Empty;
        var advertencias = new List<string>();
        if (string.IsNullOrEmpty(ruc))
            advertencias.Add("La empresa no tiene RUC configurado (Configuración > Empresa) — campo obligatorio de la cabecera.");

        // Agencia real "CSD" (Consolidado) — ya suma las 3 agencias reales
        // (Pilacoto/San Silvestre/El Salto), verificado byte a byte contra
        // el archivo real de referencia. Usar ID=1 directo, nunca sumar
        // las 4 filas de GENERAL.AGENCIA (duplicaría todo).
        var saldosLeaf = new Dictionary<string, decimal>();
        await using (var conexion = new SqlConnection(connStringSoftbank))
        {
            await conexion.OpenAsync(cancellationToken);
            const string sql = """
                USE Softbank;
                SELECT s.CODIGOCUENTA, s.SALDOINICIAL, s.TOTALDEBITO, s.TOTALCREDITO
                FROM CONTABILIDAD.SALDOCONTABLE s
                JOIN GENERAL.AGENCIA a ON a.ID = s.IDAGENCIA
                WHERE a.CODIGO = 'CSD' AND CAST(s.FECHA AS DATE) = @fecha
                """;
            await using var cmd = new SqlCommand(sql, conexion);
            cmd.Parameters.AddWithValue("@fecha", fechaCorte.ToDateTime(TimeOnly.MinValue));
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var codigo = reader.GetString(0);
                var saldoInicial = reader.GetDecimal(1);
                var totalDebito = reader.GetDecimal(2);
                var totalCredito = reader.GetDecimal(3);
                saldosLeaf[codigo] = saldoInicial + totalDebito - totalCredito;
            }
        }

        if (saldosLeaf.Count == 0)
            advertencias.Add($"No se encontró ningún saldo real en Softbank (agencia CSD) para la fecha de corte {fechaCorte:dd/MM/yyyy} — verificar que exista cierre contable de ese día.");

        // Eliminación real de transferencias internas entre agencias
        // (cuentas 1908/2908 y TODOS sus descendientes reales —
        // 190801-190805/290801-290805, verificado en vivo) — confirmado
        // byte a byte contra el XML real de referencia: el saldo real es
        // no-cero (~$6.3M cada lado), pero SEPS exige reportarlo en 0.00
        // porque no es un activo/pasivo real frente al mundo externo.
        foreach (var codigo in saldosLeaf.Keys.Where(c => c.StartsWith("1908") || c.StartsWith("2908")).ToList())
            saldosLeaf[codigo] = 0m;

        var todasLasCuentas = await db.CuentasContables.Where(c => c.Activa).OrderBy(c => c.Codigo).ToListAsync(cancellationToken);
        var hijosPorPadre = todasLasCuentas.Where(c => c.IdCuentaPadre.HasValue)
            .GroupBy(c => c.IdCuentaPadre!.Value).ToDictionary(g => g.Key, g => g.ToList());
        var saldoCalculado = new Dictionary<Guid, decimal>();

        decimal CalcularSaldo(CuentaContable cuenta)
        {
            if (saldoCalculado.TryGetValue(cuenta.Id, out var yaCalculado)) return yaCalculado;

            var resultado = cuenta.EsMayor
                // La fórmula real (SALDOINICIAL+TOTALDEBITO-TOTALCREDITO) da
                // el saldo natural de una cuenta Deudora tal cual, pero el
                // elemento (dígito 1: Pasivo/Patrimonio/Ingresos) se
                // reporta siempre en positivo en el archivo real —
                // verificado byte a byte (PASIVO real = 17.126.447,96, no
                // -17.126.447,96). **El signo se decide por el ELEMENTO
                // (cuenta.Grupo), nunca por la naturaleza de la hoja
                // individual**: una cuenta contra-activo real (ej. 1499
                // Provisión, naturaleza Acreedora pero elemento Activo) NO
                // se invierte — sigue restando dentro de Activo, tal como
                // ya lo hacía la fórmula cruda (confirmado: invertir por
                // naturaleza de hoja rompía el total real de ACTIVO al
                // duplicar en positivo las provisiones/depreciaciones).
                ? saldosLeaf.GetValueOrDefault(cuenta.Codigo, 0m) * (EsElementoAcreedor(cuenta.Grupo) ? -1m : 1m)
                : hijosPorPadre.GetValueOrDefault(cuenta.Id, []).Sum(CalcularSaldo);

            saldoCalculado[cuenta.Id] = resultado;
            return resultado;
        }

        var detalle = new List<B11CuentaDetalle>();
        foreach (var cuenta in todasLasCuentas)
        {
            if (GruposExcluidos.Any(g => cuenta.Codigo.StartsWith(g))) continue;

            var saldo = CalcularSaldo(cuenta);

            if (cuenta.EsMayor)
            {
                var puedeSerNegativo = cuenta.Grupo == GrupoCuc.Patrimonio
                    || cuenta.Codigo.StartsWith("35") || cuenta.Codigo.StartsWith("36")
                    || cuenta.Codigo is "3502" or "3504"
                    || CuentasNegativasPermitidas.Contains(cuenta.Codigo);

                if (saldo < 0 && !puedeSerNegativo)
                    advertencias.Add($"Cuenta {cuenta.Codigo} ({cuenta.Nombre}) tiene saldo negativo real ({saldo:0.00}) pero el manual no la autoriza a reportarse en negativo.");
            }

            detalle.Add(new B11CuentaDetalle(cuenta.Codigo, cuenta.Nombre, saldo));
        }

        var codigosSoftbankSinCatalogo = saldosLeaf.Keys.Except(todasLasCuentas.Select(c => c.Codigo)).ToList();
        if (codigosSoftbankSinCatalogo.Count > 0)
            advertencias.Add($"{codigosSoftbankSinCatalogo.Count} cuenta(s) con saldo real en Softbank no existen en el catálogo CUC sembrado acá: {string.Join(", ", codigosSoftbankSinCatalogo.Take(10))}{(codigosSoftbankSinCatalogo.Count > 10 ? "…" : "")}.");

        var valorCuadre = detalle.Sum(d => d.Total);

        const int registrosEsperadosCoac = 1192;
        if (detalle.Count != registrosEsperadosCoac)
            advertencias.Add(
                $"Número de registros ({detalle.Count}) no coincide exacto con el esperado oficial para COAC " +
                $"({registrosEsperadosCoac}, Manual Técnico v10.0) — diferencia de {Math.Abs(detalle.Count - registrosEsperadosCoac)}. " +
                "El código '671' del catálogo oficial (3 dígitos, no encaja en la jerarquía elemento/grupo/cuenta/subcuenta) " +
                "se excluyó de la siembra en vez de adivinar su posición — ver CLAUDE.md.");

        return new B11Result("B11", ruc, fechaCorte, detalle.Count, valorCuadre, detalle, advertencias);
    }
}
