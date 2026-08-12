using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record SocioListItem(
    Guid Id, string Numero, string Nombre, string Identificacion,
    string Agencia, string Estado);

// Lectura simple: se consulta el DbContext directo desde el controller (sin
// pasar por Application) porque no hay lógica de negocio que orquestar, solo
// proyección de datos. Los casos de uso que escriben sí van por Application
// (ver ComprobanteContableService) — esta distinción es intencional, no
// un atajo por descuido.
[ApiController]
[Route("api/socios")]
public class SociosController(Corela15DbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SocioListItem>>> Listar(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Clientes
            .Include(c => c.Persona)
            .Include(c => c.Agencia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Persona.Nombre, $"%{q}%") ||
                EF.Functions.ILike(c.Numero, $"%{q}%") ||
                EF.Functions.ILike(c.Persona.Identificacion, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(c => c.Persona.Nombre)
            .Select(c => new SocioListItem(
                c.Id, c.Numero, c.Persona.Nombre, c.Persona.Identificacion,
                c.Agencia.Nombre, c.Estado.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }
}
