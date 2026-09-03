import { useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { LogOut, User, KeyRound, X, Menu } from 'lucide-react'
import { api } from '../lib/api'
import { useAuth } from '../lib/AuthContext'

type Health = { status: string; timestampUtc: string }

function CambiarClaveModal({ onClose }: { onClose: () => void }) {
  const [contrasenaActual, setContrasenaActual] = useState('')
  const [contrasenaNueva, setContrasenaNueva] = useState('')
  const [confirmacion, setConfirmacion] = useState('')

  const cambiar = useMutation({
    mutationFn: async () => api.post('/api/auth/cambiar-clave', { contrasenaActual, contrasenaNueva }),
  })

  const noCoincide = confirmacion.length > 0 && confirmacion !== contrasenaNueva
  const mensajeError = (cambiar.error as { response?: { data?: { detail?: string } } } | undefined)?.response?.data
    ?.detail

  return (
    <div className="fixed inset-0 z-30 flex items-center justify-center bg-black/30 p-4">
      <div className="glass-strong animate-zoom-in w-full max-w-sm rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="flex items-center gap-1.5 font-medium text-graphite-100">
            <KeyRound size={16} /> Cambiar mi contraseña
          </h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        {cambiar.isSuccess ? (
          <div className="flex flex-col gap-3">
            <p className="rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
              Contraseña actualizada. Tus otras sesiones activas (en otros dispositivos) quedaron cerradas por
              seguridad — esta sigue vigente.
            </p>
            <button
              type="button"
              onClick={onClose}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white"
            >
              Cerrar
            </button>
          </div>
        ) : (
          <form
            className="flex flex-col gap-4"
            onSubmit={(e) => {
              e.preventDefault()
              if (noCoincide) return
              cambiar.mutate()
            }}
          >
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Contraseña actual</span>
              <input
                required
                type="password"
                value={contrasenaActual}
                onChange={(e) => setContrasenaActual(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Contraseña nueva (mínimo 8 caracteres)</span>
              <input
                required
                minLength={8}
                type="password"
                value={contrasenaNueva}
                onChange={(e) => setContrasenaNueva(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Confirmar contraseña nueva</span>
              <input
                required
                type="password"
                value={confirmacion}
                onChange={(e) => setConfirmacion(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
              {noCoincide && <span className="text-xs text-red-700">Las contraseñas no coinciden</span>}
            </label>

            <button
              type="submit"
              disabled={cambiar.isPending || noCoincide}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {cambiar.isPending ? 'Guardando…' : 'Cambiar contraseña'}
            </button>

            {cambiar.isError && (
              <p className="text-sm text-red-700">{mensajeError ?? 'No se pudo cambiar la contraseña.'}</p>
            )}
          </form>
        )}
      </div>
    </div>
  )
}

interface TopBarProps {
  onOpenSidebar: () => void
}

export function TopBar({ onOpenSidebar }: TopBarProps) {
  const { sesion, logout } = useAuth()
  const [mostrarCambiarClave, setMostrarCambiarClave] = useState(false)
  const { isLoading, isError } = useQuery<Health>({
    queryKey: ['health'],
    queryFn: async () => (await api.get('/health')).data,
    retry: 1,
  })

  return (
    <header className="flex h-14 shrink-0 items-center justify-between gap-3 border-b border-black/[0.06] bg-white px-4 sm:px-6">
      <div className="flex min-w-0 items-center gap-3">
        <button
          type="button"
          onClick={onOpenSidebar}
          className="-ml-1 flex shrink-0 items-center justify-center rounded-lg p-2 text-graphite-600 hover:bg-black/[0.03] hover:text-graphite-100 lg:hidden"
          aria-label="Abrir menú"
        >
          <Menu size={20} />
        </button>
        <p className="truncate text-sm font-medium text-graphite-600">
          Core financiero — Cooperativa 15 de Agosto
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-2 text-sm text-graphite-600 sm:gap-4">
        <div className="flex items-center gap-2" title={isLoading ? 'Conectando…' : isError ? 'Sin conexión' : 'Conectado'}>
          <span
            className={`h-2 w-2 shrink-0 rounded-full ${
              isLoading ? 'bg-gold-500' : isError ? 'bg-red-600' : 'bg-petrol-700 glow-petrol'
            }`}
          />
          <span className="hidden sm:inline">{isLoading ? 'Conectando…' : isError ? 'Sin conexión' : 'Conectado'}</span>
        </div>

        {sesion && (
          <>
            <div className="hidden h-4 w-px bg-black/[0.08] sm:block" />
            <div className="hidden items-center gap-1.5 sm:flex" title={sesion.roles.join(', ')}>
              <User size={14} />
              <span className="font-medium text-graphite-100">{sesion.nombreUsuario}</span>
              <span className="hidden text-xs text-graphite-600 md:inline">({sesion.roles.join(', ')})</span>
            </div>
            <button
              type="button"
              onClick={() => setMostrarCambiarClave(true)}
              title="Cambiar clave"
              className="flex items-center gap-1 rounded-lg px-2 py-1 text-xs font-medium text-graphite-600 hover:bg-black/[0.03] hover:text-graphite-100"
            >
              <KeyRound size={14} /> <span className="hidden sm:inline">Cambiar clave</span>
            </button>
            <button
              type="button"
              title="Salir"
              onClick={logout}
              className="flex items-center gap-1 rounded-lg px-2 py-1 text-xs font-medium text-graphite-600 hover:bg-black/[0.03] hover:text-red-700"
            >
              <LogOut size={14} /> Salir
            </button>
          </>
        )}
      </div>

      {mostrarCambiarClave && <CambiarClaveModal onClose={() => setMostrarCambiarClave(false)} />}
    </header>
  )
}
