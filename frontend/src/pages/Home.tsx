import { LayoutDashboard } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { modulos } from '../modules'
import { Link } from 'react-router-dom'
import { useAuth } from '../lib/AuthContext'

const estadoTexto = {
  disponible: 'Disponible',
  'en-construccion': 'En desarrollo',
  proximamente: 'Próximamente',
} as const

const estadoClase = {
  disponible: 'bg-petrol-800/10 text-petrol-700',
  'en-construccion': 'bg-gold-500/15 text-gold-300',
  proximamente: 'bg-graphite-950 text-graphite-600',
} as const

export function Home() {
  const { tieneMenu } = useAuth()
  // Antes mostraba tarjetas de TODOS los módulos sin filtrar — un usuario
  // con un solo permiso real veía (y podía abrir, vía el <Link> directo)
  // cualquier módulo del sistema desde acá, sin pasar por el filtro que sí
  // aplica el Sidebar. Mismo criterio que Sidebar.tsx: solo lo que el
  // usuario realmente tiene otorgado.
  const modulosVisibles = modulos.filter((m) => tieneMenu(m.slug))

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={LayoutDashboard}
        title="Inicio"
        subtitle="Core financiero propio de la Cooperativa 15 de Agosto"
      />

      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {modulosVisibles.map((m) => {
          const Icon = m.icon
          const habilitado = m.estado !== 'proximamente'
          const contenido = (
            <div className="flex w-full flex-col items-start gap-3">
              <div className="flex w-full items-start justify-between">
                <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-gold-500/15 text-gold-400">
                  <Icon size={20} strokeWidth={1.75} />
                </span>
                <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${estadoClase[m.estado]}`}>
                  {estadoTexto[m.estado]}
                </span>
              </div>
              <div>
                <h3 className="font-medium text-graphite-100">{m.nombre}</h3>
                <p className="text-sm text-graphite-600">{m.descripcion}</p>
              </div>
            </div>
          )

          return (
            <li key={m.slug}>
              {habilitado ? (
                <Link
                  to={m.path}
                  className="btn-hover flex w-full rounded-xl border border-black/[0.06] bg-white p-4 shadow-sm hover:border-gold-500/40"
                >
                  {contenido}
                </Link>
              ) : (
                <div className="flex w-full cursor-not-allowed rounded-xl border border-black/[0.06] bg-white/60 p-4 opacity-70">
                  {contenido}
                </div>
              )}
            </li>
          )
        })}
      </ul>
    </div>
  )
}
