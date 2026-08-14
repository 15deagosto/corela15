import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Landmark, Plus, X, Gauge, AlertTriangle, ShieldAlert } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
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
  socio: string
  producto: string
  montoSolicitado: number
  cuotas: number
  estado: string
  fechaSolicitud: string
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

  const { data: socios } = useQuery<Socio[]>({
    queryKey: ['socios', ''],
    queryFn: async () => (await api.get('/api/socios')).data,
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

function estadoVariant(estado: string) {
  if (estado === 'Desembolsada' || estado === 'Vigente') return 'exito' as const
  if (estado === 'Rechazada') return 'peligro' as const
  return 'alerta' as const
}

export function Creditos() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [mostrarFormDpf, setMostrarFormDpf] = useState(false)
  const queryClient = useQueryClient()

  const { data: productos } = useQuery<Producto[]>({
    queryKey: ['creditos-productos'],
    queryFn: async () => (await api.get('/api/creditos/productos')).data,
  })

  const { data: solicitudes, isLoading: cargandoSolicitudes } = useQuery<Solicitud[]>({
    queryKey: ['creditos-solicitudes'],
    queryFn: async () => (await api.get('/api/creditos/solicitudes')).data,
  })

  const { data: prestamos, isLoading: cargandoPrestamos } = useQuery<Prestamo[]>({
    queryKey: ['creditos-prestamos'],
    queryFn: async () => (await api.get('/api/creditos/prestamos')).data,
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

  const pagarCuota = useMutation({
    mutationFn: async (idPrestamo: string) =>
      (
        await api.post(`/api/creditos/prestamos/${idPrestamo}/pagos`, undefined, {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['creditos-prestamos'] })
    },
  })

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
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plazofijo-depositos'] })
    },
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={Landmark}
        title="Créditos"
        subtitle="Solicitudes, desembolsos y cartera de préstamos"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Nueva solicitud
            </button>
          )
        }
      />

      {mostrarForm && productos && <SolicitarForm productos={productos} onClose={() => setMostrarForm(false)} />}

      <h2 className="mb-2 text-sm font-semibold uppercase tracking-wide text-graphite-600">Solicitudes</h2>
      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Socio</Th>
            <Th>Producto</Th>
            <Th>Monto</Th>
            <Th>Cuotas</Th>
            <Th>Estado</Th>
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
              <Td>
                {s.estado === 'EnAnalisis' && (
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

      <h2 className="mb-2 mt-8 text-sm font-semibold uppercase tracking-wide text-graphite-600">Cartera de préstamos</h2>
      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Producto</Th>
            <Th>Desembolsado</Th>
            <Th>Saldo</Th>
            <Th>Tasa</Th>
            <Th>Estado</Th>
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
              <Td>
                {p.estado === 'Vigente' && (
                  <button
                    type="button"
                    disabled={pagarCuota.isPending}
                    onClick={() => pagarCuota.mutate(p.id)}
                    className="text-sm font-medium text-gold-400 hover:underline disabled:opacity-50"
                  >
                    Pagar cuota
                  </button>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
      {pagarCuota.isError && (
        <p className="mt-2 text-sm text-red-700">
          {(pagarCuota.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
            'No se pudo registrar el pago.'}
        </p>
      )}

      <div className="mb-2 mt-8 flex items-center justify-between">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-graphite-600">Plazo fijo</h2>
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
                  <button
                    type="button"
                    disabled={cancelarDpf.isPending}
                    onClick={() => cancelarDpf.mutate(d.id)}
                    className="text-sm font-medium text-gold-400 hover:underline disabled:opacity-50"
                  >
                    Cancelar
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
