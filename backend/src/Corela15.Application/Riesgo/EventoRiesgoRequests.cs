namespace Corela15.Application.Riesgo;

public record RegistrarEventoRiesgoRequest(
    int IdProceso, string Descripcion, int IdNivelImpacto, int IdNivelProbabilidad);

public record EventoRiesgoRegistradoResult(
    Guid IdEventoRiesgo, int Puntaje, string NivelRiesgo, string ColorNivelRiesgo);
