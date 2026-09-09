import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, PieChart, Pie, Cell, Legend } from 'recharts'
import { ShieldAlert, Info } from 'lucide-react'
import { ejecutarConsulta, formatearValor, formatCompact, type QueryResult, type FilterValue } from '../../lib/reporteria'
import { CortePicker } from '../../components/CortePicker'
import { Tarjeta, Panel, CargandoTablero, ErrorTablero, COLORES, ESTILO_TOOLTIP, n } from './_shared'

/** Tablero predefinido de Créditos Vinculados (partes relacionadas, Código Orgánico Monetario y Financiero). Portado de siga-web. */
export function TableroVinculados() {
  const [resumen, setResumen] = useState<QueryResult | null>(null)
  const [porAgencia, setPorAgencia] = useState<QueryResult | null>(null)
  const [porEstado, setPorEstado] = useState<QueryResult | null>(null)
  const [porBandaMora, setPorBandaMora] = useState<QueryResult | null>(null)
  const [porTipo, setPorTipo] = useState<QueryResult | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [corte, setCorte] = useState<string | null>(null)

  useEffect(() => {
    const filtros: FilterValue[] = []
    setCargando(true)
    Promise.all([
      ejecutarConsulta({ dataset: 'vinculados', dimensions: [], measures: ['operaciones', 'saldo', 'ticketProm', 'diasMoraProm'], filters: filtros, snapshot: corte, limit: 10 }),
      ejecutarConsulta({ dataset: 'vinculados', dimensions: ['agencia'], measures: ['operaciones', 'saldo'], filters: filtros, snapshot: corte, limit: 20 }),
      ejecutarConsulta({ dataset: 'vinculados', dimensions: ['estado'], measures: ['operaciones', 'saldo'], filters: filtros, snapshot: corte, orderBy: 'saldo', orderDesc: true, limit: 20 }),
      ejecutarConsulta({ dataset: 'vinculados', dimensions: ['bandaMora'], measures: ['operaciones', 'saldo'], filters: filtros, snapshot: corte, orderBy: 'bandaMora', orderDesc: false, limit: 20 }),
      ejecutarConsulta({ dataset: 'vinculados', dimensions: ['tipo'], measures: ['operaciones', 'saldo'], filters: filtros, snapshot: corte, orderBy: 'saldo', orderDesc: true, limit: 20 }),
    ])
      .then(([r, pa, pe, pbm, pt]) => {
        setResumen(r); setPorAgencia(pa); setPorEstado(pe); setPorBandaMora(pbm); setPorTipo(pt)
      })
      .catch((e) => setError(e instanceof Error ? e.message : 'Error'))
      .finally(() => setCargando(false))
  }, [corte])

  if (cargando && !resumen) return <CargandoTablero />
  if (error) return <ErrorTablero mensaje={error} />

  const r = resumen?.rows[0]

  return (
    <div className="flex flex-col gap-5">
      <header className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <ShieldAlert size={18} className="text-gold-500" />
          <div>
            <h1 className="text-lg font-bold tracking-tight text-graphite-100">Créditos Vinculados</h1>
            <p className="mt-0.5 text-xs text-graphite-600">Partes relacionadas según el Código Orgánico Monetario y Financiero</p>
          </div>
        </div>
        <CortePicker corte={corte} onChange={setCorte} />
      </header>

      <div className="flex items-start gap-2.5 rounded-xl border border-amber-500/30 bg-amber-500/[0.06] px-4 py-3">
        <Info size={15} className="mt-0.5 shrink-0 text-amber-700" />
        <p className="text-xs leading-relaxed text-amber-800">
          Este tablero solo muestra el conteo y saldo marcados con el indicador de vinculación del core — si las tablas de detalle de causal no tienen
          registros en el ambiente, no hay desglose por causal regulatoria disponible.
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Tarjeta label="Operaciones vinculadas" valor={formatearValor(n(r, 'operaciones'), 'int')} />
        <Tarjeta label="Saldo vinculado" valor={formatearValor(n(r, 'saldo'), 'money')} />
        <Tarjeta label="Ticket promedio" valor={formatearValor(n(r, 'ticketProm'), 'money')} />
        <Tarjeta label="Días de mora (prom. pond.)" valor={formatearValor(n(r, 'diasMoraProm'), 'num')} />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Saldo vinculado por agencia">
          <div className="h-64">
            <ResponsiveContainer>
              <PieChart>
                <Pie data={porAgencia?.rows ?? []} dataKey="saldo" nameKey="agencia" cx="50%" cy="50%" innerRadius={55} outerRadius={95} paddingAngle={2}>
                  {porAgencia?.rows.map((_, i) => (
                    <Cell key={i} fill={COLORES[i % COLORES.length]} />
                  ))}
                </Pie>
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Saldo vinculado por estado del crédito">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porEstado?.rows ?? []} margin={{ top: 8, right: 8, bottom: 40, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="estado" stroke="#68707b" fontSize={10} angle={-30} textAnchor="end" interval={0} height={60} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="saldo" name="Saldo" fill="#c9a665" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Saldo vinculado por banda de mora">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porBandaMora?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="bandaMora" stroke="#68707b" fontSize={9} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="saldo" name="Saldo" fill="#3d6b86" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Saldo vinculado por tipo de crédito">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porTipo?.rows ?? []} layout="vertical" margin={{ top: 8, right: 16, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" horizontal={false} />
                <XAxis type="number" stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <YAxis type="category" dataKey="tipo" stroke="#68707b" fontSize={10} width={140} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="saldo" name="Saldo" fill="#a17d3a" radius={[0, 3, 3, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>
    </div>
  )
}
