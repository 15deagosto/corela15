using Corela15.Application.Nomina;
using Corela15.Domain.Nomina;
using Corela15.Domain.Sujeto;
using Corela15.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IEmpleadoSyncService"/> para el alcance y la credencial
/// nominal real que usa. Trae TODOS los colaboradores reales de
/// NOMINA.EMPLEADO (activos e inactivos, "tal cual" pidió el usuario — un
/// colaborador desvinculado sigue siendo parte del historial real, no se
/// descarta), mapeando IDCARGO/agencia directo porque tanto `nomina.cargo`
/// (90 filas reales, IDs preservados) como `general.agencia` (IDs
/// preservados) ya fueron sembrados 1:1 desde Softbank en rondas
/// anteriores -- nunca se inventa un cargo o agencia nuevos acá.
/// </summary>
public class EmpleadoSyncService(Corela15DbContext db) : IEmpleadoSyncService
{
    public async Task<SincronizacionEmpleadosResult> SincronizarAsync(CancellationToken cancellationToken = default)
    {
        var connStringNominal = Environment.GetEnvironmentVariable("Softbank__ConnectionStringNominal")
            ?? throw new InvalidOperationException(
                "Falta Softbank__ConnectionStringNominal -- sin esta variable, la sincronización de " +
                "colaboradores reales no puede leer nombre/identificación (columnas restringidas por " +
                "permiso de columna en Softbank, requieren la credencial nominal, separada de la de " +
                "solo lectura general).");

        var detalle = new List<string>();
        var creados = 0;
        var actualizados = 0;
        var omitidos = 0;

        await using var conexion = new SqlConnection(connStringNominal);
        await conexion.OpenAsync(cancellationToken);

        const string sql = """
            USE Softbank;
            SELECT
                e.ID AS IdEmpleadoSoftbank, e.IDCARGO, e.CODIGOESTADOEMPLEADO, e.RECIBEFONDOSRESERVA,
                e.FECHARECLUTAMIENTO, e.EMAIL,
                ad.IDAGENCIA,
                p.ID AS IdPersonaSoftbank, p.IDENTIFICACION, p.NOMBRE,
                pn.PRIMERNOMBRE, pn.SEGUNDONOMBRE, pn.APELLIDOPATERNO, pn.APELLIDOMATERNO,
                pn.ESMASCULINO, pn.FECHANACIMIENTO,
                ia.PAGODECIMOMENSUAL, ia.PAGOFONDOSDERESERVAROL,
                iess.CODIGOIESS, iess.FECHAINGRESO AS FechaIngresoIess, iess.FECHASALIDA AS FechaSalidaIess
            FROM NOMINA.EMPLEADO e
            JOIN SUJETO.PERSONA_NATURAL pn ON pn.IDPERSONA = e.IDPERSONANATURAL
            JOIN SUJETO.PERSONA p ON p.ID = pn.IDPERSONA
            LEFT JOIN GENERAL.AGENCIA_DEPARTAMENTO ad ON ad.ID = e.IDAGENCIADEPARTAMENTO
            LEFT JOIN NOMINA.EMPLEADO_INFORMACIONADICIONAL ia ON ia.IDEMPLEADO = e.ID
            LEFT JOIN NOMINA.EMPLEADO_DATOS_IESS iess ON iess.IDEMPLEADO = e.ID
            ORDER BY e.ID
            """;

        var filas = new List<Dictionary<string, object?>>();
        await using (var cmd = new SqlCommand(sql, conexion))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var fila = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                    fila[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                filas.Add(fila);
            }
        }

        var cargosValidos = (await db.Cargos.Where(c => c.Activo).Select(c => c.Id).ToListAsync(cancellationToken)).ToHashSet();
        var agenciasValidas = (await db.Agencias.Where(a => a.Activa).Select(a => a.Id).ToListAsync(cancellationToken)).ToHashSet();

        foreach (var f in filas)
        {
            var idEmpleadoSoftbank = Convert.ToInt32(f["IdEmpleadoSoftbank"]);
            var identificacion = ((string?)f["IDENTIFICACION"])?.Trim();
            var idCargo = (int?)f["IDCARGO"];
            var idAgencia = (int?)f["IDAGENCIA"];

            if (string.IsNullOrWhiteSpace(identificacion))
            {
                detalle.Add($"Empleado #{idEmpleadoSoftbank}: sin identificación real, omitido");
                omitidos++;
                continue;
            }
            if (idCargo is null || !cargosValidos.Contains(idCargo.Value))
            {
                detalle.Add($"Empleado #{idEmpleadoSoftbank} ({identificacion}): cargo {idCargo} no existe/inactivo en el catálogo, omitido");
                omitidos++;
                continue;
            }
            if (idAgencia is null || !agenciasValidas.Contains(idAgencia.Value))
            {
                detalle.Add($"Empleado #{idEmpleadoSoftbank} ({identificacion}): agencia {idAgencia} no existe/inactiva en el catálogo, omitido");
                omitidos++;
                continue;
            }

            var persona = await db.Personas.FirstOrDefaultAsync(
                p => p.IdTipoIdentificacion == 1 && p.Identificacion == identificacion, cancellationToken);

            if (persona is null)
            {
                var nombre = ((string?)f["NOMBRE"])?.Trim() ?? identificacion;
                persona = new Persona
                {
                    Id = Guid.NewGuid(),
                    Identificacion = identificacion,
                    IdTipoIdentificacion = 1, // Cédula
                    Nombre = nombre,
                    Email = (string?)f["EMAIL"],
                    CreadoEn = DateTimeOffset.UtcNow,
                    CreadoPor = "sync:softbank:nomina",
                };
                db.Personas.Add(persona);
                db.PersonasNaturales.Add(new PersonaNatural
                {
                    IdPersona = persona.Id,
                    PrimerNombre = (string?)f["PRIMERNOMBRE"] ?? "",
                    SegundoNombre = (string?)f["SEGUNDONOMBRE"],
                    ApellidoPaterno = (string?)f["APELLIDOPATERNO"] ?? "",
                    ApellidoMaterno = (string?)f["APELLIDOMATERNO"],
                    FechaNacimiento = (DateTime?)f["FECHANACIMIENTO"] is DateTime fn ? DateOnly.FromDateTime(fn) : new DateOnly(1900, 1, 1),
                    EsMasculino = (bool?)f["ESMASCULINO"] ?? true,
                });
            }

            // Match real por PERSONA, no por el ID de empleado de Softbank --
            // una misma persona puede tener más de una fila real en
            // NOMINA.EMPLEADO (recontratación: salió y volvió a entrar,
            // Softbank nunca reutiliza el mismo EMPLEADO.ID). Corela15 no
            // replica ese historial de reingresos como filas separadas
            // (mismo criterio de "simplificación de esquema, no de dato" ya
            // aplicado antes) -- un solo Empleado por persona, que termina
            // reflejando su fila real más reciente (la consulta ya viene
            // ordenada por ID ascendente).
            var empleado = await db.Empleados.FirstOrDefaultAsync(
                e => e.IdPersona == persona.Id, cancellationToken);

            var estadoReal = (string?)f["CODIGOESTADOEMPLEADO"] == "A" ? EstadoEmpleado.Activo : EstadoEmpleado.Desvinculado;
            var recibeFondosReserva = (bool?)f["RECIBEFONDOSRESERVA"] ?? false;
            var fechaIngreso = (DateTime?)f["FECHARECLUTAMIENTO"] is DateTime fr ? DateOnly.FromDateTime(fr) : DateOnly.FromDateTime(DateTime.Today);

            if (empleado is null)
            {
                empleado = new Empleado
                {
                    Id = Guid.NewGuid(),
                    IdPersona = persona.Id,
                    CodigoEmpleadoSoftbank = idEmpleadoSoftbank,
                    IdAgencia = idAgencia.Value,
                    IdCargo = idCargo.Value,
                    FechaIngreso = fechaIngreso,
                    RecibeFondosReserva = recibeFondosReserva,
                    Estado = estadoReal,
                };
                db.Empleados.Add(empleado);
                creados++;
                detalle.Add($"Empleado #{idEmpleadoSoftbank} ({identificacion}, {persona.Nombre}): nuevo");
            }
            else
            {
                empleado.CodigoEmpleadoSoftbank = idEmpleadoSoftbank;
                empleado.IdAgencia = idAgencia.Value;
                empleado.IdCargo = idCargo.Value;
                empleado.FechaIngreso = fechaIngreso;
                empleado.RecibeFondosReserva = recibeFondosReserva;
                empleado.Estado = estadoReal;
                actualizados++;
                detalle.Add($"Empleado #{idEmpleadoSoftbank} ({identificacion}, {persona.Nombre}): actualizado -> {estadoReal}");
            }

            var datosAdicionales = await db.EmpleadosDatosAdicionales.FirstOrDefaultAsync(
                d => d.IdEmpleado == empleado.Id, cancellationToken);
            if (datosAdicionales is null)
            {
                datosAdicionales = new EmpleadoDatosAdicionales { IdEmpleado = empleado.Id };
                db.EmpleadosDatosAdicionales.Add(datosAdicionales);
            }
            datosAdicionales.PagoDecimoMensual = (bool?)f["PAGODECIMOMENSUAL"] ?? false;
            datosAdicionales.PagoFondosReservaRol = (bool?)f["PAGOFONDOSDERESERVAROL"] ?? false;
            datosAdicionales.CodigoIess = (string?)f["CODIGOIESS"];
            datosAdicionales.FechaIngresoIess = (DateTime?)f["FechaIngresoIess"] is DateTime fii ? DateOnly.FromDateTime(fii) : null;
            datosAdicionales.FechaSalidaIess = (DateTime?)f["FechaSalidaIess"] is DateTime fsi ? DateOnly.FromDateTime(fsi) : null;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SincronizacionEmpleadosResult(filas.Count, creados, actualizados, omitidos, detalle);
    }
}
