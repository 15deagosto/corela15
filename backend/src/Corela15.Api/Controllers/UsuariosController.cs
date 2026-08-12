using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record UsuarioListItem(
    Guid Id, string NombreUsuario, string? NombrePersona,
    string Agencia, bool Activo, IReadOnlyList<string> Roles);

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(Corela15DbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioListItem>>> Listar(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Usuarios
            .Include(u => u.Persona)
            .Include(u => u.Agencia)
            .Include(u => u.UsuarioRoles.Where(ur => ur.Activo))
                .ThenInclude(ur => ur.Rol)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.NombreUsuario, $"%{q}%") ||
                (u.Persona != null && EF.Functions.ILike(u.Persona.Nombre, $"%{q}%")));
        }

        var resultado = await query
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioListItem(
                u.Id, u.NombreUsuario, u.Persona != null ? u.Persona.Nombre : null,
                u.Agencia.Nombre, u.Activo,
                u.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }
}
