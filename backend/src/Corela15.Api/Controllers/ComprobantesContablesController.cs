using Corela15.Application.Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

[ApiController]
[Route("api/contabilidad/comprobantes")]
[Authorize(Policy = "Menu:contabilidad")]
public class ComprobantesContablesController(IComprobanteContableService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ComprobanteContableRegistradoResult>> Registrar(
        [FromBody] RegistrarComprobanteContableRequest request, CancellationToken cancellationToken)
    {
        // Errores de negocio (comprobante desbalanceado, cuenta inválida...)
        // los traduce DomainExceptionHandler de forma centralizada — ver
        // Corela15.Api/ExceptionHandling/DomainExceptionHandler.cs.
        var resultado = await service.RegistrarAsync(request, cancellationToken);
        return Created($"/api/contabilidad/comprobantes/{resultado.Id}", resultado);
    }
}
