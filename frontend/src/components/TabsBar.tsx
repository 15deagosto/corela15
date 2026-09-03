import { X, LayoutDashboard } from 'lucide-react'
import { modulos } from '../modules'
import { useTabs } from '../lib/TabsContext'

export function TabsBar() {
  const { tabs, activeSlug, activate, close } = useTabs()

  if (tabs.length <= 1) return null

  return (
    <div className="flex h-10 shrink-0 items-center gap-0.5 overflow-x-auto border-b border-black/[0.06] bg-white px-2">
      {tabs.map((tab) => {
        const modulo = modulos.find((m) => m.slug === tab.slug)
        const Icon = modulo?.icon ?? LayoutDashboard
        const activa = tab.slug === activeSlug

        return (
          <div
            key={tab.slug}
            role="button"
            tabIndex={0}
            onClick={() => activate(tab)}
            onKeyDown={(e) => e.key === 'Enter' && activate(tab)}
            className={`group flex shrink-0 cursor-pointer items-center gap-1.5 rounded-t-lg border-x border-t px-3 py-1.5 text-xs font-medium transition-colors ${
              activa
                ? 'border-black/[0.06] bg-gold-500/10 text-gold-300'
                : 'border-transparent text-graphite-600 hover:bg-black/[0.02] hover:text-graphite-100'
            }`}
          >
            <Icon size={13} strokeWidth={1.75} />
            <span className="max-w-[140px] truncate">{tab.nombre}</span>
            {tab.slug !== 'inicio' && (
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation()
                  close(tab.slug)
                }}
                className="rounded p-0.5 text-graphite-600 opacity-0 hover:bg-black/[0.06] hover:text-red-700 group-hover:opacity-100"
                title="Cerrar pestaña"
              >
                <X size={12} />
              </button>
            )}
          </div>
        )
      })}
    </div>
  )
}
