using Corela15.Application.Nomina;
using Corela15.Domain.Nomina;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class SolicitudAccionPersonalService(Corela15DbContext db) : ISolicitudAccionPersonalService
{
    private const string EstadoIngresada = "IN";
    private const string EstadoAprobada = "AP";
    private const string EstadoAnulada = "AN";

    public async Task<SolicitudAccionPersonalCreadaResult> CrearAsync(
        CrearSolicitudAccionPersonalRequest request, string registradoPor, CancellationToken cancellationToken = default)
    {
        var empleado = await db.Empleados.FirstOrDefaultAsync(e => e.Id == request.IdEmpleado, cancellationToken);
        if (empleado is null)
        {
            throw new EmpleadoInvalidoParaAccionException(request.IdEmpleado);
        }

        var tipo = await db.TiposAccionPersonal
            .FirstOrDefaultAsync(t => t.Id == request.IdTipoAccionPersonal && t.Activo, cancellationToken);
        if (tipo is null)
        {
            throw new TipoAccionPersonalInvalidoException(request.IdTipoAccionPersonal);
        }

        if (tipo.EsCambioCargoSueldo)
        {
            if (request.IdCargoNuevo is null)
            {
                throw new DatosAccionPersonalIncompletosException("Este tipo de acción requiere el cargo nuevo.");
            }

            var cargoValido = await db.Cargos.AnyAsync(c => c.Id == request.IdCargoNuevo && c.Activo, cancellationToken);
            if (!cargoValido)
            {
                throw new DatosAccionPersonalIncompletosException($"El cargo {request.IdCargoNuevo} no existe o no está activo.");
            }
        }

        if (tipo.EsCambioAgenciaDepartamento)
        {
            if (request.IdAgenciaNueva is null)
            {
                throw new DatosAccionPersonalIncompletosException("Este tipo de acción requiere la agencia nueva.");
            }

            var agenciaValida = await db.Agencias.AnyAsync(a => a.Id == request.IdAgenciaNueva && a.Activa, cancellationToken);
            if (!agenciaValida)
            {
                throw new DatosAccionPersonalIncompletosException($"La agencia {request.IdAgenciaNueva} no existe o no está activa.");
            }
        }

        if (tipo.EsFormaPagoFondosReserva && request.NuevoValorRecibeFondosReserva is null)
        {
            throw new DatosAccionPersonalIncompletosException("Este tipo de acción requiere el nuevo valor de fondos de reserva.");
        }

        var siguienteNumero = await db.SolicitudesAccionPersonal.CountAsync(cancellationToken) + 1;

        var usuarioId = await db.Usuarios
            .Where(u => u.NombreUsuario == registradoPor)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var solicitud = new SolicitudAccionPersonal
        {
            Id = Guid.NewGuid(),
            NumeroAccion = siguienteNumero,
            IdEmpleado = request.IdEmpleado,
            IdTipoAccionPersonal = request.IdTipoAccionPersonal,
            Detalle = request.Detalle,
            CodigoEstado = EstadoIngresada,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow),
            IdUsuario = usuarioId ?? Guid.Empty,
            IdCargoNuevo = request.IdCargoNuevo,
            NuevoSueldo = request.NuevoSueldo,
            IdAgenciaNueva = request.IdAgenciaNueva,
            NuevoValorRecibeFondosReserva = request.NuevoValorRecibeFondosReserva,
        };

        solicitud.Etapas.Add(new SolicitudAccionPersonalEtapa
        {
            Id = Guid.NewGuid(),
            IdSolicitudAccionPersonal = solicitud.Id,
            CodigoEstado = EstadoIngresada,
            Comentario = "Solicitud ingresada",
            Fecha = DateTimeOffset.UtcNow,
            RegistradoPor = registradoPor,
        });

        db.SolicitudesAccionPersonal.Add(solicitud);
        await db.SaveChangesAsync(cancellationToken);

        return new SolicitudAccionPersonalCreadaResult(solicitud.Id, solicitud.NumeroAccion);
    }

    public async Task AprobarAsync(
        Guid id, string? comentario, string registradoPor, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesAccionPersonal
            .Include(s => s.TipoAccionPersonal)
            .Include(s => s.Empleado)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (solicitud is null)
        {
            throw new SolicitudAccionPersonalInexistenteException(id);
        }

        if (solicitud.CodigoEstado != EstadoIngresada)
        {
            throw new SolicitudAccionPersonalNoIngresadaException(id);
        }

        var tipo = solicitud.TipoAccionPersonal;
        var empleado = solicitud.Empleado;

        if (tipo.ActivaContrato) empleado.Estado = EstadoEmpleado.Activo;
        if (tipo.DesactivaContrato) empleado.Estado = EstadoEmpleado.Desvinculado;
        if (tipo.EsCambioCargoSueldo && solicitud.IdCargoNuevo is int idCargo) empleado.IdCargo = idCargo;
        if (tipo.EsCambioCargoSueldo && solicitud.NuevoSueldo is decimal nuevoSueldo) empleado.SueldoActual = nuevoSueldo;
        if (tipo.EsCambioAgenciaDepartamento && solicitud.IdAgenciaNueva is int idAgencia) empleado.IdAgencia = idAgencia;
        if (tipo.EsFormaPagoFondosReserva && solicitud.NuevoValorRecibeFondosReserva is bool nuevoValor)
        {
            empleado.RecibeFondosReserva = nuevoValor;
        }

        solicitud.CodigoEstado = EstadoAprobada;
        db.SolicitudAccionPersonalEtapas.Add(new SolicitudAccionPersonalEtapa
        {
            Id = Guid.NewGuid(),
            IdSolicitudAccionPersonal = solicitud.Id,
            CodigoEstado = EstadoAprobada,
            Comentario = comentario,
            Fecha = DateTimeOffset.UtcNow,
            RegistradoPor = registradoPor,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AnularAsync(Guid id, string motivo, string registradoPor, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesAccionPersonal.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (solicitud is null)
        {
            throw new SolicitudAccionPersonalInexistenteException(id);
        }

        if (solicitud.CodigoEstado != EstadoIngresada)
        {
            throw new SolicitudAccionPersonalNoIngresadaException(id);
        }

        solicitud.CodigoEstado = EstadoAnulada;
        db.SolicitudAccionPersonalEtapas.Add(new SolicitudAccionPersonalEtapa
        {
            Id = Guid.NewGuid(),
            IdSolicitudAccionPersonal = solicitud.Id,
            CodigoEstado = EstadoAnulada,
            Comentario = motivo,
            Fecha = DateTimeOffset.UtcNow,
            RegistradoPor = registradoPor,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
