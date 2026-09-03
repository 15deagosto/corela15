using Corela15.Application.Common;

namespace Corela15.Application.LavadoActivos;

public record RegistrarAlertaListaControlRequest(
    Guid IdPersona, string CodigoTipoListaControl, string Detalle, string RegistradoPor);

public record AlertaListaControlResult(Guid Id);

public record ResolverAlertaListaControlRequest(Guid IdAlerta, string Comentario, string ResueltoPor);

public record AlertaListaControlDetalle(
    Guid Id, string Persona, string Identificacion, string CodigoTipoListaControl, string TipoListaControl,
    string Detalle, DateOnly FechaDeteccion, bool Resuelta, string? ComentarioResolucion, string? ResueltoPor,
    string RegistradoPor);

public interface IListaControlService
{
    /// <summary>
    /// Registra que Cumplimiento encontró una coincidencia real de una
    /// persona contra una lista de control (sentenciados, PEP, ONU, OFAC,
    /// etc.) — ver AlertaListaControl.cs para por qué el cruce no es
    /// automático en este core. Mientras la alerta esté sin resolver,
    /// cualquier movimiento sobre una cuenta de esa persona queda en
    /// espera de autorización (ver IAutorizacionTransaccionService).
    /// </summary>
    Task<AlertaListaControlResult> RegistrarAlertaAsync(
        RegistrarAlertaListaControlRequest request, CancellationToken cancellationToken = default);

    /// <summary>Resuelve la alerta (falso positivo confirmado, o el caso ya fue atendido) — nunca se borra, queda como auditoría.</summary>
    Task ResolverAlertaAsync(
        ResolverAlertaListaControlRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlertaListaControlDetalle>> ListarAsync(
        bool soloNoResueltas, CancellationToken cancellationToken = default);
}

public class PersonaInvalidaException(Guid idPersona)
    : ReglaDeNegocioException($"La persona {idPersona} no existe");

public class TipoListaControlInvalidoException(string codigo)
    : ReglaDeNegocioException($"El tipo de lista de control '{codigo}' no existe o está inactivo");

public class AlertaListaControlInvalidaException(Guid idAlerta)
    : ReglaDeNegocioException($"La alerta {idAlerta} no existe o ya fue resuelta");
