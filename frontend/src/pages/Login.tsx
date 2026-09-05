import { useState } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { Lock, User, Eye, EyeOff, ShieldCheck, Landmark, LineChart } from 'lucide-react'
import { useAuth } from '../lib/AuthContext'

type ApiError = { response?: { data?: { detail?: string } } }

const RASGOS = [
  { icon: Landmark, texto: 'Nuestras propias herramientas, hechas a medida' },
  { icon: ShieldCheck, texto: 'Acceso segmentado por rol, cada quien ve lo suyo' },
  { icon: LineChart, texto: 'Todo lo de la 15, en un solo lugar' },
]

export function Login() {
  const { login, cargando } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [nombreUsuario, setNombreUsuario] = useState('')
  const [contrasena, setContrasena] = useState('')
  const [verClave, setVerClave] = useState(false)
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
    <div className="flex min-h-svh items-stretch bg-[#f7f8fa]">
      {/* Panel de marca — solo en pantallas grandes, el petróleo corporativo
          se reserva para acá en vez de teñir el fondo general de la app. */}
      <div className="relative hidden w-[46%] flex-col justify-between overflow-hidden bg-petrol-400 p-12 text-petrol-900 lg:flex">
        <div
          className="pointer-events-none absolute inset-0 opacity-[0.07]"
          style={{
            backgroundImage:
              'radial-gradient(circle at 1px 1px, white 1px, transparent 0)',
            backgroundSize: '28px 28px',
          }}
        />
        <div
          className="pointer-events-none absolute -right-32 -top-32 h-96 w-96 rounded-full opacity-20 blur-3xl"
          style={{ background: 'radial-gradient(circle, #b58e4a, transparent 70%)' }}
        />
        <div
          className="pointer-events-none absolute -bottom-40 -left-20 h-96 w-96 rounded-full opacity-10 blur-3xl"
          style={{ background: 'radial-gradient(circle, #ffffff, transparent 70%)' }}
        />

        <div className="relative z-10 flex items-center gap-3">
          <img
            src="/logo.png"
            alt=""
            className="h-10 w-10 rounded-full bg-white/10 object-contain p-1 ring-1 ring-white/20"
            onError={(e) => {
              e.currentTarget.style.display = 'none'
            }}
          />
          <span className="text-sm font-semibold tracking-wide text-white">
            COOP. 15 DE AGOSTO
          </span>
        </div>

        <div className="relative z-10 max-w-md">
          <p className="mb-4 text-xs font-semibold uppercase tracking-[0.2em] text-gold-600">
            Apps La 15
          </p>
          <h1 className="mb-5 text-4xl font-semibold leading-[1.1] text-white">
            Corela15
          </h1>
          <p className="text-base leading-relaxed text-petrol-900/80">
            Nuestro portal, nuestras propias apps — muy pronto vas a encontrar acá todo lo de la 15.
          </p>

          <ul className="mt-10 flex flex-col gap-4">
            {RASGOS.map((r) => (
              <li key={r.texto} className="flex items-start gap-3">
                <span className="mt-0.5 flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-lg bg-white/10 ring-1 ring-white/15">
                  <r.icon size={15} className="text-gold-600" />
                </span>
                <span className="pt-1 text-sm leading-snug text-petrol-900/85">{r.texto}</span>
              </li>
            ))}
          </ul>
        </div>
      </div>

      {/* Panel de acceso */}
      <div className="flex flex-1 items-center justify-center px-4 py-12">
        <div className="w-full max-w-sm animate-fade-in">
          <div className="mb-8 flex flex-col items-center gap-3 text-center lg:hidden">
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
                Apps La 15 — Coop. 15 de Agosto
              </p>
            </div>
          </div>

          <div className="mb-7 hidden text-center lg:block">
            <p className="text-xl font-semibold text-graphite-100">Bienvenido de nuevo</p>
            <p className="mt-1 text-sm text-graphite-600">Ingresá con tu usuario del sistema.</p>
          </div>

          <form
            className="glass-card animate-zoom-in flex flex-col gap-4 rounded-2xl p-7 shadow-xl shadow-black/[0.04]"
            onSubmit={handleSubmit}
          >
            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium text-graphite-600">Usuario</span>
              <div className="relative">
                <User size={16} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-graphite-700" />
                <input
                  required
                  autoFocus
                  autoComplete="username"
                  value={nombreUsuario}
                  onChange={(e) => setNombreUsuario(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white py-2.5 pl-9 pr-3 text-graphite-100 outline-none transition-colors focus:border-gold-500/60 focus:ring-2 focus:ring-gold-500/15"
                />
              </div>
            </label>

            <label className="flex flex-col gap-1.5 text-sm">
              <span className="font-medium text-graphite-600">Contraseña</span>
              <div className="relative">
                <Lock size={16} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-graphite-700" />
                <input
                  required
                  type={verClave ? 'text' : 'password'}
                  autoComplete="current-password"
                  value={contrasena}
                  onChange={(e) => setContrasena(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white py-2.5 pl-9 pr-10 text-graphite-100 outline-none transition-colors focus:border-gold-500/60 focus:ring-2 focus:ring-gold-500/15"
                />
                <button
                  type="button"
                  onClick={() => setVerClave((v) => !v)}
                  tabIndex={-1}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-graphite-700 hover:text-graphite-400"
                  title={verClave ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                >
                  {verClave ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </label>

            <button
              type="submit"
              disabled={cargando}
              className="btn-hover mt-2 flex items-center justify-center gap-2 rounded-lg bg-gold-500 px-4 py-2.5 text-sm font-medium text-white shadow-sm shadow-gold-500/30 disabled:opacity-60"
            >
              <Lock size={16} />
              {cargando ? 'Ingresando…' : 'Ingresar'}
            </button>

            {error && (
              <p className="rounded-lg bg-red-600/10 px-3 py-2 text-center text-sm text-red-700">
                {error}
              </p>
            )}
          </form>

          <p className="mt-6 text-center text-xs text-graphite-700">
            Acceso restringido — uso exclusivo de personal autorizado de la cooperativa.
          </p>
        </div>
      </div>
    </div>
  )
}
