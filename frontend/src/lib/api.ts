import axios from 'axios'

const STORAGE_KEY = 'corela15:sesion'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5080',
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
    if (error.response?.status === 401 && !error.config?.url?.includes('/api/auth/login')) {
      localStorage.removeItem(STORAGE_KEY)
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)
