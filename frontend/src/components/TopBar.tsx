import { useQuery } from '@tanstack/react-query'
import { LogOut, User } from 'lucide-react'
import { api } from '../lib/api'
import { useAuth } from '../lib/AuthContext'

type Health = { status: string; timestampUtc: string }

export function TopBar() {
  const { sesion, logout } = useAuth()
  const { isLoading, isError } = useQuery<Health>({
    queryKey: ['health'],
    queryFn: async () => (await api.get('/health')).data,
    retry: 1,
  })

  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-black/[0.06] bg-white px-6">
      <p className="text-sm font-medium text-graphite-600">Core financiero — Cooperativa 15 de Agosto</p>
      <div className="flex items-center gap-4 text-sm text-graphite-600">
        <div className="flex items-center gap-2">
          <span
            className={`h-2 w-2 rounded-full ${
              isLoading ? 'bg-gold-500' : isError ? 'bg-red-600' : 'bg-petrol-700 glow-petrol'
            }`}
          />
          {isLoading ? 'Conectando…' : isError ? 'Sin conexión' : 'Conectado'}
        </div>

        {sesion && (
          <>
            <div className="h-4 w-px bg-black/[0.08]" />
            <div className="flex items-center gap-1.5">
              <User size={14} />
              <span className="font-medium text-graphite-100">{sesion.nombreUsuario}</span>
              <span className="text-xs text-graphite-600">({sesion.roles.join(', ')})</span>
            </div>
            <button
              type="button"
              onClick={logout}
              className="flex items-center gap-1 rounded-lg px-2 py-1 text-xs font-medium text-graphite-600 hover:bg-black/[0.03] hover:text-red-700"
            >
              <LogOut size={14} /> Salir
            </button>
          </>
        )}
      </div>
    </header>
  )
}
