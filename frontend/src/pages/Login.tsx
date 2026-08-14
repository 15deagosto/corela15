import { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Lock } from 'lucide-react'
import { useAuth } from '../lib/AuthContext'

type ApiError = { response?: { data?: { detail?: string } } }

export function Login() {
  const { login, cargando } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [nombreUsuario, setNombreUsuario] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [error, setError] = useState<string | null>(null)

  const destino = (location.state as { from?: string } | null)?.from ?? '/'

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    try {
      await login(nombreUsuario, contrasena)
      navigate(destino, { replace: true })
    } catch (err) {
      setError((err as ApiError)?.response?.data?.detail ?? 'No se pudo iniciar sesión.')
    }
  }

  return (
    <div className="flex min-h-svh items-center justify-center bg-[#f7f8fa] px-4">
      <div className="glass-card w-full max-w-sm rounded-2xl p-8">
        <div className="mb-6 flex flex-col items-center gap-3 text-center">
          <img
            src="/logo.png"
            alt="Cooperativa 15 de Agosto"
            className="h-14 w-14 rounded-full object-contain"
            onError={(e) => {
              e.currentTarget.style.display = 'none'
            }}
          />
          <div>
            <p className="text-lg font-semibold text-graphite-100">Corela15</p>
            <p className="text-xs font-medium uppercase tracking-wide text-graphite-600">
              Core financiero — Coop. 15 de Agosto
            </p>
          </div>
        </div>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Usuario</span>
            <input
              required
              autoFocus
              value={nombreUsuario}
              onChange={(e) => setNombreUsuario(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Contraseña</span>
            <input
              required
              type="password"
              value={contrasena}
              onChange={(e) => setContrasena(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <button
            type="submit"
            disabled={cargando}
            className="btn-hover mt-2 flex items-center justify-center gap-2 rounded-lg bg-gold-500 px-4 py-2.5 text-sm font-medium text-white disabled:opacity-60"
          >
            <Lock size={16} />
            {cargando ? 'Ingresando…' : 'Ingresar'}
          </button>

          {error && <p className="text-center text-sm text-red-700">{error}</p>}
        </form>
      </div>
    </div>
  )
}
