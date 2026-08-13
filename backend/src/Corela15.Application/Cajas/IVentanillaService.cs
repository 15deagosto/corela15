using Corela15.Application.Common;

namespace Corela15.Application.Cajas;

public interface IVentanillaService
{
    /// <summary>Abre la sesión de caja del día para un cajero (falla si ya tiene una abierta hoy).</summary>
    Task<VentanillaAbiertaResult> AbrirAsync(AbrirVentanillaRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cierra la ventanilla y registra el cuadre con el efectivo contado por
    /// el cajero. IMPORTANTE — limitación real, no oculta: todavía no existe
    /// el detalle de movimientos de efectivo por transacción vinculado a
    /// una ventanilla (ver Ventanilla.cs), así que esto NO compara contra un
    /// monto esperado calculado — registra el conteo declarado como
    /// "cuadrado" por definición. La reconciliación real automática requiere
    /// construir antes el vínculo transacción↔ventanilla, documentado como
    /// pendiente.
    /// </summary>
    Task<VentanillaCerradaResult> CerrarAsync(CerrarVentanillaRequest request, CancellationToken cancellationToken = default);
}

public class VentanillaYaAbiertaException(Guid idUsuario)
    : ReglaDeNegocioException($"El usuario {idUsuario} ya tiene una ventanilla abierta hoy");

public class VentanillaInvalidaException(Guid idVentanilla)
    : ReglaDeNegocioException($"La ventanilla {idVentanilla} no existe o ya está cerrada");
