using Corela15.Application.Common;
using Corela15.Domain.General;
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

[ApiController]
[Route("api/configuracion")]
[Authorize(Policy = "Menu:configuracion")]
public class ConfiguracionController(Corela15DbContext db) : ControllerBase
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
}
