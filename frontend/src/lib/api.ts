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
    // Solo 401 (sesión inválida/expirada/revocada) fuerza logout — 403
    // NUNCA lo hace. Bug real encontrado: cuando esta regla trataba
    // ambos casos igual, un usuario sin acceso a UN dataset puntual de
    // Reportería Gerencial (o UNA opción/reporte puntual de Créditos)
    // quedaba deslogueado por completo al abrir Explorador, aunque su
    // sesión seguía siendo perfectamente válida para todo lo demás. La
    // premisa original de tratar 403 como 401 ("en esta API un 403
    // siempre significa sesión con claims viejos, nunca una regla de
    // negocio") dejó de ser cierta en cuanto se agregó autorización
    // fina por recurso (`ReporteriaController` -- 403 por dataset
    // puntual vía Forbid() manual, y `OpcionFilter` -- 403 por
    // opción/reporte puntual) — ninguna de las dos es "sesión vieja",
    // son límites de permiso reales y esperados que la pantalla debe
    // poder mostrar sin perder la sesión completa.
    //
    // El caso real que sí motivó tratar 403 como logout (un menú nuevo
    // otorgado después del login, claims viejos, pantalla rota a medio
    // cargar) ya está resuelto por otro mecanismo, más reciente y más
    // correcto: AuthContext refresca los permisos automáticamente al
    // cargar la app (`POST /api/auth/refrescar`, ver "Refresco real de
    // permisos") — con solo recargar la página los claims quedan al
    // día, sin necesidad de un logout forzado.
    if (error.response?.status === 401 && !error.config?.url?.includes('/api/auth/login')) {
      localStorage.removeItem(STORAGE_KEY)
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)
