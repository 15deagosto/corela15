import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, Plus, X, Droplets, Calculator, Pencil, ClipboardList } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Proceso {
  id: number
  nombre: string
  macroProceso: string
  critico: boolean
}

interface NivelEscala {
  id: number
  nombre: string
  nivel: number
}

interface EventoRiesgo {
  id: string
  proceso: string
  descripcion: string
  nivelImpacto: string
  nivelProbabilidad: string
  nivelRiesgo: string
  colorNivelRiesgo: string
  fechaIdentificacion: string
}

interface UsuarioRiesgo {
  id: string
  nombreUsuario: string
}

interface EstadoAvanceRiesgo {
  codigo: string
  nombre: string
}

interface AvanceRiesgoEtapaItem {
  codigoEstado: string
  estado: string
  comentario: string
  registradoPor: string
  fecha: string
}

interface AvanceRiesgoDetalleItem {
  idAvanceRiesgoDetalle: string
  eventoDetectado: string
  posibleCausa: string
  tratamiento: string
  inconvenientes: string
  accionesSugeridas: string
  bitacora: AvanceRiesgoEtapaItem[]
}

interface AvanceRiesgoItem {
  id: string
  idEventoRiesgo: string
  eventoDescripcion: string
  responsable: string
  codigoEstado: string
  estado: string
  fecha: string
  detalles: AvanceRiesgoDetalleItem[]
}

interface LiquidezEstructural {
  fechaCorte: string
  npl01: number
  npl02: number
  npl03: number
  npl04: number
  npl05: number
  nplt: number
  dpl01: number
  dpl02: number
  dpl03: number
  dpl04: number
  dplt: number
  lpl: number
  nsl01: number
  nsl02: number
  nsl03: number
  nslt: number
  dsl01: number
  dsl02: number
  dsl03: number
  dslt: number
  lsl: number
}

interface IndicadorLiquidez {
  id: string
  fecha: string
  fondosDisponibles: number
  depositosCortoPlazo: number
  coeficiente: number
  minimoRegulatorio: number
  cumpleMinimo: boolean
}

interface ParametroLiquidez {
  minimoRegulatorio: number
  actualizadoEn: string
  actualizadoPor: string
}

type ApiError = { response?: { data?: { detail?: string } } }
function mensajeError(error: unknown, fallback: string) {
  return (error as ApiError)?.response?.data?.detail ?? fallback
}
function pct(v: number) {
  return `${(v * 100).toFixed(2)}%`
}

function RegistrarEventoForm({
  procesos,
  nivelesImpacto,
  nivelesProbabilidad,
  onClose,
}: {
  procesos: Proceso[]
  nivelesImpacto: NivelEscala[]
  nivelesProbabilidad: NivelEscala[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idProceso, setIdProceso] = useState('')
  const [descripcion, setDescripcion] = useState('')
  const [idNivelImpacto, setIdNivelImpacto] = useState('')
  const [idNivelProbabilidad, setIdNivelProbabilidad] = useState('')

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/riesgo/eventos', {
          idProceso: Number(idProceso),
          descripcion,
          idNivelImpacto: Number(idNivelImpacto),
          idNivelProbabilidad: Number(idNivelProbabilidad),
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['riesgo-eventos'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar evento de riesgo</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idProceso || !descripcion || !idNivelImpacto || !idNivelProbabilidad) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Proceso</span>
          <select
            required
            value={idProceso}
            onChange={(e) => setIdProceso(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {procesos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.macroProceso} — {p.nombre}
                {p.critico ? ' (crítico)' : ''}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Descripción del evento</span>
          <textarea
            required
            value={descripcion}
            onChange={(e) => setDescripcion(e.target.value)}
            rows={2}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Impacto</span>
          <select
            required
            value={idNivelImpacto}
            onChange={(e) => setIdNivelImpacto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {nivelesImpacto.map((n) => (
              <option key={n.id} value={n.id}>
                {n.nivel} — {n.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Probabilidad</span>
          <select
            required
            value={idNivelProbabilidad}
            onChange={(e) => setIdNivelProbabilidad(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {nivelesProbabilidad.map((n) => (
              <option key={n.id} value={n.id}>
                {n.nivel} — {n.nombre}
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
          <p className="sm:col-span-2 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar el evento de riesgo.'}
          </p>
        )}
      </form>
    </div>
  )
}

function NuevoPlanAccionForm({
  idEventoRiesgo,
  usuarios,
  onClose,
}: {
  idEventoRiesgo: string
  usuarios: UsuarioRiesgo[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idUsuarioResponsable, setIdUsuarioResponsable] = useState('')
  const [eventoDetectado, setEventoDetectado] = useState('')
  const [posibleCausa, setPosibleCausa] = useState('')
  const [tratamiento, setTratamiento] = useState('')
  const [inconvenientes, setInconvenientes] = useState('')
  const [accionesSugeridas, setAccionesSugeridas] = useState('')

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/riesgo/avances', {
          idEventoRiesgo,
          idUsuarioResponsable,
          horaInicio: null,
          horaFin: null,
          eventoDetectado,
          posibleCausa,
          tratamiento,
          inconvenientes,
          accionesSugeridas,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['riesgo-avances'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h4 className="text-sm font-medium text-graphite-100">Nuevo plan de acción</h4>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={16} />
        </button>
      </div>
      <form
        className="grid grid-cols-1 gap-3 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idUsuarioResponsable || !eventoDetectado || !posibleCausa || !tratamiento) return
          crear.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Responsable</span>
          <select
            required
            value={idUsuarioResponsable}
            onChange={(e) => setIdUsuarioResponsable(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {usuarios.map((u) => (
              <option key={u.id} value={u.id}>
                {u.nombreUsuario}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Evento/riesgo detectado</span>
          <textarea required rows={2} value={eventoDetectado} onChange={(e) => setEventoDetectado(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Posible causa</span>
          <textarea required rows={2} value={posibleCausa} onChange={(e) => setPosibleCausa(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Tratamiento</span>
          <textarea required rows={2} value={tratamiento} onChange={(e) => setTratamiento(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Inconvenientes</span>
          <textarea rows={2} value={inconvenientes} onChange={(e) => setInconvenientes(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Acciones sugeridas</span>
          <textarea rows={2} value={accionesSugeridas} onChange={(e) => setAccionesSugeridas(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
        </label>
        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={crear.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {crear.isPending ? 'Creando…' : 'Crear plan de acción'}
          </button>
        </div>
        {crear.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">{mensajeError(crear.error, 'No se pudo crear el plan de acción.')}</p>
        )}
      </form>
    </div>
  )
}

function CambiarEstadoAvanceForm({ idDetalle, onDone }: { idDetalle: string; onDone: () => void }) {
  const queryClient = useQueryClient()
  const [codigoEstadoNuevo, setCodigoEstadoNuevo] = useState('')
  const [comentario, setComentario] = useState('')

  const { data: estados } = useQuery<EstadoAvanceRiesgo[]>({
    queryKey: ['riesgo-estados-avance'],
    queryFn: async () => (await api.get('/api/riesgo/estados-avance-riesgo')).data,
  })

  const cambiar = useMutation({
    mutationFn: async () =>
      api.post(`/api/riesgo/avances/detalles/${idDetalle}/estado`, { codigoEstadoNuevo, comentario }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['riesgo-avances'] })
      onDone()
    },
  })

  return (
    <form
      className="flex flex-wrap items-center gap-2"
      onSubmit={(e) => {
        e.preventDefault()
        if (!codigoEstadoNuevo || !comentario) return
        cambiar.mutate()
      }}
    >
      <select
        required
        value={codigoEstadoNuevo}
        onChange={(e) => setCodigoEstadoNuevo(e.target.value)}
        className="rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
      >
        <option value="">Nuevo estado…</option>
        {estados?.map((e) => (
          <option key={e.codigo} value={e.codigo}>
            {e.nombre}
          </option>
        ))}
      </select>
      <input
        required
        value={comentario}
        onChange={(e) => setComentario(e.target.value)}
        placeholder="Comentario"
        className="rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
      />
      <button type="submit" disabled={cambiar.isPending} className="text-xs font-medium text-petrol-700 hover:underline disabled:opacity-50">
        Registrar
      </button>
      {cambiar.isError && <span className="text-xs text-red-700">{mensajeError(cambiar.error, 'No se pudo cambiar el estado.')}</span>}
    </form>
  )
}

function PlanAccionEvento({ idEventoRiesgo }: { idEventoRiesgo: string }) {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [cambiandoEstado, setCambiandoEstado] = useState<string | null>(null)

  const { data: usuarios } = useQuery<UsuarioRiesgo[]>({
    queryKey: ['riesgo-usuarios'],
    queryFn: async () => (await api.get('/api/riesgo/usuarios')).data,
  })

  const { data: avances, isLoading } = useQuery<AvanceRiesgoItem[]>({
    queryKey: ['riesgo-avances'],
    queryFn: async () => (await api.get('/api/riesgo/avances')).data,
  })

  const avancesDelEvento = avances?.filter((a) => a.idEventoRiesgo === idEventoRiesgo) ?? []

  return (
    <div className="px-4 py-3">
      <div className="mb-2 flex items-center justify-between">
        <h4 className="text-xs font-semibold uppercase tracking-wide text-gold-400">Planes de acción</h4>
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
          >
            <Plus size={13} /> Nuevo plan
          </button>
        )}
      </div>

      {mostrarForm && usuarios && (
        <NuevoPlanAccionForm idEventoRiesgo={idEventoRiesgo} usuarios={usuarios} onClose={() => setMostrarForm(false)} />
      )}

      {isLoading && <p className="text-xs text-graphite-600">Cargando…</p>}
      {!isLoading && avancesDelEvento.length === 0 && !mostrarForm && (
        <p className="text-xs text-graphite-600">Sin planes de acción todavía.</p>
      )}

      <div className="flex flex-col gap-3">
        {avancesDelEvento.map((a) => (
          <div key={a.id} className="rounded-lg border border-black/[0.06] p-3">
            <div className="mb-2 flex items-center justify-between">
              <span className="text-xs text-graphite-600">Responsable: <span className="font-medium text-graphite-100">{a.responsable}</span> — {a.fecha}</span>
              <Badge variant={a.codigoEstado === 'AN' ? 'peligro' : a.codigoEstado === 'PR' ? 'exito' : 'alerta'}>{a.estado}</Badge>
            </div>
            {a.detalles.map((d) => (
              <div key={d.idAvanceRiesgoDetalle} className="mb-2 text-xs">
                <p><span className="text-graphite-600">Evento detectado:</span> {d.eventoDetectado}</p>
                <p><span className="text-graphite-600">Causa:</span> {d.posibleCausa}</p>
                <p><span className="text-graphite-600">Tratamiento:</span> {d.tratamiento}</p>
                {d.inconvenientes && <p><span className="text-graphite-600">Inconvenientes:</span> {d.inconvenientes}</p>}
                {d.accionesSugeridas && <p><span className="text-graphite-600">Acciones sugeridas:</span> {d.accionesSugeridas}</p>}

                <div className="mt-2 border-t border-black/[0.04] pt-2">
                  <p className="mb-1 text-graphite-600">Bitácora:</p>
                  <ul className="flex flex-col gap-0.5">
                    {d.bitacora.map((e, i) => (
                      <li key={i} className="text-graphite-600">
                        {new Date(e.fecha).toLocaleString('es-EC')} — <span className="font-medium text-graphite-100">{e.estado}</span> ({e.registradoPor}): {e.comentario}
                      </li>
                    ))}
                  </ul>
                </div>

                <div className="mt-2">
                  {cambiandoEstado === d.idAvanceRiesgoDetalle ? (
                    <CambiarEstadoAvanceForm idDetalle={d.idAvanceRiesgoDetalle} onDone={() => setCambiandoEstado(null)} />
                  ) : (
                    <button
                      type="button"
                      onClick={() => setCambiandoEstado(d.idAvanceRiesgoDetalle)}
                      className="text-xs font-medium text-graphite-600 hover:underline"
                    >
                      Cambiar estado
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  )
}

function SeccionEventos() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [idExpandido, setIdExpandido] = useState<string | null>(null)

  const { data: procesos } = useQuery<Proceso[]>({
    queryKey: ['riesgo-procesos'],
    queryFn: async () => (await api.get('/api/riesgo/procesos')).data,
  })

  const { data: nivelesImpacto } = useQuery<NivelEscala[]>({
    queryKey: ['riesgo-niveles-impacto'],
    queryFn: async () => (await api.get('/api/riesgo/niveles-impacto')).data,
  })

  const { data: nivelesProbabilidad } = useQuery<NivelEscala[]>({
    queryKey: ['riesgo-niveles-probabilidad'],
    queryFn: async () => (await api.get('/api/riesgo/niveles-probabilidad')).data,
  })

  const { data: eventos, isLoading } = useQuery<EventoRiesgo[]>({
    queryKey: ['riesgo-eventos'],
    queryFn: async () => (await api.get('/api/riesgo/eventos')).data,
  })

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Eventos de riesgo (matriz impacto × probabilidad)</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Registrar evento
          </button>
        )}
      </div>

      {mostrarForm && procesos && nivelesImpacto && nivelesProbabilidad && (
        <RegistrarEventoForm
          procesos={procesos}
          nivelesImpacto={nivelesImpacto}
          nivelesProbabilidad={nivelesProbabilidad}
          onClose={() => setMostrarForm(false)}
        />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Proceso</Th>
            <Th>Descripción</Th>
            <Th>Impacto</Th>
            <Th>Probabilidad</Th>
            <Th>Nivel</Th>
            <Th>Fecha</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (eventos?.length ?? 0) === 0 && <EmptyState>Todavía no hay eventos de riesgo registrados</EmptyState>}
          {eventos?.map((ev) => (
            <>
              <tr key={ev.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{ev.proceso}</Td>
                <Td>{ev.descripcion}</Td>
                <Td>{ev.nivelImpacto}</Td>
                <Td>{ev.nivelProbabilidad}</Td>
                <Td>
                  <span
                    className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium text-white"
                    style={{ backgroundColor: ev.colorNivelRiesgo }}
                  >
                    {ev.nivelRiesgo}
                  </span>
                </Td>
                <Td>{ev.fechaIdentificacion}</Td>
                <Td>
                  <button
                    type="button"
                    onClick={() => setIdExpandido(idExpandido === ev.id ? null : ev.id)}
                    className="flex items-center gap-1 text-xs font-medium text-graphite-600 hover:underline"
                  >
                    <ClipboardList size={13} /> Plan de acción
                  </button>
                </Td>
              </tr>
              {idExpandido === ev.id && (
                <tr className="border-b border-black/[0.04] bg-black/[0.015]">
                  <td colSpan={7}>
                    <PlanAccionEvento idEventoRiesgo={ev.id} />
                  </td>
                </tr>
              )}
            </>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function SeccionLiquidezEstructural() {
  const { data, isLoading, isError, error } = useQuery<LiquidezEstructural>({
    queryKey: ['riesgo-liquidez-estructural'],
    queryFn: async () => (await api.get('/api/riesgo/liquidez-estructural')).data,
  })

  return (
    <div className="mb-8 glass-card rounded-xl p-4">
      <h3 className="text-sm font-medium text-graphite-600">Liquidez Estructural (L01) — indicador real SEPS</h3>
      <p className="mt-1 text-xs text-graphite-600">
        Fórmula oficial verificada contra el Manual Técnico de Riesgo de Liquidez (v5.0): liquidez de primera línea
        (activos líquidos inmediatos / pasivos exigibles hasta 90 días) y segunda línea (acumulado hasta 180 días),
        calculada sobre las cuentas contables reales del catálogo CUC. <strong>No incluye</strong> Volatilidad
        General/IML final — esa parte de la norma depende de una metodología propia de la entidad ("Nota Técnica")
        que todavía no está definida en este core.
      </p>

      {isLoading && <p className="mt-3 text-sm text-graphite-600">Calculando…</p>}
      {isError && <p className="mt-3 text-sm text-red-700">{mensajeError(error, 'No se pudo calcular el indicador.')}</p>}

      {data && (
        <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="rounded-lg border border-black/[0.06] p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-graphite-600">Primera línea</p>
            <p className="mt-1 text-2xl font-semibold text-graphite-100">{data.lpl.toFixed(2)}×</p>
            <p className="text-xs text-graphite-600">
              Activos líquidos {formatoUsd(data.nplt)} / Pasivos exigibles {formatoUsd(data.dplt)}
            </p>
          </div>
          <div className="rounded-lg border border-black/[0.06] p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-graphite-600">Segunda línea (acumulada)</p>
            <p className="mt-1 text-2xl font-semibold text-graphite-100">{data.lsl.toFixed(2)}×</p>
            <p className="text-xs text-graphite-600">
              Activos líquidos {formatoUsd(data.nslt)} / Pasivos exigibles {formatoUsd(data.dslt)}
            </p>
          </div>
        </div>
      )}
    </div>
  )
}

function SeccionLiquidez() {
  const queryClient = useQueryClient()
  const [editandoMinimo, setEditandoMinimo] = useState(false)
  const [nuevoMinimo, setNuevoMinimo] = useState('')

  const { data: parametro } = useQuery<ParametroLiquidez>({
    queryKey: ['riesgo-liquidez-parametro'],
    queryFn: async () => (await api.get('/api/riesgo/liquidez/parametro')).data,
  })

  const { data: historico, isLoading } = useQuery<IndicadorLiquidez[]>({
    queryKey: ['riesgo-liquidez-historico'],
    queryFn: async () => (await api.get('/api/riesgo/liquidez')).data,
  })

  const ultimo = historico?.[0]

  const calcular = useMutation({
    mutationFn: async () => (await api.post('/api/riesgo/liquidez/calcular')).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['riesgo-liquidez-historico'] })
    },
  })

  const guardarMinimo = useMutation({
    mutationFn: async () => (await api.put('/api/riesgo/liquidez/parametro', { minimoRegulatorio: Number(nuevoMinimo) / 100 })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['riesgo-liquidez-parametro'] })
      setEditandoMinimo(false)
    },
  })

  return (
    <div>
      <SeccionLiquidezEstructural />

      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h3 className="text-sm font-medium text-graphite-600">Coeficiente de liquidez (operativo, simplificado)</h3>
          <p className="text-xs text-graphite-600">Fondos disponibles / depósitos a corto plazo — norma SEPS de riesgo de liquidez.</p>
        </div>
        <button
          type="button"
          onClick={() => calcular.mutate()}
          disabled={calcular.isPending}
          className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          <Calculator size={16} />
          {calcular.isPending ? 'Calculando…' : 'Calcular ahora'}
        </button>
      </div>

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="glass-card rounded-xl p-4">
          <p className="text-xs text-graphite-600">Coeficiente actual</p>
          <p className="mt-1 text-2xl font-semibold text-graphite-100">
            {ultimo ? pct(ultimo.coeficiente) : '—'}
          </p>
          {ultimo && (
            <Badge variant={ultimo.cumpleMinimo ? 'exito' : 'peligro'}>
              {ultimo.cumpleMinimo ? 'Cumple el mínimo' : 'Por debajo del mínimo'}
            </Badge>
          )}
        </div>
        <div className="glass-card rounded-xl p-4">
          <p className="text-xs text-graphite-600">Fondos disponibles</p>
          <p className="mt-1 text-2xl font-semibold text-graphite-100">
            {ultimo ? `$${ultimo.fondosDisponibles.toLocaleString('es-EC', { minimumFractionDigits: 2 })}` : '—'}
          </p>
          <p className="text-xs text-graphite-600">Depósitos: {ultimo ? `$${ultimo.depositosCortoPlazo.toLocaleString('es-EC', { minimumFractionDigits: 2 })}` : '—'}</p>
        </div>
        <div className="glass-card rounded-xl p-4">
          <div className="flex items-center justify-between">
            <p className="text-xs text-graphite-600">Mínimo regulatorio</p>
            {!editandoMinimo && parametro && (
              <button
                type="button"
                onClick={() => {
                  setNuevoMinimo(String(parametro.minimoRegulatorio * 100))
                  setEditandoMinimo(true)
                }}
                className="text-graphite-600 hover:text-petrol-700"
              >
                <Pencil size={13} />
              </button>
            )}
          </div>
          {editandoMinimo ? (
            <form
              className="mt-1 flex items-center gap-1.5"
              onSubmit={(e) => {
                e.preventDefault()
                guardarMinimo.mutate()
              }}
            >
              <input
                autoFocus
                type="number"
                step="0.01"
                min="0"
                max="100"
                value={nuevoMinimo}
                onChange={(e) => setNuevoMinimo(e.target.value)}
                className="w-20 rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
              <span className="text-sm text-graphite-600">%</span>
              <button type="submit" disabled={guardarMinimo.isPending} className="text-xs font-medium text-petrol-700 hover:underline">
                Guardar
              </button>
              <button type="button" onClick={() => setEditandoMinimo(false)} className="text-graphite-600 hover:text-graphite-100">
                <X size={14} />
              </button>
            </form>
          ) : (
            <p className="mt-1 text-2xl font-semibold text-graphite-100">
              {parametro ? pct(parametro.minimoRegulatorio) : '—'}
            </p>
          )}
          {guardarMinimo.isError && (
            <p className="mt-1 text-xs text-red-700">{mensajeError(guardarMinimo.error, 'No se pudo actualizar.')}</p>
          )}
        </div>
      </div>

      {calcular.isError && (
        <p className="mb-4 text-sm text-red-700">{mensajeError(calcular.error, 'No se pudo calcular el coeficiente.')}</p>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Fecha</Th>
            <Th>Fondos disponibles</Th>
            <Th>Depósitos corto plazo</Th>
            <Th>Coeficiente</Th>
            <Th>Estado</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (historico?.length ?? 0) === 0 && <EmptyState>Todavía no se ha calculado el indicador</EmptyState>}
          {historico?.map((i) => (
            <tr key={i.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td>{new Date(i.fecha).toLocaleString('es-EC')}</Td>
              <Td>${i.fondosDisponibles.toLocaleString('es-EC', { minimumFractionDigits: 2 })}</Td>
              <Td>${i.depositosCortoPlazo.toLocaleString('es-EC', { minimumFractionDigits: 2 })}</Td>
              <Td className="font-medium">{pct(i.coeficiente)}</Td>
              <Td>
                <Badge variant={i.cumpleMinimo ? 'exito' : 'peligro'}>{i.cumpleMinimo ? 'Cumple' : 'No cumple'}</Badge>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

const TABS = [
  { id: 'eventos', label: 'Eventos de riesgo' },
  { id: 'liquidez', label: 'Liquidez' },
] as const
type TabId = (typeof TABS)[number]['id']

export function Riesgo() {
  const [tab, setTab] = useState<TabId>('eventos')

  return (
    <div className="animate-fade-in">
      <PageHeader icon={AlertTriangle} title="Riesgo" subtitle="Gestión de riesgo operativo y de liquidez" />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {TABS.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`flex items-center gap-1.5 rounded-t-lg px-3 py-2 text-sm font-medium transition ${
              tab === t.id ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.id === 'liquidez' && <Droplets size={14} />}
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'eventos' && <SeccionEventos />}
      {tab === 'liquidez' && <SeccionLiquidez />}
    </div>
  )
}
