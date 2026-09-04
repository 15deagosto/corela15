import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { modulos } from '../modules'
import { useAuth } from './AuthContext'

export interface TabInfo {
  slug: string
  path: string
  nombre: string
}

export const TAB_INICIO: TabInfo = { slug: 'inicio', path: '/', nombre: 'Inicio' }

const STORAGE_PREFIX = 'corela15-pestanas-abiertas'

interface TabsContextValue {
  tabs: TabInfo[]
  activeSlug: string
  activate: (tab: TabInfo) => void
  close: (slug: string) => void
}

const TabsContext = createContext<TabsContextValue | null>(null)

function cargarPestanasGuardadas(storageKey: string, tieneMenu: (codigo: string) => boolean): TabInfo[] {
  try {
    const raw = localStorage.getItem(storageKey)
    if (!raw) return [TAB_INICIO]
    const guardadas = JSON.parse(raw) as TabInfo[]
    // Revalida contra el catálogo real de módulos Y contra el permiso real
    // del usuario actual — sin el segundo chequeo, las pestañas de una
    // sesión anterior (ej. un admin) quedaban guardadas y se restauraban
    // igual para el siguiente usuario que inicia sesión en el mismo
    // navegador, mostrándole módulos que no debería poder ver. Ahora la
    // clave de localStorage además está separada por usuario (ver abajo),
    // esto es una segunda capa de defensa, no la única.
    const validas = guardadas.filter(
      (t) => t.slug === 'inicio' || (modulos.some((m) => m.slug === t.slug) && tieneMenu(t.slug)),
    )
    return validas.some((t) => t.slug === 'inicio') ? validas : [TAB_INICIO, ...validas]
  } catch {
    return [TAB_INICIO]
  }
}

/**
 * Pestañas internas — permite tener varios módulos abiertos a la vez sin
 * perder su estado (formularios a medias, paneles expandidos, scroll) al
 * cambiar entre ellos, y sin depender de pestañas del navegador. La URL
 * sigue reflejando el módulo activo (deep-link, recargar la página,
 * atrás/adelante del navegador funcionan normal); lo que cambia es que
 * `Layout` ya no desmonta el módulo anterior al navegar — ver
 * `TabsWorkspace`.
 *
 * Todo lo que intenta abrir una pestaña pasa por este componente (clic en
 * el Sidebar, clic en una tarjeta de Inicio, URL escrita a mano) — por
 * eso el chequeo de permiso real (`tieneMenu`) vive acá, en un solo
 * lugar, en vez de repetirlo en cada pantalla que podría abrir un tab.
 */
export function TabsProvider({ children }: { children: ReactNode }) {
  const location = useLocation()
  const navigate = useNavigate()
  const { sesion, tieneMenu } = useAuth()
  // Clave por usuario — un admin y un usuario limitado que comparten PC/
  // navegador nunca se pisan las pestañas guardadas del otro.
  const storageKey = `${STORAGE_PREFIX}:${sesion?.idUsuario ?? 'anon'}`

  const [tabs, setTabs] = useState<TabInfo[]>(() => cargarPestanasGuardadas(storageKey, tieneMenu))
  const [activeSlug, setActiveSlug] = useState<string>('inicio')

  useEffect(() => {
    localStorage.setItem(storageKey, JSON.stringify(tabs))
  }, [tabs, storageKey])

  // Mantiene las pestañas sincronizadas con la URL real — cubre navegación
  // directa (escribir la URL), recarga de página, y atrás/adelante. Si el
  // usuario no tiene permiso real para ese módulo (menú no otorgado), se
  // lo manda a Inicio en vez de abrir la pestaña — cubre tanto una URL
  // escrita a mano como el clic en una tarjeta de Inicio (que usa <Link>
  // directo, sin pasar por `activate`).
  useEffect(() => {
    const path = location.pathname
    const modulo = path === '/' ? TAB_INICIO : modulos.find((m) => m.path === path)
    if (!modulo) {
      // Ruta desconocida (slug inválido) — no se abre una pestaña fantasma.
      return
    }
    if (modulo !== TAB_INICIO && !tieneMenu(modulo.slug)) {
      navigate('/', { replace: true })
      return
    }
    const tab: TabInfo = modulo === TAB_INICIO ? TAB_INICIO : { slug: modulo.slug, path: modulo.path, nombre: modulo.nombre }
    setTabs((prev) => (prev.some((t) => t.slug === tab.slug) ? prev : [...prev, tab]))
    setActiveSlug(tab.slug)
  }, [location.pathname, tieneMenu, navigate])

  const activate = (tab: TabInfo) => {
    if (tab.slug !== 'inicio' && !tieneMenu(tab.slug)) return
    setTabs((prev) => (prev.some((t) => t.slug === tab.slug) ? prev : [...prev, tab]))
    setActiveSlug(tab.slug)
    navigate(tab.path)
  }

  const close = (slug: string) => {
    if (slug === 'inicio') return
    setTabs((prev) => {
      const next = prev.filter((t) => t.slug !== slug)
      if (activeSlug === slug) {
        const siguiente = next[next.length - 1] ?? TAB_INICIO
        setActiveSlug(siguiente.slug)
        navigate(siguiente.path)
      }
      return next
    })
  }

  const value = useMemo(() => ({ tabs, activeSlug, activate, close }), [tabs, activeSlug])

  return <TabsContext.Provider value={value}>{children}</TabsContext.Provider>
}

export function useTabs() {
  const ctx = useContext(TabsContext)
  if (!ctx) throw new Error('useTabs debe usarse dentro de <TabsProvider>')
  return ctx
}
