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
/// obligatorios en Ecuador) ✅ hecho, ver <see cref="EmpleadoDecimoTercero"/>
/// y relacionados, y CLAUDE.md sección "Nómina — beneficios sociales
/// reales".
/// </summary>
public class Empleado
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public int IdCargo { get; set; }
    public Cargo Cargo { get; set; } = null!;

    public DateOnly FechaIngreso { get; set; }
    public bool RecibeFondosReserva { get; set; }
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

    /// <summary>
    /// Último sueldo mensual conocido — verificado contra Softbank que
    /// `NOMINA.EMPLEADO` NO guarda un sueldo persistente (se digita cada
    /// período en `ROLPAGOS_EMPLEADO.INGRESOS`, confirmado explícitamente
    /// en la sección "Nómina — beneficios sociales reales"). Este campo es
    /// una caché real y aditiva, no una réplica de estructura de
    /// Softbank: se sincroniza automáticamente cada vez que se genera un
    /// rol de pagos que incluye al empleado, y cuando se aprueba una
    /// Acción de Personal de cambio de cargo/sueldo — nunca se edita a
    /// mano por separado, para que nunca quede desincronizado de la
    /// fuente real (el rol de pagos).
    /// </summary>
    public decimal? SueldoActual { get; set; }
}
