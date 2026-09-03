using Corela15.Application.Common;

namespace Corela15.Application.Financiero;

public interface IChequeService
{
    /// <summary>Registra el ingreso de un cheque de terceros — solo captura el documento, sin efecto contable todavía.</summary>
    Task<ChequeRegistradoResult> RegistrarAsync(RegistrarChequeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía el cheque a canje: débito 110401 Efectos de cobro inmediato /
    /// crédito 2101 Depósitos de ahorro a la vista — el socio ya queda
    /// registrado como acreedor por el valor, pero el balde real que se
    /// acredita es "Bloqueado" (no "Disponible"): no puede retirarlo hasta
    /// que el cheque se efectivice.
    /// </summary>
    Task<ChequeDepositadoResult> DepositarAsync(DepositarChequeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// El cheque se cobró: débito Caja / crédito 110401 (reclasificación
    /// pura, no toca el pasivo con el socio) — y el valor pasa del balde
    /// Bloqueado al Disponible en la cuenta del socio.
    /// </summary>
    Task<ChequeEfectivizadoResult> EfectivizarAsync(EfectivizarChequeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// El cheque rebotó: reversa exacta del depósito — débito 2101 / crédito
    /// 110401, y se retira el valor del balde Bloqueado del socio (nunca
    /// llegó a estar disponible, así que no hay saldo que recuperar de él).
    /// </summary>
    Task<ChequeProtestadoResult> ProtestarAsync(ProtestarChequeRequest request, CancellationToken cancellationToken = default);
}

public class BancoInvalidoException(int idBanco)
    : ReglaDeNegocioException($"El banco {idBanco} no existe o está inactivo");

public class MontoChequeInvalidoException(decimal valor)
    : SolicitudInvalidaException($"El valor {valor:0.00} debe ser mayor a cero");

public class ChequeInvalidoException(Guid idCheque, string estadoEsperado)
    : ReglaDeNegocioException($"El cheque {idCheque} no existe o no está en estado '{estadoEsperado}'");
