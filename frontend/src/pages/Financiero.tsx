import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FileCheck, Plus, X } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Banco {
  id: number
  nombre: string
  esNacional: boolean
}

interface CuentaParaCheque {
  id: string
  numero: string
  socio: string
}

interface ChequeItem {
  id: string
  banco: string
  cuentaCorriente: string
  numeroCheque: string
  valor: number
  cuentaDestino: string
  fechaIngreso: string
  estado: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function estadoVariant(estado: string) {
  if (estado === 'Efectivizado') return 'exito' as const
  if (estado === 'Protestado' || estado === 'Anulado' || estado === 'Extraviado') return 'peligro' as const
  return 'neutral' as const
}

function RegistrarChequeForm({
  bancos,
  cuentas,
  onClose,
}: {
  bancos: Banco[]
  cuentas: CuentaParaCheque[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idBanco, setIdBanco] = useState('')
  const [cuentaCorriente, setCuentaCorriente] = useState('')
  const [numeroCheque, setNumeroCheque] = useState('')
  const [valor, setValor] = useState('')
  const [idCuenta, setIdCuenta] = useState('')

  const registrar = useMutation({
    mutationFn: async () =>
      api.post('/api/financiero/cheques', {
        idBanco: Number(idBanco),
        cuentaCorriente,
        numeroCheque,
        valor: Number(valor),
        idCuenta,
        idAgencia: 1,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financiero-cheques'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar cheque de terceros</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idBanco || !cuentaCorriente || !numeroCheque || !valor || !idCuenta) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Banco emisor</span>
          <select
            required
            value={idBanco}
            onChange={(e) => setIdBanco(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {bancos.map((b) => (
              <option key={b.id} value={b.id}>
                {b.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Cuenta corriente emisora</span>
          <input
            required
            type="text"
            value={cuentaCorriente}
            onChange={(e) => setCuentaCorriente(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Número de cheque</span>
          <input
            required
            type="text"
            value={numeroCheque}
            onChange={(e) => setNumeroCheque(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Valor</span>
          <input
            required
            type="number"
            step="0.01"
            min="0.01"
            value={valor}
            onChange={(e) => setValor(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Cuenta de ahorros donde se deposita</span>
          <select
            required
            value={idCuenta}
            onChange={(e) => setIdCuenta(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {cuentas.map((c) => (
              <option key={c.id} value={c.id}>
                {c.numero} — {c.socio}
              </option>
            ))}
          </select>
        </label>

        <div className="flex items-end">
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Registrando…' : 'Registrar cheque'}
          </button>
        </div>

        {registrar.isError && (
          <p className="sm:col-span-3 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar el cheque.'}
          </p>
        )}
      </form>
    </div>
  )
}

function ProtestarForm({ cheque, onClose }: { cheque: ChequeItem; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [documento, setDocumento] = useState('')

  const protestar = useMutation({
    mutationFn: async () =>
      api.post(
        `/api/financiero/cheques/${cheque.id}/protestar`,
        { documento: documento || null },
        { headers: { 'Idempotency-Key': crypto.randomUUID() } },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['financiero-cheques'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">
          Protestar cheque {cheque.numeroCheque} ({cheque.banco})
        </h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      <form
        className="flex flex-wrap items-end gap-3"
        onSubmit={(e) => {
          e.preventDefault()
          protestar.mutate()
        }}
      >
        <label className="flex flex-1 flex-col gap-1 text-sm">
          <span className="text-graphite-600">Documento / acta de protesto (opcional)</span>
          <input
            type="text"
            value={documento}
            onChange={(e) => setDocumento(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <button
          type="submit"
          disabled={protestar.isPending}
          className="btn-hover rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {protestar.isPending ? 'Protestando…' : 'Confirmar protesto'}
        </button>
        {protestar.isError && (
          <p className="w-full text-sm text-red-700">
            {(protestar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo protestar el cheque.'}
          </p>
        )}
      </form>
    </div>
  )
}

export function Financiero() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [chequeAProtestar, setChequeAProtestar] = useState<ChequeItem | null>(null)

  const { data: bancos } = useQuery<Banco[]>({
    queryKey: ['financiero-bancos'],
    queryFn: async () => (await api.get('/api/financiero/bancos')).data,
  })

  const { data: cuentas } = useQuery<CuentaParaCheque[]>({
    queryKey: ['financiero-cuentas'],
    queryFn: async () => (await api.get('/api/financiero/cuentas')).data,
  })

  const { data: cheques, isLoading } = useQuery<ChequeItem[]>({
    queryKey: ['financiero-cheques'],
    queryFn: async () => (await api.get('/api/financiero/cheques')).data,
  })

  const depositar = useMutation({
    mutationFn: async (id: string) =>
      api.post(`/api/financiero/cheques/${id}/depositar`, undefined, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['financiero-cheques'] }),
  })

  const efectivizar = useMutation({
    mutationFn: async (id: string) =>
      api.post(`/api/financiero/cheques/${id}/efectivizar`, undefined, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['financiero-cheques'] }),
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={FileCheck}
        title="Financiero"
        subtitle="Cheques de terceros recibidos en depósito"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Registrar cheque
            </button>
          )
        }
      />

      {mostrarForm && bancos && cuentas && (
        <RegistrarChequeForm bancos={bancos} cuentas={cuentas} onClose={() => setMostrarForm(false)} />
      )}

      {chequeAProtestar && <ProtestarForm cheque={chequeAProtestar} onClose={() => setChequeAProtestar(null)} />}

      <p className="mb-4 text-xs text-graphite-500">
        Al depositarse, el valor queda "Bloqueado" en la cuenta del socio (no retirable) hasta que el cheque se
        efectiviza — el socio nunca puede disponer de fondos de un cheque que todavía no se cobró.
      </p>

      <TableContainer>
        <thead>
          <tr>
            <Th>Banco</Th>
            <Th>Cta. corriente</Th>
            <Th>N° cheque</Th>
            <Th>Valor</Th>
            <Th>Depositado en</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (cheques?.length ?? 0) === 0 && <EmptyState>Todavía no hay cheques registrados</EmptyState>}
          {cheques?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{c.banco}</Td>
              <Td>{c.cuentaCorriente}</Td>
              <Td>{c.numeroCheque}</Td>
              <Td className="tabular-nums">{formatoUsd(c.valor)}</Td>
              <Td>{c.cuentaDestino}</Td>
              <Td>{c.fechaIngreso}</Td>
              <Td>
                <Badge variant={estadoVariant(c.estado)}>{c.estado}</Badge>
              </Td>
              <Td>
                {c.estado === 'Ingresado' && (
                  <button
                    type="button"
                    disabled={depositar.isPending}
                    onClick={() => depositar.mutate(c.id)}
                    className="text-xs font-medium text-gold-400 hover:underline disabled:opacity-50"
                  >
                    Depositar
                  </button>
                )}
                {c.estado === 'DepositadoEnBanco' && (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      disabled={efectivizar.isPending}
                      onClick={() => efectivizar.mutate(c.id)}
                      className="text-xs font-medium text-petrol-700 hover:underline disabled:opacity-50"
                    >
                      Efectivizar
                    </button>
                    <button
                      type="button"
                      onClick={() => setChequeAProtestar(c)}
                      className="text-xs font-medium text-red-700 hover:underline"
                    >
                      Protestar
                    </button>
                  </div>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
