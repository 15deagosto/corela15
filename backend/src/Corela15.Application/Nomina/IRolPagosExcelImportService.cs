using Corela15.Application.Common;

namespace Corela15.Application.Nomina;

/// <summary>
/// Carga el rol de pagos tal cual el Excel real mensual de la cooperativa
/// ("Rol &lt;mes&gt; &lt;año&gt;.xlsx", hojas ADMINISTRATIVOS/
/// NEGOCIOS-OPERATIVOS/SERVICIOS PROFESIONALES) — pedido explícito del
/// usuario, "necesito esa matriz para poder cargar los roles". Hace match
/// por cédula (columna IDENTIFICACION) contra los empleados ya
/// sincronizados vía <see cref="IEmpleadoSyncService"/> — nunca crea un
/// empleado nuevo acá, si la cédula no existe se omite con aviso
/// explícito (sincronizar empleados primero es un paso real previo, no
/// opcional).
/// </summary>
public interface IRolPagosExcelImportService
{
    Task<ImportacionRolPagosResult> ImportarAsync(
        Stream archivoXlsx, string nombreHoja, DateOnly periodo, string tipo, string registradoPor,
        CancellationToken cancellationToken = default);
}

public record ImportacionRolPagosResult(
    Guid IdRolPagos, DateOnly Periodo, string Tipo, string Hoja,
    int FilasEnElExcel, int Cargados, int Omitidos, decimal TotalIngresos, decimal TotalEgresos,
    IReadOnlyList<string> Detalle);

public class HojaExcelInvalidaException(string hoja)
    : SolicitudInvalidaException($"No se pudo leer la hoja '{hoja}' del Excel — verificá el nombre exacto de la pestaña.");

public class ExcelSinFilasException()
    : SolicitudInvalidaException("No se encontró ninguna fila de empleado real en la hoja (se esperaba la columna IDENTIFICACION con cédulas reales debajo del encabezado).");
