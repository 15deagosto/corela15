import { useEffect, useMemo, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { CalendarRange, Plus, Trash2, Eye, X, Send, Lock, MessageSquare, Pencil, Check, FileSpreadsheet, FileText, FileDown } from 'lucide-react'
import { api } from '../lib/api'
import { useAuth } from '../lib/AuthContext'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { BotonesExportar } from '../components/BotonesExportar'
import { exportarExcel, exportarCsv, type ColumnaExportable } from '../lib/exportar'
import { exportarPlanificacionPdf } from '../lib/exportarPlanificacion'

const BASE = '/api/planificacion'
const DIAS = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado']
const INPUT_CLASS =
  'rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50'
// Misma paleta rotativa real que asigna el backend a etiquetas nuevas
// (ver PlanificacionService.PaletaColores) -- acá se ofrece como set de
// swatches para elegir un color a mano, con el mismo matiz consistente
// en toda la grilla.
const PALETA_ETIQUETAS = [
  '#c9a227', '#16213e', '#64748b', '#1d4e5f', '#7a1f2b',
  '#6b4226', '#a67c1f', '#2f6f4f', '#5b3a8e', '#8a4b2e',
]

interface Catalogo {
  codigo: string
  nombre: string
  colorHex?: string
}

interface EtiquetaDto {
  codigo: string
  nombre: string
  colorHex: string
}

interface Bloque {
  id?: string
  diaSemana: number
  horaInicio: string
  horaFin: string
  codigoEtiqueta: string
  descripcion: string
}

/** Estado del formulario -- guarda el NOMBRE escrito (combo editable), se resuelve a código real recién al guardar. */
interface BloqueEditor {
  diaSemana: number
  horaInicio: string
  horaFin: string
  nombreEtiqueta: string
  descripcion: string
}

interface PlanSemanal {
  id: string
  codigoArea: string
  area: string
  fechaInicioSemana: string
  nombreResponsable: string
  cargoResponsable: string
  bloques: (Bloque & { etiqueta: string; colorHex: string; notaGerencia: string | null })[]
  enviada: boolean
  fechaEnvio: string | null
  enviadaFueraDeTiempo: boolean
  enviadaPor: string | null
  notaGerencia: string | null
  bloqueada: boolean
  fechaLimiteEnvio: string
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
  enviada: boolean
  enviadaFueraDeTiempo: boolean
  bloqueada: boolean
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

/** Suma minutos a una hora "HH:MM", con vuelta real a 00:00 si se pasa de medianoche. */
function sumarMinutos(hora: string, minutos: number): string {
  const [h, m] = hora.split(':').map(Number)
  const total = (h * 60 + m + minutos + 24 * 60) % (24 * 60)
  return `${Math.floor(total / 60).toString().padStart(2, '0')}:${(total % 60).toString().padStart(2, '0')}`
}

// Selector de hora propio, en pasos reales de 15 minutos -- reemplaza el
// widget nativo `<input type="time">` (el reloj de rueditas del sistema
// operativo, inconsistente entre navegadores y reportado como incómodo)
// por un `<select>` simple con las horas ya en formato real de 12h.
// Acotado al horario real de la jornada -- las 24 horas completas hacían
// que el desplegable se desbordara de la pantalla, y de todos modos
// nadie planifica a las 3 de la mañana.
function generarOpcionesHora(horaMin: number, horaMax: number): string[] {
  const total = Math.floor(((horaMax - horaMin) * 60) / 15) + 1
  return Array.from({ length: total }, (_, i) => {
    const m = horaMin * 60 + i * 15
    return `${Math.floor(m / 60).toString().padStart(2, '0')}:${(m % 60).toString().padStart(2, '0')}`
  })
}

/** Horario real de jornada por día -- Lunes a Viernes 08:00-19:00, Sábado 08:00-14:00 (ver PDF de referencia). */
function rangoHorasDia(diaSemana: number): [number, number] {
  return diaSemana === 6 ? [8, 14] : [8, 19]
}

/** Hora de fin por defecto (+1h desde el inicio), nunca más allá del horario real de ese día. */
function horaFinPorDefecto(horaInicio: string, diaSemana: number): string {
  const [, maxDia] = rangoHorasDia(diaSemana)
  const candidato = sumarMinutos(horaInicio, 60)
  const tope = `${maxDia.toString().padStart(2, '0')}:00`
  return candidato > tope ? tope : candidato
}

const OPCIONES_HORA_DEFECTO = generarOpcionesHora(8, 19)

function formatearHora12(hora: string): string {
  const [h, m] = hora.split(':').map(Number)
  const periodo = h < 12 ? 'a.m.' : 'p.m.'
  const h12 = h % 12 === 0 ? 12 : h % 12
  return `${h12}:${m.toString().padStart(2, '0')} ${periodo}`
}

function HoraSelect({
  value, onChange, disabled, opciones = OPCIONES_HORA_DEFECTO,
}: { value: string; onChange: (v: string) => void; disabled?: boolean; opciones?: string[] }) {
  return (
    <select disabled={disabled} value={value} onChange={(e) => onChange(e.target.value)} className={`${INPUT_CLASS} w-auto`}>
      {opciones.map((h) => (
        <option key={h} value={h}>
          {formatearHora12(h)}
        </option>
      ))}
    </select>
  )
}

/**
 * Combo editable real, propio (no `<datalist>` nativo): el navegador
 * filtra un `<datalist>` a lo que ya está escrito en el campo, así que
 * con un valor ya cargado (editando una semana existente) solo aparecía
 * esa 1 coincidencia -- el bug reportado. Este desplegable siempre
 * muestra el catálogo completo del área al abrirse, sin filtrar por lo
 * ya escrito, y deja escribir una categoría nueva en el mismo campo.
 *
 * Cada opción real también se puede editar (nombre + color) sin salir
 * de acá -- ícono de lápiz que abre un formulario inline con la misma
 * paleta que el backend usa para asignar color a una etiqueta nueva.
 */
function EtiquetaCombo({
  value, onChange, opciones, disabled, codigoArea, onRenombrada,
}: {
  value: string
  onChange: (v: string) => void
  opciones: Catalogo[]
  disabled?: boolean
  codigoArea: string
  /** Si la etiqueta que se está editando coincide con el valor actual de este campo, actualiza el texto también acá -- evita que una renombrada quede "huérfana" en el bloque que la tenía elegida. */
  onRenombrada?: (nombreAnterior: string, nombreNuevo: string) => void
}) {
  const queryClient = useQueryClient()
  const [abierto, setAbierto] = useState(false)
  const [editandoCodigo, setEditandoCodigo] = useState<string | null>(null)
  const [nombreEdit, setNombreEdit] = useState('')
  const [colorEdit, setColorEdit] = useState('')
  const [pos, setPos] = useState<{ top: number; left: number; width: number } | null>(null)
  const ref = useRef<HTMLDivElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const panelRef = useRef<HTMLDivElement>(null)

  // El desplegable se monta en `document.body` vía portal (posicionado
  // "fixed" contra el rectángulo real del input) -- mismo motivo que
  // ModalPortal en el resto del proyecto: si quedara anidado en el DOM
  // normal, cualquier ancestro con transform/overflow/stacking context
  // propio (la tarjeta de abajo con la grilla, en este caso) podía
  // pintarse por encima o dejarlo viéndose "transparente" -- el bug real
  // reportado.
  const actualizarPos = () => {
    const r = inputRef.current?.getBoundingClientRect()
    if (r) setPos({ top: r.bottom + 4, left: r.left, width: Math.max(r.width, 288) })
  }

  useEffect(() => {
    if (!abierto) return
    actualizarPos()
    window.addEventListener('scroll', actualizarPos, true)
    window.addEventListener('resize', actualizarPos)
    return () => {
      window.removeEventListener('scroll', actualizarPos, true)
      window.removeEventListener('resize', actualizarPos)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [abierto])

  useEffect(() => {
    const onClickFuera = (e: MouseEvent) => {
      const target = e.target as Node
      if (ref.current?.contains(target)) return
      if (panelRef.current?.contains(target)) return
      setAbierto(false)
      setEditandoCodigo(null)
    }
    document.addEventListener('mousedown', onClickFuera)
    return () => document.removeEventListener('mousedown', onClickFuera)
  }, [])

  const guardarEdicion = useMutation({
    mutationFn: async (o: Catalogo) =>
      (await api.put(`${BASE}/etiquetas/${o.codigo}`, { nombre: nombreEdit, colorHex: colorEdit })).data as EtiquetaDto,
    onSuccess: (data, o) => {
      queryClient.invalidateQueries({ queryKey: ['planificacion-etiquetas', codigoArea] })
      if (o.nombre.trim().toLowerCase() === value.trim().toLowerCase()) onChange(data.nombre)
      onRenombrada?.(o.nombre, data.nombre)
      setEditandoCodigo(null)
    },
  })

  const yaExiste = opciones.some((o) => o.nombre.trim().toLowerCase() === value.trim().toLowerCase())

  return (
    <div ref={ref} className="relative">
      <input
        ref={inputRef}
        disabled={disabled}
        value={value}
        onFocus={() => {
          actualizarPos()
          setAbierto(true)
        }}
        onClick={() => {
          // Elegir una opción nunca le quita el foco al input (a propósito,
          // para que el clic sobre la opción registre antes de cerrar el
          // panel) -- así que un segundo clic sobre el mismo campo ya
          // enfocado no dispara "onFocus" de nuevo. Sin esto, había que
          // hacer clic afuera primero para poder reabrir el desplegable.
          actualizarPos()
          setAbierto(true)
        }}
        onChange={(e) => {
          onChange(e.target.value)
          actualizarPos()
          setAbierto(true)
        }}
        className={`${INPUT_CLASS} w-48`}
        placeholder="Categoría — elegí o escribí una nueva"
        title="Elegí una existente del listado o escribí una categoría nueva: se registra sola para esta área"
      />
      {abierto && !disabled && pos && createPortal(
        <div
          ref={panelRef}
          style={{ position: 'fixed', top: pos.top, left: pos.left, width: pos.width }}
          className="z-[100] max-h-72 overflow-y-auto rounded-lg border border-black/[0.08] bg-white shadow-xl"
        >
          {value.trim() && !yaExiste && (
            <button
              type="button"
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => setAbierto(false)}
              className="block w-full border-b border-black/[0.06] px-3 py-2 text-left text-xs font-medium text-gold-600 hover:bg-gold-500/10"
            >
              Usar «{value.trim()}» (categoría nueva)
            </button>
          )}
          {opciones.length === 0 ? (
            <p className="px-3 py-2 text-xs text-graphite-600">Sin categorías todavía para esta área — escribí la primera.</p>
          ) : (
            opciones.map((o) =>
              editandoCodigo === o.codigo ? (
                <div key={o.codigo} className="border-b border-black/[0.06] bg-black/[0.015] p-2">
                  <input
                    autoFocus
                    value={nombreEdit}
                    onChange={(e) => setNombreEdit(e.target.value)}
                    onMouseDown={(e) => e.stopPropagation()}
                    className={`${INPUT_CLASS} mb-2 w-full py-1 text-xs`}
                  />
                  <div className="mb-2 flex flex-wrap gap-1.5">
                    {PALETA_ETIQUETAS.map((c) => (
                      <button
                        key={c}
                        type="button"
                        onMouseDown={(e) => e.preventDefault()}
                        onClick={() => setColorEdit(c)}
                        title={c}
                        className={`h-5 w-5 rounded-full ${colorEdit === c ? 'ring-2 ring-offset-1 ring-graphite-100' : ''}`}
                        style={{ backgroundColor: c }}
                      />
                    ))}
                  </div>
                  {guardarEdicion.isError && (
                    <p className="mb-1 text-[10px] text-red-700">{mensajeError(guardarEdicion.error)}</p>
                  )}
                  <div className="flex justify-end gap-1.5">
                    <button
                      type="button"
                      onMouseDown={(e) => e.preventDefault()}
                      onClick={() => setEditandoCodigo(null)}
                      className="rounded-md px-2 py-1 text-[10px] font-medium text-graphite-600 hover:bg-black/[0.03]"
                    >
                      Cancelar
                    </button>
                    <button
                      type="button"
                      disabled={!nombreEdit.trim() || guardarEdicion.isPending}
                      onMouseDown={(e) => e.preventDefault()}
                      onClick={() => guardarEdicion.mutate(o)}
                      className="btn-hover flex items-center gap-1 rounded-md bg-gold-500 px-2 py-1 text-[10px] font-medium text-white disabled:opacity-50"
                    >
                      <Check size={11} /> Guardar
                    </button>
                  </div>
                </div>
              ) : (
                <div
                  key={o.codigo}
                  className={`group flex w-full items-center gap-2 px-3 py-2 text-left text-xs hover:bg-black/[0.03] ${
                    o.nombre === value ? 'bg-gold-500/10 font-medium text-graphite-100' : 'text-graphite-100'
                  }`}
                >
                  <button
                    type="button"
                    onMouseDown={(e) => e.preventDefault()}
                    onClick={() => {
                      onChange(o.nombre)
                      setAbierto(false)
                    }}
                    className="flex min-w-0 flex-1 items-center gap-2 text-left"
                  >
                    <span className="h-2 w-2 flex-shrink-0 rounded-sm" style={{ backgroundColor: o.colorHex }} />
                    <span className="truncate">{o.nombre}</span>
                  </button>
                  <button
                    type="button"
                    onMouseDown={(e) => e.preventDefault()}
                    onClick={() => {
                      setEditandoCodigo(o.codigo)
                      setNombreEdit(o.nombre)
                      setColorEdit(o.colorHex ?? PALETA_ETIQUETAS[0])
                    }}
                    title="Editar nombre y color"
                    className="flex-shrink-0 rounded p-1 text-graphite-600 opacity-0 hover:bg-black/[0.06] hover:text-graphite-100 group-hover:opacity-100"
                  >
                    <Pencil size={11} />
                  </button>
                </div>
              ),
            )
          )}
        </div>,
        document.body,
      )}
    </div>
  )
}

/**
 * Exportación real de una semana de planificación -- distinta de
 * `BotonesExportar` (reportes tabulares genéricos): el PDF acá es el
 * reemplazo directo del PDF manual real que cada jefatura armaba a mano
 * (ver `exportarPlanificacionPdf`, con la grilla visual real, no una
 * tabla). Excel/CSV sí siguen siendo la vista tabular simple, útil como
 * dato crudo.
 */
function BotonesExportarPlan({
  area, fechaInicioSemana, nombreResponsable, cargoResponsable, bloques,
}: {
  area: string
  fechaInicioSemana: string
  nombreResponsable: string
  cargoResponsable: string
  bloques: GrillaBloque[]
}) {
  const sinDatos = bloques.length === 0
  const columnas: ColumnaExportable<GrillaBloque>[] = [
    { header: 'Día', accessor: (b) => DIAS[b.diaSemana - 1] },
    { header: 'Horario', accessor: (b) => `${b.horaInicio.slice(0, 5)}–${b.horaFin.slice(0, 5)}` },
    { header: 'Categoría', accessor: (b) => b.etiqueta },
    { header: 'Descripción', accessor: (b) => b.descripcion },
  ]

  return (
    <div className="flex items-center gap-2">
      <button
        type="button"
        disabled={sinDatos}
        onClick={() => exportarExcel(`planificacion-${fechaInicioSemana}`, area, columnas, bloques)}
        className="flex items-center gap-1 rounded-lg border border-black/[0.08] px-2.5 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-40"
        title="Exportar a Excel"
      >
        <FileSpreadsheet size={13} /> Excel
      </button>
      <button
        type="button"
        disabled={sinDatos}
        onClick={() => exportarPlanificacionPdf({ area, fechaInicioSemana, nombreResponsable: nombreResponsable || '—', cargoResponsable, bloques })}
        className="flex items-center gap-1 rounded-lg border border-black/[0.08] px-2.5 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-40"
        title="Exportar a PDF (formato real de la planificación semanal)"
      >
        <FileText size={13} /> PDF
      </button>
      <button
        type="button"
        disabled={sinDatos}
        onClick={() => exportarCsv(`planificacion-${fechaInicioSemana}`, columnas, bloques)}
        className="flex items-center gap-1 rounded-lg border border-black/[0.08] px-2.5 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-40"
        title="Exportar a CSV"
      >
        <FileDown size={13} /> CSV
      </button>
    </div>
  )
}

export function Planificacion() {
  const { tieneMenu } = useAuth()
  const esGerencia = tieneMenu('planificacion-gerencia')
  const [tab, setTab] = useState<'editor' | 'historial'>('editor')

  const { data: areas } = useQuery<Catalogo[]>({
    queryKey: ['planificacion-areas'],
    queryFn: async () => (await api.get(`${BASE}/areas`)).data,
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

      {tab === 'editor' && <SeccionEditor areas={areas ?? []} />}
      {tab === 'historial' && <SeccionHistorial areas={areas ?? []} esGerencia={esGerencia} />}
    </div>
  )
}

interface BloqueModalDatos {
  diaSemana: number
  horaInicio: string
  horaFin: string
  nombreEtiqueta: string
  descripcion: string
}

/**
 * Formulario de un solo bloque, abierto desde la grilla (clic en un
 * espacio libre para crear, clic en un bloque real para editar) -- la
 * grilla dejó de ser una vista previa pasiva de una lista aparte, ahora
 * ES la superficie de edición real, mismo patrón de modal-por-registro
 * ya establecido en el resto del proyecto.
 */
function BloqueModal({
  datos, esNuevo, etiquetas, codigoArea, bloquesExistentes, onGuardar, onEliminar, onClose, onRenombrada,
}: {
  datos: BloqueModalDatos
  esNuevo: boolean
  etiquetas: Catalogo[]
  codigoArea: string
  /** Bloques ya reales de la semana (sin incluir este) -- para no dejar elegir un horario que se cruce con otro del mismo día. */
  bloquesExistentes: { diaSemana: number; horaInicio: string; horaFin: string }[]
  onGuardar: (datos: BloqueModalDatos) => void
  onEliminar?: () => void
  onClose: () => void
  onRenombrada: (anterior: string, nuevo: string) => void
}) {
  const [form, setForm] = useState(datos)
  const minutos = (h: string) => {
    const [hh, mm] = h.split(':').map(Number)
    return hh * 60 + mm
  }

  const actualizar = (campo: keyof BloqueModalDatos, valor: string | number) =>
    setForm((f) => {
      const actualizado = { ...f, [campo]: valor }
      if (campo === 'horaInicio' && actualizado.horaFin <= (valor as string)) {
        actualizado.horaFin = horaFinPorDefecto(valor as string, f.diaSemana)
      }
      // Cambiar de día puede dejar la hora elegida ocupada por un bloque
      // real de ese día nuevo, o fuera de su horario real (Sábado
      // termina a la 1 o 2pm) -- se reacomoda sola al primer espacio
      // libre en vez de dejar seleccionado un horario que ya no vale.
      if (campo === 'diaSemana') {
        const opcionesDiaNuevo = generarOpcionesHora(...rangoHorasDia(valor as number))
        const ocupadosDia = bloquesExistentes.filter((o) => o.diaSemana === valor)
        const sigueValido = opcionesDiaNuevo.includes(f.horaInicio) &&
          !ocupadosDia.some((o) => minutos(f.horaInicio) >= minutos(o.horaInicio) && minutos(f.horaInicio) < minutos(o.horaFin))
        if (!sigueValido) {
          const libre = opcionesDiaNuevo.find((t) => !ocupadosDia.some((o) => minutos(t) >= minutos(o.horaInicio) && minutos(t) < minutos(o.horaFin)))
          if (libre) {
            actualizado.horaInicio = libre
            actualizado.horaFin = horaFinPorDefecto(libre, valor as number)
          }
        }
      }
      return actualizado
    })

  // Solo se pueden elegir espacios realmente disponibles, dentro del
  // horario real de ese día -- las opciones del selector de hora se
  // filtran contra los bloques reales que ya existen ese día, en vez de
  // dejar armar un horario que se cruce con otro y descubrirlo recién
  // al guardar.
  const opcionesHoraDia = useMemo(() => generarOpcionesHora(...rangoHorasDia(form.diaSemana)), [form.diaSemana])
  const ocupados = bloquesExistentes.filter((o) => o.diaSemana === form.diaSemana)
  const opcionesInicio = useMemo(() => {
    const libres = opcionesHoraDia.filter((t) => !ocupados.some((o) => minutos(t) >= minutos(o.horaInicio) && minutos(t) < minutos(o.horaFin)))
    return Array.from(new Set([form.horaInicio, ...libres])).sort()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opcionesHoraDia, ocupados, form.horaInicio])
  const opcionesFin = useMemo(() => {
    const siguienteInicio = ocupados.map((o) => minutos(o.horaInicio)).filter((m) => m > minutos(form.horaInicio)).sort((a, b) => a - b)[0]
    const libres = opcionesHoraDia.filter((t) => minutos(t) > minutos(form.horaInicio) && (siguienteInicio === undefined || minutos(t) <= siguienteInicio))
    return Array.from(new Set([form.horaFin, ...libres])).sort()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [opcionesHoraDia, ocupados, form.horaInicio, form.horaFin])

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="glass-card w-full max-w-md rounded-xl p-5" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-center justify-between">
          <h4 className="text-sm font-semibold text-graphite-100">{esNuevo ? 'Nuevo bloque de actividad' : 'Editar bloque'}</h4>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={16} />
          </button>
        </div>

        <div className="flex flex-col gap-3">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-xs font-medium uppercase text-graphite-600">Día</span>
            <select value={form.diaSemana} onChange={(e) => actualizar('diaSemana', Number(e.target.value))} className={INPUT_CLASS}>
              {DIAS.map((d, idx) => (
                <option key={d} value={idx + 1}>
                  {d}
                </option>
              ))}
            </select>
          </label>
          <div className="flex items-center gap-2">
            <label className="flex flex-1 flex-col gap-1 text-sm">
              <span className="text-xs font-medium uppercase text-graphite-600">Desde</span>
              <HoraSelect value={form.horaInicio} onChange={(v) => actualizar('horaInicio', v)} opciones={opcionesInicio} />
            </label>
            <label className="flex flex-1 flex-col gap-1 text-sm">
              <span className="text-xs font-medium uppercase text-graphite-600">Hasta</span>
              <HoraSelect value={form.horaFin} onChange={(v) => actualizar('horaFin', v)} opciones={opcionesFin} />
            </label>
          </div>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-xs font-medium uppercase text-graphite-600">Categoría</span>
            <EtiquetaCombo
              value={form.nombreEtiqueta}
              onChange={(v) => actualizar('nombreEtiqueta', v)}
              opciones={etiquetas}
              codigoArea={codigoArea}
              onRenombrada={(anterior, nuevo) => {
                onRenombrada(anterior, nuevo)
                if (form.nombreEtiqueta.trim().toLowerCase() === anterior.trim().toLowerCase()) actualizar('nombreEtiqueta', nuevo)
              }}
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-xs font-medium uppercase text-graphite-600">Descripción</span>
            <input
              autoFocus={!esNuevo}
              value={form.descripcion}
              onChange={(e) => actualizar('descripcion', e.target.value)}
              className={INPUT_CLASS}
              placeholder="Descripción de la actividad"
            />
          </label>
        </div>

        <div className="mt-5 flex items-center justify-between">
          {!esNuevo && onEliminar ? (
            <button
              type="button"
              onClick={onEliminar}
              className="flex items-center gap-1 rounded-lg border border-red-300 px-3 py-1.5 text-xs font-medium text-red-700 hover:bg-red-50"
            >
              <Trash2 size={13} /> Eliminar
            </button>
          ) : (
            <span />
          )}
          <button
            type="button"
            disabled={!form.nombreEtiqueta.trim() || form.horaFin <= form.horaInicio}
            title={form.horaFin <= form.horaInicio ? 'La hora de fin debe ser posterior a la de inicio' : undefined}
            onClick={() => onGuardar(form)}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            Guardar bloque
          </button>
        </div>
      </div>
    </div>
  )
}

function SeccionEditor({ areas }: { areas: Catalogo[] }) {
  const queryClient = useQueryClient()
  const [codigoArea, setCodigoArea] = useState('')
  const [fechaInicioSemana, setFechaInicioSemana] = useState(lunesDeEstaSemana())
  const [nombreResponsable, setNombreResponsable] = useState('')
  const [cargoResponsable, setCargoResponsable] = useState('')
  const [bloques, setBloques] = useState<BloqueEditor[]>([])
  const [resultado, setResultado] = useState<PlanSemanal | null>(null)
  const [modalBloque, setModalBloque] = useState<{ idxOriginal?: number; datos: BloqueModalDatos } | null>(null)

  // Si ya existe un plan real para esta área+semana (guardado antes,
  // enviado o no), lo cargamos para seguir editando en vez de arrancar
  // un formulario en blanco -- el upsert real del backend es por
  // área+semana, así que "editar la semana" es esto.
  const { data: planesDeLaSemana } = useQuery<PlanListItem[]>({
    queryKey: ['planificacion-semana', codigoArea, fechaInicioSemana],
    queryFn: async () => (await api.get(`${BASE}/planes`, { params: { codigoArea, desde: fechaInicioSemana, hasta: fechaInicioSemana } })).data,
    enabled: !!codigoArea && !!fechaInicioSemana,
  })
  const idPlanExistente = planesDeLaSemana?.[0]?.id ?? null
  const { data: planExistente } = useQuery<PlanSemanal>({
    queryKey: ['planificacion-plan', idPlanExistente],
    queryFn: async () => (await api.get(`${BASE}/planes/${idPlanExistente}`)).data,
    enabled: !!idPlanExistente,
  })

  useEffect(() => {
    if (!planExistente) return
    setResultado(planExistente)
    setNombreResponsable(planExistente.nombreResponsable)
    setCargoResponsable(planExistente.cargoResponsable)
    setBloques(
      planExistente.bloques.map((b) => ({
        diaSemana: b.diaSemana, horaInicio: b.horaInicio.slice(0, 5), horaFin: b.horaFin.slice(0, 5),
        nombreEtiqueta: b.etiqueta, descripcion: b.descripcion,
      })),
    )
  }, [planExistente])

  // Cambiar de área o de semana limpia el formulario -- evita mezclar
  // bloques de una semana con la fecha de otra por accidente.
  useEffect(() => {
    if (!idPlanExistente) {
      setResultado(null)
      setBloques([])
      setNombreResponsable('')
      setCargoResponsable('')
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [codigoArea, fechaInicioSemana])

  // Etiquetas reales de la etiqueta elegida -- cada área tiene su propio
  // set, no uno global compartido. Se recarga al cambiar de área.
  const { data: etiquetas } = useQuery<Catalogo[]>({
    queryKey: ['planificacion-etiquetas', codigoArea],
    queryFn: async () => (await api.get(`${BASE}/etiquetas`, { params: { codigoArea } })).data,
    enabled: !!codigoArea,
  })

  const bloqueada = resultado?.bloqueada ?? false

  const enviar = useMutation({
    mutationFn: async () => (await api.post(`${BASE}/planes/${resultado!.id}/enviar`)).data as PlanSemanal,
    onSuccess: (data) => {
      setResultado(data)
      queryClient.invalidateQueries({ queryKey: ['planificacion-historial'] })
      queryClient.invalidateQueries({ queryKey: ['planificacion-semana'] })
    },
  })

  // La grilla es la superficie de edición real: clic en un espacio libre
  // abre el modal para crear un bloque nuevo ahí mismo (día+hora ya
  // calculados según dónde se hizo clic); clic en un bloque real lo
  // abre para editar. "+ Agregar bloque" sigue disponible como atajo
  // sin depender de la precisión del clic.
  const abrirNuevoBloque = (diaSemana = 1, horaInicioClic = '08:00') => {
    // El clic puede caer fuera del horario real de ese día (Sábado
    // termina más temprano) o justo sobre un horario ya ocupado -- en
    // ambos casos se reacomoda sola al primer espacio libre real, nunca
    // se abre el modal precargado con un horario inválido.
    const minutos = (h: string) => {
      const [hh, mm] = h.split(':').map(Number)
      return hh * 60 + mm
    }
    const opcionesDia = generarOpcionesHora(...rangoHorasDia(diaSemana))
    const ocupadosDia = bloques.filter((b) => b.diaSemana === diaSemana)
    const valido = opcionesDia.includes(horaInicioClic) &&
      !ocupadosDia.some((o) => minutos(horaInicioClic) >= minutos(o.horaInicio) && minutos(horaInicioClic) < minutos(o.horaFin))
    const horaReal = valido
      ? horaInicioClic
      : (opcionesDia.find((t) => !ocupadosDia.some((o) => minutos(t) >= minutos(o.horaInicio) && minutos(t) < minutos(o.horaFin))) ?? horaInicioClic)
    setModalBloque({ datos: { diaSemana, horaInicio: horaReal, horaFin: horaFinPorDefecto(horaReal, diaSemana), nombreEtiqueta: '', descripcion: '' } })
  }

  const abrirEditarBloque = (b: GrillaBloque) =>
    setModalBloque({
      idxOriginal: b.idxOriginal,
      datos: { diaSemana: b.diaSemana, horaInicio: b.horaInicio.slice(0, 5), horaFin: b.horaFin.slice(0, 5), nombreEtiqueta: b.etiqueta, descripcion: b.descripcion },
    })

  const guardarDesdeModal = (datos: BloqueModalDatos) => {
    setBloques((bs) => (modalBloque?.idxOriginal !== undefined ? bs.map((x, idx) => (idx === modalBloque.idxOriginal ? datos : x)) : [...bs, datos]))
    setModalBloque(null)
  }

  const eliminarDesdeModal = () => {
    if (modalBloque?.idxOriginal === undefined) return
    setBloques((bs) => bs.filter((_, idx) => idx !== modalBloque.idxOriginal))
    setModalBloque(null)
  }

  const renombrarEtiquetaEnBloques = (anterior: string, nuevo: string) =>
    setBloques((bs) =>
      bs.map((x) => (x.nombreEtiqueta.trim().toLowerCase() === anterior.trim().toLowerCase() ? { ...x, nombreEtiqueta: nuevo } : x)),
    )

  // Vista previa real, reactiva a cada tecla -- antes solo se veía la
  // grilla después de guardar en el servidor. Resuelve el color de cada
  // bloque contra las etiquetas ya reales del área; si el usuario
  // escribió un nombre nuevo (todavía no registrado), se muestra en gris
  // neutro hasta que se guarde (ahí sí se crea y toma su color real).
  // `idxOriginal` viaja para poder editar el bloque real al hacer clic
  // sobre él en la grilla.
  const previewBloques: GrillaBloque[] = useMemo(
    () =>
      bloques
        .map((b, i) => ({ b, i }))
        .filter(({ b }) => b.horaFin > b.horaInicio && b.nombreEtiqueta.trim())
        .map(({ b, i }) => {
          const real = etiquetas?.find((et) => et.nombre.trim().toLowerCase() === b.nombreEtiqueta.trim().toLowerCase())
          return {
            diaSemana: b.diaSemana,
            horaInicio: `${b.horaInicio}:00`,
            horaFin: `${b.horaFin}:00`,
            descripcion: b.descripcion,
            etiqueta: b.nombreEtiqueta.trim(),
            colorHex: real?.colorHex ?? '#9ca3af',
            idxOriginal: i,
          }
        }),
    [bloques, etiquetas],
  )

  const guardar = useMutation({
    mutationFn: async () => {
      // Combo editable real: cada nombre de etiqueta escrito se resuelve
      // contra el área elegida -- si ya existe (por nombre, sin
      // importar mayúsculas), se reusa; si no, se registra en el
      // momento. Nunca hace falta ir a Configuración a mano solo para
      // agregar una categoría nueva.
      const nombresUnicos = Array.from(new Set(bloques.map((b) => b.nombreEtiqueta.trim()).filter(Boolean)))
      const mapaCodigos = new Map<string, string>()
      for (const nombre of nombresUnicos) {
        const { data } = await api.post(`${BASE}/etiquetas/obtener-o-crear`, { codigoArea, nombre })
        mapaCodigos.set(nombre.toLowerCase(), data.codigo)
      }

      return (
        await api.post(`${BASE}/planes`, {
          codigoArea,
          fechaInicioSemana,
          nombreResponsable,
          cargoResponsable,
          bloques: bloques.map((b) => ({
            diaSemana: b.diaSemana,
            horaInicio: `${b.horaInicio}:00`,
            horaFin: `${b.horaFin}:00`,
            codigoEtiqueta: mapaCodigos.get(b.nombreEtiqueta.trim().toLowerCase()) ?? '',
            descripcion: b.descripcion,
          })),
        })
      ).data as PlanSemanal
    },
    onSuccess: (data) => {
      setResultado(data)
      queryClient.invalidateQueries({ queryKey: ['planificacion-historial'] })
      queryClient.invalidateQueries({ queryKey: ['planificacion-semana'] })
      queryClient.invalidateQueries({ queryKey: ['planificacion-etiquetas', codigoArea] })
    },
  })

  return (
    <div className="space-y-4">
      {resultado && <EstadoEnvioBanner plan={resultado} />}

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
          <input disabled={bloqueada} value={nombreResponsable} onChange={(e) => setNombreResponsable(e.target.value)} className={INPUT_CLASS} placeholder="Nombre completo" />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-xs font-medium uppercase text-graphite-600">Cargo</span>
          <input disabled={bloqueada} value={cargoResponsable} onChange={(e) => setCargoResponsable(e.target.value)} className={INPUT_CLASS} placeholder="Ej. Jefe de TI" />
        </label>
      </div>

      {guardar.isError && <div className="rounded-lg border border-red-300 bg-red-50 p-3 text-sm text-red-700">{mensajeError(guardar.error)}</div>}
      {enviar.isError && <div className="rounded-lg border border-red-300 bg-red-50 p-3 text-sm text-red-700">{mensajeError(enviar.error)}</div>}

      {!bloqueada && (
        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            disabled={
              !codigoArea || !fechaInicioSemana || !nombreResponsable || bloques.length === 0 ||
              bloques.some((b) => !b.nombreEtiqueta.trim()) || guardar.isPending
            }
            title={bloques.some((b) => !b.nombreEtiqueta.trim()) ? 'Elegí o escribí una categoría en cada bloque antes de guardar' : undefined}
            onClick={() => guardar.mutate()}
            className="btn-hover rounded-lg bg-gold-500 px-5 py-2.5 text-sm font-medium text-white disabled:opacity-50"
          >
            {guardar.isPending ? 'Guardando…' : 'Guardar planificación de la semana'}
          </button>
          {resultado && !resultado.enviada && (
            <button
              type="button"
              disabled={enviar.isPending}
              onClick={() => enviar.mutate()}
              className="btn-hover flex items-center gap-1.5 rounded-lg border border-gold-500 px-4 py-2.5 text-sm font-medium text-gold-600 disabled:opacity-50"
              title="Marca la planificación como enviada — antes del viernes 5pm se puede seguir editando aunque ya esté enviada."
            >
              <Send size={14} /> Marcar como enviada
            </button>
          )}
          {resultado?.enviada && (
            <button
              type="button"
              disabled={enviar.isPending}
              onClick={() => enviar.mutate()}
              className="btn-hover flex items-center gap-1.5 rounded-lg border border-black/[0.08] px-4 py-2.5 text-sm font-medium text-graphite-600 disabled:opacity-50"
              title="Reenviar tras un cambio — actualiza la fecha de envío."
            >
              <Send size={14} /> Reenviar
            </button>
          )}
        </div>
      )}

      <div className="glass-card rounded-xl p-4">
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <h3 className="font-medium text-graphite-100">Planificación de la semana</h3>
          <div className="flex flex-wrap items-center gap-2">
            {codigoArea && !bloqueada && (
              <button
                type="button"
                onClick={() => abrirNuevoBloque()}
                className="btn-hover flex items-center gap-1.5 rounded-lg border border-gold-500 px-3 py-1.5 text-sm font-medium text-gold-500"
              >
                <Plus size={14} /> Agregar bloque
              </button>
            )}
            <BotonesExportarPlan
              area={areas.find((a) => a.codigo === codigoArea)?.nombre ?? ''}
              fechaInicioSemana={fechaInicioSemana}
              nombreResponsable={nombreResponsable}
              cargoResponsable={cargoResponsable}
              bloques={previewBloques}
            />
          </div>
        </div>

        {!codigoArea ? (
          <p className="py-12 text-center text-sm text-graphite-600">Elegí un área arriba para empezar a armar la planificación.</p>
        ) : (
          <GrillaSemanal
            bloques={previewBloques}
            onClickVacio={!bloqueada ? (dia, hora) => abrirNuevoBloque(dia, hora) : undefined}
            onClickBloque={!bloqueada ? (b) => abrirEditarBloque(b) : undefined}
          />
        )}
      </div>

      {modalBloque && (
        <ModalPortal>
          <BloqueModal
            datos={modalBloque.datos}
            esNuevo={modalBloque.idxOriginal === undefined}
            etiquetas={etiquetas ?? []}
            codigoArea={codigoArea}
            bloquesExistentes={bloques
              .filter((_, idx) => idx !== modalBloque.idxOriginal)
              .map((b) => ({ diaSemana: b.diaSemana, horaInicio: b.horaInicio, horaFin: b.horaFin }))}
            onGuardar={guardarDesdeModal}
            onEliminar={modalBloque.idxOriginal !== undefined ? eliminarDesdeModal : undefined}
            onClose={() => setModalBloque(null)}
            onRenombrada={renombrarEtiquetaEnBloques}
          />
        </ModalPortal>
      )}
    </div>
  )
}

function EstadoEnvioBanner({ plan }: { plan: PlanSemanal }) {
  const limite = new Date(plan.fechaLimiteEnvio).toLocaleString('es-EC', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })

  if (plan.bloqueada) {
    return (
      <div className="flex items-center gap-2 rounded-lg border border-red-300 bg-red-50 p-3 text-sm text-red-700">
        <Lock size={16} />
        <span>
          Esta semana ya fue enviada y pasó el horario límite ({limite}) — quedó bloqueada, ya no se puede modificar.
        </span>
      </div>
    )
  }
  if (plan.enviada) {
    return (
      <div className={`flex items-center gap-2 rounded-lg border p-3 text-sm ${plan.enviadaFueraDeTiempo ? 'border-amber-300 bg-amber-50 text-amber-700' : 'border-emerald-300 bg-emerald-50 text-emerald-700'}`}>
        <Send size={16} />
        <span>
          Enviada{plan.enviadaFueraDeTiempo ? ' fuera de horario' : ''} el{' '}
          {plan.fechaEnvio && new Date(plan.fechaEnvio).toLocaleString('es-EC', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })}
          {plan.enviadaPor && ` por ${plan.enviadaPor}`}. Todavía editable hasta el corte ({limite}).
        </span>
      </div>
    )
  }
  return (
    <div className="rounded-lg border border-black/[0.06] bg-black/[0.015] p-3 text-sm text-graphite-600">
      Borrador sin enviar — corte de envío: viernes {limite}.
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

  const columnasHistorial: ColumnaExportable<PlanListItem>[] = [
    { header: 'Semana', accessor: (p) => new Date(p.fechaInicioSemana).toLocaleDateString('es-EC', { day: '2-digit', month: 'short', year: 'numeric' }) },
    { header: 'Área', accessor: (p) => p.area },
    { header: 'Responsable', accessor: (p) => p.nombreResponsable },
    { header: 'Bloques', accessor: (p) => p.cantidadBloques },
    { header: 'Estado', accessor: (p) => (p.bloqueada ? 'Bloqueada' : p.enviada ? (p.enviadaFueraDeTiempo ? 'Enviada tarde' : 'Enviada') : 'Borrador') },
    { header: 'Reportado por', accessor: (p) => p.creadoPor },
  ]

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
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
        <BotonesExportar
          nombreArchivo="planificacion-historial"
          titulo="Historial de planificación semanal"
          subtitulo={filtroArea ? areas.find((a) => a.codigo === filtroArea)?.nombre : 'Todas las áreas'}
          columnas={columnasHistorial}
          filas={planes ?? []}
        />
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Semana</Th>
            <Th>Área</Th>
            <Th>Responsable</Th>
            <Th>Bloques</Th>
            <Th>Estado</Th>
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
                <Td>
                  {p.bloqueada ? (
                    <Badge variant="peligro">Bloqueada</Badge>
                  ) : p.enviada ? (
                    <Badge variant={p.enviadaFueraDeTiempo ? 'alerta' : 'exito'}>{p.enviadaFueraDeTiempo ? 'Enviada tarde' : 'Enviada'}</Badge>
                  ) : (
                    <Badge variant="neutral">Borrador</Badge>
                  )}
                </Td>
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
          <DetallePlanModal id={planAbierto} onClose={() => setPlanAbierto(null)} esGerencia={esGerencia} />
        </ModalPortal>
      )}
    </div>
  )
}

function DetallePlanModal({ id, onClose, esGerencia }: { id: string; onClose: () => void; esGerencia: boolean }) {
  const queryClient = useQueryClient()
  const { data: plan, isLoading } = useQuery<PlanSemanal>({
    queryKey: ['planificacion-plan', id],
    queryFn: async () => (await api.get(`${BASE}/planes/${id}`)).data,
  })
  const [notaGerencia, setNotaGerencia] = useState('')
  useEffect(() => setNotaGerencia(plan?.notaGerencia ?? ''), [plan?.notaGerencia])

  const invalidar = (data: PlanSemanal) => {
    queryClient.setQueryData(['planificacion-plan', id], data)
    queryClient.invalidateQueries({ queryKey: ['planificacion-historial'] })
    queryClient.invalidateQueries({ queryKey: ['planificacion-semana'] })
  }

  const guardarNotaGeneral = useMutation({
    mutationFn: async () => (await api.post(`${BASE}/planes/${id}/nota-gerencia`, { nota: notaGerencia || null })).data as PlanSemanal,
    onSuccess: invalidar,
  })

  const guardarNotaBloque = useMutation({
    mutationFn: async ({ idBloque, nota }: { idBloque: string; nota: string }) =>
      (await api.post(`${BASE}/bloques/${idBloque}/nota-gerencia`, { nota: nota || null })).data as PlanSemanal,
    onSuccess: invalidar,
  })

  // Doble clic sobre un bloque real de la grilla -- reemplaza el
  // formulario con selector que había al final de la pantalla.
  const [bloqueNotaEditando, setBloqueNotaEditando] = useState<GrillaBloque | null>(null)

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
          <div className="space-y-4">
            <EstadoEnvioBanner plan={plan} />
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="text-sm text-graphite-600">
                Responsable: <span className="font-medium text-graphite-100">{plan.nombreResponsable}</span>
                {plan.cargoResponsable && <> — {plan.cargoResponsable}</>}
              </p>
              <BotonesExportarPlan
                area={plan.area}
                fechaInicioSemana={plan.fechaInicioSemana}
                nombreResponsable={plan.nombreResponsable}
                cargoResponsable={plan.cargoResponsable}
                bloques={plan.bloques}
              />
            </div>
            <GrillaSemanal
              bloques={plan.bloques}
              onDobleClickBloque={esGerencia ? (b) => setBloqueNotaEditando(b) : undefined}
            />
            {esGerencia && (
              <p className="-mt-2 text-xs text-graphite-600">
                Doble clic sobre un bloque de la grilla para dejar un comentario en esa hora específica.
              </p>
            )}

            {(esGerencia || plan.notaGerencia) && (
              <div className="rounded-xl border border-black/[0.06] p-4">
                <p className="mb-2 flex items-center gap-1.5 text-sm font-medium text-graphite-100">
                  <MessageSquare size={14} /> Nota general de gerencia
                </p>
                {esGerencia ? (
                  <div className="flex flex-col gap-2">
                    <textarea
                      value={notaGerencia}
                      onChange={(e) => setNotaGerencia(e.target.value)}
                      className={`${INPUT_CLASS} min-h-[70px]`}
                      placeholder="Comentario sobre toda la semana…"
                    />
                    <button
                      type="button"
                      disabled={guardarNotaGeneral.isPending}
                      onClick={() => guardarNotaGeneral.mutate()}
                      className="btn-hover self-start rounded-lg border border-gold-500 px-3 py-1.5 text-xs font-medium text-gold-600 disabled:opacity-50"
                    >
                      {guardarNotaGeneral.isPending ? 'Guardando…' : 'Guardar nota'}
                    </button>
                  </div>
                ) : (
                  <p className="whitespace-pre-wrap text-sm text-graphite-100">{plan.notaGerencia}</p>
                )}
              </div>
            )}

          </div>
        )}
      </div>

      {bloqueNotaEditando && (
        <ModalPortal>
          <NotaBloqueModal
            bloque={bloqueNotaEditando}
            onClose={() => setBloqueNotaEditando(null)}
            onGuardar={(nota) => {
              guardarNotaBloque.mutate(
                { idBloque: bloqueNotaEditando.id!, nota },
                { onSuccess: () => setBloqueNotaEditando(null) },
              )
            }}
            guardando={guardarNotaBloque.isPending}
          />
        </ModalPortal>
      )}
    </div>
  )
}

/** Comentario puntual de gerencia sobre un bloque real, abierto con doble clic sobre la celda en la grilla -- ver PlanSemanalBloque.NotaGerencia. */
function NotaBloqueModal({
  bloque, onClose, onGuardar, guardando,
}: { bloque: GrillaBloque; onClose: () => void; onGuardar: (nota: string) => void; guardando: boolean }) {
  const [nota, setNota] = useState(bloque.notaGerencia ?? '')

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="glass-card w-full max-w-md rounded-xl p-5" onClick={(e) => e.stopPropagation()}>
        <div className="mb-3 flex items-center justify-between">
          <h4 className="flex items-center gap-1.5 text-sm font-semibold text-graphite-100">
            <MessageSquare size={14} /> Comentario sobre este bloque
          </h4>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={16} />
          </button>
        </div>
        <p className="mb-3 text-xs text-graphite-600">
          {DIAS[bloque.diaSemana - 1]} {bloque.horaInicio.slice(0, 5)}–{bloque.horaFin.slice(0, 5)} — {bloque.descripcion}
        </p>
        <textarea
          autoFocus
          value={nota}
          onChange={(e) => setNota(e.target.value)}
          className={`${INPUT_CLASS} min-h-[90px] w-full`}
          placeholder="Comentario para el responsable del área…"
        />
        <div className="mt-3 flex items-center justify-end gap-2">
          {bloque.notaGerencia && (
            <button
              type="button"
              disabled={guardando}
              onClick={() => onGuardar('')}
              className="btn-hover rounded-lg border border-red-300 px-3 py-1.5 text-xs font-medium text-red-700 disabled:opacity-50"
            >
              Quitar comentario
            </button>
          )}
          <button
            type="button"
            disabled={guardando}
            onClick={() => onGuardar(nota)}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-1.5 text-xs font-medium text-white disabled:opacity-50"
          >
            {guardando ? 'Guardando…' : 'Guardar comentario'}
          </button>
        </div>
      </div>
    </div>
  )
}

/** Bloque genérico que la grilla sabe dibujar -- id opcional porque la vista previa en vivo (sin guardar todavía) no tiene ninguno real. */
interface GrillaBloque {
  id?: string
  diaSemana: number
  horaInicio: string
  horaFin: string
  descripcion: string
  etiqueta: string
  colorHex: string
  notaGerencia?: string | null
  /** Índice real dentro de `bloques` (el editor) -- para poder editarlo al hacer clic sobre el bloque en la grilla. */
  idxOriginal?: number
}

/**
 * Grilla visual real: columnas por día, bloques coloreados posicionados
 * por horario -- mismo espíritu del PDF de referencia que reemplaza este
 * módulo. Tres modos de interacción real, opcionales y mutuamente
 * exclusivos según quién la usa:
 * - `onDobleClickBloque` (solo gerencia, ver DetallePlanModal): doble
 *   clic sobre un bloque ya guardado para comentar esa hora específica.
 * - `onClickBloque` (editor, ver SeccionEditor): clic sobre un bloque
 *   real para editarlo -- la grilla ES el editor, no una vista previa
 *   pasiva de una lista aparte.
 * - `onClickVacio` (editor): clic sobre un espacio libre de un día para
 *   crear un bloque nuevo ahí mismo, con la hora ya calculada según
 *   dónde se hizo clic.
 */
function GrillaSemanal({
  bloques, onDobleClickBloque, onClickBloque, onClickVacio,
}: {
  bloques: GrillaBloque[]
  onDobleClickBloque?: (b: GrillaBloque) => void
  onClickBloque?: (b: GrillaBloque) => void
  onClickVacio?: (diaSemana: number, horaInicio: string) => void
}) {
  const HORA_MIN = 8
  const HORA_MAX = 19
  const PX_POR_MIN = 1.1
  const totalMinutos = (HORA_MAX - HORA_MIN) * 60

  const etiquetasUsadas = useMemo(() => {
    const mapa = new Map<string, { etiqueta: string; colorHex: string }>()
    bloques.forEach((b) => mapa.set(b.etiqueta, { etiqueta: b.etiqueta, colorHex: b.colorHex }))
    return Array.from(mapa.values())
  }, [bloques])

  const minutosDesde = (hora: string) => {
    const [h, m] = hora.split(':').map(Number)
    return (h - HORA_MIN) * 60 + m
  }

  const clicEnColumna = (e: React.MouseEvent<HTMLDivElement>, diaNum: number) => {
    if (!onClickVacio) return
    const rect = e.currentTarget.getBoundingClientRect()
    const minutosCrudos = (e.clientY - rect.top) / PX_POR_MIN
    const minutos = Math.max(0, Math.min(totalMinutos - 15, Math.round(minutosCrudos / 15) * 15)) + HORA_MIN * 60
    const horaInicio = `${Math.floor(minutos / 60).toString().padStart(2, '0')}:${(minutos % 60).toString().padStart(2, '0')}`
    onClickVacio(diaNum, horaInicio)
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
        {onClickVacio && (
          <span className="text-xs italic text-graphite-600">Clic en un espacio libre para agregar un bloque ahí mismo</span>
        )}
      </div>

      {/* max-h + overflow-y propio -- sin esto, "sticky" se pega contra el
          scroll de toda la página en vez del de la grilla, y el
          encabezado del día terminaba flotando encima de los bloques al
          hacer scroll (el bug real reportado). */}
      <div className="max-h-[640px] overflow-auto rounded-xl border border-black/[0.06]">
        <div className="grid min-w-[840px] grid-cols-6" style={{ height: `${totalMinutos * PX_POR_MIN}px` }}>
          {DIAS.map((dia, idx) => {
            const diaNum = idx + 1
            const bloquesDia = bloques.filter((b) => b.diaSemana === diaNum)
            return (
              <div key={dia} className="relative border-l border-black/[0.06] first:border-l-0">
                <div className="sticky top-0 z-10 border-b border-black/[0.06] bg-black/[0.02] px-2 py-1.5 text-center text-xs font-semibold uppercase text-graphite-600">
                  {dia}
                </div>
                <div
                  onClick={(e) => clicEnColumna(e, diaNum)}
                  className={`relative ${onClickVacio ? 'cursor-cell hover:bg-gold-500/[0.04]' : ''}`}
                  style={{ height: `${totalMinutos * PX_POR_MIN}px` }}
                >
                  {bloquesDia.map((b, i) => {
                    const top = minutosDesde(b.horaInicio) * PX_POR_MIN
                    const height = (minutosDesde(b.horaFin) - minutosDesde(b.horaInicio)) * PX_POR_MIN
                    const tieneNota = !!b.notaGerencia
                    const interactivo = !!(onDobleClickBloque || onClickBloque)
                    const titulo = `${b.horaInicio}–${b.horaFin}: ${b.descripcion}` +
                      (tieneNota ? `\n\nNota de gerencia: ${b.notaGerencia}` : onDobleClickBloque ? '\n\nDoble clic para comentar' : onClickBloque ? '\n\nClic para editar' : '')
                    return (
                      <div
                        key={b.id ?? i}
                        title={titulo}
                        onClick={(e) => {
                          if (!onClickBloque) return
                          e.stopPropagation()
                          onClickBloque(b)
                        }}
                        onDoubleClick={(e) => {
                          if (!onDobleClickBloque) return
                          e.stopPropagation()
                          onDobleClickBloque(b)
                        }}
                        className={`absolute left-0.5 right-0.5 overflow-hidden rounded-md p-1.5 text-[11px] leading-tight text-white ${interactivo ? 'cursor-pointer ring-1 ring-inset ring-white/0 hover:ring-white/70' : ''}`}
                        style={{ top: `${top}px`, height: `${Math.max(height, 22)}px`, backgroundColor: b.colorHex }}
                      >
                        <div className="flex items-center gap-1 font-semibold">
                          {b.horaInicio}–{b.horaFin}
                          {tieneNota && <MessageSquare size={11} className="flex-shrink-0" />}
                        </div>
                        <div className="font-medium opacity-90">{b.etiqueta}</div>
                        {b.descripcion && <div className="truncate opacity-80">{b.descripcion}</div>}
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
