import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react'
import { api } from './api'

interface Sesion {
  token: string
  idUsuario: string
  nombreUsuario: string
  roles: string[]
  menus: string[]
  estructuras: string[]
  datasets: string[]
  opciones: string[]
  cambiaClave: boolean
}

interface AuthContextValue {
  sesion: Sesion | null
  cargando: boolean
  login: (nombreUsuario: string, contrasena: string) => Promise<void>
  logout: () => Promise<void>
  tieneMenu: (codigo: string) => boolean
  tieneEstructura: (codigo: string) => boolean
  /** Datasets del módulo Reportería Gerencial (ver seguridad.dataset_reporteria) que el usuario tiene otorgados. */
  tieneDataset: (codigo: string) => boolean
  /** Tercer nivel de permiso, genérico: un reporte o acción puntual dentro de cualquier módulo (ver seguridad.opcion). */
  tieneOpcion: (codigo: string) => boolean
  /** Vuelve a pedir un token con los datos reales actuales, sin contraseña — lo usa el mount inicial y la pantalla de cambio de clave obligatorio tras completarlo. */
  refrescarPermisos: () => Promise<void>
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

function mapearSesion(data: {
  token: string
  idUsuario: string
  nombreUsuario: string
  roles: string[]
  menus: string[]
  estructuras?: string[]
  datasets?: string[]
  opciones?: string[]
  cambiaClave?: boolean
}): Sesion {
  return {
    token: data.token,
    idUsuario: data.idUsuario,
    nombreUsuario: data.nombreUsuario,
    roles: data.roles,
    menus: data.menus,
    estructuras: data.estructuras ?? [],
    datasets: data.datasets ?? [],
    opciones: data.opciones ?? [],
    cambiaClave: data.cambiaClave ?? false,
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [sesion, setSesion] = useState<Sesion | null>(leerSesionGuardada)
  const [cargando, setCargando] = useState(false)
  const yaRefresco = useRef(false)

  const refrescarPermisos = async () => {
    const { data } = await api.post('/api/auth/refrescar')
    const sesionActualizada = mapearSesion(data)
    localStorage.setItem(STORAGE_KEY, JSON.stringify(sesionActualizada))
    setSesion(sesionActualizada)
  }

  // Los permisos de una sesión ya iniciada se calculaban una sola vez al
  // login — si un admin le otorgaba/quitaba un rol a alguien que ya tenía
  // la app abierta, no se enteraba hasta cerrar sesión y volver a entrar.
  // Al cargar la app (con una sesión guardada), se pide un token nuevo con
  // los permisos reales AHORA MISMO, sin pedir contraseña — así, con solo
  // recargar la página, el usuario ve exactamente lo que le corresponde en
  // ese momento. Corre una sola vez por carga real de la app (guard con
  // ref, nunca por cada cambio de sesión) para no reintentar en bucle.
  useEffect(() => {
    if (yaRefresco.current || !sesion) return
    yaRefresco.current = true
    refrescarPermisos().catch(() => {
      // Si falla (token ya inválido, usuario bloqueado mientras tanto,
      // sin red), el interceptor de axios ya maneja el 401 real
      // limpiando la sesión y mandando a /login — acá no hay nada más
      // que hacer, la sesión vieja se queda como estaba si fue solo un
      // problema de red pasajero.
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const login = async (nombreUsuario: string, contrasena: string) => {
    setCargando(true)
    try {
      const { data } = await api.post('/api/auth/login', { nombreUsuario, contrasena })
      const nuevaSesion = mapearSesion(data)
      localStorage.setItem(STORAGE_KEY, JSON.stringify(nuevaSesion))
      setSesion(nuevaSesion)
    } finally {
      setCargando(false)
    }
  }

  const logout = async () => {
    // Revoca la sesión en el servidor ANTES de limpiar el token local — si
    // se limpia primero, el interceptor de axios ya no tiene token para
    // mandar y el logout quedaría solo del lado del cliente (el token
    // seguiría siendo válido hasta expirar solo, ver
    // seguridad.sesion_usuario). Best-effort: si el request falla (sin
    // red, backend caído), igual se limpia localmente.
    try {
      await api.post('/api/auth/logout')
    } catch {
      // sin conexión o token ya inválido — no bloquea el logout local
    }
    localStorage.removeItem(STORAGE_KEY)
    setSesion(null)
  }

  const tieneMenu = (codigo: string) => sesion?.menus.includes(codigo) ?? false
  const tieneEstructura = (codigo: string) => sesion?.estructuras.includes(codigo) ?? false
  const tieneDataset = (codigo: string) => sesion?.datasets.includes(codigo) ?? false
  const tieneOpcion = (codigo: string) => sesion?.opciones.includes(codigo) ?? false

  return (
    <AuthContext.Provider
      value={{ sesion, cargando, login, logout, tieneMenu, tieneEstructura, tieneDataset, tieneOpcion, refrescarPermisos }}
    >
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
