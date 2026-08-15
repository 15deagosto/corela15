using Corela15.Application.Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

[ApiController]
[Route("api/contabilidad/cierre-ejercicio")]
[Authorize(Policy = "Menu:contabilidad")]
public class CierreEjercicioController(ICierreEjercicioService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CierreEjercicioListItem>>> Listar(CancellationToken cancellationToken)
    {
        var resultado = await service.ListarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("cerrar")]
    public async Task<ActionResult<CierreEjercicioResult>> Cerrar(
        [FromBody] CerrarEjercicioBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.CerrarAsync(new CerrarEjercicioRequest(body.Anio, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }
}

public record CerrarEjercicioBody(int Anio);
