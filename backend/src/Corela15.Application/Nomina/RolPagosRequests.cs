namespace Corela15.Application.Nomina;

public record LineaRolPagosRequest(Guid IdEmpleado, decimal Ingresos, decimal Egresos, int DiasLaborados);

public record GenerarRolPagosRequest(
    DateOnly Periodo,
    string Tipo,
    IReadOnlyList<LineaRolPagosRequest> Lineas);

public record RolPagosGeneradoResult(Guid IdRolPagos, DateOnly Periodo, string Tipo, decimal TotalGeneral);
