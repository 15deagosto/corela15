namespace Corela15.Domain.Nomina;

public enum TipoRolPagos
{
    Mensual = 1,
    Quincenal = 2
}

public enum EstadoRolPagos
{
    Abierto = 1,
    Procesado = 2,
    Cerrado = 3
}

/// <summary>Rol de pagos mensual — verificado contra NOMINA.ROLPAGOS.</summary>
public class RolPagos
{
    public Guid Id { get; set; }

    /// <summary>Primer día del mes/quincena que cubre este rol.</summary>
    public DateOnly Periodo { get; set; }

    public TipoRolPagos Tipo { get; set; } = TipoRolPagos.Mensual;
    public EstadoRolPagos Estado { get; set; } = EstadoRolPagos.Abierto;

    public List<RolPagosEmpleado> Empleados { get; set; } = new();
}
