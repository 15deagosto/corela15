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
        var empleados = await db.Empleados
            .Where(e => idsEmpleados.Contains(e.Id) && e.Estado == EstadoEmpleado.Activo)
            .ToListAsync(cancellationToken);

        foreach (var linea in request.Lineas)
        {
            if (empleados.All(e => e.Id != linea.IdEmpleado))
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

            // Sincroniza el sueldo actual del empleado con el rol de pagos
            // recién generado — el rol de pagos es la fuente real (ver
            // CLAUDE.md), este campo es solo una caché siempre alineada.
            var empleado = empleados.First(e => e.Id == linea.IdEmpleado);
            empleado.SueldoActual = linea.Ingresos;
        }

        db.RolesPagos.Add(rolPagos);
        await db.SaveChangesAsync(cancellationToken);

        var totalGeneral = rolPagos.Empleados.Sum(e => e.Total);
        return new RolPagosGeneradoResult(rolPagos.Id, rolPagos.Periodo, rolPagos.Tipo.ToString(), totalGeneral);
    }
}
