namespace Corela15.Application.Common;

/// <summary>
/// Violación de unicidad de código/nombre en un catálogo de configuración
/// (país, moneda, rol, etc.) — HTTP 422 vía el mismo
/// DomainExceptionHandler que el resto de reglas de negocio, sin necesitar
/// una excepción nueva por catálogo.
/// </summary>
public class CodigoDuplicadoException(string entidad, string codigo)
    : ReglaDeNegocioException($"Ya existe {entidad} con el código '{codigo}'");
