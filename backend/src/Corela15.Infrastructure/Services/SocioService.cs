using Corela15.Application.Common;
using Corela15.Application.Sujeto;
using Corela15.Domain.Clientes;
using Corela15.Domain.Sujeto;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class SocioService(Corela15DbContext db) : ISocioService
{
    public async Task<SocioCreadoResult> CrearAsync(CrearSocioRequest request, CancellationToken cancellationToken = default)
    {
        if (!await db.TiposIdentificacion.AnyAsync(t => t.Id == request.IdTipoIdentificacion, cancellationToken))
        {
            throw new TipoIdentificacionInvalidoException(request.IdTipoIdentificacion);
        }

        if (!await db.Agencias.AnyAsync(a => a.Id == request.IdAgencia && a.Activa, cancellationToken))
        {
            throw new AgenciaInvalidaException(request.IdAgencia);
        }

        await ValidarCatalogosAsync(
            request.CodigoEstadoCivil, request.CodigoEducacion, request.CodigoVivienda,
            request.CodigoSectorVivienda, request.CodigoNacionalidad, request.CodigoProfesion,
            request.IdActividadEconomica, request.CodigoCausaVinculacion,
            request.CodigoCalificacionInterna, request.CodigoSectorEconomico, cancellationToken);

        if (await db.Personas.AnyAsync(
                p => p.IdTipoIdentificacion == request.IdTipoIdentificacion && p.Identificacion == request.Identificacion,
                cancellationToken))
        {
            throw new CodigoDuplicadoException("una persona", request.Identificacion);
        }

        string nombre;
        var idPersona = Guid.NewGuid();
        var persona = new Persona
        {
            Id = idPersona,
            Identificacion = request.Identificacion,
            IdTipoIdentificacion = request.IdTipoIdentificacion,
            Email = request.Email,
            IdActividadEconomica = request.IdActividadEconomica,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };

        if (request.EsPersonaNatural)
        {
            if (string.IsNullOrWhiteSpace(request.PrimerNombre) || string.IsNullOrWhiteSpace(request.ApellidoPaterno)
                || request.FechaNacimiento is null)
            {
                throw new DatosPersonaNaturalIncompletosException();
            }

            nombre = string.Join(' ', new[]
            {
                request.PrimerNombre, request.SegundoNombre, request.ApellidoPaterno, request.ApellidoMaterno,
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
            persona.Nombre = nombre;

            db.Personas.Add(persona);
            db.PersonasNaturales.Add(new PersonaNatural
            {
                IdPersona = idPersona,
                PrimerNombre = request.PrimerNombre,
                SegundoNombre = request.SegundoNombre,
                ApellidoPaterno = request.ApellidoPaterno,
                ApellidoMaterno = request.ApellidoMaterno,
                FechaNacimiento = request.FechaNacimiento.Value,
                EsMasculino = request.EsMasculino ?? true,
                CodigoEstadoCivil = request.CodigoEstadoCivil,
                CodigoEducacion = request.CodigoEducacion,
                CodigoVivienda = request.CodigoVivienda,
                CodigoSectorVivienda = request.CodigoSectorVivienda,
                CodigoNacionalidad = request.CodigoNacionalidad,
                CodigoProfesion = request.CodigoProfesion,
            });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.RazonSocial) || request.FechaCreacion is null)
            {
                throw new DatosPersonaJuridicaIncompletosException();
            }

            nombre = request.RazonSocial;
            persona.Nombre = nombre;

            db.Personas.Add(persona);
            db.PersonasJuridicas.Add(new PersonaJuridica
            {
                IdPersona = idPersona,
                RazonSocial = request.RazonSocial,
                FechaCreacion = request.FechaCreacion.Value,
            });
        }

        var ultimoNumero = await db.Clientes.CountAsync(cancellationToken);
        var numero = (ultimoNumero + 1).ToString().PadLeft(10, '0');

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            IdPersona = idPersona,
            IdAgencia = request.IdAgencia,
            Estado = EstadoCliente.Activo,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
            CodigoCausaVinculacion = request.CodigoCausaVinculacion,
            CodigoCalificacionInterna = request.CodigoCalificacionInterna,
            CodigoSectorEconomico = request.CodigoSectorEconomico,
        };
        db.Clientes.Add(cliente);

        await db.SaveChangesAsync(cancellationToken);

        return new SocioCreadoResult(cliente.Id, idPersona, numero, nombre);
    }

    public async Task ActualizarAsync(Guid idPersona, ActualizarSocioRequest request, CancellationToken cancellationToken = default)
    {
        var persona = await db.Personas
            .Include(p => p.PersonaNatural)
            .Include(p => p.PersonaJuridica)
            .FirstOrDefaultAsync(p => p.Id == idPersona, cancellationToken);

        if (persona is null)
        {
            throw new PersonaNoExisteException(idPersona);
        }

        await ValidarCatalogosAsync(
            request.CodigoEstadoCivil, request.CodigoEducacion, request.CodigoVivienda,
            request.CodigoSectorVivienda, request.CodigoNacionalidad, request.CodigoProfesion,
            request.IdActividadEconomica, request.CodigoCausaVinculacion,
            request.CodigoCalificacionInterna, request.CodigoSectorEconomico, cancellationToken);

        if (request.CodigoProvinciaDomicilio is not null)
        {
            var provinciaValida = await db.Provincias.AnyAsync(p => p.Codigo == request.CodigoProvinciaDomicilio, cancellationToken);
            if (!provinciaValida)
            {
                throw new CatalogoSocioInvalidoException("provincias", request.CodigoProvinciaDomicilio);
            }
        }

        persona.Email = request.Email;
        persona.Activos = request.Activos;
        persona.Pasivos = request.Pasivos;
        persona.Ingresos = request.Ingresos;
        persona.Egresos = request.Egresos;
        if (request.IdActividadEconomica.HasValue) persona.IdActividadEconomica = request.IdActividadEconomica;
        if (request.CallePrincipal is not null) persona.CallePrincipal = request.CallePrincipal;
        if (request.NumeroCasa is not null) persona.NumeroCasa = request.NumeroCasa;
        if (request.Barrio is not null) persona.Barrio = request.Barrio;
        if (request.CodigoProvinciaDomicilio is not null) persona.CodigoProvinciaDomicilio = request.CodigoProvinciaDomicilio;
        if (request.CalleSecundaria is not null) persona.CalleSecundaria = request.CalleSecundaria;
        if (request.CodigoPostal is not null) persona.CodigoPostal = request.CodigoPostal;
        if (request.Referencia is not null) persona.Referencia = request.Referencia;
        if (request.ParentescoServicioBasico is not null) persona.ParentescoServicioBasico = request.ParentescoServicioBasico;
        persona.ModificadoEn = DateTimeOffset.UtcNow;
        persona.ModificadoPor = request.RegistradoPor;

        if (persona.PersonaNatural is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.PrimerNombre)) persona.PersonaNatural.PrimerNombre = request.PrimerNombre;
            if (request.SegundoNombre is not null) persona.PersonaNatural.SegundoNombre = request.SegundoNombre;
            if (!string.IsNullOrWhiteSpace(request.ApellidoPaterno)) persona.PersonaNatural.ApellidoPaterno = request.ApellidoPaterno;
            if (request.ApellidoMaterno is not null) persona.PersonaNatural.ApellidoMaterno = request.ApellidoMaterno;
            if (request.EsPep.HasValue) persona.PersonaNatural.EsPep = request.EsPep.Value;
            if (request.CodigoEstadoCivil is not null) persona.PersonaNatural.CodigoEstadoCivil = request.CodigoEstadoCivil;
            if (request.CodigoEducacion is not null) persona.PersonaNatural.CodigoEducacion = request.CodigoEducacion;
            if (request.CodigoVivienda is not null) persona.PersonaNatural.CodigoVivienda = request.CodigoVivienda;
            if (request.CodigoSectorVivienda is not null) persona.PersonaNatural.CodigoSectorVivienda = request.CodigoSectorVivienda;
            if (request.CodigoNacionalidad is not null) persona.PersonaNatural.CodigoNacionalidad = request.CodigoNacionalidad;
            if (request.CodigoProfesion is not null) persona.PersonaNatural.CodigoProfesion = request.CodigoProfesion;
            if (request.CobraBonoDesarrolloHumano.HasValue) persona.PersonaNatural.CobraBonoDesarrolloHumano = request.CobraBonoDesarrolloHumano.Value;
            if (request.EsSeparacionDeBienes.HasValue) persona.PersonaNatural.EsSeparacionDeBienes = request.EsSeparacionDeBienes.Value;
            if (request.TieneDiscapacidad.HasValue) persona.PersonaNatural.TieneDiscapacidad = request.TieneDiscapacidad.Value;
            if (request.TieneCargasFamiliares.HasValue) persona.PersonaNatural.TieneCargasFamiliares = request.TieneCargasFamiliares.Value;
            if (request.NumeroCargasFamiliares.HasValue) persona.PersonaNatural.NumeroCargasFamiliares = request.NumeroCargasFamiliares.Value;

            persona.Nombre = string.Join(' ', new[]
            {
                persona.PersonaNatural.PrimerNombre, persona.PersonaNatural.SegundoNombre,
                persona.PersonaNatural.ApellidoPaterno, persona.PersonaNatural.ApellidoMaterno,
            }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
        else if (persona.PersonaJuridica is not null && !string.IsNullOrWhiteSpace(request.RazonSocial))
        {
            persona.PersonaJuridica.RazonSocial = request.RazonSocial;
            persona.Nombre = request.RazonSocial;
        }

        if (request.IdUsuarioOficial.HasValue
            && !await db.Usuarios.AnyAsync(u => u.Id == request.IdUsuarioOficial && u.Activo, cancellationToken))
        {
            throw new UsuarioOficialInvalidoException(request.IdUsuarioOficial.Value);
        }

        // Los campos de vinculación viven en Cliente, no en Persona — se
        // aplican sobre el Cliente vigente (Activo si existe, si no el más
        // reciente) de esta persona.
        if (request.CodigoCausaVinculacion is not null || request.CodigoCalificacionInterna is not null
            || request.CodigoSectorEconomico is not null || request.IdUsuarioOficial.HasValue || request.EsExento.HasValue)
        {
            var cliente = await db.Clientes
                .Where(c => c.IdPersona == idPersona)
                .OrderByDescending(c => c.Estado == EstadoCliente.Activo)
                .ThenByDescending(c => c.CreadoEn)
                .FirstOrDefaultAsync(cancellationToken);

            if (cliente is not null)
            {
                if (request.CodigoCausaVinculacion is not null) cliente.CodigoCausaVinculacion = request.CodigoCausaVinculacion;
                if (request.CodigoCalificacionInterna is not null) cliente.CodigoCalificacionInterna = request.CodigoCalificacionInterna;
                if (request.CodigoSectorEconomico is not null) cliente.CodigoSectorEconomico = request.CodigoSectorEconomico;
                if (request.IdUsuarioOficial.HasValue) cliente.IdUsuarioOficial = request.IdUsuarioOficial;
                if (request.EsExento.HasValue) cliente.EsExento = request.EsExento.Value;
                cliente.ModificadoEn = DateTimeOffset.UtcNow;
                cliente.ModificadoPor = request.RegistradoPor;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarCatalogosAsync(
        string? codigoEstadoCivil, string? codigoEducacion, string? codigoVivienda, string? codigoSectorVivienda,
        string? codigoNacionalidad, string? codigoProfesion, int? idActividadEconomica,
        string? codigoCausaVinculacion, string? codigoCalificacionInterna,
        string? codigoSectorEconomico, CancellationToken cancellationToken)
    {
        if (codigoEstadoCivil is not null && !await db.EstadosCiviles.AnyAsync(x => x.Codigo == codigoEstadoCivil, cancellationToken))
            throw new CatalogoSocioInvalidoException("estado civil", codigoEstadoCivil);
        if (codigoEducacion is not null && !await db.Educaciones.AnyAsync(x => x.Codigo == codigoEducacion, cancellationToken))
            throw new CatalogoSocioInvalidoException("nivel de educación", codigoEducacion);
        if (codigoVivienda is not null && !await db.Viviendas.AnyAsync(x => x.Codigo == codigoVivienda, cancellationToken))
            throw new CatalogoSocioInvalidoException("tipo de vivienda", codigoVivienda);
        if (codigoSectorVivienda is not null && !await db.SectoresVivienda.AnyAsync(x => x.Codigo == codigoSectorVivienda, cancellationToken))
            throw new CatalogoSocioInvalidoException("sector de vivienda", codigoSectorVivienda);
        if (codigoNacionalidad is not null && !await db.Nacionalidades.AnyAsync(x => x.Codigo == codigoNacionalidad, cancellationToken))
            throw new CatalogoSocioInvalidoException("nacionalidad", codigoNacionalidad);
        if (codigoProfesion is not null && !await db.Profesiones.AnyAsync(x => x.Codigo == codigoProfesion, cancellationToken))
            throw new CatalogoSocioInvalidoException("profesión", codigoProfesion);
        if (idActividadEconomica is not null && !await db.ActividadesEconomicas.AnyAsync(x => x.Id == idActividadEconomica, cancellationToken))
            throw new CatalogoSocioInvalidoException("actividad económica", idActividadEconomica.Value.ToString());
        if (codigoCausaVinculacion is not null && !await db.CausasVinculacion.AnyAsync(x => x.Codigo == codigoCausaVinculacion, cancellationToken))
            throw new CatalogoSocioInvalidoException("causa de vinculación", codigoCausaVinculacion);
        if (codigoCalificacionInterna is not null && !await db.CalificacionesInternas.AnyAsync(x => x.Codigo == codigoCalificacionInterna, cancellationToken))
            throw new CatalogoSocioInvalidoException("calificación interna", codigoCalificacionInterna);
        if (codigoSectorEconomico is not null && !await db.SectoresEconomicos.AnyAsync(x => x.Codigo == codigoSectorEconomico, cancellationToken))
            throw new CatalogoSocioInvalidoException("sector económico", codigoSectorEconomico);
    }

    public async Task CambiarEstadoClienteAsync(
        Guid idCliente, string nuevoEstado, string registradoPor, CancellationToken cancellationToken = default)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == idCliente, cancellationToken);
        if (cliente is null)
        {
            throw new ClienteNoExisteException(idCliente);
        }

        if (!Enum.TryParse<EstadoCliente>(nuevoEstado, ignoreCase: true, out var estado))
        {
            throw new EstadoClienteInvalidoException(nuevoEstado);
        }

        cliente.Estado = estado;
        cliente.ModificadoEn = DateTimeOffset.UtcNow;
        cliente.ModificadoPor = registradoPor;

        await db.SaveChangesAsync(cancellationToken);
    }
}
