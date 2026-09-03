using Corela15.Application.Sujeto;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Clientes;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record SocioListItem(
    Guid Id, Guid IdPersona, string Numero, string Nombre, string Identificacion,
    string Agencia, string Estado, string? CodigoProvinciaDomicilio, bool EsPersonaNatural);

public record ProvinciaListItem(string Codigo, string Nombre);

public record ActualizarDomicilioBody(string? CodigoProvinciaDomicilio);

public record PersonaBusquedaItem(Guid Id, string Nombre, string Identificacion);

public record UsuarioParaOficialItem(Guid Id, string NombreUsuario);

public record AgregarTelefonoBody(string Telefono, bool EsTelefonoMovil, bool EsPrincipal, bool NotificacionSms);

public record AgregarConyugeBody(Guid IdPersonaConyuge);

public record AgregarRepresentanteBody(Guid IdPersonaRepresentante, bool Principal, bool EjerceControl);

public record S01Cabecera(string CodigoEstructura, string Ruc, DateOnly FechaCorte, int NumeroTotalRegistros);

public record S01ElementoDetalle(
    string TipoIdentificacion, string NumeroIdentificacion, string? PaisNacimiento, string ApellidosNombres,
    DateOnly FechaNacimiento, string Genero, decimal ValorCertifAportacion, DateOnly FechaIngreso);

public record S01Result(S01Cabecera Cabecera, IReadOnlyList<S01ElementoDetalle> Detalle, IReadOnlyList<string> Advertencias);

public record CrearSocioBody(
    bool EsPersonaNatural, string Identificacion, int IdTipoIdentificacion, string? Email, int IdAgencia,
    string? PrimerNombre, string? SegundoNombre, string? ApellidoPaterno, string? ApellidoMaterno,
    DateOnly? FechaNacimiento, bool? EsMasculino,
    string? RazonSocial, DateOnly? FechaCreacion,
    string? CodigoEstadoCivil, string? CodigoEducacion, string? CodigoVivienda, string? CodigoSectorVivienda,
    string? CodigoNacionalidad, string? CodigoProfesion, int? IdActividadEconomica,
    string? CodigoCausaVinculacion, string? CodigoCalificacionInterna,
    string? CodigoSectorEconomico);

public record ActualizarSocioBody(
    string? Email, decimal? Activos, decimal? Pasivos, decimal? Ingresos, decimal? Egresos,
    string? PrimerNombre, string? SegundoNombre, string? ApellidoPaterno, string? ApellidoMaterno, bool? EsPep,
    string? RazonSocial,
    string? CodigoEstadoCivil, string? CodigoEducacion, string? CodigoVivienda, string? CodigoSectorVivienda,
    string? CodigoNacionalidad, string? CodigoProfesion, int? IdActividadEconomica,
    bool? CobraBonoDesarrolloHumano, bool? EsSeparacionDeBienes, bool? TieneDiscapacidad,
    bool? TieneCargasFamiliares, int? NumeroCargasFamiliares,
    string? CallePrincipal, string? NumeroCasa, string? Barrio, string? CodigoProvinciaDomicilio,
    string? CalleSecundaria, string? CodigoPostal, string? Referencia, string? ParentescoServicioBasico,
    string? CodigoCausaVinculacion, string? CodigoCalificacionInterna,
    string? CodigoSectorEconomico, Guid? IdUsuarioOficial, bool? EsExento);

public record ActividadEconomicaBusquedaItem(int Id, string Codigo, string Nombre, string Nivel);

public record CambiarEstadoClienteBody(string Estado);

public record CatalogoResuelto(string Codigo, string Nombre);

public record SocioTelefonoDetalle(string Telefono, bool EsTelefonoMovil, bool EsPrincipal, bool NotificacionSms);

public record SocioConyugeDetalle(string Nombre, string Identificacion);

public record SocioRepresentanteDetalle(string Nombre, string Identificacion, bool Principal, bool EjerceControl);

public record SocioPerfilLavadoResumen(DateOnly Fecha, decimal? Patrimonio, decimal? IngresoMensual, string? Categoria);

public record SocioDetalleCompleto(
    // Cliente
    Guid IdCliente, string Numero, string Agencia, string Estado, DateTimeOffset CreadoEn,
    CatalogoResuelto? CausaVinculacion, CatalogoResuelto? CalificacionInterna, CatalogoResuelto? SectorEconomico,
    // Persona
    Guid IdPersona, string Identificacion, string TipoIdentificacion, string Nombre, string? Email,
    bool EsPersonaNatural, CatalogoResuelto? ProvinciaDomicilio, string? NumeroCasa, string? Barrio, string? CallePrincipal,
    decimal? Activos, decimal? Pasivos, decimal? Ingresos, decimal? Egresos, CatalogoResuelto? ActividadEconomica,
    // Persona natural
    DateOnly? FechaNacimiento, bool? EsMasculino, bool? EsPep,
    CatalogoResuelto? EstadoCivil, CatalogoResuelto? Educacion, CatalogoResuelto? Vivienda,
    CatalogoResuelto? SectorVivienda, CatalogoResuelto? Nacionalidad, CatalogoResuelto? Profesion,
    // Persona jurídica
    DateOnly? FechaCreacionEmpresa, bool? EsGrupo, bool? EsInstitucionBancaria, bool? EsPublica,
    // Relacionados
    IReadOnlyList<SocioTelefonoDetalle> Telefonos, SocioConyugeDetalle? Conyuge,
    IReadOnlyList<SocioRepresentanteDetalle> Representantes, SocioPerfilLavadoResumen? PerfilLavado);

public record SocioPerfilDetalle(
    decimal? Activos, decimal? Pasivos, decimal? Ingresos, decimal? Egresos, bool? EsPep,
    string? CodigoEstadoCivil, string? CodigoEducacion, string? CodigoVivienda, string? CodigoSectorVivienda,
    string? CodigoNacionalidad, string? CodigoProfesion, int? IdActividadEconomica, string? NombreActividadEconomica,
    bool? CobraBonoDesarrolloHumano, bool? EsSeparacionDeBienes, bool? TieneDiscapacidad,
    bool? TieneCargasFamiliares, int? NumeroCargasFamiliares,
    string? CalleSecundaria, string? CodigoPostal, string? Referencia, string? ParentescoServicioBasico,
    string? CodigoCausaVinculacion, string? CodigoCalificacionInterna,
    string? CodigoSectorEconomico, Guid? IdUsuarioOficial, string? NombreUsuarioOficial, bool? EsExento);

// Lectura simple: se consulta el DbContext directo desde el controller (sin
// pasar por Application) porque no hay lógica de negocio que orquestar, solo
// proyección de datos. Los casos de uso que escriben sí van por Application
// (ver ComprobanteContableService) — esta distinción es intencional, no
// un atajo por descuido.
[ApiController]
[Route("api/socios")]
[Authorize(Policy = "Menu:socios")]
public class SociosController(Corela15DbContext db, IDatosPersonaService datosPersonaService, ISocioService socioService) : ControllerBase
{
    // Alta de socio nuevo (Persona + PersonaNatural/PersonaJuridica + Cliente
    // activo) — antes este core solo tenía los 5 socios sembrados por
    // Nivel0_SeedDatosPrueba, sin ninguna forma real de registrar uno nuevo.
    [HttpPost]
    public async Task<ActionResult<SocioCreadoResult>> Crear([FromBody] CrearSocioBody body, CancellationToken cancellationToken)
    {
        var resultado = await socioService.CrearAsync(
            new CrearSocioRequest(
                body.EsPersonaNatural, body.Identificacion, body.IdTipoIdentificacion, body.Email, body.IdAgencia,
                body.PrimerNombre, body.SegundoNombre, body.ApellidoPaterno, body.ApellidoMaterno,
                body.FechaNacimiento, body.EsMasculino, body.RazonSocial, body.FechaCreacion,
                body.CodigoEstadoCivil, body.CodigoEducacion, body.CodigoVivienda, body.CodigoSectorVivienda,
                body.CodigoNacionalidad, body.CodigoProfesion, body.IdActividadEconomica,
                body.CodigoCausaVinculacion, body.CodigoCalificacionInterna,
                body.CodigoSectorEconomico, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/socios/{resultado.IdCliente}", resultado);
    }

    // Edición de datos básicos ya capturados — contacto, datos financieros
    // (los mismos que usa ScoreCrediticioService/PerfilLavadoActivosService,
    // antes solo tocables por SQL directo) y nombre/EsPep o razón social.
    [HttpPut("personas/{idPersona:guid}")]
    public async Task<IActionResult> Actualizar(
        Guid idPersona, [FromBody] ActualizarSocioBody body, CancellationToken cancellationToken)
    {
        await socioService.ActualizarAsync(
            idPersona,
            new ActualizarSocioRequest(
                body.Email, body.Activos, body.Pasivos, body.Ingresos, body.Egresos,
                body.PrimerNombre, body.SegundoNombre, body.ApellidoPaterno, body.ApellidoMaterno, body.EsPep,
                body.RazonSocial,
                body.CodigoEstadoCivil, body.CodigoEducacion, body.CodigoVivienda, body.CodigoSectorVivienda,
                body.CodigoNacionalidad, body.CodigoProfesion, body.IdActividadEconomica,
                body.CobraBonoDesarrolloHumano, body.EsSeparacionDeBienes, body.TieneDiscapacidad,
                body.TieneCargasFamiliares, body.NumeroCargasFamiliares,
                body.CallePrincipal, body.NumeroCasa, body.Barrio, body.CodigoProvinciaDomicilio,
                body.CalleSecundaria, body.CodigoPostal, body.Referencia, body.ParentescoServicioBasico,
                body.CodigoCausaVinculacion, body.CodigoCalificacionInterna,
                body.CodigoSectorEconomico, body.IdUsuarioOficial, body.EsExento, User.Identity!.Name!),
            cancellationToken);
        return NoContent();
    }

    // Perfil socioeconómico completo — para prellenar el formulario de edición
    // con los catálogos configurables (ver ISocioService.ActualizarAsync).
    [HttpGet("personas/{idPersona:guid}/perfil")]
    public async Task<ActionResult<SocioPerfilDetalle>> Perfil(Guid idPersona, CancellationToken cancellationToken)
    {
        var persona = await db.Personas
            .Include(p => p.PersonaNatural)
            .Include(p => p.ActividadEconomica)
            .FirstOrDefaultAsync(p => p.Id == idPersona, cancellationToken);
        if (persona is null) return NotFound();

        var cliente = await db.Clientes
            .Include(c => c.UsuarioOficial)
            .Where(c => c.IdPersona == idPersona)
            .OrderByDescending(c => c.Estado == EstadoCliente.Activo)
            .ThenByDescending(c => c.CreadoEn)
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(new SocioPerfilDetalle(
            persona.Activos, persona.Pasivos, persona.Ingresos, persona.Egresos,
            persona.PersonaNatural?.EsPep,
            persona.PersonaNatural?.CodigoEstadoCivil, persona.PersonaNatural?.CodigoEducacion,
            persona.PersonaNatural?.CodigoVivienda, persona.PersonaNatural?.CodigoSectorVivienda,
            persona.PersonaNatural?.CodigoNacionalidad, persona.PersonaNatural?.CodigoProfesion,
            persona.IdActividadEconomica, persona.ActividadEconomica?.Nombre,
            persona.PersonaNatural?.CobraBonoDesarrolloHumano, persona.PersonaNatural?.EsSeparacionDeBienes,
            persona.PersonaNatural?.TieneDiscapacidad, persona.PersonaNatural?.TieneCargasFamiliares,
            persona.PersonaNatural?.NumeroCargasFamiliares,
            persona.CalleSecundaria, persona.CodigoPostal, persona.Referencia, persona.ParentescoServicioBasico,
            cliente?.CodigoCausaVinculacion, cliente?.CodigoCalificacionInterna, cliente?.CodigoSectorEconomico,
            cliente?.IdUsuarioOficial, cliente?.UsuarioOficial?.NombreUsuario, cliente?.EsExento));
    }

    // Vista de detalle completa (doble clic en el listado) — un solo request
    // trae todo lo capturado del socio, con los códigos de catálogo ya
    // resueltos a nombre real (no solo el código) para que la pantalla no
    // tenga que cruzar 8 catálogos por su cuenta.
    [HttpGet("{idCliente:guid}/detalle")]
    public async Task<ActionResult<SocioDetalleCompleto>> Detalle(Guid idCliente, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes
            .Include(c => c.Agencia)
            .Include(c => c.CausaVinculacion)
            .Include(c => c.CalificacionInterna)
            .Include(c => c.SectorEconomico)
            .Include(c => c.Persona).ThenInclude(p => p.TipoIdentificacion)
            .Include(c => c.Persona).ThenInclude(p => p.ProvinciaDomicilio)
            .Include(c => c.Persona).ThenInclude(p => p.ActividadEconomica)
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.EstadoCivil)
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.Educacion)
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.Vivienda)
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.SectorVivienda)
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.Nacionalidad)
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.Profesion)
            .Include(c => c.Persona).ThenInclude(p => p.PersonaJuridica)
            .FirstOrDefaultAsync(c => c.Id == idCliente, cancellationToken);
        if (cliente is null) return NotFound();

        var persona = cliente.Persona;
        var natural = persona.PersonaNatural;
        var juridica = persona.PersonaJuridica;

        var telefonos = await db.PersonasTelefonos
            .Where(t => t.IdPersona == persona.Id && t.Activo)
            .Select(t => new SocioTelefonoDetalle(t.Telefono, t.EsTelefonoMovil, t.EsPrincipal, t.NotificacionSms))
            .ToListAsync(cancellationToken);

        SocioConyugeDetalle? conyuge = null;
        if (natural is not null)
        {
            conyuge = await db.Conyuges
                .Where(c => c.IdPersonaNatural == persona.Id && c.Activo)
                .Select(c => new SocioConyugeDetalle(c.PersonaConyuge.Nombre, c.PersonaConyuge.Identificacion))
                .FirstOrDefaultAsync(cancellationToken);
        }

        var representantes = await db.Representantes
            .Where(r => r.IdPersona == persona.Id && r.Activo)
            .Select(r => new SocioRepresentanteDetalle(
                r.PersonaRepresentante.Nombre, r.PersonaRepresentante.Identificacion, r.Principal, r.EjerceControl))
            .ToListAsync(cancellationToken);

        var perfilLavado = await db.CalificacionesCliente
            .Where(p => p.IdCliente == idCliente)
            .OrderByDescending(p => p.Fecha)
            .Select(p => new SocioPerfilLavadoResumen(p.Fecha, p.Patrimonio, p.IngresoMensual, p.Categoria != null ? p.Categoria.ToString() : null))
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(new SocioDetalleCompleto(
            cliente.Id, cliente.Numero, cliente.Agencia.Nombre, cliente.Estado.ToString(), cliente.CreadoEn,
            cliente.CausaVinculacion is null ? null : new CatalogoResuelto(cliente.CausaVinculacion.Codigo, cliente.CausaVinculacion.Descripcion),
            cliente.CalificacionInterna is null ? null : new CatalogoResuelto(cliente.CalificacionInterna.Codigo, cliente.CalificacionInterna.Nombre),
            cliente.SectorEconomico is null ? null : new CatalogoResuelto(cliente.SectorEconomico.Codigo, cliente.SectorEconomico.Nombre),
            persona.Id, persona.Identificacion, persona.TipoIdentificacion.Nombre, persona.Nombre, persona.Email,
            natural is not null,
            persona.ProvinciaDomicilio is null ? null : new CatalogoResuelto(persona.ProvinciaDomicilio.Codigo, persona.ProvinciaDomicilio.Nombre),
            persona.NumeroCasa, persona.Barrio, persona.CallePrincipal,
            persona.Activos, persona.Pasivos, persona.Ingresos, persona.Egresos,
            persona.ActividadEconomica is null ? null : new CatalogoResuelto(persona.ActividadEconomica.Codigo, persona.ActividadEconomica.Nombre),
            natural?.FechaNacimiento, natural?.EsMasculino, natural?.EsPep,
            natural?.EstadoCivil is null ? null : new CatalogoResuelto(natural.EstadoCivil.Codigo, natural.EstadoCivil.Nombre),
            natural?.Educacion is null ? null : new CatalogoResuelto(natural.Educacion.Codigo, natural.Educacion.Nombre),
            natural?.Vivienda is null ? null : new CatalogoResuelto(natural.Vivienda.Codigo, natural.Vivienda.Nombre),
            natural?.SectorVivienda is null ? null : new CatalogoResuelto(natural.SectorVivienda.Codigo, natural.SectorVivienda.Nombre),
            natural?.Nacionalidad is null ? null : new CatalogoResuelto(natural.Nacionalidad.Codigo, natural.Nacionalidad.Nombre),
            natural?.Profesion is null ? null : new CatalogoResuelto(natural.Profesion.Codigo, natural.Profesion.Nombre),
            juridica?.FechaCreacion, juridica?.EsGrupo, juridica?.EsInstitucionBancaria, juridica?.EsPublica,
            telefonos, conyuge, representantes, perfilLavado));
    }

    // Activar/inactivar/suspender el Cliente — antes Cliente.Estado solo se
    // podía cambiar por SQL directo (aunque el trigger de versionado real ya
    // lo capturaba en cliente_historico desde que se construyó, nunca hubo
    // pantalla para generar el cambio).
    [HttpPatch("{idCliente:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(
        Guid idCliente, [FromBody] CambiarEstadoClienteBody body, CancellationToken cancellationToken)
    {
        await socioService.CambiarEstadoClienteAsync(idCliente, body.Estado, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SocioListItem>>> Listar(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Clientes
            .Include(c => c.Persona)
            .Include(c => c.Agencia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Persona.Nombre, $"%{q}%") ||
                EF.Functions.ILike(c.Numero, $"%{q}%") ||
                EF.Functions.ILike(c.Persona.Identificacion, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(c => c.Persona.Nombre)
            .Select(c => new SocioListItem(
                c.Id, c.IdPersona, c.Numero, c.Persona.Nombre, c.Persona.Identificacion,
                c.Agencia.Nombre, c.Estado.ToString(), c.Persona.CodigoProvinciaDomicilio,
                c.Persona.PersonaNatural != null))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("buscar-persona")]
    public async Task<ActionResult<IReadOnlyList<PersonaBusquedaItem>>> BuscarPersona(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Personas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(p => EF.Functions.ILike(p.Nombre, $"%{q}%") || EF.Functions.ILike(p.Identificacion, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(p => p.Nombre)
            .Take(20)
            .Select(p => new PersonaBusquedaItem(p.Id, p.Nombre, p.Identificacion))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Catálogo real CIIU (verificado contra GENERAL.ACTIVIDAD_ECONOMICA,
    // ver ActividadEconomica.cs) — 3.046 filas, demasiadas para un select
    // plano, se busca por código o nombre igual que BuscarPersona.
    [HttpGet("actividades-economicas")]
    public async Task<ActionResult<IReadOnlyList<ActividadEconomicaBusquedaItem>>> BuscarActividadEconomica(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.ActividadesEconomicas.Include(a => a.TipoActividad).Where(a => a.Activo).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(a => EF.Functions.ILike(a.Nombre, $"%{q}%") || EF.Functions.ILike(a.Codigo, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(a => a.Codigo)
            .Take(30)
            .Select(a => new ActividadEconomicaBusquedaItem(a.Id, a.Codigo, a.Nombre, a.TipoActividad.Nombre))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Catálogo real (verificado contra SUJETO.PROFESION, 861 filas) — lista
    // completa de un solo golpe, mismo patrón que nacionalidades (212 filas).
    [HttpGet("profesiones")]
    public async Task<ActionResult<IReadOnlyList<CatalogoResuelto>>> Profesiones(CancellationToken cancellationToken)
    {
        var resultado = await db.Profesiones
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new CatalogoResuelto(p.Codigo, p.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    // Lista de usuarios activos, para asignar el "oficial" responsable del
    // socio (Cliente.IdUsuarioOficial, campo real que ya existía sin uso).
    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaOficialItem>>> UsuariosParaOficial(CancellationToken cancellationToken)
    {
        var resultado = await db.Usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioParaOficialItem(u.Id, u.NombreUsuario))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("personas/{idPersona:guid}/telefonos")]
    public async Task<ActionResult<IReadOnlyList<TelefonoDetalle>>> Telefonos(Guid idPersona, CancellationToken cancellationToken)
    {
        var resultado = await datosPersonaService.ListarTelefonosAsync(idPersona, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("personas/{idPersona:guid}/telefonos")]
    public async Task<ActionResult<RegistroAgregadoResult>> AgregarTelefono(
        Guid idPersona, [FromBody] AgregarTelefonoBody body, CancellationToken cancellationToken)
    {
        var resultado = await datosPersonaService.AgregarTelefonoAsync(
            new AgregarTelefonoRequest(idPersona, body.Telefono, body.EsTelefonoMovil, body.EsPrincipal, body.NotificacionSms, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/socios/personas/{idPersona}/telefonos", resultado);
    }

    [HttpDelete("telefonos/{idTelefono:guid}")]
    public async Task<IActionResult> QuitarTelefono(Guid idTelefono, CancellationToken cancellationToken)
    {
        await datosPersonaService.QuitarTelefonoAsync(idTelefono, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpGet("personas/{idPersonaNatural:guid}/conyuges")]
    public async Task<ActionResult<IReadOnlyList<ConyugeDetalle>>> Conyuges(Guid idPersonaNatural, CancellationToken cancellationToken)
    {
        var resultado = await datosPersonaService.ListarConyugesAsync(idPersonaNatural, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("personas/{idPersonaNatural:guid}/conyuges")]
    public async Task<ActionResult<RegistroAgregadoResult>> AgregarConyuge(
        Guid idPersonaNatural, [FromBody] AgregarConyugeBody body, CancellationToken cancellationToken)
    {
        var resultado = await datosPersonaService.AgregarConyugeAsync(
            new AgregarConyugeRequest(idPersonaNatural, body.IdPersonaConyuge, User.Identity!.Name!), cancellationToken);
        return Created($"/api/socios/personas/{idPersonaNatural}/conyuges", resultado);
    }

    [HttpDelete("conyuges/{idConyuge:guid}")]
    public async Task<IActionResult> QuitarConyuge(Guid idConyuge, CancellationToken cancellationToken)
    {
        await datosPersonaService.QuitarConyugeAsync(idConyuge, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpGet("personas/{idPersona:guid}/representantes")]
    public async Task<ActionResult<IReadOnlyList<RepresentanteDetalle>>> Representantes(Guid idPersona, CancellationToken cancellationToken)
    {
        var resultado = await datosPersonaService.ListarRepresentantesAsync(idPersona, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("personas/{idPersona:guid}/representantes")]
    public async Task<ActionResult<RegistroAgregadoResult>> AgregarRepresentante(
        Guid idPersona, [FromBody] AgregarRepresentanteBody body, CancellationToken cancellationToken)
    {
        var resultado = await datosPersonaService.AgregarRepresentanteAsync(
            new AgregarRepresentanteRequest(idPersona, body.IdPersonaRepresentante, body.Principal, body.EjerceControl, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/socios/personas/{idPersona}/representantes", resultado);
    }

    [HttpDelete("representantes/{idRepresentante:guid}")]
    public async Task<IActionResult> QuitarRepresentante(Guid idRepresentante, CancellationToken cancellationToken)
    {
        await datosPersonaService.QuitarRepresentanteAsync(idRepresentante, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpGet("provincias")]
    public async Task<ActionResult<IReadOnlyList<ProvinciaListItem>>> Provincias(CancellationToken cancellationToken)
    {
        var resultado = await db.Provincias
            .OrderBy(p => p.Codigo)
            .Select(p => new ProvinciaListItem(p.Codigo, p.Nombre))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Captura del domicilio del socio (provincia INEC/SEPS, Tabla 05) — sin
    // invariante de negocio más allá de que el código exista (la FK ya lo
    // garantiza), mismo patrón directo-contra-DbContext de un catálogo
    // simple de Configuración, aunque este endpoint vive en Socios porque
    // es el dato del cliente, no un catálogo que se administre aparte.
    [HttpPut("{id:guid}/domicilio")]
    public async Task<IActionResult> ActualizarDomicilio(
        Guid id, [FromBody] ActualizarDomicilioBody body, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.Include(c => c.Persona).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (cliente is null)
        {
            return NotFound();
        }

        cliente.Persona.CodigoProvinciaDomicilio = body.CodigoProvinciaDomicilio;
        cliente.Persona.ModificadoEn = DateTimeOffset.UtcNow;
        cliente.Persona.ModificadoPor = User.Identity!.Name!;
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // Estructura real S01 "Socios" (Manual Tecnológico de Estructura de
    // Datos v3.0 + XSD real descargado, a diferencia de B11/B13 este SÍ
    // trajo el XSD con nombres de atributo exactos). Reporta solo socios
    // persona natural activos — género/fecha de nacimiento son campos de
    // PersonaNatural, y la muestra real del manual solo tiene ejemplos de
    // persona natural; los socios persona jurídica quedan fuera,
    // documentado como advertencia explícita, no oculto.
    private static readonly Dictionary<string, string> CodigoTipoIdentificacionSeps =
        new() { ["CED"] = "C", ["RUC"] = "R", ["PAS"] = "P" };

    [HttpGet("reportes/s01")]
    public async Task<ActionResult<S01Result>> ReporteS01(CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var empresa = await db.Empresas.FirstOrDefaultAsync(cancellationToken);
        var ruc = empresa?.Ruc ?? string.Empty;

        var idTipoCuentaCert = await db.TiposCuenta
            .Where(t => t.Codigo == "CERT").Select(t => (int?)t.Id).FirstOrDefaultAsync(cancellationToken);

        var certificadosPorCliente = idTipoCuentaCert is null
            ? new Dictionary<Guid, decimal>()
            : await db.CuentasClientes
                .Where(cc => cc.Cuenta.IdTipoCuenta == idTipoCuentaCert.Value && cc.Cuenta.Estado == EstadoCuenta.Activa)
                .SelectMany(cc => db.CuentasItemSaldo
                    .Where(cis => cis.IdCuenta == cc.IdCuenta && cis.ItemSaldo.Codigo == "DISP")
                    .Select(cis => new { cc.IdCliente, cis.Saldo }))
                .GroupBy(x => x.IdCliente)
                .Select(g => new { IdCliente = g.Key, Total = g.Sum(x => x.Saldo) })
                .ToDictionaryAsync(x => x.IdCliente, x => x.Total, cancellationToken);

        var advertencias = new List<string>();
        if (idTipoCuentaCert is null)
        {
            advertencias.Add("No existe un producto 'CERT' (Certificados de Aportación) configurado — valorCertifAportacion se reportó en 0.00 para todos los socios.");
        }

        var clientesNaturales = await db.Clientes
            .Include(c => c.Persona).ThenInclude(p => p.PersonaNatural)
            .Include(c => c.Persona).ThenInclude(p => p.Pais)
            .Include(c => c.Persona).ThenInclude(p => p.TipoIdentificacion)
            .Where(c => c.Estado == EstadoCliente.Activo && c.Persona.PersonaNatural != null)
            .OrderBy(c => c.Persona.Nombre)
            .ToListAsync(cancellationToken);

        var clientesJuridicosActivos = await db.Clientes
            .CountAsync(c => c.Estado == EstadoCliente.Activo && c.Persona.PersonaJuridica != null, cancellationToken);
        if (clientesJuridicosActivos > 0)
        {
            advertencias.Add($"{clientesJuridicosActivos} socio(s) persona jurídica activos no se incluyen en este reporte — S01 solo modela persona natural (género/fecha de nacimiento no aplican a persona jurídica en este core).");
        }

        var detalle = new List<S01ElementoDetalle>();
        foreach (var cliente in clientesNaturales)
        {
            var codigoTipoId = CodigoTipoIdentificacionSeps.GetValueOrDefault(cliente.Persona.TipoIdentificacion.Codigo);
            if (codigoTipoId is null)
            {
                advertencias.Add($"Socio {cliente.Numero}: tipo de identificación '{cliente.Persona.TipoIdentificacion.Codigo}' sin mapeo a la Tabla 02 SEPS, se omitió del detalle.");
                continue;
            }

            var paisNacimiento = cliente.Persona.Pais?.Codigo == "EC" ? "ECU" : null;
            var valorCertificado = certificadosPorCliente.GetValueOrDefault(cliente.Id, 0m);

            detalle.Add(new S01ElementoDetalle(
                codigoTipoId, cliente.Persona.Identificacion, paisNacimiento, cliente.Persona.Nombre,
                cliente.Persona.PersonaNatural!.FechaNacimiento, cliente.Persona.PersonaNatural!.EsMasculino ? "M" : "F",
                valorCertificado, DateOnly.FromDateTime(cliente.CreadoEn.UtcDateTime)));
        }

        advertencias.Add("fechaIngreso se calcula como la fecha de creación del registro de cliente (Cliente.CreadoEn) — este core no captura una fecha de ingreso como socio distinta de esa, documentado como una aproximación real, no un dato inventado.");
        advertencias.Add("asambleaGeneral/fechaRepresentanteAsamblea/directivo/fechaDirectivo (campos opcionales del XSD real) no se incluyen — este core no modela participación en asamblea ni consejo directivo (equivalente a SUJETO.CONSEJOVIGILANCIA en Softbank, fuera de alcance).");

        return Ok(new S01Result(
            new S01Cabecera("S01", ruc, hoy, detalle.Count), detalle, advertencias));
    }
}
