namespace Corela15.Domain.Nomina;

/// <summary>Rol de pagos de un empleado en particular — verificado contra NOMINA.ROLPAGOS_EMPLEADO.</summary>
public class RolPagosEmpleado
{
    public Guid Id { get; set; }

    public Guid IdRolPagos { get; set; }
    public RolPagos RolPagos { get; set; } = null!;

    public Guid IdEmpleado { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public decimal Ingresos { get; set; }
    public decimal Egresos { get; set; }
    public decimal Total { get; set; }
    public int DiasLaborados { get; set; }
    public bool Anulado { get; set; }
}
