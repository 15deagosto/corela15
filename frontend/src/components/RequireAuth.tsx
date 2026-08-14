import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../lib/AuthContext'

export function RequireAuth({ children }: { children: React.ReactNode }) {
  const { sesion } = useAuth()
  const location = useLocation()

  if (!sesion) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />
  }

  return <>{children}</>
}
