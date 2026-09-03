using Corela15.Application.Common;

namespace Corela15.Application.Sujeto;

/// <summary>
/// Canal formal de reclamos — requisito legal real de la Ley Orgánica
/// de Defensa del Consumidor en Ecuador (toda entidad financiera debe
/// tenerlo). Verificado contra SUJETO.PERSONA_RECLAMO y sus catálogos
/// reales (canal, concepto/detalle SEPS de cobros indebidos, estado,
/// tipo de resolución).
/// </summary>
public interface IReclamoService
{
    Task<ReclamoRegistradoResult> RegistrarAsync(RegistrarReclamoRequest request, CancellationToken cancellationToken = default);

    Task ResponderAsync(Guid idReclamo, ResponderReclamoRequest request, CancellationToken cancellationToken = default);
}

public record RegistrarReclamoRequest(
    Guid IdPersona, string CodigoCanalRecepcion, int IdTipoProducto, string CodigoConceptoDetalle,
    string Descripcion, string RegistradoPor);

public record ResponderReclamoRequest(
    string CodigoTipoResolucion, decimal? MontoRestituido, decimal? InteresSobreMonto, string Descripcion, string RegistradoPor);

public record ReclamoRegistradoResult(Guid Id);

public class PersonaInvalidaParaReclamoException(Guid idPersona)
    : ReglaDeNegocioException($"La persona {idPersona} no existe");

public class CanalReclamoInvalidoException(string codigo)
    : ReglaDeNegocioException($"El canal de recepción '{codigo}' no existe o no está activo");

public class TipoProductoReclamoInvalidoException(int id)
    : ReglaDeNegocioException($"El tipo de producto {id} no existe o no está activo");

public class ConceptoReclamoInvalidoException(string codigo)
    : ReglaDeNegocioException($"El concepto de reclamo '{codigo}' no existe o no está activo");

public class ReclamoInexistenteException(Guid id)
    : ReglaDeNegocioException($"El reclamo {id} no existe");

public class ReclamoYaResueltoException(Guid id)
    : ReglaDeNegocioException($"El reclamo {id} ya fue resuelto");

public class TipoResolucionInvalidoException(string codigo)
    : ReglaDeNegocioException($"El tipo de resolución '{codigo}' no existe o no está activo");
