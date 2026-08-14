import { createContext, useContext, useState, type ReactNode } from 'react'
import { api } from './api'

interface Sesion {
  token: string
  idUsuario: string
  nombreUsuario: string
  roles: string[]
  menus: string[]
}

interface AuthContextValue {
  sesion: Sesion | null
  cargando: boolean
  login: (nombreUsuario: string, contrasena: string) => Promise<void>
  logout: () => void
  tieneMenu: (codigo: string) => boolean
}

const STORAGE_KEY = 'corela15:sesion'

const AuthContext = createContext<AuthContextValue | null>(null)

function leerSesionGuardada(): Sesion | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as Sesion
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [sesion, setSesion] = useState<Sesion | null>(leerSesionGuardada)
  const [cargando, setCargando] = useState(false)

  const login = async (nombreUsuario: string, contrasena: string) => {
    setCargando(true)
    try {
      const { data } = await api.post('/api/auth/login', { nombreUsuario, contrasena })
      const nuevaSesion: Sesion = {
        token: data.token,
        idUsuario: data.idUsuario,
        nombreUsuario: data.nombreUsuario,
        roles: data.roles,
        menus: data.menus,
      }
      localStorage.setItem(STORAGE_KEY, JSON.stringify(nuevaSesion))
      setSesion(nuevaSesion)
    } finally {
      setCargando(false)
    }
  }

  const logout = () => {
    localStorage.removeItem(STORAGE_KEY)
    setSesion(null)
  }

  const tieneMenu = (codigo: string) => sesion?.menus.includes(codigo) ?? false

  return (
    <AuthContext.Provider value={{ sesion, cargando, login, logout, tieneMenu }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth debe usarse dentro de <AuthProvider>')
  return ctx
}

export function getToken(): string | null {
  return leerSesionGuardada()?.token ?? null
}

export function limpiarSesion() {
  localStorage.removeItem(STORAGE_KEY)
}
