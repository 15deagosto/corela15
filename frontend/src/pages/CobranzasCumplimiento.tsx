import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ShieldAlert, Plus, X } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Accion {
  id: number
  codigo: string
  nombre: string
}

interface PrestamoParaCobranza {
  id: string
  idCliente: string
  numero: string
  socio: string
  saldo: number
  estado: string
}

interface Gestion {
  id: string
  prestamoNumero: string
  socio: string
  accion: string
  tieneCompromisoPago: boolean
  observacion: string | null
  fecha: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function RegistrarGestionForm({
  prestamos,
  acciones,
  onClose,
}: {
  prestamos: PrestamoParaCobranza[]
  acciones: Accion[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idPrestamo, setIdPrestamo] = useState('')
  const [codigoAccionGestion, setCodigoAccionGestion] = useState(acciones[0]?.codigo ?? '')
  const [tieneCompromisoPago, setTieneCompromisoPago] = useState(false)
  const [observacion, setObservacion] = useState('')

  const prestamoSeleccionado = prestamos.find((p) => p.id === idPrestamo)

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/cobranzas/gestiones', {
          idPrestamo,
          idCliente: prestamoSeleccionado?.idCliente,
          esDeudor: true,
          codigoAccionGestion,
          tieneCompromisoPago,
          observacion: observacion || null,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cobranzas-gestiones'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar gestión de cobranza</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idPrestamo) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Préstamo</span>
          <select
            required
            value={idPrestamo}
            onChange={(e) => setIdPrestamo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {prestamos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.numero} — {p.socio} (saldo {formatoUsd(p.saldo)})
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Acción</span>
          <select
            value={codigoAccionGestion}
            onChange={(e) => setCodigoAccionGestion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            {acciones.map((a) => (
              <option key={a.codigo} value={a.codigo}>
                {a.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex items-center gap-2 text-sm sm:col-span-2">
          <input
            type="checkbox"
            checked={tieneCompromisoPago}
            onChange={(e) => setTieneCompromisoPago(e.target.checked)}
            className="h-4 w-4 rounded border-black/[0.2] accent-[#b58e4a]"
          />
          <span className="text-graphite-600">El socio se comprometió a pagar</span>
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Observación</span>
          <textarea
            value={observacion}
            onChange={(e) => setObservacion(e.target.value)}
            rows={2}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Guardando…' : 'Registrar'}
          </button>
        </div>

        {registrar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar la gestión.'}
          </p>
        )}
      </form>
    </div>
  )
}

export function CobranzasCumplimiento() {
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: acciones } = useQuery<Accion[]>({
    queryKey: ['cobranzas-acciones'],
    queryFn: async () => (await api.get('/api/cobranzas/acciones')).data,
  })

  const { data: prestamos } = useQuery<PrestamoParaCobranza[]>({
    queryKey: ['cobranzas-prestamos'],
    queryFn: async () => (await api.get('/api/cobranzas/prestamos')).data,
  })

  const { data: gestiones, isLoading } = useQuery<Gestion[]>({
    queryKey: ['cobranzas-gestiones'],
    queryFn: async () => (await api.get('/api/cobranzas/gestiones')).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={ShieldAlert}
        title="Cobranzas y Cumplimiento"
        subtitle="Gestión de mora y prevención de lavado de activos"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Registrar gestión
            </button>
          )
        }
      />

      {mostrarForm && prestamos && acciones && (
        <RegistrarGestionForm prestamos={prestamos} acciones={acciones} onClose={() => setMostrarForm(false)} />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Préstamo</Th>
            <Th>Socio</Th>
            <Th>Acción</Th>
            <Th>Compromiso de pago</Th>
            <Th>Observación</Th>
            <Th>Fecha</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (gestiones?.length ?? 0) === 0 && <EmptyState>Todavía no hay gestiones de cobranza registradas</EmptyState>}
          {gestiones?.map((g) => (
            <tr key={g.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{g.prestamoNumero}</Td>
              <Td>{g.socio}</Td>
              <Td>{g.accion}</Td>
              <Td>
                <Badge variant={g.tieneCompromisoPago ? 'exito' : 'neutral'}>
                  {g.tieneCompromisoPago ? 'Sí' : 'No'}
                </Badge>
              </Td>
              <Td>{g.observacion ?? '—'}</Td>
              <Td>{g.fecha}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
