using Corela15.Application.Ahorros;
using Corela15.Application.Cobranza;
using Corela15.Application.Colocacion;
using Corela15.Domain.Cobranza;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class GestionCobranzaService(Corela15DbContext db) : IGestionCobranzaService
{
    public async Task<GestionCobranzaRegistradaResult> RegistrarAsync(
        RegistrarGestionCobranzaRequest request, CancellationToken cancellationToken = default)
    {
        var prestamoExiste = await db.Prestamos.AnyAsync(p => p.Id == request.IdPrestamo, cancellationToken);
        if (!prestamoExiste)
        {
            throw new PrestamoInvalidoException(request.IdPrestamo);
        }

        var clienteExiste = await db.Clientes.AnyAsync(c => c.Id == request.IdCliente, cancellationToken);
        if (!clienteExiste)
        {
            throw new ClienteInvalidoException(request.IdCliente);
        }

        var accion = await db.AccionesGestion
            .FirstOrDefaultAsync(a => a.Codigo == request.CodigoAccionGestion, cancellationToken);
        if (accion is null || !accion.Activo)
        {
            throw new AccionGestionInvalidaException(request.CodigoAccionGestion);
        }

        var fecha = DateOnly.FromDateTime(DateTime.UtcNow);
        var gestion = new GestionPrestamoCobranza
        {
            Id = Guid.NewGuid(),
            IdPrestamo = request.IdPrestamo,
            IdCliente = request.IdCliente,
            EsDeudor = request.EsDeudor,
            IdAccionGestion = accion.Id,
            TieneCompromisoPago = request.TieneCompromisoPago,
            Fecha = fecha,
            Observacion = request.Observacion,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.GestionesPrestamoCobranza.Add(gestion);
        await db.SaveChangesAsync(cancellationToken);

        return new GestionCobranzaRegistradaResult(gestion.Id, fecha);
    }
}
