namespace Corela15.Application.Nomina;

/// <summary>
/// Tercera excepción real y acotada a la "regla de oro" del proyecto (el
/// backend nunca se conecta a Softbank salvo estas excepciones explícitas
/// ya documentadas: <see cref="Corela15.Application.Obligacion.
/// IObligacionSyncService"/> para OF01, <c>B11Service</c> para el balance)
/// -- acá para traer los colaboradores reales (nombre, cédula, cargo,
/// agencia, sueldo, y si acumulan fondos de reserva/décimos) desde
/// NOMINA.EMPLEADO + SUJETO.PERSONA/PERSONA_NATURAL, pedido explícito del
/// usuario: "vinculado con los sueldos, nombres, datos y todos los
/// colaboradores que tenemos en Softbank tal cual".
///
/// Como nombre/cédula son columnas restringidas por permiso de columna en
/// Softbank (mismo hallazgo ya documentado para `cartera_nominal`), esta
/// sincronización usa su PROPIA cadena de conexión nominal
/// (`Softbank__ConnectionStringNominal`), separada de
/// `Softbank__ConnectionStringRo` que usa el resto de sincronizaciones --
/// nunca se usa esa credencial nominal para otra cosa.
///
/// Pensado para correrse repetidas veces (antes de cargar el rol de cada
/// mes, por ejemplo) -- es upsert real por `Empleado.CodigoEmpleadoSoftbank`,
/// nunca duplica a un colaborador ya sincronizado.
/// </summary>
public interface IEmpleadoSyncService
{
    Task<SincronizacionEmpleadosResult> SincronizarAsync(CancellationToken cancellationToken = default);
}

public record SincronizacionEmpleadosResult(
    int EncontradosEnSoftbank,
    int Creados,
    int Actualizados,
    int Omitidos,
    IReadOnlyList<string> Detalle);
