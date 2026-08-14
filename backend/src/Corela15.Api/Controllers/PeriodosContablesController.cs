using Corela15.Application.Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

[ApiController]
[Route("api/contabilidad/periodos")]
[Authorize(Policy = "Menu:contabilidad")]
public class PeriodosContablesController(ICierrePeriodoService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PeriodoContableListItem>>> Listar(CancellationToken cancellationToken)
    {
        var resultado = await service.ListarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("cerrar")]
    public async Task<ActionResult<PeriodoCerradoResult>> Cerrar(
        [FromBody] CerrarPeriodoBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.CerrarAsync(new CerrarPeriodoRequest(body.Periodo, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }
}

public record CerrarPeriodoBody(DateOnly Periodo);
