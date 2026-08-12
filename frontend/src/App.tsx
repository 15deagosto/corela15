import { Routes, Route } from 'react-router-dom'
import { Layout } from './components/Layout'
import { Home } from './pages/Home'
import { Socios } from './pages/Socios'
import { UsuariosRoles } from './pages/UsuariosRoles'
import { Contabilidad } from './pages/Contabilidad'
import { Ahorros } from './pages/Ahorros'
import { ModuloPagina } from './pages/ModuloPagina'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/socios" element={<Socios />} />
        <Route path="/usuarios-roles" element={<UsuariosRoles />} />
        <Route path="/contabilidad" element={<Contabilidad />} />
        <Route path="/ahorros" element={<Ahorros />} />
        <Route path="/:slug" element={<ModuloPagina />} />
      </Route>
    </Routes>
  )
}

export default App
