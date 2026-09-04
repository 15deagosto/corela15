import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { KeyRound } from 'lucide-react'
import { api } from '../lib/api'
import { useAuth } from '../lib/AuthContext'

/**
 * Pantalla de bloqueo real — se muestra en vez de toda la app cuando
 * `sesion.cambiaClave` es true (usuario con clave temporal, ej. los
 * importados de Softbank con clave = su propio nombre de usuario). No es
 * un modal que se pueda cerrar: hasta que no cambie la clave, no navega a
 * ningún módulo. Reusa el mismo endpoint que "Cambiar mi contraseña" del
 * TopBar — la diferencia es que acá es obligatorio, no opcional.
 */
export function CambioClaveObligatorio() {
  const { refrescarPermisos, logout } = useAuth()
  const [contrasenaActual, setContrasenaActual] = useState('')
  const [contrasenaNueva, setContrasenaNueva] = useState('')
  const [confirmacion, setConfirmacion] = useState('')

  const cambiar = useMutation({
    mutationFn: async () => api.post('/api/auth/cambiar-clave', { contrasenaActual, contrasenaNueva }),
    onSuccess: () => refrescarPermisos(),
  })

  const noCoincide = confirmacion.length > 0 && confirmacion !== contrasenaNueva
  const mensajeError = (cambiar.error as { response?: { data?: { detail?: string } } } | undefined)?.response?.data
    ?.detail

  return (
    <div className="flex min-h-svh items-center justify-center bg-graphite-950/[0.02] p-4">
      <div className="glass-strong animate-zoom-in w-full max-w-sm rounded-xl p-6">
        <div className="mb-4 flex items-center gap-2">
          <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-gold-500/15 text-gold-400">
            <KeyRound size={18} />
          </span>
          <div>
            <h3 className="font-medium text-graphite-100">Cambio de contraseña obligatorio</h3>
            <p className="text-xs text-graphite-600">Tu clave actual es temporal — actualizala para continuar.</p>
          </div>
        </div>

        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            if (noCoincide) return
            cambiar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Contraseña temporal actual</span>
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
            {cambiar.isPending ? 'Guardando…' : 'Cambiar contraseña y continuar'}
          </button>

          {cambiar.isError && <p className="text-xs text-red-700">{mensajeError ?? 'Ocurrió un error inesperado'}</p>}

          <button
            type="button"
            onClick={() => logout()}
            className="text-center text-xs text-graphite-600 underline hover:text-graphite-100"
          >
            Cerrar sesión
          </button>
        </form>
      </div>
    </div>
  )
}
