using Corela15.Application.Riesgo;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record ProcesoListItem(int Id, string Nombre, string MacroProceso, bool Critico);

public record NivelEscalaListItem(int Id, string Nombre, int Nivel);

public record EventoRiesgoListItem(
    Guid Id, string Proceso, string Descripcion, string NivelImpacto, string NivelProbabilidad,
    string NivelRiesgo, string ColorNivelRiesgo, DateOnly FechaIdentificacion);

[ApiController]
[Route("api/riesgo")]
[Authorize(Policy = "Menu:riesgo")]
public class RiesgoController(
    Corela15DbContext db, IEventoRiesgoService service, IIndicadorLiquidezService indicadorLiquidezService) : ControllerBase
{
    [HttpGet("procesos")]
    public async Task<ActionResult<IReadOnlyList<ProcesoListItem>>> Procesos(CancellationToken cancellationToken)
    {
        var resultado = await db.Procesos
            .Include(p => p.MacroProceso)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProcesoListItem(p.Id, p.Nombre, p.MacroProceso.Nombre, p.Critico))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("niveles-impacto")]
    public async Task<ActionResult<IReadOnlyList<NivelEscalaListItem>>> NivelesImpacto(CancellationToken cancellationToken)
    {
        var resultado = await db.NivelesImpacto
            .Where(n => n.Activo)
            .OrderBy(n => n.Nivel)
            .Select(n => new NivelEscalaListItem(n.Id, n.Nombre, n.Nivel))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("niveles-probabilidad")]
    public async Task<ActionResult<IReadOnlyList<NivelEscalaListItem>>> NivelesProbabilidad(CancellationToken cancellationToken)
    {
        var resultado = await db.NivelesProbabilidad
            .Where(n => n.Activo)
            .OrderBy(n => n.Nivel)
            .Select(n => new NivelEscalaListItem(n.Id, n.Nombre, n.Nivel))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("eventos")]
    public async Task<ActionResult<IReadOnlyList<EventoRiesgoListItem>>> Eventos(CancellationToken cancellationToken)
    {
        var resultado = await db.EventosRiesgo
            .Include(e => e.Proceso)
            .Include(e => e.NivelImpacto)
            .Include(e => e.NivelProbabilidad)
            .Include(e => e.NivelRiesgo)
            .OrderByDescending(e => e.FechaIdentificacion)
            .Select(e => new EventoRiesgoListItem(
                e.Id, e.Proceso.Nombre, e.Descripcion, e.NivelImpacto.Nombre, e.NivelProbabilidad.Nombre,
                e.NivelRiesgo.Nombre, e.NivelRiesgo.Color ?? "#999999", e.FechaIdentificacion))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("eventos")]
    public async Task<ActionResult<EventoRiesgoRegistradoResult>> Registrar(
        [FromBody] RegistrarEventoRiesgoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await service.RegistrarAsync(request, cancellationToken);
        return Created($"/api/riesgo/eventos/{resultado.IdEventoRiesgo}", resultado);
    }

    [HttpGet("liquidez")]
    public async Task<ActionResult<IReadOnlyList<IndicadorLiquidezResult>>> HistoricoLiquidez(CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.ListarHistoricoAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("liquidez/calcular")]
    public async Task<ActionResult<IndicadorLiquidezResult>> CalcularLiquidez(CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.CalcularAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("liquidez/parametro")]
    public async Task<ActionResult<ParametroLiquidezResult>> ParametroLiquidez(CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.ObtenerParametroAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPut("liquidez/parametro")]
    public async Task<ActionResult<ParametroLiquidezResult>> ActualizarParametroLiquidez(
        [FromBody] ActualizarParametroLiquidezBody body, CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.ActualizarParametroAsync(body.MinimoRegulatorio, User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }
}

public record ActualizarParametroLiquidezBody(decimal MinimoRegulatorio);
