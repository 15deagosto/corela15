using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class CuentaContableAdminService(Corela15DbContext db) : ICuentaContableAdminService
{
    public async Task<CuentaContableCreadaResult> CrearAsync(
        CrearCuentaContableRequest request, CancellationToken cancellationToken = default)
    {
        if (await db.CuentasContables.AnyAsync(c => c.Codigo == request.Codigo, cancellationToken))
        {
            throw new CodigoDuplicadoException("una cuenta contable", request.Codigo);
        }

        if (!Enum.TryParse<GrupoCuc>(request.Grupo, ignoreCase: true, out var grupo))
        {
            throw new SolicitudInvalidaExceptionGenerica($"Grupo inválido: {request.Grupo}");
        }
        if (!Enum.TryParse<NaturalezaCuenta>(request.Naturaleza, ignoreCase: true, out var naturaleza))
        {
            throw new SolicitudInvalidaExceptionGenerica($"Naturaleza inválida: {request.Naturaleza}");
        }

        if (request.IdCuentaPadre is not null)
        {
            var padre = await db.CuentasContables.FirstOrDefaultAsync(c => c.Id == request.IdCuentaPadre, cancellationToken);
            if (padre is null || padre.EsMayor)
            {
                throw new CuentaContablePadreInvalidaException(request.IdCuentaPadre.Value);
            }
        }

        var cuenta = new CuentaContable
        {
            Id = Guid.NewGuid(),
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            Grupo = grupo,
            Naturaleza = naturaleza,
            IdCuentaPadre = request.IdCuentaPadre,
            EsMayor = request.EsMayor,
            Activa = true,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.RegistradoPor,
        };
        db.CuentasContables.Add(cuenta);
        await db.SaveChangesAsync(cancellationToken);

        return new CuentaContableCreadaResult(cuenta.Id, cuenta.Codigo);
    }

    public async Task ActualizarAsync(ActualizarCuentaContableRequest request, CancellationToken cancellationToken = default)
    {
        var cuenta = await db.CuentasContables.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cuenta is null)
        {
            throw new CuentaContableNoExisteException(request.Id);
        }

        if (!request.Activa && cuenta.Activa)
        {
            var saldoActual = await db.SaldosContables
                .Where(s => s.IdCuentaContable == cuenta.Id)
                .SumAsync(s => (decimal?)s.SaldoFinal, cancellationToken) ?? 0m;
            if (saldoActual != 0)
            {
                throw new CuentaContableConSaldoException(cuenta.Codigo);
            }

            var enUso = await db.TiposTransaccion.AnyAsync(
                t => t.Activo && (t.IdCuentaContableDebito == cuenta.Id || t.IdCuentaContableCredito == cuenta.Id),
                cancellationToken);
            if (enUso)
            {
                throw new CuentaContableEnUsoException(cuenta.Codigo);
            }
        }

        cuenta.Nombre = request.Nombre;
        cuenta.Activa = request.Activa;
        cuenta.ModificadoEn = DateTimeOffset.UtcNow;
        cuenta.ModificadoPor = request.RegistradoPor;

        await db.SaveChangesAsync(cancellationToken);
    }
}
