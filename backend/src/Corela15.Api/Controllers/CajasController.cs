using Corela15.Application.Cajas;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record UsuarioParaCajaListItem(Guid Id, string NombreUsuario);

public record VentanillaListItem(
    Guid Id, string Usuario, string Agencia, DateOnly Fecha, bool Cerrada, bool Cuadrada);

[ApiController]
[Route("api/cajas")]
public class CajasController(Corela15DbContext db, IVentanillaService ventanillaService) : ControllerBase
{
    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaCajaListItem>>> Usuarios(CancellationToken cancellationToken)
    {
        var resultado = await db.Usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioParaCajaListItem(u.Id, u.NombreUsuario))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("ventanillas")]
    public async Task<ActionResult<IReadOnlyList<VentanillaListItem>>> Ventanillas(CancellationToken cancellationToken)
    {
        var resultado = await db.Ventanillas
            .Include(v => v.Usuario)
            .Include(v => v.Agencia)
            .OrderByDescending(v => v.Fecha)
            .Select(v => new VentanillaListItem(v.Id, v.Usuario.NombreUsuario, v.Agencia.Nombre, v.Fecha, v.Cerrada, v.Cuadrada))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("ventanillas")]
    public async Task<ActionResult<VentanillaAbiertaResult>> Abrir(
        [FromBody] AbrirVentanillaRequest request, CancellationToken cancellationToken)
    {
        var resultado = await ventanillaService.AbrirAsync(request, cancellationToken);
        return Created($"/api/cajas/ventanillas/{resultado.IdVentanilla}", resultado);
    }

    [HttpPost("ventanillas/{idVentanilla:guid}/cerrar")]
    public async Task<ActionResult<VentanillaCerradaResult>> Cerrar(
        Guid idVentanilla, [FromBody] CerrarVentanillaBody body, CancellationToken cancellationToken)
    {
        var resultado = await ventanillaService.CerrarAsync(
            new CerrarVentanillaRequest(idVentanilla, body.TotalEfectivoContado, body.TotalCheque, body.RegistradoPor),
            cancellationToken);
        return Ok(resultado);
    }
}

public record CerrarVentanillaBody(decimal TotalEfectivoContado, decimal TotalCheque, string RegistradoPor);
