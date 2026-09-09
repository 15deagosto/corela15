import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, PieChart, Pie, Cell, Legend } from 'recharts'
import { ClipboardList } from 'lucide-react'
import { ejecutarConsulta, formatearValor, formatCompact, type QueryResult, type FilterValue } from '../../lib/reporteria'
import { PeriodPicker } from '../../components/PeriodPicker'
import { calcularPeriodo, type Periodo } from '../../lib/period'
import { Tarjeta, Panel, CargandoTablero, ErrorTablero, COLORES, ESTILO_TOOLTIP, n } from './_shared'

/** Tablero predefinido de Solicitudes de Crédito (embudo de originación previo al desembolso). Portado de siga-web. */
export function TableroSolicitudes() {
  const [resumen, setResumen] = useState<QueryResult | null>(null)
  const [porEstado, setPorEstado] = useState<QueryResult | null>(null)
  const [porAgencia, setPorAgencia] = useState<QueryResult | null>(null)
  const [porTipo, setPorTipo] = useState<QueryResult | null>(null)
  const [porMes, setPorMes] = useState<QueryResult | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [periodo, setPeriodo] = useState<Periodo>(() => calcularPeriodo('todo'))

  useEffect(() => {
    const filtrosFecha: FilterValue[] = []
    if (periodo.desde) filtrosFecha.push({ id: 'fechaDesde', values: [periodo.desde] })
    if (periodo.hasta) filtrosFecha.push({ id: 'fechaHasta', values: [periodo.hasta] })

    setCargando(true)
    Promise.all([
      ejecutarConsulta({
        dataset: 'solicitudes', dimensions: [],
        measures: ['solicitudes', 'montoSolicitado', 'montoAprobado', 'pctAnuladas', 'pctNegadas', 'pctLiquidadas', 'ticketSolicitado'],
        filters: filtrosFecha, limit: 10,
      }),
      ejecutarConsulta({ dataset: 'solicitudes', dimensions: ['estadoSolicitud'], measures: ['solicitudes'], filters: filtrosFecha, orderBy: 'solicitudes', orderDesc: true, limit: 20 }),
      ejecutarConsulta({ dataset: 'solicitudes', dimensions: ['agencia'], measures: ['solicitudes', 'montoSolicitado'], filters: filtrosFecha, limit: 20 }),
      ejecutarConsulta({ dataset: 'solicitudes', dimensions: ['tipo'], measures: ['solicitudes', 'montoSolicitado'], filters: filtrosFecha, orderBy: 'montoSolicitado', orderDesc: true, limit: 20 }),
      ejecutarConsulta({ dataset: 'solicitudes', dimensions: ['mes'], measures: ['solicitudes', 'montoSolicitado'], filters: filtrosFecha, orderBy: 'mes', orderDesc: false, limit: 36 }),
    ])
      .then(([r, pe, pa, pt, pm]) => {
        setResumen(r); setPorEstado(pe); setPorAgencia(pa); setPorTipo(pt); setPorMes(pm)
      })
      .catch((e) => setError(e instanceof Error ? e.message : 'Error'))
      .finally(() => setCargando(false))
  }, [periodo.desde, periodo.hasta])

  if (cargando && !resumen) return <CargandoTablero />
  if (error) return <ErrorTablero mensaje={error} />

  const r = resumen?.rows[0]

  return (
    <div className="flex flex-col gap-5">
      <header className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <ClipboardList size={18} className="text-gold-500" />
          <div>
            <h1 className="text-lg font-bold tracking-tight text-graphite-100">Solicitudes de Crédito</h1>
            <p className="mt-0.5 text-xs text-graphite-600">Trámite previo al desembolso: digitación, comité, aprobación, negación, anulación</p>
          </div>
        </div>
        <PeriodPicker periodo={periodo} onChange={setPeriodo} />
      </header>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Tarjeta label="Solicitudes" valor={formatearValor(n(r, 'solicitudes'), 'int')} sub={`Ticket prom. ${formatearValor(n(r, 'ticketSolicitado'), 'money')}`} />
        <Tarjeta label="Monto solicitado" valor={formatearValor(n(r, 'montoSolicitado'), 'money')} />
        <Tarjeta label="Monto aprobado" valor={formatearValor(n(r, 'montoAprobado'), 'money')} />
        <Tarjeta
          label="% Liquidadas"
          valor={formatearValor(n(r, 'pctLiquidadas'), 'pct')}
          sub={`Negadas ${formatearValor(n(r, 'pctNegadas'), 'pct')} · Anuladas ${formatearValor(n(r, 'pctAnuladas'), 'pct')}`}
        />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Solicitudes por estado">
          <div className="h-64">
            <ResponsiveContainer>
              <PieChart>
                <Pie data={porEstado?.rows ?? []} dataKey="solicitudes" nameKey="estadoSolicitud" cx="50%" cy="50%" innerRadius={55} outerRadius={95} paddingAngle={2}>
                  {porEstado?.rows.map((_, i) => (
                    <Cell key={i} fill={COLORES[i % COLORES.length]} />
                  ))}
                </Pie>
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'int')} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Monto solicitado por agencia">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porAgencia?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="agencia" stroke="#68707b" fontSize={10} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="montoSolicitado" name="Monto solicitado" fill="#c9a665" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Monto solicitado por tipo de crédito">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porTipo?.rows ?? []} layout="vertical" margin={{ top: 8, right: 16, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" horizontal={false} />
                <XAxis type="number" stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <YAxis type="category" dataKey="tipo" stroke="#68707b" fontSize={10} width={140} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="montoSolicitado" name="Monto solicitado" fill="#3d6b86" radius={[0, 3, 3, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Evolución mensual de solicitudes">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porMes?.rows ?? []} margin={{ top: 8, right: 8, bottom: 40, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="mes" stroke="#68707b" fontSize={10} angle={-40} textAnchor="end" interval={0} height={55} />
                <YAxis stroke="#68707b" fontSize={11} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'int')} />
                <Bar dataKey="solicitudes" name="Solicitudes" fill="#a17d3a" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>
    </div>
  )
}
