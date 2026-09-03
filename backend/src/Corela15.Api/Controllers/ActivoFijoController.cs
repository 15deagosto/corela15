using Corela15.Api.Idempotencia;
using Corela15.Application.ActivoFijo;
using Corela15.Application.Common;
using Corela15.Domain.ActivoFijo;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record EstructuraListItem(
    int Id, string Nombre, bool SeDeprecia, decimal PorcentajeDepreciacionAnual, bool EsBienIntangible, bool Activo);

public record ResponsableListItem(Guid Id, string Nombre, string Agencia, bool Activo);

public record ActivoListItem(
    Guid Id, string? Codigo, string Categoria, string Agencia, string Detalle, decimal Valor,
    decimal DepreciacionAcumulada, string Condicion, string Estado, string? Responsable);

public record ActivoDetalle(
    Guid Id, string? Codigo, int IdEstructura, string Categoria, int IdAgencia, string Agencia, string Detalle,
    DateOnly FechaCompra, decimal Valor, string? Marca, string? Modelo, string? Serie, bool EsVehiculo,
    bool EsBienDeControl, bool EsBienIntangible, bool Asegurado, string? Color, string? Motor, string? Chasis,
    string? Placa, string? Cilindraje, int? AnioMatriculacion, int? AnioVehiculo, string Condicion, string Estado,
    decimal DepreciacionAcumulada, decimal ValorLibros, string? Responsable, Guid? IdResponsable);

public record TrasladoListItem(
    Guid Id, string ActivoDetalle, string Concepto, DateOnly Fecha, string Motivo, string? AgenciaOrigen,
    string AgenciaDestino, string Estado);

public record SolicitudBajaListItem(
    Guid Id, string ActivoDetalle, string Motivo, string? Detalle, DateOnly FechaSolicitud, string Estado, Guid? IdComprobante);

[ApiController]
[Route("api/activofijo")]
[Authorize(Policy = "Menu:activofijo")]
public class ActivoFijoController(Corela15DbContext db, IActivoService activoService) : ControllerBase
{
    [HttpGet("estructuras")]
    public async Task<ActionResult<IReadOnlyList<EstructuraListItem>>> Estructuras(CancellationToken ct)
    {
        var resultado = await db.EstructurasActivoFijo
            .OrderBy(e => e.Nombre)
            .Select(e => new EstructuraListItem(e.Id, e.Nombre, e.SeDeprecia, e.PorcentajeDepreciacionAnual, e.EsBienIntangible, e.Activo))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("responsables")]
    public async Task<ActionResult<IReadOnlyList<ResponsableListItem>>> Responsables(CancellationToken ct)
    {
        var resultado = await db.ResponsablesActivoFijo
            .Include(r => r.Persona).Include(r => r.Agencia)
            .OrderBy(r => r.Persona.Nombre)
            .Select(r => new ResponsableListItem(r.Id, r.Persona.Nombre, r.Agencia.Nombre, r.Activo))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("responsables")]
    public async Task<ActionResult<ResponsableListItem>> CrearResponsable(
        [FromBody] CrearResponsableBody body, CancellationToken ct)
    {
        if (await db.ResponsablesActivoFijo.AnyAsync(r => r.IdPersona == body.IdPersona && r.Activo, ct))
        {
            throw new CodigoDuplicadoException("un responsable activo para", body.IdPersona.ToString());
        }

        var responsable = new Responsable { Id = Guid.NewGuid(), IdPersona = body.IdPersona, IdAgencia = body.IdAgencia, Activo = true };
        db.ResponsablesActivoFijo.Add(responsable);
        await db.SaveChangesAsync(ct);

        var persona = await db.Personas.Where(p => p.Id == body.IdPersona).Select(p => p.Nombre).FirstAsync(ct);
        var agencia = await db.Agencias.Where(a => a.Id == body.IdAgencia).Select(a => a.Nombre).FirstAsync(ct);
        return Created($"/api/activofijo/responsables/{responsable.Id}", new ResponsableListItem(responsable.Id, persona, agencia, true));
    }

    [HttpPut("responsables/{id:guid}")]
    public async Task<IActionResult> ActualizarResponsable(Guid id, [FromBody] ActualizarResponsableBody body, CancellationToken ct)
    {
        var responsable = await db.ResponsablesActivoFijo.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (responsable is null) return NotFound();
        responsable.IdAgencia = body.IdAgencia;
        responsable.Activo = body.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("motivos-traslado")]
    public async Task<ActionResult<IReadOnlyList<MotivoTrasladoActivo>>> MotivosTraslado(CancellationToken ct) =>
        Ok(await db.MotivosTrasladoActivo.Where(m => m.Activo).OrderBy(m => m.Nombre).ToListAsync(ct));

    [HttpGet("motivos-baja")]
    public async Task<ActionResult<IReadOnlyList<MotivoBaja>>> MotivosBaja(CancellationToken ct) =>
        Ok(await db.MotivosBajaActivo.Where(m => m.Activo).OrderBy(m => m.Detalle).ToListAsync(ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivoListItem>>> Listar(CancellationToken ct)
    {
        var resultado = await db.Activos
            .Include(a => a.Estructura).Include(a => a.Agencia)
            .OrderByDescending(a => a.CreadoEn)
            .Select(a => new ActivoListItem(
                a.Id, a.Codigo, a.Estructura.Nombre, a.Agencia.Nombre, a.Detalle, a.Valor, a.DepreciacionAcumulada,
                a.Condicion.ToString(), a.Estado.ToString(),
                db.ActivosResponsables.Where(ar => ar.IdActivo == a.Id && ar.Activa).Select(ar => ar.Responsable.Persona.Nombre).FirstOrDefault()))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ActivoDetalle>> Detalle(Guid id, CancellationToken ct)
    {
        var activo = await db.Activos.Include(a => a.Estructura).Include(a => a.Agencia)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (activo is null) return NotFound();

        var responsable = await db.ActivosResponsables
            .Where(ar => ar.IdActivo == id && ar.Activa)
            .Select(ar => new { ar.IdResponsable, Nombre = ar.Responsable.Persona.Nombre })
            .FirstOrDefaultAsync(ct);

        return Ok(new ActivoDetalle(
            activo.Id, activo.Codigo, activo.IdEstructura, activo.Estructura.Nombre, activo.IdAgencia, activo.Agencia.Nombre,
            activo.Detalle, activo.FechaCompra, activo.Valor, activo.Marca, activo.Modelo, activo.Serie, activo.EsVehiculo,
            activo.EsBienDeControl, activo.EsBienIntangible, activo.Asegurado, activo.Color, activo.Motor, activo.Chasis,
            activo.Placa, activo.Cilindraje, activo.AnioMatriculacion, activo.AnioVehiculo, activo.Condicion.ToString(),
            activo.Estado.ToString(), activo.DepreciacionAcumulada, activo.Valor - activo.DepreciacionAcumulada,
            responsable?.Nombre, responsable?.IdResponsable));
    }

    [HttpPost]
    public async Task<ActionResult<ActivoRegistradoResult>> Registrar([FromBody] RegistrarActivoBody body, CancellationToken ct)
    {
        var resultado = await activoService.RegistrarAsync(
            new RegistrarActivoRequest(
                body.IdEstructura, body.IdAgencia, body.Codigo, body.Detalle, body.FechaCompra, body.Valor,
                body.Marca, body.Modelo, body.Serie, body.EsVehiculo, body.EsBienDeControl, body.Color, body.Motor,
                body.Chasis, body.Placa, body.Cilindraje, body.AnioMatriculacion, body.AnioVehiculo, body.Asegurado,
                body.IdResponsableInicial, User.Identity!.Name!),
            ct);
        return Created($"/api/activofijo/{resultado.IdActivo}", resultado);
    }

    [HttpPost("{id:guid}/responsable")]
    public async Task<IActionResult> AsignarResponsable(Guid id, [FromBody] AsignarResponsableBody body, CancellationToken ct)
    {
        await activoService.AsignarResponsableAsync(new AsignarResponsableRequest(id, body.IdResponsable, User.Identity!.Name!), ct);
        return NoContent();
    }

    [HttpPost("depreciacion/ejecutar")]
    public async Task<ActionResult<DepreciacionEjecutadaResult>> EjecutarDepreciacion(CancellationToken ct)
    {
        var resultado = await activoService.EjecutarDepreciacionAsync(User.Identity!.Name!, ct);
        return Ok(resultado);
    }

    [HttpGet("traslados")]
    public async Task<ActionResult<IReadOnlyList<TrasladoListItem>>> Traslados(CancellationToken ct)
    {
        var resultado = await db.TrasladosActivo
            .Include(t => t.Activo).Include(t => t.MotivoTraslado).Include(t => t.AgenciaOrigen).Include(t => t.AgenciaDestino)
            .OrderByDescending(t => t.CreadoEn)
            .Select(t => new TrasladoListItem(
                t.Id, t.Activo.Detalle, t.Concepto, t.Fecha, t.MotivoTraslado.Nombre,
                t.AgenciaOrigen.Nombre, t.AgenciaDestino.Nombre, t.Estado.ToString()))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("traslados")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<TrasladoCreadoResult>> CrearTraslado([FromBody] CrearTrasladoBody body, CancellationToken ct)
    {
        var resultado = await activoService.CrearTrasladoAsync(
            new CrearTrasladoRequest(
                body.IdActivo, body.Concepto, body.IdMotivoTraslado, body.Razon,
                body.IdResponsableDestino, body.IdAgenciaDestino, User.Identity!.Name!),
            ct);
        return Created($"/api/activofijo/traslados/{resultado.IdTraslado}", resultado);
    }

    [HttpPost("traslados/{id:guid}/procesar")]
    [RequireIdempotencyKey]
    public async Task<IActionResult> ProcesarTraslado(Guid id, CancellationToken ct)
    {
        await activoService.ProcesarTrasladoAsync(id, User.Identity!.Name!, ct);
        return NoContent();
    }

    [HttpPost("traslados/{id:guid}/anular")]
    public async Task<IActionResult> AnularTraslado(Guid id, CancellationToken ct)
    {
        await activoService.AnularTrasladoAsync(id, User.Identity!.Name!, ct);
        return NoContent();
    }

    [HttpGet("bajas")]
    public async Task<ActionResult<IReadOnlyList<SolicitudBajaListItem>>> Bajas(CancellationToken ct)
    {
        var resultado = await db.SolicitudesActivoBaja
            .Include(s => s.Activo).Include(s => s.MotivoBaja)
            .OrderByDescending(s => s.CreadoEn)
            .Select(s => new SolicitudBajaListItem(
                s.Id, s.Activo.Detalle, s.MotivoBaja.Detalle, s.Detalle, s.FechaSolicitud, s.Estado.ToString(), s.IdComprobante))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("bajas")]
    public async Task<ActionResult<SolicitudBajaCreadaResult>> SolicitarBaja([FromBody] SolicitarBajaBody body, CancellationToken ct)
    {
        var resultado = await activoService.SolicitarBajaAsync(
            new SolicitarBajaRequest(body.IdActivo, body.IdMotivoBaja, body.Detalle, User.Identity!.Name!), ct);
        return Created($"/api/activofijo/bajas/{resultado.IdSolicitud}", resultado);
    }

    [HttpPost("bajas/{id:guid}/autorizar")]
    public async Task<IActionResult> AutorizarBaja(Guid id, CancellationToken ct)
    {
        await activoService.AutorizarBajaAsync(id, User.Identity!.Name!, ct);
        return NoContent();
    }

    [HttpPost("bajas/{id:guid}/procesar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<BajaProcesadaResult>> ProcesarBaja(Guid id, CancellationToken ct)
    {
        var resultado = await activoService.ProcesarBajaAsync(id, User.Identity!.Name!, ct);
        return Ok(resultado);
    }
}

public record CrearResponsableBody(Guid IdPersona, int IdAgencia);
public record ActualizarResponsableBody(int IdAgencia, bool Activo);

public record RegistrarActivoBody(
    int IdEstructura, int IdAgencia, string? Codigo, string Detalle, DateOnly FechaCompra, decimal Valor,
    string? Marca, string? Modelo, string? Serie, bool EsVehiculo, bool EsBienDeControl, string? Color,
    string? Motor, string? Chasis, string? Placa, string? Cilindraje, int? AnioMatriculacion, int? AnioVehiculo,
    bool Asegurado, Guid? IdResponsableInicial);

public record AsignarResponsableBody(Guid IdResponsable);

public record CrearTrasladoBody(
    Guid IdActivo, string Concepto, int IdMotivoTraslado, string? Razon, Guid? IdResponsableDestino, int IdAgenciaDestino);

public record SolicitarBajaBody(Guid IdActivo, int IdMotivoBaja, string? Detalle);
