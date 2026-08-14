using Corela15.Application.Colocacion;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Credito;
using Corela15.Domain.General;
using Corela15.Domain.Inversion;
using Corela15.Domain.Seguridad;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

// Catálogos de configuración de Nivel 0 (general.* y seguridad.rol) — CRUD
// simple sin invariantes de negocio más allá de código/nombre único, por
// eso se resuelve directo contra el DbContext acá en vez de una capa
// Application con un servicio por catálogo que solo reenviaría la llamada
// sin agregar lógica real. Los casos de uso con reglas de negocio de verdad
// (comprobantes, préstamos, ventanillas...) siguen yendo por Application.

public record PaisDto(int Id, string Codigo, string Nombre);
public record CrearPaisRequest(string Codigo, string Nombre);

public record MonedaDto(int Id, string Codigo, string Nombre, string Simbolo);
public record CrearMonedaRequest(string Codigo, string Nombre, string Simbolo);

public record TipoIdentificacionDto(int Id, string Codigo, string Nombre);
public record CrearTipoIdentificacionRequest(string Codigo, string Nombre);

public record AgenciaDto(int Id, int IdEmpresa, string Codigo, string Nombre, bool EsOperativa, bool Activa);
public record CrearAgenciaRequest(int IdEmpresa, string Codigo, string Nombre, bool EsOperativa);
public record ActualizarAgenciaRequest(string Nombre, bool EsOperativa, bool Activa);

public record EmpresaDto(int Id, string Codigo, string Nombre, string Ruc, int IdMoneda, int IdPais);
public record ActualizarEmpresaRequest(string Nombre, string Ruc, int IdMoneda, int IdPais);

public record RolDto(int Id, string Nombre, int Nivel);
public record CrearRolRequest(string Nombre, int Nivel);
public record ActualizarRolRequest(string Nombre, int Nivel);

public record MenuAsignadoDto(int IdMenu, string Codigo, string Nombre, bool Asignado);
public record ActualizarRolMenuRequest(List<int> IdsMenu);

public record CuentaContablePlanDto(
    Guid Id, string Codigo, string Nombre, string Grupo, string Naturaleza,
    bool EsMayor, bool Activa, string? CodigoPadre);
public record CrearCuentaContablePlanRequest(
    string Codigo, string Nombre, string Grupo, string Naturaleza, Guid? IdCuentaPadre, bool EsMayor);
public record ActualizarCuentaContablePlanRequest(string Nombre, bool Activa);

public record TipoComprobanteContableDto(int Id, string Codigo, string Nombre);
public record CrearTipoComprobanteContableRequest(string Codigo, string Nombre);

public record TipoCuentaDto(
    int Id, string Codigo, string Nombre, decimal SaldoMinimo, bool PermiteDebitoPrestamo,
    decimal? SaldoMinimoConPrestamo, decimal TasaInteresAnual, bool Activo);
public record CrearTipoCuentaRequest(
    string Codigo, string Nombre, decimal SaldoMinimo, bool PermiteDebitoPrestamo,
    decimal? SaldoMinimoConPrestamo, decimal TasaInteresAnual);
public record ActualizarTipoCuentaRequest(
    string Nombre, decimal SaldoMinimo, bool PermiteDebitoPrestamo,
    decimal? SaldoMinimoConPrestamo, decimal TasaInteresAnual, bool Activo);

public record TipoPrestamoDto(
    int Id, string Codigo, string Nombre, decimal MontoMinimo, decimal MontoMaximo,
    int PlazoMinimoDias, int PlazoMaximoDias, decimal TasaAnual, string SegmentoBce, bool Activo);

public record TasaTechoBceDto(int Id, string Segmento, decimal TasaMaxima, DateOnly FechaVigenciaDesde, bool Activo);
public record CrearTasaTechoBceRequest(string Segmento, decimal TasaMaxima, DateOnly FechaVigenciaDesde);
public record ActualizarTasaTechoBceRequest(decimal TasaMaxima, bool Activo);

public record ItemPlazoTasaDto(
    Guid Id, int PlazoDiasMin, int PlazoDiasMax, decimal MontoMin, decimal? MontoMax,
    string TipoPersona, decimal Tasa, DateOnly FechaVigenciaDesde, bool Activo);
public record CrearItemPlazoTasaRequest(
    int PlazoDiasMin, int PlazoDiasMax, decimal MontoMin, decimal? MontoMax,
    string TipoPersona, decimal Tasa, DateOnly FechaVigenciaDesde);
public record ActualizarItemPlazoTasaRequest(decimal Tasa, bool Activo);

public record CategoriaRiesgoCarteraDto(
    int Id, string Codigo, string Nombre, int DiasMoraInicio, int DiasMoraFin, decimal PorcentajeProvision, bool Activo);
public record ActualizarCategoriaRiesgoCarteraRequest(int DiasMoraInicio, int DiasMoraFin, decimal PorcentajeProvision, bool Activo);

[ApiController]
[Route("api/configuracion")]
[Authorize(Policy = "Menu:configuracion")]
public class ConfiguracionController(
    Corela15DbContext db, ICuentaContableAdminService cuentaContableAdmin, ITipoPrestamoAdminService tipoPrestamoAdmin) : ControllerBase
{
    // ---- Países ----

    [HttpGet("paises")]
    public async Task<ActionResult<IReadOnlyList<PaisDto>>> Paises(CancellationToken ct)
    {
        var resultado = await db.Paises.OrderBy(p => p.Nombre)
            .Select(p => new PaisDto(p.Id, p.Codigo, p.Nombre)).ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("paises")]
    public async Task<ActionResult<PaisDto>> CrearPais([FromBody] CrearPaisRequest request, CancellationToken ct)
    {
        if (await db.Paises.AnyAsync(p => p.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("un país", request.Codigo);
        }

        var pais = new Pais { Codigo = request.Codigo, Nombre = request.Nombre };
        db.Paises.Add(pais);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/paises/{pais.Id}", new PaisDto(pais.Id, pais.Codigo, pais.Nombre));
    }

    [HttpPut("paises/{id:int}")]
    public async Task<ActionResult<PaisDto>> ActualizarPais(int id, [FromBody] CrearPaisRequest request, CancellationToken ct)
    {
        var pais = await db.Paises.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pais is null) return NotFound();

        if (await db.Paises.AnyAsync(p => p.Codigo == request.Codigo && p.Id != id, ct))
        {
            throw new CodigoDuplicadoException("un país", request.Codigo);
        }

        pais.Codigo = request.Codigo;
        pais.Nombre = request.Nombre;
        await db.SaveChangesAsync(ct);
        return Ok(new PaisDto(pais.Id, pais.Codigo, pais.Nombre));
    }

    // ---- Monedas ----

    [HttpGet("monedas")]
    public async Task<ActionResult<IReadOnlyList<MonedaDto>>> Monedas(CancellationToken ct)
    {
        var resultado = await db.Monedas.OrderBy(m => m.Nombre)
            .Select(m => new MonedaDto(m.Id, m.Codigo, m.Nombre, m.Simbolo)).ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("monedas")]
    public async Task<ActionResult<MonedaDto>> CrearMoneda([FromBody] CrearMonedaRequest request, CancellationToken ct)
    {
        if (await db.Monedas.AnyAsync(m => m.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("una moneda", request.Codigo);
        }

        var moneda = new Moneda { Codigo = request.Codigo, Nombre = request.Nombre, Simbolo = request.Simbolo };
        db.Monedas.Add(moneda);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/monedas/{moneda.Id}", new MonedaDto(moneda.Id, moneda.Codigo, moneda.Nombre, moneda.Simbolo));
    }

    [HttpPut("monedas/{id:int}")]
    public async Task<ActionResult<MonedaDto>> ActualizarMoneda(int id, [FromBody] CrearMonedaRequest request, CancellationToken ct)
    {
        var moneda = await db.Monedas.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (moneda is null) return NotFound();

        if (await db.Monedas.AnyAsync(m => m.Codigo == request.Codigo && m.Id != id, ct))
        {
            throw new CodigoDuplicadoException("una moneda", request.Codigo);
        }

        moneda.Codigo = request.Codigo;
        moneda.Nombre = request.Nombre;
        moneda.Simbolo = request.Simbolo;
        await db.SaveChangesAsync(ct);
        return Ok(new MonedaDto(moneda.Id, moneda.Codigo, moneda.Nombre, moneda.Simbolo));
    }

    // ---- Tipos de identificación ----

    [HttpGet("tipos-identificacion")]
    public async Task<ActionResult<IReadOnlyList<TipoIdentificacionDto>>> TiposIdentificacion(CancellationToken ct)
    {
        var resultado = await db.TiposIdentificacion.OrderBy(t => t.Nombre)
            .Select(t => new TipoIdentificacionDto(t.Id, t.Codigo, t.Nombre)).ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("tipos-identificacion")]
    public async Task<ActionResult<TipoIdentificacionDto>> CrearTipoIdentificacion(
        [FromBody] CrearTipoIdentificacionRequest request, CancellationToken ct)
    {
        if (await db.TiposIdentificacion.AnyAsync(t => t.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("un tipo de identificación", request.Codigo);
        }

        var tipo = new TipoIdentificacion { Codigo = request.Codigo, Nombre = request.Nombre };
        db.TiposIdentificacion.Add(tipo);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tipos-identificacion/{tipo.Id}", new TipoIdentificacionDto(tipo.Id, tipo.Codigo, tipo.Nombre));
    }

    [HttpPut("tipos-identificacion/{id:int}")]
    public async Task<ActionResult<TipoIdentificacionDto>> ActualizarTipoIdentificacion(
        int id, [FromBody] CrearTipoIdentificacionRequest request, CancellationToken ct)
    {
        var tipo = await db.TiposIdentificacion.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tipo is null) return NotFound();

        if (await db.TiposIdentificacion.AnyAsync(t => t.Codigo == request.Codigo && t.Id != id, ct))
        {
            throw new CodigoDuplicadoException("un tipo de identificación", request.Codigo);
        }

        tipo.Codigo = request.Codigo;
        tipo.Nombre = request.Nombre;
        await db.SaveChangesAsync(ct);
        return Ok(new TipoIdentificacionDto(tipo.Id, tipo.Codigo, tipo.Nombre));
    }

    // ---- Agencias ----

    [HttpGet("agencias")]
    public async Task<ActionResult<IReadOnlyList<AgenciaDto>>> Agencias(CancellationToken ct)
    {
        var resultado = await db.Agencias.OrderBy(a => a.Nombre)
            .Select(a => new AgenciaDto(a.Id, a.IdEmpresa, a.Codigo, a.Nombre, a.EsOperativa, a.Activa)).ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("agencias")]
    public async Task<ActionResult<AgenciaDto>> CrearAgencia([FromBody] CrearAgenciaRequest request, CancellationToken ct)
    {
        if (await db.Agencias.AnyAsync(a => a.IdEmpresa == request.IdEmpresa && a.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("una agencia", request.Codigo);
        }

        var agencia = new Agencia
        {
            IdEmpresa = request.IdEmpresa,
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            EsOperativa = request.EsOperativa,
            Activa = true,
        };
        db.Agencias.Add(agencia);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/agencias/{agencia.Id}",
            new AgenciaDto(agencia.Id, agencia.IdEmpresa, agencia.Codigo, agencia.Nombre, agencia.EsOperativa, agencia.Activa));
    }

    [HttpPut("agencias/{id:int}")]
    public async Task<ActionResult<AgenciaDto>> ActualizarAgencia(int id, [FromBody] ActualizarAgenciaRequest request, CancellationToken ct)
    {
        var agencia = await db.Agencias.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agencia is null) return NotFound();

        agencia.Nombre = request.Nombre;
        agencia.EsOperativa = request.EsOperativa;
        agencia.Activa = request.Activa;
        await db.SaveChangesAsync(ct);
        return Ok(new AgenciaDto(agencia.Id, agencia.IdEmpresa, agencia.Codigo, agencia.Nombre, agencia.EsOperativa, agencia.Activa));
    }

    // ---- Empresa (fila única) ----

    [HttpGet("empresa")]
    public async Task<ActionResult<EmpresaDto>> Empresa(CancellationToken ct)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(ct);
        if (empresa is null) return NotFound();
        return Ok(new EmpresaDto(empresa.Id, empresa.Codigo, empresa.Nombre, empresa.Ruc, empresa.IdMoneda, empresa.IdPais));
    }

    [HttpPut("empresa")]
    public async Task<ActionResult<EmpresaDto>> ActualizarEmpresa([FromBody] ActualizarEmpresaRequest request, CancellationToken ct)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(ct);
        if (empresa is null) return NotFound();

        empresa.Nombre = request.Nombre;
        empresa.Ruc = request.Ruc;
        empresa.IdMoneda = request.IdMoneda;
        empresa.IdPais = request.IdPais;
        await db.SaveChangesAsync(ct);
        return Ok(new EmpresaDto(empresa.Id, empresa.Codigo, empresa.Nombre, empresa.Ruc, empresa.IdMoneda, empresa.IdPais));
    }

    // ---- Roles ----

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyList<RolDto>>> Roles(CancellationToken ct)
    {
        var resultado = await db.Roles.OrderByDescending(r => r.Nivel)
            .Select(r => new RolDto(r.Id, r.Nombre, r.Nivel)).ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("roles")]
    public async Task<ActionResult<RolDto>> CrearRol([FromBody] CrearRolRequest request, CancellationToken ct)
    {
        if (await db.Roles.AnyAsync(r => r.Nombre == request.Nombre, ct))
        {
            throw new CodigoDuplicadoException("un rol", request.Nombre);
        }

        var rol = new Rol { Nombre = request.Nombre, Nivel = request.Nivel };
        db.Roles.Add(rol);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/roles/{rol.Id}", new RolDto(rol.Id, rol.Nombre, rol.Nivel));
    }

    [HttpPut("roles/{id:int}")]
    public async Task<ActionResult<RolDto>> ActualizarRol(int id, [FromBody] ActualizarRolRequest request, CancellationToken ct)
    {
        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rol is null) return NotFound();

        if (await db.Roles.AnyAsync(r => r.Nombre == request.Nombre && r.Id != id, ct))
        {
            throw new CodigoDuplicadoException("un rol", request.Nombre);
        }

        rol.Nombre = request.Nombre;
        rol.Nivel = request.Nivel;
        await db.SaveChangesAsync(ct);
        return Ok(new RolDto(rol.Id, rol.Nombre, rol.Nivel));
    }

    // ---- Permisos por rol (rol_menu) ----
    // Hasta ahora solo editable por migración (Nivel0_MenuRolMenu) — sin
    // pantalla real, había que tocar la base a mano para cambiar qué
    // módulos ve un rol. Mismo patrón simple directo contra el DbContext:
    // es un flag de asignación N:M sin invariante de negocio más allá de
    // "no duplicar la fila", igual que el resto de catálogos de acá.
    //
    // Limitación real, no oculta: los permisos de un usuario ya logueado
    // se calculan una sola vez al login (claims del JWT) — un cambio acá
    // no afecta una sesión activa hasta el próximo login. Documentado
    // también en CLAUDE.md junto con la revocación de tokens pendiente.

    [HttpGet("roles/{id:int}/menus")]
    public async Task<ActionResult<IReadOnlyList<MenuAsignadoDto>>> MenusDelRol(int id, CancellationToken ct)
    {
        if (!await db.Roles.AnyAsync(r => r.Id == id, ct))
        {
            return NotFound();
        }

        var asignados = await db.RolesMenu
            .Where(rm => rm.IdRol == id && rm.Activo)
            .Select(rm => rm.IdMenu)
            .ToListAsync(ct);

        var resultado = await db.Menus
            .Where(m => m.Activo)
            .OrderBy(m => m.Orden)
            .Select(m => new MenuAsignadoDto(m.Id, m.Codigo, m.Nombre, asignados.Contains(m.Id)))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPut("roles/{id:int}/menus")]
    public async Task<IActionResult> ActualizarMenusDelRol(
        int id, [FromBody] ActualizarRolMenuRequest request, CancellationToken ct)
    {
        if (!await db.Roles.AnyAsync(r => r.Id == id, ct))
        {
            return NotFound();
        }

        var existentes = await db.RolesMenu.Where(rm => rm.IdRol == id).ToListAsync(ct);

        foreach (var existente in existentes)
        {
            existente.Activo = request.IdsMenu.Contains(existente.IdMenu);
        }

        var idsExistentes = existentes.Select(e => e.IdMenu).ToHashSet();
        foreach (var idMenu in request.IdsMenu.Where(idMenu => !idsExistentes.Contains(idMenu)))
        {
            db.RolesMenu.Add(new RolMenu { IdRol = id, IdMenu = idMenu, Activo = true });
        }

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Plan de cuentas (Nivel 1) ----
    // A diferencia de los catálogos de arriba, sí tiene invariantes reales
    // (versionado por trigger, jerarquía, no se puede desactivar una cuenta
    // con saldo o en uso) — por eso pasa por Application
    // (ICuentaContableAdminService), no directo contra el DbContext.

    [HttpGet("plan-cuentas")]
    public async Task<ActionResult<IReadOnlyList<CuentaContablePlanDto>>> PlanCuentas(CancellationToken ct)
    {
        var resultado = await db.CuentasContables
            .Include(c => c.CuentaPadre)
            .OrderBy(c => c.Codigo)
            .Select(c => new CuentaContablePlanDto(
                c.Id, c.Codigo, c.Nombre, c.Grupo.ToString(), c.Naturaleza.ToString(),
                c.EsMayor, c.Activa, c.CuentaPadre != null ? c.CuentaPadre.Codigo : null))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("plan-cuentas")]
    public async Task<ActionResult<CuentaContableCreadaResult>> CrearCuentaContable(
        [FromBody] CrearCuentaContablePlanRequest request, CancellationToken ct)
    {
        var resultado = await cuentaContableAdmin.CrearAsync(
            new CrearCuentaContableRequest(
                request.Codigo, request.Nombre, request.Grupo, request.Naturaleza,
                request.IdCuentaPadre, request.EsMayor, User.Identity!.Name!),
            ct);
        return Created($"/api/configuracion/plan-cuentas/{resultado.Id}", resultado);
    }

    [HttpPut("plan-cuentas/{id:guid}")]
    public async Task<ActionResult> ActualizarCuentaContable(
        Guid id, [FromBody] ActualizarCuentaContablePlanRequest request, CancellationToken ct)
    {
        await cuentaContableAdmin.ActualizarAsync(
            new ActualizarCuentaContableRequest(id, request.Nombre, request.Activa, User.Identity!.Name!), ct);
        return NoContent();
    }

    // ---- Tipos de comprobante contable ----

    [HttpGet("tipos-comprobante")]
    public async Task<ActionResult<IReadOnlyList<TipoComprobanteContableDto>>> TiposComprobante(CancellationToken ct)
    {
        var resultado = await db.TiposComprobanteContable
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoComprobanteContableDto(t.Id, t.Codigo, t.Nombre))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("tipos-comprobante")]
    public async Task<ActionResult<TipoComprobanteContableDto>> CrearTipoComprobante(
        [FromBody] CrearTipoComprobanteContableRequest request, CancellationToken ct)
    {
        if (await db.TiposComprobanteContable.AnyAsync(t => t.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("un tipo de comprobante", request.Codigo);
        }

        var tipo = new Corela15.Domain.Contabilidad.TipoComprobanteContable
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
        };
        db.TiposComprobanteContable.Add(tipo);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tipos-comprobante/{tipo.Id}", new TipoComprobanteContableDto(tipo.Id, tipo.Codigo, tipo.Nombre));
    }

    [HttpPut("tipos-comprobante/{id:int}")]
    public async Task<ActionResult<TipoComprobanteContableDto>> ActualizarTipoComprobante(
        int id, [FromBody] CrearTipoComprobanteContableRequest request, CancellationToken ct)
    {
        var tipo = await db.TiposComprobanteContable.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tipo is null) return NotFound();

        if (await db.TiposComprobanteContable.AnyAsync(t => t.Codigo == request.Codigo && t.Id != id, ct))
        {
            throw new CodigoDuplicadoException("un tipo de comprobante", request.Codigo);
        }

        tipo.Codigo = request.Codigo;
        tipo.Nombre = request.Nombre;
        await db.SaveChangesAsync(ct);
        return Ok(new TipoComprobanteContableDto(tipo.Id, tipo.Codigo, tipo.Nombre));
    }

    // ---- Tipos de cuenta (productos de Ahorros, Nivel 2) ----
    // Catálogo simple: sin invariante más allá de código único, ya que
    // SaldoMinimo/TasaInteresAnual/PermiteDebitoPrestamo no dependen de
    // ninguna otra entidad para ser válidos.

    [HttpGet("tipos-cuenta")]
    public async Task<ActionResult<IReadOnlyList<TipoCuentaDto>>> TiposCuenta(CancellationToken ct)
    {
        var resultado = await db.TiposCuenta
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoCuentaDto(
                t.Id, t.Codigo, t.Nombre, t.SaldoMinimo, t.PermiteDebitoPrestamo,
                t.SaldoMinimoConPrestamo, t.TasaInteresAnual, t.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("tipos-cuenta")]
    public async Task<ActionResult<TipoCuentaDto>> CrearTipoCuenta([FromBody] CrearTipoCuentaRequest request, CancellationToken ct)
    {
        if (await db.TiposCuenta.AnyAsync(t => t.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("un tipo de cuenta", request.Codigo);
        }

        var tipo = new TipoCuenta
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            SaldoMinimo = request.SaldoMinimo,
            PermiteDebitoPrestamo = request.PermiteDebitoPrestamo,
            SaldoMinimoConPrestamo = request.SaldoMinimoConPrestamo,
            TasaInteresAnual = request.TasaInteresAnual,
            Activo = true,
        };
        db.TiposCuenta.Add(tipo);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tipos-cuenta/{tipo.Id}", new TipoCuentaDto(
            tipo.Id, tipo.Codigo, tipo.Nombre, tipo.SaldoMinimo, tipo.PermiteDebitoPrestamo,
            tipo.SaldoMinimoConPrestamo, tipo.TasaInteresAnual, tipo.Activo));
    }

    [HttpPut("tipos-cuenta/{id:int}")]
    public async Task<ActionResult<TipoCuentaDto>> ActualizarTipoCuenta(
        int id, [FromBody] ActualizarTipoCuentaRequest request, CancellationToken ct)
    {
        var tipo = await db.TiposCuenta.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tipo is null) return NotFound();

        tipo.Nombre = request.Nombre;
        tipo.SaldoMinimo = request.SaldoMinimo;
        tipo.PermiteDebitoPrestamo = request.PermiteDebitoPrestamo;
        tipo.SaldoMinimoConPrestamo = request.SaldoMinimoConPrestamo;
        tipo.TasaInteresAnual = request.TasaInteresAnual;
        tipo.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new TipoCuentaDto(
            tipo.Id, tipo.Codigo, tipo.Nombre, tipo.SaldoMinimo, tipo.PermiteDebitoPrestamo,
            tipo.SaldoMinimoConPrestamo, tipo.TasaInteresAnual, tipo.Activo));
    }

    // ---- Tipos de préstamo (productos de Crédito, Nivel 3) ----
    // Sí tiene un invariante real (la tasa no puede exceder el techo BCE
    // vigente del segmento) — pasa por Application (ITipoPrestamoAdminService),
    // mismo motivo que el plan de cuentas.

    [HttpGet("tipos-prestamo")]
    public async Task<ActionResult<IReadOnlyList<TipoPrestamoDto>>> TiposPrestamo(CancellationToken ct)
    {
        var resultado = await db.TiposPrestamo
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoPrestamoDto(
                t.Id, t.Codigo, t.Nombre, t.MontoMinimo, t.MontoMaximo,
                t.PlazoMinimoDias, t.PlazoMaximoDias, t.TasaAnual, t.SegmentoBce, t.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("tipos-prestamo")]
    public async Task<ActionResult<TipoPrestamoAdminResult>> CrearTipoPrestamo(
        [FromBody] CrearTipoPrestamoRequest request, CancellationToken ct)
    {
        var resultado = await tipoPrestamoAdmin.CrearAsync(request, ct);
        return Created($"/api/configuracion/tipos-prestamo/{resultado.Id}", resultado);
    }

    [HttpPut("tipos-prestamo/{id:int}")]
    public async Task<ActionResult> ActualizarTipoPrestamo(
        int id, [FromBody] ActualizarTipoPrestamoRequest request, CancellationToken ct)
    {
        await tipoPrestamoAdmin.ActualizarAsync(id, request, ct);
        return NoContent();
    }

    // ---- Tasas techo BCE ----
    // Catálogo simple con vigencia real: nunca se edita una tasa ya
    // sembrada (se conserva el historial), se agrega una fila nueva con
    // FechaVigenciaDesde — PUT solo permite corregir un error de tipeo o
    // desactivar, no cambiar el valor histórico.

    [HttpGet("tasas-techo-bce")]
    public async Task<ActionResult<IReadOnlyList<TasaTechoBceDto>>> TasasTechoBce(CancellationToken ct)
    {
        var resultado = await db.TasasTechoBce
            .OrderBy(t => t.Segmento).ThenByDescending(t => t.FechaVigenciaDesde)
            .Select(t => new TasaTechoBceDto(t.Id, t.Segmento, t.TasaMaxima, t.FechaVigenciaDesde, t.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("tasas-techo-bce")]
    public async Task<ActionResult<TasaTechoBceDto>> CrearTasaTechoBce(
        [FromBody] CrearTasaTechoBceRequest request, CancellationToken ct)
    {
        if (await db.TasasTechoBce.AnyAsync(t => t.Segmento == request.Segmento && t.FechaVigenciaDesde == request.FechaVigenciaDesde, ct))
        {
            throw new CodigoDuplicadoException("una tasa techo", $"{request.Segmento} — {request.FechaVigenciaDesde:yyyy-MM-dd}");
        }

        var tasa = new TasaTechoBce
        {
            Segmento = request.Segmento,
            TasaMaxima = request.TasaMaxima,
            FechaVigenciaDesde = request.FechaVigenciaDesde,
            Activo = true,
        };
        db.TasasTechoBce.Add(tasa);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tasas-techo-bce/{tasa.Id}",
            new TasaTechoBceDto(tasa.Id, tasa.Segmento, tasa.TasaMaxima, tasa.FechaVigenciaDesde, tasa.Activo));
    }

    [HttpPut("tasas-techo-bce/{id:int}")]
    public async Task<ActionResult<TasaTechoBceDto>> ActualizarTasaTechoBce(
        int id, [FromBody] ActualizarTasaTechoBceRequest request, CancellationToken ct)
    {
        var tasa = await db.TasasTechoBce.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tasa is null) return NotFound();

        tasa.TasaMaxima = request.TasaMaxima;
        tasa.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new TasaTechoBceDto(tasa.Id, tasa.Segmento, tasa.TasaMaxima, tasa.FechaVigenciaDesde, tasa.Activo));
    }

    // ---- Tablero de tasas DPF (item_plazo_tasa, Nivel 3) ----

    [HttpGet("tablero-tasas-dpf")]
    public async Task<ActionResult<IReadOnlyList<ItemPlazoTasaDto>>> TableroTasasDpf(CancellationToken ct)
    {
        var resultado = await db.ItemsPlazoTasa
            .OrderBy(t => t.PlazoDiasMin)
            .Select(t => new ItemPlazoTasaDto(
                t.Id, t.PlazoDiasMin, t.PlazoDiasMax, t.MontoMin, t.MontoMax,
                t.TipoPersona.ToString(), t.Tasa, t.FechaVigenciaDesde, t.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("tablero-tasas-dpf")]
    public async Task<ActionResult<ItemPlazoTasaDto>> CrearItemPlazoTasa(
        [FromBody] CrearItemPlazoTasaRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<TipoPersonaTasa>(request.TipoPersona, ignoreCase: true, out var tipoPersona))
        {
            throw new SolicitudInvalidaExceptionGenerica($"Tipo de persona inválido: {request.TipoPersona}");
        }

        var item = new ItemPlazoTasa
        {
            Id = Guid.NewGuid(),
            PlazoDiasMin = request.PlazoDiasMin,
            PlazoDiasMax = request.PlazoDiasMax,
            MontoMin = request.MontoMin,
            MontoMax = request.MontoMax,
            TipoPersona = tipoPersona,
            Tasa = request.Tasa,
            FechaVigenciaDesde = request.FechaVigenciaDesde,
            Activo = true,
        };
        db.ItemsPlazoTasa.Add(item);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tablero-tasas-dpf/{item.Id}", new ItemPlazoTasaDto(
            item.Id, item.PlazoDiasMin, item.PlazoDiasMax, item.MontoMin, item.MontoMax,
            item.TipoPersona.ToString(), item.Tasa, item.FechaVigenciaDesde, item.Activo));
    }

    [HttpPut("tablero-tasas-dpf/{id:guid}")]
    public async Task<ActionResult<ItemPlazoTasaDto>> ActualizarItemPlazoTasa(
        Guid id, [FromBody] ActualizarItemPlazoTasaRequest request, CancellationToken ct)
    {
        var item = await db.ItemsPlazoTasa.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (item is null) return NotFound();

        item.Tasa = request.Tasa;
        item.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new ItemPlazoTasaDto(
            item.Id, item.PlazoDiasMin, item.PlazoDiasMax, item.MontoMin, item.MontoMax,
            item.TipoPersona.ToString(), item.Tasa, item.FechaVigenciaDesde, item.Activo));
    }

    // ---- Categorías de riesgo de cartera (matriz A1-E, Nivel 3) ----

    [HttpGet("categorias-riesgo-cartera")]
    public async Task<ActionResult<IReadOnlyList<CategoriaRiesgoCarteraDto>>> CategoriasRiesgoCartera(CancellationToken ct)
    {
        var resultado = await db.CategoriasRiesgoCartera
            .OrderBy(c => c.DiasMoraInicio)
            .Select(c => new CategoriaRiesgoCarteraDto(
                c.Id, c.Codigo, c.Nombre, c.DiasMoraInicio, c.DiasMoraFin, c.PorcentajeProvision, c.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPut("categorias-riesgo-cartera/{id:int}")]
    public async Task<ActionResult<CategoriaRiesgoCarteraDto>> ActualizarCategoriaRiesgoCartera(
        int id, [FromBody] ActualizarCategoriaRiesgoCarteraRequest request, CancellationToken ct)
    {
        var categoria = await db.CategoriasRiesgoCartera.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (categoria is null) return NotFound();

        categoria.DiasMoraInicio = request.DiasMoraInicio;
        categoria.DiasMoraFin = request.DiasMoraFin;
        categoria.PorcentajeProvision = request.PorcentajeProvision;
        categoria.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new CategoriaRiesgoCarteraDto(
            categoria.Id, categoria.Codigo, categoria.Nombre, categoria.DiasMoraInicio,
            categoria.DiasMoraFin, categoria.PorcentajeProvision, categoria.Activo));
    }
}
