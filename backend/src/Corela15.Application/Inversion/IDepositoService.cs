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

    /// <summary>
    /// Renueva un DPF vigente: cierra el origen (Estado=Renovado, nunca
    /// Cancelado — sigue siendo capital del socio, no una redención) y abre
    /// un depósito nuevo leyendo la tasa vigente en ItemPlazoTasa EN ESE
    /// MOMENTO, nunca copiando la tasa del depósito origen — es exactamente
    /// el aprendizaje del incidente real documentado en
    /// 01-contexto-origen.md (#2: "la tasa aplicada al renovar un
    /// certificado a menudo no coincidía con el tablero de tasas vigente").
    /// Si hay incremento de capital, registra el asiento de la diferencia
    /// (débito Caja / crédito Depósitos a plazo fijo); si no hay
    /// incremento, no genera comprobante porque el saldo de la subcuenta
    /// contable no cambia, solo se re-papela bajo un nuevo código de DPF.
    /// Queda registrado en DepositoRenovacion (origen→destino, valor,
    /// incremento, fecha) para trazabilidad completa de la cadena de
    /// renovaciones.
    /// </summary>
    Task<DepositoRenovadoResult> RenovarAsync(RenovarDepositoRequest request, CancellationToken cancellationToken = default);
}

public class ClienteInvalidoParaDepositoException(Guid idCliente)
    : ReglaDeNegocioException($"El cliente {idCliente} no existe o no está activo");

public class SinTasaVigenteException(int plazoDias, decimal monto)
    : ReglaDeNegocioException(
        $"No hay una tasa vigente en el tablero para {plazoDias} días y monto {monto:0.00} — revisar inversion.item_plazo_tasa");

public class DepositoInvalidoException(Guid idDeposito)
    : ReglaDeNegocioException($"El depósito {idDeposito} no existe o no está vigente");

public class IncrementoCapitalInvalidoException(decimal incremento)
    : ReglaDeNegocioException($"El incremento de capital ({incremento:0.00}) no puede ser negativo");
