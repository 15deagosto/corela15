import { useQuery } from '@tanstack/react-query'
import { api } from './lib/api'

type Health = { status: string; timestampUtc: string }

const niveles = [
  { numero: 0, nombre: 'Cimientos', detalle: 'Personas, Seguridad, Catálogos', estado: 'listo' },
  { numero: 1, nombre: 'Motor contable', detalle: 'Plan de cuentas SEPS, asientos, saldos', estado: 'pendiente' },
  { numero: 2, nombre: 'Ahorros', detalle: 'Captación a la vista', estado: 'pendiente' },
  { numero: 3, nombre: 'Plazo Fijo / Crédito', detalle: 'Colocación', estado: 'pendiente' },
  { numero: 4, nombre: 'Cobranzas / Cumplimiento', detalle: 'Prevención de lavado de activos', estado: 'pendiente' },
] as const

function EstadoBadge({ estado }: { estado: string }) {
  const isListo = estado === 'listo'
  return (
    <span
      className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${
        isListo
          ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300'
          : 'bg-zinc-100 text-zinc-500 dark:bg-zinc-800 dark:text-zinc-400'
      }`}
    >
      {isListo ? 'Listo' : 'Pendiente'}
    </span>
  )
}

function App() {
  const { data, isLoading, isError } = useQuery<Health>({
    queryKey: ['health'],
    queryFn: async () => (await api.get('/health')).data,
    retry: 1,
  })

  return (
    <div className="min-h-svh bg-zinc-50 text-zinc-900 dark:bg-zinc-950 dark:text-zinc-100">
      <header className="border-b border-zinc-200 dark:border-zinc-800">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-6 py-4">
          <div>
            <h1 className="text-lg font-semibold">Corela15 · Core financiero</h1>
            <p className="text-sm text-zinc-500 dark:text-zinc-400">
              Cooperativa 15 de Agosto — desarrollo local
            </p>
          </div>
          <div className="flex items-center gap-2 text-sm">
            <span
              className={`h-2 w-2 rounded-full ${
                isLoading ? 'bg-amber-400' : isError ? 'bg-red-500' : 'bg-emerald-500'
              }`}
            />
            {isLoading ? 'Conectando…' : isError ? 'API no disponible' : `API ok · ${data?.status}`}
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-6 py-10">
        <h2 className="mb-1 text-xl font-semibold">Módulos por nivel de dependencia</h2>
        <p className="mb-6 text-sm text-zinc-500 dark:text-zinc-400">
          Orden de construcción según 01-contexto-origen.md — un nivel nunca depende de uno posterior.
        </p>

        <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          {niveles.map((n) => (
            <li
              key={n.numero}
              className="rounded-xl border border-zinc-200 bg-white p-4 shadow-sm dark:border-zinc-800 dark:bg-zinc-900"
            >
              <div className="mb-1 flex items-center justify-between">
                <span className="text-xs font-medium text-zinc-400">Nivel {n.numero}</span>
                <EstadoBadge estado={n.estado} />
              </div>
              <h3 className="font-medium">{n.nombre}</h3>
              <p className="text-sm text-zinc-500 dark:text-zinc-400">{n.detalle}</p>
            </li>
          ))}
        </ul>
      </main>
    </div>
  )
}

export default App
