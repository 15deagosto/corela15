namespace Corela15.Application.Common;

/// <summary>
/// Base para excepciones de reglas de negocio que la capa Api debe traducir
/// a una respuesta HTTP específica — ver
/// Corela15.Api/ExceptionHandling/DomainExceptionHandler.cs. Cada caso de
/// uso nuevo que valida algo (saldo, estado, existencia) hereda de acá en
/// vez de que cada controller tenga que enumerar try/catch por excepción:
/// un módulo nuevo (ej. desembolso de préstamo) queda con manejo de errores
/// consistente sin escribir nada de más.
/// </summary>
public abstract class DomainException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

/// <summary>Violación de una regla de negocio (ej. saldo insuficiente, comprobante desbalanceado) — HTTP 422.</summary>
public abstract class ReglaDeNegocioException(string message) : DomainException(message, 422);

/// <summary>Datos de entrada inválidos por forma, no por regla de negocio — HTTP 400.</summary>
public abstract class SolicitudInvalidaException(string message) : DomainException(message, 400);
