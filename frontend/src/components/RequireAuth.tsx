import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../lib/AuthContext'
import { CambioClaveObligatorio } from './CambioClaveObligatorio'

export function RequireAuth({ children }: { children: React.ReactNode }) {
  const { sesion } = useAuth()
  const location = useLocation()

  if (!sesion) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />
  }

  // Clave temporal (ej. usuarios importados de Softbank con clave = su
  // propio nombre de usuario) — bloquea toda la app hasta que la cambien,
  // no se puede navegar a ningún módulo mientras esto sea true.
  if (sesion.cambiaClave) {
    return <CambioClaveObligatorio />
  }

  return <>{children}</>
}
