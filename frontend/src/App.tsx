import { Routes, Route } from 'react-router-dom'
import { Layout } from './components/Layout'
import { Home } from './pages/Home'
import { ModuloPagina } from './pages/ModuloPagina'

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/:slug" element={<ModuloPagina />} />
      </Route>
    </Routes>
  )
}

export default App
