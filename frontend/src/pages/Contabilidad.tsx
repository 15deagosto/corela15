import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Calculator, Lock, AlertTriangle, FileBarChart } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { BotonesExportar } from '../components/BotonesExportar'
import type { ColumnaExportable } from '../lib/exportar'
import { api } from '../lib/api'

interface CuentaContable {
  id: string
  codigo: string
  nombre: string
  grupo: string
  naturaleza: string
  esMayor: boolean
  activa: boolean
  codigoPadre: string | null
}

interface PeriodoContable {
  periodo: string
  cerrado: boolean
  fechaCierre: string | null
  cerradoPor: string | null
}

type ApiError = { response?: { data?: { detail?: string } } }

function mensajeError(error: unknown, fallback: string) {
  return (error as ApiError)?.response?.data?.detail ?? fallback
}

interface BalanceComprobacionLinea {
  codigo: string
  nombre: string
  grupo: string
  saldoInicial: number
  debitos: number
  creditos: number
  saldoFinal: number
}

interface BalanceComprobacionResult {
  periodo: string
  lineas: BalanceComprobacionLinea[]
  totalDebitos: number
  totalCreditos: number
  cuadrado: boolean
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function SeccionBalanceComprobacion() {
  const hoy = new Date()
  const [periodo, setPeriodo] = useState(
    `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}`,
  )

  const { data: periodosDisponibles } = useQuery<string[]>({
    queryKey: ['contabilidad-reportes-periodos'],
    queryFn: async () => (await api.get('/api/contabilidad/reportes/periodos')).data,
  })

  const { data: balance, isLoading } = useQuery<BalanceComprobacionResult>({
    queryKey: ['contabilidad-balance-comprobacion', periodo],
    queryFn: async () =>
      (await api.get('/api/contabilidad/reportes/balance-comprobacion', { params: { periodo: `${periodo}-01` } })).data,
    enabled: !!periodo,
  })

  return (
    <div>
      <div className="glass-card mb-4 rounded-xl p-4">
        <div className="flex flex-wrap items-end justify-between gap-3">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Período</span>
            <input
              type="month"
              value={periodo}
              onChange={(e) => setPeriodo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          {periodosDisponibles && periodosDisponibles.length > 0 && (
            <div className="flex flex-wrap gap-1.5 text-xs text-graphite-600">
              <span>Con movimientos:</span>
              {periodosDisponibles.map((p) => (
                <button
                  key={p}
                  type="button"
                  onClick={() => setPeriodo(p.slice(0, 7))}
                  className="rounded-full bg-graphite-950 px-2 py-0.5 hover:bg-gold-500/15 hover:text-gold-400"
                >
                  {p.slice(0, 7)}
                </button>
              ))}
            </div>
          )}
          {balance && (
            <Badge variant={balance.cuadrado ? 'exito' : 'peligro'}>
              {balance.cuadrado ? 'Cuadrado' : 'Descuadrado'}
            </Badge>
          )}
        </div>
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Cuenta</Th>
            <Th>Saldo inicial</Th>
            <Th>Débitos</Th>
            <Th>Créditos</Th>
            <Th>Saldo final</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (balance?.lineas.length ?? 0) === 0 && (
            <EmptyState>Sin movimientos registrados en este período</EmptyState>
          )}
          {balance?.lineas.map((l) => (
            <tr key={l.codigo} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-mono text-xs font-medium">{l.codigo}</Td>
              <Td>{l.nombre}</Td>
              <Td className="tabular-nums">{formatoUsd(l.saldoInicial)}</Td>
              <Td className="tabular-nums">{formatoUsd(l.debitos)}</Td>
              <Td className="tabular-nums">{formatoUsd(l.creditos)}</Td>
              <Td className="tabular-nums font-medium">{formatoUsd(l.saldoFinal)}</Td>
            </tr>
          ))}
        </tbody>
        {balance && (balance.lineas.length ?? 0) > 0 && (
          <tfoot>
            <tr className="border-t border-black/[0.08] font-semibold">
              <Td colSpan={3}>Totales</Td>
              <Td className="tabular-nums">{formatoUsd(balance.totalDebitos)}</Td>
              <Td className="tabular-nums">{formatoUsd(balance.totalCreditos)}</Td>
              <Td />
            </tr>
          </tfoot>
        )}
      </TableContainer>
    </div>
  )
}

function SeccionCierrePeriodo() {
  const queryClient = useQueryClient()
  const hoy = new Date()
  const [periodo, setPeriodo] = useState(
    `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}`,
  )

  const { data: periodos, isLoading } = useQuery<PeriodoContable[]>({
    queryKey: ['contabilidad-periodos'],
    queryFn: async () => (await api.get('/api/contabilidad/periodos')).data,
  })

  const cerrar = useMutation({
    mutationFn: async () =>
      (await api.post('/api/contabilidad/periodos/cerrar', { periodo: `${periodo}-01` })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contabilidad-periodos'] })
    },
  })

  return (
    <div>
      <div className="glass-card mb-4 rounded-xl p-4">
        <form
          className="flex flex-wrap items-end gap-3"
          onSubmit={(e) => {
            e.preventDefault()
            cerrar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Período a cerrar</span>
            <input
              type="month"
              value={periodo}
              onChange={(e) => setPeriodo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <button
            type="submit"
            disabled={cerrar.isPending}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            <Lock size={16} />
            {cerrar.isPending ? 'Cerrando…' : 'Cerrar período'}
          </button>
        </form>
        {cerrar.isError && (
          <p className="mt-2 text-sm text-red-700">{mensajeError(cerrar.error, 'No se pudo cerrar el período.')}</p>
        )}
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Período</Th>
            <Th>Estado</Th>
            <Th>Fecha de cierre</Th>
            <Th>Cerrado por</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (periodos?.length ?? 0) === 0 && <EmptyState>Todavía no se ha cerrado ningún período</EmptyState>}
          {periodos?.map((p) => (
            <tr key={p.periodo} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{p.periodo.slice(0, 7)}</Td>
              <Td>
                <Badge variant={p.cerrado ? 'peligro' : 'exito'}>{p.cerrado ? 'Cerrado' : 'Abierto'}</Badge>
              </Td>
              <Td>{p.fechaCierre ? new Date(p.fechaCierre).toLocaleString('es-EC') : '—'}</Td>
              <Td>{p.cerradoPor ?? '—'}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

function SeccionPlanCuentas() {
  const [q, setQ] = useState('')

  const { data, isLoading } = useQuery<CuentaContable[]>({
    queryKey: ['cuentas-contables', q],
    queryFn: async () => (await api.get('/api/contabilidad/cuentas', { params: { q: q || undefined } })).data,
  })

  return (
    <div>
      <div className="mb-4">
        <SearchBar value={q} onChange={setQ} placeholder="Buscar por código o nombre…" />
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Cuenta</Th>
            <Th>Grupo</Th>
            <Th>Naturaleza</Th>
            <Th>Tipo</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>No se encontraron cuentas</EmptyState>}
          {data?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-mono text-xs font-medium">{c.codigo}</Td>
              <Td style={{ paddingLeft: `${c.codigo.length > 1 ? (c.codigo.length - 1) * 12 + 16 : 16}px` }}>
                {c.nombre}
              </Td>
              <Td>{c.grupo}</Td>
              <Td>
                <Badge variant={c.naturaleza === 'Deudora' ? 'alerta' : 'exito'}>{c.naturaleza}</Badge>
              </Td>
              <Td>
                <Badge>{c.esMayor ? 'Detalle' : 'Agrupación'}</Badge>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface CierreEjercicio {
  anio: number
  fechaCierre: string
  totalIngresos: number
  totalGastos: number
  utilidad: number
  cerradoPor: string
}

function SeccionCierreEjercicio() {
  const queryClient = useQueryClient()
  const [anio, setAnio] = useState(String(new Date().getFullYear()))

  const { data: cierres, isLoading } = useQuery<CierreEjercicio[]>({
    queryKey: ['contabilidad-cierre-ejercicio'],
    queryFn: async () => (await api.get('/api/contabilidad/cierre-ejercicio')).data,
  })

  const cerrar = useMutation({
    mutationFn: async () =>
      (await api.post('/api/contabilidad/cierre-ejercicio/cerrar', { anio: Number(anio) })).data as CierreEjercicio,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contabilidad-cierre-ejercicio'] })
    },
  })

  return (
    <div>
      <div className="glass-card mb-4 rounded-xl p-4">
        <form
          className="flex flex-wrap items-end gap-3"
          onSubmit={(e) => {
            e.preventDefault()
            cerrar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Año a cerrar</span>
            <input
              type="number"
              min="2020"
              max="2100"
              value={anio}
              onChange={(e) => setAnio(e.target.value)}
              className="w-28 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <button
            type="submit"
            disabled={cerrar.isPending}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            <Lock size={16} />
            {cerrar.isPending ? 'Cerrando…' : 'Cerrar ejercicio'}
          </button>
        </form>

        <p className="mt-3 text-xs text-graphite-600">
          Liquida el saldo acumulado de todas las cuentas de ingresos y gastos del año contra patrimonio (utilidad o
          pérdida del ejercicio). El comprobante queda fechado el 31 de diciembre del año — si ese período ya está
          cerrado (pestaña "Cierre de período"), primero hay que revisar esa fecha.
        </p>

        {cerrar.isSuccess && (
          <p className="mt-3 rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
            Ejercicio {cerrar.data.anio} cerrado: ingresos {formatoUsd(cerrar.data.totalIngresos)}, gastos{' '}
            {formatoUsd(cerrar.data.totalGastos)}, {cerrar.data.utilidad >= 0 ? 'utilidad' : 'pérdida'}{' '}
            {formatoUsd(Math.abs(cerrar.data.utilidad))}.
          </p>
        )}
        {cerrar.isError && (
          <p className="mt-3 text-sm text-red-700">{mensajeError(cerrar.error, 'No se pudo cerrar el ejercicio.')}</p>
        )}
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Año</Th>
            <Th>Ingresos</Th>
            <Th>Gastos</Th>
            <Th>Resultado</Th>
            <Th>Fecha de cierre</Th>
            <Th>Cerrado por</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (cierres?.length ?? 0) === 0 && <EmptyState>Todavía no se ha cerrado ningún ejercicio</EmptyState>}
          {cierres?.map((c) => (
            <tr key={c.anio} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{c.anio}</Td>
              <Td className="tabular-nums">{formatoUsd(c.totalIngresos)}</Td>
              <Td className="tabular-nums">{formatoUsd(c.totalGastos)}</Td>
              <Td className="tabular-nums font-medium">
                <Badge variant={c.utilidad >= 0 ? 'exito' : 'peligro'}>
                  {c.utilidad >= 0 ? 'Utilidad' : 'Pérdida'} {formatoUsd(Math.abs(c.utilidad))}
                </Badge>
              </Td>
              <Td>{new Date(c.fechaCierre).toLocaleString('es-EC')}</Td>
              <Td>{c.cerradoPor}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface EstadoFinancieroDetalleItem {
  codigoCuentaContable: string
  nombreCuentaContable: string
  saldoCuentaContable: number
}

interface EstadoFinancieroCabecera {
  codigoEstructura: string
  ruc: string
  fechaCorte: string
  numeroTotalRegistros: number
  valorCuadre: number
}

interface EstadoFinancieroResult {
  cabecera: EstadoFinancieroCabecera
  detalle: EstadoFinancieroDetalleItem[]
  advertencias: string[]
}

function SeccionEstadosFinancieros() {
  const [estructura, setEstructura] = useState<'B11' | 'B13'>('B11')
  const hoy = new Date()
  const [periodo, setPeriodo] = useState(
    `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}`,
  )

  const { data, isLoading } = useQuery<EstadoFinancieroResult>({
    queryKey: ['contabilidad-estado-financiero', estructura, periodo],
    queryFn: async () =>
      (
        await api.get(`/api/contabilidad/reportes/${estructura.toLowerCase()}`, {
          params: estructura === 'B11' ? { periodo: `${periodo}-01` } : undefined,
        })
      ).data,
  })

  return (
    <div>
      <div className="glass-card mb-4 rounded-xl p-4">
        <div className="mb-3 flex items-center gap-2">
          <FileBarChart size={16} className="text-graphite-600" />
          <p className="text-xs text-graphite-600">
            Estructura real del "Manual Técnico de Estructuras de Datos - Estados Financieros" v10.0 de SEPS —
            cabecera, detalle y controles de validación tal como los define el manual oficial.
          </p>
        </div>
        <div className="flex flex-wrap items-end gap-3">
          <div className="flex gap-1.5">
            {(['B11', 'B13'] as const).map((e) => (
              <button
                key={e}
                type="button"
                onClick={() => setEstructura(e)}
                className={`rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
                  estructura === e
                    ? 'border-gold-500/50 bg-gold-500/10 text-gold-300'
                    : 'border-black/[0.08] text-graphite-600 hover:bg-black/[0.02]'
                }`}
              >
                {e} — {e === 'B11' ? 'Mensual' : 'Diario'}
              </button>
            ))}
          </div>
          {estructura === 'B11' && (
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Mes de corte</span>
              <input
                type="month"
                value={periodo}
                onChange={(e) => setPeriodo(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          )}
          {estructura === 'B13' && (
            <p className="text-xs text-graphite-600">
              B13 solo puede generarse a la fecha de hoy — el modelo de saldos de este core es mensual, no diario, así
              que no existe una foto exacta de un día pasado arbitrario.
            </p>
          )}
        </div>
      </div>

      {data && (
        <div className="glass-card mb-4 rounded-xl p-4">
          <h3 className="mb-2 text-sm font-medium text-graphite-100">Cabecera</h3>
          <div className="grid grid-cols-2 gap-3 text-sm sm:grid-cols-5">
            <div>
              <p className="text-xs text-graphite-600">Código</p>
              <p className="font-medium text-graphite-100">{data.cabecera.codigoEstructura}</p>
            </div>
            <div>
              <p className="text-xs text-graphite-600">RUC</p>
              <p className="font-medium text-graphite-100">{data.cabecera.ruc || '—'}</p>
            </div>
            <div>
              <p className="text-xs text-graphite-600">Fecha de corte</p>
              <p className="font-medium text-graphite-100">{data.cabecera.fechaCorte}</p>
            </div>
            <div>
              <p className="text-xs text-graphite-600">N.º de registros</p>
              <p className="font-medium text-graphite-100">{data.cabecera.numeroTotalRegistros}</p>
            </div>
            <div>
              <p className="text-xs text-graphite-600">Valor de cuadre</p>
              <p className="font-medium text-graphite-100">{formatoUsd(data.cabecera.valorCuadre)}</p>
            </div>
          </div>
        </div>
      )}

      {data && data.advertencias.length > 0 && (
        <div className="glass-card mb-4 rounded-xl border border-gold-500/30 p-4">
          <div className="mb-2 flex items-center gap-1.5 text-sm font-medium text-gold-300">
            <AlertTriangle size={15} /> Advertencias de validación
          </div>
          <ul className="list-inside list-disc space-y-1 text-xs text-graphite-600">
            {data.advertencias.map((a, i) => (
              <li key={i}>{a}</li>
            ))}
          </ul>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código de cuenta contable</Th>
            <Th>Nombre de la cuenta contable</Th>
            <Th>Saldo de la cuenta contable</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.detalle.length ?? 0) === 0 && <EmptyState>Sin cuentas con saldo en esta fecha de corte</EmptyState>}
          {data?.detalle.map((d) => (
            <tr key={d.codigoCuentaContable} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-mono text-xs font-medium">{d.codigoCuentaContable}</Td>
              <Td>{d.nombreCuentaContable}</Td>
              <Td className="tabular-nums">{formatoUsd(d.saldoCuentaContable)}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

const TABS = [
  { id: 'plan-cuentas', label: 'Plan de cuentas' },
  { id: 'balance', label: 'Balance de comprobación' },
  { id: 'mayor-auxiliar', label: 'Mayor auxiliar' },
  { id: 'estados-financieros', label: 'B11 / B13 SEPS' },
  { id: 'cierre', label: 'Cierre de período' },
  { id: 'cierre-ejercicio', label: 'Cierre de resultados' },
] as const
type TabId = (typeof TABS)[number]['id']

export function Contabilidad() {
  const [tab, setTab] = useState<TabId>('plan-cuentas')

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Calculator} title="Contabilidad" subtitle="Plan de cuentas, sobre el Catálogo Único de Cuentas (CUC) de la SEPS" />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {TABS.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`rounded-t-lg px-3 py-2 text-sm font-medium transition ${
              tab === t.id
                ? 'border-b-2 border-gold-500 text-graphite-100'
                : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'plan-cuentas' && <SeccionPlanCuentas />}
      {tab === 'balance' && <SeccionBalanceComprobacion />}
      {tab === 'mayor-auxiliar' && <SeccionMayorAuxiliar />}
      {tab === 'estados-financieros' && <SeccionEstadosFinancieros />}
      {tab === 'cierre' && <SeccionCierrePeriodo />}
      {tab === 'cierre-ejercicio' && <SeccionCierreEjercicio />}
    </div>
  )
}

interface MayorAuxiliarLinea {
  fecha: string
  numeroComprobante: number
  descripcion: string
  debito: number
  credito: number
  saldoCorriente: number
}

interface MayorAuxiliarResult {
  codigoCuenta: string
  nombreCuenta: string
  desde: string
  hasta: string
  saldoInicial: number
  lineas: MayorAuxiliarLinea[]
  saldoFinal: number
  totalDebitos: number
  totalCreditos: number
}

function formatoUsdMayor(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

const COLUMNAS_MAYOR_AUXILIAR: ColumnaExportable<MayorAuxiliarLinea>[] = [
  { header: 'Fecha', accessor: (l) => l.fecha },
  { header: 'Comprobante', accessor: (l) => l.numeroComprobante },
  { header: 'Descripción', accessor: (l) => l.descripcion },
  { header: 'Débito', accessor: (l) => l.debito },
  { header: 'Crédito', accessor: (l) => l.credito },
  { header: 'Saldo', accessor: (l) => l.saldoCorriente },
]

function SeccionMayorAuxiliar() {
  const [q, setQ] = useState('')
  const [cuenta, setCuenta] = useState<CuentaContable | null>(null)
  const [desde, setDesde] = useState(() => new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10))
  const [hasta, setHasta] = useState(() => new Date().toISOString().slice(0, 10))

  const { data: resultados } = useQuery<CuentaContable[]>({
    queryKey: ['mayor-auxiliar-buscar-cuenta', q],
    queryFn: async () => (await api.get('/api/contabilidad/cuentas', { params: { q: q || undefined } })).data,
    enabled: q.length >= 2,
  })

  const { data: reporte, isLoading } = useQuery<MayorAuxiliarResult>({
    queryKey: ['mayor-auxiliar', cuenta?.id, desde, hasta],
    queryFn: async () =>
      (await api.get('/api/contabilidad/reportes/mayor-auxiliar', { params: { idCuenta: cuenta!.id, desde, hasta } })).data,
    enabled: !!cuenta,
  })

  return (
    <div>
      <p className="mb-4 text-sm text-graphite-600">
        Historial de movimientos de una cuenta contable específica en un rango de fechas, con saldo corriente — el
        detalle que complementa al Balance de Comprobación (que agrega por período completo).
      </p>

      <div className="mb-4 flex flex-wrap items-end gap-3">
        <div className="relative min-w-[280px] flex-1">
          <SearchBar value={q} onChange={setQ} placeholder="Buscar cuenta contable (código o nombre)…" />
          {q.length >= 2 && (resultados?.length ?? 0) > 0 && !cuenta && (
            <div className="absolute z-10 mt-1 max-h-56 w-full overflow-y-auto rounded-lg border border-black/[0.08] bg-white shadow-lg">
              {resultados?.filter((r) => r.esMayor).map((r) => (
                <button
                  key={r.id}
                  type="button"
                  onClick={() => {
                    setCuenta(r)
                    setQ('')
                  }}
                  className="block w-full px-3 py-2 text-left text-sm hover:bg-black/[0.03]"
                >
                  {r.codigo} — {r.nombre}
                </button>
              ))}
            </div>
          )}
        </div>
        <label className="flex flex-col gap-1 text-xs">
          <span className="text-graphite-600">Desde</span>
          <input
            type="date"
            value={desde}
            onChange={(e) => setDesde(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
        <label className="flex flex-col gap-1 text-xs">
          <span className="text-graphite-600">Hasta</span>
          <input
            type="date"
            value={hasta}
            onChange={(e) => setHasta(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>
      </div>

      {cuenta && (
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <p className="text-sm text-graphite-600">
            Cuenta seleccionada: <strong>{cuenta.codigo} — {cuenta.nombre}</strong>{' '}
            <button type="button" onClick={() => setCuenta(null)} className="ml-2 text-xs text-gold-400 hover:underline">
              Cambiar
            </button>
          </p>
          {reporte && (
            <BotonesExportar
              nombreArchivo={`mayor_auxiliar_${cuenta.codigo}`}
              titulo={`Mayor auxiliar — ${cuenta.codigo} ${cuenta.nombre}`}
              subtitulo={`Del ${desde} al ${hasta} — Saldo inicial ${formatoUsdMayor(reporte.saldoInicial)}`}
              columnas={COLUMNAS_MAYOR_AUXILIAR}
              filas={reporte.lineas}
            />
          )}
        </div>
      )}

      {!cuenta && <EmptyState>Buscá y seleccioná una cuenta contable de detalle para ver su mayor auxiliar</EmptyState>}

      {cuenta && isLoading && <EmptyState>Cargando…</EmptyState>}

      {cuenta && reporte && (
        <>
          <div className="mb-4 grid grid-cols-2 gap-3 sm:grid-cols-4">
            <div className="glass-card rounded-lg px-3 py-2">
              <p className="text-xs text-graphite-600">Saldo inicial</p>
              <p className="text-sm font-semibold text-graphite-100">{formatoUsdMayor(reporte.saldoInicial)}</p>
            </div>
            <div className="glass-card rounded-lg px-3 py-2">
              <p className="text-xs text-graphite-600">Total débitos</p>
              <p className="text-sm font-semibold text-graphite-100">{formatoUsdMayor(reporte.totalDebitos)}</p>
            </div>
            <div className="glass-card rounded-lg px-3 py-2">
              <p className="text-xs text-graphite-600">Total créditos</p>
              <p className="text-sm font-semibold text-graphite-100">{formatoUsdMayor(reporte.totalCreditos)}</p>
            </div>
            <div className="glass-card rounded-lg px-3 py-2">
              <p className="text-xs text-graphite-600">Saldo final</p>
              <p className="text-sm font-semibold text-graphite-100">{formatoUsdMayor(reporte.saldoFinal)}</p>
            </div>
          </div>

          <TableContainer>
            <thead>
              <tr>
                <Th>Fecha</Th>
                <Th>Comprobante</Th>
                <Th>Descripción</Th>
                <Th>Débito</Th>
                <Th>Crédito</Th>
                <Th>Saldo</Th>
              </tr>
            </thead>
            <tbody>
              {reporte.lineas.length === 0 && <EmptyState>Sin movimientos en el rango</EmptyState>}
              {reporte.lineas.map((l, i) => (
                <tr key={i} className="border-b border-black/[0.04] last:border-0">
                  <Td>{l.fecha}</Td>
                  <Td className="tabular-nums">{l.numeroComprobante}</Td>
                  <Td>{l.descripcion}</Td>
                  <Td className="tabular-nums">{l.debito > 0 ? formatoUsdMayor(l.debito) : '—'}</Td>
                  <Td className="tabular-nums">{l.credito > 0 ? formatoUsdMayor(l.credito) : '—'}</Td>
                  <Td className="tabular-nums font-medium">{formatoUsdMayor(l.saldoCorriente)}</Td>
                </tr>
              ))}
            </tbody>
          </TableContainer>
        </>
      )}
    </div>
  )
}
