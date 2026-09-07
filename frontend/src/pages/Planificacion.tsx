import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { CalendarRange, Plus, Trash2, Eye, X } from 'lucide-react'
import { api } from '../lib/api'
import { useAuth } from '../lib/AuthContext'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'

const BASE = '/api/planificacion'
const DIAS = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado']
const INPUT_CLASS =
  'rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50'

interface Catalogo {
  codigo: string
  nombre: string
  colorHex?: string
}

interface Bloque {
  id?: string
  diaSemana: number
  horaInicio: string
  horaFin: string
  codigoEtiqueta: string
  descripcion: string
}

interface PlanSemanal {
  id: string
  codigoArea: string
  area: string
  fechaInicioSemana: string
  nombreResponsable: string
  cargoResponsable: string
  bloques: (Bloque & { etiqueta: string; colorHex: string })[]
  creadoEn: string
  creadoPor: string
  modificadoEn: string | null
}

interface PlanListItem {
  id: string
  codigoArea: string
  area: string
  fechaInicioSemana: string
  nombreResponsable: string
  cantidadBloques: number
  creadoEn: string
  creadoPor: string
}

function lunesDeEstaSemana(): string {
  const hoy = new Date()
  const dia = hoy.getDay() // 0=domingo
  const diff = dia === 0 ? -6 : 1 - dia
  const lunes = new Date(hoy)
  lunes.setDate(hoy.getDate() + diff)
  return lunes.toISOString().slice(0, 10)
}

function mensajeError(e: unknown): string {
  const err = e as { response?: { data?: { detail?: string } }; message?: string }
  return err.response?.data?.detail ?? err.message ?? 'Ocurrió un error inesperado'
}

export function Planificacion() {
  const { tieneMenu } = useAuth()
  const esGerencia = tieneMenu('planificacion-gerencia')
  const [tab, setTab] = useState<'editor' | 'historial'>('editor')

  const { data: areas } = useQuery<Catalogo[]>({
    queryKey: ['planificacion-areas'],
    queryFn: async () => (await api.get(`${BASE}/areas`)).data,
  })
  const { data: etiquetas } = useQuery<Catalogo[]>({
    queryKey: ['planificacion-etiquetas'],
    queryFn: async () => (await api.get(`${BASE}/etiquetas`)).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={CalendarRange}
        title="Planificación"
        subtitle={
          esGerencia
            ? 'Planificación semanal por área — tenés visibilidad de gerencia sobre todas las áreas'
            : 'Planificación semanal de tu área — solo vos ves y editás lo que reportás acá'
        }
      />

      <div className="mb-4 flex gap-1 border-b border-black/[0.06]">
        {(['editor', 'historial'] as const).map((t) => (
          <button
            key={t}
            type="button"
            onClick={() => setTab(t)}
            className={`border-b-2 px-4 py-2 text-sm font-medium ${
              tab === t ? 'border-gold-500 text-graphite-100' : 'border-transparent text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t === 'editor' ? 'Nueva / editar semana' : 'Historial'}
          </button>
        ))}
      </div>

      {tab === 'editor' && <SeccionEditor areas={areas ?? []} etiquetas={etiquetas ?? []} />}
      {tab === 'historial' && <SeccionHistorial areas={areas ?? []} esGerencia={esGerencia} />}
    </div>
  )
}

function SeccionEditor({ areas, etiquetas }: { areas: Catalogo[]; etiquetas: Catalogo[] }) {
  const queryClient = useQueryClient()
  const [codigoArea, setCodigoArea] = useState('')
  const [fechaInicioSemana, setFechaInicioSemana] = useState(lunesDeEstaSemana())
  const [nombreResponsable, setNombreResponsable] = useState('')
  const [cargoResponsable, setCargoResponsable] = useState('')
  const [bloques, setBloques] = useState<Bloque[]>([])
  const [resultado, setResultado] = useState<PlanSemanal | null>(null)

  const agregarBloque = () =>
    setBloques((b) => [...b, { diaSemana: 1, horaInicio: '08:00', horaFin: '09:00', codigoEtiqueta: etiquetas[0]?.codigo ?? '', descripcion: '' }])

  const actualizarBloque = (i: number, campo: keyof Bloque, valor: string | number) =>
    setBloques((b) => b.map((x, idx) => (idx === i ? { ...x, [campo]: valor } : x)))

  const quitarBloque = (i: number) => setBloques((b) => b.filter((_, idx) => idx !== i))

  const guardar = useMutation({
    mutationFn: async () =>
      (
        await api.post(`${BASE}/planes`, {
          codigoArea,
          fechaInicioSemana,
          nombreResponsable,
          cargoResponsable,
          bloques: bloques.map((b) => ({ ...b, horaInicio: `${b.horaInicio}:00`, horaFin: `${b.horaFin}:00` })),
        })
      ).data as PlanSemanal,
    onSuccess: (data) => {
      setResultado(data)
      queryClient.invalidateQueries({ queryKey: ['planificacion-historial'] })
    },
  })

  return (
    <div className="space-y-4">
      <div className="glass-card grid grid-cols-1 gap-3 rounded-xl p-4 sm:grid-cols-2 lg:grid-cols-4">
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-xs font-medium uppercase text-graphite-600">Área</span>
          <select value={codigoArea} onChange={(e) => setCodigoArea(e.target.value)} className={INPUT_CLASS}>
            <option value="">Seleccionar…</option>
            {areas.map((a) => (
              <option key={a.codigo} value={a.codigo}>
                {a.nombre}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-xs font-medium uppercase text-graphite-600">Semana (lunes)</span>
          <input type="date" value={fechaInicioSemana} onChange={(e) => setFechaInicioSemana(e.target.value)} className={INPUT_CLASS} />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-xs font-medium uppercase text-graphite-600">Responsable</span>
          <input value={nombreResponsable} onChange={(e) => setNombreResponsable(e.target.value)} className={INPUT_CLASS} placeholder="Nombre completo" />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-xs font-medium uppercase text-graphite-600">Cargo</span>
          <input value={cargoResponsable} onChange={(e) => setCargoResponsable(e.target.value)} className={INPUT_CLASS} placeholder="Ej. Jefe de TI" />
        </label>
      </div>

      <div className="glass-card rounded-xl p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">Bloques de actividad</h3>
          <button
            type="button"
            onClick={agregarBloque}
            className="btn-hover flex items-center gap-1.5 rounded-lg border border-gold-500 px-3 py-1.5 text-sm font-medium text-gold-500"
          >
            <Plus size={14} /> Agregar bloque
          </button>
        </div>

        {bloques.length === 0 ? (
          <p className="py-8 text-center text-sm text-graphite-600">Sin bloques todavía — agregá al menos uno.</p>
        ) : (
          <div className="space-y-2">
            {bloques.map((b, i) => (
              <div key={i} className="flex flex-wrap items-center gap-2 rounded-lg border border-black/[0.06] p-2">
                <select value={b.diaSemana} onChange={(e) => actualizarBloque(i, 'diaSemana', Number(e.target.value))} className={`${INPUT_CLASS} w-auto`}>
                  {DIAS.map((d, idx) => (
                    <option key={d} value={idx + 1}>
                      {d}
                    </option>
                  ))}
                </select>
                <input type="time" value={b.horaInicio} onChange={(e) => actualizarBloque(i, 'horaInicio', e.target.value)} className={`${INPUT_CLASS} w-auto`} />
                <span className="text-graphite-600">a</span>
                <input type="time" value={b.horaFin} onChange={(e) => actualizarBloque(i, 'horaFin', e.target.value)} className={`${INPUT_CLASS} w-auto`} />
                <select value={b.codigoEtiqueta} onChange={(e) => actualizarBloque(i, 'codigoEtiqueta', e.target.value)} className={`${INPUT_CLASS} w-auto`}>
                  {etiquetas.map((et) => (
                    <option key={et.codigo} value={et.codigo}>
                      {et.nombre}
                    </option>
                  ))}
                </select>
                <input
                  value={b.descripcion}
                  onChange={(e) => actualizarBloque(i, 'descripcion', e.target.value)}
                  className={`${INPUT_CLASS} min-w-[200px] flex-1`}
                  placeholder="Descripción de la actividad"
                />
                <button type="button" onClick={() => quitarBloque(i)} className="text-graphite-600 hover:text-red-600">
                  <Trash2 size={16} />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {guardar.isError && <div className="rounded-lg border border-red-300 bg-red-50 p-3 text-sm text-red-700">{mensajeError(guardar.error)}</div>}

      <button
        type="button"
        disabled={!codigoArea || !fechaInicioSemana || !nombreResponsable || bloques.length === 0 || guardar.isPending}
        onClick={() => guardar.mutate()}
        className="btn-hover rounded-lg bg-gold-500 px-5 py-2.5 text-sm font-medium text-white disabled:opacity-50"
      >
        Guardar planificación de la semana
      </button>

      {resultado && (
        <div className="space-y-3">
          <p className="text-sm font-medium text-graphite-100">Vista previa — así queda guardada:</p>
          <GrillaSemanal plan={resultado} />
        </div>
      )}
    </div>
  )
}

function SeccionHistorial({ areas, esGerencia }: { areas: Catalogo[]; esGerencia: boolean }) {
  const [filtroArea, setFiltroArea] = useState('')
  const [planAbierto, setPlanAbierto] = useState<string | null>(null)

  const { data: planes, isLoading } = useQuery<PlanListItem[]>({
    queryKey: ['planificacion-historial', filtroArea],
    queryFn: async () => (await api.get(`${BASE}/planes`, { params: { codigoArea: filtroArea || undefined } })).data,
  })

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <select value={filtroArea} onChange={(e) => setFiltroArea(e.target.value)} className={`${INPUT_CLASS} w-auto`}>
          <option value="">Todas las áreas</option>
          {areas.map((a) => (
            <option key={a.codigo} value={a.codigo}>
              {a.nombre}
            </option>
          ))}
        </select>
        {esGerencia && <Badge variant="exito">Vista de gerencia — todas las áreas</Badge>}
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Semana</Th>
            <Th>Área</Th>
            <Th>Responsable</Th>
            <Th>Bloques</Th>
            <Th>Reportado por</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <EmptyState>Cargando...</EmptyState>
          ) : !planes || planes.length === 0 ? (
            <EmptyState>Sin planificaciones registradas todavía.</EmptyState>
          ) : (
            planes.map((p) => (
              <tr key={p.id} className="border-t border-black/[0.04] hover:bg-black/[0.015]">
                <Td>{new Date(p.fechaInicioSemana).toLocaleDateString('es-EC', { day: '2-digit', month: 'short', year: 'numeric' })}</Td>
                <Td>{p.area}</Td>
                <Td>{p.nombreResponsable}</Td>
                <Td>{p.cantidadBloques}</Td>
                <Td>{p.creadoPor}</Td>
                <Td>
                  <button type="button" onClick={() => setPlanAbierto(p.id)} className="flex items-center gap-1 text-sm text-gold-600 hover:underline">
                    <Eye size={14} /> Ver
                  </button>
                </Td>
              </tr>
            ))
          )}
        </tbody>
      </TableContainer>

      {planAbierto && (
        <ModalPortal>
          <DetallePlanModal id={planAbierto} onClose={() => setPlanAbierto(null)} />
        </ModalPortal>
      )}
    </div>
  )
}

function DetallePlanModal({ id, onClose }: { id: string; onClose: () => void }) {
  const { data: plan, isLoading } = useQuery<PlanSemanal>({
    queryKey: ['planificacion-plan', id],
    queryFn: async () => (await api.get(`${BASE}/planes/${id}`)).data,
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card max-h-[90vh] w-full max-w-4xl overflow-y-auto rounded-xl p-6"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between">
          <h3 className="text-lg font-semibold text-graphite-100">
            {plan ? `${plan.area} — semana del ${new Date(plan.fechaInicioSemana).toLocaleDateString('es-EC')}` : 'Cargando…'}
          </h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>
        {isLoading || !plan ? (
          <p className="text-sm text-graphite-600">Cargando…</p>
        ) : (
          <>
            <p className="mb-4 text-sm text-graphite-600">
              Responsable: <span className="font-medium text-graphite-100">{plan.nombreResponsable}</span>
              {plan.cargoResponsable && <> — {plan.cargoResponsable}</>}
            </p>
            <GrillaSemanal plan={plan} />
          </>
        )}
      </div>
    </div>
  )
}

/** Grilla visual real: columnas por día, bloques coloreados posicionados por horario -- mismo espíritu del PDF de referencia que reemplaza este módulo. */
function GrillaSemanal({ plan }: { plan: PlanSemanal }) {
  const HORA_MIN = 8
  const HORA_MAX = 18
  const totalMinutos = (HORA_MAX - HORA_MIN) * 60

  const etiquetasUsadas = useMemo(() => {
    const mapa = new Map<string, { etiqueta: string; colorHex: string }>()
    plan.bloques.forEach((b) => mapa.set(b.codigoEtiqueta, { etiqueta: b.etiqueta, colorHex: b.colorHex }))
    return Array.from(mapa.values())
  }, [plan.bloques])

  const minutosDesde = (hora: string) => {
    const [h, m] = hora.split(':').map(Number)
    return (h - HORA_MIN) * 60 + m
  }

  return (
    <div className="space-y-3">
      <div className="flex flex-wrap gap-3">
        {etiquetasUsadas.map((e) => (
          <span key={e.etiqueta} className="flex items-center gap-1.5 text-xs text-graphite-600">
            <span className="h-2.5 w-2.5 rounded-sm" style={{ backgroundColor: e.colorHex }} />
            {e.etiqueta}
          </span>
        ))}
      </div>

      <div className="overflow-x-auto rounded-xl border border-black/[0.06]">
        <div className="grid min-w-[720px] grid-cols-6" style={{ height: `${totalMinutos * 0.9}px` }}>
          {DIAS.map((dia, idx) => {
            const diaNum = idx + 1
            const bloquesDia = plan.bloques.filter((b) => b.diaSemana === diaNum)
            return (
              <div key={dia} className="relative border-l border-black/[0.06] first:border-l-0">
                <div className="sticky top-0 z-10 border-b border-black/[0.06] bg-black/[0.02] px-2 py-1.5 text-center text-xs font-semibold uppercase text-graphite-600">
                  {dia}
                </div>
                <div className="relative" style={{ height: `${totalMinutos * 0.9}px` }}>
                  {bloquesDia.map((b) => {
                    const top = minutosDesde(b.horaInicio) * 0.9
                    const height = (minutosDesde(b.horaFin) - minutosDesde(b.horaInicio)) * 0.9
                    return (
                      <div
                        key={b.id}
                        title={`${b.horaInicio}–${b.horaFin}: ${b.descripcion}`}
                        className="absolute left-0.5 right-0.5 overflow-hidden rounded-md p-1 text-[10px] leading-tight text-white"
                        style={{ top: `${top}px`, height: `${Math.max(height, 18)}px`, backgroundColor: b.colorHex }}
                      >
                        <div className="font-semibold">
                          {b.horaInicio}–{b.horaFin}
                        </div>
                        <div className="truncate">{b.descripcion}</div>
                      </div>
                    )
                  })}
                </div>
              </div>
            )
          })}
        </div>
      </div>
    </div>
  )
}
