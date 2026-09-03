import { useTabs } from '../lib/TabsContext'
import { componentesPorSlug } from '../lib/moduleComponents'

/**
 * Renderiza TODOS los módulos abiertos a la vez, ocultando con CSS los que
 * no están activos (nunca los desmonta) — así un formulario a medias, un
 * panel expandido, o el scroll de una tabla larga siguen ahí exactamente
 * igual al volver a esa pestaña. El costo es memoria/DOM extra por cada
 * pestaña abierta, aceptable para el número de módulos reales de este
 * core (11 más Inicio) — nunca decenas.
 */
export function TabsWorkspace() {
  const { tabs, activeSlug } = useTabs()

  return (
    <>
      {tabs.map((tab) => {
        const Componente = componentesPorSlug[tab.slug]
        if (!Componente) return null
        return (
          <div key={tab.slug} className={tab.slug === activeSlug ? 'block' : 'hidden'}>
            <Componente />
          </div>
        )
      })}
    </>
  )
}
