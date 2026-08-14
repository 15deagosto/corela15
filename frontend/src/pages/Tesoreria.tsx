import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Landmark as LandmarkIcon, Plus, X, HandCoins } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Persona {
  id: string
  nombre: string
}

interface CuentaPorCobrar {
  id: string
  concepto: string
  persona: string
  montoInicial: number
  saldo: number
  fechaCreacion: string
  fechaVencimiento: string
  estado: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function RegistrarCxCForm({ personas, onClose }: { personas: Persona[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [concepto, setConcepto] = useState('')
  const [idPersona, setIdPersona] = useState('')
  const [cuotas, setCuotas] = useState('1')
  const [montoInicial, setMontoInicial] = useState('')
  const [fechaVencimiento, setFechaVencimiento] = useState('')

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/tesoreria/cuentas-por-cobrar', {
          concepto,
          idAgencia: 1,
          idPersona,
          cuotas: Number(cuotas),
          montoInicial: Number(montoInicial),
          fechaVencimiento,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-cxc'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar cuenta por cobrar</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!concepto || !idPersona || !montoInicial || !fechaVencimiento) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Concepto</span>
          <input
            required
            type="text"
            value={concepto}
            onChange={(e) => setConcepto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Persona</span>
          <select
            required
            value={idPersona}
            onChange={(e) => setIdPersona(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {personas.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Cuotas</span>
          <input
            type="number"
            min="1"
            value={cuotas}
            onChange={(e) => setCuotas(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Monto inicial</span>
          <input
            required
            type="number"
            step="0.01"
            min="0.01"
            value={montoInicial}
            onChange={(e) => setMontoInicial(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Fecha de vencimiento</span>
          <input
            required
            type="date"
            value={fechaVencimiento}
            onChange={(e) => setFechaVencimiento(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Registrando…' : 'Registrar'}
          </button>
        </div>

        {registrar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar la cuenta por cobrar.'}
          </p>
        )}
      </form>
    </div>
  )
}

function AbonarForm({ cuenta, onClose }: { cuenta: CuentaPorCobrar; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [monto, setMonto] = useState('')

  const abonar = useMutation({
    mutationFn: async () =>
      (
        await api.post(`/api/tesoreria/cuentas-por-cobrar/${cuenta.id}/abonos`, {
          monto: Number(monto),
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-cxc'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">
          Abonar — {cuenta.concepto} ({cuenta.persona})
        </h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="flex items-end gap-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!monto) return
          abonar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Monto a abonar (saldo {formatoUsd(cuenta.saldo)})</span>
          <input
            required
            type="number"
            step="0.01"
            min="0.01"
            value={monto}
            onChange={(e) => setMonto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <button
          type="submit"
          disabled={abonar.isPending}
          className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {abonar.isPending ? 'Abonando…' : 'Abonar'}
        </button>

        {abonar.isError && (
          <p className="text-sm text-red-700">
            {(abonar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar el abono.'}
          </p>
        )}
      </form>
    </div>
  )
}

export function Tesoreria() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [cuentaAAbonar, setCuentaAAbonar] = useState<CuentaPorCobrar | null>(null)

  const { data: personas } = useQuery<Persona[]>({
    queryKey: ['tesoreria-personas'],
    queryFn: async () => (await api.get('/api/tesoreria/cuentas-por-cobrar/personas')).data,
  })

  const { data: cuentas, isLoading } = useQuery<CuentaPorCobrar[]>({
    queryKey: ['tesoreria-cxc'],
    queryFn: async () => (await api.get('/api/tesoreria/cuentas-por-cobrar')).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={LandmarkIcon}
        title="Tesorería"
        subtitle="Cuentas por cobrar internas de la cooperativa"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Registrar cuenta por cobrar
            </button>
          )
        }
      />

      {mostrarForm && personas && <RegistrarCxCForm personas={personas} onClose={() => setMostrarForm(false)} />}

      {cuentaAAbonar && <AbonarForm cuenta={cuentaAAbonar} onClose={() => setCuentaAAbonar(null)} />}

      <TableContainer>
        <thead>
          <tr>
            <Th>Concepto</Th>
            <Th>Persona</Th>
            <Th>Monto inicial</Th>
            <Th>Saldo</Th>
            <Th>Vencimiento</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (cuentas?.length ?? 0) === 0 && <EmptyState>Todavía no hay cuentas por cobrar registradas</EmptyState>}
          {cuentas?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{c.concepto}</Td>
              <Td>{c.persona}</Td>
              <Td>{formatoUsd(c.montoInicial)}</Td>
              <Td>{formatoUsd(c.saldo)}</Td>
              <Td>{c.fechaVencimiento}</Td>
              <Td>
                <Badge variant={c.estado === 'Cancelada' ? 'neutral' : c.estado === 'Castigada' ? 'peligro' : 'exito'}>
                  {c.estado}
                </Badge>
              </Td>
              <Td>
                {c.estado === 'Vigente' && (
                  <button
                    type="button"
                    onClick={() => setCuentaAAbonar(c)}
                    className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                  >
                    <HandCoins size={13} /> Abonar
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
