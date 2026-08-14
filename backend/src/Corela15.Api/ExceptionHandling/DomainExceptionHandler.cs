using Corela15.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.ExceptionHandling;

/// <summary>
/// Traduce cualquier <see cref="DomainException"/> lanzada desde un caso de
/// uso (Application) a una respuesta HTTP consistente, sin que cada
/// controller tenga que enumerar try/catch por tipo de excepción — un caso
/// de uso nuevo (ej. desembolso de préstamo) queda con manejo de errores
/// correcto por el solo hecho de heredar de DomainException/
/// ReglaDeNegocioException/SolicitudInvalidaException.
/// </summary>
public class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
        {
            return false;
        }

        httpContext.Response.StatusCode = domainException.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = domainException.StatusCode,
                Title = domainException.StatusCode switch
                {
                    400 => "Bad Request",
                    401 => "Unauthorized",
                    409 => "Conflict",
                    422 => "Unprocessable Entity",
                    _ => "Error",
                },
                Detail = domainException.Message,
            },
            cancellationToken);

        return true;
    }
}
