import { Routes, Route } from 'react-router-dom'
import { Layout } from './components/Layout'
import { RequireAuth } from './components/RequireAuth'
import { Login } from './pages/Login'
import { modulos } from './modules'

// Los módulos ya no se renderizan acá — `Layout` monta cada uno vía
// `TabsWorkspace` (ver lib/TabsContext.tsx) para mantenerlos abiertos como
// pestañas internas sin perder su estado al cambiar entre ellos. Estas
// rutas solo existen para que la URL real (deep-link, recarga, atrás/
// adelante del navegador) siga funcionando — el `element` no se usa
// porque `Layout` no tiene `<Outlet/>`.
function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        element={
          <RequireAuth>
            <Layout />
          </RequireAuth>
        }
      >
        <Route path="/" element={null} />
        {modulos.map((m) => (
          <Route key={m.slug} path={m.path} element={null} />
        ))}
        <Route path="*" element={null} />
      </Route>
    </Routes>
  )
}

export default App
