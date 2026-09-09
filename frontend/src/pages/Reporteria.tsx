import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { BarChart3, Play, Save, Star, Trash2, X } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { BotonesExportar } from '../components/BotonesExportar'
import { useAuth } from '../lib/AuthContext'
import {
  listarDatasets, ejecutarConsulta, catalogoFiltro, listarTableros, obtenerTablero, crearTablero,
  borrarTablero, marcarFavorito, quitarFavorito, formatearValor,
  type DatasetInfo, type FilterValue, type QueryResult, type DefinicionTablero,
} from '../lib/reporteria'
import { TableroCartera } from './Reporteria/TableroCartera'
import { TableroCaptaciones } from './Reporteria/TableroCaptaciones'
import { TableroContabilidad } from './Reporteria/TableroContabilidad'
import { TableroSocios } from './Reporteria/TableroSocios'
import { TableroSolicitudes } from './Reporteria/TableroSolicitudes'
import { TableroVinculados } from './Reporteria/TableroVinculados'
import { WidgetCard } from './Reporteria/WidgetCard'

const INPUT_CLASS =
  'w-full rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50'

function mensajeError(e: unknown): string {
  const err = e as { response?: { data?: { detail?: string } }; message?: string }
  return err.response?.data?.detail ?? err.message ?? 'Ocurrió un error inesperado'
}

/** Tableros predefinidos reales (gráficos + tarjetas resumen, portados tal cual de siga-web/src/pages) — el gate de cada uno es el dataset real que consume. */
const TABLEROS_PREDEFINIDOS = [
  { id: 'cartera', label: 'Cartera de Crédito', dataset: 'cartera', Componente: TableroCartera },
  { id: 'captaciones', label: 'Captaciones', dataset: 'ahorros', Componente: TableroCaptaciones },
  { id: 'contabilidad', label: 'Contabilidad', dataset: 'contabilidad', Componente: TableroContabilidad },
  { id: 'socios', label: 'Socios', dataset: 'socios', Componente: TableroSocios },
  { id: 'solicitudes', label: 'Solicitudes de Crédito', dataset: 'solicitudes', Componente: TableroSolicitudes },
  { id: 'vinculados', label: 'Créditos Vinculados', dataset: 'vinculados', Componente: TableroVinculados },
] as const

/**
 * Reportería Gerencial — motor semántico portado del proyecto SIGA,
 * **incluidos los 6 tableros predefinidos reales** (gráficos con Recharts,
 * tarjetas resumen, tablas de detalle) tal como existían en SIGA — no un
 * explorador genérico. A diferencia de SIGA (login propio contra AD,
 * permisos por un solo rol fijo Gerencia/Negocios/TI), acá el login es el
 * único de Corela15 y la visibilidad de cada tablero viene del claim real
 * `dataset` del JWT (`seguridad.dataset_reporteria`) — `tieneDataset` gatea
 * cada pestaña antes de montarla.
 *
 * "Explorador" (armar una consulta ad-hoc eligiendo dimensiones/métricas/
 * filtros) y "Mis tableros" (guardar y reabrir una consulta armada) se
 * mantienen como pestañas aparte, mismo criterio que SIGA (que también los
 * tenía como páginas separadas de los tableros predefinidos).
 */
export function Reporteria() {
  const { tieneDataset } = useAuth()
  const { data: datasets = [] } = useQuery({ queryKey: ['reporteria-datasets'], queryFn: listarDatasets })

  const predefinidosVisibles = useMemo(() => TABLEROS_PREDEFINIDOS.filter((t) => tieneDataset(t.dataset)), [tieneDataset])

  const tabs = useMemo(
    () => [
      ...predefinidosVisibles.map((t) => ({ id: t.id, label: t.label })),
      { id: 'explorador', label: 'Explorador' },
      { id: 'tableros', label: 'Mis tableros' },
    ],
    [predefinidosVisibles],
  )

  const [tab, setTab] = useState<string>('')
  useEffect(() => {
    if (!tab && tabs.length > 0) setTab(tabs[0].id)
  }, [tab, tabs])

  const predefinidoActivo = predefinidosVisibles.find((t) => t.id === tab)

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={BarChart3}
        title="Reportería Gerencial"
        subtitle="Cartera, captaciones, contabilidad y socios — tableros reales con gráficos, más un explorador libre sin escribir SQL"
      />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`flex items-center gap-1.5 rounded-t-lg px-3 py-2 text-sm font-medium transition ${
              tab === t.id ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {predefinidoActivo ? (
        <predefinidoActivo.Componente />
      ) : tab === 'explorador' ? (
        <SeccionExplorador datasets={datasets} />
      ) : tab === 'tableros' ? (
        <SeccionTableros />
      ) : (
        <MensajeVacio>No tenés datasets otorgados en Reportería Gerencial todavía.</MensajeVacio>
      )}
    </div>
  )
}

function MensajeVacio({ children }: { children: ReactNode }) {
  return <div className="rounded-xl border border-black/[0.06] bg-white px-4 py-12 text-center text-sm text-graphite-600">{children}</div>
}

function SeccionExplorador({ datasets }: { datasets: DatasetInfo[] }) {
  const [datasetId, setDatasetId] = useState<string>('')
  const datasetActivo = datasets.find((d) => d.id === datasetId) ?? datasets[0] ?? null

  if (!datasetActivo) {
    return <MensajeVacio>No tenés datasets otorgados en Reportería Gerencial todavía.</MensajeVacio>
  }

  return (
    <div className="flex flex-col gap-4">
      <label className="flex max-w-xs flex-col gap-1 text-sm">
        <span className="text-graphite-600">Dataset a explorar</span>
        <select value={datasetActivo.id} onChange={(e) => setDatasetId(e.target.value)} className={INPUT_CLASS}>
          {datasets.map((d) => (
            <option key={d.id} value={d.id}>
              {d.label}
            </option>
          ))}
        </select>
      </label>
      <ExploradorDataset key={datasetActivo.id} dataset={datasetActivo} />
    </div>
  )
}

// ------------------------------------------------------------------
// Explorador de un dataset (dimensiones/métricas/filtros → resultado)
// ------------------------------------------------------------------

function ExploradorDataset({ dataset }: { dataset: DatasetInfo }) {
  const [dims, setDims] = useState<string[]>([])
  const [measures, setMeasures] = useState<string[]>([])
  const [filtros, setFiltros] = useState<Record<string, string>>({})
  const [snapshot, setSnapshot] = useState('')
  const [resultado, setResultado] = useState<QueryResult | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [nombreTablero, setNombreTablero] = useState('')

  const consultar = useMutation({
    mutationFn: () => {
      const filters: FilterValue[] = dataset.filters
        .map((f) => {
          const valor = filtros[f.id]
          if (!valor) return null
          if (f.type === 'dateFrom' || f.type === 'dateTo') return { id: f.id, values: [valor] }
          return { id: f.id, values: valor.split(',').map((v) => v.trim()).filter(Boolean) }
        })
        .filter((f): f is FilterValue => f !== null && f.values.length > 0)

      return ejecutarConsulta({
        dataset: dataset.id, dimensions: dims, measures, filters,
        snapshot: snapshot || null, limit: 5000,
      })
    },
    onSuccess: setResultado,
  })

  const guardarTablero = useMutation({
    mutationFn: () => {
      const filters: FilterValue[] = dataset.filters
        .map((f) => {
          const valor = filtros[f.id]
          if (!valor) return null
          if (f.type === 'dateFrom' || f.type === 'dateTo') return { id: f.id, values: [valor] }
          return { id: f.id, values: valor.split(',').map((v) => v.trim()).filter(Boolean) }
        })
        .filter((f): f is FilterValue => f !== null && f.values.length > 0)

      const definicion: DefinicionTablero = {
        widgets: [{
          id: crypto.randomUUID(), title: nombreTablero, kind: dims.length > 0 && measures.length > 0 ? 'bar' : 'tabla',
          dataset: dataset.id, dimensions: dims, measures, filters, limit: 5000, layout: { x: 0, y: 0, w: 12, h: 8 },
        }],
      }
      return crearTablero({ nombre: nombreTablero, definicion: JSON.stringify(definicion), esPublico: false, rolesPermitidos: [] })
    },
    onSuccess: () => {
      setGuardando(false)
      setNombreTablero('')
    },
  })

  const toggle = (lista: string[], set: (v: string[]) => void, id: string) =>
    set(lista.includes(id) ? lista.filter((x) => x !== id) : [...lista, id])

  const dimsPorGrupo = useMemo(() => {
    const grupos = new Map<string, typeof dataset.dimensions>()
    for (const d of dataset.dimensions) {
      const g = d.group ?? 'General'
      grupos.set(g, [...(grupos.get(g) ?? []), d])
    }
    return grupos
  }, [dataset.dimensions])

  return (
    <div className="flex flex-col gap-4">
      {dataset.description && <p className="text-sm text-graphite-600">{dataset.description}</p>}

      <div className="glass-card grid grid-cols-1 gap-4 rounded-xl p-4 md:grid-cols-3">
        <div>
          <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Dimensiones</p>
          <div className="flex flex-col gap-2">
            {[...dimsPorGrupo.entries()].map(([grupo, items]) => (
              <div key={grupo}>
                <p className="text-[11px] font-medium text-graphite-500">{grupo}</p>
                {items.map((d) => (
                  <label key={d.id} className="flex items-center gap-1.5 text-sm text-graphite-100">
                    <input type="checkbox" checked={dims.includes(d.id)} onChange={() => toggle(dims, setDims, d.id)} />
                    {d.label}
                  </label>
                ))}
              </div>
            ))}
          </div>
        </div>

        <div>
          <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Métricas</p>
          <div className="flex flex-col gap-1">
            {dataset.measures.map((m) => (
              <label key={m.id} className="flex items-center gap-1.5 text-sm text-graphite-100" title={m.description ?? undefined}>
                <input type="checkbox" checked={measures.includes(m.id)} onChange={() => toggle(measures, setMeasures, m.id)} />
                {m.label}
              </label>
            ))}
          </div>
        </div>

        <div>
          <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Filtros</p>
          <div className="flex flex-col gap-2">
            {dataset.filters.map((f) => (
              <FiltroInput key={f.id} dataset={dataset.id} filtro={f} valor={filtros[f.id] ?? ''} onChange={(v) => setFiltros((s) => ({ ...s, [f.id]: v }))} />
            ))}
            {dataset.supportsSnapshot && (
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Fecha de corte (histórico, opcional)</span>
                <input type="date" value={snapshot} onChange={(e) => setSnapshot(e.target.value)} className={INPUT_CLASS} />
              </label>
            )}
          </div>
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => consultar.mutate()}
          disabled={consultar.isPending || (dims.length === 0 && measures.length === 0)}
          className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          <Play size={14} /> {consultar.isPending ? 'Consultando…' : 'Consultar'}
        </button>

        {!guardando ? (
          <button type="button" onClick={() => setGuardando(true)} className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] px-3 py-2 text-sm text-graphite-100 hover:bg-black/[0.02]">
            <Save size={14} /> Guardar como tablero
          </button>
        ) : (
          <form
            onSubmit={(e) => {
              e.preventDefault()
              guardarTablero.mutate()
            }}
            className="flex items-center gap-2"
          >
            <input
              autoFocus
              required
              value={nombreTablero}
              onChange={(e) => setNombreTablero(e.target.value)}
              placeholder="Nombre del tablero"
              className={`${INPUT_CLASS} w-56`}
            />
            <button type="submit" disabled={guardarTablero.isPending} className="text-sm font-medium text-petrol-700 hover:underline disabled:opacity-50">
              Guardar
            </button>
            <button type="button" onClick={() => setGuardando(false)} className="text-graphite-600 hover:text-graphite-100">
              <X size={16} />
            </button>
          </form>
        )}

        {consultar.isError && <span className="text-xs text-red-700">{mensajeError(consultar.error)}</span>}
        {guardarTablero.isError && <span className="text-xs text-red-700">{mensajeError(guardarTablero.error)}</span>}
        {guardarTablero.isSuccess && <span className="text-xs text-petrol-700">Tablero guardado.</span>}
      </div>

      {resultado && <ResultadoTabla dataset={dataset} resultado={resultado} />}
    </div>
  )
}

function FiltroInput({ dataset, filtro, valor, onChange }: { dataset: string; filtro: DatasetInfo['filters'][number]; valor: string; onChange: (v: string) => void }) {
  const { data: opciones } = useQuery({
    queryKey: ['reporteria-catalogo', dataset, filtro.id],
    queryFn: () => catalogoFiltro(dataset, filtro.id),
    enabled: filtro.hasCatalog,
  })

  if (filtro.type === 'dateFrom' || filtro.type === 'dateTo') {
    return (
      <label className="flex flex-col gap-1 text-sm">
        <span className="text-graphite-600">{filtro.label}</span>
        <input type="date" value={valor} onChange={(e) => onChange(e.target.value)} className={INPUT_CLASS} />
      </label>
    )
  }

  if (filtro.hasCatalog) {
    return (
      <label className="flex flex-col gap-1 text-sm">
        <span className="text-graphite-600">{filtro.label}</span>
        <select value={valor} onChange={(e) => onChange(e.target.value)} className={INPUT_CLASS}>
          <option value="">Todos</option>
          {opciones?.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      </label>
    )
  }

  return (
    <label className="flex flex-col gap-1 text-sm">
      <span className="text-graphite-600">{filtro.label}</span>
      <input value={valor} onChange={(e) => onChange(e.target.value)} placeholder="Separar varios valores con coma" className={INPUT_CLASS} />
    </label>
  )
}

function ResultadoTabla({ dataset, resultado }: { dataset: DatasetInfo; resultado: QueryResult }) {
  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center justify-between">
        <p className="text-xs text-graphite-600">
          {resultado.rows.length} filas · {resultado.elapsedMs} ms
        </p>
        <BotonesExportar
          nombreArchivo={`reporteria-${dataset.id}`}
          titulo={dataset.label}
          columnas={resultado.columns.map((c) => ({ header: c.label, accessor: (fila: Record<string, string | number | null>) => fila[c.id] ?? '' }))}
          filas={resultado.rows}
        />
      </div>
      <TableContainer>
        <thead>
          <tr>
            {resultado.columns.map((c) => (
              <Th key={c.id}>{c.label}</Th>
            ))}
          </tr>
        </thead>
        <tbody>
          {resultado.rows.length === 0 ? (
            <tr>
              <Td colSpan={resultado.columns.length}>
                <EmptyState>Sin resultados para esta combinación.</EmptyState>
              </Td>
            </tr>
          ) : (
            resultado.rows.map((fila, i) => (
              <tr key={i}>
                {resultado.columns.map((c) => (
                  <Td key={c.id}>{formatearValor(fila[c.id], c.format)}</Td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ------------------------------------------------------------------
// Mis tableros
// ------------------------------------------------------------------

function SeccionTableros() {
  const qc = useQueryClient()
  const { data: tableros = [] } = useQuery({ queryKey: ['reporteria-tableros'], queryFn: listarTableros })
  const [abierto, setAbierto] = useState<{ id: number; nombre: string; widgets: DefinicionTablero['widgets'] } | null>(null)

  const toggleFavorito = useMutation({
    mutationFn: (t: { id: number; esFavorito: boolean }) => (t.esFavorito ? quitarFavorito(t.id) : marcarFavorito(t.id)),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['reporteria-tableros'] }),
  })

  const eliminar = useMutation({
    mutationFn: (id: number) => borrarTablero(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['reporteria-tableros'] }),
  })

  const abrir = async (id: number) => {
    const t = await obtenerTablero(id)
    const def = JSON.parse(t.definicion) as DefinicionTablero
    setAbierto({ id, nombre: t.nombre, widgets: def.widgets })
  }

  if (abierto) {
    return (
      <div className="flex flex-col gap-3">
        <div className="flex items-center justify-between">
          <button type="button" onClick={() => setAbierto(null)} className="text-sm font-medium text-petrol-700 hover:underline">
            ← Volver a Mis tableros
          </button>
          <h2 className="text-sm font-bold text-graphite-100">{abierto.nombre}</h2>
        </div>
        {/* Grilla de 12 columnas, misma unidad de layout (x/y/w/h) que guardaba el constructor visual de SIGA -- el orden vertical se resuelve con el orden natural del arreglo (y no se usa para reordenar), simplificación consciente frente al drag-and-drop libre original. */}
        <div className="grid grid-cols-12 gap-4">
          {abierto.widgets.map((w) => (
            <div key={w.id} style={{ gridColumn: `span ${Math.min(w.layout?.w ?? 6, 12)} / span ${Math.min(w.layout?.w ?? 6, 12)}`, minHeight: `${(w.layout?.h ?? 4) * 44}px` }}>
              <WidgetCard widget={w} />
            </div>
          ))}
        </div>
      </div>
    )
  }

  return (
    <TableContainer>
      <thead>
        <tr>
          <Th></Th>
          <Th>Nombre</Th>
          <Th>Datasets</Th>
          <Th>Propietario</Th>
          <Th>Modificado</Th>
          <Th></Th>
        </tr>
      </thead>
      <tbody>
        {tableros.length === 0 ? (
          <tr>
            <Td colSpan={6}>
              <EmptyState>Todavía no guardaste ningún tablero — armá una consulta en cualquier dataset y usá "Guardar como tablero".</EmptyState>
            </Td>
          </tr>
        ) : (
          tableros.map((t) => (
            <tr key={t.id}>
              <Td>
                <button type="button" onClick={() => toggleFavorito.mutate(t)} title={t.esFavorito ? 'Quitar de favoritos' : 'Marcar como favorito'}>
                  <Star size={15} className={t.esFavorito ? 'fill-gold-500 text-gold-500' : 'text-graphite-400'} />
                </button>
              </Td>
              <Td>
                <button type="button" onClick={() => abrir(t.id)} className="font-medium text-petrol-700 hover:underline">
                  {t.nombre}
                </button>
                {t.descripcion && <p className="text-xs text-graphite-600">{t.descripcion}</p>}
              </Td>
              <Td>{t.datasets.join(', ')}</Td>
              <Td>{t.propietario}{t.esMio ? ' (yo)' : ''}</Td>
              <Td>{new Date(t.modificadoEn).toLocaleString('es-EC')}</Td>
              <Td>
                {t.esMio && (
                  <button type="button" onClick={() => confirm(`¿Eliminar "${t.nombre}"?`) && eliminar.mutate(t.id)} className="text-graphite-500 hover:text-red-700" title="Eliminar">
                    <Trash2 size={14} />
                  </button>
                )}
              </Td>
            </tr>
          ))
        )}
      </tbody>
    </TableContainer>
  )
}
