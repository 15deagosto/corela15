import { Fragment, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus, X, Pencil } from 'lucide-react'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

type ApiError = { response?: { data?: { detail?: string } } }

function mensajeError(error: unknown, fallback: string) {
  return (error as ApiError)?.response?.data?.detail ?? fallback
}

function pct(v: number) {
  return `${(v * 100).toFixed(2)}%`
}

const SEGMENTOS_BCE = [
  'Productivo Corporativo',
  'Productivo Empresarial',
  'Productivo PYMES',
  'Consumo Ordinario',
  'Consumo Prioritario',
  'Vivienda de Interés Público',
  'Vivienda',
  'Microcrédito Minorista',
  'Microcrédito Acumulación Simple',
  'Microcrédito Acumulación Ampliada',
]

// Tabla 13 "Tipo de Crédito" real de SEPS (Manual Técnico de Tablas de
// Información v34.0, vigente desde 01/03/2024) — el código que exige el
// campo 14 de la estructura C01 "Operaciones concedidas". Opcional acá
// (todavía no se envía C01 a SEPS), pero clasificar el producto contra
// la tabla oficial desde ahora evita retrofittearlo después.
const TIPOS_CREDITO_SEPS = [
  { codigo: '', nombre: 'Sin clasificar' },
  { codigo: 'CP', nombre: 'CP — Productivo corporativo' },
  { codigo: 'EP', nombre: 'EP — Productivo empresarial' },
  { codigo: 'PY', nombre: 'PY — Productivo pymes' },
  { codigo: 'CO', nombre: 'CO — Consumo' },
  { codigo: 'EC', nombre: 'EC — Educativo' },
  { codigo: 'ES', nombre: 'ES — Educativo social' },
  { codigo: 'VI', nombre: 'VI — Vivienda interés público' },
  { codigo: 'VS', nombre: 'VS — Vivienda interés social' },
  { codigo: 'IN', nombre: 'IN — Inmobiliario' },
  { codigo: 'MI', nombre: 'MI — Microcrédito minorista' },
  { codigo: 'AS', nombre: 'AS — Microcrédito acumulación simple' },
  { codigo: 'AA', nombre: 'AA — Microcrédito acumulación ampliada' },
  { codigo: 'NA', nombre: 'NA — No aplica' },
]

// ---------- Tipos de cuenta (productos de Ahorros) ----------

interface TipoCuenta {
  id: number
  codigo: string
  nombre: string
  saldoMinimo: number
  permiteDebitoPrestamo: boolean
  saldoMinimoConPrestamo: number | null
  tasaInteresAnual: number
  activo: boolean
}

export function TabTiposCuenta() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<TipoCuenta | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [saldoMinimo, setSaldoMinimo] = useState('0')
  const [permiteDebitoPrestamo, setPermiteDebitoPrestamo] = useState(false)
  const [saldoMinimoConPrestamo, setSaldoMinimoConPrestamo] = useState('')
  const [tasaInteresAnual, setTasaInteresAnual] = useState('0')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<TipoCuenta[]>({
    queryKey: ['config-tipos-cuenta'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-cuenta')).data,
  })

  const guardar = useMutation({
    mutationFn: async () => {
      const payload = {
        nombre,
        saldoMinimo: Number(saldoMinimo) || 0,
        permiteDebitoPrestamo,
        saldoMinimoConPrestamo: saldoMinimoConPrestamo ? Number(saldoMinimoConPrestamo) : null,
        tasaInteresAnual: Number(tasaInteresAnual) / 100 || 0,
      }
      return editando
        ? (await api.put(`/api/configuracion/tipos-cuenta/${editando.id}`, { ...payload, activo })).data
        : (await api.post('/api/configuracion/tipos-cuenta', { codigo, ...payload })).data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-tipos-cuenta'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setSaldoMinimo('0')
    setPermiteDebitoPrestamo(false)
    setSaldoMinimoConPrestamo('')
    setTasaInteresAnual('0')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: TipoCuenta) => {
    setEditando(item)
    setNombre(item.nombre)
    setSaldoMinimo(String(item.saldoMinimo))
    setPermiteDebitoPrestamo(item.permiteDebitoPrestamo)
    setSaldoMinimoConPrestamo(item.saldoMinimoConPrestamo != null ? String(item.saldoMinimoConPrestamo) : '')
    setTasaInteresAnual(String(item.tasaInteresAnual * 100))
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Productos de ahorro</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nuevo producto
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre || (!editando && !codigo)) return
              guardar.mutate()
            }}
          >
            <input
              required
              disabled={!!editando}
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-60"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-2"
            />

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Saldo mínimo</span>
              <input
                type="number" step="0.01" min="0"
                value={saldoMinimo}
                onChange={(e) => setSaldoMinimo(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Tasa de interés anual (%)</span>
              <input
                type="number" step="0.01" min="0"
                value={tasaInteresAnual}
                onChange={(e) => setTasaInteresAnual(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Saldo mínimo con préstamo</span>
              <input
                type="number" step="0.01" min="0"
                placeholder="—"
                value={saldoMinimoConPrestamo}
                onChange={(e) => setSaldoMinimoConPrestamo(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>

            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={permiteDebitoPrestamo} onChange={(e) => setPermiteDebitoPrestamo(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
              <span className="text-graphite-600">Permite débito automático de cuota</span>
            </label>
            {editando && (
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                <span className="text-graphite-600">Activo</span>
              </label>
            )}

            <div className="flex items-center gap-2 sm:col-span-3">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : editando ? 'Actualizar' : 'Crear'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-3 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar el producto.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Saldo mínimo</Th>
            <Th>Tasa interés</Th>
            <Th>Débito préstamo</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
              <Td>${item.saldoMinimo.toFixed(2)}</Td>
              <Td>{pct(item.tasaInteresAnual)}</Td>
              <Td>
                <Badge variant={item.permiteDebitoPrestamo ? 'exito' : 'neutral'}>{item.permiteDebitoPrestamo ? 'Sí' : 'No'}</Badge>
              </Td>
              <Td>
                <Badge variant={item.activo ? 'exito' : 'neutral'}>{item.activo ? 'Activo' : 'Inactivo'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => abrirEditar(item)}
                  className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                >
                  <Pencil size={13} /> Editar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Tipos de préstamo (productos de Crédito) ----------

interface TipoPrestamo {
  id: number
  codigo: string
  nombre: string
  montoMinimo: number
  montoMaximo: number
  plazoMinimoDias: number
  plazoMaximoDias: number
  tasaAnual: number
  segmentoBce: string
  activo: boolean
  codigoTipoCreditoSeps: string | null
  codigoTipoSeguro: string | null
}

interface TipoSeguro {
  codigo: string
  nombre: string
  valorMensual: number
  activo: boolean
}

export function TabTiposPrestamo() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<TipoPrestamo | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [montoMinimo, setMontoMinimo] = useState('0')
  const [montoMaximo, setMontoMaximo] = useState('0')
  const [plazoMinimoDias, setPlazoMinimoDias] = useState('30')
  const [plazoMaximoDias, setPlazoMaximoDias] = useState('360')
  const [tasaAnual, setTasaAnual] = useState('0')
  const [segmentoBce, setSegmentoBce] = useState(SEGMENTOS_BCE[4])
  const [codigoTipoCreditoSeps, setCodigoTipoCreditoSeps] = useState('')
  const [codigoTipoSeguro, setCodigoTipoSeguro] = useState('')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<TipoPrestamo[]>({
    queryKey: ['config-tipos-prestamo'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-prestamo')).data,
  })

  const { data: tiposSeguro } = useQuery<TipoSeguro[]>({
    queryKey: ['config-tipos-seguro'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-seguro')).data,
  })

  const guardar = useMutation({
    mutationFn: async () => {
      const payload = {
        nombre,
        montoMinimo: Number(montoMinimo) || 0,
        montoMaximo: Number(montoMaximo) || 0,
        plazoMinimoDias: Number(plazoMinimoDias) || 0,
        plazoMaximoDias: Number(plazoMaximoDias) || 0,
        tasaAnual: Number(tasaAnual) / 100 || 0,
        segmentoBce,
        codigoTipoCreditoSeps: codigoTipoCreditoSeps || null,
        codigoTipoSeguro: codigoTipoSeguro || null,
      }
      return editando
        ? (await api.put(`/api/configuracion/tipos-prestamo/${editando.id}`, { ...payload, activo })).data
        : (await api.post('/api/configuracion/tipos-prestamo', { codigo, ...payload })).data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-tipos-prestamo'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setMontoMinimo('0')
    setMontoMaximo('0')
    setPlazoMinimoDias('30')
    setPlazoMaximoDias('360')
    setTasaAnual('0')
    setSegmentoBce(SEGMENTOS_BCE[4])
    setCodigoTipoCreditoSeps('')
    setCodigoTipoSeguro('')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: TipoPrestamo) => {
    setEditando(item)
    setNombre(item.nombre)
    setMontoMinimo(String(item.montoMinimo))
    setMontoMaximo(String(item.montoMaximo))
    setPlazoMinimoDias(String(item.plazoMinimoDias))
    setPlazoMaximoDias(String(item.plazoMaximoDias))
    setTasaAnual(String(item.tasaAnual * 100))
    setSegmentoBce(item.segmentoBce)
    setCodigoTipoCreditoSeps(item.codigoTipoCreditoSeps ?? '')
    setCodigoTipoSeguro(item.codigoTipoSeguro ?? '')
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Productos de crédito</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nuevo producto
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre || (!editando && !codigo)) return
              guardar.mutate()
            }}
          >
            <input
              required
              disabled={!!editando}
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-60"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-2"
            />

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Monto mínimo</span>
              <input type="number" step="0.01" min="0" value={montoMinimo} onChange={(e) => setMontoMinimo(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Monto máximo</span>
              <input type="number" step="0.01" min="0" value={montoMaximo} onChange={(e) => setMontoMaximo(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Tasa anual (%)</span>
              <input type="number" step="0.01" min="0" value={tasaAnual} onChange={(e) => setTasaAnual(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Plazo mínimo (días)</span>
              <input type="number" min="1" value={plazoMinimoDias} onChange={(e) => setPlazoMinimoDias(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Plazo máximo (días)</span>
              <input type="number" min="1" value={plazoMaximoDias} onChange={(e) => setPlazoMaximoDias(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Segmento BCE (techo regulatorio)</span>
              <select value={segmentoBce} onChange={(e) => setSegmentoBce(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50">
                {SEGMENTOS_BCE.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Tipo de crédito SEPS (Tabla 13, para C01)</span>
              <select value={codigoTipoCreditoSeps} onChange={(e) => setCodigoTipoCreditoSeps(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50">
                {TIPOS_CREDITO_SEPS.map((t) => (
                  <option key={t.codigo} value={t.codigo}>{t.nombre}</option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Seguro de desgravamen (flat, opcional)</span>
              <select value={codigoTipoSeguro} onChange={(e) => setCodigoTipoSeguro(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50">
                <option value="">Sin seguro</option>
                {tiposSeguro?.filter((t) => t.activo).map((t) => (
                  <option key={t.codigo} value={t.codigo}>{t.nombre} (${t.valorMensual.toFixed(2)}/mes)</option>
                ))}
              </select>
            </label>

            {editando && (
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                <span className="text-graphite-600">Activo</span>
              </label>
            )}

            <div className="flex items-center gap-2 sm:col-span-3">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : editando ? 'Actualizar' : 'Crear'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-3 text-sm text-red-700">
                {mensajeError(guardar.error, 'No se pudo guardar el producto. Verifique que la tasa no exceda el techo BCE del segmento.')}
              </p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Monto</Th>
            <Th>Plazo</Th>
            <Th>Tasa</Th>
            <Th>Segmento BCE</Th>
            <Th>Tipo SEPS</Th>
            <Th>Seguro</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
              <Td>${item.montoMinimo.toFixed(0)} – ${item.montoMaximo.toFixed(0)}</Td>
              <Td>{item.plazoMinimoDias}–{item.plazoMaximoDias}d</Td>
              <Td>{pct(item.tasaAnual)}</Td>
              <Td className="text-xs">{item.segmentoBce}</Td>
              <Td>
                {item.codigoTipoCreditoSeps ? <Badge>{item.codigoTipoCreditoSeps}</Badge> : <span className="text-graphite-700">—</span>}
              </Td>
              <Td>
                {item.codigoTipoSeguro
                  ? <Badge variant="alerta">{tiposSeguro?.find((t) => t.codigo === item.codigoTipoSeguro)?.nombre ?? item.codigoTipoSeguro}</Badge>
                  : <span className="text-graphite-700">—</span>}
              </Td>
              <Td>
                <Badge variant={item.activo ? 'exito' : 'neutral'}>{item.activo ? 'Activo' : 'Inactivo'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => abrirEditar(item)}
                  className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                >
                  <Pencil size={13} /> Editar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Tasas techo BCE ----------

interface TasaTechoBce {
  id: number
  segmento: string
  tasaMaxima: number
  fechaVigenciaDesde: string
  activo: boolean
}

export function TabTasasTechoBce() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<TasaTechoBce | null>(null)
  const [segmento, setSegmento] = useState(SEGMENTOS_BCE[0])
  const [tasaMaxima, setTasaMaxima] = useState('0')
  const [fechaVigenciaDesde, setFechaVigenciaDesde] = useState('')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<TasaTechoBce[]>({
    queryKey: ['config-tasas-techo-bce'],
    queryFn: async () => (await api.get('/api/configuracion/tasas-techo-bce')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? (await api.put(`/api/configuracion/tasas-techo-bce/${editando.id}`, { tasaMaxima: Number(tasaMaxima) / 100, activo })).data
        : (
            await api.post('/api/configuracion/tasas-techo-bce', {
              segmento,
              tasaMaxima: Number(tasaMaxima) / 100,
              fechaVigenciaDesde,
            })
          ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-tasas-techo-bce'] })
      queryClient.invalidateQueries({ queryKey: ['config-tipos-prestamo'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setSegmento(SEGMENTOS_BCE[0])
    setTasaMaxima('0')
    setFechaVigenciaDesde('')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: TasaTechoBce) => {
    setEditando(item)
    setTasaMaxima(String(item.tasaMaxima * 100))
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <div>
          <h3 className="text-sm font-medium text-graphite-600">Tasas techo BCE</h3>
          <p className="text-xs text-graphite-600">
            Publicadas mensualmente por la Junta de Política y Regulación Monetaria y Financiera — nunca se edita una
            tasa histórica, se agrega una fila nueva con su fecha de vigencia.
          </p>
        </div>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex shrink-0 items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nueva tasa
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!editando && !fechaVigenciaDesde) return
              guardar.mutate()
            }}
          >
            {!editando && (
              <>
                <select value={segmento} onChange={(e) => setSegmento(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-2">
                  {SEGMENTOS_BCE.map((s) => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
                <input
                  required
                  type="date"
                  value={fechaVigenciaDesde}
                  onChange={(e) => setFechaVigenciaDesde(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </>
            )}
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Tasa máxima (%)</span>
              <input type="number" step="0.01" min="0" value={tasaMaxima} onChange={(e) => setTasaMaxima(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            {editando && (
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                <span className="text-graphite-600">Activa</span>
              </label>
            )}

            <div className="flex items-center gap-2 sm:col-span-3">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : editando ? 'Actualizar' : 'Crear'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-3 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar la tasa.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Segmento</Th>
            <Th>Tasa máxima</Th>
            <Th>Vigente desde</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.segmento}</Td>
              <Td>{pct(item.tasaMaxima)}</Td>
              <Td>{item.fechaVigenciaDesde}</Td>
              <Td>
                <Badge variant={item.activo ? 'exito' : 'neutral'}>{item.activo ? 'Activa' : 'Inactiva'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => abrirEditar(item)}
                  className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                >
                  <Pencil size={13} /> Editar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Tablero de tasas DPF ----------

interface ItemPlazoTasa {
  id: string
  plazoDiasMin: number
  plazoDiasMax: number
  montoMin: number
  montoMax: number | null
  tipoPersona: string
  tasa: number
  fechaVigenciaDesde: string
  activo: boolean
}

export function TabTableroTasasDpf() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<ItemPlazoTasa | null>(null)
  const [plazoDiasMin, setPlazoDiasMin] = useState('30')
  const [plazoDiasMax, setPlazoDiasMax] = useState('90')
  const [montoMin, setMontoMin] = useState('50')
  const [montoMax, setMontoMax] = useState('')
  const [tipoPersona, setTipoPersona] = useState('Ambas')
  const [tasa, setTasa] = useState('0')
  const [fechaVigenciaDesde, setFechaVigenciaDesde] = useState('')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<ItemPlazoTasa[]>({
    queryKey: ['config-tablero-dpf'],
    queryFn: async () => (await api.get('/api/configuracion/tablero-tasas-dpf')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? (await api.put(`/api/configuracion/tablero-tasas-dpf/${editando.id}`, { tasa: Number(tasa) / 100, activo })).data
        : (
            await api.post('/api/configuracion/tablero-tasas-dpf', {
              plazoDiasMin: Number(plazoDiasMin) || 0,
              plazoDiasMax: Number(plazoDiasMax) || 0,
              montoMin: Number(montoMin) || 0,
              montoMax: montoMax ? Number(montoMax) : null,
              tipoPersona,
              tasa: Number(tasa) / 100,
              fechaVigenciaDesde,
            })
          ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-tablero-dpf'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setPlazoDiasMin('30')
    setPlazoDiasMax('90')
    setMontoMin('50')
    setMontoMax('')
    setTipoPersona('Ambas')
    setTasa('0')
    setFechaVigenciaDesde('')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: ItemPlazoTasa) => {
    setEditando(item)
    setTasa(String(item.tasa * 100))
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Tablero de tasas — Depósitos a Plazo Fijo</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nuevo rango
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!editando && !fechaVigenciaDesde) return
              guardar.mutate()
            }}
          >
            {!editando && (
              <>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-xs text-graphite-600">Plazo mínimo (días)</span>
                  <input type="number" min="1" value={plazoDiasMin} onChange={(e) => setPlazoDiasMin(e.target.value)}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-xs text-graphite-600">Plazo máximo (días)</span>
                  <input type="number" min="1" value={plazoDiasMax} onChange={(e) => setPlazoDiasMax(e.target.value)}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-xs text-graphite-600">Tipo de persona</span>
                  <select value={tipoPersona} onChange={(e) => setTipoPersona(e.target.value)}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50">
                    <option value="Ambas">Ambas</option>
                    <option value="Natural">Natural</option>
                    <option value="Juridica">Jurídica</option>
                  </select>
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-xs text-graphite-600">Monto mínimo</span>
                  <input type="number" step="0.01" min="0" value={montoMin} onChange={(e) => setMontoMin(e.target.value)}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-xs text-graphite-600">Monto máximo (vacío = sin tope)</span>
                  <input type="number" step="0.01" min="0" value={montoMax} onChange={(e) => setMontoMax(e.target.value)}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-xs text-graphite-600">Vigente desde</span>
                  <input required type="date" value={fechaVigenciaDesde} onChange={(e) => setFechaVigenciaDesde(e.target.value)}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
                </label>
              </>
            )}
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Tasa (%)</span>
              <input type="number" step="0.01" min="0" value={tasa} onChange={(e) => setTasa(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            {editando && (
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                <span className="text-graphite-600">Activo</span>
              </label>
            )}

            <div className="flex items-center gap-2 sm:col-span-3">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : editando ? 'Actualizar' : 'Crear'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-3 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar el rango de tasa.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Plazo</Th>
            <Th>Monto</Th>
            <Th>Tipo de persona</Th>
            <Th>Tasa</Th>
            <Th>Vigente desde</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.plazoDiasMin}–{item.plazoDiasMax}d</Td>
              <Td>${item.montoMin.toFixed(0)}{item.montoMax != null ? ` – $${item.montoMax.toFixed(0)}` : '+'}</Td>
              <Td>{item.tipoPersona}</Td>
              <Td>{pct(item.tasa)}</Td>
              <Td>{item.fechaVigenciaDesde}</Td>
              <Td>
                <Badge variant={item.activo ? 'exito' : 'neutral'}>{item.activo ? 'Activo' : 'Inactivo'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => abrirEditar(item)}
                  className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                >
                  <Pencil size={13} /> Editar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Categorías de riesgo de cartera (matriz A1-E) ----------

interface CategoriaRiesgoCartera {
  id: number
  codigo: string
  nombre: string
  diasMoraInicio: number
  diasMoraFin: number
  porcentajeProvision: number
  activo: boolean
}

export function TabCategoriasRiesgoCartera() {
  const queryClient = useQueryClient()
  const [editando, setEditando] = useState<CategoriaRiesgoCartera | null>(null)
  const [diasMoraInicio, setDiasMoraInicio] = useState('0')
  const [diasMoraFin, setDiasMoraFin] = useState('0')
  const [porcentajeProvision, setPorcentajeProvision] = useState('0')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<CategoriaRiesgoCartera[]>({
    queryKey: ['config-categorias-riesgo'],
    queryFn: async () => (await api.get('/api/configuracion/categorias-riesgo-cartera')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      (
        await api.put(`/api/configuracion/categorias-riesgo-cartera/${editando!.id}`, {
          diasMoraInicio: Number(diasMoraInicio) || 0,
          diasMoraFin: Number(diasMoraFin) || 0,
          porcentajeProvision: Number(porcentajeProvision) / 100 || 0,
          activo,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-categorias-riesgo'] })
      setEditando(null)
    },
  })

  const abrirEditar = (item: CategoriaRiesgoCartera) => {
    setEditando(item)
    setDiasMoraInicio(String(item.diasMoraInicio))
    setDiasMoraFin(String(item.diasMoraFin))
    setPorcentajeProvision(String(item.porcentajeProvision * 100))
    setActivo(item.activo)
  }

  return (
    <div>
      <div className="mb-4">
        <h3 className="text-sm font-medium text-graphite-600">Matriz de calificación de riesgo de cartera</h3>
        <p className="text-xs text-graphite-600">
          Norma para la Gestión del Riesgo de Crédito en las COAC, Art. 44 — 9 categorías fijas (A1-E), no se crean
          categorías nuevas, solo se ajustan sus rangos de días de mora y % de provisión.
        </p>
      </div>

      {editando && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-4"
            onSubmit={(e) => {
              e.preventDefault()
              guardar.mutate()
            }}
          >
            <p className="text-sm font-medium text-graphite-100 sm:col-span-4">
              Editando {editando.codigo} — {editando.nombre}
            </p>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Días de mora desde</span>
              <input type="number" min="0" value={diasMoraInicio} onChange={(e) => setDiasMoraInicio(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Días de mora hasta</span>
              <input type="number" min="0" value={diasMoraFin} onChange={(e) => setDiasMoraFin(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">% de provisión</span>
              <input type="number" step="0.01" min="0" max="100" value={porcentajeProvision} onChange={(e) => setPorcentajeProvision(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50" />
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
              <span className="text-graphite-600">Activa</span>
            </label>

            <div className="flex items-center gap-2 sm:col-span-4">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : 'Actualizar'}
              </button>
              <button type="button" onClick={() => setEditando(null)} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-4 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar la categoría.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Categoría</Th>
            <Th>Rango de mora</Th>
            <Th>% Provisión</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo} — {item.nombre}</Td>
              <Td>{item.diasMoraInicio}–{item.diasMoraFin === 999999 ? '∞' : item.diasMoraFin} días</Td>
              <Td>{pct(item.porcentajeProvision)}</Td>
              <Td>
                <Badge variant={item.activo ? 'exito' : 'neutral'}>{item.activo ? 'Activa' : 'Inactiva'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => abrirEditar(item)}
                  className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                >
                  <Pencil size={13} /> Editar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Grupos de aprobadores (motor de aprobaciones — FlujoTrabajo) ----------

interface GrupoContable {
  codigo: string
  nombre: string
  montoMinimo: number
  montoMaximo: number
  activo: boolean
}

interface GrupoContableUsuarioItem {
  id: number
  idUsuario: string
  nombreUsuario: string
  activo: boolean
}

interface UsuarioBusqueda {
  id: string
  nombreUsuario: string
}

function BuscarUsuario({ onSeleccionar }: { onSeleccionar: (u: UsuarioBusqueda) => void }) {
  const [q, setQ] = useState('')
  const { data: usuarios } = useQuery<{ id: string; nombreUsuario: string }[]>({
    queryKey: ['config-buscar-usuario', q],
    queryFn: async () => (await api.get('/api/usuarios', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  return (
    <div className="flex flex-col gap-1">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar usuario para agregar al grupo…"
        className="rounded-lg border border-black/[0.08] bg-white px-3 py-1.5 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
      />
      {q.length >= 2 && (usuarios?.length ?? 0) > 0 && (
        <div className="max-h-32 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
          {usuarios?.map((u) => (
            <button
              key={u.id}
              type="button"
              onClick={() => {
                onSeleccionar(u)
                setQ('')
              }}
              className="block w-full px-3 py-1.5 text-left text-xs hover:bg-black/[0.02]"
            >
              {u.nombreUsuario}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

function MiembrosDelGrupo({ codigo, onClose }: { codigo: string; onClose: () => void }) {
  const queryClient = useQueryClient()

  const { data: usuarios, isLoading } = useQuery<GrupoContableUsuarioItem[]>({
    queryKey: ['config-grupo-contable-usuarios', codigo],
    queryFn: async () => (await api.get(`/api/configuracion/flujo-trabajo/grupos-contables/${codigo}/usuarios`)).data,
  })

  const agregar = useMutation({
    mutationFn: async (idUsuario: string) =>
      api.post(`/api/configuracion/flujo-trabajo/grupos-contables/${codigo}/usuarios`, { idUsuario }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['config-grupo-contable-usuarios', codigo] }),
  })

  const quitar = useMutation({
    mutationFn: async (id: number) => api.delete(`/api/configuracion/flujo-trabajo/grupos-contables-usuarios/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['config-grupo-contable-usuarios', codigo] }),
  })

  return (
    <tr>
      <Td colSpan={5}>
        <div className="glass-card animate-zoom-in rounded-xl p-4">
          <div className="mb-3 flex items-center justify-between">
            <h4 className="text-sm font-medium text-graphite-100">Usuarios autorizados a decidir en {codigo}</h4>
            <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
              <X size={16} />
            </button>
          </div>

          {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}

          <ul className="mb-3 flex flex-col gap-1">
            {(usuarios?.filter((u) => u.activo).length ?? 0) === 0 && (
              <li className="text-xs text-graphite-600">Ningún usuario activo en este grupo — nadie podrá aprobar esta etapa.</li>
            )}
            {usuarios?.filter((u) => u.activo).map((u) => (
              <li key={u.id} className="flex items-center justify-between text-xs">
                <span>{u.nombreUsuario}</span>
                <button
                  type="button"
                  disabled={quitar.isPending}
                  onClick={() => quitar.mutate(u.id)}
                  className="text-graphite-600 hover:text-red-700 disabled:opacity-50"
                >
                  Quitar
                </button>
              </li>
            ))}
          </ul>

          <BuscarUsuario onSeleccionar={(u) => agregar.mutate(u.id)} />
        </div>
      </Td>
    </tr>
  )
}

interface EtapaFlujo {
  id: number
  nombre: string
  tipoEtapa: string
  orden: number
  activa: boolean
}

interface EtapaGrupoRuteo {
  id: number
  etapa: string
  agencia: string
  codigoGrupoContable: string
  activa: boolean
}

function EtapasDelMotor() {
  const [etapaAbierta, setEtapaAbierta] = useState<number | null>(null)

  const { data: etapas, isLoading } = useQuery<EtapaFlujo[]>({
    queryKey: ['config-flujo-trabajo-etapas'],
    queryFn: async () => (await api.get('/api/configuracion/flujo-trabajo/etapas')).data,
  })

  const { data: ruteo } = useQuery<EtapaGrupoRuteo[]>({
    queryKey: ['config-flujo-trabajo-etapa-grupos', etapaAbierta],
    queryFn: async () => (await api.get(`/api/configuracion/flujo-trabajo/etapas/${etapaAbierta}/grupos`)).data,
    enabled: etapaAbierta !== null,
  })

  return (
    <div className="mb-6">
      <h3 className="mb-2 text-sm font-medium text-graphite-600">Etapas del motor (estructura, solo lectura)</h3>
      <p className="mb-3 text-xs text-graphite-600">
        La secuencia de etapas y a qué grupo rutea cada una es estructura del motor — se ajusta por migración, no
        desde acá (cambiarla mal rompería el flujo real). Lo que sí se administra abajo es quién integra cada grupo.
      </p>
      <TableContainer>
        <thead>
          <tr>
            <Th>Orden</Th>
            <Th>Etapa</Th>
            <Th>Tipo</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {etapas?.map((e) => (
            <Fragment key={e.id}>
              <tr className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="tabular-nums">{e.orden}</Td>
                <Td className="font-medium">{e.nombre}</Td>
                <Td>{e.tipoEtapa}</Td>
                <Td>
                  <Badge variant={e.activa ? 'exito' : 'neutral'}>{e.activa ? 'Activa' : 'Inactiva'}</Badge>
                </Td>
                <Td>
                  <button
                    type="button"
                    onClick={() => setEtapaAbierta(etapaAbierta === e.id ? null : e.id)}
                    className="text-xs font-medium text-gold-400 hover:underline"
                  >
                    Ver ruteo
                  </button>
                </Td>
              </tr>
              {etapaAbierta === e.id && (
                <tr>
                  <Td colSpan={5}>
                    <div className="rounded-lg border border-black/[0.08] bg-white p-2 text-xs">
                      {(ruteo?.length ?? 0) === 0 && <p className="text-graphite-600">Sin ruteo configurado para esta etapa.</p>}
                      {ruteo?.map((r) => (
                        <div key={r.id} className="flex items-center justify-between py-0.5">
                          <span>{r.agencia} → grupo <strong>{r.codigoGrupoContable}</strong></span>
                          <Badge variant={r.activa ? 'exito' : 'neutral'}>{r.activa ? 'Activo' : 'Inactivo'}</Badge>
                        </div>
                      ))}
                    </div>
                  </Td>
                </tr>
              )}
            </Fragment>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

export function TabGruposContables() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<GrupoContable | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [montoMinimo, setMontoMinimo] = useState('0.01')
  const [montoMaximo, setMontoMaximo] = useState('999999999')
  const [activo, setActivo] = useState(true)
  const [grupoAbierto, setGrupoAbierto] = useState<string | null>(null)

  const { data, isLoading } = useQuery<GrupoContable[]>({
    queryKey: ['config-grupos-contables'],
    queryFn: async () => (await api.get('/api/configuracion/flujo-trabajo/grupos-contables')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? api.put(`/api/configuracion/flujo-trabajo/grupos-contables/${editando.codigo}`, {
            nombre, montoMinimo: Number(montoMinimo), montoMaximo: Number(montoMaximo), activo,
          })
        : api.post('/api/configuracion/flujo-trabajo/grupos-contables', {
            codigo, nombre, montoMinimo: Number(montoMinimo), montoMaximo: Number(montoMaximo),
          }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-grupos-contables'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setMontoMinimo('0.01')
    setMontoMaximo('999999999')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: GrupoContable) => {
    setEditando(item)
    setCodigo(item.codigo)
    setNombre(item.nombre)
    setMontoMinimo(item.montoMinimo.toString())
    setMontoMaximo(item.montoMaximo.toString())
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <p className="mb-4 text-xs text-graphite-600">
        Motor real de aprobaciones (verificado contra FLUJOTRABAJO.GRUPO_CONTABLE de Softbank) — cada grupo autoriza
        montos dentro de su rango. El Comité de Crédito (código <code>COM-CRED</code>) es el que valida
        Aprobar/Rechazar en Créditos → Solicitudes: si un usuario no está en un grupo activo cuyo rango cubra el
        monto, no puede decidir esa solicitud aunque tenga acceso al menú.
      </p>

      <EtapasDelMotor />

      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Grupos de aprobadores</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nuevo
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-4 sm:items-center"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre || (!editando && !codigo)) return
              guardar.mutate()
            }}
          >
            <input
              required
              disabled={!!editando}
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value.toUpperCase())}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-50"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              type="number"
              step="0.01"
              placeholder="Monto mínimo"
              value={montoMinimo}
              onChange={(e) => setMontoMinimo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              type="number"
              step="0.01"
              placeholder="Monto máximo"
              value={montoMaximo}
              onChange={(e) => setMontoMaximo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            {editando && (
              <label className="flex items-center gap-1.5 text-sm text-graphite-600">
                <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} /> Activo
              </label>
            )}
            <div className="flex items-center gap-2 sm:col-span-4">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : editando ? 'Actualizar' : 'Crear'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-4 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar el grupo.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Rango de monto</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {data?.map((g) => (
            <Fragment key={g.codigo}>
              <tr className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{g.codigo}</Td>
                <Td>{g.nombre}</Td>
                <Td className="tabular-nums">
                  ${g.montoMinimo.toFixed(2)} – ${g.montoMaximo.toFixed(2)}
                </Td>
                <Td>
                  <Badge variant={g.activo ? 'exito' : 'neutral'}>{g.activo ? 'Activo' : 'Inactivo'}</Badge>
                </Td>
                <Td>
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => abrirEditar(g)}
                      className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                    >
                      <Pencil size={13} /> Editar
                    </button>
                    <button
                      type="button"
                      onClick={() => setGrupoAbierto(grupoAbierto === g.codigo ? null : g.codigo)}
                      className="text-xs font-medium text-gold-400 hover:underline"
                    >
                      Usuarios
                    </button>
                  </div>
                </Td>
              </tr>
              {grupoAbierto === g.codigo && (
                <MiembrosDelGrupo codigo={g.codigo} onClose={() => setGrupoAbierto(null)} />
              )}
            </Fragment>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Tipos de convenio (empleador/sindicato con descuento vía rol de pagos) ----------

interface TipoConvenio {
  codigo: string
  nombre: string
  idAgencia: number
  agencia: string
  esCooperativa: boolean
  valorAhorro: number
  activo: boolean
}

interface AgenciaSimple {
  id: number
  nombre: string
  activa: boolean
}

export function TabTiposConvenio() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<TipoConvenio | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [idAgencia, setIdAgencia] = useState('')
  const [esCooperativa, setEsCooperativa] = useState(false)
  const [valorAhorro, setValorAhorro] = useState('0')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<TipoConvenio[]>({
    queryKey: ['config-tipos-convenio'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-convenio')).data,
  })

  const { data: agencias } = useQuery<AgenciaSimple[]>({
    queryKey: ['config-agencias-simple'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const guardar = useMutation({
    mutationFn: async () => {
      const payload = { nombre, idAgencia: Number(idAgencia), esCooperativa, valorAhorro: Number(valorAhorro) || 0 }
      return editando
        ? (await api.put(`/api/configuracion/tipos-convenio/${editando.codigo}`, { ...payload, activo })).data
        : (await api.post('/api/configuracion/tipos-convenio', { codigo, ...payload })).data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-tipos-convenio'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setIdAgencia(agencias?.[0] ? String(agencias[0].id) : '')
    setEsCooperativa(false)
    setValorAhorro('0')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: TipoConvenio) => {
    setEditando(item)
    setNombre(item.nombre)
    setIdAgencia(String(item.idAgencia))
    setEsCooperativa(item.esCooperativa)
    setValorAhorro(String(item.valorAhorro))
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">
          Convenios (empleador/sindicato con descuento vía rol de pagos)
        </h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nuevo convenio
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre || !idAgencia || (!editando && !codigo)) return
              guardar.mutate()
            }}
          >
            <input
              required
              disabled={!!editando}
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-60"
            />
            <input
              required
              placeholder="Nombre del convenio (empleador/sindicato)"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-2"
            />

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Agencia</span>
              <select
                required
                value={idAgencia}
                onChange={(e) => setIdAgencia(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar…</option>
                {agencias?.filter((a) => a.activa).map((a) => (
                  <option key={a.id} value={a.id}>{a.nombre}</option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-xs text-graphite-600">Valor ahorro (opcional)</span>
              <input
                type="number" step="0.01" min="0"
                value={valorAhorro}
                onChange={(e) => setValorAhorro(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={esCooperativa} onChange={(e) => setEsCooperativa(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
              <span className="text-graphite-600">Es cooperativa</span>
            </label>

            {editando && (
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                <span className="text-graphite-600">Activo</span>
              </label>
            )}

            <div className="flex items-center gap-2 sm:col-span-3">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : editando ? 'Actualizar' : 'Crear'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-3 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar el convenio.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Agencia</Th>
            <Th>Es cooperativa</Th>
            <Th>Valor ahorro</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin convenios registrados</EmptyState>}
          {data?.map((item) => (
            <tr key={item.codigo} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
              <Td>{item.agencia}</Td>
              <Td>
                <Badge variant={item.esCooperativa ? 'exito' : 'neutral'}>{item.esCooperativa ? 'Sí' : 'No'}</Badge>
              </Td>
              <Td>${item.valorAhorro.toFixed(2)}</Td>
              <Td>
                <Badge variant={item.activo ? 'exito' : 'neutral'}>{item.activo ? 'Activo' : 'Inactivo'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => abrirEditar(item)}
                  className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                >
                  <Pencil size={13} /> Editar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
