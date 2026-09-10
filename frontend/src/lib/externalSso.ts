import { api } from './api'
import type { Modulo } from '../modules'

/**
 * Abre un módulo externo (CredVault, hoy el único con `ssoTicketEndpoint`)
 * ya autenticado con la sesión real de la persona, sin pedirle login de
 * nuevo -- pide un ticket corto de un solo uso a Corela15 y lo manda a
 * `${externalUrl}/sso?ticket=...`, donde la app destino lo valida y abre
 * SU PROPIA sesión (su propio usuario, sus propias credenciales visibles
 * según su rol ahí -- nunca una cuenta compartida entre personas).
 *
 * Si el módulo no tiene `ssoTicketEndpoint`, o el pedido de ticket falla
 * por cualquier motivo (sin permiso, sin cuenta del otro lado, red caída),
 * cae al enlace externo simple de siempre -- nunca deja el clic sin
 * efecto. Único punto real de esta lógica: usado por Sidebar/Home/
 * TabsContext en vez de repetirla en cada uno.
 */
export async function urlDestinoModuloExterno(modulo: Modulo): Promise<string | undefined> {
  if (!modulo.externalUrl) return undefined
  if (!modulo.ssoTicketEndpoint) return modulo.externalUrl

  try {
    const { data } = await api.post<{ ticket: string }>(modulo.ssoTicketEndpoint)
    return `${modulo.externalUrl}/sso?ticket=${encodeURIComponent(data.ticket)}`
  } catch {
    return modulo.externalUrl
  }
}

export async function abrirModuloExterno(modulo: Modulo): Promise<void> {
  if (!modulo.externalUrl) return

  // El navegador solo permite `window.open` sin bloqueo de popups si pasa
  // SINCRÓNICAMENTE dentro del gesto del usuario (el clic) -- pedir el
  // ticket primero (async) rompería ese gesto y la pestaña se abriría
  // bloqueada en silencio. Por eso se abre la pestaña en blanco ya mismo,
  // en el mismo clic, y recién después se le asigna la URL real (con o
  // sin ticket) cuando el pedido termina.
  const ventana = modulo.ssoTicketEndpoint ? window.open('', '_blank', 'noopener,noreferrer') : null
  const destino = await urlDestinoModuloExterno(modulo)
  if (!destino) return

  if (ventana) {
    ventana.location.href = destino
  } else {
    window.open(destino, '_blank', 'noopener,noreferrer')
  }
}
