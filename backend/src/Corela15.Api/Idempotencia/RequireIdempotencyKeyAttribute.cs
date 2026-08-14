namespace Corela15.Api.Idempotencia;

/// <summary>
/// Marca un endpoint que mueve dinero real (depósito, pago de cuota,
/// apertura/cancelación de DPF, abono...) como exigiendo el header
/// `Idempotency-Key` — ver IdempotenciaFilter. Un reintento de red o un
/// doble-clic con la misma clave devuelve la respuesta original guardada,
/// no repite el movimiento.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RequireIdempotencyKeyAttribute : Attribute;
