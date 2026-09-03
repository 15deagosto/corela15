using Corela15.Application.Sujeto;
using Corela15.Domain.Sujeto;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class DatosPersonaService(Corela15DbContext db) : IDatosPersonaService
{
    public async Task<IReadOnlyList<TelefonoDetalle>> ListarTelefonosAsync(
        Guid idPersona, CancellationToken cancellationToken = default)
    {
        return await db.PersonasTelefonos
            .Where(t => t.IdPersona == idPersona && t.Activo)
            .Select(t => new TelefonoDetalle(t.Id, t.Telefono, t.EsTelefonoMovil, t.EsPrincipal, t.NotificacionSms))
            .ToListAsync(cancellationToken);
    }

    public async Task<RegistroAgregadoResult> AgregarTelefonoAsync(
        AgregarTelefonoRequest request, CancellationToken cancellationToken = default)
    {
        var personaExiste = await db.Personas.AnyAsync(p => p.Id == request.IdPersona, cancellationToken);
        if (!personaExiste)
        {
            throw new PersonaNoExisteException(request.IdPersona);
        }

        if (request.EsPrincipal)
        {
            await db.PersonasTelefonos
                .Where(t => t.IdPersona == request.IdPersona && t.Activo && t.EsPrincipal)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.EsPrincipal, false), cancellationToken);
        }

        var telefono = new PersonaTelefono
        {
            Id = Guid.NewGuid(),
            IdPersona = request.IdPersona,
            Telefono = request.Telefono,
            EsTelefonoMovil = request.EsTelefonoMovil,
            EsPrincipal = request.EsPrincipal,
            NotificacionSms = request.NotificacionSms,
            Activo = true,
        };
        db.PersonasTelefonos.Add(telefono);
        await db.SaveChangesAsync(cancellationToken);

        return new RegistroAgregadoResult(telefono.Id);
    }

    public async Task QuitarTelefonoAsync(Guid idTelefono, string registradoPor, CancellationToken cancellationToken = default)
    {
        var telefono = await db.PersonasTelefonos.FirstOrDefaultAsync(t => t.Id == idTelefono && t.Activo, cancellationToken);
        if (telefono is null)
        {
            throw new RegistroNoExisteException(idTelefono);
        }

        telefono.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConyugeDetalle>> ListarConyugesAsync(
        Guid idPersonaNatural, CancellationToken cancellationToken = default)
    {
        return await db.Conyuges
            .Include(c => c.PersonaConyuge)
            .Where(c => c.IdPersonaNatural == idPersonaNatural && c.Activo)
            .Select(c => new ConyugeDetalle(c.Id, c.IdPersonaConyuge, c.PersonaConyuge.Nombre, c.PersonaConyuge.Identificacion))
            .ToListAsync(cancellationToken);
    }

    public async Task<RegistroAgregadoResult> AgregarConyugeAsync(
        AgregarConyugeRequest request, CancellationToken cancellationToken = default)
    {
        var esPersonaNatural = await db.PersonasNaturales.AnyAsync(pn => pn.IdPersona == request.IdPersonaNatural, cancellationToken);
        if (!esPersonaNatural)
        {
            throw new PersonaNaturalNoExisteException(request.IdPersonaNatural);
        }

        if (request.IdPersonaConyuge == request.IdPersonaNatural)
        {
            throw new ConyugeEsLaMismaPersonaException(request.IdPersonaNatural);
        }

        var conyugeExiste = await db.Personas.AnyAsync(p => p.Id == request.IdPersonaConyuge, cancellationToken);
        if (!conyugeExiste)
        {
            throw new PersonaNoExisteException(request.IdPersonaConyuge);
        }

        var yaTieneConyuge = await db.Conyuges
            .AnyAsync(c => c.IdPersonaNatural == request.IdPersonaNatural && c.Activo, cancellationToken);
        if (yaTieneConyuge)
        {
            throw new YaTieneConyugeActivoException(request.IdPersonaNatural);
        }

        var conyuge = new Conyuge
        {
            Id = Guid.NewGuid(),
            IdPersonaNatural = request.IdPersonaNatural,
            IdPersonaConyuge = request.IdPersonaConyuge,
            Activo = true,
        };
        db.Conyuges.Add(conyuge);
        await db.SaveChangesAsync(cancellationToken);

        return new RegistroAgregadoResult(conyuge.Id);
    }

    public async Task QuitarConyugeAsync(Guid idConyuge, string registradoPor, CancellationToken cancellationToken = default)
    {
        var conyuge = await db.Conyuges.FirstOrDefaultAsync(c => c.Id == idConyuge && c.Activo, cancellationToken);
        if (conyuge is null)
        {
            throw new RegistroNoExisteException(idConyuge);
        }

        conyuge.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RepresentanteDetalle>> ListarRepresentantesAsync(
        Guid idPersona, CancellationToken cancellationToken = default)
    {
        return await db.Representantes
            .Include(r => r.PersonaRepresentante)
            .Where(r => r.IdPersona == idPersona && r.Activo)
            .Select(r => new RepresentanteDetalle(
                r.Id, r.IdPersonaRepresentante, r.PersonaRepresentante.Nombre, r.PersonaRepresentante.Identificacion,
                r.Principal, r.EjerceControl))
            .ToListAsync(cancellationToken);
    }

    public async Task<RegistroAgregadoResult> AgregarRepresentanteAsync(
        AgregarRepresentanteRequest request, CancellationToken cancellationToken = default)
    {
        var personaExiste = await db.Personas.AnyAsync(p => p.Id == request.IdPersona, cancellationToken);
        if (!personaExiste)
        {
            throw new PersonaNoExisteException(request.IdPersona);
        }

        if (request.IdPersonaRepresentante == request.IdPersona)
        {
            throw new RepresentanteEsLaMismaPersonaException(request.IdPersona);
        }

        var representanteExiste = await db.Personas.AnyAsync(p => p.Id == request.IdPersonaRepresentante, cancellationToken);
        if (!representanteExiste)
        {
            throw new PersonaNoExisteException(request.IdPersonaRepresentante);
        }

        if (request.Principal)
        {
            await db.Representantes
                .Where(r => r.IdPersona == request.IdPersona && r.Activo && r.Principal)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Principal, false), cancellationToken);
        }

        var representante = new Representante
        {
            Id = Guid.NewGuid(),
            IdPersona = request.IdPersona,
            IdPersonaRepresentante = request.IdPersonaRepresentante,
            Principal = request.Principal,
            EjerceControl = request.EjerceControl,
            Activo = true,
        };
        db.Representantes.Add(representante);
        await db.SaveChangesAsync(cancellationToken);

        return new RegistroAgregadoResult(representante.Id);
    }

    public async Task QuitarRepresentanteAsync(Guid idRepresentante, string registradoPor, CancellationToken cancellationToken = default)
    {
        var representante = await db.Representantes.FirstOrDefaultAsync(r => r.Id == idRepresentante && r.Activo, cancellationToken);
        if (representante is null)
        {
            throw new RegistroNoExisteException(idRepresentante);
        }

        representante.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
    }
}
