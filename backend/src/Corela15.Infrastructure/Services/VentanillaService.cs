using Corela15.Application.Cajas;
using Corela15.Application.Common;
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

        // Saldo esperado real (ver VentanillaItemCaja/ComprobanteContableService)
        // — antes el cuadre solo registraba el conteo declarado como
        // "cuadrado" por definición, sin nada real contra qué compararlo.
        // Sin movimientos de efectivo reales todavía (ventanilla recién
        // abierta y cerrada sin transacciones), el saldo esperado es 0.
        var itemCajaEfectivo = await db.VentanillasItemCaja
            .Include(v => v.ItemCaja)
            .FirstOrDefaultAsync(v => v.IdVentanilla == ventanilla.Id && v.ItemCaja.Codigo == "EFE", cancellationToken);
        var saldoEsperado = itemCajaEfectivo?.Saldo ?? 0;
        var diferenciaEfectivo = request.TotalEfectivoContado - saldoEsperado;

        ventanilla.Cerrada = true;
        ventanilla.Cuadrada = diferenciaEfectivo == 0;
        ventanilla.PuedeTransaccionar = false;

        if (itemCajaEfectivo is not null)
        {
            itemCajaEfectivo.SaldoCuadre = request.TotalEfectivoContado;
        }

        var cuadre = new VentanillaCuadre
        {
            Id = Guid.NewGuid(),
            IdVentanilla = ventanilla.Id,
            FechaProceso = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalEfectivo = request.TotalEfectivoContado,
            TotalCheque = request.TotalCheque,
            Total = request.TotalEfectivoContado + request.TotalCheque,
            DiferenciaEfectivo = diferenciaEfectivo,
            DiferenciaCheque = 0,
            EstaCuadrado = diferenciaEfectivo == 0,
            Aprobada = false,
            Activa = true,
        };
        db.VentanillasCuadre.Add(cuadre);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictoConcurrenciaException($"La ventanilla {ventanilla.Id}");
        }

        return new VentanillaCerradaResult(cuadre.Id, cuadre.EstaCuadrado, saldoEsperado, diferenciaEfectivo);
    }
}
