namespace Corela15.Application.Cobranza;

public record RegistrarGestionCobranzaRequest(
    Guid IdPrestamo,
    Guid IdCliente,
    bool EsDeudor,
    string CodigoAccionGestion,
    bool TieneCompromisoPago,
    string? Observacion,
    string RegistradoPor);

public record GestionCobranzaRegistradaResult(Guid IdGestion, DateOnly Fecha);
