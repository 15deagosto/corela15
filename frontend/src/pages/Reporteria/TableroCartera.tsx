import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, PieChart, Pie, Cell, Legend } from 'recharts'
import { Loader2, Landmark } from 'lucide-react'
import { ejecutarConsulta, formatearValor, formatCompact, type QueryResult } from '../../lib/reporteria'
import { CortePicker } from '../../components/CortePicker'
import { Tarjeta, Panel, CargandoTablero, ErrorTablero, ESTILO_TOOLTIP, n } from './_shared'

const COLORES_CALIF: Record<string, string> = {
  'A-1': '#c9a665', 'A-2': '#b58e4a', 'A-3': '#82642c',
  'B-1': '#5a8ba8', 'B-2': '#3d6b86',
  'C-1': '#f59e0b', 'C-2': '#f97316',
  D: '#f43f5e', E: '#be123c',
}

/** Tablero predefinido de Cartera — cifras validadas contra el reporte oficial ÍNDICE DE MOROSIDAD del core (ver siga/CLAUDE.md). Portado tal cual de siga-web. */
export function TableroCartera() {
  const [total, setTotal] = useState<QueryResult | null>(null)
  const [porAgencia, setPorAgencia] = useState<QueryResult | null>(null)
  const [porCalificacion, setPorCalificacion] = useState<QueryResult | null>(null)
  const [porAsesor, setPorAsesor] = useState<QueryResult | null>(null)
  const [porBanda, setPorBanda] = useState<QueryResult | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [corte, setCorte] = useState<string | null>(null)

  useEffect(() => {
    const base = { dataset: 'cartera', filters: [], snapshot: corte, limit: 1000 }
    setCargando(true)
    Promise.all([
      ejecutarConsulta({ ...base, dimensions: [], measures: ['operaciones', 'socios', 'saldo', 'vigente', 'vencido', 'ndi', 'moraSeps', 'provReq'] }),
      ejecutarConsulta({ ...base, dimensions: ['agencia'], measures: ['operaciones', 'saldo', 'improductiva', 'moraSeps'] }),
      ejecutarConsulta({ ...base, dimensions: ['calificacion'], measures: ['operaciones', 'saldo', 'provReq'], orderBy: 'calificacion', orderDesc: false }),
      ejecutarConsulta({ ...base, dimensions: ['agencia', 'asesor'], measures: ['operaciones', 'saldo', 'moraSeps'], orderBy: 'saldo' }),
      ejecutarConsulta({ ...base, dimensions: ['bandaMora'], measures: ['operaciones', 'saldo'], orderBy: 'bandaMora', orderDesc: false }),
    ])
      .then(([t, a, c, s, b]) => {
        setTotal(t); setPorAgencia(a); setPorCalificacion(c); setPorAsesor(s); setPorBanda(b)
      })
      .catch((e) => setError(e instanceof Error ? e.message : 'Error'))
      .finally(() => setCargando(false))
  }, [corte])

  if (cargando && !total) return <CargandoTablero />
  if (error) return <ErrorTablero mensaje={error} />

  const t = total?.rows[0]
  const saldo = n(t, 'saldo')

  return (
    <div className="flex flex-col gap-5">
      <header className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <Landmark size={18} className="text-gold-500" />
          <div>
            <h1 className="text-lg font-bold tracking-tight text-graphite-100">Cartera de Crédito</h1>
            <p className="mt-0.5 text-xs text-graphite-600">Cifras validadas contra el reporte oficial del core — ÍNDICE DE MOROSIDAD</p>
          </div>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          {cargando && <Loader2 className="animate-spin text-graphite-500" size={16} />}
          <CortePicker corte={corte} onChange={setCorte} />
        </div>
      </header>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Tarjeta label="Cartera bruta" valor={formatearValor(saldo, 'money')} />
        <Tarjeta label="Operaciones" valor={formatearValor(n(t, 'operaciones'), 'int')} sub={`${formatearValor(n(t, 'socios'), 'int')} socios`} />
        <Tarjeta label="Morosidad SEPS" valor={formatearValor(n(t, 'moraSeps'), 'pct')} alerta={n(t, 'moraSeps') > 10} sub={`${formatearValor(n(t, 'improductiva'), 'money')} improductiva`} />
        <Tarjeta label="Provisión requerida" valor={formatearValor(n(t, 'provReq'), 'money')} sub={`${formatearValor(saldo > 0 ? (n(t, 'provReq') / saldo) * 100 : 0, 'pct')} de la cartera`} />
      </div>

      <Panel titulo="Composición de la cartera">
        <div className="mb-3 flex h-9 gap-1 overflow-hidden rounded-lg">
          <Barra valor={n(t, 'vigente')} total={saldo} color="bg-gold-500" etiqueta="Vigente" />
          <Barra valor={n(t, 'vencido')} total={saldo} color="bg-amber-500" etiqueta="Vencido" />
          <Barra valor={n(t, 'ndi')} total={saldo} color="bg-rose-500" etiqueta="No devenga" />
        </div>
        <div className="grid grid-cols-3 gap-4 text-sm">
          <Detalle etiqueta="Vigente" valor={n(t, 'vigente')} total={saldo} color="text-gold-500" />
          <Detalle etiqueta="Vencido" valor={n(t, 'vencido')} total={saldo} color="text-amber-600" />
          <Detalle etiqueta="No devenga (NDI)" valor={n(t, 'ndi')} total={saldo} color="text-rose-600" />
        </div>
        <p className="mt-3 text-xs text-graphite-600">La morosidad SEPS suma vencido y no devenga. Mostrar solo el vencido subreporta significativamente la mora real.</p>
      </Panel>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Cartera y morosidad por agencia">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porAgencia?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="agencia" stroke="#68707b" fontSize={11} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="saldo" name="Cartera" fill="#c9a665" radius={[3, 3, 0, 0]} />
                <Bar dataKey="improductiva" name="Improductiva" fill="#f43f5e" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
          <table className="mt-3 w-full text-xs">
            <thead className="border-b border-black/[0.06] text-graphite-600">
              <tr>
                <th className="py-1.5 text-left">Agencia</th>
                <th className="py-1.5 text-right">Ops</th>
                <th className="py-1.5 text-right">Cartera</th>
                <th className="py-1.5 text-right">Mora SEPS</th>
              </tr>
            </thead>
            <tbody>
              {porAgencia?.rows.map((r, i) => (
                <tr key={i} className="border-b border-black/[0.04]">
                  <td className="py-1.5 text-graphite-100">{String(r.agencia)}</td>
                  <td className="text-right tabular-nums text-graphite-600">{formatearValor(r.operaciones, 'int')}</td>
                  <td className="text-right tabular-nums text-graphite-600">{formatearValor(r.saldo, 'money')}</td>
                  <td className={`text-right tabular-nums ${Number(r.moraSeps) > 15 ? 'text-rose-600' : 'text-graphite-600'}`}>{formatearValor(r.moraSeps, 'pct')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </Panel>

        <Panel titulo="Distribución por calificación de riesgo">
          <div className="h-64">
            <ResponsiveContainer>
              <PieChart>
                <Pie data={porCalificacion?.rows ?? []} dataKey="saldo" nameKey="calificacion" cx="50%" cy="50%" innerRadius={55} outerRadius={95} paddingAngle={2}>
                  {porCalificacion?.rows.map((r, i) => (
                    <Cell key={i} fill={COLORES_CALIF[String(r.calificacion)] ?? '#525a66'} />
                  ))}
                </Pie>
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <Panel titulo="Cartera por banda de mora">
        <div className="h-56">
          <ResponsiveContainer>
            <BarChart data={porBanda?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
              <CartesianGrid stroke="#ddd3bd" vertical={false} />
              <XAxis dataKey="bandaMora" stroke="#68707b" fontSize={11} />
              <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
              <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
              <Bar dataKey="saldo" fill="#3d6b86" radius={[3, 3, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </Panel>

      <Panel titulo="Cartera por asesor">
        <p className="mb-3 text-xs text-amber-600/90">
          Las carteras con morosidad superior al 60&nbsp;% suelen corresponder a gestión de recuperación, no a originación. Verificar con Negocios antes de usar este panel para evaluar productividad.
        </p>
        <div className="max-h-96 overflow-auto">
          <table className="w-full text-xs">
            <thead className="sticky top-0 border-b border-black/[0.06] bg-white text-graphite-600">
              <tr>
                <th className="py-1.5 text-left">Agencia</th>
                <th className="py-1.5 text-left">Asesor</th>
                <th className="py-1.5 text-right">Ops</th>
                <th className="py-1.5 text-right">Cartera</th>
                <th className="py-1.5 text-right">Mora SEPS</th>
              </tr>
            </thead>
            <tbody>
              {porAsesor?.rows.map((r, i) => {
                const mora = Number(r.moraSeps ?? 0)
                return (
                  <tr key={i} className="border-b border-black/[0.04] hover:bg-black/[0.02]">
                    <td className="py-1.5 text-graphite-600">{String(r.agencia)}</td>
                    <td className="py-1.5 text-graphite-100">{String(r.asesor)}</td>
                    <td className="text-right tabular-nums text-graphite-600">{formatearValor(r.operaciones, 'int')}</td>
                    <td className="text-right tabular-nums text-graphite-600">{formatearValor(r.saldo, 'money')}</td>
                    <td className={`text-right tabular-nums ${mora > 60 ? 'text-rose-600' : mora > 15 ? 'text-amber-600' : 'text-graphite-600'}`}>{formatearValor(mora, 'pct')}</td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      </Panel>
    </div>
  )
}

function Barra({ valor, total, color, etiqueta }: { valor: number; total: number; color: string; etiqueta: string }) {
  const pct = total > 0 ? (valor / total) * 100 : 0
  if (pct < 0.5) return null
  return (
    <div className={`${color} flex items-center justify-center`} style={{ width: `${pct}%` }} title={etiqueta}>
      {pct > 8 && <span className="text-[10px] font-medium text-graphite-950">{pct.toFixed(1)} %</span>}
    </div>
  )
}

function Detalle({ etiqueta, valor, total, color }: { etiqueta: string; valor: number; total: number; color: string }) {
  return (
    <div>
      <div className="text-xs text-graphite-600">{etiqueta}</div>
      <div className={`tabular-nums font-medium ${color}`}>{formatearValor(valor, 'money')}</div>
      <div className="text-xs tabular-nums text-graphite-600">{formatearValor(total > 0 ? (valor / total) * 100 : 0, 'pct')}</div>
    </div>
  )
}
