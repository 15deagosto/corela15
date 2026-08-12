import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Calculator } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface CuentaContable {
  id: string
  codigo: string
  nombre: string
  grupo: string
  naturaleza: string
  esMayor: boolean
  activa: boolean
  codigoPadre: string | null
}

export function Contabilidad() {
  const [q, setQ] = useState('')

  const { data, isLoading } = useQuery<CuentaContable[]>({
    queryKey: ['cuentas-contables', q],
    queryFn: async () => (await api.get('/api/contabilidad/cuentas', { params: { q: q || undefined } })).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Calculator} title="Contabilidad" subtitle="Plan de cuentas, sobre el Catálogo Único de Cuentas (CUC) de la SEPS" />

      <div className="mb-4">
        <SearchBar value={q} onChange={setQ} placeholder="Buscar por código o nombre…" />
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Cuenta</Th>
            <Th>Grupo</Th>
            <Th>Naturaleza</Th>
            <Th>Tipo</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>No se encontraron cuentas</EmptyState>}
          {data?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-mono text-xs font-medium">{c.codigo}</Td>
              <Td style={{ paddingLeft: `${c.codigo.length > 1 ? (c.codigo.length - 1) * 12 + 16 : 16}px` }}>
                {c.nombre}
              </Td>
              <Td>{c.grupo}</Td>
              <Td>
                <Badge variant={c.naturaleza === 'Deudora' ? 'alerta' : 'exito'}>{c.naturaleza}</Badge>
              </Td>
              <Td>
                <Badge>{c.esMayor ? 'Detalle' : 'Agrupación'}</Badge>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
