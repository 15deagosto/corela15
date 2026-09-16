using Corela15.Api.Idempotencia;
using Corela15.Application.Portafolio;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record InstitucionListItem(string Codigo, string Nombre, string TipoInstitucion, bool Activa);

public record InversionListItem(
    Guid Id, string Documento, string Institucion, decimal ValorNominal, decimal Tasa,
    DateOnly FechaCompra, DateOnly FechaVencimiento, string Estado,
    string? CodigoCalificacionRiesgo, string? NombreCalificacionRiesgo,
    string? CodigoCalificadoraRiesgo, string? NombreCalificadoraRiesgo,
    DateOnly? FechaUltimaCalificacion, decimal? ProvisionConstituida);

public record CalificacionRiesgoListItem(string Codigo, string Nombre, bool Activo);
public record CalificadoraRiesgoListItem(string Codigo, string Nombre, bool Activo);

public record CalificarRiesgoBody(
    string? CodigoCalificacionRiesgo, string? CodigoCalificadoraRiesgo,
    DateOnly? FechaUltimaCalificacion, decimal? ProvisionConstituida);

public record I02Cabecera(string CodigoEstructura, string Ruc, DateOnly FechaCorte, int NumeroTotalRegistros);

public record I02ElementoDetalle(
    string Documento, string CodigoInstitucion, string NombreInstitucion, DateOnly FechaCompra,
    DateOnly FechaVencimiento, string CuentaContable, decimal ValorLibros, string Estado,
    string? CodigoCalificacionRiesgo, string? CodigoCalificadoraRiesgo,
    DateOnly? FechaUltimaCalificacion, decimal Tasa, decimal? ProvisionConstituida);

public record I02Result(I02Cabecera Cabecera, IReadOnlyList<I02ElementoDetalle> Detalle);

[ApiController]
[Route("api/portafolio")]
[Authorize(Policy = "Menu:portafolio")]
public class PortafolioController(Corela15DbContext db, IInversionPortafolioService inversionService) : ControllerBase
{
    [HttpGet("instituciones")]
    public async Task<ActionResult<IReadOnlyList<InstitucionListItem>>> Instituciones(CancellationToken ct)
    {
        var resultado = await db.Instituciones
            .Include(i => i.TipoInstitucion)
            .Where(i => i.Activa)
            .OrderBy(i => i.Nombre)
            .Select(i => new InstitucionListItem(i.Codigo, i.Nombre, i.TipoInstitucion.Nombre, i.Activa))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("inversiones")]
    public async Task<ActionResult<IReadOnlyList<InversionListItem>>> Inversiones(CancellationToken ct)
    {
        var resultado = await db.InversionesPortafolio
            .Include(i => i.Institucion)
            .Include(i => i.CalificacionRiesgo)
            .Include(i => i.CalificadoraRiesgo)
            .OrderByDescending(i => i.FechaCompra)
            .Select(i => new InversionListItem(
                i.Id, i.Documento, i.Institucion.Nombre, i.ValorNominal, i.Tasa, i.FechaCompra, i.FechaVencimiento, i.Estado.ToString(),
                i.CodigoCalificacionRiesgo, i.CalificacionRiesgo == null ? null : i.CalificacionRiesgo.Nombre,
                i.CodigoCalificadoraRiesgo, i.CalificadoraRiesgo == null ? null : i.CalificadoraRiesgo.Nombre,
                i.FechaUltimaCalificacion, i.ProvisionConstituida))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("calificaciones-riesgo")]
    public async Task<ActionResult<IReadOnlyList<CalificacionRiesgoListItem>>> CalificacionesRiesgo(CancellationToken ct)
    {
        var resultado = await db.CalificacionesRiesgo.Where(c => c.Activo).OrderBy(c => c.Codigo)
            .Select(c => new CalificacionRiesgoListItem(c.Codigo, c.Nombre, c.Activo)).ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("calificadoras-riesgo")]
    public async Task<ActionResult<IReadOnlyList<CalificadoraRiesgoListItem>>> CalificadorasRiesgo(CancellationToken ct)
    {
        var resultado = await db.CalificadorasRiesgo.Where(c => c.Activo).OrderBy(c => c.Codigo)
            .Select(c => new CalificadoraRiesgoListItem(c.Codigo, c.Nombre, c.Activo)).ToListAsync(ct);
        return Ok(resultado);
    }

    // Directo contra el DbContext — es una actualización de datos de
    // referencia externa sobre una inversión ya existente (misma
    // calificación que trae el archivo I02 real), sin ningún invariante
    // contable de por medio, mismo criterio que los flags simples del
    // resto del core (AcreditaPrestamo/DebitoSpi).
    [HttpPatch("inversiones/{id:guid}/calificacion-riesgo")]
    public async Task<IActionResult> CalificarRiesgo(Guid id, [FromBody] CalificarRiesgoBody body, CancellationToken ct)
    {
        var inversion = await db.InversionesPortafolio.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (inversion is null) return NotFound();

        if (body.CodigoCalificacionRiesgo is not null &&
            !await db.CalificacionesRiesgo.AnyAsync(c => c.Codigo == body.CodigoCalificacionRiesgo && c.Activo, ct))
            return UnprocessableEntity(new { detail = $"La calificación de riesgo '{body.CodigoCalificacionRiesgo}' no existe o está inactiva." });

        if (body.CodigoCalificadoraRiesgo is not null &&
            !await db.CalificadorasRiesgo.AnyAsync(c => c.Codigo == body.CodigoCalificadoraRiesgo && c.Activo, ct))
            return UnprocessableEntity(new { detail = $"La calificadora de riesgo '{body.CodigoCalificadoraRiesgo}' no existe o está inactiva." });

        inversion.CodigoCalificacionRiesgo = body.CodigoCalificacionRiesgo;
        inversion.CodigoCalificadoraRiesgo = body.CodigoCalificadoraRiesgo;
        inversion.FechaUltimaCalificacion = body.FechaUltimaCalificacion;
        inversion.ProvisionConstituida = body.ProvisionConstituida;
        inversion.ModificadoEn = DateTimeOffset.UtcNow;
        inversion.ModificadoPor = User.Identity!.Name!;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // Estructura real I02 (SEPS, "Saldos de Inversiones") — verificado
    // contra el archivo real de referencia (agosto 2026): cuentaContable
    // por inversión se resuelve con la misma lógica real ya usada al
    // abrir/renovar (ResolverCuentaInversion, por plazo × sector de la
    // institución contraparte), nunca un valor fijo ni recalculado a
    // mano — un solo lugar de verdad para esa regla.
    [HttpGet("reportes/i02")]
    public async Task<ActionResult<I02Result>> ReporteI02(CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var empresa = await db.Empresas.FirstOrDefaultAsync(ct);
        var ruc = empresa?.Ruc ?? string.Empty;

        var inversiones = await db.InversionesPortafolio
            .Include(i => i.Institucion).ThenInclude(inst => inst.TipoInstitucion)
            .Where(i => i.Estado == Corela15.Domain.Portafolio.EstadoInversionPortafolio.Activa)
            .OrderBy(i => i.FechaCompra)
            .ToListAsync(ct);

        var detalle = inversiones.Select(i =>
        {
            var diasPlazo = i.FechaVencimiento.DayNumber - i.FechaCompra.DayNumber;
            var esSectorPopular = InversionPortafolioService.EsSectorFinancieroPopular(i.Institucion.CodigoTipoInstitucion);
            var cuenta = InversionPortafolioService.ResolverCuentaInversion(diasPlazo, esSectorPopular);
            return new I02ElementoDetalle(
                i.Documento, i.CodigoInstitucion, i.Institucion.Nombre, i.FechaCompra, i.FechaVencimiento,
                cuenta, i.ValorNominal, i.Estado.ToString(),
                i.CodigoCalificacionRiesgo, i.CodigoCalificadoraRiesgo, i.FechaUltimaCalificacion, i.Tasa, i.ProvisionConstituida);
        }).ToList();

        return Ok(new I02Result(new I02Cabecera("I02", ruc, hoy, detalle.Count), detalle));
    }

    [HttpPost("inversiones")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<InversionAbiertaResult>> Abrir([FromBody] AbrirInversionBody body, CancellationToken ct)
    {
        var resultado = await inversionService.AbrirAsync(
            new AbrirInversionRequest(
                body.Documento, body.IdAgencia, body.CodigoInstitucion, body.ValorNominal, body.Tasa,
                body.FechaCompra, body.FechaVencimiento, User.Identity!.Name!),
            ct);
        return Created($"/api/portafolio/inversiones/{resultado.IdInversion}", resultado);
    }

    [HttpPost("inversiones/{id:guid}/cancelar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<InversionCanceladaResult>> Cancelar(Guid id, CancellationToken ct)
    {
        var resultado = await inversionService.CancelarAsync(new CancelarInversionRequest(id, User.Identity!.Name!), ct);
        return Ok(resultado);
    }

    [HttpPost("inversiones/{id:guid}/renovar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<InversionRenovadaResult>> Renovar(Guid id, [FromBody] RenovarInversionBody body, CancellationToken ct)
    {
        var resultado = await inversionService.RenovarAsync(
            new RenovarInversionRequest(id, body.DocumentoNuevo, body.FechaVencimientoNueva, User.Identity!.Name!), ct);
        return Ok(resultado);
    }
}

public record AbrirInversionBody(
    string Documento, int IdAgencia, string CodigoInstitucion, decimal ValorNominal, decimal Tasa,
    DateOnly FechaCompra, DateOnly FechaVencimiento);

public record RenovarInversionBody(string DocumentoNuevo, DateOnly FechaVencimientoNueva);
