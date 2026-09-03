using Corela15.Application.Nomina;
using Corela15.Domain.Nomina;
using Corela15.Domain.Sujeto;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class EmpleadoService(Corela15DbContext db) : IEmpleadoService
{
    public async Task<EmpleadoCreadoResult> CrearAsync(
        CrearEmpleadoRequest request, string registradoPor, CancellationToken cancellationToken = default)
    {
        var agenciaValida = await db.Agencias.AnyAsync(a => a.Id == request.IdAgencia && a.Activa, cancellationToken);
        if (!agenciaValida)
        {
            throw new AgenciaInvalidaParaEmpleadoException(request.IdAgencia);
        }

        var cargoValido = await db.Cargos.AnyAsync(c => c.Id == request.IdCargo && c.Activo, cancellationToken);
        if (!cargoValido)
        {
            throw new CargoInvalidoException(request.IdCargo);
        }

        Persona persona;

        if (request.IdPersona is Guid idPersonaExistente)
        {
            var personaExistente = await db.Personas.FirstOrDefaultAsync(p => p.Id == idPersonaExistente, cancellationToken);
            if (personaExistente is null)
            {
                throw new PersonaInvalidaParaEmpleadoException(idPersonaExistente);
            }

            var yaEsEmpleado = await db.Empleados.AnyAsync(e => e.IdPersona == idPersonaExistente, cancellationToken);
            if (yaEsEmpleado)
            {
                throw new PersonaYaEsEmpleadoException(idPersonaExistente);
            }

            persona = personaExistente;
        }
        else
        {
            if (request.IdTipoIdentificacion is null || string.IsNullOrWhiteSpace(request.Identificacion)
                || string.IsNullOrWhiteSpace(request.PrimerNombre) || string.IsNullOrWhiteSpace(request.ApellidoPaterno)
                || request.FechaNacimiento is null)
            {
                throw new DatosPersonaEmpleadoIncompletosException();
            }

            var tipoIdentificacionValido = await db.TiposIdentificacion.AnyAsync(t => t.Id == request.IdTipoIdentificacion, cancellationToken);
            if (!tipoIdentificacionValido)
            {
                throw new TipoIdentificacionInvalidoParaEmpleadoException(request.IdTipoIdentificacion.Value);
            }

            var identificacionDuplicada = await db.Personas.AnyAsync(
                p => p.IdTipoIdentificacion == request.IdTipoIdentificacion && p.Identificacion == request.Identificacion,
                cancellationToken);
            if (identificacionDuplicada)
            {
                throw new IdentificacionDuplicadaParaEmpleadoException(request.Identificacion);
            }

            var nombreCompleto = string.Join(' ', new[]
            {
                request.PrimerNombre, request.SegundoNombre, request.ApellidoPaterno, request.ApellidoMaterno,
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            persona = new Persona
            {
                Id = Guid.NewGuid(),
                Identificacion = request.Identificacion,
                IdTipoIdentificacion = request.IdTipoIdentificacion.Value,
                Nombre = nombreCompleto,
                Email = request.Email,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = registradoPor,
            };

            db.Personas.Add(persona);
            db.PersonasNaturales.Add(new PersonaNatural
            {
                IdPersona = persona.Id,
                PrimerNombre = request.PrimerNombre,
                SegundoNombre = request.SegundoNombre,
                ApellidoPaterno = request.ApellidoPaterno,
                ApellidoMaterno = request.ApellidoMaterno,
                FechaNacimiento = request.FechaNacimiento.Value,
                EsMasculino = request.EsMasculino ?? true,
            });
        }

        var empleado = new Empleado
        {
            Id = Guid.NewGuid(),
            IdPersona = persona.Id,
            IdAgencia = request.IdAgencia,
            IdCargo = request.IdCargo,
            FechaIngreso = request.FechaIngreso,
            RecibeFondosReserva = request.RecibeFondosReserva,
            Estado = EstadoEmpleado.Activo,
            SueldoActual = request.SueldoInicial,
        };

        db.Empleados.Add(empleado);
        db.EmpleadosDatosAdicionales.Add(new EmpleadoDatosAdicionales { IdEmpleado = empleado.Id });
        await db.SaveChangesAsync(cancellationToken);

        return new EmpleadoCreadoResult(empleado.Id, persona.Id, persona.Nombre);
    }

    public async Task ActualizarAsync(
        Guid idEmpleado, ActualizarEmpleadoRequest request, CancellationToken cancellationToken = default)
    {
        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == idEmpleado, cancellationToken);
        if (empleado is null)
        {
            throw new EmpleadoInexistenteException(idEmpleado);
        }

        var agenciaValida = await db.Agencias.AnyAsync(a => a.Id == request.IdAgencia && a.Activa, cancellationToken);
        if (!agenciaValida)
        {
            throw new AgenciaInvalidaParaEmpleadoException(request.IdAgencia);
        }

        var cargoValido = await db.Cargos.AnyAsync(c => c.Id == request.IdCargo && c.Activo, cancellationToken);
        if (!cargoValido)
        {
            throw new CargoInvalidoException(request.IdCargo);
        }

        if (!Enum.TryParse<EstadoEmpleado>(request.Estado, ignoreCase: true, out var estado))
        {
            throw new EstadoEmpleadoInvalidoException(request.Estado);
        }

        empleado.IdAgencia = request.IdAgencia;
        empleado.IdCargo = request.IdCargo;
        empleado.RecibeFondosReserva = request.RecibeFondosReserva;
        empleado.Estado = estado;

        await db.SaveChangesAsync(cancellationToken);
    }
}
