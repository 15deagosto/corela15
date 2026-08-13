import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'

type Health = { status: string; timestampUtc: string }

export function TopBar() {
  const { isLoading, isError } = useQuery<Health>({
    queryKey: ['health'],
    queryFn: async () => (await api.get('/health')).data,
    retry: 1,
  })

  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-black/[0.06] bg-white px-6">
      <p className="text-sm font-medium text-graphite-600">Core financiero — Cooperativa 15 de Agosto</p>
      <div className="flex items-center gap-2 text-sm text-graphite-600">
        <span
          className={`h-2 w-2 rounded-full ${
            isLoading ? 'bg-gold-500' : isError ? 'bg-red-600' : 'bg-petrol-700 glow-petrol'
          }`}
        />
        {isLoading ? 'Conectando…' : isError ? 'Sin conexión' : 'Conectado'}
      </div>
    </header>
  )
}
