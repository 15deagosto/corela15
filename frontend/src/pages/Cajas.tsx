import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Wallet, Plus, X, Lock } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface UsuarioParaCaja {
  id: string
  nombreUsuario: string
}

interface VentanillaListItem {
  id: string
  usuario: string
  agencia: string
  fecha: string
  cerrada: boolean
  cuadrada: boolean
}

function AbrirVentanillaForm({ usuarios, onClose }: { usuarios: UsuarioParaCaja[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [idUsuario, setIdUsuario] = useState('')

  const abrir = useMutation({
    mutationFn: async () => (await api.post('/api/cajas/ventanillas', { idUsuario, idAgencia: 1 })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-ventanillas'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Abrir ventanilla</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idUsuario) return
          abrir.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Cajero</span>
          <select
            required
            value={idUsuario}
            onChange={(e) => setIdUsuario(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {usuarios.map((u) => (
              <option key={u.id} value={u.id}>
                {u.nombreUsuario}
              </option>
            ))}
          </select>
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={abrir.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {abrir.isPending ? 'Abriendo…' : 'Abrir ventanilla'}
          </button>
        </div>

        {abrir.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(abrir.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo abrir la ventanilla.'}
          </p>
        )}
      </form>
    </div>
  )
}

function CerrarVentanillaForm({ ventanilla, onClose }: { ventanilla: VentanillaListItem; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [totalEfectivoContado, setTotalEfectivoContado] = useState('')
  const [totalCheque, setTotalCheque] = useState('0')

  const cerrar = useMutation({
    mutationFn: async () =>
      (
        await api.post(`/api/cajas/ventanillas/${ventanilla.id}/cerrar`, {
          totalEfectivoContado: Number(totalEfectivoContado),
          totalCheque: Number(totalCheque),
          registradoPor: ventanilla.usuario,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-ventanillas'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">
          Cerrar ventanilla — {ventanilla.usuario} ({ventanilla.agencia})
        </h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!totalEfectivoContado) return
          cerrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Efectivo contado</span>
          <input
            required
            type="number"
            step="0.01"
            min="0"
            value={totalEfectivoContado}
            onChange={(e) => setTotalEfectivoContado(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Cheques</span>
          <input
            type="number"
            step="0.01"
            min="0"
            value={totalCheque}
            onChange={(e) => setTotalCheque(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={cerrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {cerrar.isPending ? 'Cerrando…' : 'Cerrar y cuadrar'}
          </button>
        </div>

        {cerrar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(cerrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo cerrar la ventanilla.'}
          </p>
        )}
      </form>
    </div>
  )
}

export function Cajas() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [ventanillaACerrar, setVentanillaACerrar] = useState<VentanillaListItem | null>(null)

  const { data: usuarios } = useQuery<UsuarioParaCaja[]>({
    queryKey: ['cajas-usuarios'],
    queryFn: async () => (await api.get('/api/cajas/usuarios')).data,
  })

  const { data: ventanillas, isLoading } = useQuery<VentanillaListItem[]>({
    queryKey: ['cajas-ventanillas'],
    queryFn: async () => (await api.get('/api/cajas/ventanillas')).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={Wallet}
        title="Cajas"
        subtitle="Apertura, transacciones y cuadre diario de ventanillas"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Abrir ventanilla
            </button>
          )
        }
      />

      {mostrarForm && usuarios && <AbrirVentanillaForm usuarios={usuarios} onClose={() => setMostrarForm(false)} />}

      {ventanillaACerrar && (
        <CerrarVentanillaForm ventanilla={ventanillaACerrar} onClose={() => setVentanillaACerrar(null)} />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Cajero</Th>
            <Th>Agencia</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th>Cuadre</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (ventanillas?.length ?? 0) === 0 && <EmptyState>Todavía no hay ventanillas registradas</EmptyState>}
          {ventanillas?.map((v) => (
            <tr key={v.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{v.usuario}</Td>
              <Td>{v.agencia}</Td>
              <Td>{v.fecha}</Td>
              <Td>
                <Badge variant={v.cerrada ? 'neutral' : 'exito'}>{v.cerrada ? 'Cerrada' : 'Abierta'}</Badge>
              </Td>
              <Td>
                {v.cerrada ? <Badge variant={v.cuadrada ? 'exito' : 'peligro'}>{v.cuadrada ? 'Cuadrada' : 'Descuadrada'}</Badge> : '—'}
              </Td>
              <Td>
                {!v.cerrada && (
                  <button
                    type="button"
                    onClick={() => setVentanillaACerrar(v)}
                    className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                  >
                    <Lock size={13} /> Cerrar
                  </button>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
