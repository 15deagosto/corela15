import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Landmark as LandmarkIcon, Plus, X, HandCoins, Ban, ShoppingCart, Trash2 } from 'lucide-react'
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

interface CuentaPorPagar {
  id: string
  concepto: string
  persona: string
  cuotas: number
  montoInicial: number
  saldo: number
  fechaCreacion: string
  fechaVencimiento: string
  estado: string
}

interface FormaCancelacion {
  codigo: string
  nombre: string
  activo: boolean
}

interface Proveedor {
  id: string
  identificacion: string
  idTipoIdentificacion: number
  tipoIdentificacion: string
  nombre: string
  direccion: string | null
  telefono: string | null
  email: string | null
  esContribuyenteEspecial: boolean
  obligadoLlevarContabilidad: boolean
  activo: boolean
}

interface TipoIdentificacionCompra {
  id: number
  nombre: string
}

interface TipoComprobanteCompra {
  codigo: string
  nombre: string
}

interface CuentaContableCompra {
  id: string
  codigo: string
  nombre: string
}

interface CompraDetalleItem {
  idCuentaContable: string
  cuentaContable: string
  detalle: string
  cantidad: number
  valorUnitario: number
  porcentajeIva: number
  subtotal: number
  montoIva: number
  total: number
}

interface Compra {
  id: string
  numero: string
  proveedor: string
  codigoSustento: string
  tipoComprobante: string
  establecimiento: string
  puntoEmision: string
  secuencial: string
  autorizacion: string
  fechaEmision: string
  concepto: string
  subtotal: number
  montoIva: number
  montoRetencion: number
  total: number
  saldo: number
  estado: string
  detalle: CompraDetalleItem[]
}

const SUSTENTOS_COMPRA = [
  { codigo: '01', nombre: '01 — Crédito tributario' },
  { codigo: '02', nombre: '02 — Costo o gasto' },
  { codigo: '04', nombre: '04 — Costo o gasto por reembolso' },
]

function mensajeErrorTesoreria(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
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
        await api.post(
          `/api/tesoreria/cuentas-por-cobrar/${cuenta.id}/abonos`,
          { monto: Number(monto) },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
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

function RegistrarCxPForm({ personas, onClose }: { personas: Persona[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [concepto, setConcepto] = useState('')
  const [idPersona, setIdPersona] = useState('')
  const [cuotas, setCuotas] = useState('1')
  const [montoInicial, setMontoInicial] = useState('')
  const [fechaVencimiento, setFechaVencimiento] = useState('')

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          '/api/tesoreria/cuentas-por-pagar',
          {
            concepto,
            idAgencia: 1,
            idPersona,
            cuotas: Number(cuotas),
            montoInicial: Number(montoInicial),
            fechaVencimiento,
          },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-cxp'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar cuenta por pagar</h3>
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
          <span className="text-graphite-600">Persona / proveedor</span>
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
              'No se pudo registrar la cuenta por pagar.'}
          </p>
        )}
      </form>
    </div>
  )
}

function PagarCxPForm({
  cuenta,
  formasCancelacion,
  onClose,
}: {
  cuenta: CuentaPorPagar
  formasCancelacion: FormaCancelacion[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [monto, setMonto] = useState('')
  const [codigoFormaCancelacion, setCodigoFormaCancelacion] = useState('')

  const pagar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/tesoreria/cuentas-por-pagar/${cuenta.id}/pagos`,
          { monto: Number(monto), codigoFormaCancelacion },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-cxp'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">
          Pagar — {cuenta.concepto} ({cuenta.persona})
        </h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="flex flex-wrap items-end gap-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!monto || !codigoFormaCancelacion) return
          pagar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Monto a pagar (saldo {formatoUsd(cuenta.saldo)})</span>
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

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Forma de cancelación</span>
          <select
            required
            value={codigoFormaCancelacion}
            onChange={(e) => setCodigoFormaCancelacion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {formasCancelacion.map((f) => (
              <option key={f.codigo} value={f.codigo}>
                {f.nombre}
              </option>
            ))}
          </select>
        </label>

        <button
          type="submit"
          disabled={pagar.isPending}
          className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {pagar.isPending ? 'Pagando…' : 'Pagar'}
        </button>

        {pagar.isError && (
          <p className="w-full text-sm text-red-700">
            {(pagar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar el pago.'}
          </p>
        )}
      </form>
    </div>
  )
}

function AnularCxPForm({ cuenta, onClose }: { cuenta: CuentaPorPagar; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [motivo, setMotivo] = useState('')

  const anular = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/tesoreria/cuentas-por-pagar/${cuenta.id}/anular`,
          { motivo },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-cxp'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">
          Anular — {cuenta.concepto} ({cuenta.persona})
        </h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="flex flex-wrap items-end gap-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!motivo) return
          anular.mutate()
        }}
      >
        <label className="flex flex-1 flex-col gap-1 text-sm">
          <span className="text-graphite-600">Motivo de anulación</span>
          <input
            required
            type="text"
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <button
          type="submit"
          disabled={anular.isPending}
          className="btn-hover rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {anular.isPending ? 'Anulando…' : 'Anular'}
        </button>

        {anular.isError && (
          <p className="w-full text-sm text-red-700">
            {(anular.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo anular la cuenta por pagar.'}
          </p>
        )}
      </form>
    </div>
  )
}

function SeccionCuentasPorCobrar() {
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
    <div>
      <div className="mb-4 flex justify-end">
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <Plus size={16} /> Registrar cuenta por cobrar
          </button>
        )}
      </div>

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

function SeccionCuentasPorPagar() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [cuentaAPagar, setCuentaAPagar] = useState<CuentaPorPagar | null>(null)
  const [cuentaAAnular, setCuentaAAnular] = useState<CuentaPorPagar | null>(null)

  const { data: personas } = useQuery<Persona[]>({
    queryKey: ['tesoreria-personas'],
    queryFn: async () => (await api.get('/api/tesoreria/cuentas-por-cobrar/personas')).data,
  })

  const { data: cuentas, isLoading } = useQuery<CuentaPorPagar[]>({
    queryKey: ['tesoreria-cxp'],
    queryFn: async () => (await api.get('/api/tesoreria/cuentas-por-pagar')).data,
  })

  const { data: formasCancelacion } = useQuery<FormaCancelacion[]>({
    queryKey: ['tesoreria-formas-cancelacion'],
    queryFn: async () => (await api.get('/api/tesoreria/cuentas-por-pagar/formas-cancelacion')).data,
  })

  const formasActivas = (formasCancelacion ?? []).filter((f) => f.activo)

  return (
    <div>
      <div className="mb-4 flex justify-end">
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <Plus size={16} /> Registrar cuenta por pagar
          </button>
        )}
      </div>

      {mostrarForm && personas && <RegistrarCxPForm personas={personas} onClose={() => setMostrarForm(false)} />}

      {cuentaAPagar && (
        <PagarCxPForm cuenta={cuentaAPagar} formasCancelacion={formasActivas} onClose={() => setCuentaAPagar(null)} />
      )}

      {cuentaAAnular && <AnularCxPForm cuenta={cuentaAAnular} onClose={() => setCuentaAAnular(null)} />}

      <TableContainer>
        <thead>
          <tr>
            <Th>Concepto</Th>
            <Th>Persona / proveedor</Th>
            <Th>Cuotas</Th>
            <Th>Monto inicial</Th>
            <Th>Saldo</Th>
            <Th>Vencimiento</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (cuentas?.length ?? 0) === 0 && <EmptyState>Todavía no hay cuentas por pagar registradas</EmptyState>}
          {cuentas?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{c.concepto}</Td>
              <Td>{c.persona}</Td>
              <Td>{c.cuotas}</Td>
              <Td>{formatoUsd(c.montoInicial)}</Td>
              <Td>{formatoUsd(c.saldo)}</Td>
              <Td>{c.fechaVencimiento}</Td>
              <Td>
                <Badge variant={c.estado === 'Cancelada' ? 'neutral' : c.estado === 'Anulada' ? 'peligro' : 'exito'}>
                  {c.estado}
                </Badge>
              </Td>
              <Td>
                {c.estado === 'Vigente' && (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => setCuentaAPagar(c)}
                      className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                    >
                      <HandCoins size={13} /> Pagar
                    </button>
                    {c.saldo === c.montoInicial && (
                      <button
                        type="button"
                        onClick={() => setCuentaAAnular(c)}
                        className="flex items-center gap-1 text-xs font-medium text-red-700 hover:underline"
                      >
                        <Ban size={13} /> Anular
                      </button>
                    )}
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

export function Tesoreria() {
  const [tab, setTab] = useState<'cxc' | 'cxp' | 'compras'>('cxc')

  return (
    <div className="animate-fade-in">
      <PageHeader icon={LandmarkIcon} title="Tesorería" subtitle="Cuentas por cobrar y por pagar internas de la cooperativa" />

      <div className="mb-6 flex gap-1 border-b border-black/[0.08]">
        <button
          type="button"
          onClick={() => setTab('cxc')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            tab === 'cxc' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
          }`}
        >
          Cuentas por cobrar
        </button>
        <button
          type="button"
          onClick={() => setTab('cxp')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            tab === 'cxp' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
          }`}
        >
          Cuentas por pagar
        </button>
        <button
          type="button"
          onClick={() => setTab('compras')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            tab === 'compras' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
          }`}
        >
          Compras
        </button>
      </div>

      {tab === 'cxc' && <SeccionCuentasPorCobrar />}
      {tab === 'cxp' && <SeccionCuentasPorPagar />}
      {tab === 'compras' && <SeccionCompras />}
    </div>
  )
}

function NuevoProveedorForm({ onCreado, onClose }: { onCreado: (idProveedor: string) => void; onClose: () => void }) {
  const [idTipoIdentificacion, setIdTipoIdentificacion] = useState('')
  const [identificacion, setIdentificacion] = useState('')
  const [nombre, setNombre] = useState('')
  const [direccion, setDireccion] = useState('')
  const [telefono, setTelefono] = useState('')
  const [email, setEmail] = useState('')
  const [esContribuyenteEspecial, setEsContribuyenteEspecial] = useState(false)
  const [obligadoLlevarContabilidad, setObligadoLlevarContabilidad] = useState(false)

  const { data: tiposIdentificacion } = useQuery<TipoIdentificacionCompra[]>({
    queryKey: ['tesoreria-compras-tipos-identificacion'],
    queryFn: async () => (await api.get('/api/tesoreria/compras/tipos-identificacion')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/tesoreria/compras/proveedores', {
          idTipoIdentificacion: Number(idTipoIdentificacion),
          identificacion,
          nombre,
          direccion: direccion || null,
          telefono: telefono || null,
          email: email || null,
          esContribuyenteEspecial,
          obligadoLlevarContabilidad,
        })
      ).data as { id: string },
    onSuccess: (data) => onCreado(data.id),
  })

  return (
    <div className="glass-card mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-graphite-100">Nuevo proveedor</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={16} />
        </button>
      </div>
      <form
        className="grid grid-cols-1 gap-3 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idTipoIdentificacion || !identificacion || !nombre) return
          crear.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Tipo de identificación</span>
          <select
            value={idTipoIdentificacion}
            onChange={(e) => setIdTipoIdentificacion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {tiposIdentificacion?.map((t) => (
              <option key={t.id} value={t.id}>
                {t.nombre}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Identificación</span>
          <input
            value={identificacion}
            onChange={(e) => setIdentificacion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Nombre / razón social</span>
          <input
            value={nombre}
            onChange={(e) => setNombre(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Dirección</span>
          <input
            value={direccion}
            onChange={(e) => setDireccion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Teléfono</span>
          <input
            value={telefono}
            onChange={(e) => setTelefono(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Email</span>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={esContribuyenteEspecial} onChange={(e) => setEsContribuyenteEspecial(e.target.checked)} />
          <span className="text-graphite-600">Contribuyente especial</span>
        </label>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={obligadoLlevarContabilidad} onChange={(e) => setObligadoLlevarContabilidad(e.target.checked)} />
          <span className="text-graphite-600">Obligado a llevar contabilidad</span>
        </label>
        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={crear.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {crear.isPending ? 'Guardando…' : 'Registrar proveedor'}
          </button>
        </div>
      </form>
      {crear.isError && <p className="mt-2 text-sm text-red-700">{mensajeErrorTesoreria(crear.error, 'No se pudo registrar el proveedor.')}</p>}
    </div>
  )
}

interface LineaCompraForm {
  idCuentaContable: string
  cuentaContableNombre: string
  detalle: string
  cantidad: string
  valorUnitario: string
  porcentajeIva: string
}

function BuscadorCuentaContable({ onSeleccionar }: { onSeleccionar: (cuenta: CuentaContableCompra) => void }) {
  const [q, setQ] = useState('')
  const { data: resultados } = useQuery<CuentaContableCompra[]>({
    queryKey: ['tesoreria-compras-cuentas', q],
    queryFn: async () => (await api.get('/api/tesoreria/compras/cuentas-contables', { params: { q: q || undefined } })).data,
    enabled: q.length >= 2,
  })

  return (
    <div className="relative">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar cuenta contable (código o nombre)…"
        className="w-full rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
      />
      {q.length >= 2 && (resultados?.length ?? 0) > 0 && (
        <div className="absolute z-10 mt-1 max-h-48 w-full overflow-y-auto rounded-lg border border-black/[0.08] bg-white shadow-lg">
          {resultados?.map((c) => (
            <button
              key={c.id}
              type="button"
              onClick={() => {
                onSeleccionar(c)
                setQ('')
              }}
              className="block w-full px-3 py-2 text-left text-sm hover:bg-black/[0.03]"
            >
              {c.codigo} — {c.nombre}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

function RegistrarCompraForm({ proveedores, onClose }: { proveedores: Proveedor[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [idProveedor, setIdProveedor] = useState('')
  const [codigoSustento, setCodigoSustento] = useState('01')
  const [codigoTipoComprobante, setCodigoTipoComprobante] = useState('01')
  const [establecimiento, setEstablecimiento] = useState('001')
  const [puntoEmision, setPuntoEmision] = useState('001')
  const [secuencial, setSecuencial] = useState('')
  const [autorizacion, setAutorizacion] = useState('')
  const [fechaEmision, setFechaEmision] = useState('')
  const [concepto, setConcepto] = useState('')
  const [montoRetencion, setMontoRetencion] = useState('0')
  const [lineas, setLineas] = useState<LineaCompraForm[]>([])
  const [cuentaPendiente, setCuentaPendiente] = useState<CuentaContableCompra | null>(null)
  const [detalleLinea, setDetalleLinea] = useState('')
  const [cantidadLinea, setCantidadLinea] = useState('1')
  const [valorLinea, setValorLinea] = useState('')
  const [ivaLinea, setIvaLinea] = useState('0.12')

  const { data: tiposComprobante } = useQuery<TipoComprobanteCompra[]>({
    queryKey: ['tesoreria-compras-tipos-comprobante'],
    queryFn: async () => (await api.get('/api/tesoreria/compras/tipos-comprobante')).data,
  })

  const subtotalLineas = lineas.reduce((acc, l) => acc + Number(l.cantidad) * Number(l.valorUnitario), 0)
  const ivaLineas = lineas.reduce((acc, l) => acc + Number(l.cantidad) * Number(l.valorUnitario) * Number(l.porcentajeIva), 0)
  const totalLineas = subtotalLineas + ivaLineas

  const agregarLinea = () => {
    if (!cuentaPendiente || !detalleLinea || !cantidadLinea || !valorLinea) return
    setLineas((prev) => [
      ...prev,
      {
        idCuentaContable: cuentaPendiente.id,
        cuentaContableNombre: `${cuentaPendiente.codigo} — ${cuentaPendiente.nombre}`,
        detalle: detalleLinea,
        cantidad: cantidadLinea,
        valorUnitario: valorLinea,
        porcentajeIva: ivaLinea,
      },
    ])
    setCuentaPendiente(null)
    setDetalleLinea('')
    setCantidadLinea('1')
    setValorLinea('')
  }

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          '/api/tesoreria/compras',
          {
            idProveedor,
            idAgencia: 1,
            codigoSustento,
            codigoTipoComprobante,
            establecimiento,
            puntoEmision,
            secuencial,
            autorizacion,
            fechaEmision,
            concepto,
            montoRetencion: Number(montoRetencion) || 0,
            detalle: lineas.map((l) => ({
              idCuentaContable: l.idCuentaContable,
              detalle: l.detalle,
              cantidad: Number(l.cantidad),
              valorUnitario: Number(l.valorUnitario),
              porcentajeIva: Number(l.porcentajeIva),
            })),
          },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-compras'] })
      onClose()
    },
  })

  return (
    <div className="glass-card mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-graphite-100">Registrar compra / factura de proveedor</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={16} />
        </button>
      </div>

      <form
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idProveedor || !secuencial || !autorizacion || !fechaEmision || !concepto || lineas.length === 0) return
          registrar.mutate()
        }}
      >
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            <span className="text-graphite-600">Proveedor</span>
            <select
              value={idProveedor}
              onChange={(e) => setIdProveedor(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {proveedores.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nombre} — {p.identificacion}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Sustento tributario</span>
            <select
              value={codigoSustento}
              onChange={(e) => setCodigoSustento(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              {SUSTENTOS_COMPRA.map((s) => (
                <option key={s.codigo} value={s.codigo}>
                  {s.nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Tipo de comprobante</span>
            <select
              value={codigoTipoComprobante}
              onChange={(e) => setCodigoTipoComprobante(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              {tiposComprobante?.map((t) => (
                <option key={t.codigo} value={t.codigo}>
                  {t.nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Fecha de emisión</span>
            <input
              type="date"
              value={fechaEmision}
              onChange={(e) => setFechaEmision(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Establecimiento</span>
            <input
              value={establecimiento}
              onChange={(e) => setEstablecimiento(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Punto de emisión</span>
            <input
              value={puntoEmision}
              onChange={(e) => setPuntoEmision(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Secuencial</span>
            <input
              value={secuencial}
              onChange={(e) => setSecuencial(e.target.value)}
              placeholder="000012345"
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            <span className="text-graphite-600">Autorización SRI</span>
            <input
              value={autorizacion}
              onChange={(e) => setAutorizacion(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm sm:col-span-3">
            <span className="text-graphite-600">Concepto</span>
            <input
              value={concepto}
              onChange={(e) => setConcepto(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Retención (valor)</span>
            <input
              type="number"
              step="0.01"
              min="0"
              value={montoRetencion}
              onChange={(e) => setMontoRetencion(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
        </div>

        <div className="rounded-lg border border-black/[0.08] p-3">
          <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Líneas de detalle</h4>
          {lineas.length > 0 && (
            <table className="mb-3 w-full text-sm">
              <thead>
                <tr className="text-left text-xs text-graphite-600">
                  <th className="pb-1">Cuenta</th>
                  <th className="pb-1">Detalle</th>
                  <th className="pb-1">Cant.</th>
                  <th className="pb-1">V. Unit.</th>
                  <th className="pb-1">IVA</th>
                  <th className="pb-1"></th>
                </tr>
              </thead>
              <tbody>
                {lineas.map((l, i) => (
                  <tr key={i} className="border-t border-black/[0.04]">
                    <td className="py-1">{l.cuentaContableNombre}</td>
                    <td className="py-1">{l.detalle}</td>
                    <td className="py-1 tabular-nums">{l.cantidad}</td>
                    <td className="py-1 tabular-nums">{formatoUsd(Number(l.valorUnitario))}</td>
                    <td className="py-1 tabular-nums">{(Number(l.porcentajeIva) * 100).toFixed(0)}%</td>
                    <td className="py-1">
                      <button
                        type="button"
                        onClick={() => setLineas((prev) => prev.filter((_, idx) => idx !== i))}
                        className="text-graphite-600 hover:text-red-700"
                      >
                        <Trash2 size={14} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          <div className="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-end">
            <div className="min-w-[240px] flex-1">
              <BuscadorCuentaContable onSeleccionar={setCuentaPendiente} />
              {cuentaPendiente && <p className="mt-1 text-xs text-petrol-700">Seleccionada: {cuentaPendiente.codigo} — {cuentaPendiente.nombre}</p>}
            </div>
            <input
              value={detalleLinea}
              onChange={(e) => setDetalleLinea(e.target.value)}
              placeholder="Detalle"
              className="w-40 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              type="number"
              step="0.01"
              min="0.01"
              value={cantidadLinea}
              onChange={(e) => setCantidadLinea(e.target.value)}
              placeholder="Cant."
              className="w-20 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              type="number"
              step="0.01"
              min="0.01"
              value={valorLinea}
              onChange={(e) => setValorLinea(e.target.value)}
              placeholder="V. unitario"
              className="w-28 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <select
              value={ivaLinea}
              onChange={(e) => setIvaLinea(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="0">IVA 0%</option>
              <option value="0.12">IVA 12%</option>
              <option value="0.14">IVA 14%</option>
            </select>
            <button
              type="button"
              onClick={agregarLinea}
              className="rounded-lg border border-black/[0.08] px-3 py-2 text-sm font-medium text-graphite-100 hover:bg-black/[0.02]"
            >
              Agregar línea
            </button>
          </div>

          {lineas.length > 0 && (
            <p className="mt-2 text-sm text-graphite-600">
              Subtotal {formatoUsd(subtotalLineas)} + IVA {formatoUsd(ivaLineas)} = <strong>Total {formatoUsd(totalLineas)}</strong>
              {Number(montoRetencion) > 0 && <> — Neto a pagar {formatoUsd(totalLineas - Number(montoRetencion))}</>}
            </p>
          )}
        </div>

        <div>
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Registrando…' : 'Registrar compra'}
          </button>
        </div>
        {registrar.isError && <p className="text-sm text-red-700">{mensajeErrorTesoreria(registrar.error, 'No se pudo registrar la compra.')}</p>}
      </form>
    </div>
  )
}

function PagarCompraForm({ compra, formasCancelacion, onClose }: { compra: Compra; formasCancelacion: FormaCancelacion[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [monto, setMonto] = useState(String(compra.saldo))
  const [codigoFormaCancelacion, setCodigoFormaCancelacion] = useState('')

  const pagar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/tesoreria/compras/${compra.id}/pagos`,
          { monto: Number(monto), codigoFormaCancelacion },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-compras'] })
      onClose()
    },
  })

  return (
    <div className="glass-card mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-graphite-100">Pagar compra {compra.numero} — {compra.proveedor}</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={16} />
        </button>
      </div>
      <form
        className="flex flex-col gap-3 sm:flex-row sm:items-end"
        onSubmit={(e) => {
          e.preventDefault()
          if (!monto || !codigoFormaCancelacion) return
          pagar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Monto (saldo {formatoUsd(compra.saldo)})</span>
          <input
            type="number"
            step="0.01"
            min="0.01"
            max={compra.saldo}
            value={monto}
            onChange={(e) => setMonto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Forma de cancelación</span>
          <select
            value={codigoFormaCancelacion}
            onChange={(e) => setCodigoFormaCancelacion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {formasCancelacion.map((f) => (
              <option key={f.codigo} value={f.codigo}>
                {f.nombre}
              </option>
            ))}
          </select>
        </label>
        <button
          type="submit"
          disabled={pagar.isPending}
          className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {pagar.isPending ? 'Pagando…' : 'Pagar'}
        </button>
      </form>
      {pagar.isError && <p className="mt-2 text-sm text-red-700">{mensajeErrorTesoreria(pagar.error, 'No se pudo registrar el pago.')}</p>}
    </div>
  )
}

function AnularCompraForm({ compra, onClose }: { compra: Compra; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [motivo, setMotivo] = useState('')

  const anular = useMutation({
    mutationFn: async () =>
      api.post(
        `/api/tesoreria/compras/${compra.id}/anular`,
        { motivo },
        { headers: { 'Idempotency-Key': crypto.randomUUID() } },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tesoreria-compras'] })
      onClose()
    },
  })

  return (
    <div className="glass-card mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-semibold text-graphite-100">Anular compra {compra.numero}</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={16} />
        </button>
      </div>
      <form
        className="flex flex-col gap-3 sm:flex-row sm:items-end"
        onSubmit={(e) => {
          e.preventDefault()
          if (!motivo) return
          anular.mutate()
        }}
      >
        <label className="flex flex-1 flex-col gap-1 text-sm">
          <span className="text-graphite-600">Motivo (obligatorio)</span>
          <input
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <button
          type="submit"
          disabled={anular.isPending}
          className="rounded-lg border border-red-700/30 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-700/5 disabled:opacity-60"
        >
          Confirmar anulación
        </button>
      </form>
      {anular.isError && <p className="mt-2 text-sm text-red-700">{mensajeErrorTesoreria(anular.error, 'No se pudo anular.')}</p>}
    </div>
  )
}

function SeccionCompras() {
  const [mostrarProveedorForm, setMostrarProveedorForm] = useState(false)
  const [mostrarCompraForm, setMostrarCompraForm] = useState(false)
  const [compraAPagar, setCompraAPagar] = useState<Compra | null>(null)
  const [compraAAnular, setCompraAAnular] = useState<Compra | null>(null)

  const { data: proveedores, refetch: refetchProveedores } = useQuery<Proveedor[]>({
    queryKey: ['tesoreria-compras-proveedores'],
    queryFn: async () => (await api.get('/api/tesoreria/compras/proveedores')).data,
  })

  const { data: compras, isLoading } = useQuery<Compra[]>({
    queryKey: ['tesoreria-compras'],
    queryFn: async () => (await api.get('/api/tesoreria/compras')).data,
  })

  const { data: formasCancelacion } = useQuery<FormaCancelacion[]>({
    queryKey: ['tesoreria-formas-cancelacion'],
    queryFn: async () => (await api.get('/api/tesoreria/cuentas-por-pagar/formas-cancelacion')).data,
  })

  const formasActivas = (formasCancelacion ?? []).filter((f) => f.activo)

  return (
    <div>
      <p className="mb-4 text-sm text-graphite-600">
        Registro real de compras/facturas de proveedores, con los campos de identificación tributaria SRI
        (sustento, comprobante, establecimiento-puntoEmisión-secuencial-autorización) y el pasivo real con el
        proveedor.
      </p>

      <div className="mb-4 flex flex-wrap justify-end gap-2">
        {!mostrarProveedorForm && (
          <button
            type="button"
            onClick={() => setMostrarProveedorForm(true)}
            className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] px-3 py-2 text-sm font-medium text-graphite-100 hover:bg-black/[0.02]"
          >
            <Plus size={16} /> Nuevo proveedor
          </button>
        )}
        {!mostrarCompraForm && (
          <button
            type="button"
            onClick={() => setMostrarCompraForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <ShoppingCart size={16} /> Registrar compra
          </button>
        )}
      </div>

      {mostrarProveedorForm && (
        <NuevoProveedorForm
          onCreado={() => {
            refetchProveedores()
            setMostrarProveedorForm(false)
          }}
          onClose={() => setMostrarProveedorForm(false)}
        />
      )}

      {mostrarCompraForm && (
        <RegistrarCompraForm proveedores={proveedores ?? []} onClose={() => setMostrarCompraForm(false)} />
      )}

      {compraAPagar && (
        <PagarCompraForm compra={compraAPagar} formasCancelacion={formasActivas} onClose={() => setCompraAPagar(null)} />
      )}

      {compraAAnular && <AnularCompraForm compra={compraAAnular} onClose={() => setCompraAAnular(null)} />}

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Proveedor</Th>
            <Th>Comprobante</Th>
            <Th>Fecha emisión</Th>
            <Th>Total</Th>
            <Th>Saldo</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (compras?.length ?? 0) === 0 && <EmptyState>Todavía no hay compras registradas</EmptyState>}
          {compras?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{c.numero}</Td>
              <Td>{c.proveedor}</Td>
              <Td>{c.tipoComprobante} {c.establecimiento}-{c.puntoEmision}-{c.secuencial}</Td>
              <Td>{c.fechaEmision}</Td>
              <Td className="tabular-nums">{formatoUsd(c.total)}</Td>
              <Td className="tabular-nums">{formatoUsd(c.saldo)}</Td>
              <Td>
                <Badge variant={c.estado === 'Procesada' ? 'exito' : c.estado === 'Anulada' ? 'peligro' : 'neutral'}>
                  {c.estado}
                </Badge>
              </Td>
              <Td>
                {c.estado === 'Procesada' && (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => setCompraAPagar(c)}
                      className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                    >
                      <HandCoins size={13} /> Pagar
                    </button>
                    {c.saldo === c.total - c.montoRetencion && (
                      <button
                        type="button"
                        onClick={() => setCompraAAnular(c)}
                        className="flex items-center gap-1 text-xs font-medium text-red-700 hover:underline"
                      >
                        <Ban size={13} /> Anular
                      </button>
                    )}
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
