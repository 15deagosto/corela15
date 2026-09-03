using Corela15.Application.Common;

namespace Corela15.Application.Portafolio;

public interface IInversionPortafolioService
{
    /// <summary>
    /// Abre una inversión propia de la cooperativa en otra institución
    /// financiera (débito grupo 13 Inversiones, subcuenta real según el
    /// plazo y el sector de la institución contraparte / crédito Caja).
    /// </summary>
    Task<InversionAbiertaResult> AbrirAsync(AbrirInversionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela una inversión vigente, devolviendo el capital más el
    /// interés devengado proporcional al tiempo transcurrido (misma
    /// fórmula que Deposito.CancelarAsync — interés = ValorNominal × Tasa
    /// × díasTranscurridos / 365, acotado al plazo contratado).
    /// </summary>
    Task<InversionCanceladaResult> CancelarAsync(CancelarInversionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renueva una inversión vigente bajo un documento nuevo, liquidando
    /// el interés devengado del origen antes de continuar — mismo
    /// tratamiento que DepositoService.RenovarAsync.
    /// </summary>
    Task<InversionRenovadaResult> RenovarAsync(RenovarInversionRequest request, CancellationToken cancellationToken = default);
}

public class MontoInversionInvalidoException(decimal valor)
    : SolicitudInvalidaException($"El valor nominal {valor:0.00} debe ser mayor a cero");

public class InstitucionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La institución '{codigo}' no existe o está inactiva");

public class InversionInvalidaException(Guid idInversion)
    : ReglaDeNegocioException($"La inversión {idInversion} no existe o ya no está activa");
