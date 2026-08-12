using Corela15.Application.Contabilidad;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

[ApiController]
[Route("api/contabilidad/comprobantes")]
public class ComprobantesContablesController(IComprobanteContableService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ComprobanteContableRegistradoResult>> Registrar(
        [FromBody] RegistrarComprobanteContableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await service.RegistrarAsync(request, cancellationToken);
            return Created($"/api/contabilidad/comprobantes/{resultado.Id}", resultado);
        }
        catch (ComprobanteDesbalanceadoException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (CuentaContableInvalidaException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
        catch (ArgumentException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
