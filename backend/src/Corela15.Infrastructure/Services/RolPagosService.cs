using Corela15.Application.Nomina;
using Corela15.Domain.Nomina;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class RolPagosService(Corela15DbContext db) : IRolPagosService
{
    public async Task<RolPagosGeneradoResult> GenerarAsync(
        GenerarRolPagosRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lineas.Count == 0)
        {
            throw new RolPagosSinLineasException();
        }

        if (!Enum.TryParse<TipoRolPagos>(request.Tipo, ignoreCase: true, out var tipo))
        {
            throw new TipoRolPagosInvalidoException(request.Tipo);
        }

        var yaExiste = await db.RolesPagos
            .AnyAsync(r => r.Periodo == request.Periodo && r.Tipo == tipo, cancellationToken);
        if (yaExiste)
        {
            throw new RolPagosDuplicadoException(request.Periodo, request.Tipo);
        }

        var idsEmpleados = request.Lineas.Select(l => l.IdEmpleado).ToList();
        var empleadosActivos = await db.Empleados
            .Where(e => idsEmpleados.Contains(e.Id) && e.Estado == EstadoEmpleado.Activo)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        foreach (var linea in request.Lineas)
        {
            if (!empleadosActivos.Contains(linea.IdEmpleado))
            {
                throw new EmpleadoInvalidoException(linea.IdEmpleado);
            }
        }

        var rolPagos = new RolPagos
        {
            Id = Guid.NewGuid(),
            Periodo = request.Periodo,
            Tipo = tipo,
            Estado = EstadoRolPagos.Procesado,
        };

        foreach (var linea in request.Lineas)
        {
            rolPagos.Empleados.Add(new RolPagosEmpleado
            {
                Id = Guid.NewGuid(),
                IdRolPagos = rolPagos.Id,
                IdEmpleado = linea.IdEmpleado,
                Ingresos = linea.Ingresos,
                Egresos = linea.Egresos,
                Total = linea.Ingresos - linea.Egresos,
                DiasLaborados = linea.DiasLaborados,
                Anulado = false,
            });
        }

        db.RolesPagos.Add(rolPagos);
        await db.SaveChangesAsync(cancellationToken);

        var totalGeneral = rolPagos.Empleados.Sum(e => e.Total);
        return new RolPagosGeneradoResult(rolPagos.Id, rolPagos.Periodo, rolPagos.Tipo.ToString(), totalGeneral);
    }
}
