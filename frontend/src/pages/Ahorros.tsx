import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { PiggyBank, Plus, X, ArrowDownCircle, ArrowUpCircle } from 'lucide-react'
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
  saldoDisponible: number
}

interface Socio {
  id: string
  numero: string
  nombre: string
}

interface TipoTransaccion {
  codigo: string
  nombre: string
  signoSaldoCuenta: number
}

interface AbrirCuentaPayload {
  idCliente: string
  idTipoCuenta: number
  idAgencia: number
  montoInicial: number
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function AbrirCuentaForm({ productos, onClose }: { productos: Producto[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [idCliente, setIdCliente] = useState('')
  const [idTipoCuenta, setIdTipoCuenta] = useState(productos[0]?.id ?? 0)
  const [montoInicial, setMontoInicial] = useState('0')

  const { data: socios } = useQuery<Socio[]>({
    queryKey: ['socios', ''],
    queryFn: async () => (await api.get('/api/socios')).data,
  })

  const abrir = useMutation({
    mutationFn: async (payload: AbrirCuentaPayload) => (await api.post('/api/ahorros/cuentas', payload)).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Abrir cuenta de ahorro</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idCliente) return
          abrir.mutate({
            idCliente,
            idTipoCuenta,
            idAgencia: 1,
            montoInicial: Number(montoInicial) || 0,
          })
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Socio</span>
          <select
            required
            value={idCliente}
            onChange={(e) => setIdCliente(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {socios?.map((s) => (
              <option key={s.id} value={s.id}>
                {s.numero} — {s.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Producto</span>
          <select
            value={idTipoCuenta}
            onChange={(e) => setIdTipoCuenta(Number(e.target.value))}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            {productos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Depósito inicial (USD)</span>
          <input
            type="number"
            min="0"
            step="0.01"
            value={montoInicial}
            onChange={(e) => setMontoInicial(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={abrir.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {abrir.isPending ? 'Abriendo…' : 'Abrir cuenta'}
          </button>
        </div>

        {abrir.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            No se pudo abrir la cuenta. Revisá los datos e intentá de nuevo.
          </p>
        )}
      </form>
    </div>
  )
}

function MovimientoModal({ cuenta, onClose }: { cuenta: CuentaAhorro; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [codigoTipoTransaccion, setCodigoTipoTransaccion] = useState('DEP-EFEC')
  const [monto, setMonto] = useState('0')

  const { data: tipos } = useQuery<TipoTransaccion[]>({
    queryKey: ['ahorros-tipos-transaccion'],
    queryFn: async () => (await api.get('/api/ahorros/tipos-transaccion')).data,
  })

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/ahorros/cuentas/${cuenta.id}/movimientos`,
          { codigoTipoTransaccion, monto: Number(monto) || 0 },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
      onClose()
    },
  })

  const mensajeError = (registrar.error as { response?: { data?: { detail?: string } } } | undefined)?.response?.data
    ?.detail

  return (
    <div className="fixed inset-0 z-20 flex items-center justify-center bg-black/30 p-4">
      <div className="glass-strong animate-zoom-in w-full max-w-sm rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h3 className="font-medium text-graphite-100">Movimiento — cuenta {cuenta.numero}</h3>
            <p className="text-xs text-graphite-600">Saldo disponible: {formatoUsd(cuenta.saldoDisponible)}</p>
          </div>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            registrar.mutate()
          }}
        >
          <div className="flex gap-2">
            {tipos?.map((t) => (
              <button
                key={t.codigo}
                type="button"
                onClick={() => setCodigoTipoTransaccion(t.codigo)}
                className={`flex flex-1 items-center justify-center gap-1.5 rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
                  codigoTipoTransaccion === t.codigo
                    ? 'border-gold-500/50 bg-gold-500/10 text-gold-300'
                    : 'border-black/[0.08] text-graphite-600 hover:bg-black/[0.02]'
                }`}
              >
                {t.signoSaldoCuenta > 0 ? <ArrowDownCircle size={16} /> : <ArrowUpCircle size={16} />}
                {t.nombre}
              </button>
            ))}
          </div>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Monto (USD)</span>
            <input
              type="number"
              min="0.01"
              step="0.01"
              required
              value={monto}
              onChange={(e) => setMonto(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Procesando…' : 'Confirmar'}
          </button>

          {registrar.isError && (
            <p className="text-sm text-red-700">{mensajeError ?? 'No se pudo registrar el movimiento.'}</p>
          )}
        </form>
      </div>
    </div>
  )
}

export function Ahorros() {
  const [q, setQ] = useState('')
  const [mostrarForm, setMostrarForm] = useState(false)
  const [cuentaMovimiento, setCuentaMovimiento] = useState<CuentaAhorro | null>(null)

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
      <PageHeader
        icon={PiggyBank}
        title="Ahorros"
        subtitle="Cuentas de ahorro y captación a la vista"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Abrir cuenta
            </button>
          )
        }
      />

      {mostrarForm && productos && <AbrirCuentaForm productos={productos} onClose={() => setMostrarForm(false)} />}
      {cuentaMovimiento && (
        <MovimientoModal cuenta={cuentaMovimiento} onClose={() => setCuentaMovimiento(null)} />
      )}

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
            <Th>Saldo</Th>
            <Th>Estado</Th>
            <Th>Acciones</Th>
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
              <Td className="tabular-nums">{formatoUsd(c.saldoDisponible)}</Td>
              <Td>
                <Badge variant={c.estado === 'Activa' ? 'exito' : 'neutral'}>{c.estado}</Badge>
              </Td>
              <Td>
                {c.estado === 'Activa' && (
                  <button
                    type="button"
                    onClick={() => setCuentaMovimiento(c)}
                    className="text-sm font-medium text-gold-400 hover:underline"
                  >
                    Movimiento
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
