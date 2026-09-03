import type { ReactNode } from 'react'
import { createPortal } from 'react-dom'

/**
 * Monta su contenido directo en `document.body` en vez de en el punto del
 * árbol donde se declara. Necesario para todo modal `fixed inset-0`: casi
 * toda página del proyecto envuelve su contenido en un `<div
 * className="animate-fade-in">` — esa animación usa `transform` (incluso
 * en su estado final, `translateY(0)` sigue siendo un `transform`), y
 * cualquier `transform` en un ancestro crea un nuevo containing block para
 * los descendientes `position: fixed`. Sin este portal, el modal deja de
 * ser relativo al viewport y queda confinado/cortado al cuadro del
 * elemento animado — el bug real que motivó este componente (encontrado
 * primero en Mesa de Servicio, después confirmado en Usuarios y roles).
 */
export function ModalPortal({ children }: { children: ReactNode }) {
  return createPortal(children, document.body)
}
