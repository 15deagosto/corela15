import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ShieldCheck } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface UsuarioRol {
  id: string
  nombreUsuario: string
  nombrePersona: string | null
  agencia: string
  activo: boolean
  roles: string[]
}

export function UsuariosRoles() {
  const [q, setQ] = useState('')

  const { data, isLoading } = useQuery<UsuarioRol[]>({
    queryKey: ['usuarios', q],
    queryFn: async () => (await api.get('/api/usuarios', { params: { q: q || undefined } })).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader icon={ShieldCheck} title="Usuarios y roles" subtitle="Accesos, permisos y seguridad del sistema" />

      <div className="mb-4">
        <SearchBar value={q} onChange={setQ} placeholder="Buscar usuario o nombre…" />
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Usuario</Th>
            <Th>Nombre</Th>
            <Th>Agencia</Th>
            <Th>Estado</Th>
            <Th>Roles</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>No se encontraron usuarios</EmptyState>}
          {data?.map((u) => (
            <tr key={u.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{u.nombreUsuario}</Td>
              <Td>{u.nombrePersona ?? '—'}</Td>
              <Td>{u.agencia}</Td>
              <Td>
                <Badge variant={u.activo ? 'exito' : 'neutral'}>{u.activo ? 'Activo' : 'Inactivo'}</Badge>
              </Td>
              <Td>
                <div className="flex flex-wrap gap-1.5">
                  {u.roles.length === 0 && <span className="text-graphite-700">—</span>}
                  {u.roles.map((r) => (
                    <Badge key={r}>{r}</Badge>
                  ))}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
