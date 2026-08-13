using Corela15.Application.Common;

namespace Corela15.Application.Inversion;

public interface IDepositoService
{
    /// <summary>
    /// Abre un DPF: busca la tasa vigente en el tablero (ItemPlazoTasa) para
    /// el monto/plazo/tipo de persona — nunca hardcodeada, siempre leída del
    /// tablero vigente al momento de abrir (mismo aprendizaje documentado en
    /// Deposito.cs sobre la renovación) — y registra el asiento (débito Caja
    /// / crédito Depósitos a plazo fijo) vía TipoTransaccion. Atómico.
    /// </summary>
    Task<DepositoAbiertoResult> AbrirAsync(AbrirDepositoRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela/redime un DPF: revierte el asiento (débito Depósitos a plazo
    /// fijo / crédito Caja) y marca el depósito como Cancelado. No calcula
    /// interés devengado todavía — devuelve el capital nominal; el cálculo
    /// de interés acumulado a la fecha de corte queda pendiente (ver
    /// CLAUDE.md).
    /// </summary>
    Task<DepositoCanceladoResult> CancelarAsync(CancelarDepositoRequest request, CancellationToken cancellationToken = default);
}

public class ClienteInvalidoParaDepositoException(Guid idCliente)
    : ReglaDeNegocioException($"El cliente {idCliente} no existe o no está activo");

public class SinTasaVigenteException(int plazoDias, decimal monto)
    : ReglaDeNegocioException(
        $"No hay una tasa vigente en el tablero para {plazoDias} días y monto {monto:0.00} — revisar inversion.item_plazo_tasa");

public class DepositoInvalidoException(Guid idDeposito)
    : ReglaDeNegocioException($"El depósito {idDeposito} no existe o no está vigente");
