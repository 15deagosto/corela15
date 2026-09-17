/**
 * `crypto.randomUUID()` real -- pero esa función solo existe en contexto
 * seguro (HTTPS/localhost). Bug real encontrado: un usuario entrando por
 * `http://<IP>:8080` (no HTTPS) tiraba un TypeError síncrono al armar el
 * header `Idempotency-Key`, y como el error pasaba antes de cualquier
 * llamada de red, el mensaje/operación fallaba en silencio -- no quedaba
 * ni un solo intento en los logs del backend, parecía "no hace nada".
 * Mismo problema real ya documentado para `navigator.clipboard.write`.
 *
 * Esta función nunca falla: usa el `crypto.randomUUID` real cuando existe
 * (contexto seguro), y si no, arma un UUID v4 real con `Math.random` --
 * suficiente para una clave de idempotencia (no necesita ser
 * criptográficamente robusta, solo única por request real del usuario).
 */
export function idempotencyKey(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    try {
      return crypto.randomUUID()
    } catch {
      // sigue al fallback
    }
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
    const r = (Math.random() * 16) | 0
    const v = c === 'x' ? r : (r & 0x3) | 0x8
    return v.toString(16)
  })
}
