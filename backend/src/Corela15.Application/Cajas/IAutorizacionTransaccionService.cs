using Corela15.Application.Ahorros;
using Corela15.Application.Common;

namespace Corela15.Application.Cajas;

public record AutorizacionPendienteDetalle(
    Guid Id, string NumeroCuenta, string Socio, string CodigoTipoTransaccion, decimal Monto,
    string Detalle, DateTimeOffset CreadoEn, string RegistradoPor);

public interface IAutorizacionTransaccionService
{
    Task<IReadOnlyList<AutorizacionPendienteDetalle>> ListarPendientesAsync(CancellationToken cancellationToken = default);

    /// <summary>Aprueba la transacción en espera y recién ahí la ejecuta (saldo + asiento contable), nunca antes.</summary>
    Task<MovimientoCuentaRegistradoResult> AprobarAsync(
        Guid idAutorizacion, string autorizadoPor, CancellationToken cancellationToken = default);

    /// <summary>Rechaza la transacción — nunca se ejecuta, el saldo de la cuenta no se toca.</summary>
    Task RechazarAsync(
        Guid idAutorizacion, string comentario, string autorizadoPor, CancellationToken cancellationToken = default);
}

public class AutorizacionInvalidaException(Guid idAutorizacion)
    : ReglaDeNegocioException($"La autorización {idAutorizacion} no existe o ya fue procesada");
