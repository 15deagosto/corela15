using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.Nomina;

public enum EstadoEmpleado
{
    Activo = 1,
    Vacaciones = 2,
    LicenciaSinSueldo = 3,
    Desvinculado = 4
}

/// <summary>
/// Empleado de la cooperativa (no socio) — verificado contra NOMINA.EMPLEADO.
/// Décimos, fondos de reserva y provisión de vacaciones
/// (EMPLEADO_DECIMOTERCERO/_DECIMOCUARTO/_FONDOSRESERVA/_PROVISION_VACACION,
/// obligatorios en Ecuador) quedan fuera de alcance inicial: se agregan
/// cuando se construya el cálculo real de rol de pagos.
/// </summary>
public class Empleado
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public string Cargo { get; set; } = string.Empty;
    public DateOnly FechaIngreso { get; set; }
    public bool RecibeFondosReserva { get; set; }
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;
}
