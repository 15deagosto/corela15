import { useParams } from 'react-router-dom'
import { Construction } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { modulos } from '../modules'

export function ModuloPagina() {
  const { slug } = useParams()
  const modulo = modulos.find((m) => m.slug === slug)

  if (!modulo) {
    return null
  }

  return (
    <div className="animate-fade-in">
      <PageHeader icon={modulo.icon} title={modulo.nombre} subtitle={modulo.descripcion} />

      <div className="glass-card flex flex-col items-center gap-3 rounded-xl px-6 py-16 text-center">
        <Construction size={28} strokeWidth={1.5} className="text-gold-400" />
        <p className="font-medium text-graphite-100">
          {modulo.estado === 'disponible' ? 'Falta la pantalla para este módulo' : 'Este módulo está en desarrollo'}
        </p>
        <p className="max-w-sm text-sm text-graphite-600">
          {modulo.estado === 'disponible'
            ? 'La estructura de datos ya existe en el backend; falta construir la pantalla para usarla.'
            : 'Todavía se está construyendo tanto la base de datos como la pantalla.'}
        </p>
      </div>
    </div>
  )
}
