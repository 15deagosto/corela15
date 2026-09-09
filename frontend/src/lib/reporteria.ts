import { api } from './api'

export interface ItemInfo {
  id: string
  label: string
  group: string | null
}

export interface MeasureInfo {
  id: string
  label: string
  format: 'money' | 'pct' | 'int' | 'num'
  description: string | null
  higherIsWorse: boolean
}

export interface FilterInfo {
  id: string
  label: string
  type: 'multi' | 'single' | 'dateFrom' | 'dateTo'
  hasCatalog: boolean
}

export interface DatasetInfo {
  id: string
  label: string
  description: string | null
  supportsSnapshot: boolean
  dimensions: ItemInfo[]
  measures: MeasureInfo[]
  filters: FilterInfo[]
}

export interface FilterValue {
  id: string
  values: string[]
}

export interface QueryRequestBody {
  dataset: string
  dimensions: string[]
  measures: string[]
  filters: FilterValue[]
  snapshot?: string | null
  orderBy?: string | null
  orderDesc?: boolean
  limit?: number
}

export interface ColumnMeta {
  id: string
  label: string
  kind: 'dimension' | 'measure'
  format: string
}

export interface QueryResult {
  columns: ColumnMeta[]
  rows: Record<string, string | number | null>[]
  elapsedMs: number
  truncated: boolean
}

export interface CatalogItem {
  value: string
  label: string
}

export async function listarDatasets(): Promise<DatasetInfo[]> {
  const { data } = await api.get('/api/reporteria/datasets')
  return data
}

export async function ejecutarConsulta(body: QueryRequestBody): Promise<QueryResult> {
  const { data } = await api.post('/api/reporteria/query', body)
  return data
}

export async function catalogoFiltro(dataset: string, filtroId: string): Promise<CatalogItem[]> {
  const { data } = await api.get(`/api/reporteria/${dataset}/filtros/${filtroId}/catalogo`)
  return data
}

// --- Tableros guardados ---

export interface TableroResumen {
  id: number
  nombre: string
  descripcion: string | null
  propietario: string
  esPublico: boolean
  rolesPermitidos: string[]
  esPredefinido: boolean
  modificadoEn: string
  esMio: boolean
  esFavorito: boolean
  datasets: string[]
}

export interface Tablero extends Omit<TableroResumen, 'modificadoEn' | 'datasets'> {
  definicion: string
  creadoEn: string
  modificadoEn: string
}

/** Tipo de visualización de un widget, mismo catálogo que usaba el constructor visual de SIGA. */
export type TipoWidget = 'kpi' | 'bar' | 'line' | 'pie' | 'tabla'

export interface Widget {
  id: string
  title: string
  kind: TipoWidget
  dataset: string
  dimensions: string[]
  measures: string[]
  filters: FilterValue[]
  orderBy?: string | null
  orderDesc?: boolean
  limit?: number
  layout: { x: number; y: number; w: number; h: number }
}

export interface DefinicionTablero {
  widgets: Widget[]
}

export async function listarTableros(): Promise<TableroResumen[]> {
  const { data } = await api.get('/api/reporteria/tableros')
  return data
}

export async function obtenerTablero(id: number): Promise<Tablero> {
  const { data } = await api.get(`/api/reporteria/tableros/${id}`)
  return data
}

export async function crearTablero(body: { nombre: string; descripcion?: string | null; definicion: string; esPublico: boolean; rolesPermitidos: string[] }): Promise<Tablero> {
  const { data } = await api.post('/api/reporteria/tableros', body)
  return data
}

export async function borrarTablero(id: number): Promise<void> {
  await api.delete(`/api/reporteria/tableros/${id}`)
}

export async function marcarFavorito(id: number): Promise<void> {
  await api.post(`/api/reporteria/tableros/${id}/favorito`)
}

export async function quitarFavorito(id: number): Promise<void> {
  await api.delete(`/api/reporteria/tableros/${id}/favorito`)
}

// --- formato es-EC, portado tal cual de siga-web/src/lib/api.ts ---

const nfMoney = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 })
const nfInt = new Intl.NumberFormat('es-EC', { maximumFractionDigits: 0 })
const nfNum = new Intl.NumberFormat('es-EC', { minimumFractionDigits: 2, maximumFractionDigits: 2 })

export function formatearValor(valor: unknown, formato: string): string {
  if (valor === null || valor === undefined) return '—'
  const n = Number(valor)
  switch (formato) {
    case 'money':
      return Number.isNaN(n) ? String(valor) : nfMoney.format(n)
    case 'pct':
      return Number.isNaN(n) ? String(valor) : `${nfNum.format(n)} %`
    case 'int':
      return Number.isNaN(n) ? String(valor) : nfInt.format(n)
    case 'num':
      return Number.isNaN(n) ? String(valor) : nfNum.format(n)
    default:
      return String(valor)
  }
}

/** Compacto para ejes de gráficos: 1,2 M / 850 k */
export function formatCompact(n: number): string {
  if (Math.abs(n) >= 1_000_000) return `${nfNum.format(n / 1_000_000)} M`
  if (Math.abs(n) >= 1_000) return `${nfInt.format(n / 1_000)} k`
  return nfInt.format(n)
}
