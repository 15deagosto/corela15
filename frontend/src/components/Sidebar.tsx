import { NavLink } from 'react-router-dom'
import { LayoutDashboard } from 'lucide-react'
import { modulos, type EstadoModulo } from '../modules'

const tagPorEstado: Record<Exclude<EstadoModulo, 'disponible'>, string> = {
  'en-construccion': 'EN DESARROLLO',
  proximamente: 'PRONTO',
}

function itemClase(activo: boolean, habilitado: boolean) {
  if (!habilitado) {
    return 'text-graphite-700 cursor-not-allowed'
  }
  return activo
    ? 'bg-gold-500/10 text-gold-300 border-l-2 border-gold-400'
    : 'text-graphite-300 border-l-2 border-transparent hover:bg-black/[0.03] hover:text-graphite-100'
}

export function Sidebar() {
  return (
    <aside className="flex h-full w-[280px] shrink-0 flex-col border-r border-black/[0.06] bg-white">
      <div className="flex items-center gap-3 px-5 py-5">
        <img
          src="/logo.png"
          alt="Cooperativa 15 de Agosto"
          className="h-9 w-9 rounded-full object-contain"
          onError={(e) => {
            e.currentTarget.style.display = 'none'
          }}
        />
        <div>
          <p className="text-base font-semibold leading-tight text-graphite-100">Corela15</p>
          <p className="text-[11px] font-medium uppercase tracking-wide text-graphite-600">
            Coop. 15 de Agosto
          </p>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto px-3 pb-4">
        <NavLink
          to="/"
          end
          className={({ isActive }) =>
            `mb-4 flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${itemClase(isActive, true)}`
          }
        >
          <LayoutDashboard size={17} strokeWidth={1.75} />
          Inicio
        </NavLink>

        <p className="mb-1 px-3 text-[11px] font-semibold uppercase tracking-wide text-graphite-600">
          Módulos
        </p>
        <ul className="flex flex-col gap-0.5">
          {modulos.map((m) => {
            const Icon = m.icon
            const habilitado = m.estado !== 'proximamente'
            const contenido = (
              <span className="flex w-full items-center gap-2.5">
                <Icon size={17} strokeWidth={1.75} />
                <span className="flex-1 text-left">{m.nombre}</span>
                {m.estado !== 'disponible' && (
                  <span className="rounded-full bg-graphite-950 px-1.5 py-0.5 text-[10px] font-semibold tracking-wide text-graphite-700">
                    {tagPorEstado[m.estado]}
                  </span>
                )}
              </span>
            )

            return (
              <li key={m.slug}>
                {habilitado ? (
                  <NavLink
                    to={m.path}
                    className={({ isActive }) =>
                      `flex items-center rounded-lg px-3 py-2 text-sm font-medium transition-colors ${itemClase(isActive, true)}`
                    }
                  >
                    {contenido}
                  </NavLink>
                ) : (
                  <span className={`flex items-center rounded-lg px-3 py-2 text-sm font-medium ${itemClase(false, false)}`}>
                    {contenido}
                  </span>
                )}
              </li>
            )
          })}
        </ul>
      </nav>
    </aside>
  )
}
