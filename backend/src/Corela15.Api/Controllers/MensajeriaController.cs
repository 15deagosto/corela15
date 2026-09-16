using System.Security.Claims;
using Corela15.Api.Idempotencia;
using Corela15.Application.Mensajeria;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record EnviarWhatsappBody(string NumeroDestino, Guid? IdPersonaDestino, string Texto);

public record PersonaConTelefonosItem(Guid Id, string Nombre, string Identificacion, IReadOnlyList<string> Telefonos);

/// <summary>
/// Mensajería saliente de WhatsApp — pensado para el caso real de un
/// usuario (ej. cajera) sin acceso a la app de WhatsApp que necesita
/// avisarle algo puntual a un socio usando un número real de la
/// cooperativa. Ver <see cref="Corela15.Domain.Mensajeria.MensajeWhatsapp"/>
/// para el detalle completo (por qué viaja como plantilla, y la
/// limitación real de "solo envío", sin bandeja de respuestas).
/// </summary>
[ApiController]
[Route("api/mensajeria")]
[Authorize(Policy = "Menu:mensajeria-whatsapp")]
public class MensajeriaController(IWhatsAppService service, Corela15DbContext db) : ControllerBase
{
    [HttpPost("whatsapp/enviar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<MensajeWhatsappDto>> Enviar(
        [FromBody] EnviarWhatsappBody body, CancellationToken cancellationToken)
    {
        // La agencia se deriva del token (claim "agencia", ver
        // AuthService/AuthController), nunca del cliente — mismo criterio
        // que RegistradoPor en Ahorros/Créditos/Cajas: quien envía no elige
        // por su cuenta desde qué agencia queda registrado el mensaje.
        var idAgencia = int.Parse(User.FindFirst("agencia")!.Value);

        var resultado = await service.EnviarAsync(
            new EnviarMensajeWhatsappRequest(
                body.NumeroDestino, body.IdPersonaDestino, body.Texto, idAgencia, User.Identity!.Name!),
            cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("whatsapp/historial")]
    public async Task<ActionResult<IReadOnlyList<MensajeWhatsappDto>>> Historial(
        [FromQuery] int? dias, CancellationToken cancellationToken)
    {
        var resultado = await service.ListarAsync(new ListarMensajesWhatsappFiltro(dias), cancellationToken);
        return Ok(resultado);
    }

    // Buscador propio, scoped a Menu:mensajeria-whatsapp -- no reutiliza
    // /api/socios/buscar-persona porque un usuario con este menú (ej. una
    // cajera) no necesariamente tiene Menu:socios (mismo criterio ya
    // aplicado en NominaController.BuscarPersona). Devuelve los teléfonos
    // ya capturados de la persona (sujeto.persona_telefono) para que la
    // pantalla pueda ofrecerlos directo, sin que la cajera tenga que
    // volver a tipear un número que el sistema ya tiene.
    [HttpGet("personas/buscar")]
    public async Task<ActionResult<IReadOnlyList<PersonaConTelefonosItem>>> BuscarPersona(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Personas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(p => EF.Functions.ILike(p.Nombre, $"%{q}%") || EF.Functions.ILike(p.Identificacion, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(p => p.Nombre)
            .Take(20)
            .Select(p => new PersonaConTelefonosItem(
                p.Id, p.Nombre, p.Identificacion,
                db.PersonasTelefonos.Where(t => t.IdPersona == p.Id && t.Activo).Select(t => t.Telefono).ToList()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }
}
