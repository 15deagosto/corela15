using Corela15.Application.Common;

namespace Corela15.Application.Cobranza;

public interface IGestionCobranzaService
{
    /// <summary>Registra un contacto de cobranza (llamada, visita, acuerdo de pago...) sobre un préstamo.</summary>
    Task<GestionCobranzaRegistradaResult> RegistrarAsync(
        RegistrarGestionCobranzaRequest request, CancellationToken cancellationToken = default);
}

public class AccionGestionInvalidaException(string codigo)
    : ReglaDeNegocioException($"La acción de gestión '{codigo}' no existe o está inactiva");
