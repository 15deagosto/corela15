using Microsoft.AspNetCore.Mvc.Filters;

namespace Corela15.Api.Autorizacion;

/// <summary>
/// Intercepta los endpoints marcados con <see cref="RequireOpcionAttribute"/>
/// y exige el claim real "opcion" con ese código -- mismo criterio que
/// <c>Corela15.Api.Idempotencia.IdempotenciaFilter</c> (filtro global,
/// activado solo por el atributo, sin tocar Program.cs por cada código
/// nuevo). Nunca reemplaza la policy de menú del controller (que sigue
/// aplicando primero, vía [Authorize]) -- esto es un chequeo adicional,
/// más fino, encima de esa.
/// </summary>
public class OpcionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requerido = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireOpcionAttribute>().FirstOrDefault();

        if (requerido is not null && !context.HttpContext.User.HasClaim("opcion", requerido.Codigo))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
            return;
        }

        await next();
    }
}
