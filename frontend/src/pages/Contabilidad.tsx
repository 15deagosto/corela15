import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Calculator, Lock } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
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

const TABS = [
  { id: 'plan-cuentas', label: 'Plan de cuentas' },
  { id: 'balance', label: 'Balance de comprobación' },
  { id: 'cierre', label: 'Cierre de período' },
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
      {tab === 'cierre' && <SeccionCierrePeriodo />}
    </div>
  )
}
