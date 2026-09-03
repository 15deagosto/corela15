using Corela15.Application.Colocacion;
using Corela15.Domain.Clientes;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Credito;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class GarantiaService(Corela15DbContext db) : IGarantiaService
{
    public async Task<GaranteAgregadoResult> AgregarGaranteAsync(
        AgregarGaranteRequest request, CancellationToken cancellationToken = default)
    {
        var solicitud = await db.SolicitudesPrestamo.FirstOrDefaultAsync(s => s.Id == request.IdSolicitud, cancellationToken);
        if (solicitud is null || solicitud.Estado != EstadoSolicitud.EnAnalisis)
        {
            throw new SolicitudPrestamoNoEnAnalisisException(request.IdSolicitud);
        }

        if (request.IdClienteGarante == solicitud.IdCliente)
        {
            throw new GaranteEsElTitularException(request.IdClienteGarante);
        }

        var garante = await db.Clientes.FirstOrDefaultAsync(c => c.Id == request.IdClienteGarante, cancellationToken);
        if (garante is null || garante.Estado != EstadoCliente.Activo)
        {
            throw new GaranteClienteInvalidoException(request.IdClienteGarante);
        }

        var entidad = new SolicitudPrestamoGarantia
        {
            Id = Guid.NewGuid(),
            IdSolicitudPrestamo = request.IdSolicitud,
            IdClienteGarante = request.IdClienteGarante,
            Detalle = request.Detalle,
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.SolicitudesPrestamoGarantia.Add(entidad);
        await db.SaveChangesAsync(cancellationToken);

        return new GaranteAgregadoResult(entidad.Id);
    }

    public async Task QuitarGaranteAsync(QuitarGaranteRequest request, CancellationToken cancellationToken = default)
    {
        var garantia = await db.SolicitudesPrestamoGarantia
            .Include(g => g.SolicitudPrestamo)
            .FirstOrDefaultAsync(g => g.Id == request.IdGarantia && g.Activo, cancellationToken);
        if (garantia is null)
        {
            throw new GarantiaInvalidaException(request.IdGarantia);
        }
        if (garantia.SolicitudPrestamo.Estado != EstadoSolicitud.EnAnalisis)
        {
            throw new SolicitudPrestamoNoEnAnalisisException(garantia.IdSolicitudPrestamo);
        }

        garantia.Activo = false;
        garantia.ModificadoEn = DateTimeOffset.UtcNow;
        garantia.ModificadoPor = request.RegistradoPor;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GaranteDetalle>> ListarPorSolicitudAsync(
        Guid idSolicitud, CancellationToken cancellationToken = default)
    {
        return await db.SolicitudesPrestamoGarantia
            .Include(g => g.ClienteGarante).ThenInclude(c => c.Persona)
            .Where(g => g.IdSolicitudPrestamo == idSolicitud && g.Activo)
            .Select(g => new GaranteDetalle(g.Id, g.IdClienteGarante, g.ClienteGarante.Persona.Nombre, g.Detalle, null, null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GaranteDetalle>> ListarPorPrestamoAsync(
        Guid idPrestamo, CancellationToken cancellationToken = default)
    {
        return await db.PrestamosGarantias
            .Include(g => g.ClienteGarante).ThenInclude(c => c.Persona)
            .Include(g => g.EstadoGarantia)
            .Where(g => g.IdPrestamo == idPrestamo)
            .Select(g => new GaranteDetalle(
                g.Id, g.IdClienteGarante, g.ClienteGarante.Persona.Nombre, null,
                g.CodigoEstadoGarantia, g.EstadoGarantia.Detalle))
            .ToListAsync(cancellationToken);
    }
}
