using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record CuentaContableListItem(
    Guid Id, string Codigo, string Nombre, string Grupo, string Naturaleza,
    bool EsMayor, bool Activa, string? CodigoPadre);

[ApiController]
[Route("api/contabilidad/cuentas")]
[Authorize(Policy = "Menu:contabilidad")]
public class CuentasContablesController(Corela15DbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CuentaContableListItem>>> Listar(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.CuentasContables.Include(c => c.CuentaPadre).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Nombre, $"%{q}%") || EF.Functions.ILike(c.Codigo, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(c => c.Codigo)
            .Select(c => new CuentaContableListItem(
                c.Id, c.Codigo, c.Nombre, c.Grupo.ToString(), c.Naturaleza.ToString(),
                c.EsMayor, c.Activa, c.CuentaPadre != null ? c.CuentaPadre.Codigo : null))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }
}
