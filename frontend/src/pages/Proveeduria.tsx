import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Boxes, Plus, X, PackagePlus } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Articulo {
  codigo: string
  nombre: string
  tipoArticulo: string
  marca: string | null
  multiplo: string
  activo: boolean
}

interface Usuario {
  id: string
  nombreUsuario: string
}

interface AgenciaItem {
  id: number
  nombre: string
}

interface Bodega {
  id: string
  agencia: string
  responsable: string
  activo: boolean
}

interface StockItem {
  id: string
  codigoArticulo: string
  nombreArticulo: string
  cantidad: number
  precioUnitario: number
  valorTotal: number
}

interface Solicitud {
  id: string
  bodega: string
  solicitante: string
  detalle: string | null
  estado: string
  fechaSistema: string
  valorEstimado: number
}

interface LineaSolicitud {
  codigoArticulo: string
  nombreArticulo: string
  cantidad: number
  precioUnitario: number
  detalle: string | null
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function estadoVariant(estado: string) {
  if (estado === 'Procesada') return 'exito' as const
  if (estado === 'Anulada') return 'peligro' as const
  return 'neutral' as const
}

// ---------- Bodegas ----------

function SeccionBodegas() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [idAgencia, setIdAgencia] = useState('1')
  const [idUsuarioResponsable, setIdUsuarioResponsable] = useState('')
  const [bodegaExpandida, setBodegaExpandida] = useState<string | null>(null)
  const [ingresarStockBodega, setIngresarStockBodega] = useState<Bodega | null>(null)

  const { data: usuarios } = useQuery<Usuario[]>({
    queryKey: ['proveeduria-usuarios'],
    queryFn: async () => (await api.get('/api/proveeduria/usuarios')).data,
  })

  const { data: agencias } = useQuery<AgenciaItem[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const { data: bodegas, isLoading } = useQuery<Bodega[]>({
    queryKey: ['proveeduria-bodegas'],
    queryFn: async () => (await api.get('/api/proveeduria/bodegas')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      api.post('/api/proveeduria/bodegas', { idAgencia: Number(idAgencia), idUsuarioResponsable }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['proveeduria-bodegas'] })
      setMostrarForm(false)
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
            <Plus size={16} /> Nueva bodega
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
          <div className="mb-4 flex items-center justify-between">
            <h3 className="font-medium text-graphite-100">Nueva bodega</h3>
            <button type="button" onClick={() => setMostrarForm(false)} className="text-graphite-600 hover:text-graphite-100">
              <X size={18} />
            </button>
          </div>
          <form
            className="flex flex-wrap items-end gap-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!idUsuarioResponsable) return
              crear.mutate()
            }}
          >
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
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Responsable</span>
              <select
                required
                value={idUsuarioResponsable}
                onChange={(e) => setIdUsuarioResponsable(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar…</option>
                {usuarios?.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.nombreUsuario}
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
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Agencia</Th>
            <Th>Responsable</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (bodegas?.length ?? 0) === 0 && <EmptyState>Sin bodegas registradas</EmptyState>}
          {bodegas?.map((b) => (
            <tr key={b.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{b.agencia}</Td>
              <Td>{b.responsable}</Td>
              <Td>
                <Badge variant={b.activo ? 'exito' : 'neutral'}>{b.activo ? 'Activa' : 'Inactiva'}</Badge>
              </Td>
              <Td>
                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={() => setBodegaExpandida(bodegaExpandida === b.id ? null : b.id)}
                    className="text-xs font-medium text-petrol-700 hover:underline"
                  >
                    Ver stock
                  </button>
                  <button
                    type="button"
                    onClick={() => setIngresarStockBodega(b)}
                    className="flex items-center gap-1 text-xs font-medium text-gold-400 hover:underline"
                  >
                    <PackagePlus size={13} /> Ingresar stock
                  </button>
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>

      {bodegaExpandida && <StockBodegaPanel idBodega={bodegaExpandida} />}
      {ingresarStockBodega && (
        <IngresarStockForm bodega={ingresarStockBodega} onClose={() => setIngresarStockBodega(null)} />
      )}
    </div>
  )
}

function StockBodegaPanel({ idBodega }: { idBodega: string }) {
  const { data: stock, isLoading } = useQuery<StockItem[]>({
    queryKey: ['proveeduria-stock', idBodega],
    queryFn: async () => (await api.get(`/api/proveeduria/bodegas/${idBodega}/stock`)).data,
  })

  return (
    <div className="glass-card animate-zoom-in mt-2 rounded-xl p-4">
      <h4 className="mb-3 text-sm font-medium text-graphite-100">Stock (kardex)</h4>
      <TableContainer>
        <thead>
          <tr>
            <Th>Artículo</Th>
            <Th>Cantidad</Th>
            <Th>Costo unitario</Th>
            <Th>Valor total</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (stock?.length ?? 0) === 0 && <EmptyState>Sin existencias</EmptyState>}
          {stock?.map((s) => (
            <tr key={s.id} className="border-b border-black/[0.04] last:border-0">
              <Td className="font-medium">{s.nombreArticulo}</Td>
              <Td className="tabular-nums">{s.cantidad}</Td>
              <Td className="tabular-nums">{formatoUsd(s.precioUnitario)}</Td>
              <Td className="tabular-nums">{formatoUsd(s.valorTotal)}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

function IngresarStockForm({ bodega, onClose }: { bodega: Bodega; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [codigoArticulo, setCodigoArticulo] = useState('')
  const [cantidad, setCantidad] = useState('')
  const [precioUnitario, setPrecioUnitario] = useState('')

  const { data: articulos } = useQuery<Articulo[]>({
    queryKey: ['proveeduria-articulos'],
    queryFn: async () => (await api.get('/api/proveeduria/articulos')).data,
  })

  const ingresar = useMutation({
    mutationFn: async () =>
      api.post(
        `/api/proveeduria/bodegas/${bodega.id}/ingresar-stock`,
        { codigoArticulo, cantidad: Number(cantidad), precioUnitario: Number(precioUnitario) },
        { headers: { 'Idempotency-Key': crypto.randomUUID() } },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['proveeduria-stock', bodega.id] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 mt-4 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Ingresar stock — {bodega.agencia}</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      <form
        className="grid grid-cols-1 gap-3 sm:grid-cols-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!codigoArticulo || !cantidad || !precioUnitario) return
          ingresar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-3">
          <span className="text-graphite-600">Artículo</span>
          <select
            required
            value={codigoArticulo}
            onChange={(e) => setCodigoArticulo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {articulos?.map((a) => (
              <option key={a.codigo} value={a.codigo}>
                {a.nombre} ({a.multiplo})
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Cantidad</span>
          <input
            required
            type="number"
            min="1"
            value={cantidad}
            onChange={(e) => setCantidad(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Costo unitario</span>
          <input
            required
            type="number"
            step="0.0001"
            min="0.0001"
            value={precioUnitario}
            onChange={(e) => setPrecioUnitario(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <div className="flex items-end">
          <button
            type="submit"
            disabled={ingresar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {ingresar.isPending ? 'Ingresando…' : 'Ingresar'}
          </button>
        </div>
        {ingresar.isError && (
          <p className="sm:col-span-3 text-sm text-red-700">
            {(ingresar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo ingresar el stock.'}
          </p>
        )}
      </form>
    </div>
  )
}

// ---------- Solicitudes de pedido ----------

function CrearSolicitudForm({
  bodegas,
  usuarios,
  articulos,
  onClose,
}: {
  bodegas: Bodega[]
  usuarios: Usuario[]
  articulos: Articulo[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idBodega, setIdBodega] = useState('')
  const [idUsuarioSolicitante, setIdUsuarioSolicitante] = useState('')
  const [detalle, setDetalle] = useState('')
  const [lineas, setLineas] = useState<{ codigoArticulo: string; cantidad: string }[]>([{ codigoArticulo: '', cantidad: '' }])

  const crear = useMutation({
    mutationFn: async () =>
      api.post('/api/proveeduria/solicitudes', {
        idBodega,
        idUsuarioSolicitante,
        detalle: detalle || null,
        lineas: lineas
          .filter((l) => l.codigoArticulo && l.cantidad)
          .map((l) => ({ codigoArticulo: l.codigoArticulo, cantidad: Number(l.cantidad), detalle: null })),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['proveeduria-solicitudes'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nueva solicitud de pedido</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idBodega || !idUsuarioSolicitante) return
          crear.mutate()
        }}
      >
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Bodega</span>
            <select
              required
              value={idBodega}
              onChange={(e) => setIdBodega(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {bodegas.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.agencia}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Solicitante</span>
            <select
              required
              value={idUsuarioSolicitante}
              onChange={(e) => setIdUsuarioSolicitante(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {usuarios.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.nombreUsuario}
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
        </div>

        <div className="flex flex-col gap-2">
          <span className="text-sm text-graphite-600">Artículos</span>
          {lineas.map((linea, i) => (
            <div key={i} className="flex items-center gap-2">
              <select
                value={linea.codigoArticulo}
                onChange={(e) => {
                  const nuevas = [...lineas]
                  nuevas[i] = { ...nuevas[i], codigoArticulo: e.target.value }
                  setLineas(nuevas)
                }}
                className="flex-1 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar artículo…</option>
                {articulos.map((a) => (
                  <option key={a.codigo} value={a.codigo}>
                    {a.nombre} ({a.multiplo})
                  </option>
                ))}
              </select>
              <input
                type="number"
                min="1"
                placeholder="Cantidad"
                value={linea.cantidad}
                onChange={(e) => {
                  const nuevas = [...lineas]
                  nuevas[i] = { ...nuevas[i], cantidad: e.target.value }
                  setLineas(nuevas)
                }}
                className="w-28 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
              {lineas.length > 1 && (
                <button
                  type="button"
                  onClick={() => setLineas(lineas.filter((_, idx) => idx !== i))}
                  className="text-graphite-600 hover:text-red-700"
                >
                  <X size={16} />
                </button>
              )}
            </div>
          ))}
          <button
            type="button"
            onClick={() => setLineas([...lineas, { codigoArticulo: '', cantidad: '' }])}
            className="w-fit text-xs font-medium text-petrol-700 hover:underline"
          >
            + Agregar artículo
          </button>
        </div>

        <div>
          <button
            type="submit"
            disabled={crear.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {crear.isPending ? 'Creando…' : 'Crear solicitud'}
          </button>
        </div>

        {crear.isError && (
          <p className="text-sm text-red-700">
            {(crear.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo crear la solicitud.'}
          </p>
        )}
      </form>
    </div>
  )
}

function LineasSolicitudPanel({ idSolicitud }: { idSolicitud: string }) {
  const { data: lineas, isLoading } = useQuery<LineaSolicitud[]>({
    queryKey: ['proveeduria-lineas', idSolicitud],
    queryFn: async () => (await api.get(`/api/proveeduria/solicitudes/${idSolicitud}/lineas`)).data,
  })

  return (
    <div className="glass-card animate-zoom-in mt-2 rounded-xl p-4">
      <TableContainer>
        <thead>
          <tr>
            <Th>Artículo</Th>
            <Th>Cantidad</Th>
            <Th>Costo unitario</Th>
            <Th>Subtotal</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {lineas?.map((l, i) => (
            <tr key={i} className="border-b border-black/[0.04] last:border-0">
              <Td className="font-medium">{l.nombreArticulo}</Td>
              <Td className="tabular-nums">{l.cantidad}</Td>
              <Td className="tabular-nums">{formatoUsd(l.precioUnitario)}</Td>
              <Td className="tabular-nums">{formatoUsd(l.cantidad * l.precioUnitario)}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

function SeccionSolicitudes() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [solicitudExpandida, setSolicitudExpandida] = useState<string | null>(null)

  const { data: bodegas } = useQuery<Bodega[]>({
    queryKey: ['proveeduria-bodegas'],
    queryFn: async () => (await api.get('/api/proveeduria/bodegas')).data,
  })
  const { data: usuarios } = useQuery<Usuario[]>({
    queryKey: ['proveeduria-usuarios'],
    queryFn: async () => (await api.get('/api/proveeduria/usuarios')).data,
  })
  const { data: articulos } = useQuery<Articulo[]>({
    queryKey: ['proveeduria-articulos'],
    queryFn: async () => (await api.get('/api/proveeduria/articulos')).data,
  })
  const { data: solicitudes, isLoading } = useQuery<Solicitud[]>({
    queryKey: ['proveeduria-solicitudes'],
    queryFn: async () => (await api.get('/api/proveeduria/solicitudes')).data,
  })

  const procesar = useMutation({
    mutationFn: async (id: string) =>
      api.post(`/api/proveeduria/solicitudes/${id}/procesar`, undefined, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['proveeduria-solicitudes'] })
      queryClient.invalidateQueries({ queryKey: ['proveeduria-stock'] })
    },
  })

  const anular = useMutation({
    mutationFn: async (id: string) => api.post(`/api/proveeduria/solicitudes/${id}/anular`, { comentario: null }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['proveeduria-solicitudes'] }),
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
            <Plus size={16} /> Nueva solicitud
          </button>
        )}
      </div>

      {mostrarForm && bodegas && usuarios && articulos && (
        <CrearSolicitudForm bodegas={bodegas} usuarios={usuarios} articulos={articulos} onClose={() => setMostrarForm(false)} />
      )}

      {procesar.isError && (
        <p className="mb-4 text-sm text-red-700">
          {(procesar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
            'No se pudo procesar la solicitud.'}
        </p>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Bodega</Th>
            <Th>Solicitante</Th>
            <Th>Detalle</Th>
            <Th>Valor estimado</Th>
            <Th>Estado</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (solicitudes?.length ?? 0) === 0 && <EmptyState>Todavía no hay solicitudes de pedido</EmptyState>}
          {solicitudes?.map((s) => (
            <tr key={s.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{s.bodega}</Td>
              <Td>{s.solicitante}</Td>
              <Td>{s.detalle ?? '—'}</Td>
              <Td className="tabular-nums">{formatoUsd(s.valorEstimado)}</Td>
              <Td>
                <Badge variant={estadoVariant(s.estado)}>{s.estado}</Badge>
              </Td>
              <Td>
                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={() => setSolicitudExpandida(solicitudExpandida === s.id ? null : s.id)}
                    className="text-xs font-medium text-petrol-700 hover:underline"
                  >
                    Ver líneas
                  </button>
                  {s.estado === 'Ingresada' && (
                    <>
                      <button
                        type="button"
                        disabled={procesar.isPending}
                        onClick={() => procesar.mutate(s.id)}
                        className="text-xs font-medium text-gold-400 hover:underline disabled:opacity-50"
                      >
                        Procesar
                      </button>
                      <button
                        type="button"
                        onClick={() => anular.mutate(s.id)}
                        className="text-xs font-medium text-red-700 hover:underline"
                      >
                        Anular
                      </button>
                    </>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>

      {solicitudExpandida && <LineasSolicitudPanel idSolicitud={solicitudExpandida} />}
    </div>
  )
}

// ---------- Página principal ----------

export function Proveeduria() {
  const [tab, setTab] = useState<'solicitudes' | 'bodegas'>('solicitudes')

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Boxes} title="Proveeduría" subtitle="Bodega de suministros — stock, solicitudes de pedido y consumo" />

      <div className="mb-6 flex gap-1 border-b border-black/[0.08]">
        <button
          type="button"
          onClick={() => setTab('solicitudes')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            tab === 'solicitudes' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
          }`}
        >
          Solicitudes de pedido
        </button>
        <button
          type="button"
          onClick={() => setTab('bodegas')}
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            tab === 'bodegas' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
          }`}
        >
          Bodegas
        </button>
      </div>

      {tab === 'solicitudes' ? <SeccionSolicitudes /> : <SeccionBodegas />}
    </div>
  )
}
