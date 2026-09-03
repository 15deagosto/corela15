using Corela15.Application.FlujoTrabajo;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class FlujoTrabajoService(Corela15DbContext db) : IFlujoTrabajoService
{
    public async Task<EtapaRegistrada> ValidarYRegistrarAsync(
        int idEtapa, int idAgencia, decimal monto, string registradoPor, CancellationToken cancellationToken = default)
    {
        var etapa = await db.Etapas.FirstOrDefaultAsync(e => e.Id == idEtapa, cancellationToken);
        var nombreEtapa = etapa?.Nombre ?? $"#{idEtapa}";

        var ruteo = await db.EtapasGrupoContable
            .Include(r => r.GrupoContable)
            .Where(r => r.IdEtapa == idEtapa && r.IdAgencia == idAgencia && r.Activa
                && r.GrupoContable.Activo && monto >= r.GrupoContable.MontoMinimo && monto <= r.GrupoContable.MontoMaximo)
            .OrderBy(r => r.Orden)
            .FirstOrDefaultAsync(cancellationToken);

        if (ruteo is null)
        {
            throw new EtapaNoConfiguradaException(nombreEtapa, idAgencia);
        }

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == registradoPor, cancellationToken);
        var autorizado = usuario is not null && await db.GruposContablesUsuarios
            .AnyAsync(gu => gu.CodigoGrupoContable == ruteo.CodigoGrupoContable && gu.IdUsuario == usuario.Id && gu.Activo, cancellationToken);

        if (!autorizado)
        {
            throw new UsuarioNoAutorizadoParaEtapaException(registradoPor, nombreEtapa);
        }

        return new EtapaRegistrada(idEtapa, ruteo.CodigoGrupoContable, ruteo.GrupoContable.Nombre);
    }
}
