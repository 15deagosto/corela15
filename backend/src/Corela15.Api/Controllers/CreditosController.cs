using Corela15.Api.Idempotencia;
using Corela15.Application.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record TipoPrestamoListItem(
    int Id, string Codigo, string Nombre, decimal MontoMinimo, decimal MontoMaximo, decimal TasaAnual, bool Activo);

public record SolicitudPrestamoListItem(
    Guid Id, string Numero, string Socio, string Producto, decimal MontoSolicitado, int Cuotas, string Estado, DateOnly FechaSolicitud);

public record PrestamoListItem(
    Guid Id, string Numero, string Producto, decimal DeudaInicial, decimal Saldo, decimal Tasa, string Estado,
    DateOnly FechaAdjudicacion, bool DebitoSpi);

[ApiController]
[Route("api/creditos")]
[Authorize(Policy = "Menu:creditos")]
public class CreditosController(
    Corela15DbContext db, IPrestamoService prestamoService, IProvisionCarteraService provisionCarteraService,
    IScoreCrediticioService scoreCrediticioService, IAutoDebitoSpiService autoDebitoSpiService) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<ActionResult<IReadOnlyList<TipoPrestamoListItem>>> Productos(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposPrestamo
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoPrestamoListItem(t.Id, t.Codigo, t.Nombre, t.MontoMinimo, t.MontoMaximo, t.TasaAnual, t.Activo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("solicitudes")]
    public async Task<ActionResult<IReadOnlyList<SolicitudPrestamoListItem>>> Solicitudes(CancellationToken cancellationToken)
    {
        var resultado = await db.SolicitudesPrestamo
            .Include(s => s.Cliente).ThenInclude(c => c.Persona)
            .Include(s => s.TipoPrestamo)
            .OrderByDescending(s => s.FechaSolicitud)
            .Select(s => new SolicitudPrestamoListItem(
                s.Id, s.Numero, s.Cliente.Persona.Nombre, s.TipoPrestamo.Nombre,
                s.MontoSolicitado, s.Cuotas, s.Estado.ToString(), s.FechaSolicitud))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("prestamos")]
    public async Task<ActionResult<IReadOnlyList<PrestamoListItem>>> Prestamos(CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .OrderByDescending(p => p.FechaAdjudicacion)
            .Select(p => new PrestamoListItem(
                p.Id, p.Numero, p.TipoPrestamo.Nombre, p.DeudaInicial, p.Saldo, p.Tasa, p.Estado.ToString(),
                p.FechaAdjudicacion, p.DebitoSpi))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("solicitudes")]
    public async Task<ActionResult<SolicitudPrestamoCreadaResult>> Solicitar(
        [FromBody] SolicitarPrestamoBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.SolicitarAsync(
            new SolicitarPrestamoRequest(body.IdCliente, body.IdTipoPrestamo, body.IdAgencia, body.MontoSolicitado, body.Cuotas, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/creditos/solicitudes/{resultado.IdSolicitud}", resultado);
    }

    [HttpPost("solicitudes/{idSolicitud:guid}/desembolsar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PrestamoDesembolsadoResult>> Desembolsar(
        Guid idSolicitud, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.DesembolsarAsync(
            new DesembolsarPrestamoRequest(idSolicitud, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("prestamos/{idPrestamo:guid}/pagos")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PagoCuotaRegistradoResult>> PagarCuota(
        Guid idPrestamo, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.PagarCuotaAsync(
            new PagarCuotaRequest(idPrestamo, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("provision-cartera/calcular")]
    public async Task<ActionResult<ProvisionCarteraCalculadaResult>> CalcularProvisionCartera(CancellationToken cancellationToken)
    {
        var resultado = await provisionCarteraService.EjecutarCalculoAsync(User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("clientes/{idCliente:guid}/score")]
    public async Task<ActionResult<ScoreCrediticioResult>> CalcularScore(Guid idCliente, CancellationToken cancellationToken)
    {
        var resultado = await scoreCrediticioService.CalcularAsync(idCliente, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("clientes/{idCliente:guid}/score/historial")]
    public async Task<ActionResult<IReadOnlyList<ScoreCrediticioResult>>> HistorialScore(Guid idCliente, CancellationToken cancellationToken)
    {
        var resultado = await scoreCrediticioService.HistorialAsync(idCliente, cancellationToken);
        return Ok(resultado);
    }

    [HttpPatch("prestamos/{idPrestamo:guid}/debito-spi")]
    public async Task<IActionResult> ConfigurarDebitoSpi(
        Guid idPrestamo, [FromBody] ConfigurarDebitoSpiBody body, CancellationToken cancellationToken)
    {
        await autoDebitoSpiService.ConfigurarAsync(idPrestamo, body.Activar, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpPost("auto-debito-spi/ejecutar")]
    public async Task<ActionResult<AutoDebitoSpiEjecutadoResult>> EjecutarAutoDebitoSpi(CancellationToken cancellationToken)
    {
        var resultado = await autoDebitoSpiService.EjecutarAsync(User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("auto-debito-spi/historial")]
    public async Task<ActionResult<IReadOnlyList<AutoDebitoSpiDetalle>>> HistorialAutoDebitoSpi(CancellationToken cancellationToken)
    {
        var resultado = await autoDebitoSpiService.HistorialAsync(cancellationToken);
        return Ok(resultado);
    }
}

public record ConfigurarDebitoSpiBody(bool Activar);

public record SolicitarPrestamoBody(Guid IdCliente, int IdTipoPrestamo, int IdAgencia, decimal MontoSolicitado, int Cuotas);
