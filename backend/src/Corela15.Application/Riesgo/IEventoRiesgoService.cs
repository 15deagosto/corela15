using Corela15.Application.Common;

namespace Corela15.Application.Riesgo;

public interface IEventoRiesgoService
{
    /// <summary>
    /// Registra un evento de riesgo calculando el nivel automáticamente:
    /// puntaje = NivelImpacto.Nivel × NivelProbabilidad.Nivel (1-25), que se
    /// ubica en el rango de `riesgo.nivel_riesgo` correspondiente (Bajo
    /// 1-6, Moderado 7-12, Alto 13-19, Extremo 20-25) — el operador nunca
    /// elige el nivel a mano, se deriva de la matriz, igual que
    /// RIESGOOPERATIVO verificado contra Softbank.
    /// </summary>
    Task<EventoRiesgoRegistradoResult> RegistrarAsync(
        RegistrarEventoRiesgoRequest request, CancellationToken cancellationToken = default);
}

public class ProcesoInvalidoException(int idProceso)
    : ReglaDeNegocioException($"El proceso {idProceso} no existe o no está activo");

public class NivelImpactoInvalidoException(int idNivelImpacto)
    : ReglaDeNegocioException($"El nivel de impacto {idNivelImpacto} no existe o no está activo");

public class NivelProbabilidadInvalidoException(int idNivelProbabilidad)
    : ReglaDeNegocioException($"El nivel de probabilidad {idNivelProbabilidad} no existe o no está activo");

public class SinNivelRiesgoParaPuntajeException(int puntaje)
    : ReglaDeNegocioException($"No hay un nivel de riesgo configurado que cubra el puntaje {puntaje} — revisar riesgo.nivel_riesgo");
