import { useQuery } from '@tanstack/react-query'
import { api } from './lib/api'
import { modulos, type EstadoModulo } from './modules'

type Health = { status: string; timestampUtc: string }

const estadoTexto: Record<EstadoModulo, string> = {
  disponible: 'Disponible',
  'en-construccion': 'En construcción',
  proximamente: 'Próximamente',
}

const estadoClase: Record<EstadoModulo, string> = {
  disponible: 'bg-petrol-800/10 text-petrol-700',
  'en-construccion': 'bg-gold-500/15 text-gold-300',
  proximamente: 'bg-graphite-950 text-graphite-600',
}

function App() {
  const { isLoading, isError } = useQuery<Health>({
    queryKey: ['health'],
    queryFn: async () => (await api.get('/health')).data,
    retry: 1,
  })

  return (
    <div className="min-h-full font-sans">
      <header className="glass sticky top-0 z-10">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-4">
          <div className="flex items-center gap-3">
            <img
              src="/logo.png"
              alt="Cooperativa 15 de Agosto"
              className="h-9 w-9 rounded-full object-contain animate-fade-in"
              onError={(e) => {
                e.currentTarget.style.display = 'none'
              }}
            />
            <div>
              <h1 className="text-lg font-semibold text-graphite-100">
                Corela15 <span className="text-gold-400">·</span> Core financiero
              </h1>
              <p className="text-sm text-graphite-500">Cooperativa 15 de Agosto</p>
            </div>
          </div>
          <div className="flex items-center gap-2 text-sm text-graphite-500">
            <span
              className={`h-2 w-2 rounded-full ${
                isLoading ? 'bg-gold-500' : isError ? 'bg-red-600' : 'bg-petrol-700 glow-petrol'
              }`}
            />
            {isLoading ? 'Conectando…' : isError ? 'Sin conexión' : 'Conectado'}
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-6 py-10">
        <h2 className="mb-1 text-xl font-semibold text-graphite-100">Módulos</h2>
        <p className="mb-6 text-sm text-graphite-500">Elegí un módulo para empezar</p>

        <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {modulos.map((m) => {
            const Icon = m.icon
            const habilitado = m.estado === 'disponible'
            return (
              <li key={m.slug}>
                <button
                  type="button"
                  disabled={!habilitado}
                  className={`glass-card hover-zoom btn-hover flex w-full flex-col items-start gap-3 rounded-2xl p-5 text-left ${
                    habilitado ? 'cursor-pointer' : 'cursor-not-allowed opacity-70'
                  }`}
                >
                  <div className="flex w-full items-start justify-between">
                    <span className="glow-gold flex h-11 w-11 items-center justify-center rounded-xl bg-gold-500/15 text-gold-400">
                      <Icon size={22} strokeWidth={1.75} />
                    </span>
                    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${estadoClase[m.estado]}`}>
                      {estadoTexto[m.estado]}
                    </span>
                  </div>
                  <div>
                    <h3 className="font-medium text-graphite-100">{m.nombre}</h3>
                    <p className="text-sm text-graphite-500">{m.descripcion}</p>
                  </div>
                </button>
              </li>
            )
          })}
        </ul>
      </main>
    </div>
  )
}

export default App
