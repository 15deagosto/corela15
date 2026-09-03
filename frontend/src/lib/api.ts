import axios from 'axios'

const STORAGE_KEY = 'corela15:sesion'

// Por defecto apunta al backend en el MISMO host desde el que se sirvió el
// frontend (window.location.hostname) — nunca "localhost" fijo. Si el
// frontend se abre por IP de red (ej. http://172.16.0.77:5174, para
// compartir la app con un compañero en la misma red), "localhost" fijo
// apuntaría a la propia máquina de quien lo abre, no a la del backend real.
// VITE_API_URL sigue disponible para forzar una URL distinta si hace falta.
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? `http://${window.location.hostname}:5080`,
})

api.interceptors.request.use((config) => {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (raw) {
    try {
      const { token } = JSON.parse(raw) as { token?: string }
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
    } catch {
      // sesión corrupta en localStorage — se ignora, el 401 subsecuente limpia la sesión
    }
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    // 401 = sesión inválida/expirada. 403 = el token es válido pero no
    // trae el permiso de menú que este endpoint exige — en esta API eso
    // pasa solo cuando los permisos del rol cambiaron después del login
    // (los claims del JWT se calculan una sola vez al iniciar sesión, ver
    // CLAUDE.md "Autenticación real"), nunca como un estado de negocio
    // esperado. Antes un 403 fallaba en silencio y dejaba la pantalla a
    // medio cargar sin ninguna salida visible — se trata igual que un
    // 401 real: cierra la sesión local y manda a login. Las pestañas
    // abiertas no se pierden (viven en una clave de localStorage
    // separada, `corela15-pestanas-abiertas`), así que volver a entrar
    // las restaura tal como estaban.
    if (
      (error.response?.status === 401 || error.response?.status === 403) &&
      !error.config?.url?.includes('/api/auth/login')
    ) {
      localStorage.removeItem(STORAGE_KEY)
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)
