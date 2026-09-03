using Corela15.Application.Sujeto;
using Corela15.Domain.Sujeto;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ReclamoService(Corela15DbContext db) : IReclamoService
{
    private const string EstadoEnTramite = "1";
    private const string EstadoResuelto = "2";

    public async Task<ReclamoRegistradoResult> RegistrarAsync(
        RegistrarReclamoRequest request, CancellationToken cancellationToken = default)
    {
        var personaValida = await db.Personas.AnyAsync(p => p.Id == request.IdPersona, cancellationToken);
        if (!personaValida)
        {
            throw new PersonaInvalidaParaReclamoException(request.IdPersona);
        }

        var canalValido = await db.CanalesReclamo.AnyAsync(c => c.Codigo == request.CodigoCanalRecepcion && c.Activo, cancellationToken);
        if (!canalValido)
        {
            throw new CanalReclamoInvalidoException(request.CodigoCanalRecepcion);
        }

        var productoValido = await db.TiposProductoReclamo.AnyAsync(t => t.Id == request.IdTipoProducto && t.Activo, cancellationToken);
        if (!productoValido)
        {
            throw new TipoProductoReclamoInvalidoException(request.IdTipoProducto);
        }

        var conceptoValido = await db.ConceptosReclamoDetalle.AnyAsync(c => c.Codigo == request.CodigoConceptoDetalle && c.Activo, cancellationToken);
        if (!conceptoValido)
        {
            throw new ConceptoReclamoInvalidoException(request.CodigoConceptoDetalle);
        }

        var reclamo = new Reclamo
        {
            Id = Guid.NewGuid(),
            IdPersona = request.IdPersona,
            CodigoCanalRecepcion = request.CodigoCanalRecepcion,
            FechaRecepcion = DateOnly.FromDateTime(DateTime.UtcNow),
            IdTipoProducto = request.IdTipoProducto,
            CodigoConceptoDetalle = request.CodigoConceptoDetalle,
            CodigoEstado = EstadoEnTramite,
            Descripcion = request.Descripcion,
            RegistradoPor = request.RegistradoPor,
            CreadoEn = DateTimeOffset.UtcNow,
        };

        db.Reclamos.Add(reclamo);
        await db.SaveChangesAsync(cancellationToken);

        return new ReclamoRegistradoResult(reclamo.Id);
    }

    public async Task ResponderAsync(
        Guid idReclamo, ResponderReclamoRequest request, CancellationToken cancellationToken = default)
    {
        var reclamo = await db.Reclamos.FirstOrDefaultAsync(r => r.Id == idReclamo, cancellationToken);
        if (reclamo is null)
        {
            throw new ReclamoInexistenteException(idReclamo);
        }

        if (reclamo.CodigoEstado == EstadoResuelto)
        {
            throw new ReclamoYaResueltoException(idReclamo);
        }

        var tipoResolucionValido = await db.TiposResolucionReclamo
            .AnyAsync(t => t.Codigo == request.CodigoTipoResolucion && t.Activo, cancellationToken);
        if (!tipoResolucionValido)
        {
            throw new TipoResolucionInvalidoException(request.CodigoTipoResolucion);
        }

        db.ReclamosRespuesta.Add(new ReclamoRespuesta
        {
            Id = Guid.NewGuid(),
            IdReclamo = idReclamo,
            CodigoTipoResolucion = request.CodigoTipoResolucion,
            MontoRestituido = request.MontoRestituido,
            InteresSobreMonto = request.InteresSobreMonto,
            Descripcion = request.Descripcion,
            RegistradoPor = request.RegistradoPor,
            CreadoEn = DateTimeOffset.UtcNow,
        });

        reclamo.CodigoEstado = EstadoResuelto;
        await db.SaveChangesAsync(cancellationToken);
    }
}
