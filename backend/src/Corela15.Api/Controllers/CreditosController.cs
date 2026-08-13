using Corela15.Application.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record TipoPrestamoListItem(
    int Id, string Codigo, string Nombre, decimal MontoMinimo, decimal MontoMaximo, decimal TasaAnual, bool Activo);

public record SolicitudPrestamoListItem(
    Guid Id, string Numero, string Socio, string Producto, decimal MontoSolicitado, int Cuotas, string Estado, DateOnly FechaSolicitud);

public record PrestamoListItem(
    Guid Id, string Numero, string Producto, decimal DeudaInicial, decimal Saldo, decimal Tasa, string Estado, DateOnly FechaAdjudicacion);

[ApiController]
[Route("api/creditos")]
public class CreditosController(Corela15DbContext db, IPrestamoService prestamoService) : ControllerBase
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
                p.Id, p.Numero, p.TipoPrestamo.Nombre, p.DeudaInicial, p.Saldo, p.Tasa, p.Estado.ToString(), p.FechaAdjudicacion))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("solicitudes")]
    public async Task<ActionResult<SolicitudPrestamoCreadaResult>> Solicitar(
        [FromBody] SolicitarPrestamoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.SolicitarAsync(request, cancellationToken);
        return Created($"/api/creditos/solicitudes/{resultado.IdSolicitud}", resultado);
    }

    [HttpPost("solicitudes/{idSolicitud:guid}/desembolsar")]
    public async Task<ActionResult<PrestamoDesembolsadoResult>> Desembolsar(
        Guid idSolicitud, [FromBody] DesembolsarBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.DesembolsarAsync(
            new DesembolsarPrestamoRequest(idSolicitud, body.RegistradoPor), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("prestamos/{idPrestamo:guid}/pagos")]
    public async Task<ActionResult<PagoCuotaRegistradoResult>> PagarCuota(
        Guid idPrestamo, [FromBody] DesembolsarBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.PagarCuotaAsync(
            new PagarCuotaRequest(idPrestamo, body.RegistradoPor), cancellationToken);
        return Ok(resultado);
    }
}

public record DesembolsarBody(string RegistradoPor);
