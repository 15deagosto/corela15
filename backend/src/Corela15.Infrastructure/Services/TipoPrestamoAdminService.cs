using Corela15.Application.Colocacion;
using Corela15.Application.Common;
using Corela15.Domain.Credito;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class TipoPrestamoAdminService(Corela15DbContext db) : ITipoPrestamoAdminService
{
    public async Task<TipoPrestamoAdminResult> CrearAsync(
        CrearTipoPrestamoRequest request, CancellationToken cancellationToken = default)
    {
        if (await db.TiposPrestamo.AnyAsync(t => t.Codigo == request.Codigo, cancellationToken))
        {
            throw new CodigoDuplicadoException("un tipo de préstamo", request.Codigo);
        }

        await ValidarTasaContraTechoAsync(request.TasaAnual, request.SegmentoBce, cancellationToken);
        await ValidarTipoSeguroAsync(request.CodigoTipoSeguro, cancellationToken);

        var tipo = new TipoPrestamo
        {
            Codigo = request.Codigo,
            Nombre = request.Nombre,
            MontoMinimo = request.MontoMinimo,
            MontoMaximo = request.MontoMaximo,
            PlazoMinimoDias = request.PlazoMinimoDias,
            PlazoMaximoDias = request.PlazoMaximoDias,
            TasaAnual = request.TasaAnual,
            SegmentoBce = request.SegmentoBce,
            CodigoTipoCreditoSeps = request.CodigoTipoCreditoSeps,
            CodigoTipoSeguro = request.CodigoTipoSeguro,
            Activo = true,
        };
        db.TiposPrestamo.Add(tipo);
        await db.SaveChangesAsync(cancellationToken);

        return new TipoPrestamoAdminResult(tipo.Id, tipo.Codigo);
    }

    public async Task ActualizarAsync(int id, ActualizarTipoPrestamoRequest request, CancellationToken cancellationToken = default)
    {
        var tipo = await db.TiposPrestamo.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tipo is null)
        {
            throw new TipoPrestamoNoExisteException(id);
        }

        await ValidarTasaContraTechoAsync(request.TasaAnual, request.SegmentoBce, cancellationToken);
        await ValidarTipoSeguroAsync(request.CodigoTipoSeguro, cancellationToken);

        tipo.Nombre = request.Nombre;
        tipo.MontoMinimo = request.MontoMinimo;
        tipo.MontoMaximo = request.MontoMaximo;
        tipo.PlazoMinimoDias = request.PlazoMinimoDias;
        tipo.PlazoMaximoDias = request.PlazoMaximoDias;
        tipo.TasaAnual = request.TasaAnual;
        tipo.SegmentoBce = request.SegmentoBce;
        tipo.CodigoTipoCreditoSeps = request.CodigoTipoCreditoSeps;
        tipo.CodigoTipoSeguro = request.CodigoTipoSeguro;
        tipo.Activo = request.Activo;

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarTipoSeguroAsync(string? codigoTipoSeguro, CancellationToken cancellationToken)
    {
        if (codigoTipoSeguro is null) return;
        var existe = await db.TiposSeguro.AnyAsync(t => t.Codigo == codigoTipoSeguro && t.Activo, cancellationToken);
        if (!existe)
        {
            throw new TipoSeguroInvalidoException(codigoTipoSeguro);
        }
    }

    private async Task ValidarTasaContraTechoAsync(decimal tasaAnual, string segmentoBce, CancellationToken cancellationToken)
    {
        var techoVigente = await db.TasasTechoBce
            .Where(t => t.Segmento == segmentoBce && t.Activo && t.FechaVigenciaDesde <= DateOnly.FromDateTime(DateTime.UtcNow))
            .OrderByDescending(t => t.FechaVigenciaDesde)
            .FirstOrDefaultAsync(cancellationToken);
        if (techoVigente is null)
        {
            throw new SegmentoBceInvalidoException(segmentoBce);
        }
        if (tasaAnual > techoVigente.TasaMaxima)
        {
            throw new TasaExcedeTechoBceException(tasaAnual, techoVigente.TasaMaxima, segmentoBce);
        }
    }
}
