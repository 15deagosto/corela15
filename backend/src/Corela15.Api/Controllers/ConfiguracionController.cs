using Corela15.Application.Colocacion;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Clientes;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Contabilidad;
using Corela15.Domain.Credito;
using Corela15.Domain.FlujoTrabajo;
using Corela15.Domain.General;
using Corela15.Domain.Inversion;
using Corela15.Domain.Planificacion;
using Corela15.Domain.Seguridad;
using Corela15.Domain.Sujeto;
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

public record RolDto(int Id, string Nombre, int Nivel, int DiasCambioClave, bool PermiteConsolidadoCliente);
public record CrearRolRequest(string Nombre, int Nivel, int DiasCambioClave, bool PermiteConsolidadoCliente);
public record ActualizarRolRequest(string Nombre, int Nivel, int DiasCambioClave, bool PermiteConsolidadoCliente);

public record MenuAsignadoDto(int IdMenu, string Codigo, string Nombre, bool Asignado);
public record ActualizarRolMenuRequest(List<int> IdsMenu);

public record TipoEstructuraDto(string Codigo, string Nombre, bool Activo);
public record CrearTipoEstructuraRequest(string Codigo, string Nombre);
public record ActualizarTipoEstructuraRequest(string Nombre, bool Activo);

public record TipoEstructuraAsignadaDto(string Codigo, string Nombre, bool Asignado);
public record ActualizarRolTipoEstructuraRequest(List<string> CodigosTipoEstructura);

public record AreaPlanificacionDto(string Codigo, string Nombre, bool Activo);
public record CrearAreaPlanificacionRequest(string Codigo, string Nombre);
public record ActualizarAreaPlanificacionRequest(string Nombre, bool Activo);

public record EtiquetaPlanificacionDto(string Codigo, string Nombre, string ColorHex, bool Activo);
public record CrearEtiquetaPlanificacionRequest(string Codigo, string Nombre, string ColorHex);
public record ActualizarEtiquetaPlanificacionRequest(string Nombre, string ColorHex, bool Activo);

public record CuentaContablePlanDto(
    Guid Id, string Codigo, string Nombre, string Grupo, string Naturaleza,
    bool EsMayor, bool Activa, string? CodigoPadre);
public record CrearCuentaContablePlanRequest(
    string Codigo, string Nombre, string Grupo, string Naturaleza, Guid? IdCuentaPadre, bool EsMayor);
public record ActualizarCuentaContablePlanRequest(string Nombre, bool Activa);

public record TipoComprobanteContableDto(int Id, string Codigo, string Nombre);

public record FormaCancelacionDto(
    string Codigo, string Nombre, bool EsEfectivo, bool EsCheque, bool EsTransferencia,
    bool EsCausal, bool EsCuenta, bool Activo);

public record TipoTransaccionCuentaProductoDto(
    int Id, int IdTipoTransaccion, string TipoTransaccion, int IdTipoCuenta, string TipoCuenta,
    string CodigoCuentaDebito, string CodigoCuentaCredito, bool Activo);
public record CrearTipoTransaccionCuentaProductoRequest(
    int IdTipoTransaccion, int IdTipoCuenta, string CodigoCuentaDebito, string CodigoCuentaCredito);
public record ActualizarTipoTransaccionCuentaProductoRequest(
    string CodigoCuentaDebito, string CodigoCuentaCredito, bool Activo);
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
    int PlazoMinimoDias, int PlazoMaximoDias, decimal TasaAnual, string SegmentoBce, bool Activo,
    string? CodigoTipoCreditoSeps, string? CodigoTipoSeguro);

public record TipoSeguroDto(string Codigo, string Nombre, decimal ValorMensual, int BeneficiarioAdicional, bool Activo);

public record TipoConvenioDto(
    string Codigo, string Nombre, int IdAgencia, string Agencia, bool EsCooperativa, decimal ValorAhorro, bool Activo);

public record CrearTipoConvenioRequest(
    string Codigo, string Nombre, int IdAgencia, bool EsCooperativa, decimal ValorAhorro);

public record ActualizarTipoConvenioRequest(
    string Nombre, int IdAgencia, bool EsCooperativa, decimal ValorAhorro, bool Activo);

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

// Catálogos socioeconómicos reales de Socios (SUJETO.PERSONA_NATURAL /
// CLIENTES.CLIENTE en Softbank) — mismo patrón simple código+nombre que
// País/Moneda/Tipo de identificación, ver CLAUDE.md sección "CRUD real de
// Socios y Usuarios y roles".
public record CatalogoCodigoNombreDto(string Codigo, string Nombre, bool Activo);
public record CrearCatalogoCodigoNombreRequest(string Codigo, string Nombre);
public record ActualizarCatalogoCodigoNombreRequest(string Nombre, bool Activo);

// Motor de aprobaciones (FlujoTrabajo) — ver CLAUDE.md "Motor de
// aprobaciones genérico (FlujoTrabajo)". Solo se expone CRUD de
// GrupoContable/GrupoContableUsuario acá: son los únicos elementos que un
// administrador necesita tocar en operación normal (quién puede aprobar,
// y con qué rango de monto). Etapa/EtapaGrupoContable/EtapaRetorno quedan
// de solo lectura — son la estructura del motor en sí (a qué etapa ruteo,
// en qué orden), cambiarlos mal rompería el flujo real; se editan por
// migración cuando haga falta un tipo de etapa nuevo, mismo criterio que
// otros catálogos estructurales del core (ClasificacionCartera, etc.).
public record EtapaFlujoDto(int Id, string Nombre, string TipoEtapa, int Orden, bool Activa);

public record GrupoContableDto(string Codigo, string Nombre, decimal MontoMinimo, decimal MontoMaximo, bool Activo);
public record CrearGrupoContableRequest(string Codigo, string Nombre, decimal MontoMinimo, decimal MontoMaximo);
public record ActualizarGrupoContableRequest(string Nombre, decimal MontoMinimo, decimal MontoMaximo, bool Activo);

public record EtapaGrupoContableDto(int Id, int IdEtapa, string Etapa, int IdAgencia, string Agencia, string CodigoGrupoContable, bool Activa);

public record GrupoContableUsuarioDto(int Id, Guid IdUsuario, string NombreUsuario, bool Activo);
public record AgregarGrupoContableUsuarioRequest(Guid IdUsuario);

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
            .Select(r => new RolDto(r.Id, r.Nombre, r.Nivel, r.DiasCambioClave, r.PermiteConsolidadoCliente)).ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("roles")]
    public async Task<ActionResult<RolDto>> CrearRol([FromBody] CrearRolRequest request, CancellationToken ct)
    {
        if (await db.Roles.AnyAsync(r => r.Nombre == request.Nombre, ct))
        {
            throw new CodigoDuplicadoException("un rol", request.Nombre);
        }

        var rol = new Rol
        {
            Nombre = request.Nombre, Nivel = request.Nivel,
            DiasCambioClave = request.DiasCambioClave, PermiteConsolidadoCliente = request.PermiteConsolidadoCliente,
        };
        db.Roles.Add(rol);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/roles/{rol.Id}",
            new RolDto(rol.Id, rol.Nombre, rol.Nivel, rol.DiasCambioClave, rol.PermiteConsolidadoCliente));
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
        rol.DiasCambioClave = request.DiasCambioClave;
        rol.PermiteConsolidadoCliente = request.PermiteConsolidadoCliente;
        await db.SaveChangesAsync(ct);
        return Ok(new RolDto(rol.Id, rol.Nombre, rol.Nivel, rol.DiasCambioClave, rol.PermiteConsolidadoCliente));
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

    // ---- Tipos de estructura (segundo nivel de permiso, dentro del menú
    // "estructuras-financieras") — mismo patrón exacto que menús: catálogo
    // simple con CRUD directo (código+nombre+activo), y una asignación N:M
    // por rol (rol_tipo_estructura) con el mismo "reconciliar la lista
    // completa" ya usado en menús/roles por usuario. Permite crear un rol
    // que vea el módulo pero solo pueda generar ciertas estructuras (ej.
    // solo OF01), el requisito real que motivó este segundo nivel.

    [HttpGet("tipos-estructura")]
    public async Task<ActionResult<IReadOnlyList<TipoEstructuraDto>>> TiposEstructura(CancellationToken ct)
    {
        var resultado = await db.TiposEstructura
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoEstructuraDto(t.Codigo, t.Nombre, t.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("tipos-estructura")]
    public async Task<ActionResult<TipoEstructuraDto>> CrearTipoEstructura(
        [FromBody] CrearTipoEstructuraRequest request, CancellationToken ct)
    {
        if (await db.TiposEstructura.AnyAsync(t => t.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("un tipo de estructura", request.Codigo);
        }

        var tipo = new TipoEstructura { Codigo = request.Codigo, Nombre = request.Nombre, Activo = true };
        db.TiposEstructura.Add(tipo);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tipos-estructura/{tipo.Codigo}", new TipoEstructuraDto(tipo.Codigo, tipo.Nombre, tipo.Activo));
    }

    [HttpPut("tipos-estructura/{codigo}")]
    public async Task<ActionResult<TipoEstructuraDto>> ActualizarTipoEstructura(
        string codigo, [FromBody] ActualizarTipoEstructuraRequest request, CancellationToken ct)
    {
        var tipo = await db.TiposEstructura.FirstOrDefaultAsync(t => t.Codigo == codigo, ct);
        if (tipo is null) return NotFound();

        tipo.Nombre = request.Nombre;
        tipo.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new TipoEstructuraDto(tipo.Codigo, tipo.Nombre, tipo.Activo));
    }

    [HttpGet("roles/{id:int}/tipos-estructura")]
    public async Task<ActionResult<IReadOnlyList<TipoEstructuraAsignadaDto>>> TiposEstructuraDelRol(int id, CancellationToken ct)
    {
        if (!await db.Roles.AnyAsync(r => r.Id == id, ct))
        {
            return NotFound();
        }

        var asignados = await db.RolesTipoEstructura
            .Where(re => re.IdRol == id && re.Activo)
            .Select(re => re.CodigoTipoEstructura)
            .ToListAsync(ct);

        var resultado = await db.TiposEstructura
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoEstructuraAsignadaDto(t.Codigo, t.Nombre, asignados.Contains(t.Codigo)))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPut("roles/{id:int}/tipos-estructura")]
    public async Task<IActionResult> ActualizarTiposEstructuraDelRol(
        int id, [FromBody] ActualizarRolTipoEstructuraRequest request, CancellationToken ct)
    {
        if (!await db.Roles.AnyAsync(r => r.Id == id, ct))
        {
            return NotFound();
        }

        var existentes = await db.RolesTipoEstructura.Where(re => re.IdRol == id).ToListAsync(ct);

        foreach (var existente in existentes)
        {
            existente.Activo = request.CodigosTipoEstructura.Contains(existente.CodigoTipoEstructura);
        }

        var codigosExistentes = existentes.Select(e => e.CodigoTipoEstructura).ToHashSet();
        foreach (var codigo in request.CodigosTipoEstructura.Where(c => !codigosExistentes.Contains(c)))
        {
            db.RolesTipoEstructura.Add(new RolTipoEstructura { IdRol = id, CodigoTipoEstructura = codigo, Activo = true });
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

    // ---- Formas de cancelación (CxP, verificado contra CONTABILIDAD.FORMA_CANCELACION) ----
    // Las 5 banderas de clasificación (es_efectivo/es_cheque/es_transferencia/
    // es_causal/es_cuenta) son la naturaleza real de cada forma según Softbank
    // — no se editan (mismo criterio que las 9 categorías fijas de
    // CategoriaRiesgoCartera), solo nombre y estado activo.

    [HttpGet("formas-cancelacion")]
    public async Task<ActionResult<IReadOnlyList<FormaCancelacionDto>>> FormasCancelacion(CancellationToken ct) =>
        Ok(await db.FormasCancelacion.OrderBy(x => x.Codigo)
            .Select(x => new FormaCancelacionDto(
                x.Codigo, x.Nombre, x.EsEfectivo, x.EsCheque, x.EsTransferencia, x.EsCausal, x.EsCuenta, x.Activo))
            .ToListAsync(ct));

    [HttpPut("formas-cancelacion/{codigo}")]
    public async Task<IActionResult> ActualizarFormaCancelacion(
        string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var f = await db.FormasCancelacion.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (f is null) return NotFound();
        f.Nombre = r.Nombre;
        f.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
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
                t.PlazoMinimoDias, t.PlazoMaximoDias, t.TasaAnual, t.SegmentoBce, t.Activo,
                t.CodigoTipoCreditoSeps, t.CodigoTipoSeguro))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    // Catálogo real de seguro de desgravamen flat (ver TipoSeguro.cs) —
    // solo lectura, mismo criterio que las categorías fijas de riesgo de
    // cartera: los 3 tipos reales (Individual/Deudor y Adicional/Deudor y
    // Familiares) no se crean ni se borran desde acá.
    [HttpGet("tipos-seguro")]
    public async Task<ActionResult<IReadOnlyList<TipoSeguroDto>>> TiposSeguro(CancellationToken ct)
    {
        var resultado = await db.TiposSeguro
            .OrderBy(t => t.ValorMensual)
            .Select(t => new TipoSeguroDto(t.Codigo, t.Nombre, t.ValorMensual, t.BeneficiarioAdicional, t.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    // Convenios reales (empleador/sindicato con descuento vía rol de pagos,
    // ver credito.tipo_convenio) — a diferencia de tipos-seguro, este SÍ es
    // un catálogo operativo real que crece (nuevos convenios se firman con
    // el tiempo), CRUD completo directo contra el DbContext (código+nombre,
    // sin invariante de negocio más allá de unicidad — mismo patrón que
    // catálogos simples de Configuración).
    [HttpGet("tipos-convenio")]
    public async Task<ActionResult<IReadOnlyList<TipoConvenioDto>>> TiposConvenio(CancellationToken ct)
    {
        var resultado = await db.TiposConvenio
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoConvenioDto(
                t.Codigo, t.Nombre, t.IdAgencia, t.Agencia.Nombre, t.EsCooperativa, t.ValorAhorro, t.Activo))
            .ToListAsync(ct);

        return Ok(resultado);
    }

    [HttpPost("tipos-convenio")]
    public async Task<ActionResult<TipoConvenioDto>> CrearTipoConvenio(
        [FromBody] CrearTipoConvenioRequest request, CancellationToken ct)
    {
        if (await db.TiposConvenio.AnyAsync(t => t.Codigo == request.Codigo, ct))
        {
            throw new CodigoDuplicadoException("un convenio", request.Codigo);
        }

        var agencia = await db.Agencias.FirstOrDefaultAsync(a => a.Id == request.IdAgencia && a.Activa, ct);
        if (agencia is null) return BadRequest("La agencia no existe o está inactiva.");

        var convenio = new TipoConvenio
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            IdAgencia = request.IdAgencia,
            EsCooperativa = request.EsCooperativa,
            ValorAhorro = request.ValorAhorro,
            Activo = true,
        };
        db.TiposConvenio.Add(convenio);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tipos-convenio/{convenio.Codigo}", new TipoConvenioDto(
            convenio.Codigo, convenio.Nombre, convenio.IdAgencia, agencia.Nombre, convenio.EsCooperativa, convenio.ValorAhorro, convenio.Activo));
    }

    [HttpPut("tipos-convenio/{codigo}")]
    public async Task<ActionResult<TipoConvenioDto>> ActualizarTipoConvenio(
        string codigo, [FromBody] ActualizarTipoConvenioRequest request, CancellationToken ct)
    {
        var convenio = await db.TiposConvenio.Include(t => t.Agencia).FirstOrDefaultAsync(t => t.Codigo == codigo, ct);
        if (convenio is null) return NotFound();

        var agencia = await db.Agencias.FirstOrDefaultAsync(a => a.Id == request.IdAgencia && a.Activa, ct);
        if (agencia is null) return BadRequest("La agencia no existe o está inactiva.");

        convenio.Nombre = request.Nombre;
        convenio.IdAgencia = request.IdAgencia;
        convenio.EsCooperativa = request.EsCooperativa;
        convenio.ValorAhorro = request.ValorAhorro;
        convenio.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new TipoConvenioDto(
            convenio.Codigo, convenio.Nombre, convenio.IdAgencia, agencia.Nombre, convenio.EsCooperativa, convenio.ValorAhorro, convenio.Activo));
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

    // ---- Catálogos socioeconómicos de Socios (verificados contra Softbank) ----

    [HttpGet("estados-civiles")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> EstadosCiviles(CancellationToken ct) =>
        Ok(await db.EstadosCiviles.OrderBy(x => x.Nombre)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Nombre, x.Activo)).ToListAsync(ct));

    [HttpPost("estados-civiles")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearEstadoCivil([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.EstadosCiviles.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("un estado civil", r.Codigo);
        db.EstadosCiviles.Add(new EstadoCivil { Codigo = r.Codigo, Nombre = r.Nombre, Activo = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/estados-civiles/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("estados-civiles/{codigo}")]
    public async Task<IActionResult> ActualizarEstadoCivil(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.EstadosCiviles.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Nombre = r.Nombre; e.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("educacion")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> Educacion(CancellationToken ct) =>
        Ok(await db.Educaciones.OrderBy(x => x.Nombre)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Nombre, x.Activo)).ToListAsync(ct));

    [HttpPost("educacion")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearEducacion([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.Educaciones.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("un nivel de educación", r.Codigo);
        db.Educaciones.Add(new Educacion { Codigo = r.Codigo, Nombre = r.Nombre, Activo = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/educacion/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("educacion/{codigo}")]
    public async Task<IActionResult> ActualizarEducacion(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.Educaciones.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Nombre = r.Nombre; e.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("vivienda")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> Vivienda(CancellationToken ct) =>
        Ok(await db.Viviendas.OrderBy(x => x.Nombre)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Nombre, x.Activo)).ToListAsync(ct));

    [HttpPost("vivienda")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearVivienda([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.Viviendas.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("un tipo de vivienda", r.Codigo);
        db.Viviendas.Add(new Vivienda { Codigo = r.Codigo, Nombre = r.Nombre, Activo = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/vivienda/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("vivienda/{codigo}")]
    public async Task<IActionResult> ActualizarVivienda(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.Viviendas.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Nombre = r.Nombre; e.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("sector-vivienda")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> SectorVivienda(CancellationToken ct) =>
        Ok(await db.SectoresVivienda.OrderBy(x => x.Nombre)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Nombre, x.Activo)).ToListAsync(ct));

    [HttpPost("sector-vivienda")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearSectorVivienda([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.SectoresVivienda.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("un sector de vivienda", r.Codigo);
        db.SectoresVivienda.Add(new SectorVivienda { Codigo = r.Codigo, Nombre = r.Nombre, Activo = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/sector-vivienda/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("sector-vivienda/{codigo}")]
    public async Task<IActionResult> ActualizarSectorVivienda(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.SectoresVivienda.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Nombre = r.Nombre; e.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("nacionalidades")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> Nacionalidades(CancellationToken ct) =>
        Ok(await db.Nacionalidades.OrderBy(x => x.Nombre)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Nombre, x.Activo)).ToListAsync(ct));

    [HttpPost("nacionalidades")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearNacionalidad([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.Nacionalidades.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("una nacionalidad", r.Codigo);
        db.Nacionalidades.Add(new Nacionalidad { Codigo = r.Codigo, Nombre = r.Nombre, Activo = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/nacionalidades/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("nacionalidades/{codigo}")]
    public async Task<IActionResult> ActualizarNacionalidad(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.Nacionalidades.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Nombre = r.Nombre; e.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("causas-vinculacion")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> CausasVinculacion(CancellationToken ct) =>
        Ok(await db.CausasVinculacion.OrderBy(x => x.Codigo)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Descripcion, x.Activa)).ToListAsync(ct));

    [HttpPost("causas-vinculacion")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearCausaVinculacion([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.CausasVinculacion.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("una causa de vinculación", r.Codigo);
        db.CausasVinculacion.Add(new CausaVinculacion { Codigo = r.Codigo, Descripcion = r.Nombre, Activa = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/causas-vinculacion/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("causas-vinculacion/{codigo}")]
    public async Task<IActionResult> ActualizarCausaVinculacion(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.CausasVinculacion.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Descripcion = r.Nombre; e.Activa = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("calificaciones-internas")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> CalificacionesInternas(CancellationToken ct) =>
        Ok(await db.CalificacionesInternas.OrderBy(x => x.Nombre)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Nombre, x.Activa)).ToListAsync(ct));

    [HttpPost("calificaciones-internas")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearCalificacionInterna([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.CalificacionesInternas.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("una calificación interna", r.Codigo);
        db.CalificacionesInternas.Add(new CalificacionInterna { Codigo = r.Codigo, Nombre = r.Nombre, Activa = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/calificaciones-internas/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("calificaciones-internas/{codigo}")]
    public async Task<IActionResult> ActualizarCalificacionInterna(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.CalificacionesInternas.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Nombre = r.Nombre; e.Activa = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("sectores-economicos")]
    public async Task<ActionResult<IReadOnlyList<CatalogoCodigoNombreDto>>> SectoresEconomicos(CancellationToken ct) =>
        Ok(await db.SectoresEconomicos.OrderBy(x => x.Nombre)
            .Select(x => new CatalogoCodigoNombreDto(x.Codigo, x.Nombre, x.Activa)).ToListAsync(ct));

    [HttpPost("sectores-economicos")]
    public async Task<ActionResult<CatalogoCodigoNombreDto>> CrearSectorEconomico([FromBody] CrearCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        if (await db.SectoresEconomicos.AnyAsync(x => x.Codigo == r.Codigo, ct)) throw new CodigoDuplicadoException("un sector económico", r.Codigo);
        db.SectoresEconomicos.Add(new SectorEconomico { Codigo = r.Codigo, Nombre = r.Nombre, Activa = true });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/sectores-economicos/{r.Codigo}", new CatalogoCodigoNombreDto(r.Codigo, r.Nombre, true));
    }

    [HttpPut("sectores-economicos/{codigo}")]
    public async Task<IActionResult> ActualizarSectorEconomico(string codigo, [FromBody] ActualizarCatalogoCodigoNombreRequest r, CancellationToken ct)
    {
        var e = await db.SectoresEconomicos.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (e is null) return NotFound();
        e.Nombre = r.Nombre; e.Activa = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Override contable por producto (ver TipoTransaccionCuentaProducto.cs) ----

    [HttpGet("tipo-transaccion-cuenta-producto")]
    public async Task<ActionResult<IReadOnlyList<TipoTransaccionCuentaProductoDto>>> TipoTransaccionCuentaProducto(CancellationToken ct) =>
        Ok(await db.TiposTransaccionCuentaProducto
            .Include(o => o.TipoTransaccion).Include(o => o.TipoCuenta)
            .Include(o => o.CuentaContableDebito).Include(o => o.CuentaContableCredito)
            .OrderBy(o => o.TipoTransaccion.Codigo).ThenBy(o => o.TipoCuenta.Codigo)
            .Select(o => new TipoTransaccionCuentaProductoDto(
                o.Id, o.IdTipoTransaccion, o.TipoTransaccion.Codigo, o.IdTipoCuenta, o.TipoCuenta.Codigo,
                o.CuentaContableDebito.Codigo, o.CuentaContableCredito.Codigo, o.Activo))
            .ToListAsync(ct));

    [HttpPost("tipo-transaccion-cuenta-producto")]
    public async Task<ActionResult<TipoTransaccionCuentaProductoDto>> CrearTipoTransaccionCuentaProducto(
        [FromBody] CrearTipoTransaccionCuentaProductoRequest r, CancellationToken ct)
    {
        if (await db.TiposTransaccionCuentaProducto.AnyAsync(
                o => o.IdTipoTransaccion == r.IdTipoTransaccion && o.IdTipoCuenta == r.IdTipoCuenta, ct))
        {
            throw new CodigoDuplicadoException("un override contable", $"{r.IdTipoTransaccion}/{r.IdTipoCuenta}");
        }

        var idDebito = await db.CuentasContables.Where(c => c.Codigo == r.CodigoCuentaDebito).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(ct);
        var idCredito = await db.CuentasContables.Where(c => c.Codigo == r.CodigoCuentaCredito).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(ct);
        if (idDebito is null || idCredito is null) return BadRequest("Cuenta contable débito/crédito no existe.");

        var entidad = new TipoTransaccionCuentaProducto
        {
            IdTipoTransaccion = r.IdTipoTransaccion, IdTipoCuenta = r.IdTipoCuenta,
            IdCuentaContableDebito = idDebito.Value, IdCuentaContableCredito = idCredito.Value, Activo = true,
        };
        db.TiposTransaccionCuentaProducto.Add(entidad);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/tipo-transaccion-cuenta-producto/{entidad.Id}",
            new TipoTransaccionCuentaProductoDto(entidad.Id, r.IdTipoTransaccion, "", r.IdTipoCuenta, "", r.CodigoCuentaDebito, r.CodigoCuentaCredito, true));
    }

    [HttpPut("tipo-transaccion-cuenta-producto/{id:int}")]
    public async Task<IActionResult> ActualizarTipoTransaccionCuentaProducto(
        int id, [FromBody] ActualizarTipoTransaccionCuentaProductoRequest r, CancellationToken ct)
    {
        var entidad = await db.TiposTransaccionCuentaProducto.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (entidad is null) return NotFound();

        var idDebito = await db.CuentasContables.Where(c => c.Codigo == r.CodigoCuentaDebito).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(ct);
        var idCredito = await db.CuentasContables.Where(c => c.Codigo == r.CodigoCuentaCredito).Select(c => (Guid?)c.Id).FirstOrDefaultAsync(ct);
        if (idDebito is null || idCredito is null) return BadRequest("Cuenta contable débito/crédito no existe.");

        entidad.IdCuentaContableDebito = idDebito.Value;
        entidad.IdCuentaContableCredito = idCredito.Value;
        entidad.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Motor de aprobaciones (FlujoTrabajo) ----

    [HttpGet("flujo-trabajo/etapas")]
    public async Task<ActionResult<IReadOnlyList<EtapaFlujoDto>>> Etapas(CancellationToken ct) =>
        Ok(await db.Etapas.Include(e => e.TipoEtapa).OrderBy(e => e.Orden)
            .Select(e => new EtapaFlujoDto(e.Id, e.Nombre, e.TipoEtapa.Nombre, e.Orden, e.Activa)).ToListAsync(ct));

    [HttpGet("flujo-trabajo/etapas/{idEtapa:int}/grupos")]
    public async Task<ActionResult<IReadOnlyList<EtapaGrupoContableDto>>> RuteoDeEtapa(int idEtapa, CancellationToken ct) =>
        Ok(await db.EtapasGrupoContable.Include(r => r.Etapa).Include(r => r.Agencia)
            .Where(r => r.IdEtapa == idEtapa)
            .Select(r => new EtapaGrupoContableDto(r.Id, r.IdEtapa, r.Etapa.Nombre, r.IdAgencia, r.Agencia.Nombre, r.CodigoGrupoContable, r.Activa))
            .ToListAsync(ct));

    [HttpGet("flujo-trabajo/grupos-contables")]
    public async Task<ActionResult<IReadOnlyList<GrupoContableDto>>> GruposContables(CancellationToken ct) =>
        Ok(await db.GruposContables.OrderBy(g => g.Nombre)
            .Select(g => new GrupoContableDto(g.Codigo, g.Nombre, g.MontoMinimo, g.MontoMaximo, g.Activo)).ToListAsync(ct));

    [HttpPost("flujo-trabajo/grupos-contables")]
    public async Task<ActionResult<GrupoContableDto>> CrearGrupoContable([FromBody] CrearGrupoContableRequest r, CancellationToken ct)
    {
        if (await db.GruposContables.AnyAsync(g => g.Codigo == r.Codigo, ct))
        {
            throw new CodigoDuplicadoException("un grupo de aprobadores", r.Codigo);
        }

        db.GruposContables.Add(new GrupoContable
        {
            Codigo = r.Codigo, Nombre = r.Nombre, MontoMinimo = r.MontoMinimo, MontoMaximo = r.MontoMaximo, Activo = true,
        });
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/flujo-trabajo/grupos-contables/{r.Codigo}", new GrupoContableDto(r.Codigo, r.Nombre, r.MontoMinimo, r.MontoMaximo, true));
    }

    [HttpPut("flujo-trabajo/grupos-contables/{codigo}")]
    public async Task<IActionResult> ActualizarGrupoContable(string codigo, [FromBody] ActualizarGrupoContableRequest r, CancellationToken ct)
    {
        var g = await db.GruposContables.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (g is null) return NotFound();
        g.Nombre = r.Nombre; g.MontoMinimo = r.MontoMinimo; g.MontoMaximo = r.MontoMaximo; g.Activo = r.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("flujo-trabajo/grupos-contables/{codigo}/usuarios")]
    public async Task<ActionResult<IReadOnlyList<GrupoContableUsuarioDto>>> UsuariosDelGrupo(string codigo, CancellationToken ct) =>
        Ok(await db.GruposContablesUsuarios.Include(u => u.Usuario)
            .Where(u => u.CodigoGrupoContable == codigo)
            .Select(u => new GrupoContableUsuarioDto(u.Id, u.IdUsuario, u.Usuario.NombreUsuario, u.Activo))
            .ToListAsync(ct));

    // Agregar/reactivar un usuario en el grupo — sin esto, solo los
    // usuarios sembrados por migración podrían aprobar créditos para
    // siempre; esta es la pantalla real que lo resuelve.
    [HttpPost("flujo-trabajo/grupos-contables/{codigo}/usuarios")]
    public async Task<IActionResult> AgregarUsuarioAlGrupo(
        string codigo, [FromBody] AgregarGrupoContableUsuarioRequest r, CancellationToken ct)
    {
        if (!await db.GruposContables.AnyAsync(g => g.Codigo == codigo, ct)) return NotFound();
        if (!await db.Usuarios.AnyAsync(u => u.Id == r.IdUsuario, ct)) return NotFound();

        var existente = await db.GruposContablesUsuarios
            .FirstOrDefaultAsync(u => u.CodigoGrupoContable == codigo && u.IdUsuario == r.IdUsuario, ct);
        if (existente is not null)
        {
            existente.Activo = true;
        }
        else
        {
            db.GruposContablesUsuarios.Add(new GrupoContableUsuario { CodigoGrupoContable = codigo, IdUsuario = r.IdUsuario, Activo = true });
        }

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("flujo-trabajo/grupos-contables-usuarios/{id:int}")]
    public async Task<IActionResult> QuitarUsuarioDelGrupo(int id, CancellationToken ct)
    {
        var u = await db.GruposContablesUsuarios.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u is null) return NotFound();
        u.Activo = false;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // Catálogos del módulo Planificación -- áreas de la cooperativa y
    // etiquetas/categorías (con color real) para clasificar cada bloque
    // de la planificación semanal. Mismo patrón simple que el resto.

    [HttpGet("planificacion/areas")]
    public async Task<ActionResult<IReadOnlyList<AreaPlanificacionDto>>> AreasPlanificacion(CancellationToken ct)
        => Ok(await db.AreasPlanificacion.OrderBy(a => a.Nombre)
            .Select(a => new AreaPlanificacionDto(a.Codigo, a.Nombre, a.Activo)).ToListAsync(ct));

    [HttpPost("planificacion/areas")]
    public async Task<ActionResult<AreaPlanificacionDto>> CrearAreaPlanificacion(CrearAreaPlanificacionRequest request, CancellationToken ct)
    {
        if (await db.AreasPlanificacion.AnyAsync(a => a.Codigo == request.Codigo, ct))
            throw new CodigoDuplicadoException("un área", request.Codigo);

        var area = new AreaPlanificacion { Codigo = request.Codigo, Nombre = request.Nombre, Activo = true };
        db.AreasPlanificacion.Add(area);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/planificacion/areas/{area.Codigo}", new AreaPlanificacionDto(area.Codigo, area.Nombre, area.Activo));
    }

    [HttpPut("planificacion/areas/{codigo}")]
    public async Task<ActionResult<AreaPlanificacionDto>> ActualizarAreaPlanificacion(string codigo, ActualizarAreaPlanificacionRequest request, CancellationToken ct)
    {
        var area = await db.AreasPlanificacion.FirstOrDefaultAsync(a => a.Codigo == codigo, ct);
        if (area is null) return NotFound();
        area.Nombre = request.Nombre;
        area.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new AreaPlanificacionDto(area.Codigo, area.Nombre, area.Activo));
    }

    [HttpGet("planificacion/etiquetas")]
    public async Task<ActionResult<IReadOnlyList<EtiquetaPlanificacionDto>>> EtiquetasPlanificacion(CancellationToken ct)
        => Ok(await db.EtiquetasPlanificacion.OrderBy(e => e.Nombre)
            .Select(e => new EtiquetaPlanificacionDto(e.Codigo, e.Nombre, e.ColorHex, e.Activo)).ToListAsync(ct));

    [HttpPost("planificacion/etiquetas")]
    public async Task<ActionResult<EtiquetaPlanificacionDto>> CrearEtiquetaPlanificacion(CrearEtiquetaPlanificacionRequest request, CancellationToken ct)
    {
        if (await db.EtiquetasPlanificacion.AnyAsync(e => e.Codigo == request.Codigo, ct))
            throw new CodigoDuplicadoException("una etiqueta", request.Codigo);

        var etiqueta = new EtiquetaPlanificacion { Codigo = request.Codigo, Nombre = request.Nombre, ColorHex = request.ColorHex, Activo = true };
        db.EtiquetasPlanificacion.Add(etiqueta);
        await db.SaveChangesAsync(ct);
        return Created($"/api/configuracion/planificacion/etiquetas/{etiqueta.Codigo}", new EtiquetaPlanificacionDto(etiqueta.Codigo, etiqueta.Nombre, etiqueta.ColorHex, etiqueta.Activo));
    }

    [HttpPut("planificacion/etiquetas/{codigo}")]
    public async Task<ActionResult<EtiquetaPlanificacionDto>> ActualizarEtiquetaPlanificacion(string codigo, ActualizarEtiquetaPlanificacionRequest request, CancellationToken ct)
    {
        var etiqueta = await db.EtiquetasPlanificacion.FirstOrDefaultAsync(e => e.Codigo == codigo, ct);
        if (etiqueta is null) return NotFound();
        etiqueta.Nombre = request.Nombre;
        etiqueta.ColorHex = request.ColorHex;
        etiqueta.Activo = request.Activo;
        await db.SaveChangesAsync(ct);
        return Ok(new EtiquetaPlanificacionDto(etiqueta.Codigo, etiqueta.Nombre, etiqueta.ColorHex, etiqueta.Activo));
    }
}
