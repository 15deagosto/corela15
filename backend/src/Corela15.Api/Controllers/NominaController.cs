using Corela15.Application.Nomina;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record EmpleadoListItem(
    Guid Id, Guid IdPersona, string Nombre, string Identificacion, int IdCargo, string Cargo, int IdAgencia, string Agencia,
    bool RecibeFondosReserva, DateOnly FechaIngreso, string Estado, decimal? SueldoActual);

public record CargoListItem(int Id, string Nombre, bool SeProrrateaSueldo, bool EsCargoExterno, bool Activo);

public record RolPagosListItem(
    Guid Id, DateOnly Periodo, string Tipo, string Estado, int CantidadEmpleados, decimal TotalGeneral);

public record BeneficioEmpleadoItem(
    Guid IdEmpleado, string Nombre, decimal Proyectado, decimal Acumulado, decimal Pagado, decimal Pendiente, DateOnly? UltimoDevengo);

public record PagarBeneficioBody(Guid IdEmpleado);

public record ParametroNominaItem(int Id, decimal SalarioBasicoUnificado);

public record ActualizarParametroNominaBody(decimal SalarioBasicoUnificado);

public record TipoAccionPersonalItem(
    int Id, string Detalle, bool ActivaContrato, bool DesactivaContrato, bool EsCambioCargoSueldo,
    bool EsCambioAgenciaDepartamento, bool EsFormaPagoDecimos, bool EsFormaPagoFondosReserva);

public record SolicitudAccionPersonalEtapaItem(string CodigoEstado, string? Comentario, DateTimeOffset Fecha, string RegistradoPor);

public record SolicitudAccionPersonalItem(
    Guid Id, int NumeroAccion, Guid IdEmpleado, string Empleado, int IdTipoAccionPersonal, string TipoAccionPersonal,
    string Detalle, string CodigoEstado, string Estado, DateOnly Fecha, int? IdCargoNuevo, decimal? NuevoSueldo,
    int? IdAgenciaNueva, bool? NuevoValorRecibeFondosReserva, IReadOnlyList<SolicitudAccionPersonalEtapaItem> Etapas);

public record AprobarAccionPersonalBody(string? Comentario);

public record AnularAccionPersonalBody(string Motivo);

public record TipoContratoItem(string Codigo, string Nombre, bool EsTiempoCompleto, bool EsTiempoParcial, bool TieneFechaSalida);

public record EmpleadoContratoItem(
    Guid Id, string CodigoTipoContrato, string TipoContrato, int NumeroContrato, DateOnly FechaIngreso,
    DateOnly? FechaSalida, bool Activo);

public record CrearEmpleadoContratoBody(string CodigoTipoContrato, DateOnly FechaIngreso);

public record EmpleadoDatosAdicionalesItem(
    Guid IdEmpleado, string? CodigoIess, DateOnly? FechaIngresoIess, DateOnly? FechaSalidaIess,
    DateOnly? FechaIngresoMinisterioLaboral, DateOnly? FechaSalidaMinisterioLaboral, int? NumeroCargasFamiliares,
    bool PagoDecimoMensual, bool PagoFondosReservaRol, bool ExtensionConyugal);

public record ActualizarDatosAdicionalesBody(
    string? CodigoIess, DateOnly? FechaIngresoIess, DateOnly? FechaSalidaIess,
    DateOnly? FechaIngresoMinisterioLaboral, DateOnly? FechaSalidaMinisterioLaboral, int? NumeroCargasFamiliares,
    bool PagoDecimoMensual, bool PagoFondosReservaRol, bool ExtensionConyugal);

public record EmpleadoTelefonoItem(Guid Id, string Telefono, bool EsTelefonoMovil, bool EsPrincipal, bool NotificacionSms);

public record AgregarEmpleadoTelefonoBody(string Telefono, bool EsTelefonoMovil, bool EsPrincipal, bool NotificacionSms);

public record EmpleadoPersonaDetalle(
    Guid IdPersona, string Nombre, string Identificacion, string? Email,
    string? CallePrincipal, string? NumeroCasa, string? Barrio, CatalogoResuelto? ProvinciaDomicilio,
    DateOnly? FechaNacimiento, bool? EsMasculino,
    CatalogoResuelto? EstadoCivil, CatalogoResuelto? Educacion, CatalogoResuelto? Nacionalidad,
    IReadOnlyList<EmpleadoTelefonoItem> Telefonos);

public record ActualizarEmpleadoPersonaBody(
    string? Email, string? CallePrincipal, string? NumeroCasa, string? Barrio, string? CodigoProvinciaDomicilio,
    string? PrimerNombre, string? SegundoNombre, string? ApellidoPaterno, string? ApellidoMaterno,
    DateOnly? FechaNacimiento, bool? EsMasculino,
    string? CodigoEstadoCivil, string? CodigoEducacion, string? CodigoNacionalidad);

[ApiController]
[Route("api/nomina")]
[Authorize(Policy = "Menu:nomina")]
public class NominaController(
    Corela15DbContext db, IRolPagosService rolPagosService, IBeneficioSocialService beneficioSocialService,
    IEmpleadoService empleadoService, ISolicitudAccionPersonalService solicitudAccionPersonalService,
    ICalculoImpuestoRentaService calculoImpuestoRentaService,
    Corela15.Application.Sujeto.ISocioService socioService, Corela15.Application.Sujeto.IDatosPersonaService datosPersonaService)
    : ControllerBase
{
    [HttpGet("empleados")]
    public async Task<ActionResult<IReadOnlyList<EmpleadoListItem>>> Empleados(CancellationToken cancellationToken)
    {
        var resultado = await db.Empleados
            .OrderBy(e => e.Persona.Nombre)
            .Select(e => new EmpleadoListItem(
                e.Id, e.IdPersona, e.Persona.Nombre, e.Persona.Identificacion, e.IdCargo, e.Cargo.Nombre, e.IdAgencia, e.Agencia.Nombre,
                e.RecibeFondosReserva, e.FechaIngreso, e.Estado.ToString(), e.SueldoActual))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("empleados/{idEmpleado:guid}/persona-detalle")]
    public async Task<ActionResult<EmpleadoPersonaDetalle>> PersonaDetalle(Guid idEmpleado, CancellationToken cancellationToken)
    {
        var empleado = await db.Empleados
            .Include(e => e.Persona).ThenInclude(p => p.ProvinciaDomicilio)
            .Include(e => e.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.EstadoCivil)
            .Include(e => e.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.Educacion)
            .Include(e => e.Persona).ThenInclude(p => p.PersonaNatural!).ThenInclude(pn => pn.Nacionalidad)
            .FirstOrDefaultAsync(e => e.Id == idEmpleado, cancellationToken);
        if (empleado is null) return NotFound();

        var persona = empleado.Persona;
        var natural = persona.PersonaNatural;

        var telefonos = (await datosPersonaService.ListarTelefonosAsync(persona.Id, cancellationToken))
            .Select(t => new EmpleadoTelefonoItem(t.Id, t.Telefono, t.EsTelefonoMovil, t.EsPrincipal, t.NotificacionSms))
            .ToList();

        return Ok(new EmpleadoPersonaDetalle(
            persona.Id, persona.Nombre, persona.Identificacion, persona.Email,
            persona.CallePrincipal, persona.NumeroCasa, persona.Barrio,
            persona.ProvinciaDomicilio is null ? null : new CatalogoResuelto(persona.ProvinciaDomicilio.Codigo, persona.ProvinciaDomicilio.Nombre),
            natural?.FechaNacimiento, natural?.EsMasculino,
            natural?.EstadoCivil is null ? null : new CatalogoResuelto(natural.EstadoCivil.Codigo, natural.EstadoCivil.Nombre),
            natural?.Educacion is null ? null : new CatalogoResuelto(natural.Educacion.Codigo, natural.Educacion.Nombre),
            natural?.Nacionalidad is null ? null : new CatalogoResuelto(natural.Nacionalidad.Codigo, natural.Nacionalidad.Nombre),
            telefonos));
    }

    [HttpPut("empleados/{idEmpleado:guid}/persona")]
    public async Task<IActionResult> ActualizarPersona(
        Guid idEmpleado, [FromBody] ActualizarEmpleadoPersonaBody body, CancellationToken cancellationToken)
    {
        var idPersona = await db.Empleados.Where(e => e.Id == idEmpleado).Select(e => (Guid?)e.IdPersona).FirstOrDefaultAsync(cancellationToken);
        if (idPersona is null) return NotFound();

        var registradoPor = User.Identity?.Name ?? "sistema";
        await socioService.ActualizarAsync(
            idPersona.Value,
            new Corela15.Application.Sujeto.ActualizarSocioRequest(
                body.Email, null, null, null, null,
                body.PrimerNombre, body.SegundoNombre, body.ApellidoPaterno, body.ApellidoMaterno, null,
                null,
                body.CodigoEstadoCivil, body.CodigoEducacion, null, null,
                body.CodigoNacionalidad, null, null,
                null, null, null, null, null,
                body.CallePrincipal, body.NumeroCasa, body.Barrio, body.CodigoProvinciaDomicilio,
                null, null, null, null,
                null, null, null, null, null, registradoPor),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("empleados/{idEmpleado:guid}/telefonos")]
    public async Task<ActionResult<IReadOnlyList<EmpleadoTelefonoItem>>> Telefonos(Guid idEmpleado, CancellationToken cancellationToken)
    {
        var idPersona = await db.Empleados.Where(e => e.Id == idEmpleado).Select(e => (Guid?)e.IdPersona).FirstOrDefaultAsync(cancellationToken);
        if (idPersona is null) return NotFound();

        var telefonos = (await datosPersonaService.ListarTelefonosAsync(idPersona.Value, cancellationToken))
            .Select(t => new EmpleadoTelefonoItem(t.Id, t.Telefono, t.EsTelefonoMovil, t.EsPrincipal, t.NotificacionSms));
        return Ok(telefonos);
    }

    [HttpPost("empleados/{idEmpleado:guid}/telefonos")]
    public async Task<IActionResult> AgregarTelefono(
        Guid idEmpleado, [FromBody] AgregarEmpleadoTelefonoBody body, CancellationToken cancellationToken)
    {
        var idPersona = await db.Empleados.Where(e => e.Id == idEmpleado).Select(e => (Guid?)e.IdPersona).FirstOrDefaultAsync(cancellationToken);
        if (idPersona is null) return NotFound();

        var registradoPor = User.Identity?.Name ?? "sistema";
        var resultado = await datosPersonaService.AgregarTelefonoAsync(
            new Corela15.Application.Sujeto.AgregarTelefonoRequest(
                idPersona.Value, body.Telefono, body.EsTelefonoMovil, body.EsPrincipal, body.NotificacionSms, registradoPor),
            cancellationToken);

        return Created($"/api/nomina/empleados/{idEmpleado}/telefonos/{resultado.Id}", resultado);
    }

    [HttpDelete("telefonos/{id:guid}")]
    public async Task<IActionResult> QuitarTelefono(Guid id, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        await datosPersonaService.QuitarTelefonoAsync(id, registradoPor, cancellationToken);
        return NoContent();
    }

    [HttpGet("personas/buscar")]
    public async Task<ActionResult<IReadOnlyList<PersonaBusquedaItem>>> BuscarPersona(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Personas.Where(p => !db.Empleados.Any(e => e.IdPersona == p.Id));
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

    [HttpPost("empleados")]
    public async Task<ActionResult<EmpleadoCreadoResult>> CrearEmpleado(
        [FromBody] CrearEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        var resultado = await empleadoService.CrearAsync(request, registradoPor, cancellationToken);
        return Created($"/api/nomina/empleados/{resultado.IdEmpleado}", resultado);
    }

    [HttpPut("empleados/{id:guid}")]
    public async Task<IActionResult> ActualizarEmpleado(
        Guid id, [FromBody] ActualizarEmpleadoRequest request, CancellationToken cancellationToken)
    {
        await empleadoService.ActualizarAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("cargos")]
    public async Task<ActionResult<IReadOnlyList<CargoListItem>>> Cargos(CancellationToken cancellationToken)
    {
        var resultado = await db.Cargos
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CargoListItem(c.Id, c.Nombre, c.SeProrrateaSueldo, c.EsCargoExterno, c.Activo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("parametro")]
    public async Task<ActionResult<ParametroNominaItem>> Parametro(CancellationToken cancellationToken)
    {
        var parametro = await db.ParametrosNomina.FirstOrDefaultAsync(cancellationToken);
        return parametro is null ? NotFound() : Ok(new ParametroNominaItem(parametro.Id, parametro.SalarioBasicoUnificado));
    }

    [HttpPut("parametro")]
    public async Task<ActionResult<ParametroNominaItem>> ActualizarParametro(
        [FromBody] ActualizarParametroNominaBody body, CancellationToken cancellationToken)
    {
        var parametro = await db.ParametrosNomina.FirstOrDefaultAsync(cancellationToken);
        if (parametro is null) return NotFound();

        parametro.SalarioBasicoUnificado = body.SalarioBasicoUnificado;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new ParametroNominaItem(parametro.Id, parametro.SalarioBasicoUnificado));
    }

    [HttpGet("roles-pagos")]
    public async Task<ActionResult<IReadOnlyList<RolPagosListItem>>> RolesPagos(CancellationToken cancellationToken)
    {
        var resultado = await db.RolesPagos
            .Include(r => r.Empleados)
            .OrderByDescending(r => r.Periodo)
            .Select(r => new RolPagosListItem(
                r.Id, r.Periodo, r.Tipo.ToString(), r.Estado.ToString(),
                r.Empleados.Count, r.Empleados.Sum(e => e.Total)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("roles-pagos")]
    public async Task<ActionResult<RolPagosGeneradoResult>> Generar(
        [FromBody] GenerarRolPagosRequest request, CancellationToken cancellationToken)
    {
        var resultado = await rolPagosService.GenerarAsync(request, cancellationToken);
        return Created($"/api/nomina/roles-pagos/{resultado.IdRolPagos}", resultado);
    }

    // Décimos, fondos de reserva y provisión de vacaciones (ver CLAUDE.md
    // "Nómina — beneficios sociales reales")

    [HttpPost("beneficios/devengo")]
    public async Task<ActionResult<DevengoBeneficiosResult>> EjecutarDevengoBeneficios(CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        var resultado = await beneficioSocialService.EjecutarDevengoAsync(registradoPor, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("beneficios/decimo-tercero")]
    public async Task<ActionResult<IReadOnlyList<BeneficioEmpleadoItem>>> DecimoTercero(CancellationToken cancellationToken) =>
        Ok(await db.EmpleadosDecimoTercero
            .OrderBy(d => d.Empleado.Persona.Nombre)
            .Select(d => new BeneficioEmpleadoItem(
                d.IdEmpleado, d.Empleado.Persona.Nombre, d.Proyectado, d.Acumulado, d.Pagado, d.Acumulado - d.Pagado, d.UltimoDevengo))
            .ToListAsync(cancellationToken));

    [HttpGet("beneficios/decimo-cuarto")]
    public async Task<ActionResult<IReadOnlyList<BeneficioEmpleadoItem>>> DecimoCuarto(CancellationToken cancellationToken) =>
        Ok(await db.EmpleadosDecimoCuarto
            .OrderBy(d => d.Empleado.Persona.Nombre)
            .Select(d => new BeneficioEmpleadoItem(
                d.IdEmpleado, d.Empleado.Persona.Nombre, d.Proyectado, d.Acumulado, d.Pagado, d.Acumulado - d.Pagado, d.UltimoDevengo))
            .ToListAsync(cancellationToken));

    [HttpGet("beneficios/fondos-reserva")]
    public async Task<ActionResult<IReadOnlyList<BeneficioEmpleadoItem>>> FondosReserva(CancellationToken cancellationToken) =>
        Ok(await db.EmpleadosFondosReserva
            .OrderBy(d => d.Empleado.Persona.Nombre)
            .Select(d => new BeneficioEmpleadoItem(
                d.IdEmpleado, d.Empleado.Persona.Nombre, d.Proyectado, d.Acumulado, d.Pagado, d.Acumulado - d.Pagado, d.UltimoDevengo))
            .ToListAsync(cancellationToken));

    [HttpGet("beneficios/provision-vacaciones")]
    public async Task<ActionResult<IReadOnlyList<BeneficioEmpleadoItem>>> ProvisionVacaciones(CancellationToken cancellationToken) =>
        Ok(await db.EmpleadosProvisionVacacion
            .OrderBy(d => d.Empleado.Persona.Nombre)
            .Select(d => new BeneficioEmpleadoItem(
                d.IdEmpleado, d.Empleado.Persona.Nombre, 0m, d.Acumulado, d.Pagado, d.Acumulado - d.Pagado, d.UltimoDevengo))
            .ToListAsync(cancellationToken));

    [HttpPost("beneficios/decimo-tercero/pagar")]
    public async Task<ActionResult<PagoBeneficioResult>> PagarDecimoTercero([FromBody] PagarBeneficioBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        return Ok(await beneficioSocialService.PagarDecimoTerceroAsync(body.IdEmpleado, registradoPor, cancellationToken));
    }

    [HttpPost("beneficios/decimo-cuarto/pagar")]
    public async Task<ActionResult<PagoBeneficioResult>> PagarDecimoCuarto([FromBody] PagarBeneficioBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        return Ok(await beneficioSocialService.PagarDecimoCuartoAsync(body.IdEmpleado, registradoPor, cancellationToken));
    }

    [HttpPost("beneficios/fondos-reserva/pagar")]
    public async Task<ActionResult<PagoBeneficioResult>> PagarFondosReserva([FromBody] PagarBeneficioBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        return Ok(await beneficioSocialService.PagarFondosReservaAsync(body.IdEmpleado, registradoPor, cancellationToken));
    }

    [HttpPost("beneficios/provision-vacaciones/pagar")]
    public async Task<ActionResult<PagoBeneficioResult>> PagarVacaciones([FromBody] PagarBeneficioBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        return Ok(await beneficioSocialService.PagarVacacionesAsync(body.IdEmpleado, registradoPor, cancellationToken));
    }

    // Acción de personal (ver CLAUDE.md "Acción de personal — workflow real")

    [HttpGet("tipos-accion-personal")]
    public async Task<ActionResult<IReadOnlyList<TipoAccionPersonalItem>>> TiposAccionPersonal(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposAccionPersonal
            .Where(t => t.Activo)
            .OrderBy(t => t.Detalle)
            .Select(t => new TipoAccionPersonalItem(
                t.Id, t.Detalle, t.ActivaContrato, t.DesactivaContrato, t.EsCambioCargoSueldo,
                t.EsCambioAgenciaDepartamento, t.EsFormaPagoDecimos, t.EsFormaPagoFondosReserva))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("acciones-personal")]
    public async Task<ActionResult<IReadOnlyList<SolicitudAccionPersonalItem>>> AccionesPersonal(CancellationToken cancellationToken)
    {
        var resultado = await db.SolicitudesAccionPersonal
            .Include(s => s.Empleado).ThenInclude(e => e.Persona)
            .Include(s => s.TipoAccionPersonal)
            .Include(s => s.Etapas)
            .OrderByDescending(s => s.NumeroAccion)
            .Select(s => new SolicitudAccionPersonalItem(
                s.Id, s.NumeroAccion, s.IdEmpleado, s.Empleado.Persona.Nombre, s.IdTipoAccionPersonal, s.TipoAccionPersonal.Detalle,
                s.Detalle, s.CodigoEstado,
                s.CodigoEstado == "IN" ? "Ingresada" : s.CodigoEstado == "AP" ? "Aprobada" : "Anulada",
                s.Fecha, s.IdCargoNuevo, s.NuevoSueldo, s.IdAgenciaNueva, s.NuevoValorRecibeFondosReserva,
                s.Etapas.OrderBy(e => e.Fecha)
                    .Select(e => new SolicitudAccionPersonalEtapaItem(e.CodigoEstado, e.Comentario, e.Fecha, e.RegistradoPor))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("acciones-personal")]
    public async Task<ActionResult<SolicitudAccionPersonalCreadaResult>> CrearAccionPersonal(
        [FromBody] CrearSolicitudAccionPersonalRequest request, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        var resultado = await solicitudAccionPersonalService.CrearAsync(request, registradoPor, cancellationToken);
        return Created($"/api/nomina/acciones-personal/{resultado.Id}", resultado);
    }

    [HttpPost("acciones-personal/{id:guid}/aprobar")]
    public async Task<IActionResult> AprobarAccionPersonal(
        Guid id, [FromBody] AprobarAccionPersonalBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        await solicitudAccionPersonalService.AprobarAsync(id, body.Comentario, registradoPor, cancellationToken);
        return NoContent();
    }

    [HttpPost("acciones-personal/{id:guid}/anular")]
    public async Task<IActionResult> AnularAccionPersonal(
        Guid id, [FromBody] AnularAccionPersonalBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        await solicitudAccionPersonalService.AnularAsync(id, body.Motivo, registradoPor, cancellationToken);
        return NoContent();
    }

    // Datos adicionales reales del empleado (ver CLAUDE.md "Datos
    // adicionales del empleado — cierre real de Nómina")

    [HttpGet("tipos-contrato")]
    public async Task<ActionResult<IReadOnlyList<TipoContratoItem>>> TiposContrato(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposContrato
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoContratoItem(t.Codigo, t.Nombre, t.EsTiempoCompleto, t.EsTiempoParcial, t.TieneFechaSalida))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("empleados/{idEmpleado:guid}/contratos")]
    public async Task<ActionResult<IReadOnlyList<EmpleadoContratoItem>>> Contratos(Guid idEmpleado, CancellationToken cancellationToken)
    {
        var resultado = await db.EmpleadosContrato
            .Where(c => c.IdEmpleado == idEmpleado)
            .OrderByDescending(c => c.FechaIngreso)
            .Select(c => new EmpleadoContratoItem(
                c.Id, c.CodigoTipoContrato, c.TipoContrato.Nombre, c.NumeroContrato, c.FechaIngreso, c.FechaSalida, c.Activo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("empleados/{idEmpleado:guid}/contratos")]
    public async Task<ActionResult<EmpleadoContratoItem>> CrearContrato(
        Guid idEmpleado, [FromBody] CrearEmpleadoContratoBody body, CancellationToken cancellationToken)
    {
        var empleadoExiste = await db.Empleados.AnyAsync(e => e.Id == idEmpleado, cancellationToken);
        if (!empleadoExiste) return NotFound();

        var tipo = await db.TiposContrato.FirstOrDefaultAsync(t => t.Codigo == body.CodigoTipoContrato && t.Activo, cancellationToken);
        if (tipo is null) return BadRequest(new { detail = $"El tipo de contrato '{body.CodigoTipoContrato}' no existe o no está activo." });

        var siguienteNumero = await db.EmpleadosContrato.CountAsync(c => c.IdEmpleado == idEmpleado, cancellationToken) + 1;

        // El contrato anterior activo (si existe) se cierra con fecha de salida al día previo al nuevo ingreso.
        var contratoAnterior = await db.EmpleadosContrato
            .Where(c => c.IdEmpleado == idEmpleado && c.Activo)
            .FirstOrDefaultAsync(cancellationToken);
        if (contratoAnterior is not null)
        {
            contratoAnterior.Activo = false;
            contratoAnterior.FechaSalida ??= body.FechaIngreso.AddDays(-1);
        }

        var contrato = new Corela15.Domain.Nomina.EmpleadoContrato
        {
            Id = Guid.NewGuid(),
            IdEmpleado = idEmpleado,
            CodigoTipoContrato = body.CodigoTipoContrato,
            NumeroContrato = siguienteNumero,
            FechaIngreso = body.FechaIngreso,
            Activo = true,
        };

        db.EmpleadosContrato.Add(contrato);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/nomina/empleados/{idEmpleado}/contratos/{contrato.Id}",
            new EmpleadoContratoItem(contrato.Id, tipo.Codigo, tipo.Nombre, contrato.NumeroContrato, contrato.FechaIngreso, contrato.FechaSalida, contrato.Activo));
    }

    [HttpGet("empleados/{idEmpleado:guid}/datos-adicionales")]
    public async Task<ActionResult<EmpleadoDatosAdicionalesItem>> DatosAdicionales(Guid idEmpleado, CancellationToken cancellationToken)
    {
        var datos = await db.EmpleadosDatosAdicionales.FirstOrDefaultAsync(d => d.IdEmpleado == idEmpleado, cancellationToken);
        if (datos is null) return NotFound();

        return Ok(new EmpleadoDatosAdicionalesItem(
            datos.IdEmpleado, datos.CodigoIess, datos.FechaIngresoIess, datos.FechaSalidaIess,
            datos.FechaIngresoMinisterioLaboral, datos.FechaSalidaMinisterioLaboral, datos.NumeroCargasFamiliares,
            datos.PagoDecimoMensual, datos.PagoFondosReservaRol, datos.ExtensionConyugal));
    }

    [HttpPut("empleados/{idEmpleado:guid}/datos-adicionales")]
    public async Task<ActionResult<EmpleadoDatosAdicionalesItem>> ActualizarDatosAdicionales(
        Guid idEmpleado, [FromBody] ActualizarDatosAdicionalesBody body, CancellationToken cancellationToken)
    {
        var datos = await db.EmpleadosDatosAdicionales.FirstOrDefaultAsync(d => d.IdEmpleado == idEmpleado, cancellationToken);
        if (datos is null) return NotFound();

        datos.CodigoIess = body.CodigoIess;
        datos.FechaIngresoIess = body.FechaIngresoIess;
        datos.FechaSalidaIess = body.FechaSalidaIess;
        datos.FechaIngresoMinisterioLaboral = body.FechaIngresoMinisterioLaboral;
        datos.FechaSalidaMinisterioLaboral = body.FechaSalidaMinisterioLaboral;
        datos.NumeroCargasFamiliares = body.NumeroCargasFamiliares;
        datos.PagoDecimoMensual = body.PagoDecimoMensual;
        datos.PagoFondosReservaRol = body.PagoFondosReservaRol;
        datos.ExtensionConyugal = body.ExtensionConyugal;

        await db.SaveChangesAsync(cancellationToken);

        return Ok(new EmpleadoDatosAdicionalesItem(
            datos.IdEmpleado, datos.CodigoIess, datos.FechaIngresoIess, datos.FechaSalidaIess,
            datos.FechaIngresoMinisterioLaboral, datos.FechaSalidaMinisterioLaboral, datos.NumeroCargasFamiliares,
            datos.PagoDecimoMensual, datos.PagoFondosReservaRol, datos.ExtensionConyugal));
    }

    // Aporte Patronal IESS (ver CLAUDE.md, comparte servicio con los beneficios sociales)

    [HttpGet("beneficios/aporte-patronal")]
    public async Task<ActionResult<IReadOnlyList<BeneficioEmpleadoItem>>> AportePatronal(CancellationToken cancellationToken) =>
        Ok(await db.EmpleadosAportePatronal
            .OrderBy(d => d.Empleado.Persona.Nombre)
            .Select(d => new BeneficioEmpleadoItem(
                d.IdEmpleado, d.Empleado.Persona.Nombre, d.Proyectado, d.Acumulado, d.Pagado, d.Acumulado - d.Pagado, d.UltimoDevengo))
            .ToListAsync(cancellationToken));

    [HttpPost("beneficios/aporte-patronal/pagar")]
    public async Task<ActionResult<PagoBeneficioResult>> PagarAportePatronal([FromBody] PagarBeneficioBody body, CancellationToken cancellationToken)
    {
        var registradoPor = User.Identity?.Name ?? "sistema";
        return Ok(await beneficioSocialService.PagarAportePatronalAsync(body.IdEmpleado, registradoPor, cancellationToken));
    }

    // Impuesto a la Renta — relación de dependencia (reporte real de solo cálculo)

    [HttpPost("empleados/{idEmpleado:guid}/impuesto-renta/calcular")]
    public async Task<ActionResult<CalculoImpuestoRentaResult>> CalcularImpuestoRenta(Guid idEmpleado, CancellationToken cancellationToken) =>
        Ok(await calculoImpuestoRentaService.CalcularAsync(idEmpleado, cancellationToken));

    [HttpGet("empleados/{idEmpleado:guid}/impuesto-renta/historial")]
    public async Task<ActionResult<IReadOnlyList<CalculoImpuestoRentaResult>>> HistorialImpuestoRenta(
        Guid idEmpleado, CancellationToken cancellationToken) =>
        Ok(await calculoImpuestoRentaService.HistorialAsync(idEmpleado, cancellationToken));
}
