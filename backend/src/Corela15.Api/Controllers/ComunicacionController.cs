using System.Security.Claims;
using Corela15.Api.Idempotencia;
using Corela15.Application.Comunicacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

public record CrearCanalBody(string Nombre, string? Descripcion, IReadOnlyList<Guid> IdsMiembrosIniciales);

public record AgregarMiembroBody(Guid IdUsuario);

public record ReenviarMensajeBody(Guid IdCanalDestino);

/// <summary>Cuerpo del envío — multipart/form-data, el archivo (opcional) viaja junto con el texto (opcional) en el mismo request; un mensaje real siempre trae al menos uno de los dos.</summary>
public class EnviarMensajeBody
{
    public string? Texto { get; set; }
    public IFormFile? Archivo { get; set; }
}

/// <summary>
/// Comunicación interna real — canales de grupo y mensajes directos 1:1
/// dentro del propio core, con entrega en tiempo real vía
/// <see cref="Corela15.Api.Hubs.ComunicacionHub"/>. Reemplaza al módulo de
/// WhatsApp (decisión explícita del usuario: sin costo, sin depender de
/// Meta, todo dentro de la app real de la cooperativa).
/// </summary>
[ApiController]
[Route("api/comunicacion")]
[Authorize(Policy = "Menu:comunicacion-interna")]
public class ComunicacionController(IComunicacionService service) : ControllerBase
{
    [HttpGet("canales")]
    public async Task<ActionResult<IReadOnlyList<CanalListItemDto>>> ListarCanales(CancellationToken cancellationToken) =>
        Ok(await service.ListarCanalesAsync(IdUsuarioActual(), cancellationToken));

    // Autoservicio real: cualquier canal de grupo activo, marcando cuáles
    // ya integra el usuario — para poder unirse con un clic a uno que no
    // creó él (ej. "Cajas"/"Balcón de Servicio", sembrados desde el día
    // uno, o uno nuevo que armó otra persona).
    [HttpGet("canales/descubrir")]
    public async Task<ActionResult<IReadOnlyList<CanalDescubribleDto>>> Descubrir(CancellationToken cancellationToken) =>
        Ok(await service.ListarCanalesDescubriblesAsync(IdUsuarioActual(), cancellationToken));

    [HttpPost("canales")]
    public async Task<ActionResult<CanalDto>> CrearCanal([FromBody] CrearCanalBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.CrearCanalAsync(
            new CrearCanalRequest(body.Nombre, body.Descripcion, body.IdsMiembrosIniciales, IdUsuarioActual()),
            cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("directo/{idUsuario:guid}")]
    public async Task<ActionResult<CanalDto>> ObtenerODirecto(Guid idUsuario, CancellationToken cancellationToken) =>
        Ok(await service.ObtenerOCrearDirectoAsync(IdUsuarioActual(), idUsuario, cancellationToken));

    [HttpPost("canales/{idCanal:guid}/miembros")]
    public async Task<IActionResult> AgregarMiembro(
        Guid idCanal, [FromBody] AgregarMiembroBody body, CancellationToken cancellationToken)
    {
        await service.AgregarMiembroAsync(idCanal, body.IdUsuario, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpPost("canales/{idCanal:guid}/salir")]
    public async Task<IActionResult> Salir(Guid idCanal, CancellationToken cancellationToken)
    {
        await service.SalirDelCanalAsync(idCanal, IdUsuarioActual(), cancellationToken);
        return NoContent();
    }

    [HttpGet("canales/{idCanal:guid}/mensajes")]
    public async Task<ActionResult<IReadOnlyList<MensajeDto>>> ListarMensajes(
        Guid idCanal, [FromQuery] Guid? antesDe, CancellationToken cancellationToken) =>
        Ok(await service.ListarMensajesAsync(idCanal, IdUsuarioActual(), antesDe, cancellationToken));

    [HttpPost("canales/{idCanal:guid}/mensajes")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<MensajeDto>> EnviarMensaje(
        Guid idCanal, [FromForm] EnviarMensajeBody body, CancellationToken cancellationToken)
    {
        Stream? stream = null;
        try
        {
            ArchivoAdjuntoEntrada? archivo = null;
            if (body.Archivo is not null)
            {
                stream = body.Archivo.OpenReadStream();
                archivo = new ArchivoAdjuntoEntrada(stream, body.Archivo.FileName, body.Archivo.ContentType, body.Archivo.Length);
            }

            return Ok(await service.EnviarMensajeAsync(idCanal, IdUsuarioActual(), body.Texto, archivo, cancellationToken));
        }
        finally
        {
            if (stream is not null) await stream.DisposeAsync();
        }
    }

    [HttpPost("mensajes/{idMensaje:guid}/reenviar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<MensajeDto>> ReenviarMensaje(
        Guid idMensaje, [FromBody] ReenviarMensajeBody body, CancellationToken cancellationToken) =>
        Ok(await service.ReenviarMensajeAsync(idMensaje, body.IdCanalDestino, IdUsuarioActual(), cancellationToken));

    [HttpGet("mensajes/{idMensaje:guid}/adjunto")]
    public async Task<IActionResult> DescargarAdjunto(Guid idMensaje, CancellationToken cancellationToken)
    {
        var resultado = await service.DescargarAdjuntoAsync(idMensaje, IdUsuarioActual(), cancellationToken);
        return File(resultado.Contenido, string.IsNullOrWhiteSpace(resultado.ContentType) ? "application/octet-stream" : resultado.ContentType, resultado.NombreArchivo);
    }

    [HttpPost("canales/{idCanal:guid}/marcar-leido")]
    public async Task<IActionResult> MarcarLeido(Guid idCanal, CancellationToken cancellationToken)
    {
        await service.MarcarLeidoAsync(idCanal, IdUsuarioActual(), cancellationToken);
        return NoContent();
    }

    [HttpGet("usuarios/buscar")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaChatDto>>> BuscarUsuarios(
        [FromQuery] string? q, CancellationToken cancellationToken) =>
        Ok(await service.BuscarUsuariosAsync(q, IdUsuarioActual(), cancellationToken));

    private Guid IdUsuarioActual() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")!.Value);
}
