using Corela15.Application.Common;

namespace Corela15.Application.FlujoTrabajo;

public record EtapaRegistrada(int IdEtapa, string CodigoGrupoContable, string NombreGrupoContable);

public interface IFlujoTrabajoService
{
    /// <summary>
    /// Valida que el usuario que intenta decidir una etapa (ej. aprobar el
    /// Comité de Crédito) pertenezca al grupo de aprobadores real que
    /// Softbank define para esa etapa+agencia (`flujotrabajo.
    /// etapa_grupo_contable` → `grupo_contable_usuario`), y que el monto de
    /// la operación caiga dentro del rango que ese grupo puede autorizar
    /// (`grupo_contable.monto_minimo/monto_maximo`). Antes de este motor,
    /// CUALQUIER usuario con acceso al menú de Créditos podía aprobar
    /// cualquier monto — un gap real de segregación de funciones, ahora
    /// cerrado. Lanza UsuarioNoAutorizadoParaEtapaException (422) si no
    /// pertenece a ningún grupo válido.
    /// </summary>
    Task<EtapaRegistrada> ValidarYRegistrarAsync(
        int idEtapa, int idAgencia, decimal monto, string registradoPor, CancellationToken cancellationToken = default);
}

public class EtapaNoConfiguradaException(string etapa, int idAgencia)
    : ReglaDeNegocioException($"La etapa '{etapa}' no tiene un grupo de aprobadores configurado para la agencia {idAgencia} — configurarlo en Configuración antes de poder decidir esta solicitud");

public class UsuarioNoAutorizadoParaEtapaException(string registradoPor, string etapa)
    : ReglaDeNegocioException($"El usuario '{registradoPor}' no pertenece al grupo de aprobadores de la etapa '{etapa}' para este monto — no está autorizado a decidir esta solicitud");
