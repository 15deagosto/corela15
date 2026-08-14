using System.Security.Claims;
using System.Text.Json;
using Corela15.Application.Common;
using Corela15.Domain.Seguridad;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Corela15.Api.Idempotencia;

/// <summary>
/// Intercepta los endpoints marcados con <see cref="RequireIdempotencyKeyAttribute"/>
/// (los que mueven dinero real): exige el header `Idempotency-Key`, y si ya
/// se vio esa clave (para el mismo usuario y la misma ruta) devuelve la
/// respuesta guardada en vez de ejecutar el caso de uso otra vez — un
/// reintento de red por timeout, o un doble-clic en el frontend, ya no
/// duplica un depósito, un pago de cuota, o un abono.
/// </summary>
public class IdempotenciaFilter(Corela15DbContext db, IOptions<JsonOptions> jsonOptions) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var requiereIdempotencia = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireIdempotencyKeyAttribute>().Any();
        if (!requiereIdempotencia)
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var claves)
            || string.IsNullOrWhiteSpace(claves.ToString()))
        {
            throw new FaltaIdempotencyKeyException();
        }

        var clave = claves.ToString();
        var idUsuario = Guid.Parse(
            context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? context.HttpContext.User.FindFirst("sub")!.Value);
        var ruta = context.HttpContext.Request.Path.ToString();

        var existente = await db.SolicitudesIdempotentes.AsNoTracking().FirstOrDefaultAsync(
            s => s.Clave == clave && s.IdUsuario == idUsuario && s.Ruta == ruta,
            context.HttpContext.RequestAborted);

        if (existente is not null)
        {
            context.Result = new ContentResult
            {
                StatusCode = existente.CodigoEstado,
                Content = existente.CuerpoRespuesta,
                ContentType = "application/json",
            };
            return;
        }

        var ejecutado = await next();

        if (ejecutado.Result is ObjectResult { StatusCode: >= 200 and < 300 } resultado)
        {
            db.SolicitudesIdempotentes.Add(new SolicitudIdempotente
            {
                Id = Guid.NewGuid(),
                Clave = clave,
                IdUsuario = idUsuario,
                Ruta = ruta,
                CodigoEstado = resultado.StatusCode ?? 200,
                CuerpoRespuesta = JsonSerializer.Serialize(resultado.Value, jsonOptions.Value.JsonSerializerOptions),
                CreadoEn = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(context.HttpContext.RequestAborted);
        }
        // Si la operación falló (4xx/5xx), no se guarda nada — el cliente
        // puede reintentar con la misma clave sin quedar bloqueado por un
        // intento fallido.
    }
}
