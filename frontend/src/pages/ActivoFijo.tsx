import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Package, Plus, X, ArrowLeftRight, Trash2, TrendingDown } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Estructura {
  id: number
  nombre: string
  seDeprecia: boolean
  porcentajeDepreciacionAnual: number
  esBienIntangible: boolean
  activo: boolean
}

interface Responsable {
  id: string
  nombre: string
  agencia: string
  activo: boolean
}

interface ActivoItem {
  id: string
  codigo: string | null
  categoria: string
  agencia: string
  detalle: string
  valor: number
  depreciacionAcumulada: number
  condicion: string
  estado: string
  responsable: string | null
}

interface MotivoTraslado {
  id: number
  nombre: string
  esDefinitivo: boolean
}

interface MotivoBaja {
  id: number
  detalle: string
  esDonacion: boolean
}

interface Traslado {
  id: string
  activoDetalle: string
  concepto: string
  fecha: string
  motivo: string
  agenciaOrigen: string | null
  agenciaDestino: string
  estado: string
}

interface SolicitudBaja {
  id: string
  activoDetalle: string
  motivo: string
  detalle: string | null
  fechaSolicitud: string
  estado: string
  idComprobante: string | null
}

interface Persona {
  id: string
  nombre: string
}

interface AgenciaItem {
  id: number
  nombre: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function estadoActivoVariant(estado: string) {
  if (estado === 'Baja' || estado === 'Anulado') return 'peligro' as const
  if (estado === 'Pendiente') return 'neutral' as const
  return 'exito' as const
}

// ---------- Responsables ----------

function SeccionResponsables() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [idPersona, setIdPersona] = useState('')
  const [idAgencia, setIdAgencia] = useState('1')

  const { data: personas } = useQuery<Persona[]>({
    queryKey: ['tesoreria-personas'],
    queryFn: async () => (await api.get('/api/tesoreria/cuentas-por-cobrar/personas')).data,
  })

  const { data: agencias } = useQuery<AgenciaItem[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const { data: responsables, isLoading } = useQuery<Responsable[]>({
    queryKey: ['activofijo-responsables'],
    queryFn: async () => (await api.get('/api/activofijo/responsables')).data,
  })

  const crear = useMutation({
    mutationFn: async () => api.post('/api/activofijo/responsables', { idPersona, idAgencia: Number(idAgencia) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activofijo-responsables'] })
      setMostrarForm(false)
      setIdPersona('')
    },
  })

  const toggleActivo = useMutation({
    mutationFn: async ({ id, activo }: { id: string; activo: boolean }) =>
      api.put(`/api/activofijo/responsables/${id}`, { idAgencia: agencias?.[0]?.id ?? 1, activo }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['activofijo-responsables'] }),
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
            <Plus size={16} /> Nuevo responsable
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="font-medium text-graphite-100">Nuevo responsable</h3>
            <button type="button" onClick={() => setMostrarForm(false)} className="text-graphite-600 hover:text-graphite-100">
              <X size={18} />
            </button>
          </div>
          <form
            className="flex flex-wrap items-end gap-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!idPersona) return
              crear.mutate()
            }}
          >
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Persona</span>
              <select
                required
                value={idPersona}
                onChange={(e) => setIdPersona(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar…</option>
                {personas?.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.nombre}
                  </option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Agencia</span>
              <select
                value={idAgencia}
                onChange={(e) => setIdAgencia(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              >
                {agencias?.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.nombre}
                  </option>
                ))}
              </select>
            </label>
            <button
              type="submit"
              disabled={crear.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {crear.isPending ? 'Creando…' : 'Crear'}
            </button>
            {crear.isError && (
              <p className="w-full text-sm text-red-700">
                {(crear.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
                  'No se pudo crear el responsable.'}
              </p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Nombre</Th>
            <Th>Agencia</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (responsables?.length ?? 0) === 0 && <EmptyState>Sin responsables registrados</EmptyState>}
          {responsables?.map((r) => (
            <tr key={r.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{r.nombre}</Td>
              <Td>{r.agencia}</Td>
              <Td>
                <button type="button" onClick={() => toggleActivo.mutate({ id: r.id, activo: !r.activo })}>
                  <Badge variant={r.activo ? 'exito' : 'neutral'}>{r.activo ? 'Activo' : 'Inactivo'}</Badge>
                </button>
              </Td>
              <Td></Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Activos ----------

function RegistrarActivoForm({
  estructuras,
  responsables,
  onClose,
}: {
  estructuras: Estructura[]
  responsables: Responsable[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idEstructura, setIdEstructura] = useState('')
  const [codigo, setCodigo] = useState('')
  const [detalle, setDetalle] = useState('')
  const [fechaCompra, setFechaCompra] = useState('')
  const [valor, setValor] = useState('')
  const [marca, setMarca] = useState('')
  const [modelo, setModelo] = useState('')
  const [serie, setSerie] = useState('')
  const [idResponsableInicial, setIdResponsableInicial] = useState('')

  const registrar = useMutation({
    mutationFn: async () =>
      api.post('/api/activofijo', {
        idEstructura: Number(idEstructura),
        idAgencia: 1,
        codigo: codigo || null,
        detalle,
        fechaCompra,
        valor: Number(valor),
        marca: marca || null,
        modelo: modelo || null,
        serie: serie || null,
        esVehiculo: false,
        esBienDeControl: false,
        asegurado: false,
        idResponsableInicial: idResponsableInicial || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activofijo-activos'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar activo fijo</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idEstructura || !detalle || !fechaCompra || !valor) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Categoría</span>
          <select
            required
            value={idEstructura}
            onChange={(e) => setIdEstructura(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {estructuras.map((e) => (
              <option key={e.id} value={e.id}>
                {e.nombre} {e.seDeprecia ? `(${e.porcentajeDepreciacionAnual}%/año)` : '(no deprecia)'}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Código / etiqueta</span>
          <input
            type="text"
            value={codigo}
            onChange={(e) => setCodigo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Fecha de compra</span>
          <input
            required
            type="date"
            max={new Date().toISOString().slice(0, 10)}
            value={fechaCompra}
            onChange={(e) => setFechaCompra(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-3">
          <span className="text-graphite-600">Detalle</span>
          <input
            required
            type="text"
            value={detalle}
            onChange={(e) => setDetalle(e.target.value)}
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

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Marca</span>
          <input
            type="text"
            value={marca}
            onChange={(e) => setMarca(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Modelo</span>
          <input
            type="text"
            value={modelo}
            onChange={(e) => setModelo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Serie</span>
          <input
            type="text"
            value={serie}
            onChange={(e) => setSerie(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Responsable inicial</span>
          <select
            value={idResponsableInicial}
            onChange={(e) => setIdResponsableInicial(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Sin asignar</option>
            {responsables.map((r) => (
              <option key={r.id} value={r.id}>
                {r.nombre}
              </option>
            ))}
          </select>
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
          <p className="sm:col-span-3 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar el activo.'}
          </p>
        )}
      </form>
    </div>
  )
}

function SeccionActivos() {
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: estructuras } = useQuery<Estructura[]>({
    queryKey: ['activofijo-estructuras'],
    queryFn: async () => (await api.get('/api/activofijo/estructuras')).data,
  })

  const { data: responsables } = useQuery<Responsable[]>({
    queryKey: ['activofijo-responsables'],
    queryFn: async () => (await api.get('/api/activofijo/responsables')).data,
  })

  const { data: activos, isLoading } = useQuery<ActivoItem[]>({
    queryKey: ['activofijo-activos'],
    queryFn: async () => (await api.get('/api/activofijo')).data,
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
            <Plus size={16} /> Registrar activo
          </button>
        )}
      </div>

      {mostrarForm && estructuras && responsables && (
        <RegistrarActivoForm estructuras={estructuras} responsables={responsables} onClose={() => setMostrarForm(false)} />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Categoría</Th>
            <Th>Detalle</Th>
            <Th>Valor</Th>
            <Th>Deprec. acum.</Th>
            <Th>Condición</Th>
            <Th>Responsable</Th>
            <Th>Estado</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (activos?.length ?? 0) === 0 && <EmptyState>Todavía no hay activos fijos registrados</EmptyState>}
          {activos?.map((a) => (
            <tr key={a.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{a.codigo ?? '—'}</Td>
              <Td>{a.categoria}</Td>
              <Td>{a.detalle}</Td>
              <Td className="tabular-nums">{formatoUsd(a.valor)}</Td>
              <Td className="tabular-nums">{formatoUsd(a.depreciacionAcumulada)}</Td>
              <Td>{a.condicion}</Td>
              <Td>{a.responsable ?? '—'}</Td>
              <Td>
                <Badge variant={estadoActivoVariant(a.estado)}>{a.estado}</Badge>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Depreciación ----------

interface DepreciacionResultado {
  fecha: string
  agenciasProcesadas: number
  activosDepreciados: number
  totalDepreciado: number
  idsComprobante: string[]
}

function SeccionDepreciacion() {
  const queryClient = useQueryClient()

  const ejecutar = useMutation({
    mutationFn: async () => (await api.post('/api/activofijo/depreciacion/ejecutar')).data as DepreciacionResultado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activofijo-activos'] })
    },
  })

  return (
    <div>
      <div className="glass-card rounded-xl p-5">
        <div className="mb-3 flex items-center gap-2">
          <TrendingDown size={18} className="text-gold-400" />
          <h3 className="font-medium text-graphite-100">Depreciación mensual de activos fijos</h3>
        </div>
        <p className="mb-4 text-sm text-graphite-600">
          Calcula la depreciación del mes en curso para todos los activos vigentes que deprecian, agrupados por
          agencia — un comprobante contable por agencia (débito gasto de depreciación / crédito depreciación
          acumulada, por categoría real). Correr esto dos veces el mismo mes no duplica nada.
        </p>
        <button
          type="button"
          disabled={ejecutar.isPending}
          onClick={() => ejecutar.mutate()}
          className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {ejecutar.isPending ? 'Calculando…' : 'Ejecutar depreciación del mes'}
        </button>

        {ejecutar.isSuccess && (
          <p className="mt-4 rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
            {ejecutar.data.activosDepreciados === 0
              ? 'No había activos pendientes de depreciar este mes.'
              : `${ejecutar.data.activosDepreciados} activo(s) depreciados en ${ejecutar.data.agenciasProcesadas} agencia(s) — total ${formatoUsd(ejecutar.data.totalDepreciado)}.`}
          </p>
        )}
        {ejecutar.isError && (
          <p className="mt-4 text-sm text-red-700">
            {(ejecutar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo ejecutar la depreciación.'}
          </p>
        )}
      </div>
    </div>
  )
}

// ---------- Traslados ----------

function CrearTrasladoForm({
  activos,
  responsables,
  motivos,
  onClose,
}: {
  activos: ActivoItem[]
  responsables: Responsable[]
  motivos: MotivoTraslado[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idActivo, setIdActivo] = useState('')
  const [concepto, setConcepto] = useState('')
  const [idMotivoTraslado, setIdMotivoTraslado] = useState('')
  const [razon, setRazon] = useState('')
  const [idResponsableDestino, setIdResponsableDestino] = useState('')

  const crear = useMutation({
    mutationFn: async () =>
      api.post(
        '/api/activofijo/traslados',
        {
          idActivo,
          concepto,
          idMotivoTraslado: Number(idMotivoTraslado),
          razon: razon || null,
          idResponsableDestino: idResponsableDestino || null,
          idAgenciaDestino: 1,
        },
        { headers: { 'Idempotency-Key': crypto.randomUUID() } },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activofijo-traslados'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nuevo traslado</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      <form
        className="grid grid-cols-1 gap-3 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idActivo || !concepto || !idMotivoTraslado) return
          crear.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Activo</span>
          <select
            required
            value={idActivo}
            onChange={(e) => setIdActivo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {activos
              .filter((a) => a.estado === 'Activo')
              .map((a) => (
                <option key={a.id} value={a.id}>
                  {a.codigo ?? a.detalle} — {a.categoria}
                </option>
              ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Concepto</span>
          <input
            required
            type="text"
            value={concepto}
            onChange={(e) => setConcepto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Motivo</span>
          <select
            required
            value={idMotivoTraslado}
            onChange={(e) => setIdMotivoTraslado(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {motivos.map((m) => (
              <option key={m.id} value={m.id}>
                {m.nombre}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Nuevo responsable</span>
          <select
            value={idResponsableDestino}
            onChange={(e) => setIdResponsableDestino(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Sin cambio</option>
            {responsables.map((r) => (
              <option key={r.id} value={r.id}>
                {r.nombre}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Razón (opcional)</span>
          <input
            type="text"
            value={razon}
            onChange={(e) => setRazon(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={crear.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {crear.isPending ? 'Creando…' : 'Crear traslado'}
          </button>
        </div>
        {crear.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(crear.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo crear el traslado.'}
          </p>
        )}
      </form>
    </div>
  )
}

function SeccionTraslados() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: activos } = useQuery<ActivoItem[]>({
    queryKey: ['activofijo-activos'],
    queryFn: async () => (await api.get('/api/activofijo')).data,
  })
  const { data: responsables } = useQuery<Responsable[]>({
    queryKey: ['activofijo-responsables'],
    queryFn: async () => (await api.get('/api/activofijo/responsables')).data,
  })
  const { data: motivos } = useQuery<MotivoTraslado[]>({
    queryKey: ['activofijo-motivos-traslado'],
    queryFn: async () => (await api.get('/api/activofijo/motivos-traslado')).data,
  })
  const { data: traslados, isLoading } = useQuery<Traslado[]>({
    queryKey: ['activofijo-traslados'],
    queryFn: async () => (await api.get('/api/activofijo/traslados')).data,
  })

  const procesar = useMutation({
    mutationFn: async (id: string) =>
      api.post(`/api/activofijo/traslados/${id}/procesar`, undefined, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activofijo-traslados'] })
      queryClient.invalidateQueries({ queryKey: ['activofijo-activos'] })
    },
  })

  const anular = useMutation({
    mutationFn: async (id: string) => api.post(`/api/activofijo/traslados/${id}/anular`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['activofijo-traslados'] }),
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
            <ArrowLeftRight size={16} /> Nuevo traslado
          </button>
        )}
      </div>

      {mostrarForm && activos && responsables && motivos && (
        <CrearTrasladoForm activos={activos} responsables={responsables} motivos={motivos} onClose={() => setMostrarForm(false)} />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Activo</Th>
            <Th>Concepto</Th>
            <Th>Motivo</Th>
            <Th>Destino</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (traslados?.length ?? 0) === 0 && <EmptyState>Sin traslados registrados</EmptyState>}
          {traslados?.map((t) => (
            <tr key={t.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{t.activoDetalle}</Td>
              <Td>{t.concepto}</Td>
              <Td>{t.motivo}</Td>
              <Td>{t.agenciaDestino}</Td>
              <Td>
                <Badge variant={t.estado === 'Procesado' ? 'exito' : t.estado === 'Anulado' ? 'peligro' : 'neutral'}>
                  {t.estado}
                </Badge>
              </Td>
              <Td>
                {t.estado === 'Pendiente' && (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => procesar.mutate(t.id)}
                      className="text-xs font-medium text-petrol-700 hover:underline"
                    >
                      Procesar
                    </button>
                    <button
                      type="button"
                      onClick={() => anular.mutate(t.id)}
                      className="text-xs font-medium text-red-700 hover:underline"
                    >
                      Anular
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

// ---------- Bajas ----------

function SolicitarBajaForm({
  activos,
  motivos,
  onClose,
}: {
  activos: ActivoItem[]
  motivos: MotivoBaja[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idActivo, setIdActivo] = useState('')
  const [idMotivoBaja, setIdMotivoBaja] = useState('')
  const [detalle, setDetalle] = useState('')

  const solicitar = useMutation({
    mutationFn: async () =>
      api.post('/api/activofijo/bajas', { idActivo, idMotivoBaja: Number(idMotivoBaja), detalle: detalle || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activofijo-bajas'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Solicitar baja de activo</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      <form
        className="grid grid-cols-1 gap-3 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idActivo || !idMotivoBaja) return
          solicitar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Activo</span>
          <select
            required
            value={idActivo}
            onChange={(e) => setIdActivo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {activos
              .filter((a) => a.estado === 'Activo')
              .map((a) => (
                <option key={a.id} value={a.id}>
                  {a.codigo ?? a.detalle} — {a.categoria} ({formatoUsd(a.valor - a.depreciacionAcumulada)} en libros)
                </option>
              ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Motivo</span>
          <select
            required
            value={idMotivoBaja}
            onChange={(e) => setIdMotivoBaja(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {motivos.map((m) => (
              <option key={m.id} value={m.id}>
                {m.detalle}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Detalle (opcional)</span>
          <input
            type="text"
            value={detalle}
            onChange={(e) => setDetalle(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={solicitar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {solicitar.isPending ? 'Enviando…' : 'Solicitar baja'}
          </button>
        </div>
        {solicitar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(solicitar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo solicitar la baja.'}
          </p>
        )}
      </form>
    </div>
  )
}

function SeccionBajas() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: activos } = useQuery<ActivoItem[]>({
    queryKey: ['activofijo-activos'],
    queryFn: async () => (await api.get('/api/activofijo')).data,
  })
  const { data: motivos } = useQuery<MotivoBaja[]>({
    queryKey: ['activofijo-motivos-baja'],
    queryFn: async () => (await api.get('/api/activofijo/motivos-baja')).data,
  })
  const { data: bajas, isLoading } = useQuery<SolicitudBaja[]>({
    queryKey: ['activofijo-bajas'],
    queryFn: async () => (await api.get('/api/activofijo/bajas')).data,
  })

  const autorizar = useMutation({
    mutationFn: async (id: string) => api.post(`/api/activofijo/bajas/${id}/autorizar`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['activofijo-bajas'] }),
  })

  const procesar = useMutation({
    mutationFn: async (id: string) =>
      api.post(`/api/activofijo/bajas/${id}/procesar`, undefined, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activofijo-bajas'] })
      queryClient.invalidateQueries({ queryKey: ['activofijo-activos'] })
    },
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
            <Trash2 size={16} /> Solicitar baja
          </button>
        )}
      </div>

      {mostrarForm && activos && motivos && (
        <SolicitarBajaForm activos={activos} motivos={motivos} onClose={() => setMostrarForm(false)} />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Activo</Th>
            <Th>Motivo</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (bajas?.length ?? 0) === 0 && <EmptyState>Sin solicitudes de baja</EmptyState>}
          {bajas?.map((b) => (
            <tr key={b.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{b.activoDetalle}</Td>
              <Td>{b.motivo}</Td>
              <Td>{b.fechaSolicitud}</Td>
              <Td>
                <Badge variant={b.estado === 'Procesado' ? 'exito' : b.estado === 'Anulado' ? 'peligro' : 'neutral'}>
                  {b.estado}
                </Badge>
              </Td>
              <Td>
                {b.estado === 'Ingresado' && (
                  <button
                    type="button"
                    onClick={() => autorizar.mutate(b.id)}
                    className="text-xs font-medium text-petrol-700 hover:underline"
                  >
                    Autorizar
                  </button>
                )}
                {b.estado === 'Autorizado' && (
                  <button
                    type="button"
                    onClick={() => procesar.mutate(b.id)}
                    className="text-xs font-medium text-gold-400 hover:underline"
                  >
                    Procesar baja
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

// ---------- Página principal ----------

const TABS = [
  { id: 'activos', label: 'Activos' },
  { id: 'depreciacion', label: 'Depreciación' },
  { id: 'traslados', label: 'Traslados' },
  { id: 'bajas', label: 'Bajas' },
  { id: 'responsables', label: 'Responsables' },
] as const

export function ActivoFijo() {
  const [tab, setTab] = useState<(typeof TABS)[number]['id']>('activos')

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Package} title="Activo Fijo" subtitle="Bienes, depreciación, traslados y bajas" />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.08]">
        {TABS.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`px-4 py-2 text-sm font-medium transition-colors ${
              tab === t.id ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'activos' && <SeccionActivos />}
      {tab === 'depreciacion' && <SeccionDepreciacion />}
      {tab === 'traslados' && <SeccionTraslados />}
      {tab === 'bajas' && <SeccionBajas />}
      {tab === 'responsables' && <SeccionResponsables />}
    </div>
  )
}
