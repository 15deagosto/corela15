import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { PiggyBank } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Producto {
  id: number
  codigo: string
  nombre: string
  permiteDebitoPrestamo: boolean
  activo: boolean
}

interface CuentaAhorro {
  id: string
  numero: string
  producto: string
  agencia: string
  estado: string
  fechaApertura: string
}

export function Ahorros() {
  const [q, setQ] = useState('')

  const { data: productos } = useQuery<Producto[]>({
    queryKey: ['ahorros-productos'],
    queryFn: async () => (await api.get('/api/ahorros/productos')).data,
  })

  const { data: cuentas, isLoading } = useQuery<CuentaAhorro[]>({
    queryKey: ['ahorros-cuentas', q],
    queryFn: async () => (await api.get('/api/ahorros/cuentas', { params: { q: q || undefined } })).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader icon={PiggyBank} title="Ahorros" subtitle="Cuentas de ahorro y captación a la vista" />

      <div className="mb-6 flex flex-wrap gap-2">
        {productos?.map((p) => (
          <div key={p.id} className="glass-card rounded-lg px-3 py-2 text-sm">
            <p className="font-medium text-graphite-100">{p.nombre}</p>
            <p className="text-xs text-graphite-600">
              {p.permiteDebitoPrestamo ? 'Admite débito de préstamo' : 'Sin débito de préstamo'}
            </p>
          </div>
        ))}
      </div>

      <div className="mb-4">
        <SearchBar value={q} onChange={setQ} placeholder="Buscar por número de cuenta…" />
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Producto</Th>
            <Th>Agencia</Th>
            <Th>Apertura</Th>
            <Th>Estado</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (cuentas?.length ?? 0) === 0 && (
            <EmptyState>Todavía no hay cuentas de ahorro abiertas</EmptyState>
          )}
          {cuentas?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{c.numero}</Td>
              <Td>{c.producto}</Td>
              <Td>{c.agencia}</Td>
              <Td>{c.fechaApertura}</Td>
              <Td>
                <Badge variant={c.estado === 'Activa' ? 'exito' : 'neutral'}>{c.estado}</Badge>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
