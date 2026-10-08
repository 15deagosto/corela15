using System.Globalization;
using Corela15.Application.Nomina;
using Corela15.Domain.Nomina;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Xlsx;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IRolPagosExcelImportService"/>. Las columnas de abajo
/// (A..AR) se verificaron una vez contra el formato real de la
/// cooperativa ("Rol Julio 2026.xlsx", hoja ADMINISTRATIVOS) -- las 3
/// hojas reales (ADMINISTRATIVOS/NEGOCIOS-OPERATIVOS/SERVICIOS
/// PROFESIONALES) comparten el mismo membrete/diseño institucional, así
/// que se asume la misma posición de columna en las 3. Si un mes futuro
/// cambia el layout, hay que volver a verificar acá -- documentado a
/// propósito, no una garantía permanente.
/// </summary>
public class RolPagosExcelImportService(Corela15DbContext db) : IRolPagosExcelImportService
{
    // Índices de columna 0-based (A=0, B=1, ... AA=26, AB=27...)
    private const int ColIdentificacion = 1;
    private const int ColDiasLab = 7;
    private const int ColSueldoAfiliado = 8;
    private const int ColSueldoProporcional = 9;
    private const int ColHSup50 = 10;
    private const int ColHSup100 = 11;
    private const int ColHExtra50 = 12;
    private const int ColHExtra100 = 13;
    private const int ColMovilizacion = 15;
    private const int ColBonificaciones = 16;
    private const int ColComponenteSalarial = 17;
    private const int ColComisiones = 18;
    private const int ColFrAcumulaFlag = 19;
    private const int ColFrValorAcumulado = 21;
    private const int ColFrValorMensual = 22;
    private const int ColD13AcumulaFlag = 23;
    private const int ColD13ValorAcumulado = 25;
    private const int ColD13ValorMensual = 26;
    private const int ColD14AcumulaFlag = 27;
    private const int ColD14ValorAcumulado = 29;
    private const int ColD14ValorMensual = 30;
    private const int ColTotalIngresos = 31;
    private const int ColAporteIndividualIess = 32;
    private const int ColSanciones = 33;
    private const int ColAtrasos = 34;
    private const int ColDescuentos = 35;
    private const int ColAnticipoSueldo = 36;
    private const int ColSubsidioIess = 37;
    private const int ColActasFiniquito = 38;
    private const int ColQuirografario = 39;
    private const int ColHipotecario = 40;
    private const int ColRetencionRenta = 41;
    private const int ColTotalEgresos = 42;
    private const int ColLiquidoARecibir = 43;

    private static decimal Dec(Dictionary<int, string> fila, int col)
        => fila.TryGetValue(col, out var v) && decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;

    private static bool EsSi(Dictionary<int, string> fila, int col)
        => fila.TryGetValue(col, out var v) && v.Trim().Equals("SI", StringComparison.OrdinalIgnoreCase);

    public async Task<ImportacionRolPagosResult> ImportarAsync(
        Stream archivoXlsx, string nombreHoja, DateOnly periodo, string tipo, string registradoPor,
        CancellationToken cancellationToken = default)
    {
        List<Dictionary<int, string>> filas;
        try
        {
            filas = XlsxReader.LeerHoja(archivoXlsx, nombreHoja);
        }
        catch (Exception)
        {
            throw new HojaExcelInvalidaException(nombreHoja);
        }

        // Fila de encabezado real: la que tiene "IDENTIFICACION" en la
        // columna B -- se busca en vez de asumir un número de fila fijo,
        // porque el membrete de arriba (decimocuarto/mes/año) puede
        // desplazarse mes a mes.
        var filaEncabezado = filas.FindIndex(f =>
            f.TryGetValue(ColIdentificacion, out var v) && v.Trim().Equals("IDENTIFICACION", StringComparison.OrdinalIgnoreCase));
        if (filaEncabezado < 0)
        {
            throw new ExcelSinFilasException();
        }

        var filasEmpleado = new List<Dictionary<int, string>>();
        for (var i = filaEncabezado + 1; i < filas.Count; i++)
        {
            var cedula = filas[i].GetValueOrDefault(ColIdentificacion, "").Trim();
            // Fin de la lista de empleados: la fila de TOTALES/firmas ya no
            // trae una cédula real (solo dígitos) en esta columna.
            if (cedula.Length == 0 || !cedula.All(char.IsDigit)) break;
            filasEmpleado.Add(filas[i]);
        }
        if (filasEmpleado.Count == 0)
        {
            throw new ExcelSinFilasException();
        }

        if (!Enum.TryParse<TipoRolPagos>(tipo, ignoreCase: true, out var tipoEnum))
        {
            throw new TipoRolPagosInvalidoException(tipo);
        }

        var rolPagos = await db.RolesPagos.FirstOrDefaultAsync(
            r => r.Periodo == periodo && r.Tipo == tipoEnum, cancellationToken);
        if (rolPagos is null)
        {
            rolPagos = new RolPagos { Id = Guid.NewGuid(), Periodo = periodo, Tipo = tipoEnum, Estado = EstadoRolPagos.Abierto };
            db.RolesPagos.Add(rolPagos);
            await db.SaveChangesAsync(cancellationToken);
        }

        var detalle = new List<string>();
        var cargados = 0;
        var omitidos = 0;
        decimal totalIngresos = 0;
        decimal totalEgresos = 0;

        foreach (var fila in filasEmpleado)
        {
            var cedula = fila[ColIdentificacion].Trim();
            var empleado = await db.Empleados
                .Include(e => e.Persona)
                .FirstOrDefaultAsync(e => e.Persona.Identificacion == cedula, cancellationToken);

            if (empleado is null)
            {
                detalle.Add($"Cédula {cedula}: no existe como empleado sincronizado, omitida (sincronizá empleados primero)");
                omitidos++;
                continue;
            }

            var ingresos = Dec(fila, ColTotalIngresos);
            var egresos = Dec(fila, ColTotalEgresos);
            var liquido = Dec(fila, ColLiquidoARecibir);

            var linea = await db.RolesPagosEmpleado.FirstOrDefaultAsync(
                r => r.IdRolPagos == rolPagos.Id && r.IdEmpleado == empleado.Id, cancellationToken);
            if (linea is null)
            {
                linea = new RolPagosEmpleado { Id = Guid.NewGuid(), IdRolPagos = rolPagos.Id, IdEmpleado = empleado.Id };
                db.RolesPagosEmpleado.Add(linea);
            }

            linea.DiasLaborados = (int)Dec(fila, ColDiasLab);
            linea.SueldoAfiliado = Dec(fila, ColSueldoAfiliado);
            linea.SueldoProporcional = Dec(fila, ColSueldoProporcional);
            linea.HorasSuplementarias50 = Dec(fila, ColHSup50);
            linea.HorasSuplementarias100 = Dec(fila, ColHSup100);
            linea.HorasExtraordinarias50 = Dec(fila, ColHExtra50);
            linea.HorasExtraordinarias100 = Dec(fila, ColHExtra100);
            linea.Movilizacion = Dec(fila, ColMovilizacion);
            linea.Bonificaciones = Dec(fila, ColBonificaciones);
            linea.ComponenteSalarial = Dec(fila, ColComponenteSalarial);
            linea.Comisiones = Dec(fila, ColComisiones);
            linea.FondosReservaAcumula = EsSi(fila, ColFrAcumulaFlag);
            linea.FondosReservaValorAcumulado = Dec(fila, ColFrValorAcumulado);
            linea.FondosReservaValorMensual = Dec(fila, ColFrValorMensual);
            linea.DecimoTerceroAcumula = EsSi(fila, ColD13AcumulaFlag);
            linea.DecimoTerceroValorAcumulado = Dec(fila, ColD13ValorAcumulado);
            linea.DecimoTerceroValorMensual = Dec(fila, ColD13ValorMensual);
            linea.DecimoCuartoAcumula = EsSi(fila, ColD14AcumulaFlag);
            linea.DecimoCuartoValorAcumulado = Dec(fila, ColD14ValorAcumulado);
            linea.DecimoCuartoValorMensual = Dec(fila, ColD14ValorMensual);
            linea.AporteIndividualIess = Dec(fila, ColAporteIndividualIess);
            linea.Sanciones = Dec(fila, ColSanciones);
            linea.Atrasos = Dec(fila, ColAtrasos);
            linea.Descuentos = Dec(fila, ColDescuentos);
            linea.AnticipoSueldo = Dec(fila, ColAnticipoSueldo);
            linea.SubsidioIess = Dec(fila, ColSubsidioIess);
            linea.ActasFiniquito = Dec(fila, ColActasFiniquito);
            linea.PrestamoQuirografario = Dec(fila, ColQuirografario);
            linea.PrestamoHipotecario = Dec(fila, ColHipotecario);
            linea.RetencionRenta = Dec(fila, ColRetencionRenta);
            linea.Ingresos = ingresos;
            linea.Egresos = egresos;
            linea.Total = liquido;
            linea.Anulado = false;

            // Mismo criterio ya documentado en Empleado.SueldoActual: se
            // sincroniza solo, nunca a mano, cada vez que el empleado
            // aparece en un rol real -- acá el rol real es este Excel.
            empleado.SueldoActual = Dec(fila, ColSueldoAfiliado);

            totalIngresos += ingresos;
            totalEgresos += egresos;
            cargados++;
            detalle.Add($"Cédula {cedula} ({empleado.Persona.Nombre}): cargada -- ingresos {ingresos:C}, egresos {egresos:C}");
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ImportacionRolPagosResult(
            rolPagos.Id, periodo, tipo, nombreHoja, filasEmpleado.Count, cargados, omitidos,
            totalIngresos, totalEgresos, detalle);
    }
}
