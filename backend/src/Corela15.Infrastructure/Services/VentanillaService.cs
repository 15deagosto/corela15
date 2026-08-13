using Corela15.Application.Cajas;
using Corela15.Domain.Cajas;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class VentanillaService(Corela15DbContext db) : IVentanillaService
{
    public async Task<VentanillaAbiertaResult> AbrirAsync(
        AbrirVentanillaRequest request, CancellationToken cancellationToken = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var yaAbierta = await db.Ventanillas
            .AnyAsync(v => v.IdUsuario == request.IdUsuario && v.Fecha == hoy, cancellationToken);
        if (yaAbierta)
        {
            throw new VentanillaYaAbiertaException(request.IdUsuario);
        }

        var ventanilla = new Ventanilla
        {
            Id = Guid.NewGuid(),
            Fecha = hoy,
            IdAgencia = request.IdAgencia,
            IdUsuario = request.IdUsuario,
            Cuadrada = false,
            Cerrada = false,
            PuedeTransaccionar = true,
        };
        db.Ventanillas.Add(ventanilla);
        await db.SaveChangesAsync(cancellationToken);

        return new VentanillaAbiertaResult(ventanilla.Id, hoy);
    }

    public async Task<VentanillaCerradaResult> CerrarAsync(
        CerrarVentanillaRequest request, CancellationToken cancellationToken = default)
    {
        var ventanilla = await db.Ventanillas.FirstOrDefaultAsync(v => v.Id == request.IdVentanilla, cancellationToken);
        if (ventanilla is null || ventanilla.Cerrada)
        {
            throw new VentanillaInvalidaException(request.IdVentanilla);
        }

        ventanilla.Cerrada = true;
        ventanilla.Cuadrada = true;
        ventanilla.PuedeTransaccionar = false;

        var cuadre = new VentanillaCuadre
        {
            Id = Guid.NewGuid(),
            IdVentanilla = ventanilla.Id,
            FechaProceso = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalEfectivo = request.TotalEfectivoContado,
            TotalCheque = request.TotalCheque,
            Total = request.TotalEfectivoContado + request.TotalCheque,
            DiferenciaEfectivo = 0,
            DiferenciaCheque = 0,
            EstaCuadrado = true,
            Aprobada = false,
            Activa = true,
        };
        db.VentanillasCuadre.Add(cuadre);

        await db.SaveChangesAsync(cancellationToken);

        return new VentanillaCerradaResult(cuadre.Id, cuadre.EstaCuadrado);
    }
}
