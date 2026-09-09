using Corela15.Application.Seguridad;
using Corela15.Domain.Seguridad;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record UsuarioListItem(
    Guid Id, string NombreUsuario, string? NombrePersona, string? Email,
    string Agencia, bool Activo, bool TieneBloqueo, IReadOnlyList<string> Roles);

public record ConfigurarBloqueoBody(bool Bloquear);

public record CrearUsuarioBody(
    string NombreUsuario, string ContrasenaInicial, int IdAgencia, Guid? IdPersona,
    bool UsaDispositivoMovil, bool PermiteRiesgoOperativo, bool PermiteConsultaEmpleados,
    bool ValidaIp, bool CambiaClave, int? DiasCambioClave);

public record ActualizarUsuarioBody(
    int IdAgencia, Guid? IdPersona, bool PuedeIngresarSistema,
    bool UsaDispositivoMovil, bool PermiteRiesgoOperativo, bool PermiteConsultaEmpleados,
    bool ValidaIp, bool CambiaClave, int? DiasCambioClave);

public record UsuarioDetalleDto(
    Guid Id, string NombreUsuario, string? NombrePersona, string? Email, string? CodigoUsuarioSoftbank,
    int IdAgencia, string Agencia,
    bool PuedeIngresarSistema, bool TieneBloqueo, bool UsaDispositivoMovil, bool PermiteRiesgoOperativo,
    bool PermiteConsultaEmpleados, bool ValidaIp, bool CambiaClave, int? DiasCambioClave,
    string? CodigoAreaPlanificacion, string? AreaPlanificacion);

public record ConfigurarAreaPlanificacionBody(string? CodigoAreaPlanificacion);

public record HorarioAccesoDto(Guid Id, int DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, bool EsReceso, bool Activo);

public record CrearHorarioAccesoBody(int DiaSemana, TimeOnly HoraInicio, TimeOnly HoraFin, bool EsReceso);

public record RolTemporalDto(Guid Id, int IdRol, string Rol, DateTimeOffset FechaCaducidad, bool Vigente);

public record AsignarRolTemporalBody(int IdRol, DateTimeOffset FechaCaducidad);

public record AgenciaTemporalDto(Guid Id, int IdAgenciaOrigen, string AgenciaOrigen, int IdAgenciaActual, string AgenciaActual, DateTimeOffset FechaCaducidad, bool Vigente);

public record AsignarAgenciaTemporalBody(int IdAgenciaActual, DateTimeOffset FechaCaducidad);

public record ResetearClaveBody(string ClaveNueva);

public record RolAsignadoDto(int Id, string Nombre, bool Asignado);

public record ActualizarRolesUsuarioRequest(IReadOnlyList<int> IdsRol);

public record MenuUsuarioDto(int IdMenu, string Codigo, string Nombre, bool OtorgadoPorRol, bool OtorgadoDirecto);
public record ActualizarMenusUsuarioRequest(IReadOnlyList<int> IdsMenu);
public record DatasetUsuarioDto(string Codigo, string Nombre, bool OtorgadoPorRol, bool OtorgadoDirecto);
public record ActualizarDatasetsUsuarioRequest(IReadOnlyList<string> CodigosDataset);

public record OpcionUsuarioDto(string Codigo, string Nombre, string CodigoMenu, string NombreMenu, bool OtorgadoPorRol, bool OtorgadoDirecto);
public record ActualizarOpcionesUsuarioRequest(IReadOnlyList<string> CodigosOpcion);

[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = "Menu:usuarios-roles")]
public class UsuariosController(Corela15DbContext db, IAuthService authService) : ControllerBase
{
    // Alta de usuario nuevo — antes solo existían los 2 usuarios sembrados
    // por Nivel0_PasswordHashReal, sin ninguna forma real de crear uno.
    [HttpPost]
    public async Task<ActionResult<object>> Crear([FromBody] CrearUsuarioBody body, CancellationToken cancellationToken)
    {
        var id = await authService.CrearUsuarioAsync(
            new CrearUsuarioRequest(
                body.NombreUsuario, body.ContrasenaInicial, body.IdAgencia, body.IdPersona,
                body.UsaDispositivoMovil, body.PermiteRiesgoOperativo, body.PermiteConsultaEmpleados,
                body.ValidaIp, body.CambiaClave, body.DiasCambioClave),
            User.Identity!.Name!, cancellationToken);
        return Created($"/api/usuarios/{id}", new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarUsuarioBody body, CancellationToken cancellationToken)
    {
        await authService.ActualizarUsuarioAsync(
            id, new ActualizarUsuarioRequest(
                body.IdAgencia, body.IdPersona, body.PuedeIngresarSistema,
                body.UsaDispositivoMovil, body.PermiteRiesgoOperativo, body.PermiteConsultaEmpleados,
                body.ValidaIp, body.CambiaClave, body.DiasCambioClave),
            User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    // Vista de detalle completa (para el modal de edición) — antes el
    // formulario de edición solo mandaba agencia/persona/acceso a ciegas,
    // sin poder ver primero el estado real de los toggles.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioDetalleDto>> Detalle(Guid id, CancellationToken cancellationToken)
    {
        var u = await db.Usuarios.Include(x => x.Persona).Include(x => x.Agencia).Include(x => x.AreaPlanificacion)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (u is null) return NotFound();

        return Ok(new UsuarioDetalleDto(
            u.Id, u.NombreUsuario, u.Persona?.Nombre ?? u.NombreCompleto, u.Email, u.CodigoUsuarioSoftbank,
            u.IdAgencia, u.Agencia.Nombre,
            u.PuedeIngresarSistema, u.TieneBloqueo, u.UsaDispositivoMovil, u.PermiteRiesgoOperativo,
            u.PermiteConsultaEmpleados, u.ValidaIp, u.CambiaClave, u.DiasCambioClave,
            u.CodigoAreaPlanificacion, u.AreaPlanificacion?.Nombre));
    }

    // Área real de planificación (ver Corela15.Domain.Planificacion) --
    // quién puede cargar/ver el plan semanal de qué área. Flag simple sin
    // invariante de negocio más allá de que el área exista y esté activa,
    // mismo patrón directo-contra-DbContext que AcreditaPrestamo/DebitoSpi.
    [HttpPatch("{id:guid}/area-planificacion")]
    public async Task<IActionResult> ConfigurarAreaPlanificacion(
        Guid id, [FromBody] ConfigurarAreaPlanificacionBody body, CancellationToken cancellationToken)
    {
        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (u is null) return NotFound();

        if (body.CodigoAreaPlanificacion is not null &&
            !await db.AreasPlanificacion.AnyAsync(a => a.Codigo == body.CodigoAreaPlanificacion && a.Activo, cancellationToken))
        {
            return BadRequest("El área de planificación no existe o no está activa");
        }

        u.CodigoAreaPlanificacion = body.CodigoAreaPlanificacion;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Horarios reales de acceso (ver HorarioAccesoUsuario.cs) — aplicados de
    // verdad en AuthService.LoginAsync, no solo datos decorativos. CRUD
    // directo contra el DbContext: sin invariante de negocio más allá de
    // existencia del usuario.
    [HttpGet("{id:guid}/horarios")]
    public async Task<ActionResult<IReadOnlyList<HorarioAccesoDto>>> Horarios(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await db.HorariosAccesoUsuario
            .Where(h => h.IdUsuario == id)
            .OrderBy(h => h.DiaSemana).ThenBy(h => h.EsReceso).ThenBy(h => h.HoraInicio)
            .Select(h => new HorarioAccesoDto(h.Id, h.DiaSemana, h.HoraInicio, h.HoraFin, h.EsReceso, h.Activo))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/horarios")]
    public async Task<ActionResult<HorarioAccesoDto>> AgregarHorario(
        Guid id, [FromBody] CrearHorarioAccesoBody body, CancellationToken cancellationToken)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == id, cancellationToken)) return NotFound();
        if (body.DiaSemana is < 1 or > 7) return BadRequest("DiaSemana debe estar entre 1 (lunes) y 7 (domingo)");
        if (body.HoraFin <= body.HoraInicio) return BadRequest("HoraFin debe ser posterior a HoraInicio");

        var horario = new HorarioAccesoUsuario
        {
            Id = Guid.NewGuid(), IdUsuario = id, DiaSemana = body.DiaSemana,
            HoraInicio = body.HoraInicio, HoraFin = body.HoraFin, EsReceso = body.EsReceso, Activo = true,
        };
        db.HorariosAccesoUsuario.Add(horario);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/usuarios/{id}/horarios/{horario.Id}",
            new HorarioAccesoDto(horario.Id, horario.DiaSemana, horario.HoraInicio, horario.HoraFin, horario.EsReceso, true));
    }

    [HttpDelete("{id:guid}/horarios/{idHorario:guid}")]
    public async Task<IActionResult> QuitarHorario(Guid id, Guid idHorario, CancellationToken cancellationToken)
    {
        var horario = await db.HorariosAccesoUsuario.FirstOrDefaultAsync(h => h.Id == idHorario && h.IdUsuario == id, cancellationToken);
        if (horario is null) return NotFound();
        horario.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Rol temporal (licencia/vacaciones/reemplazo) — se suma a los roles
    // permanentes al calcular permisos en el próximo login, ver
    // AuthService.LoginAsync. Nunca reemplaza al rol permanente, solo agrega.
    [HttpGet("{id:guid}/roles-temporales")]
    public async Task<ActionResult<IReadOnlyList<RolTemporalDto>>> RolesTemporales(Guid id, CancellationToken cancellationToken)
    {
        var ahora = DateTimeOffset.UtcNow;
        var resultado = await db.UsuariosRolTemporal
            .Include(rt => rt.Rol)
            .Where(rt => rt.IdUsuario == id && rt.Activo)
            .OrderByDescending(rt => rt.FechaCaducidad)
            .Select(rt => new RolTemporalDto(rt.Id, rt.IdRol, rt.Rol.Nombre, rt.FechaCaducidad, rt.FechaCaducidad > ahora))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/roles-temporales")]
    public async Task<ActionResult<RolTemporalDto>> AsignarRolTemporal(
        Guid id, [FromBody] AsignarRolTemporalBody body, CancellationToken cancellationToken)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == id, cancellationToken)) return NotFound();
        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == body.IdRol, cancellationToken);
        if (rol is null) return BadRequest("El rol no existe");
        if (body.FechaCaducidad <= DateTimeOffset.UtcNow) return BadRequest("FechaCaducidad debe ser futura");

        var rolTemporal = new UsuarioRolTemporal
        {
            Id = Guid.NewGuid(), IdUsuario = id, IdRol = body.IdRol, FechaCaducidad = body.FechaCaducidad,
            CreadoEn = DateTimeOffset.UtcNow, CreadoPor = User.Identity!.Name!, Activo = true,
        };
        db.UsuariosRolTemporal.Add(rolTemporal);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/usuarios/{id}/roles-temporales/{rolTemporal.Id}",
            new RolTemporalDto(rolTemporal.Id, rolTemporal.IdRol, rol.Nombre, rolTemporal.FechaCaducidad, true));
    }

    [HttpDelete("{id:guid}/roles-temporales/{idRolTemporal:guid}")]
    public async Task<IActionResult> QuitarRolTemporal(Guid id, Guid idRolTemporal, CancellationToken cancellationToken)
    {
        var rt = await db.UsuariosRolTemporal.FirstOrDefaultAsync(x => x.Id == idRolTemporal && x.IdUsuario == id, cancellationToken);
        if (rt is null) return NotFound();
        rt.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Agencia temporal (cubrir a un cajero de otra sucursal) — solo una
    // vigente a la vez tiene sentido real, así que asignar una nueva
    // desactiva cualquier otra activa de este usuario.
    [HttpGet("{id:guid}/agencia-temporal")]
    public async Task<ActionResult<AgenciaTemporalDto?>> AgenciaTemporal(Guid id, CancellationToken cancellationToken)
    {
        var ahora = DateTimeOffset.UtcNow;
        var resultado = await db.UsuariosAgenciaTemporal
            .Include(at => at.AgenciaOrigen).Include(at => at.AgenciaActual)
            .Where(at => at.IdUsuario == id && at.Activo)
            .OrderByDescending(at => at.FechaCaducidad)
            .Select(at => new AgenciaTemporalDto(
                at.Id, at.IdAgenciaOrigen, at.AgenciaOrigen.Nombre, at.IdAgenciaActual, at.AgenciaActual.Nombre,
                at.FechaCaducidad, at.FechaCaducidad > ahora))
            .FirstOrDefaultAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/agencia-temporal")]
    public async Task<ActionResult<AgenciaTemporalDto>> AsignarAgenciaTemporal(
        Guid id, [FromBody] AsignarAgenciaTemporalBody body, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null) return NotFound();
        var agenciaActual = await db.Agencias.FirstOrDefaultAsync(a => a.Id == body.IdAgenciaActual && a.Activa, cancellationToken);
        if (agenciaActual is null) return BadRequest("La agencia no existe o no está activa");
        if (body.FechaCaducidad <= DateTimeOffset.UtcNow) return BadRequest("FechaCaducidad debe ser futura");

        var existentes = await db.UsuariosAgenciaTemporal.Where(at => at.IdUsuario == id && at.Activo).ToListAsync(cancellationToken);
        foreach (var e in existentes) e.Activo = false;

        var nueva = new UsuarioAgenciaTemporal
        {
            Id = Guid.NewGuid(), IdUsuario = id, IdAgenciaOrigen = usuario.IdAgencia, IdAgenciaActual = body.IdAgenciaActual,
            FechaCaducidad = body.FechaCaducidad, CreadoEn = DateTimeOffset.UtcNow, CreadoPor = User.Identity!.Name!, Activo = true,
        };
        db.UsuariosAgenciaTemporal.Add(nueva);
        await db.SaveChangesAsync(cancellationToken);

        var agenciaOrigenNombre = await db.Agencias.Where(a => a.Id == usuario.IdAgencia).Select(a => a.Nombre).FirstAsync(cancellationToken);
        return Created($"/api/usuarios/{id}/agencia-temporal/{nueva.Id}",
            new AgenciaTemporalDto(nueva.Id, nueva.IdAgenciaOrigen, agenciaOrigenNombre, nueva.IdAgenciaActual, agenciaActual.Nombre, nueva.FechaCaducidad, true));
    }

    [HttpDelete("{id:guid}/agencia-temporal/{idAgenciaTemporal:guid}")]
    public async Task<IActionResult> QuitarAgenciaTemporal(Guid id, Guid idAgenciaTemporal, CancellationToken cancellationToken)
    {
        var at = await db.UsuariosAgenciaTemporal.FirstOrDefaultAsync(x => x.Id == idAgenciaTemporal && x.IdUsuario == id, cancellationToken);
        if (at is null) return NotFound();
        at.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Reseteo de contraseña por un administrador — distinto de
    // POST /api/auth/cambiar-clave (self-service). Revoca sesiones activas
    // del usuario afectado, mismo criterio que bloqueo/cambio de clave propio.
    [HttpPost("{id:guid}/resetear-clave")]
    public async Task<IActionResult> ResetearClave(Guid id, [FromBody] ResetearClaveBody body, CancellationToken cancellationToken)
    {
        await authService.ResetearClaveAsync(id, body.ClaveNueva, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    // Roles por usuario (seguridad.usuario_rol) — sin invariante de negocio
    // más allá de existencia, mismo patrón directo-contra-DbContext y mismo
    // "reconciliar la lista completa" que ConfiguracionController usa para
    // menús por rol. Antes solo se podía tocar por migración/SQL.
    [HttpGet("{id:guid}/roles")]
    public async Task<ActionResult<IReadOnlyList<RolAsignadoDto>>> RolesDelUsuario(Guid id, CancellationToken cancellationToken)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == id, cancellationToken))
        {
            return NotFound();
        }

        var asignados = await db.UsuarioRoles
            .Where(ur => ur.IdUsuario == id && ur.Activo)
            .Select(ur => ur.IdRol)
            .ToListAsync(cancellationToken);

        var resultado = await db.Roles
            .OrderBy(r => r.Nombre)
            .Select(r => new RolAsignadoDto(r.Id, r.Nombre, asignados.Contains(r.Id)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> ActualizarRolesDelUsuario(
        Guid id, [FromBody] ActualizarRolesUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == id, cancellationToken))
        {
            return NotFound();
        }

        var existentes = await db.UsuarioRoles.Where(ur => ur.IdUsuario == id).ToListAsync(cancellationToken);
        foreach (var existente in existentes)
        {
            existente.Activo = request.IdsRol.Contains(existente.IdRol);
        }

        var idsExistentes = existentes.Select(e => e.IdRol).ToHashSet();
        foreach (var idRol in request.IdsRol.Where(idRol => !idsExistentes.Contains(idRol)))
        {
            db.UsuarioRoles.Add(new Corela15.Domain.Seguridad.UsuarioRol
            {
                IdUsuario = id, IdRol = idRol, Activo = true, AsignadoEn = DateTimeOffset.UtcNow,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ---- Permisos directos por usuario (menús y datasets de Reportería
    // Gerencial) — segundo camino real, además de los roles: permite darle
    // acceso a un módulo puntual (o un dataset puntual) a una sola persona
    // sin crear ni tocar un rol para eso. Nunca resta lo que el rol ya
    // otorga (ver UsuarioMenu.cs/AuthService.EmitirTokenAsync, es una
    // unión) — el PUT solo reconcilia la lista de otorgamientos DIRECTOS;
    // "OtorgadoPorRol" en el GET es puramente informativo, para que quien
    // administra vea de dónde viene cada acceso real antes de tocar nada.

    [HttpGet("{id:guid}/menus")]
    public async Task<ActionResult<IReadOnlyList<MenuUsuarioDto>>> MenusDelUsuario(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null) return NotFound();

        var idsRol = await db.UsuarioRoles.Where(ur => ur.IdUsuario == id && ur.Activo).Select(ur => ur.IdRol).ToListAsync(cancellationToken);
        var menusPorRol = await db.RolesMenu.Where(rm => rm.Activo && idsRol.Contains(rm.IdRol)).Select(rm => rm.IdMenu).ToListAsync(cancellationToken);
        var menusDirectos = await db.UsuariosMenu.Where(um => um.IdUsuario == id && um.Activo).Select(um => um.IdMenu).ToListAsync(cancellationToken);

        var resultado = await db.Menus
            .Where(m => m.Activo)
            .OrderBy(m => m.Orden)
            .Select(m => new MenuUsuarioDto(m.Id, m.Codigo, m.Nombre, menusPorRol.Contains(m.Id), menusDirectos.Contains(m.Id)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}/menus")]
    public async Task<IActionResult> ActualizarMenusDelUsuario(
        Guid id, [FromBody] ActualizarMenusUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == id, cancellationToken)) return NotFound();

        var existentes = await db.UsuariosMenu.Where(um => um.IdUsuario == id).ToListAsync(cancellationToken);
        foreach (var existente in existentes)
        {
            existente.Activo = request.IdsMenu.Contains(existente.IdMenu);
        }

        var idsExistentes = existentes.Select(e => e.IdMenu).ToHashSet();
        foreach (var idMenu in request.IdsMenu.Where(idMenu => !idsExistentes.Contains(idMenu)))
        {
            db.UsuariosMenu.Add(new Corela15.Domain.Seguridad.UsuarioMenu { IdUsuario = id, IdMenu = idMenu, Activo = true });
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/datasets-reporteria")]
    public async Task<ActionResult<IReadOnlyList<DatasetUsuarioDto>>> DatasetsReporteriaDelUsuario(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null) return NotFound();

        var idsRol = await db.UsuarioRoles.Where(ur => ur.IdUsuario == id && ur.Activo).Select(ur => ur.IdRol).ToListAsync(cancellationToken);
        var datasetsPorRol = await db.RolesDatasetReporteria.Where(rd => rd.Activo && idsRol.Contains(rd.IdRol)).Select(rd => rd.CodigoDataset).ToListAsync(cancellationToken);
        var datasetsDirectos = await db.UsuariosDatasetReporteria.Where(ud => ud.IdUsuario == id && ud.Activo).Select(ud => ud.CodigoDataset).ToListAsync(cancellationToken);

        var resultado = await db.DatasetsReporteria
            .Where(d => d.Activo)
            .OrderBy(d => d.Nombre)
            .Select(d => new DatasetUsuarioDto(d.Codigo, d.Nombre, datasetsPorRol.Contains(d.Codigo), datasetsDirectos.Contains(d.Codigo)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}/datasets-reporteria")]
    public async Task<IActionResult> ActualizarDatasetsReporteriaDelUsuario(
        Guid id, [FromBody] ActualizarDatasetsUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == id, cancellationToken)) return NotFound();

        var existentes = await db.UsuariosDatasetReporteria.Where(ud => ud.IdUsuario == id).ToListAsync(cancellationToken);
        foreach (var existente in existentes)
        {
            existente.Activo = request.CodigosDataset.Contains(existente.CodigoDataset);
        }

        var codigosExistentes = existentes.Select(e => e.CodigoDataset).ToHashSet();
        foreach (var codigo in request.CodigosDataset.Where(c => !codigosExistentes.Contains(c)))
        {
            db.UsuariosDatasetReporteria.Add(new Corela15.Domain.Seguridad.UsuarioDatasetReporteria { IdUsuario = id, CodigoDataset = codigo, Activo = true });
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/opciones")]
    public async Task<ActionResult<IReadOnlyList<OpcionUsuarioDto>>> OpcionesDelUsuario(Guid id, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null) return NotFound();

        var idsRol = await db.UsuarioRoles.Where(ur => ur.IdUsuario == id && ur.Activo).Select(ur => ur.IdRol).ToListAsync(cancellationToken);
        var opcionesPorRol = await db.RolesOpcion.Where(ro => ro.Activo && idsRol.Contains(ro.IdRol)).Select(ro => ro.CodigoOpcion).ToListAsync(cancellationToken);
        var opcionesDirectas = await db.UsuariosOpcion.Where(uo => uo.IdUsuario == id && uo.Activo).Select(uo => uo.CodigoOpcion).ToListAsync(cancellationToken);

        var resultado = await db.Opciones
            .Where(o => o.Activo)
            .OrderBy(o => o.CodigoMenu).ThenBy(o => o.Nombre)
            .Select(o => new OpcionUsuarioDto(o.Codigo, o.Nombre, o.CodigoMenu, o.Menu.Nombre, opcionesPorRol.Contains(o.Codigo), opcionesDirectas.Contains(o.Codigo)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPut("{id:guid}/opciones")]
    public async Task<IActionResult> ActualizarOpcionesDelUsuario(
        Guid id, [FromBody] ActualizarOpcionesUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == id, cancellationToken)) return NotFound();

        var existentes = await db.UsuariosOpcion.Where(uo => uo.IdUsuario == id).ToListAsync(cancellationToken);
        foreach (var existente in existentes)
        {
            existente.Activo = request.CodigosOpcion.Contains(existente.CodigoOpcion);
        }

        var codigosExistentes = existentes.Select(e => e.CodigoOpcion).ToHashSet();
        foreach (var codigo in request.CodigosOpcion.Where(c => !codigosExistentes.Contains(c)))
        {
            db.UsuariosOpcion.Add(new Corela15.Domain.Seguridad.UsuarioOpcion { IdUsuario = id, CodigoOpcion = codigo, Activo = true });
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioListItem>>> Listar(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Agencia)
            .Include(u => u.UsuarioRoles.Where(ur => ur.Activo))
                .ThenInclude(ur => ur.Rol)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.NombreUsuario, $"%{q}%") ||
                EF.Functions.ILike(u.NombreCompleto ?? "", $"%{q}%") ||
                (u.Persona != null && EF.Functions.ILike(u.Persona.Nombre, $"%{q}%")));
        }

        var resultado = await query
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioListItem(
                u.Id, u.NombreUsuario, u.Persona != null ? u.Persona.Nombre : u.NombreCompleto, u.Email,
                u.Agencia.Nombre, u.Activo, u.TieneBloqueo,
                u.UsuarioRoles.Where(ur => ur.Activo).Select(ur => ur.Rol.Nombre).ToList()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("{id:guid}/sesiones")]
    public async Task<ActionResult<IReadOnlyList<SesionUsuarioResult>>> Sesiones(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await authService.ListarSesionesAsync(id, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/sesiones/revocar-todas")]
    public async Task<IActionResult> RevocarTodasLasSesiones(Guid id, CancellationToken cancellationToken)
    {
        await authService.RevocarTodasLasSesionesAsync(id, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/bloqueo")]
    public async Task<IActionResult> ConfigurarBloqueo(
        Guid id, [FromBody] ConfigurarBloqueoBody body, CancellationToken cancellationToken)
    {
        await authService.ConfigurarBloqueoAsync(id, body.Bloquear, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }
}
