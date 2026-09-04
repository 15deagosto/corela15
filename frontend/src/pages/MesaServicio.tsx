import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { LifeBuoy, Plus, X, MessageSquare, History, Info, UserCheck, Star, Clock } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { useAuth } from '../lib/AuthContext'
import { api } from '../lib/api'

interface Catalogo {
  codigo: string
  nombre: string
}

interface AgenteItem {
  id: string
  nombreUsuario: string
}

interface TicketListItem {
  id: string
  numero: string
  titulo: string
  categoria: string
  prioridad: string
  codigoEstado: string
  estado: string
  agencia: string
  usuarioAsignado: string | null
  fechaLimiteSla: string
  vencidoSla: boolean
  calificacion: number | null
  creadoEn: string
  creadoPor: string
}

interface TicketDto {
  id: string
  numero: string
  titulo: string
  descripcion: string
  codigoCategoria: string
  categoria: string
  codigoPrioridad: string
  prioridad: string
  codigoEstado: string
  estado: string
  agencia: string
  usuarioAsignado: string | null
  fechaLimiteSla: string
  vencidoSla: boolean
  fechaCierre: string | null
  fechaPrimeraRespuesta: string | null
  tiempoPrimeraRespuestaHoras: number | null
  tiempoResolucionHoras: number | null
  calificacion: number | null
  comentarioCalificacion: string | null
  creadoEn: string
  creadoPor: string
}

interface TicketComentario {
  comentario: string
  registradoPor: string
  fecha: string
}

interface TicketEtapa {
  estadoAnterior: string
  estadoNuevo: string
  comentario: string | null
  registradoPor: string
  fecha: string
  horasEnEstadoAnterior: number | null
}

interface TicketDetalle {
  ticket: TicketDto
  comentarios: TicketComentario[]
  bitacora: TicketEtapa[]
}

const BASE = '/api/mesa-servicio'
const INPUT_CLASS =
  'w-full rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50'

function estadoVariant(codigo: string): 'exito' | 'peligro' | 'neutral' | 'alerta' {
  if (codigo === 'CANCELADO') return 'peligro'
  if (codigo === 'CERRADO' || codigo === 'RESUELTO') return 'neutral'
  if (codigo === 'ABIERTO') return 'alerta'
  return 'exito'
}

function formatoFecha(iso: string) {
  return new Date(iso).toLocaleString('es-EC', { dateStyle: 'short', timeStyle: 'short' })
}

function formatoHoras(horas: number | null) {
  if (horas === null) return '—'
  if (horas < 1) return `${Math.round(horas * 60)} min`
  if (horas < 48) return `${horas.toFixed(1)} h`
  return `${(horas / 24).toFixed(1)} días`
}

/** Resuelve un código de estado (ej. "EN_PROGRESO") al nombre real del catálogo (ej. "En Progreso") — cae al código formateado si el catálogo todavía no cargó. */
function nombreEstado(estados: Catalogo[] | undefined, codigo: string) {
  return estados?.find((e) => e.codigo === codigo)?.nombre ?? codigo.replaceAll('_', ' ')
}

function Estrellas({ valor }: { valor: number }) {
  return (
    <div className="flex items-center gap-0.5">
      {[1, 2, 3, 4, 5].map((n) => (
        <Star key={n} size={13} className={n <= valor ? 'fill-gold-500 text-gold-500' : 'text-graphite-700'} />
      ))}
    </div>
  )
}

export function MesaServicio() {
  const queryClient = useQueryClient()
  const { sesion, tieneMenu } = useAuth()
  const esAgente = tieneMenu('mesa-servicio-agente')
  const [soloMios, setSoloMios] = useState(false)
  // Sin permiso de agente, por defecto se ve solo lo que uno mismo reportó
  // — antes no había forma de filtrar esto, un usuario normal veía todos
  // los tickets de toda la cooperativa mezclados en una sola lista.
  const [misTickets, setMisTickets] = useState(!esAgente)
  const [filtroEstado, setFiltroEstado] = useState('')
  const [modalNuevo, setModalNuevo] = useState(false)
  const [ticketAbierto, setTicketAbierto] = useState<string | null>(null)

  const { data: tickets, isLoading } = useQuery<TicketListItem[]>({
    queryKey: ['mesa-servicio-tickets', filtroEstado, soloMios, misTickets],
    queryFn: async () =>
      (
        await api.get(`${BASE}/tickets`, {
          params: {
            codigoEstado: filtroEstado || undefined,
            soloMios: soloMios || undefined,
            misTickets: misTickets || undefined,
          },
        })
      ).data,
  })

  const { data: estados } = useQuery<Catalogo[]>({
    queryKey: ['mesa-servicio-estados'],
    queryFn: async () => (await api.get(`${BASE}/estados`)).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={LifeBuoy}
        title="Mesa de Servicio"
        subtitle={
          esAgente
            ? 'Control de incidencias — tenés permiso de agente: podés tomar, reasignar y resolver tickets'
            : 'Control de incidencias — reportá una incidencia y seguí su avance en tiempo real'
        }
      />

      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2">
          <select value={filtroEstado} onChange={(e) => setFiltroEstado(e.target.value)} className={`${INPUT_CLASS} w-auto`}>
            <option value="">Todos los estados</option>
            {estados?.map((e) => (
              <option key={e.codigo} value={e.codigo}>
                {e.nombre}
              </option>
            ))}
          </select>
          {esAgente ? (
            <label className="flex items-center gap-1.5 text-sm text-graphite-600">
              <input type="checkbox" checked={soloMios} onChange={(e) => setSoloMios(e.target.checked)} />
              Solo asignados a mí
            </label>
          ) : (
            <label className="flex items-center gap-1.5 text-sm text-graphite-600">
              <input type="checkbox" checked={misTickets} onChange={(e) => setMisTickets(e.target.checked)} />
              Ver solo lo que yo reporté
            </label>
          )}
        </div>
        <button
          type="button"
          onClick={() => setModalNuevo(true)}
          className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
        >
          <Plus size={15} /> Nuevo ticket
        </button>
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Título</Th>
            <Th>Categoría</Th>
            <Th>Prioridad</Th>
            <Th>Estado</Th>
            <Th>Asignado</Th>
            <Th>Debería resolverse</Th>
            <Th>Calificación</Th>
            <Th>Creado</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <EmptyState>Cargando...</EmptyState>
          ) : !tickets || tickets.length === 0 ? (
            <EmptyState>No hay tickets que coincidan con el filtro.</EmptyState>
          ) : (
            tickets.map((t) => (
              <tr
                key={t.id}
                className="cursor-pointer border-t border-black/[0.04] hover:bg-black/[0.015]"
                onDoubleClick={() => setTicketAbierto(t.id)}
                title="Doble clic para ver el detalle"
              >
                <Td>{t.numero}</Td>
                <Td>{t.titulo}</Td>
                <Td>{t.categoria}</Td>
                <Td>{t.prioridad}</Td>
                <Td>
                  <Badge variant={estadoVariant(t.codigoEstado)}>{t.estado}</Badge>
                </Td>
                <Td>{t.usuarioAsignado ?? '—'}</Td>
                <Td>
                  {t.vencidoSla ? (
                    <Badge variant="peligro">Vencido</Badge>
                  ) : (
                    formatoFecha(t.fechaLimiteSla)
                  )}
                </Td>
                <Td>{t.calificacion ? <Estrellas valor={t.calificacion} /> : '—'}</Td>
                <Td title={t.creadoPor}>{formatoFecha(t.creadoEn)}</Td>
              </tr>
            ))
          )}
        </tbody>
      </TableContainer>

      {modalNuevo && (
        <ModalPortal>
          <NuevoTicketModal
            esAgente={esAgente}
            onClose={() => setModalNuevo(false)}
            onCreado={(id) => {
              setModalNuevo(false)
              queryClient.invalidateQueries({ queryKey: ['mesa-servicio-tickets'] })
              setTicketAbierto(id)
            }}
          />
        </ModalPortal>
      )}

      {ticketAbierto && (
        <ModalPortal>
          <TicketModal
            idTicket={ticketAbierto}
            esAgente={esAgente}
            usuarioActual={sesion?.nombreUsuario}
            onClose={() => {
              setTicketAbierto(null)
              queryClient.invalidateQueries({ queryKey: ['mesa-servicio-tickets'] })
            }}
          />
        </ModalPortal>
      )}
    </div>
  )
}

function NuevoTicketModal({
  esAgente,
  onClose,
  onCreado,
}: {
  esAgente: boolean
  onClose: () => void
  onCreado: (id: string) => void
}) {
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState({
    titulo: '',
    descripcion: '',
    codigoCategoria: '',
    codigoPrioridad: '',
    idAgencia: 1,
    idUsuarioAsignado: '' as string,
  })

  const { data: categorias } = useQuery<Catalogo[]>({
    queryKey: ['mesa-servicio-categorias'],
    queryFn: async () => (await api.get(`${BASE}/categorias`)).data,
  })
  const { data: prioridades } = useQuery<(Catalogo & { horasSla: number })[]>({
    queryKey: ['mesa-servicio-prioridades'],
    queryFn: async () => (await api.get(`${BASE}/prioridades`)).data,
  })
  const { data: agentes } = useQuery<AgenteItem[]>({
    queryKey: ['mesa-servicio-agentes'],
    queryFn: async () => (await api.get(`${BASE}/agentes`)).data,
    enabled: esAgente,
  })

  const crear = useMutation({
    mutationFn: async () => {
      const resp = await api.post(`${BASE}/tickets`, {
        ...form,
        idUsuarioAsignado: form.idUsuarioAsignado || null,
      })
      return resp.data as TicketDto
    },
    onSuccess: (data) => onCreado(data.id),
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card flex max-h-[90vh] w-full max-w-lg flex-col overflow-hidden rounded-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-black/[0.06] p-5 pb-4">
          <h2 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
            <LifeBuoy size={16} /> Nuevo ticket
          </h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <div className="flex-1 space-y-3 overflow-y-auto p-5">
          {error && <p className="rounded-lg bg-red-500/10 px-3 py-2 text-xs text-red-600">{error}</p>}

          <div>
            <label className="mb-1 block text-xs font-medium text-graphite-600">Título</label>
            <input value={form.titulo} onChange={(e) => setForm((f) => ({ ...f, titulo: e.target.value }))} className={INPUT_CLASS} />
          </div>
          <div>
            <label className="mb-1 block text-xs font-medium text-graphite-600">Descripción</label>
            <textarea
              rows={4}
              value={form.descripcion}
              onChange={(e) => setForm((f) => ({ ...f, descripcion: e.target.value }))}
              className={INPUT_CLASS}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-graphite-600">Categoría</label>
              <select
                value={form.codigoCategoria}
                onChange={(e) => setForm((f) => ({ ...f, codigoCategoria: e.target.value }))}
                className={INPUT_CLASS}
              >
                <option value="">Seleccionar...</option>
                {categorias?.map((c) => (
                  <option key={c.codigo} value={c.codigo}>
                    {c.nombre}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="mb-1 block text-xs font-medium text-graphite-600">Prioridad</label>
              <select
                value={form.codigoPrioridad}
                onChange={(e) => setForm((f) => ({ ...f, codigoPrioridad: e.target.value }))}
                className={INPUT_CLASS}
              >
                <option value="">Seleccionar...</option>
                {prioridades?.map((p) => (
                  <option key={p.codigo} value={p.codigo}>
                    {p.nombre} — respuesta en {p.horasSla} h
                  </option>
                ))}
              </select>
            </div>
          </div>
          {esAgente && (
            <div>
              <label className="mb-1 block text-xs font-medium text-graphite-600">Asignar a un agente (opcional)</label>
              <select
                value={form.idUsuarioAsignado}
                onChange={(e) => setForm((f) => ({ ...f, idUsuarioAsignado: e.target.value }))}
                className={INPUT_CLASS}
              >
                <option value="">Sin asignar — queda disponible para que cualquier agente lo tome</option>
                {agentes?.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.nombreUsuario}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>

        <div className="flex justify-end gap-2 border-t border-black/[0.06] p-4">
          <button type="button" onClick={onClose} className="rounded-lg px-3 py-2 text-sm text-graphite-600">
            Cancelar
          </button>
          <button
            type="button"
            onClick={() => crear.mutate()}
            disabled={crear.isPending || !form.titulo || !form.descripcion || !form.codigoCategoria || !form.codigoPrioridad}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            Registrar
          </button>
        </div>
      </div>
    </div>
  )
}

const TABS_TICKET = [
  { id: 'detalle', label: 'Detalle', icon: Info },
  { id: 'comentarios', label: 'Comentarios', icon: MessageSquare },
  { id: 'bitacora', label: 'Historial', icon: History },
] as const

function TicketModal({
  idTicket,
  esAgente,
  usuarioActual,
  onClose,
}: {
  idTicket: string
  esAgente: boolean
  usuarioActual?: string
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<(typeof TABS_TICKET)[number]['id']>('detalle')
  const [comentario, setComentario] = useState('')
  const [nuevoEstado, setNuevoEstado] = useState('')
  const [comentarioEstado, setComentarioEstado] = useState('')
  const [calificacionForm, setCalificacionForm] = useState(0)
  const [comentarioCalificacion, setComentarioCalificacion] = useState('')
  const [error, setError] = useState<string | null>(null)

  const { data: detalle, refetch } = useQuery<TicketDetalle>({
    queryKey: ['mesa-servicio-ticket', idTicket],
    queryFn: async () => (await api.get(`${BASE}/tickets/${idTicket}`)).data,
  })
  const { data: estados } = useQuery<Catalogo[]>({
    queryKey: ['mesa-servicio-estados'],
    queryFn: async () => (await api.get(`${BASE}/estados`)).data,
  })
  const { data: agentes } = useQuery<AgenteItem[]>({
    queryKey: ['mesa-servicio-agentes'],
    queryFn: async () => (await api.get(`${BASE}/agentes`)).data,
    enabled: esAgente,
  })

  const invalidarTodo = () => {
    refetch()
    queryClient.invalidateQueries({ queryKey: ['mesa-servicio-tickets'] })
  }

  const comentar = useMutation({
    mutationFn: async () => api.post(`${BASE}/tickets/${idTicket}/comentarios`, { comentario }),
    onSuccess: () => {
      setComentario('')
      refetch()
    },
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  const cambiarEstado = useMutation({
    mutationFn: async () =>
      api.post(`${BASE}/tickets/${idTicket}/estado`, { codigoEstado: nuevoEstado, comentario: comentarioEstado || null }),
    onSuccess: () => {
      setNuevoEstado('')
      setComentarioEstado('')
      invalidarTodo()
    },
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  const asignar = useMutation({
    mutationFn: async (idUsuarioAsignado: string | null) => api.post(`${BASE}/tickets/${idTicket}/asignar`, { idUsuarioAsignado }),
    onSuccess: invalidarTodo,
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  const tomar = useMutation({
    mutationFn: async () => api.post(`${BASE}/tickets/${idTicket}/tomar`),
    onSuccess: invalidarTodo,
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  const calificar = useMutation({
    mutationFn: async () =>
      api.post(`${BASE}/tickets/${idTicket}/calificar`, {
        calificacion: calificacionForm,
        comentario: comentarioCalificacion || null,
      }),
    onSuccess: invalidarTodo,
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  if (!detalle) return null
  const { ticket } = detalle
  const cerrado = ticket.codigoEstado === 'CERRADO' || ticket.codigoEstado === 'CANCELADO'
  const calificable = ['RESUELTO', 'CERRADO'].includes(ticket.codigoEstado)
  const esCreador = usuarioActual && usuarioActual.toLowerCase() === ticket.creadoPor.toLowerCase()
  const puedeCalificar = esCreador && calificable && !ticket.calificacion

  // Quién lo resolvió y qué dijo — la última entrada de la bitácora que
  // llevó al ticket a Resuelto/Cerrado. Se calcula acá, del lado del
  // cliente, con datos que ya vienen en el detalle — no hace falta un
  // campo nuevo del backend. Es lo primero que un usuario normal quiere
  // ver, así que va destacado arriba de todo, no escondido en la pestaña
  // de bitácora técnica.
  const entradaResolucion = [...detalle.bitacora]
    .reverse()
    .find((h) => h.estadoNuevo === 'RESUELTO' || h.estadoNuevo === 'CERRADO')

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden rounded-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-black/[0.06] p-5 pb-4">
          <h2 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
            <LifeBuoy size={16} /> {ticket.numero} — {ticket.titulo}
          </h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <div className="flex flex-1 overflow-hidden">
          <div className="flex w-44 flex-shrink-0 flex-col gap-0.5 border-r border-black/[0.06] p-2">
            {TABS_TICKET.map((t) => (
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

          <div className="flex-1 space-y-4 overflow-y-auto p-5">
            {error && <p className="rounded-lg bg-red-500/10 px-3 py-2 text-xs text-red-600">{error}</p>}

            {tab === 'detalle' && (
              <>
                <div className="flex flex-wrap items-center gap-2">
                  <Badge variant={estadoVariant(ticket.codigoEstado)}>{ticket.estado}</Badge>
                  <span className="text-xs text-graphite-600">{ticket.categoria}</span>
                  <span className="text-xs text-graphite-600">· Prioridad {ticket.prioridad}</span>
                  <span className="text-xs text-graphite-600">· Agencia {ticket.agencia}</span>
                </div>
                <p className="whitespace-pre-wrap text-sm text-graphite-100">{ticket.descripcion}</p>

                {entradaResolucion && (
                  <div className="rounded-lg border border-emerald-500/30 bg-emerald-500/5 p-3">
                    <p className="flex items-center gap-1.5 text-sm font-medium text-emerald-700">
                      <UserCheck size={14} /> Resuelto por {entradaResolucion.registradoPor}
                    </p>
                    <p className="mt-0.5 text-xs text-graphite-600">{formatoFecha(entradaResolucion.fecha)}</p>
                    {entradaResolucion.comentario ? (
                      <p className="mt-2 text-sm text-graphite-100">"{entradaResolucion.comentario}"</p>
                    ) : (
                      <p className="mt-2 text-xs italic text-graphite-600">
                        No se dejó un comentario de qué se hizo — podés preguntar en Comentarios si necesitás más detalle.
                      </p>
                    )}
                  </div>
                )}

                <div className="grid grid-cols-2 gap-3 text-xs text-graphite-600">
                  <div>
                    <p className="font-medium text-graphite-600">Creado por</p>
                    <p className="text-graphite-100">{ticket.creadoPor}</p>
                  </div>
                  <div>
                    <p className="font-medium text-graphite-600">Asignado a</p>
                    <p className="text-graphite-100">{ticket.usuarioAsignado ?? '—'}</p>
                  </div>
                  <div>
                    <p className="font-medium text-graphite-600">Debería resolverse antes de</p>
                    <p className="text-graphite-100">
                      {formatoFecha(ticket.fechaLimiteSla)}
                      {ticket.vencidoSla && <Badge variant="peligro"> Vencido</Badge>}
                    </p>
                  </div>
                  <div>
                    <p className="font-medium text-graphite-600">Cierre</p>
                    <p className="text-graphite-100">{ticket.fechaCierre ? formatoFecha(ticket.fechaCierre) : '—'}</p>
                  </div>
                </div>

                {/* Sin permiso de agente: solo lectura, ni reasignar ni cambiar estado — la mesa de servicio lo trabaja, no cualquiera. */}
                {!esAgente && !cerrado && (
                  <p className="rounded-lg border border-black/[0.06] bg-black/[0.015] p-3 text-xs text-graphite-600">
                    Este ticket lo trabaja la mesa de servicio — vas a poder verlo avanzar acá y calificarlo cuando se
                    resuelva, sin necesitar hacer nada más por ahora.
                  </p>
                )}

                {esAgente && !cerrado && (
                  <>
                    <div className="flex items-end gap-2">
                      <div className="flex-1">
                        <label className="mb-1 block text-xs font-medium text-graphite-600">Reasignar a otro agente</label>
                        <select
                          value=""
                          onChange={(e) => {
                            if (!e.target.value) return
                            asignar.mutate(e.target.value === 'NONE' ? null : e.target.value)
                          }}
                          className={INPUT_CLASS}
                        >
                          <option value="" disabled>
                            Seleccionar...
                          </option>
                          <option value="NONE">Sin asignar</option>
                          {agentes?.map((a) => (
                            <option key={a.id} value={a.id}>
                              {a.nombreUsuario}
                            </option>
                          ))}
                        </select>
                      </div>
                      {ticket.usuarioAsignado === null && (
                        <button
                          type="button"
                          onClick={() => tomar.mutate()}
                          disabled={tomar.isPending}
                          className="btn-hover flex flex-shrink-0 items-center gap-1.5 rounded-lg border border-gold-500/40 px-3 py-2 text-xs font-medium text-gold-300 hover:bg-gold-500/10 disabled:opacity-50"
                        >
                          <UserCheck size={13} /> Tomar
                        </button>
                      )}
                    </div>

                    <div className="border-t border-black/[0.06] pt-4">
                      <label className="mb-1 block text-xs font-medium text-graphite-600">Cambiar estado</label>
                      <div className="space-y-2">
                        <select value={nuevoEstado} onChange={(e) => setNuevoEstado(e.target.value)} className={INPUT_CLASS}>
                          <option value="">Seleccionar estado...</option>
                          {estados?.filter((e) => e.codigo !== ticket.codigoEstado).map((e) => (
                            <option key={e.codigo} value={e.codigo}>
                              {e.nombre}
                            </option>
                          ))}
                        </select>
                        <input
                          placeholder="Comentario (opcional)"
                          value={comentarioEstado}
                          onChange={(e) => setComentarioEstado(e.target.value)}
                          className={INPUT_CLASS}
                        />
                        <button
                          type="button"
                          disabled={!nuevoEstado || cambiarEstado.isPending}
                          onClick={() => cambiarEstado.mutate()}
                          className="btn-hover rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white disabled:opacity-50"
                        >
                          Guardar estado
                        </button>
                      </div>
                    </div>
                  </>
                )}

                {cerrado && (
                  <p className="border-t border-black/[0.06] pt-4 text-xs text-graphite-600">
                    Este ticket ya está {ticket.estado.toLowerCase()} — no admite más cambios.
                  </p>
                )}

                {/* Calificación — solo la ve/completa quien reportó, solo cuando ya está resuelto. */}
                {(ticket.calificacion || puedeCalificar) && (
                  <div className="border-t border-black/[0.06] pt-4">
                    <label className="mb-2 block text-xs font-medium text-graphite-600">
                      {ticket.calificacion ? 'Calificación de la atención' : '¿Cómo calificás la atención recibida?'}
                    </label>
                    {ticket.calificacion ? (
                      <div className="space-y-1">
                        <Estrellas valor={ticket.calificacion} />
                        {ticket.comentarioCalificacion && (
                          <p className="text-xs text-graphite-600">"{ticket.comentarioCalificacion}"</p>
                        )}
                      </div>
                    ) : (
                      <div className="space-y-2">
                        <div className="flex items-center gap-1">
                          {[1, 2, 3, 4, 5].map((n) => (
                            <button key={n} type="button" onClick={() => setCalificacionForm(n)}>
                              <Star
                                size={22}
                                className={n <= calificacionForm ? 'fill-gold-500 text-gold-500' : 'text-graphite-700'}
                              />
                            </button>
                          ))}
                        </div>
                        <input
                          placeholder="Comentario (opcional)"
                          value={comentarioCalificacion}
                          onChange={(e) => setComentarioCalificacion(e.target.value)}
                          className={INPUT_CLASS}
                        />
                        <button
                          type="button"
                          disabled={!calificacionForm || calificar.isPending}
                          onClick={() => calificar.mutate()}
                          className="btn-hover rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white disabled:opacity-50"
                        >
                          Enviar calificación
                        </button>
                      </div>
                    )}
                  </div>
                )}
              </>
            )}

            {tab === 'comentarios' && (
              <>
                <div className="space-y-3">
                  {detalle.comentarios.length === 0 ? (
                    <p className="text-xs text-graphite-600">Sin comentarios todavía.</p>
                  ) : (
                    detalle.comentarios.map((c, i) => (
                      <div key={i} className="rounded-lg border border-black/[0.06] p-3">
                        <p className="text-sm text-graphite-100">{c.comentario}</p>
                        <p className="mt-1 text-xs text-graphite-600">
                          {c.registradoPor} · {formatoFecha(c.fecha)}
                        </p>
                      </div>
                    ))
                  )}
                </div>
                {!cerrado && (
                  <div className="flex gap-2 border-t border-black/[0.06] pt-4">
                    <input
                      placeholder="Agregar comentario..."
                      value={comentario}
                      onChange={(e) => setComentario(e.target.value)}
                      className={INPUT_CLASS}
                    />
                    <button
                      type="button"
                      disabled={!comentario || comentar.isPending}
                      onClick={() => comentar.mutate()}
                      className="btn-hover flex-shrink-0 rounded-lg bg-gold-500 px-3 py-2 text-xs font-medium text-white disabled:opacity-50"
                    >
                      Enviar
                    </button>
                  </div>
                )}
              </>
            )}

            {tab === 'bitacora' && (
              <div className="space-y-4">
                {/* Los tiempos de SLA son una métrica de gestión interna — solo le sirven a quien resuelve tickets, a un usuario normal solo le confunde. */}
                {esAgente && (
                  <div className="grid grid-cols-2 gap-3 rounded-lg border border-black/[0.06] p-3 text-xs">
                    <div className="flex items-start gap-2">
                      <Clock size={14} className="mt-0.5 flex-shrink-0 text-graphite-600" />
                      <div>
                        <p className="font-medium text-graphite-600">Tiempo de primera respuesta</p>
                        <p className="text-graphite-100">{formatoHoras(ticket.tiempoPrimeraRespuestaHoras)}</p>
                      </div>
                    </div>
                    <div className="flex items-start gap-2">
                      <Clock size={14} className="mt-0.5 flex-shrink-0 text-graphite-600" />
                      <div>
                        <p className="font-medium text-graphite-600">Tiempo total de resolución</p>
                        <p className="text-graphite-100">{formatoHoras(ticket.tiempoResolucionHoras)}</p>
                      </div>
                    </div>
                  </div>
                )}
                <div className="space-y-3">
                  {detalle.bitacora.map((h, i) => {
                    const nombreNuevo = nombreEstado(estados, h.estadoNuevo)
                    const nombreAnterior = nombreEstado(estados, h.estadoAnterior)
                    return (
                      <div key={i} className="flex items-start gap-3 text-xs">
                        <History size={14} className="mt-0.5 flex-shrink-0 text-graphite-600" />
                        <div>
                          <p className="text-graphite-100">
                            {h.estadoAnterior ? `${nombreAnterior} → ${nombreNuevo}` : `Ticket creado (${nombreNuevo})`}
                          </p>
                          {h.comentario && <p className="text-graphite-600">"{h.comentario}"</p>}
                          <p className="text-graphite-700">
                            {h.registradoPor} · {formatoFecha(h.fecha)}
                            {esAgente && h.horasEnEstadoAnterior !== null && (
                              <> · estuvo {formatoHoras(h.horasEnEstadoAnterior)} en el estado anterior</>
                            )}
                          </p>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

function mensajeError(e: unknown): string {
  const err = e as { response?: { data?: { detail?: string } }; message?: string }
  return err.response?.data?.detail ?? err.message ?? 'Ocurrió un error inesperado'
}
