using Corela15.Application.Nomina;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record EmpleadoListItem(Guid Id, string Nombre, string Cargo);

public record RolPagosListItem(
    Guid Id, DateOnly Periodo, string Tipo, string Estado, int CantidadEmpleados, decimal TotalGeneral);

[ApiController]
[Route("api/nomina")]
[Authorize(Policy = "Menu:nomina")]
public class NominaController(Corela15DbContext db, IRolPagosService rolPagosService) : ControllerBase
{
    [HttpGet("empleados")]
    public async Task<ActionResult<IReadOnlyList<EmpleadoListItem>>> Empleados(CancellationToken cancellationToken)
    {
        var resultado = await db.Empleados
            .Include(e => e.Persona)
            .Where(e => e.Estado == Domain.Nomina.EstadoEmpleado.Activo)
            .OrderBy(e => e.Persona.Nombre)
            .Select(e => new EmpleadoListItem(e.Id, e.Persona.Nombre, e.Cargo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("roles-pagos")]
    public async Task<ActionResult<IReadOnlyList<RolPagosListItem>>> RolesPagos(CancellationToken cancellationToken)
    {
        var resultado = await db.RolesPagos
            .Include(r => r.Empleados)
            .OrderByDescending(r => r.Periodo)
            .Select(r => new RolPagosListItem(
                r.Id, r.Periodo, r.Tipo.ToString(), r.Estado.ToString(),
                r.Empleados.Count, r.Empleados.Sum(e => e.Total)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("roles-pagos")]
    public async Task<ActionResult<RolPagosGeneradoResult>> Generar(
        [FromBody] GenerarRolPagosRequest request, CancellationToken cancellationToken)
    {
        var resultado = await rolPagosService.GenerarAsync(request, cancellationToken);
        return Created($"/api/nomina/roles-pagos/{resultado.IdRolPagos}", resultado);
    }
}
