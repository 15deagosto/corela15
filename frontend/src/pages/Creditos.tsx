import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Landmark, Plus, X, Gauge, AlertTriangle, ShieldAlert, Zap, CalendarClock, FileLock2, Flame } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { BotonesExportar } from '../components/BotonesExportar'
import { ModalPortal } from '../components/ModalPortal'
import type { ColumnaExportable } from '../lib/exportar'
import { api } from '../lib/api'

interface Producto {
  id: number
  codigo: string
  nombre: string
  montoMinimo: number
  montoMaximo: number
  tasaAnual: number
}

interface Solicitud {
  id: string
  numero: string
  idCliente: string
  socio: string
  producto: string
  montoSolicitado: number
  cuotas: number
  estado: string
  fechaSolicitud: string
  montoAprobado: number | null
  aprobadoPor: string | null
  comentarioAprobacion: string | null
  etapaActual: string | null
}

interface Prestamo {
  id: string
  numero: string
  producto: string
  deudaInicial: number
  saldo: number
  tasa: number
  estado: string
  fechaAdjudicacion: string
  debitoSpi: boolean
  codigoUsuarioAsesor: string | null
  nombreConvenio: string | null
}

interface PagoCuotaResultado {
  numeroCuota: number
  montoCapital: number
  montoInteres: number
  saldoResultante: number
  prestamoCancelado: boolean
  idComprobanteContable: string
  diasMoraCuota: number
  montoInteresMora: number
}

interface RubroManualDisponible {
  id: number
  nombre: string
  esCuentaPorCobrar: boolean
}

interface RubroManualCargado {
  idPrestamoRubro: string
  nombreRubro: string
  monto: number
  fecha: string
  estado: string
  idCuentaPorCobrar: string | null
  idComprobante: string | null
}

interface AutoDebitoSpiDetalle {
  numeroPrestamo: string
  numeroCuenta: string | null
  numeroCuota: number | null
  debitado: boolean
  motivo: string
  monto: number
}

interface AutoDebitoSpiResultado {
  fecha: string
  debitados: number
  omitidos: number
  totalDebitado: number
  idComprobanteContable: string | null
  detalles: AutoDebitoSpiDetalle[]
}

interface Socio {
  id: string
  numero: string
  nombre: string
}

interface ScoreCrediticio {
  id: string
  idCliente: string
  fecha: string
  puntaje: number
  categoria: string
  ratioIngresoEgreso: number | null
  ratioEndeudamiento: number | null
  tienePrestamoCastigado: boolean
  prestamosCancelados: number
  esPep: boolean
}

interface Deposito {
  id: string
  codigo: string
  socio: string
  monto: number
  tasa: number
  plazoDias: number
  fechaCreacion: string
  fechaVencimiento: string
  estado: string
}

interface DepositoRenovado {
  idDepositoDestino: string
  codigoDestino: string
  montoNuevo: number
  tasaAplicada: number
  fechaVencimiento: string
  interesPagado: number
  idComprobanteContable: string | null
}

interface DepositoRenovacionHistorial {
  codigoOrigen: string
  codigoDestino: string
  valor: number
  valorIncremento: number
  tasaAplicada: number
  fechaRenovacion: string
}

interface DepositoCancelado {
  valorDevuelto: number
  interesPagado: number
  idComprobanteContable: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function categoriaVariant(categoria: string) {
  if (categoria === 'RiesgoBajo') return 'exito' as const
  if (categoria === 'RiesgoAlto') return 'peligro' as const
  return 'alerta' as const
}

function categoriaLabel(categoria: string) {
  if (categoria === 'RiesgoBajo') return 'Riesgo bajo'
  if (categoria === 'RiesgoAlto') return 'Riesgo alto'
  return 'Riesgo medio'
}

function ScoreCrediticioPanel({ idCliente }: { idCliente: string }) {
  const queryClient = useQueryClient()

  const { data: historial } = useQuery<ScoreCrediticio[]>({
    queryKey: ['creditos-score-historial', idCliente],
    queryFn: async () => (await api.get(`/api/creditos/clientes/${idCliente}/score/historial`)).data,
    enabled: !!idCliente,
  })

  const calcular = useMutation({
    mutationFn: async () => (await api.post(`/api/creditos/clientes/${idCliente}/score`)).data as ScoreCrediticio,
    onSuccess: (nuevoScore) => {
      queryClient.setQueryData<ScoreCrediticio[]>(['creditos-score-historial', idCliente], (prev) => [
        nuevoScore,
        ...(prev ?? []),
      ])
    },
  })

  useEffect(() => {
    if (idCliente && (historial?.length ?? 0) === 0 && !calcular.isPending && !calcular.isSuccess) {
      calcular.mutate()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [idCliente, historial])

  if (!idCliente) return null

  const score = calcular.data ?? historial?.[0]

  return (
    <div className="sm:col-span-2 rounded-lg border border-black/[0.08] bg-graphite-950/40 p-3">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-graphite-600">
          <Gauge size={14} /> Calificación crediticia del socio
        </div>
        <button
          type="button"
          onClick={() => calcular.mutate()}
          disabled={calcular.isPending}
          className="text-xs font-medium text-gold-400 hover:underline disabled:opacity-50"
        >
          {calcular.isPending ? 'Calculando…' : 'Recalcular'}
        </button>
      </div>

      {calcular.isPending && !score && <p className="mt-2 text-sm text-graphite-600">Calculando score…</p>}

      {score && (
        <div className="mt-2 flex flex-wrap items-center gap-3">
          <span className="text-2xl font-semibold tabular-nums text-graphite-100">{score.puntaje}</span>
          <Badge variant={categoriaVariant(score.categoria)}>{categoriaLabel(score.categoria)}</Badge>
          {score.esPep && (
            <span className="inline-flex items-center gap-1 text-xs font-medium text-gold-300">
              <ShieldAlert size={13} /> Persona expuesta políticamente (PEP)
            </span>
          )}
          {score.tienePrestamoCastigado && (
            <span className="inline-flex items-center gap-1 text-xs font-medium text-red-700">
              <AlertTriangle size={13} /> Tiene préstamo castigado en el historial
            </span>
          )}
          {score.ratioIngresoEgreso !== null && (
            <span className="text-xs text-graphite-600">
              Ingreso neto: {(score.ratioIngresoEgreso * 100).toFixed(0)}%
            </span>
          )}
          {score.ratioEndeudamiento !== null && (
            <span className="text-xs text-graphite-600">
              Endeudamiento: {(score.ratioEndeudamiento * 100).toFixed(0)}%
            </span>
          )}
        </div>
      )}
    </div>
  )
}

function SolicitarForm({ productos, onClose }: { productos: Producto[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [idCliente, setIdCliente] = useState('')
  const [idTipoPrestamo, setIdTipoPrestamo] = useState(productos[0]?.id ?? 0)
  const [montoSolicitado, setMontoSolicitado] = useState('1000')
  const [cuotas, setCuotas] = useState('12')
  const [codigoTipoConvenio, setCodigoTipoConvenio] = useState('')

  const { data: socios } = useQuery<Socio[]>({
    queryKey: ['socios', ''],
    queryFn: async () => (await api.get('/api/socios')).data,
  })

  const { data: convenios } = useQuery<{ codigo: string; nombre: string; activo: boolean }[]>({
    queryKey: ['config-tipos-convenio'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-convenio')).data,
  })

  const solicitar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/creditos/solicitudes', {
          idCliente,
          idTipoPrestamo,
          idAgencia: 1,
          montoSolicitado: Number(montoSolicitado) || 0,
          cuotas: Number(cuotas) || 0,
          codigoTipoConvenio: codigoTipoConvenio || null,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-solicitudes'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nueva solicitud de crédito</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idCliente) return
          solicitar.mutate()
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

        <ScoreCrediticioPanel idCliente={idCliente} />

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Producto</span>
          <select
            value={idTipoPrestamo}
            onChange={(e) => setIdTipoPrestamo(Number(e.target.value))}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            {productos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre} ({(p.tasaAnual * 100).toFixed(2)}% anual)
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Monto solicitado (USD)</span>
          <input
            type="number"
            min="1"
            step="0.01"
            value={montoSolicitado}
            onChange={(e) => setMontoSolicitado(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Cuotas (meses)</span>
          <input
            type="number"
            min="1"
            max="120"
            value={cuotas}
            onChange={(e) => setCuotas(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Convenio (opcional)</span>
          <select
            value={codigoTipoConvenio}
            onChange={(e) => setCodigoTipoConvenio(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Sin convenio</option>
            {convenios?.filter((c) => c.activo).map((c) => (
              <option key={c.codigo} value={c.codigo}>{c.nombre}</option>
            ))}
          </select>
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={solicitar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {solicitar.isPending ? 'Enviando…' : 'Registrar solicitud'}
          </button>
        </div>

        {solicitar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(solicitar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar la solicitud.'}
          </p>
        )}
      </form>
    </div>
  )
}

function AbrirDpfForm({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient()
  const [idCliente, setIdCliente] = useState('')
  const [monto, setMonto] = useState('500')
  const [plazoDias, setPlazoDias] = useState('180')

  const { data: socios } = useQuery<Socio[]>({
    queryKey: ['socios', ''],
    queryFn: async () => (await api.get('/api/socios')).data,
  })

  const abrir = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          '/api/plazofijo/depositos',
          { idCliente, idAgencia: 1, monto: Number(monto) || 0, plazoDias: Number(plazoDias) || 0, esPersonaJuridica: false },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plazofijo-depositos'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Abrir depósito a plazo fijo</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idCliente) return
          abrir.mutate()
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
          <span className="text-graphite-600">Monto (USD)</span>
          <input
            type="number"
            min="50"
            step="0.01"
            value={monto}
            onChange={(e) => setMonto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Plazo (días)</span>
          <input
            type="number"
            min="30"
            max="720"
            value={plazoDias}
            onChange={(e) => setPlazoDias(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2 sm:col-span-3">
          <button
            type="submit"
            disabled={abrir.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {abrir.isPending ? 'Abriendo…' : 'Abrir DPF'}
          </button>
        </div>

        {abrir.isError && (
          <p className="sm:col-span-3 text-sm text-red-700">
            {(abrir.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo abrir el depósito.'}
          </p>
        )}
      </form>
    </div>
  )
}

function RenovarDpfModal({ deposito, onClose }: { deposito: Deposito; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [plazoDias, setPlazoDias] = useState(String(deposito.plazoDias))
  const [incrementoCapital, setIncrementoCapital] = useState('0')
  const [esPersonaJuridica, setEsPersonaJuridica] = useState(false)

  const renovar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/plazofijo/depositos/${deposito.id}/renovar`,
          { plazoDias: Number(plazoDias) || 0, incrementoCapital: Number(incrementoCapital) || 0, esPersonaJuridica },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data as DepositoRenovado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plazofijo-depositos'] })
      queryClient.invalidateQueries({ queryKey: ['plazofijo-renovaciones'] })
    },
  })

  const mensajeError = (renovar.error as { response?: { data?: { detail?: string } } } | undefined)?.response?.data
    ?.detail

  return (
    <div className="fixed inset-0 z-20 flex items-center justify-center bg-black/30 p-4">
      <div className="glass-strong animate-zoom-in w-full max-w-sm rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h3 className="font-medium text-graphite-100">Renovar DPF {deposito.codigo}</h3>
            <p className="text-xs text-graphite-600">
              Capital actual: {formatoUsd(deposito.monto)} — tasa original {(deposito.tasa * 100).toFixed(2)}%
            </p>
          </div>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        {renovar.isSuccess ? (
          <div className="flex flex-col gap-3">
            <p className="rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
              Renovado como <strong>{renovar.data.codigoDestino}</strong> por {formatoUsd(renovar.data.montoNuevo)} a la
              tasa vigente <strong>{(renovar.data.tasaAplicada * 100).toFixed(2)}%</strong> (venc.{' '}
              {renovar.data.fechaVencimiento})
              {renovar.data.interesPagado > 0 && (
                <>
                  {' '}
                  — se liquidó <strong>{formatoUsd(renovar.data.interesPagado)}</strong> de interés devengado del
                  depósito original antes de re-papelarlo
                </>
              )}
              .
            </p>
            <button
              type="button"
              onClick={onClose}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white"
            >
              Cerrar
            </button>
          </div>
        ) : (
          <form
            className="flex flex-col gap-4"
            onSubmit={(e) => {
              e.preventDefault()
              renovar.mutate()
            }}
          >
            <p className="text-xs text-graphite-600">
              La tasa se toma del tablero vigente en este momento, no la tasa original del depósito.
            </p>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Nuevo plazo (días)</span>
              <input
                type="number"
                min="30"
                max="720"
                value={plazoDias}
                onChange={(e) => setPlazoDias(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Incremento de capital (USD, opcional)</span>
              <input
                type="number"
                min="0"
                step="0.01"
                value={incrementoCapital}
                onChange={(e) => setIncrementoCapital(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>

            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input
                type="checkbox"
                checked={esPersonaJuridica}
                onChange={(e) => setEsPersonaJuridica(e.target.checked)}
              />
              El titular es persona jurídica
            </label>

            <button
              type="submit"
              disabled={renovar.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {renovar.isPending ? 'Renovando…' : 'Confirmar renovación'}
            </button>

            {renovar.isError && <p className="text-sm text-red-700">{mensajeError ?? 'No se pudo renovar el depósito.'}</p>}
          </form>
        )}
      </div>
    </div>
  )
}

function SeccionRenovacionesDpf() {
  const { data: renovaciones } = useQuery<DepositoRenovacionHistorial[]>({
    queryKey: ['plazofijo-renovaciones'],
    queryFn: async () => (await api.get('/api/plazofijo/renovaciones')).data,
  })

  return (
    <div className="mt-8">
      <h2 className="mb-2 text-sm font-semibold uppercase tracking-wide text-graphite-600">Renovaciones de DPF</h2>
      <TableContainer>
        <thead>
          <tr>
            <Th>Origen</Th>
            <Th>Destino</Th>
            <Th>Valor renovado</Th>
            <Th>Incremento</Th>
            <Th>Tasa aplicada</Th>
            <Th>Fecha</Th>
          </tr>
        </thead>
        <tbody>
          {(renovaciones?.length ?? 0) === 0 && <EmptyState>Todavía no se ha renovado ningún DPF</EmptyState>}
          {renovaciones?.map((r, i) => (
            <tr key={i} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{r.codigoOrigen}</Td>
              <Td>{r.codigoDestino}</Td>
              <Td className="tabular-nums">{formatoUsd(r.valor)}</Td>
              <Td className="tabular-nums">{r.valorIncremento > 0 ? formatoUsd(r.valorIncremento) : '—'}</Td>
              <Td>{(r.tasaAplicada * 100).toFixed(2)}%</Td>
              <Td>{r.fechaRenovacion}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

function estadoVariant(estado: string) {
  if (estado === 'Desembolsada' || estado === 'Vigente' || estado === 'Aprobada') return 'exito' as const
  if (estado === 'Rechazada' || estado === 'Castigado') return 'peligro' as const
  return 'alerta' as const
}

function SeccionAutoDebitoSpi() {
  const queryClient = useQueryClient()

  const { data: historial } = useQuery<AutoDebitoSpiDetalle[]>({
    queryKey: ['creditos-auto-debito-spi-historial'],
    queryFn: async () => (await api.get('/api/creditos/auto-debito-spi/historial')).data,
  })

  const ejecutar = useMutation({
    mutationFn: async () => (await api.post('/api/creditos/auto-debito-spi/ejecutar')).data as AutoDebitoSpiResultado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-auto-debito-spi-historial'] })
      queryClient.invalidateQueries({ queryKey: ['creditos-prestamos'] })
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
    },
  })

  return (
    <div className="mt-8">
      <div className="mb-2 flex items-center justify-between">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-graphite-600">Auto-débito de cuota por SPI</h2>
        <button
          type="button"
          onClick={() => ejecutar.mutate()}
          disabled={ejecutar.isPending}
          className="btn-hover flex items-center gap-1.5 rounded-lg border border-black/[0.08] bg-white px-3 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-60"
        >
          <Zap size={14} />
          {ejecutar.isPending ? 'Procesando…' : 'Ejecutar auto-débito SPI'}
        </button>
      </div>

      <p className="mb-3 text-xs text-graphite-600">
        Debita la próxima cuota de cada préstamo con débito SPI activo, cruzando las tres configuraciones (préstamo,
        producto de la cuenta y saldo disponible) como una sola fuente de verdad antes de mover dinero. Corre
        automáticamente todos los días a las 06:00 UTC (job programático) — este botón es solo para corridas
        manuales fuera de ese horario, y correrlo de nuevo el mismo día no duplica nada.
      </p>

      {ejecutar.isSuccess && (
        <p className="mb-3 rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
          Corrida del {ejecutar.data.fecha}: {ejecutar.data.debitados} cuota(s) debitada(s) (
          {formatoUsd(ejecutar.data.totalDebitado)}), {ejecutar.data.omitidos} omitida(s).
        </p>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Préstamo</Th>
            <Th>Cuenta</Th>
            <Th>Cuota</Th>
            <Th>Resultado</Th>
            <Th>Motivo</Th>
            <Th>Monto</Th>
          </tr>
        </thead>
        <tbody>
          {(historial?.length ?? 0) === 0 && <EmptyState>Todavía no se ha ejecutado el auto-débito SPI</EmptyState>}
          {historial?.map((d, i) => (
            <tr key={i} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{d.numeroPrestamo}</Td>
              <Td>{d.numeroCuenta ?? '—'}</Td>
              <Td>{d.numeroCuota ?? '—'}</Td>
              <Td>
                <Badge variant={d.debitado ? 'exito' : 'alerta'}>{d.debitado ? 'Debitado' : 'Omitido'}</Badge>
              </Td>
              <Td className="text-xs text-graphite-600">{d.motivo}</Td>
              <Td className="tabular-nums">{formatoUsd(d.monto)}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface Garante {
  id: string
  idClienteGarante: string
  garante: string
  detalle: string | null
  codigoEstadoGarantia: string | null
  estadoGarantia: string | null
}

function GarantesPanel({ idSolicitud, idTitular }: { idSolicitud: string; idTitular: string }) {
  const queryClient = useQueryClient()
  const [idClienteGarante, setIdClienteGarante] = useState('')
  const [detalle, setDetalle] = useState('')

  const { data: garantes } = useQuery<Garante[]>({
    queryKey: ['creditos-garantes-solicitud', idSolicitud],
    queryFn: async () => (await api.get(`/api/creditos/solicitudes/${idSolicitud}/garantes`)).data,
  })

  const { data: socios } = useQuery<Socio[]>({
    queryKey: ['socios', ''],
    queryFn: async () => (await api.get('/api/socios')).data,
  })

  const agregar = useMutation({
    mutationFn: async () =>
      (
        await api.post(`/api/creditos/solicitudes/${idSolicitud}/garantes`, {
          idClienteGarante,
          detalle: detalle || null,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-garantes-solicitud', idSolicitud] })
      setIdClienteGarante('')
      setDetalle('')
    },
  })

  const quitar = useMutation({
    mutationFn: async (idGarantia: string) => api.delete(`/api/creditos/garantes/${idGarantia}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-garantes-solicitud', idSolicitud] })
    },
  })

  const opcionesSocios = socios?.filter((s) => s.id !== idTitular)

  return (
    <div className="rounded-lg border border-black/[0.08] bg-graphite-950/40 p-3">
      <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Garantes / codeudores</p>

      {(garantes?.length ?? 0) === 0 && <p className="mb-2 text-xs text-graphite-600">Sin garantes registrados.</p>}
      {garantes && garantes.length > 0 && (
        <ul className="mb-2 flex flex-col gap-1">
          {garantes.map((g) => (
            <li key={g.id} className="flex items-center justify-between text-sm">
              <span>
                {g.garante} {g.detalle && <span className="text-xs text-graphite-600">— {g.detalle}</span>}
              </span>
              <button
                type="button"
                disabled={quitar.isPending}
                onClick={() => quitar.mutate(g.id)}
                className="text-xs text-graphite-600 hover:text-red-700 disabled:opacity-50"
              >
                Quitar
              </button>
            </li>
          ))}
        </ul>
      )}

      <div className="flex flex-col gap-2 sm:flex-row">
        <select
          value={idClienteGarante}
          onChange={(e) => setIdClienteGarante(e.target.value)}
          className="flex-1 rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
        >
          <option value="">Agregar garante…</option>
          {opcionesSocios?.map((s) => (
            <option key={s.id} value={s.id}>
              {s.numero} — {s.nombre}
            </option>
          ))}
        </select>
        <button
          type="button"
          disabled={!idClienteGarante || agregar.isPending}
          onClick={() => agregar.mutate()}
          className="rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white disabled:opacity-50"
        >
          Agregar
        </button>
      </div>
      {agregar.isError && (
        <p className="mt-1 text-xs text-red-700">
          {(agregar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
            'No se pudo agregar el garante.'}
        </p>
      )}
    </div>
  )
}

interface SolicitudEtapaHistItem {
  etapaAnterior: string | null
  etapa: string
  comentario: string | null
  registradoPor: string
  fecha: string
  esRetorno: boolean
}

// Bitácora real del motor de aprobaciones (ver CLAUDE.md "Motor de
// aprobaciones genérico") — solo aparece si ya hubo alguna decisión
// registrada sobre esta solicitud, no es requisito para poder aprobarla.
function EtapasHistorialPanel({ idSolicitud }: { idSolicitud: string }) {
  const { data } = useQuery<SolicitudEtapaHistItem[]>({
    queryKey: ['solicitud-etapas-historial', idSolicitud],
    queryFn: async () => (await api.get(`/api/creditos/solicitudes/${idSolicitud}/etapas-historial`)).data,
  })

  if (!data || data.length === 0) return null

  return (
    <div className="mb-4 rounded-lg border border-black/[0.08] bg-white p-2">
      <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-graphite-600">Historial de decisiones</p>
      <ul className="flex flex-col gap-1">
        {data.map((h, i) => (
          <li key={i} className="text-xs text-graphite-100">
            <span className={h.esRetorno ? 'text-red-700' : 'text-petrol-700'}>{h.etapa}</span>
            {' — '}
            {h.registradoPor} ({new Date(h.fecha).toLocaleString('es-EC')})
            {h.comentario && <span className="text-graphite-600"> — {h.comentario}</span>}
          </li>
        ))}
      </ul>
    </div>
  )
}

function ComiteCreditoModal({ solicitud, onClose }: { solicitud: Solicitud; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [montoAprobado, setMontoAprobado] = useState(String(solicitud.montoSolicitado))
  const [comentario, setComentario] = useState('')
  const [modo, setModo] = useState<'aprobar' | 'rechazar'>('aprobar')

  const decidir = useMutation({
    mutationFn: async () => {
      if (modo === 'aprobar') {
        return (
          await api.post(`/api/creditos/solicitudes/${solicitud.id}/aprobar`, {
            montoAprobado: Number(montoAprobado) || 0,
            comentario: comentario || null,
          })
        ).data
      }
      return (
        await api.post(`/api/creditos/solicitudes/${solicitud.id}/rechazar`, { comentario })
      ).data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-solicitudes'] })
      onClose()
    },
  })

  const mensajeError = (decidir.error as { response?: { data?: { detail?: string } } } | undefined)?.response?.data
    ?.detail

  return (
    <div className="fixed inset-0 z-20 flex items-center justify-center bg-black/30 p-4">
      <div className="glass-strong animate-zoom-in w-full max-w-sm rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h3 className="font-medium text-graphite-100">Comité de Crédito — {solicitud.numero}</h3>
            <p className="text-xs text-graphite-600">
              {solicitud.socio} — {solicitud.producto} — solicitado {formatoUsd(solicitud.montoSolicitado)}
            </p>
          </div>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <div className="mb-4">
          <GarantesPanel idSolicitud={solicitud.id} idTitular={solicitud.idCliente} />
        </div>

        <EtapasHistorialPanel idSolicitud={solicitud.id} />

        <div className="mb-4 flex gap-1 rounded-lg bg-graphite-950/40 p-1">
          <button
            type="button"
            onClick={() => setModo('aprobar')}
            className={`flex-1 rounded-md px-3 py-1.5 text-sm font-medium transition ${modo === 'aprobar' ? 'bg-gold-500 text-white' : 'text-graphite-600'}`}
          >
            Aprobar
          </button>
          <button
            type="button"
            onClick={() => setModo('rechazar')}
            className={`flex-1 rounded-md px-3 py-1.5 text-sm font-medium transition ${modo === 'rechazar' ? 'bg-red-700 text-white' : 'text-graphite-600'}`}
          >
            Rechazar
          </button>
        </div>

        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            decidir.mutate()
          }}
        >
          {modo === 'aprobar' && (
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Monto aprobado (USD)</span>
              <input
                type="number"
                min="1"
                step="0.01"
                value={montoAprobado}
                onChange={(e) => setMontoAprobado(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          )}

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Comentario {modo === 'rechazar' ? '(motivo, obligatorio)' : '(opcional)'}</span>
            <textarea
              required={modo === 'rechazar'}
              value={comentario}
              onChange={(e) => setComentario(e.target.value)}
              rows={2}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <button
            type="submit"
            disabled={decidir.isPending}
            className={`btn-hover rounded-lg px-4 py-2 text-sm font-medium text-white disabled:opacity-60 ${modo === 'aprobar' ? 'bg-gold-500' : 'bg-red-700'}`}
          >
            {decidir.isPending ? 'Guardando…' : modo === 'aprobar' ? 'Confirmar aprobación' : 'Confirmar rechazo'}
          </button>

          {decidir.isError && <p className="text-sm text-red-700">{mensajeError ?? 'No se pudo registrar la decisión.'}</p>}
        </form>
      </div>
    </div>
  )
}

function SeccionSolicitudes({ productos }: { productos: Producto[] | undefined }) {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [solicitudEnComite, setSolicitudEnComite] = useState<Solicitud | null>(null)
  const queryClient = useQueryClient()

  const { data: solicitudes, isLoading: cargandoSolicitudes } = useQuery<Solicitud[]>({
    queryKey: ['creditos-solicitudes'],
    queryFn: async () => (await api.get('/api/creditos/solicitudes')).data,
  })

  const desembolsar = useMutation({
    mutationFn: async (idSolicitud: string) =>
      (
        await api.post(`/api/creditos/solicitudes/${idSolicitud}/desembolsar`, undefined, {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-solicitudes'] })
      queryClient.invalidateQueries({ queryKey: ['creditos-prestamos'] })
    },
  })

  return (
    <div>
      <div className="mb-2 flex items-center justify-between">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-graphite-600">Solicitudes de crédito</h2>
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <Plus size={16} /> Nueva solicitud
          </button>
        )}
      </div>

      {mostrarForm && productos && <SolicitarForm productos={productos} onClose={() => setMostrarForm(false)} />}
      {solicitudEnComite && (
        <ModalPortal>
          <ComiteCreditoModal solicitud={solicitudEnComite} onClose={() => setSolicitudEnComite(null)} />
        </ModalPortal>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Socio</Th>
            <Th>Producto</Th>
            <Th>Monto</Th>
            <Th>Cuotas</Th>
            <Th>Estado</Th>
            <Th>Comité</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {cargandoSolicitudes && <EmptyState>Cargando…</EmptyState>}
          {!cargandoSolicitudes && (solicitudes?.length ?? 0) === 0 && (
            <EmptyState>Todavía no hay solicitudes de crédito</EmptyState>
          )}
          {solicitudes?.map((s) => (
            <tr key={s.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{s.numero}</Td>
              <Td>{s.socio}</Td>
              <Td>{s.producto}</Td>
              <Td className="tabular-nums">{formatoUsd(s.montoSolicitado)}</Td>
              <Td>{s.cuotas}</Td>
              <Td>
                <Badge variant={estadoVariant(s.estado)}>{s.estado}</Badge>
              </Td>
              <Td className="text-xs text-graphite-600">
                {s.montoAprobado !== null ? (
                  <span title={s.comentarioAprobacion ?? undefined}>
                    {formatoUsd(s.montoAprobado)} por {s.aprobadoPor ?? '—'}
                  </span>
                ) : (
                  '—'
                )}
                {s.etapaActual && (
                  <div className="mt-0.5">
                    <Badge variant={s.etapaActual === 'NEGADA' ? 'peligro' : 'neutral'}>{s.etapaActual}</Badge>
                  </div>
                )}
              </Td>
              <Td>
                {s.estado === 'EnAnalisis' && (
                  <button
                    type="button"
                    onClick={() => setSolicitudEnComite(s)}
                    className="text-sm font-medium text-gold-400 hover:underline"
                  >
                    Enviar a Comité
                  </button>
                )}
                {s.estado === 'Aprobada' && (
                  <button
                    type="button"
                    disabled={desembolsar.isPending}
                    onClick={() => desembolsar.mutate(s.id)}
                    className="text-sm font-medium text-gold-400 hover:underline disabled:opacity-50"
                  >
                    Desembolsar
                  </button>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
      {desembolsar.isError && (
        <p className="mt-2 text-sm text-red-700">
          {(desembolsar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
            'No se pudo desembolsar el préstamo.'}
        </p>
      )}
    </div>
  )
}

function RubrosManualesPanel({ prestamo }: { prestamo: Prestamo }) {
  const queryClient = useQueryClient()
  const [idRubro, setIdRubro] = useState('')
  const [monto, setMonto] = useState('')
  const [detalle, setDetalle] = useState('')

  const { data: disponibles } = useQuery<RubroManualDisponible[]>({
    queryKey: ['creditos-rubros-manuales-disponibles'],
    queryFn: async () => (await api.get('/api/creditos/rubros-manuales-disponibles')).data,
  })

  const { data: cargados, isLoading } = useQuery<RubroManualCargado[]>({
    queryKey: ['creditos-rubros-manuales', prestamo.id],
    queryFn: async () => (await api.get(`/api/creditos/prestamos/${prestamo.id}/rubros-manuales`)).data,
  })

  const cargar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/creditos/prestamos/${prestamo.id}/rubros-manuales`,
          { idRubro: Number(idRubro), monto: Number(monto), detalle },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data as RubroManualCargado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-rubros-manuales', prestamo.id] })
      setIdRubro('')
      setMonto('')
      setDetalle('')
    },
  })

  return (
    <div className="glass-card animate-zoom-in mt-2 rounded-xl p-4">
      <h4 className="mb-1 text-sm font-medium text-graphite-100">
        Rubros manuales — préstamo {prestamo.numero}
      </h4>
      <p className="mb-3 text-xs text-graphite-500">
        Cargos adicionales fuera de la cuota normal (gastos judiciales, notificaciones, certificados...). Los rubros
        marcados como "genera CxC" crean automáticamente una cuenta por cobrar real en Tesorería — al abonarla ahí,
        este cargo se marca cobrado acá también.
      </p>

      <form
        className="mb-4 grid grid-cols-1 gap-2 sm:grid-cols-4 sm:items-end"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idRubro || !monto) return
          cargar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-xs sm:col-span-2">
          <span className="text-graphite-600">Rubro</span>
          <select
            required
            value={idRubro}
            onChange={(e) => setIdRubro(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {disponibles?.map((r) => (
              <option key={r.id} value={r.id}>
                {r.nombre}
                {r.esCuentaPorCobrar ? ' (genera CxC)' : ''}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-xs">
          <span className="text-graphite-600">Monto</span>
          <input
            required
            type="number"
            step="0.01"
            min="0.01"
            value={monto}
            onChange={(e) => setMonto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-xs">
          <span className="text-graphite-600">Detalle</span>
          <input
            type="text"
            value={detalle}
            onChange={(e) => setDetalle(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <button
          type="submit"
          disabled={cargar.isPending}
          className="btn-hover rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white disabled:opacity-60 sm:col-span-4 sm:w-fit"
        >
          {cargar.isPending ? 'Cargando…' : 'Cargar rubro'}
        </button>
        {cargar.isError && (
          <p className="text-xs text-red-700 sm:col-span-4">
            {(cargar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo cargar el rubro.'}
          </p>
        )}
      </form>

      <TableContainer>
        <thead>
          <tr>
            <Th>Rubro</Th>
            <Th>Monto</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th>CxC vinculada</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (cargados?.length ?? 0) === 0 && <EmptyState>Sin rubros manuales cargados</EmptyState>}
          {cargados?.map((r) => (
            <tr key={r.idPrestamoRubro} className="border-b border-black/[0.04] last:border-0">
              <Td className="font-medium">{r.nombreRubro}</Td>
              <Td className="tabular-nums">{formatoUsd(r.monto)}</Td>
              <Td>{r.fecha}</Td>
              <Td>
                <Badge variant={r.estado === 'C' ? 'neutral' : 'exito'}>{r.estado === 'C' ? 'Cobrado' : 'Pendiente'}</Badge>
              </Td>
              <Td>{r.idCuentaPorCobrar ? 'Sí — ver en Tesorería' : '—'}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface DiferimientoCuota {
  id: string
  diasDiferidos: number
  comentario: string
  fecha: string
  registradoPor: string
}

interface PagareCustodiaMovimiento {
  codigoEstado: string
  ubicacion: string
  esRecepcion: boolean
  fecha: string
  registradoPor: string
}

interface PagareCustodiaInfo {
  id: string
  codigoEstado: string
  estado: string
  ubicacion: string
  fechaActualizacion: string
  movimientos: PagareCustodiaMovimiento[]
}

function mensajeErrorCreditos(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
}

function TabDiferimiento({ prestamo }: { prestamo: Prestamo }) {
  const queryClient = useQueryClient()
  const [diasDiferidos, setDiasDiferidos] = useState('')
  const [comentario, setComentario] = useState('')

  const { data: diferimientos, isLoading } = useQuery<DiferimientoCuota[]>({
    queryKey: ['creditos-diferimientos', prestamo.id],
    queryFn: async () => (await api.get(`/api/creditos/prestamos/${prestamo.id}/diferimientos`)).data,
  })

  const diferir = useMutation({
    mutationFn: async () =>
      (
        await api.post(`/api/creditos/prestamos/${prestamo.id}/diferimientos`, {
          diasDiferidos: Number(diasDiferidos),
          comentario,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-diferimientos', prestamo.id] })
      setDiasDiferidos('')
      setComentario('')
    },
  })

  return (
    <div className="flex flex-col gap-6">
      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-2 text-sm font-semibold text-graphite-100">Nuevo diferimiento (período de gracia)</h3>
        <p className="mb-3 text-sm text-graphite-600">
          Desplaza la fecha de vencimiento de todas las cuotas de capital todavía pendientes por la cantidad de días
          indicada. No mueve dinero, no genera ningún asiento.
        </p>
        <form
          className="flex flex-col gap-3 sm:flex-row sm:items-end"
          onSubmit={(e) => {
            e.preventDefault()
            if (!diasDiferidos || !comentario) return
            diferir.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Días a diferir</span>
            <input
              type="number"
              min="1"
              value={diasDiferidos}
              onChange={(e) => setDiasDiferidos(e.target.value)}
              className="w-28 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-1 flex-col gap-1 text-sm">
            <span className="text-graphite-600">Motivo</span>
            <input
              value={comentario}
              onChange={(e) => setComentario(e.target.value)}
              placeholder="Ej. Diferimiento por pérdida de empleo"
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <button
            type="submit"
            disabled={diferir.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {diferir.isPending ? 'Aplicando…' : 'Diferir cuotas'}
          </button>
        </form>
        {diferir.isError && <p className="mt-2 text-sm text-red-700">{mensajeErrorCreditos(diferir.error, 'No se pudo diferir.')}</p>}
      </div>

      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-3 text-sm font-semibold text-graphite-100">Historial de diferimientos</h3>
        {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}
        {!isLoading && (diferimientos?.length ?? 0) === 0 && <p className="text-sm text-graphite-600">Sin diferimientos registrados.</p>}
        <div className="flex flex-col">
          {diferimientos?.map((d) => (
            <div key={d.id} className="border-b border-black/[0.04] py-2.5 text-sm last:border-0">
              <p className="font-medium text-graphite-100">
                {d.diasDiferidos} días — {d.fecha}
              </p>
              <p className="text-graphite-600">{d.comentario} ({d.registradoPor})</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

function TabCustodiaPagare({ prestamo }: { prestamo: Prestamo }) {
  const queryClient = useQueryClient()
  const [codigoEstado, setCodigoEstado] = useState<'E' | 'R'>('E')
  const [ubicacion, setUbicacion] = useState('')

  const { data: custodia } = useQuery<PagareCustodiaInfo | null>({
    queryKey: ['creditos-custodia-pagare', prestamo.id],
    queryFn: async () => {
      try {
        return (await api.get(`/api/creditos/prestamos/${prestamo.id}/custodia-pagare`)).data
      } catch {
        return null
      }
    },
  })

  const registrar = useMutation({
    mutationFn: async () =>
      (await api.post(`/api/creditos/prestamos/${prestamo.id}/custodia-pagare`, { codigoEstado, ubicacion })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-custodia-pagare', prestamo.id] })
      setUbicacion('')
    },
  })

  return (
    <div className="flex flex-col gap-6">
      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-3 text-sm font-semibold text-graphite-100">Custodia física del pagaré</h3>
        {custodia ? (
          <div className="mb-3 flex flex-col">
            <CampoLecturaCreditos label="Estado" valor={<Badge variant={custodia.codigoEstado === 'R' ? 'exito' : 'alerta'}>{custodia.estado}</Badge>} />
            <CampoLecturaCreditos label="Ubicación" valor={custodia.ubicacion} />
            <CampoLecturaCreditos label="Última actualización" valor={custodia.fechaActualizacion} />
          </div>
        ) : (
          <p className="mb-3 text-sm text-graphite-600">Sin custodia registrada todavía para este préstamo.</p>
        )}

        <form
          className="flex flex-col gap-3 sm:flex-row sm:items-end"
          onSubmit={(e) => {
            e.preventDefault()
            if (!ubicacion) return
            registrar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Estado</span>
            <select
              value={codigoEstado}
              onChange={(e) => setCodigoEstado(e.target.value as 'E' | 'R')}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="E">Entregado</option>
              <option value="R">Receptado</option>
            </select>
          </label>
          <label className="flex flex-1 flex-col gap-1 text-sm">
            <span className="text-graphite-600">Ubicación física</span>
            <input
              value={ubicacion}
              onChange={(e) => setUbicacion(e.target.value)}
              placeholder="Ej. Bóveda Matriz, archivo A-Z"
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            Registrar
          </button>
        </form>
        {registrar.isError && <p className="mt-2 text-sm text-red-700">{mensajeErrorCreditos(registrar.error, 'No se pudo registrar.')}</p>}
      </div>

      {custodia && custodia.movimientos.length > 0 && (
        <div className="glass-card rounded-xl p-4">
          <h3 className="mb-3 text-sm font-semibold text-graphite-100">Bitácora de custodia</h3>
          <div className="flex flex-col">
            {custodia.movimientos.map((m, i) => (
              <div key={i} className="border-b border-black/[0.04] py-2 text-sm last:border-0">
                <span className="font-medium text-graphite-100">{m.esRecepcion ? 'Receptado' : 'Entregado'}</span>
                <span className="text-graphite-600"> — {m.ubicacion} — {new Date(m.fecha).toLocaleString('es-EC')} ({m.registradoPor})</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function CampoLecturaCreditos({ label, valor }: { label: string; valor: React.ReactNode }) {
  return (
    <div className="flex items-baseline justify-between gap-4 border-b border-black/[0.04] py-2 text-sm last:border-0">
      <span className="text-graphite-600">{label}</span>
      <span className="text-right font-medium text-graphite-100">{valor}</span>
    </div>
  )
}

function TabCastigo({ prestamo, onCastigado }: { prestamo: Prestamo; onCastigado: () => void }) {
  const queryClient = useQueryClient()
  const [comentario, setComentario] = useState('')
  const [confirmando, setConfirmando] = useState(false)

  const castigar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/creditos/prestamos/${prestamo.id}/castigar`,
          { comentario },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-prestamos'] })
      onCastigado()
    },
  })

  if (prestamo.estado === 'Castigado') {
    return (
      <div className="glass-card rounded-xl p-4">
        <p className="text-sm text-graphite-600">
          Este préstamo ya fue castigado — el saldo de <strong>{formatoUsd(prestamo.saldo)}</strong> fue reversado
          contra la provisión para créditos incobrables.
        </p>
      </div>
    )
  }

  return (
    <div className="glass-card rounded-xl p-4">
      <h3 className="mb-2 text-sm font-semibold text-graphite-100">Castigo formal de cartera</h3>
      <p className="mb-3 text-sm text-graphite-600">
        Reversa el saldo vivo (<strong>{formatoUsd(prestamo.saldo)}</strong>) de la cartera de créditos contra la
        provisión para créditos incobrables (débito 1499 / crédito 1401) y marca el préstamo como Castigado. Acción
        irreversible.
      </p>

      {!confirmando ? (
        <button
          type="button"
          onClick={() => setConfirmando(true)}
          className="rounded-lg border border-red-700/30 px-4 py-2 text-sm font-medium text-red-700 hover:bg-red-700/5"
        >
          Castigar préstamo
        </button>
      ) : (
        <form
          className="flex flex-col gap-3"
          onSubmit={(e) => {
            e.preventDefault()
            if (!comentario) return
            castigar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Motivo del castigo (obligatorio)</span>
            <textarea
              required
              rows={2}
              value={comentario}
              onChange={(e) => setComentario(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <div className="flex items-center gap-3">
            <button
              type="submit"
              disabled={castigar.isPending}
              className="btn-hover rounded-lg bg-red-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {castigar.isPending ? 'Castigando…' : 'Confirmar castigo'}
            </button>
            <button type="button" onClick={() => setConfirmando(false)} className="text-sm text-graphite-600 hover:text-graphite-100">
              Cancelar
            </button>
          </div>
          {castigar.isError && <p className="text-sm text-red-700">{mensajeErrorCreditos(castigar.error, 'No se pudo castigar.')}</p>}
        </form>
      )}
    </div>
  )
}

const TABS_GESTION_PRESTAMO = [
  { id: 'diferimiento', label: 'Diferimiento de cuotas', icon: CalendarClock },
  { id: 'custodia', label: 'Custodia de pagaré', icon: FileLock2 },
  { id: 'castigo', label: 'Castigo de cartera', icon: Flame },
] as const
type TabGestionPrestamoId = (typeof TABS_GESTION_PRESTAMO)[number]['id']

function GestionEspecialPrestamoModal({ prestamo, onClose }: { prestamo: Prestamo; onClose: () => void }) {
  const [tab, setTab] = useState<TabGestionPrestamoId>('diferimiento')

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card flex max-h-[85vh] w-full max-w-3xl flex-col overflow-hidden rounded-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-black/[0.06] p-5 pb-4">
          <h2 className="text-sm font-semibold text-graphite-100">Gestión especial — préstamo {prestamo.numero}</h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <div className="flex flex-1 overflow-hidden">
          <div className="flex w-52 flex-shrink-0 flex-col gap-0.5 border-r border-black/[0.06] p-2">
            {TABS_GESTION_PRESTAMO.map((t) => (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className={`flex items-center gap-2 rounded-lg px-3 py-2 text-left text-xs font-medium transition-colors ${
                  tab === t.id ? 'bg-gold-500/10 text-gold-300' : 'text-graphite-600 hover:bg-black/[0.02]'
                }`}
              >
                <t.icon size={14} /> {t.label}
              </button>
            ))}
          </div>

          <div className="flex-1 overflow-y-auto p-5">
            {tab === 'diferimiento' && <TabDiferimiento prestamo={prestamo} />}
            {tab === 'custodia' && <TabCustodiaPagare prestamo={prestamo} />}
            {tab === 'castigo' && <TabCastigo prestamo={prestamo} onCastigado={onClose} />}
          </div>
        </div>
      </div>
    </div>
  )
}

function SeccionCartera() {
  const queryClient = useQueryClient()
  const [prestamoRubros, setPrestamoRubros] = useState<Prestamo | null>(null)
  const [prestamoGestion, setPrestamoGestion] = useState<Prestamo | null>(null)

  const { data: prestamos, isLoading: cargandoPrestamos } = useQuery<Prestamo[]>({
    queryKey: ['creditos-prestamos'],
    queryFn: async () => (await api.get('/api/creditos/prestamos')).data,
  })

  const pagarCuota = useMutation({
    mutationFn: async (idPrestamo: string) =>
      (
        await api.post(`/api/creditos/prestamos/${idPrestamo}/pagos`, undefined, {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
        })
      ).data as PagoCuotaResultado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-prestamos'] })
    },
  })

  const toggleDebitoSpi = useMutation({
    mutationFn: async ({ idPrestamo, activar }: { idPrestamo: string; activar: boolean }) =>
      api.patch(`/api/creditos/prestamos/${idPrestamo}/debito-spi`, { activar }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-prestamos'] })
    },
  })

  return (
    <div>
      <h2 className="mb-2 text-sm font-semibold uppercase tracking-wide text-graphite-600">Cartera de préstamos</h2>
      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Producto</Th>
            <Th>Desembolsado</Th>
            <Th>Saldo</Th>
            <Th>Tasa</Th>
            <Th>Estado</Th>
            <Th>Asesor</Th>
            <Th>Convenio</Th>
            <Th>Débito SPI</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {cargandoPrestamos && <EmptyState>Cargando…</EmptyState>}
          {!cargandoPrestamos && (prestamos?.length ?? 0) === 0 && <EmptyState>Todavía no hay préstamos desembolsados</EmptyState>}
          {prestamos?.map((p) => (
            <tr key={p.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{p.numero}</Td>
              <Td>{p.producto}</Td>
              <Td className="tabular-nums">{formatoUsd(p.deudaInicial)}</Td>
              <Td className="tabular-nums">{formatoUsd(p.saldo)}</Td>
              <Td>{(p.tasa * 100).toFixed(2)}%</Td>
              <Td>
                <Badge variant={estadoVariant(p.estado)}>{p.estado}</Badge>
              </Td>
              <Td>{p.codigoUsuarioAsesor ?? '—'}</Td>
              <Td title={p.nombreConvenio ?? undefined}>
                {p.nombreConvenio ? <Badge variant="alerta">Convenio</Badge> : '—'}
              </Td>
              <Td>
                {p.estado === 'Vigente' ? (
                  <button
                    type="button"
                    disabled={toggleDebitoSpi.isPending}
                    onClick={() => toggleDebitoSpi.mutate({ idPrestamo: p.id, activar: !p.debitoSpi })}
                    className="disabled:opacity-50"
                    title="Habilita que la cuota se debite automáticamente por SPI cuando llegue el sueldo del socio a su cuenta"
                  >
                    <Badge variant={p.debitoSpi ? 'exito' : 'neutral'}>{p.debitoSpi ? 'Activo' : 'Inactivo'}</Badge>
                  </button>
                ) : (
                  '—'
                )}
              </Td>
              <Td>
                {p.estado === 'Vigente' && (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      disabled={pagarCuota.isPending}
                      onClick={() => pagarCuota.mutate(p.id)}
                      className="text-sm font-medium text-gold-400 hover:underline disabled:opacity-50"
                    >
                      Pagar cuota
                    </button>
                    <button
                      type="button"
                      onClick={() => setPrestamoRubros(prestamoRubros?.id === p.id ? null : p)}
                      className="text-sm font-medium text-petrol-700 hover:underline"
                    >
                      Rubros
                    </button>
                    <button
                      type="button"
                      onClick={() => setPrestamoGestion(p)}
                      className="text-sm font-medium text-graphite-600 hover:underline"
                      title="Diferimiento de cuotas, custodia de pagaré, castigo de cartera"
                    >
                      Gestión especial
                    </button>
                  </div>
                )}
                {p.estado === 'Castigado' && (
                  <button
                    type="button"
                    onClick={() => setPrestamoGestion(p)}
                    className="text-sm font-medium text-graphite-600 hover:underline"
                  >
                    Ver castigo
                  </button>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>

      {prestamoRubros && <RubrosManualesPanel prestamo={prestamoRubros} />}
      {prestamoGestion && (
        <ModalPortal>
          <GestionEspecialPrestamoModal prestamo={prestamoGestion} onClose={() => setPrestamoGestion(null)} />
        </ModalPortal>
      )}
      {pagarCuota.isSuccess && (
        <p className="mt-2 rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
          Cuota {pagarCuota.data.numeroCuota} pagada: {formatoUsd(pagarCuota.data.montoCapital)} de capital +{' '}
          {formatoUsd(pagarCuota.data.montoInteres)} de interés
          {pagarCuota.data.montoInteresMora > 0 && (
            <>
              {' '}
              + <strong>{formatoUsd(pagarCuota.data.montoInteresMora)} de interés de mora</strong> (
              {pagarCuota.data.diasMoraCuota} días de atraso)
            </>
          )}
          .
        </p>
      )}
      {pagarCuota.isError && (
        <p className="mt-2 text-sm text-red-700">
          {(pagarCuota.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
            'No se pudo registrar el pago.'}
        </p>
      )}

      <SeccionAutoDebitoSpi />
    </div>
  )
}

function SeccionPlazoFijo() {
  const [mostrarFormDpf, setMostrarFormDpf] = useState(false)
  const [depositoARenovar, setDepositoARenovar] = useState<Deposito | null>(null)
  const queryClient = useQueryClient()

  const { data: depositos, isLoading: cargandoDepositos } = useQuery<Deposito[]>({
    queryKey: ['plazofijo-depositos'],
    queryFn: async () => (await api.get('/api/plazofijo/depositos')).data,
  })

  const cancelarDpf = useMutation({
    mutationFn: async (idDeposito: string) =>
      (
        await api.post(`/api/plazofijo/depositos/${idDeposito}/cancelar`, undefined, {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
        })
      ).data as DepositoCancelado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plazofijo-depositos'] })
    },
  })

  return (
    <div>
      <div className="mb-2 flex items-center justify-between">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-graphite-600">Depósitos a plazo fijo</h2>
        {!mostrarFormDpf && (
          <button
            type="button"
            onClick={() => setMostrarFormDpf(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Abrir DPF
          </button>
        )}
      </div>

      {mostrarFormDpf && <AbrirDpfForm onClose={() => setMostrarFormDpf(false)} />}
      {depositoARenovar && (
        <ModalPortal>
          <RenovarDpfModal deposito={depositoARenovar} onClose={() => setDepositoARenovar(null)} />
        </ModalPortal>
      )}

      {cancelarDpf.isSuccess && (
        <p className="mb-4 rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
          Depósito cancelado: {formatoUsd(cancelarDpf.data.valorDevuelto)} de capital
          {cancelarDpf.data.interesPagado > 0 && (
            <> + {formatoUsd(cancelarDpf.data.interesPagado)} de interés devengado proporcional</>
          )}
          .
        </p>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Socio</Th>
            <Th>Monto</Th>
            <Th>Tasa</Th>
            <Th>Plazo</Th>
            <Th>Vencimiento</Th>
            <Th>Estado</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {cargandoDepositos && <EmptyState>Cargando…</EmptyState>}
          {!cargandoDepositos && (depositos?.length ?? 0) === 0 && <EmptyState>Todavía no hay depósitos a plazo fijo</EmptyState>}
          {depositos?.map((d) => (
            <tr key={d.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{d.codigo}</Td>
              <Td>{d.socio}</Td>
              <Td className="tabular-nums">{formatoUsd(d.monto)}</Td>
              <Td>{(d.tasa * 100).toFixed(2)}%</Td>
              <Td>{d.plazoDias} días</Td>
              <Td>{d.fechaVencimiento}</Td>
              <Td>
                <Badge variant={estadoVariant(d.estado)}>{d.estado}</Badge>
              </Td>
              <Td>
                {d.estado === 'Vigente' && (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => setDepositoARenovar(d)}
                      className="text-sm font-medium text-gold-400 hover:underline"
                    >
                      Renovar
                    </button>
                    <button
                      type="button"
                      disabled={cancelarDpf.isPending}
                      onClick={() => cancelarDpf.mutate(d.id)}
                      className="text-sm font-medium text-graphite-600 hover:underline disabled:opacity-50"
                    >
                      Cancelar
                    </button>
                  </div>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>

      <SeccionRenovacionesDpf />
    </div>
  )
}

const TABS = [
  { id: 'solicitudes', label: 'Solicitudes' },
  { id: 'cartera', label: 'Cartera de préstamos' },
  { id: 'plazofijo', label: 'Plazo fijo' },
  { id: 'reportes', label: 'Reportes' },
] as const
type TabId = (typeof TABS)[number]['id']

export function Creditos() {
  const [tab, setTab] = useState<TabId>('solicitudes')

  const { data: productos } = useQuery<Producto[]>({
    queryKey: ['creditos-productos'],
    queryFn: async () => (await api.get('/api/creditos/productos')).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={Landmark}
        title="Créditos"
        subtitle="Solicitudes, desembolsos, cartera de préstamos y plazo fijo"
      />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {TABS.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`rounded-t-lg px-3 py-2 text-sm font-medium transition ${
              tab === t.id
                ? 'border-b-2 border-gold-500 text-graphite-100'
                : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'solicitudes' && <SeccionSolicitudes productos={productos} />}
      {tab === 'cartera' && <SeccionCartera />}
      {tab === 'plazofijo' && <SeccionPlazoFijo />}
      {tab === 'reportes' && <SeccionReportesCreditos />}
    </div>
  )
}

interface ConcesionCreditoItem {
  numero: string
  producto: string
  cliente: string
  monto: number
  tasa: number
  fechaAdjudicacion: string
  fechaVencimiento: string
  estado: string
}

interface CreditoPrecanceladoItem {
  numero: string
  producto: string
  cliente: string
  monto: number
  fechaAdjudicacion: string
  fechaVencimientoNominal: string
  fechaCancelacionReal: string
}

interface ProximoVencimientoItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  numeroCuota: number
  montoCuota: number
  fechaVencimiento: string
}

const COLUMNAS_CONCESION: ColumnaExportable<ConcesionCreditoItem>[] = [
  { header: 'Número', accessor: (c) => c.numero },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Monto', accessor: (c) => c.monto },
  { header: 'Tasa', accessor: (c) => `${(c.tasa * 100).toFixed(2)}%` },
  { header: 'Adjudicación', accessor: (c) => c.fechaAdjudicacion },
  { header: 'Vencimiento', accessor: (c) => c.fechaVencimiento },
  { header: 'Estado', accessor: (c) => c.estado },
]

const COLUMNAS_PRECANCELADOS: ColumnaExportable<CreditoPrecanceladoItem>[] = [
  { header: 'Número', accessor: (c) => c.numero },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Monto', accessor: (c) => c.monto },
  { header: 'Vencimiento nominal', accessor: (c) => c.fechaVencimientoNominal },
  { header: 'Cancelación real', accessor: (c) => c.fechaCancelacionReal },
]

const COLUMNAS_VENCIMIENTOS: ColumnaExportable<ProximoVencimientoItem>[] = [
  { header: 'Préstamo', accessor: (v) => v.numeroPrestamo },
  { header: 'Producto', accessor: (v) => v.producto },
  { header: 'Socio', accessor: (v) => v.cliente },
  { header: 'Cuota', accessor: (v) => v.numeroCuota },
  { header: 'Monto', accessor: (v) => v.montoCuota },
  { header: 'Vencimiento', accessor: (v) => v.fechaVencimiento },
]

interface CreditoCanceladoItem {
  numero: string
  producto: string
  cliente: string
  monto: number
  fechaAdjudicacion: string
  fechaCancelacion: string
}

interface AnexoGarantiaItem {
  numeroPrestamo: string
  producto: string
  saldo: number
  titular: string
  garante: string
  estadoGarantia: string
}

const COLUMNAS_CANCELADOS: ColumnaExportable<CreditoCanceladoItem>[] = [
  { header: 'Número', accessor: (c) => c.numero },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Monto', accessor: (c) => c.monto },
  { header: 'Adjudicación', accessor: (c) => c.fechaAdjudicacion },
  { header: 'Cancelación', accessor: (c) => c.fechaCancelacion },
]

const COLUMNAS_GARANTIAS: ColumnaExportable<AnexoGarantiaItem>[] = [
  { header: 'Préstamo', accessor: (g) => g.numeroPrestamo },
  { header: 'Producto', accessor: (g) => g.producto },
  { header: 'Saldo', accessor: (g) => g.saldo },
  { header: 'Titular', accessor: (g) => g.titular },
  { header: 'Garante', accessor: (g) => g.garante },
  { header: 'Estado garantía', accessor: (g) => g.estadoGarantia },
]

interface CreditoMoraAsesorItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  asesor: string | null
  saldo: number
  diasMora: number
}

interface IndiceMorosidadItem {
  carteraTotal: number
  carteraVencida: number
  indice: number
  totalPrestamos: number
  prestamosVencidos: number
}

interface DebitoSpiNoProcesadoItem {
  numeroPrestamo: string
  numeroCuota: number | null
  monto: number
  motivo: string
  fecha: string
}

const COLUMNAS_MORA_ASESOR: ColumnaExportable<CreditoMoraAsesorItem>[] = [
  { header: 'Préstamo', accessor: (c) => c.numeroPrestamo },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Asesor', accessor: (c) => c.asesor ?? '—' },
  { header: 'Saldo', accessor: (c) => c.saldo },
  { header: 'Días de mora', accessor: (c) => c.diasMora },
]

const COLUMNAS_SPI_NO_PROCESADOS: ColumnaExportable<DebitoSpiNoProcesadoItem>[] = [
  { header: 'Préstamo', accessor: (d) => d.numeroPrestamo },
  { header: 'Cuota', accessor: (d) => d.numeroCuota ?? '—' },
  { header: 'Monto', accessor: (d) => d.monto },
  { header: 'Motivo', accessor: (d) => d.motivo },
  { header: 'Fecha', accessor: (d) => d.fecha },
]

interface AnexoCarteraCastigadaItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  saldoTransferido: number
  fecha: string
  comentario: string
  registradoPor: string
}

interface CalificacionPrestamoItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  saldo: number
  diasMora: number
  codigoCategoria: string
  categoria: string
  porcentajeProvision: number
  provisionRequerida: number
}

interface GastoJudicialItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  rubro: string
  proyectado: number
  cobrado: number
  fecha: string
  estado: string
}

const COLUMNAS_CARTERA_CASTIGADA: ColumnaExportable<AnexoCarteraCastigadaItem>[] = [
  { header: 'Préstamo', accessor: (c) => c.numeroPrestamo },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Saldo transferido', accessor: (c) => c.saldoTransferido },
  { header: 'Fecha', accessor: (c) => c.fecha },
  { header: 'Comentario', accessor: (c) => c.comentario },
  { header: 'Registrado por', accessor: (c) => c.registradoPor },
]

const COLUMNAS_CALIFICACION: ColumnaExportable<CalificacionPrestamoItem>[] = [
  { header: 'Préstamo', accessor: (c) => c.numeroPrestamo },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Saldo', accessor: (c) => c.saldo },
  { header: 'Días de mora', accessor: (c) => c.diasMora },
  { header: 'Categoría', accessor: (c) => `${c.codigoCategoria} — ${c.categoria}` },
  { header: '% Provisión', accessor: (c) => `${(c.porcentajeProvision * 100).toFixed(2)}%` },
  { header: 'Provisión requerida', accessor: (c) => c.provisionRequerida },
]

const COLUMNAS_GASTOS_JUDICIALES: ColumnaExportable<GastoJudicialItem>[] = [
  { header: 'Préstamo', accessor: (g) => g.numeroPrestamo },
  { header: 'Producto', accessor: (g) => g.producto },
  { header: 'Socio', accessor: (g) => g.cliente },
  { header: 'Rubro', accessor: (g) => g.rubro },
  { header: 'Proyectado', accessor: (g) => g.proyectado },
  { header: 'Cobrado', accessor: (g) => g.cobrado },
  { header: 'Fecha', accessor: (g) => g.fecha },
  { header: 'Estado', accessor: (g) => g.estado },
]

interface ConsolidadoTipoCarteraItem {
  producto: string
  cantidadPrestamos: number
  saldoTotal: number
  tasaPromedio: number
}

interface ConsolidadoSeguroDesgravamenItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  saldoPendiente: number
}

const COLUMNAS_CONSOLIDADO_TIPO_CARTERA: ColumnaExportable<ConsolidadoTipoCarteraItem>[] = [
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Cantidad préstamos', accessor: (c) => c.cantidadPrestamos },
  { header: 'Saldo total', accessor: (c) => c.saldoTotal },
  { header: 'Tasa promedio', accessor: (c) => `${(c.tasaPromedio * 100).toFixed(2)}%` },
]

const COLUMNAS_SEGURO_DESGRAVAMEN: ColumnaExportable<ConsolidadoSeguroDesgravamenItem>[] = [
  { header: 'Préstamo', accessor: (s) => s.numeroPrestamo },
  { header: 'Producto', accessor: (s) => s.producto },
  { header: 'Socio', accessor: (s) => s.cliente },
  { header: 'Saldo pendiente', accessor: (s) => s.saldoPendiente },
]

interface AnexoCarteraCastigadaAgenciaItem {
  agencia: string
  cantidadCastigos: number
  totalCastigado: number
}

interface AnexoCarteraCastigadaClienteItem {
  cliente: string
  identificacion: string
  cantidadCastigos: number
  totalCastigado: number
}

interface AnexoItemCreditoItem {
  numeroPrestamo: string
  numeroCuota: number
  rubro: string
  codigoTipoRubro: string
  proyectado: number
  cobrado: number
  estado: string
  fechaVencimiento: string
}

interface CreditoVinculadoItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  saldo: number
  codigoCausaVinculacion: string
  causaVinculacion: string
}

const COLUMNAS_CASTIGADA_AGENCIA: ColumnaExportable<AnexoCarteraCastigadaAgenciaItem>[] = [
  { header: 'Agencia', accessor: (c) => c.agencia },
  { header: 'Cantidad de castigos', accessor: (c) => c.cantidadCastigos },
  { header: 'Total castigado', accessor: (c) => c.totalCastigado },
]

const COLUMNAS_CASTIGADA_CLIENTE: ColumnaExportable<AnexoCarteraCastigadaClienteItem>[] = [
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Identificación', accessor: (c) => c.identificacion },
  { header: 'Cantidad de castigos', accessor: (c) => c.cantidadCastigos },
  { header: 'Total castigado', accessor: (c) => c.totalCastigado },
]

const COLUMNAS_ITEM_CREDITO: ColumnaExportable<AnexoItemCreditoItem>[] = [
  { header: 'Préstamo', accessor: (r) => r.numeroPrestamo },
  { header: 'Cuota', accessor: (r) => r.numeroCuota },
  { header: 'Rubro', accessor: (r) => r.rubro },
  { header: 'Proyectado', accessor: (r) => r.proyectado },
  { header: 'Cobrado', accessor: (r) => r.cobrado },
  { header: 'Estado', accessor: (r) => r.estado },
  { header: 'Vencimiento', accessor: (r) => r.fechaVencimiento },
]

const COLUMNAS_VINCULADOS: ColumnaExportable<CreditoVinculadoItem>[] = [
  { header: 'Préstamo', accessor: (c) => c.numeroPrestamo },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Saldo', accessor: (c) => c.saldo },
  { header: 'Código causa', accessor: (c) => c.codigoCausaVinculacion },
  { header: 'Causa de vinculación', accessor: (c) => c.causaVinculacion },
]

interface PrestamoPorConvenioItem {
  numeroPrestamo: string
  producto: string
  cliente: string
  saldo: number
  codigoTipoConvenio: string
  convenio: string
}

interface AbonoConvenioItem {
  numeroPrestamo: string
  convenio: string
  numeroCuota: number
  capital: number
  fecha: string
}

interface EntregaRecuperacionItem {
  anio: number
  mes: number
  entregado: number
  recuperado: number
  diferencia: number
}

const COLUMNAS_POR_CONVENIO: ColumnaExportable<PrestamoPorConvenioItem>[] = [
  { header: 'Préstamo', accessor: (c) => c.numeroPrestamo },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Saldo', accessor: (c) => c.saldo },
  { header: 'Convenio', accessor: (c) => c.convenio },
]

const COLUMNAS_ABONOS_CONVENIO: ColumnaExportable<AbonoConvenioItem>[] = [
  { header: 'Préstamo', accessor: (a) => a.numeroPrestamo },
  { header: 'Convenio', accessor: (a) => a.convenio },
  { header: 'Cuota', accessor: (a) => a.numeroCuota },
  { header: 'Capital abonado', accessor: (a) => a.capital },
  { header: 'Fecha', accessor: (a) => a.fecha },
]

const COLUMNAS_ENTREGA_RECUPERACION: ColumnaExportable<EntregaRecuperacionItem>[] = [
  { header: 'Año', accessor: (e) => e.anio },
  { header: 'Mes', accessor: (e) => e.mes },
  { header: 'Entregado (desembolsos)', accessor: (e) => e.entregado },
  { header: 'Recuperado (capital cobrado)', accessor: (e) => e.recuperado },
  { header: 'Diferencia', accessor: (e) => e.diferencia },
]

function SeccionReportesCreditos() {
  const [reporte, setReporte] = useState<
    | 'concesion'
    | 'precancelados'
    | 'vencimientos'
    | 'cancelados'
    | 'garantias'
    | 'mora-asesor'
    | 'indice-morosidad'
    | 'spi-no-procesados'
    | 'cartera-castigada'
    | 'calificacion'
    | 'gastos-judiciales'
    | 'consolidado-tipo-cartera'
    | 'seguro-desgravamen'
    | 'castigada-agencia'
    | 'castigada-cliente'
    | 'item-credito'
    | 'vinculados'
    | 'por-convenio'
    | 'abonos-convenio'
    | 'entrega-recuperacion'
  >('concesion')
  const [desde, setDesde] = useState(() => new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10))
  const [hasta, setHasta] = useState(() => new Date().toISOString().slice(0, 10))
  const [dias, setDias] = useState('30')
  const [codigoAsesor, setCodigoAsesor] = useState('')
  const [numeroPrestamoFiltro, setNumeroPrestamoFiltro] = useState('')

  const { data: concesion } = useQuery<ConcesionCreditoItem[]>({
    queryKey: ['creditos-reporte-concesion', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/concesion-credito', { params: { desde, hasta } })).data,
    enabled: reporte === 'concesion',
  })

  const { data: precancelados } = useQuery<CreditoPrecanceladoItem[]>({
    queryKey: ['creditos-reporte-precancelados', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/creditos-precancelados', { params: { desde, hasta } })).data,
    enabled: reporte === 'precancelados',
  })

  const { data: vencimientos } = useQuery<ProximoVencimientoItem[]>({
    queryKey: ['creditos-reporte-vencimientos', dias],
    queryFn: async () => (await api.get('/api/creditos/reportes/proximos-vencimientos', { params: { dias: Number(dias) } })).data,
    enabled: reporte === 'vencimientos',
  })

  const { data: cancelados } = useQuery<CreditoCanceladoItem[]>({
    queryKey: ['creditos-reporte-cancelados', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/creditos-cancelados', { params: { desde, hasta } })).data,
    enabled: reporte === 'cancelados',
  })

  const { data: garantias } = useQuery<AnexoGarantiaItem[]>({
    queryKey: ['creditos-reporte-garantias'],
    queryFn: async () => (await api.get('/api/creditos/reportes/anexo-garantias')).data,
    enabled: reporte === 'garantias',
  })

  const { data: moraAsesor } = useQuery<CreditoMoraAsesorItem[]>({
    queryKey: ['creditos-reporte-mora-asesor', codigoAsesor],
    queryFn: async () =>
      (
        await api.get('/api/creditos/reportes/creditos-mora-asesor', {
          params: codigoAsesor ? { codigoUsuarioAsesor: codigoAsesor } : {},
        })
      ).data,
    enabled: reporte === 'mora-asesor',
  })

  const { data: indiceMorosidad } = useQuery<IndiceMorosidadItem>({
    queryKey: ['creditos-reporte-indice-morosidad'],
    queryFn: async () => (await api.get('/api/creditos/reportes/indice-morosidad')).data,
    enabled: reporte === 'indice-morosidad',
  })

  const { data: spiNoProcesados } = useQuery<DebitoSpiNoProcesadoItem[]>({
    queryKey: ['creditos-reporte-spi-no-procesados', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/debitos-spi-no-procesados', { params: { desde, hasta } })).data,
    enabled: reporte === 'spi-no-procesados',
  })

  const { data: carteraCastigada } = useQuery<AnexoCarteraCastigadaItem[]>({
    queryKey: ['creditos-reporte-cartera-castigada', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/anexo-cartera-castigada', { params: { desde, hasta } })).data,
    enabled: reporte === 'cartera-castigada',
  })

  const { data: calificacion } = useQuery<CalificacionPrestamoItem[]>({
    queryKey: ['creditos-reporte-calificacion'],
    queryFn: async () => (await api.get('/api/creditos/reportes/calificacion-prestamos')).data,
    enabled: reporte === 'calificacion',
  })

  const { data: gastosJudiciales } = useQuery<GastoJudicialItem[]>({
    queryKey: ['creditos-reporte-gastos-judiciales', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/gastos-judiciales', { params: { desde, hasta } })).data,
    enabled: reporte === 'gastos-judiciales',
  })

  const { data: consolidadoTipoCartera } = useQuery<ConsolidadoTipoCarteraItem[]>({
    queryKey: ['creditos-reporte-consolidado-tipo-cartera'],
    queryFn: async () => (await api.get('/api/creditos/reportes/consolidado-tipo-cartera')).data,
    enabled: reporte === 'consolidado-tipo-cartera',
  })

  const { data: seguroDesgravamen } = useQuery<ConsolidadoSeguroDesgravamenItem[]>({
    queryKey: ['creditos-reporte-seguro-desgravamen'],
    queryFn: async () => (await api.get('/api/creditos/reportes/consolidado-seguro-desgravamen')).data,
    enabled: reporte === 'seguro-desgravamen',
  })

  const { data: castigadaAgencia } = useQuery<AnexoCarteraCastigadaAgenciaItem[]>({
    queryKey: ['creditos-reporte-castigada-agencia'],
    queryFn: async () => (await api.get('/api/creditos/reportes/anexo-cartera-castigada-agencia')).data,
    enabled: reporte === 'castigada-agencia',
  })

  const { data: castigadaCliente } = useQuery<AnexoCarteraCastigadaClienteItem[]>({
    queryKey: ['creditos-reporte-castigada-cliente'],
    queryFn: async () => (await api.get('/api/creditos/reportes/anexo-cartera-castigada-cliente')).data,
    enabled: reporte === 'castigada-cliente',
  })

  const { data: itemCredito } = useQuery<AnexoItemCreditoItem[]>({
    queryKey: ['creditos-reporte-item-credito', numeroPrestamoFiltro],
    queryFn: async () =>
      (
        await api.get('/api/creditos/reportes/anexo-item-credito', {
          params: numeroPrestamoFiltro ? { numeroPrestamo: numeroPrestamoFiltro } : {},
        })
      ).data,
    enabled: reporte === 'item-credito',
  })

  const { data: vinculados } = useQuery<CreditoVinculadoItem[]>({
    queryKey: ['creditos-reporte-vinculados'],
    queryFn: async () => (await api.get('/api/creditos/reportes/creditos-vinculados')).data,
    enabled: reporte === 'vinculados',
  })

  const { data: porConvenio } = useQuery<PrestamoPorConvenioItem[]>({
    queryKey: ['creditos-reporte-por-convenio'],
    queryFn: async () => (await api.get('/api/creditos/reportes/prestamo-por-convenio')).data,
    enabled: reporte === 'por-convenio',
  })

  const { data: abonosConvenio } = useQuery<AbonoConvenioItem[]>({
    queryKey: ['creditos-reporte-abonos-convenio', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/abonos-por-convenio', { params: { desde, hasta } })).data,
    enabled: reporte === 'abonos-convenio',
  })

  const { data: entregaRecuperacion } = useQuery<EntregaRecuperacionItem[]>({
    queryKey: ['creditos-reporte-entrega-recuperacion', desde, hasta],
    queryFn: async () => (await api.get('/api/creditos/reportes/entrega-recuperacion', { params: { desde, hasta } })).data,
    enabled: reporte === 'entrega-recuperacion',
  })

  return (
    <div>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex gap-1 rounded-lg border border-black/[0.08] p-1">
          {(
            [
              ['concesion', 'Concesión de crédito'],
              ['precancelados', 'Créditos precancelados'],
              ['vencimientos', 'Próximos vencimientos'],
              ['cancelados', 'Créditos cancelados'],
              ['garantias', 'Anexo garantías'],
              ['mora-asesor', 'Créditos en mora por asesor'],
              ['indice-morosidad', 'Índice de morosidad'],
              ['spi-no-procesados', 'Débitos SPI no procesados'],
              ['cartera-castigada', 'Anexo cartera castigada'],
              ['calificacion', 'Calificación y provisión'],
              ['gastos-judiciales', 'Gastos judiciales'],
              ['consolidado-tipo-cartera', 'Consolidado por producto'],
              ['seguro-desgravamen', 'Seguro desgravamen'],
              ['castigada-agencia', 'Cartera castigada por agencia'],
              ['castigada-cliente', 'Cartera castigada por cliente'],
              ['item-credito', 'Anexo ítems de crédito'],
              ['vinculados', 'Créditos vinculados'],
              ['por-convenio', 'Préstamos por convenio'],
              ['abonos-convenio', 'Abonos por convenio'],
              ['entrega-recuperacion', 'Entrega vs recuperación'],
            ] as const
          ).map(([id, label]) => (
            <button
              key={id}
              type="button"
              onClick={() => setReporte(id)}
              className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
                reporte === id ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.02]'
              }`}
            >
              {label}
            </button>
          ))}
        </div>
        {reporte === 'concesion' && (
          <BotonesExportar
            nombreArchivo="creditos_concesion"
            titulo="Concesión de crédito"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_CONCESION}
            filas={concesion ?? []}
          />
        )}
        {reporte === 'precancelados' && (
          <BotonesExportar
            nombreArchivo="creditos_precancelados"
            titulo="Créditos precancelados"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_PRECANCELADOS}
            filas={precancelados ?? []}
          />
        )}
        {reporte === 'vencimientos' && (
          <BotonesExportar
            nombreArchivo="creditos_proximos_vencimientos"
            titulo="Próximos vencimientos"
            subtitulo={`Próximos ${dias} días`}
            columnas={COLUMNAS_VENCIMIENTOS}
            filas={vencimientos ?? []}
          />
        )}
        {reporte === 'cancelados' && (
          <BotonesExportar
            nombreArchivo="creditos_cancelados"
            titulo="Créditos cancelados"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_CANCELADOS}
            filas={cancelados ?? []}
          />
        )}
        {reporte === 'garantias' && (
          <BotonesExportar
            nombreArchivo="creditos_anexo_garantias"
            titulo="Anexo garantías"
            columnas={COLUMNAS_GARANTIAS}
            filas={garantias ?? []}
          />
        )}
        {reporte === 'mora-asesor' && (
          <BotonesExportar
            nombreArchivo="creditos_mora_asesor"
            titulo="Créditos en mora por asesor"
            subtitulo={codigoAsesor ? `Asesor: ${codigoAsesor}` : 'Todos los asesores'}
            columnas={COLUMNAS_MORA_ASESOR}
            filas={moraAsesor ?? []}
          />
        )}
        {reporte === 'indice-morosidad' && indiceMorosidad && (
          <BotonesExportar
            nombreArchivo="creditos_indice_morosidad"
            titulo="Índice de morosidad"
            columnas={[
              { header: 'Cartera total', accessor: () => indiceMorosidad.carteraTotal },
              { header: 'Cartera vencida', accessor: () => indiceMorosidad.carteraVencida },
              { header: 'Índice', accessor: () => `${(indiceMorosidad.indice * 100).toFixed(2)}%` },
              { header: 'Total préstamos', accessor: () => indiceMorosidad.totalPrestamos },
              { header: 'Préstamos vencidos', accessor: () => indiceMorosidad.prestamosVencidos },
            ]}
            filas={[indiceMorosidad]}
          />
        )}
        {reporte === 'spi-no-procesados' && (
          <BotonesExportar
            nombreArchivo="creditos_spi_no_procesados"
            titulo="Débitos SPI no procesados"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_SPI_NO_PROCESADOS}
            filas={spiNoProcesados ?? []}
          />
        )}
        {reporte === 'cartera-castigada' && (
          <BotonesExportar
            nombreArchivo="creditos_cartera_castigada"
            titulo="Anexo cartera castigada"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_CARTERA_CASTIGADA}
            filas={carteraCastigada ?? []}
          />
        )}
        {reporte === 'calificacion' && (
          <BotonesExportar
            nombreArchivo="creditos_calificacion_provision"
            titulo="Calificación y provisión de préstamos"
            columnas={COLUMNAS_CALIFICACION}
            filas={calificacion ?? []}
          />
        )}
        {reporte === 'gastos-judiciales' && (
          <BotonesExportar
            nombreArchivo="creditos_gastos_judiciales"
            titulo="Gastos judiciales"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_GASTOS_JUDICIALES}
            filas={gastosJudiciales ?? []}
          />
        )}
        {reporte === 'consolidado-tipo-cartera' && (
          <BotonesExportar
            nombreArchivo="creditos_consolidado_tipo_cartera"
            titulo="Consolidado por producto"
            columnas={COLUMNAS_CONSOLIDADO_TIPO_CARTERA}
            filas={consolidadoTipoCartera ?? []}
          />
        )}
        {reporte === 'seguro-desgravamen' && (
          <BotonesExportar
            nombreArchivo="creditos_seguro_desgravamen"
            titulo="Consolidado seguro desgravamen"
            columnas={COLUMNAS_SEGURO_DESGRAVAMEN}
            filas={seguroDesgravamen ?? []}
          />
        )}
        {reporte === 'castigada-agencia' && (
          <BotonesExportar
            nombreArchivo="creditos_castigada_agencia"
            titulo="Cartera castigada por agencia"
            columnas={COLUMNAS_CASTIGADA_AGENCIA}
            filas={castigadaAgencia ?? []}
          />
        )}
        {reporte === 'castigada-cliente' && (
          <BotonesExportar
            nombreArchivo="creditos_castigada_cliente"
            titulo="Cartera castigada por cliente"
            columnas={COLUMNAS_CASTIGADA_CLIENTE}
            filas={castigadaCliente ?? []}
          />
        )}
        {reporte === 'item-credito' && (
          <BotonesExportar
            nombreArchivo="creditos_anexo_item_credito"
            titulo="Anexo ítems de crédito"
            subtitulo={numeroPrestamoFiltro ? `Préstamo ${numeroPrestamoFiltro}` : 'Cartera vigente completa'}
            columnas={COLUMNAS_ITEM_CREDITO}
            filas={itemCredito ?? []}
          />
        )}
        {reporte === 'vinculados' && (
          <BotonesExportar
            nombreArchivo="creditos_vinculados"
            titulo="Créditos vinculados"
            columnas={COLUMNAS_VINCULADOS}
            filas={vinculados ?? []}
          />
        )}
        {reporte === 'por-convenio' && (
          <BotonesExportar
            nombreArchivo="creditos_por_convenio"
            titulo="Préstamos por convenio"
            columnas={COLUMNAS_POR_CONVENIO}
            filas={porConvenio ?? []}
          />
        )}
        {reporte === 'abonos-convenio' && (
          <BotonesExportar
            nombreArchivo="creditos_abonos_convenio"
            titulo="Abonos por convenio"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_ABONOS_CONVENIO}
            filas={abonosConvenio ?? []}
          />
        )}
        {reporte === 'entrega-recuperacion' && (
          <BotonesExportar
            nombreArchivo="creditos_entrega_recuperacion"
            titulo="Entrega vs recuperación"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_ENTREGA_RECUPERACION}
            filas={entregaRecuperacion ?? []}
          />
        )}
      </div>
      <div className="mb-4 flex flex-wrap items-end gap-3">
        {reporte === 'mora-asesor' ? (
          <label className="flex flex-col gap-1 text-xs">
            <span className="text-graphite-600">Código de asesor (opcional)</span>
            <input
              type="text"
              placeholder="Ej. ACHICAIZA — vacío = todos"
              value={codigoAsesor}
              onChange={(e) => setCodigoAsesor(e.target.value.toUpperCase())}
              className="w-56 rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
        ) : reporte === 'item-credito' ? (
          <label className="flex flex-col gap-1 text-xs">
            <span className="text-graphite-600">Número de préstamo (opcional)</span>
            <input
              type="text"
              placeholder="Vacío = toda la cartera vigente"
              value={numeroPrestamoFiltro}
              onChange={(e) => setNumeroPrestamoFiltro(e.target.value)}
              className="w-56 rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
        ) : reporte === 'garantias' ||
          reporte === 'indice-morosidad' ||
          reporte === 'calificacion' ||
          reporte === 'consolidado-tipo-cartera' ||
          reporte === 'seguro-desgravamen' ||
          reporte === 'castigada-agencia' ||
          reporte === 'castigada-cliente' ||
          reporte === 'vinculados' ||
          reporte === 'por-convenio' ? null : reporte === 'vencimientos' ? (
          <label className="flex flex-col gap-1 text-xs">
            <span className="text-graphite-600">Días hacia adelante</span>
            <input
              type="number"
              min="1"
              value={dias}
              onChange={(e) => setDias(e.target.value)}
              className="w-24 rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
        ) : (
          <>
            <label className="flex flex-col gap-1 text-xs">
              <span className="text-graphite-600">Desde</span>
              <input
                type="date"
                value={desde}
                onChange={(e) => setDesde(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-xs">
              <span className="text-graphite-600">Hasta</span>
              <input
                type="date"
                value={hasta}
                onChange={(e) => setHasta(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          </>
        )}
      </div>

      {reporte === 'concesion' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Número</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Monto</Th>
              <Th>Tasa</Th>
              <Th>Adjudicación</Th>
              <Th>Vencimiento</Th>
              <Th>Estado</Th>
            </tr>
          </thead>
          <tbody>
            {(concesion?.length ?? 0) === 0 && <EmptyState>Sin créditos concedidos en el rango</EmptyState>}
            {concesion?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numero}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(c.monto)}</Td>
                <Td>{(c.tasa * 100).toFixed(2)}%</Td>
                <Td>{c.fechaAdjudicacion}</Td>
                <Td>{c.fechaVencimiento}</Td>
                <Td>
                  <Badge variant={estadoVariant(c.estado)}>{c.estado}</Badge>
                </Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'precancelados' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Número</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Monto</Th>
              <Th>Vencimiento nominal</Th>
              <Th>Cancelación real</Th>
            </tr>
          </thead>
          <tbody>
            {(precancelados?.length ?? 0) === 0 && <EmptyState>Sin créditos precancelados en el rango</EmptyState>}
            {precancelados?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numero}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(c.monto)}</Td>
                <Td>{c.fechaVencimientoNominal}</Td>
                <Td>{c.fechaCancelacionReal}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'vencimientos' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Cuota</Th>
              <Th>Monto</Th>
              <Th>Vencimiento</Th>
            </tr>
          </thead>
          <tbody>
            {(vencimientos?.length ?? 0) === 0 && <EmptyState>Sin cuotas por vencer en el rango</EmptyState>}
            {vencimientos?.map((v, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{v.numeroPrestamo}</Td>
                <Td>{v.producto}</Td>
                <Td>{v.cliente}</Td>
                <Td>{v.numeroCuota}</Td>
                <Td className="tabular-nums">{formatoUsd(v.montoCuota)}</Td>
                <Td>{v.fechaVencimiento}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'cancelados' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Número</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Monto</Th>
              <Th>Adjudicación</Th>
              <Th>Cancelación</Th>
            </tr>
          </thead>
          <tbody>
            {(cancelados?.length ?? 0) === 0 && <EmptyState>Sin créditos cancelados en el rango</EmptyState>}
            {cancelados?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numero}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(c.monto)}</Td>
                <Td>{c.fechaAdjudicacion}</Td>
                <Td>{c.fechaCancelacion}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'garantias' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Saldo</Th>
              <Th>Titular</Th>
              <Th>Garante</Th>
              <Th>Estado garantía</Th>
            </tr>
          </thead>
          <tbody>
            {(garantias?.length ?? 0) === 0 && <EmptyState>Sin garantías activas</EmptyState>}
            {garantias?.map((g, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{g.numeroPrestamo}</Td>
                <Td>{g.producto}</Td>
                <Td className="tabular-nums">{formatoUsd(g.saldo)}</Td>
                <Td>{g.titular}</Td>
                <Td>{g.garante}</Td>
                <Td>{g.estadoGarantia}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'mora-asesor' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Asesor</Th>
              <Th>Saldo</Th>
              <Th>Días de mora</Th>
            </tr>
          </thead>
          <tbody>
            {(moraAsesor?.length ?? 0) === 0 && <EmptyState>Sin cartera en mora para este filtro</EmptyState>}
            {moraAsesor?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numeroPrestamo}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td>{c.asesor ?? '—'}</Td>
                <Td className="tabular-nums">{formatoUsd(c.saldo)}</Td>
                <Td>
                  <Badge variant={c.diasMora > 30 ? 'peligro' : 'alerta'}>{c.diasMora} días</Badge>
                </Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'indice-morosidad' && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <div className="glass-card rounded-xl p-4">
            <p className="text-xs text-graphite-600">Cartera total</p>
            <p className="mt-1 text-xl font-semibold tabular-nums text-graphite-100">
              {formatoUsd(indiceMorosidad?.carteraTotal ?? 0)}
            </p>
          </div>
          <div className="glass-card rounded-xl p-4">
            <p className="text-xs text-graphite-600">Cartera vencida</p>
            <p className="mt-1 text-xl font-semibold tabular-nums text-graphite-100">
              {formatoUsd(indiceMorosidad?.carteraVencida ?? 0)}
            </p>
          </div>
          <div className="glass-card rounded-xl p-4">
            <p className="text-xs text-graphite-600">Índice de morosidad</p>
            <p className="mt-1 text-xl font-semibold tabular-nums text-graphite-100">
              {((indiceMorosidad?.indice ?? 0) * 100).toFixed(2)}%
            </p>
            <p className="mt-1 text-xs text-graphite-600">
              {indiceMorosidad?.prestamosVencidos ?? 0} de {indiceMorosidad?.totalPrestamos ?? 0} préstamos vigentes
            </p>
          </div>
        </div>
      )}

      {reporte === 'spi-no-procesados' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Cuota</Th>
              <Th>Monto</Th>
              <Th>Motivo</Th>
              <Th>Fecha</Th>
            </tr>
          </thead>
          <tbody>
            {(spiNoProcesados?.length ?? 0) === 0 && <EmptyState>Sin débitos SPI omitidos en el rango</EmptyState>}
            {spiNoProcesados?.map((d, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{d.numeroPrestamo}</Td>
                <Td>{d.numeroCuota ?? '—'}</Td>
                <Td className="tabular-nums">{formatoUsd(d.monto)}</Td>
                <Td>{d.motivo}</Td>
                <Td>{d.fecha}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'cartera-castigada' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Saldo transferido</Th>
              <Th>Fecha</Th>
              <Th>Comentario</Th>
              <Th>Registrado por</Th>
            </tr>
          </thead>
          <tbody>
            {(carteraCastigada?.length ?? 0) === 0 && <EmptyState>Sin castigos de cartera en el rango</EmptyState>}
            {carteraCastigada?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numeroPrestamo}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(c.saldoTransferido)}</Td>
                <Td>{c.fecha}</Td>
                <Td>{c.comentario}</Td>
                <Td>{c.registradoPor}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'calificacion' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Saldo</Th>
              <Th>Días mora</Th>
              <Th>Categoría</Th>
              <Th>% Provisión</Th>
              <Th>Provisión requerida</Th>
            </tr>
          </thead>
          <tbody>
            {(calificacion?.length ?? 0) === 0 && <EmptyState>Sin cartera vigente para calificar</EmptyState>}
            {calificacion?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numeroPrestamo}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(c.saldo)}</Td>
                <Td>{c.diasMora}</Td>
                <Td>
                  <Badge variant={c.diasMora > 90 ? 'peligro' : c.diasMora > 30 ? 'alerta' : 'neutral'}>
                    {c.codigoCategoria} — {c.categoria}
                  </Badge>
                </Td>
                <Td>{(c.porcentajeProvision * 100).toFixed(2)}%</Td>
                <Td className="tabular-nums">{formatoUsd(c.provisionRequerida)}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'gastos-judiciales' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Rubro</Th>
              <Th>Proyectado</Th>
              <Th>Cobrado</Th>
              <Th>Fecha</Th>
              <Th>Estado</Th>
            </tr>
          </thead>
          <tbody>
            {(gastosJudiciales?.length ?? 0) === 0 && <EmptyState>Sin gastos judiciales en el rango</EmptyState>}
            {gastosJudiciales?.map((g, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{g.numeroPrestamo}</Td>
                <Td>{g.producto}</Td>
                <Td>{g.cliente}</Td>
                <Td>{g.rubro}</Td>
                <Td className="tabular-nums">{formatoUsd(g.proyectado)}</Td>
                <Td className="tabular-nums">{formatoUsd(g.cobrado)}</Td>
                <Td>{g.fecha}</Td>
                <Td>{g.estado}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'consolidado-tipo-cartera' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Producto</Th>
              <Th>Cantidad préstamos</Th>
              <Th>Saldo total</Th>
              <Th>Tasa promedio</Th>
            </tr>
          </thead>
          <tbody>
            {(consolidadoTipoCartera?.length ?? 0) === 0 && <EmptyState>Sin cartera vigente</EmptyState>}
            {consolidadoTipoCartera?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.producto}</Td>
                <Td>{c.cantidadPrestamos}</Td>
                <Td className="tabular-nums">{formatoUsd(c.saldoTotal)}</Td>
                <Td>{(c.tasaPromedio * 100).toFixed(2)}%</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'seguro-desgravamen' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Saldo pendiente</Th>
            </tr>
          </thead>
          <tbody>
            {(seguroDesgravamen?.length ?? 0) === 0 && <EmptyState>Sin saldo pendiente de seguro desgravamen</EmptyState>}
            {seguroDesgravamen?.map((s, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{s.numeroPrestamo}</Td>
                <Td>{s.producto}</Td>
                <Td>{s.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(s.saldoPendiente)}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'castigada-agencia' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Agencia</Th>
              <Th>Cantidad de castigos</Th>
              <Th>Total castigado</Th>
            </tr>
          </thead>
          <tbody>
            {(castigadaAgencia?.length ?? 0) === 0 && <EmptyState>Sin castigos registrados</EmptyState>}
            {castigadaAgencia?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.agencia}</Td>
                <Td>{c.cantidadCastigos}</Td>
                <Td className="tabular-nums">{formatoUsd(c.totalCastigado)}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'castigada-cliente' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Socio</Th>
              <Th>Identificación</Th>
              <Th>Cantidad de castigos</Th>
              <Th>Total castigado</Th>
            </tr>
          </thead>
          <tbody>
            {(castigadaCliente?.length ?? 0) === 0 && <EmptyState>Sin castigos registrados</EmptyState>}
            {castigadaCliente?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.cliente}</Td>
                <Td>{c.identificacion}</Td>
                <Td>{c.cantidadCastigos}</Td>
                <Td className="tabular-nums">{formatoUsd(c.totalCastigado)}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'item-credito' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Cuota</Th>
              <Th>Rubro</Th>
              <Th>Proyectado</Th>
              <Th>Cobrado</Th>
              <Th>Estado</Th>
              <Th>Vencimiento</Th>
            </tr>
          </thead>
          <tbody>
            {(itemCredito?.length ?? 0) === 0 && <EmptyState>Sin rubros para este filtro</EmptyState>}
            {itemCredito?.map((r, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{r.numeroPrestamo}</Td>
                <Td>{r.numeroCuota}</Td>
                <Td>{r.rubro}</Td>
                <Td className="tabular-nums">{formatoUsd(r.proyectado)}</Td>
                <Td className="tabular-nums">{formatoUsd(r.cobrado)}</Td>
                <Td>
                  <Badge variant={r.estado === 'C' ? 'exito' : 'neutral'}>{r.estado === 'C' ? 'Cobrado' : 'Pendiente'}</Badge>
                </Td>
                <Td>{r.fechaVencimiento}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'vinculados' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Saldo</Th>
              <Th>Causa de vinculación</Th>
            </tr>
          </thead>
          <tbody>
            {(vinculados?.length ?? 0) === 0 && <EmptyState>Sin cartera vinculada</EmptyState>}
            {vinculados?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numeroPrestamo}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(c.saldo)}</Td>
                <Td title={c.causaVinculacion}>
                  <Badge variant="alerta">{c.codigoCausaVinculacion}</Badge>
                </Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'por-convenio' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Producto</Th>
              <Th>Socio</Th>
              <Th>Saldo</Th>
              <Th>Convenio</Th>
            </tr>
          </thead>
          <tbody>
            {(porConvenio?.length ?? 0) === 0 && <EmptyState>Sin préstamos con convenio asignado</EmptyState>}
            {porConvenio?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numeroPrestamo}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.cliente}</Td>
                <Td className="tabular-nums">{formatoUsd(c.saldo)}</Td>
                <Td>{c.convenio}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'abonos-convenio' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Préstamo</Th>
              <Th>Convenio</Th>
              <Th>Cuota</Th>
              <Th>Capital abonado</Th>
              <Th>Fecha</Th>
            </tr>
          </thead>
          <tbody>
            {(abonosConvenio?.length ?? 0) === 0 && <EmptyState>Sin abonos de convenio en el rango</EmptyState>}
            {abonosConvenio?.map((a, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{a.numeroPrestamo}</Td>
                <Td>{a.convenio}</Td>
                <Td>{a.numeroCuota}</Td>
                <Td className="tabular-nums">{formatoUsd(a.capital)}</Td>
                <Td>{a.fecha}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'entrega-recuperacion' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Mes</Th>
              <Th>Entregado (desembolsos)</Th>
              <Th>Recuperado (capital cobrado)</Th>
              <Th>Diferencia</Th>
            </tr>
          </thead>
          <tbody>
            {(entregaRecuperacion?.length ?? 0) === 0 && (
              <EmptyState>Sin entregas ni recuperaciones en el rango</EmptyState>
            )}
            {entregaRecuperacion?.map((e, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">
                  {e.anio}-{String(e.mes).padStart(2, '0')}
                </Td>
                <Td className="tabular-nums">{formatoUsd(e.entregado)}</Td>
                <Td className="tabular-nums">{formatoUsd(e.recuperado)}</Td>
                <Td className={`tabular-nums font-medium ${e.diferencia < 0 ? 'text-red-600' : 'text-emerald-600'}`}>
                  {formatoUsd(e.diferencia)}
                </Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}
    </div>
  )
}
