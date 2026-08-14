import { Fragment, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ShieldCheck, KeyRound, X } from 'lucide-react'
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

interface SesionUsuario {
  id: string
  emitidaEn: string
  expiraEn: string
  direccionIp: string | null
  revocada: boolean
  revocadaEn: string | null
  revocadaPor: string | null
  vigente: boolean
}

function formatoFecha(iso: string) {
  return new Date(iso).toLocaleString('es-EC')
}

function SesionesDelUsuario({ usuario, onClose }: { usuario: UsuarioRol; onClose: () => void }) {
  const queryClient = useQueryClient()

  const { data: sesiones, isLoading } = useQuery<SesionUsuario[]>({
    queryKey: ['usuario-sesiones', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}/sesiones`)).data,
  })

  const revocarTodas = useMutation({
    mutationFn: async () => api.post(`/api/usuarios/${usuario.id}/sesiones/revocar-todas`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-sesiones', usuario.id] })
    },
  })

  const hayVigentes = sesiones?.some((s) => s.vigente) ?? false

  return (
    <tr>
      <Td colSpan={5}>
        <div className="glass-card animate-zoom-in rounded-xl p-4">
          <div className="mb-3 flex items-center justify-between">
            <h4 className="flex items-center gap-1.5 text-sm font-medium text-graphite-100">
              <KeyRound size={15} /> Sesiones de {usuario.nombreUsuario}
            </h4>
            <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
              <X size={16} />
            </button>
          </div>

          {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}

          {sesiones && (
            <>
              <div className="mb-3 flex items-center gap-3">
                <button
                  type="button"
                  onClick={() => revocarTodas.mutate()}
                  disabled={!hayVigentes || revocarTodas.isPending}
                  className="btn-hover rounded-lg bg-red-600 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-40"
                >
                  {revocarTodas.isPending ? 'Cerrando…' : 'Cerrar todas las sesiones'}
                </button>
                {revocarTodas.isSuccess && (
                  <span className="text-xs text-petrol-700">
                    Listo — cualquier token emitido antes de ahora ya no es válido, aunque no haya expirado.
                  </span>
                )}
                {!hayVigentes && !revocarTodas.isSuccess && (
                  <span className="text-xs text-graphite-600">No hay sesiones activas para cerrar.</span>
                )}
              </div>

              <div className="max-h-64 overflow-y-auto rounded-lg border border-black/[0.06]">
                <table className="w-full text-xs">
                  <thead>
                    <tr className="bg-black/[0.02] text-left text-graphite-600">
                      <th className="px-3 py-1.5 font-medium">Emitida</th>
                      <th className="px-3 py-1.5 font-medium">Expira</th>
                      <th className="px-3 py-1.5 font-medium">IP</th>
                      <th className="px-3 py-1.5 font-medium">Estado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {sesiones.length === 0 && (
                      <tr>
                        <td colSpan={4} className="px-3 py-4 text-center text-graphite-600">
                          Sin sesiones registradas
                        </td>
                      </tr>
                    )}
                    {sesiones.map((s) => (
                      <tr key={s.id} className="border-t border-black/[0.04]">
                        <td className="px-3 py-1.5 text-graphite-100">{formatoFecha(s.emitidaEn)}</td>
                        <td className="px-3 py-1.5 text-graphite-100">{formatoFecha(s.expiraEn)}</td>
                        <td className="px-3 py-1.5 text-graphite-600">{s.direccionIp ?? '—'}</td>
                        <td className="px-3 py-1.5">
                          {s.vigente ? (
                            <Badge variant="exito">Vigente</Badge>
                          ) : s.revocada ? (
                            <Badge variant="peligro">Revocada</Badge>
                          ) : (
                            <Badge variant="neutral">Expirada</Badge>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </div>
      </Td>
    </tr>
  )
}

export function UsuariosRoles() {
  const [q, setQ] = useState('')
  const [usuarioSesiones, setUsuarioSesiones] = useState<UsuarioRol | null>(null)

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
            <Fragment key={u.id}>
              <tr className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{u.nombreUsuario}</Td>
                <Td>{u.nombrePersona ?? '—'}</Td>
                <Td>{u.agencia}</Td>
                <Td>
                  <Badge variant={u.activo ? 'exito' : 'neutral'}>{u.activo ? 'Activo' : 'Inactivo'}</Badge>
                </Td>
                <Td>
                  <div className="flex flex-wrap items-center justify-between gap-1.5">
                    <div className="flex flex-wrap gap-1.5">
                      {u.roles.length === 0 && <span className="text-graphite-700">—</span>}
                      {u.roles.map((r) => (
                        <Badge key={r}>{r}</Badge>
                      ))}
                    </div>
                    <button
                      type="button"
                      onClick={() => setUsuarioSesiones(usuarioSesiones?.id === u.id ? null : u)}
                      className="flex items-center gap-1 text-xs font-medium text-gold-400 hover:underline"
                    >
                      <KeyRound size={13} /> Sesiones
                    </button>
                  </div>
                </Td>
              </tr>
              {usuarioSesiones?.id === u.id && (
                <SesionesDelUsuario usuario={u} onClose={() => setUsuarioSesiones(null)} />
              )}
            </Fragment>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
