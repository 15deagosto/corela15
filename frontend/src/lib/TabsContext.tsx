import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { modulos } from '../modules'

export interface TabInfo {
  slug: string
  path: string
  nombre: string
}

export const TAB_INICIO: TabInfo = { slug: 'inicio', path: '/', nombre: 'Inicio' }

const STORAGE_KEY = 'corela15-pestanas-abiertas'

interface TabsContextValue {
  tabs: TabInfo[]
  activeSlug: string
  activate: (tab: TabInfo) => void
  close: (slug: string) => void
}

const TabsContext = createContext<TabsContextValue | null>(null)

function cargarPestanasGuardadas(): TabInfo[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return [TAB_INICIO]
    const guardadas = JSON.parse(raw) as TabInfo[]
    // Revalida contra el catálogo real de módulos — si un slug guardado ya
    // no existe (módulo renombrado/quitado), se descarta en vez de dejar
    // una pestaña rota.
    const validas = guardadas.filter(
      (t) => t.slug === 'inicio' || modulos.some((m) => m.slug === t.slug),
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
 */
export function TabsProvider({ children }: { children: ReactNode }) {
  const location = useLocation()
  const navigate = useNavigate()

  const [tabs, setTabs] = useState<TabInfo[]>(cargarPestanasGuardadas)
  const [activeSlug, setActiveSlug] = useState<string>('inicio')

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(tabs))
  }, [tabs])

  // Mantiene las pestañas sincronizadas con la URL real — cubre navegación
  // directa (escribir la URL), recarga de página, y atrás/adelante.
  useEffect(() => {
    const path = location.pathname
    const modulo = path === '/' ? TAB_INICIO : modulos.find((m) => m.path === path)
    if (!modulo) {
      // Ruta desconocida (slug inválido) — no se abre una pestaña fantasma.
      return
    }
    const tab: TabInfo = modulo === TAB_INICIO ? TAB_INICIO : { slug: modulo.slug, path: modulo.path, nombre: modulo.nombre }
    setTabs((prev) => (prev.some((t) => t.slug === tab.slug) ? prev : [...prev, tab]))
    setActiveSlug(tab.slug)
  }, [location.pathname])

  const activate = (tab: TabInfo) => {
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
