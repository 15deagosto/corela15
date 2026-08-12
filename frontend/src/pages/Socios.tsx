import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Users } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Socio {
  id: string
  numero: string
  nombre: string
  identificacion: string
  agencia: string
  estado: string
}

export function Socios() {
  const [q, setQ] = useState('')

  const { data, isLoading } = useQuery<Socio[]>({
    queryKey: ['socios', q],
    queryFn: async () => (await api.get('/api/socios', { params: { q: q || undefined } })).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Users} title="Socios" subtitle="Personas vinculadas a la cooperativa" />

      <div className="mb-4">
        <SearchBar value={q} onChange={setQ} placeholder="Buscar por nombre, número o identificación…" />
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Nombre</Th>
            <Th>Identificación</Th>
            <Th>Agencia</Th>
            <Th>Estado</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>No se encontraron socios</EmptyState>}
          {data?.map((s) => (
            <tr key={s.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{s.numero}</Td>
              <Td>{s.nombre}</Td>
              <Td>{s.identificacion}</Td>
              <Td>{s.agencia}</Td>
              <Td>
                <Badge variant={s.estado === 'Activo' ? 'exito' : 'neutral'}>{s.estado}</Badge>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
