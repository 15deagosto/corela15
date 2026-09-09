import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, PieChart, Pie, Cell, Legend } from 'recharts'
import { Loader2, PiggyBank } from 'lucide-react'
import { ejecutarConsulta, formatearValor, formatCompact, type QueryResult, type FilterValue } from '../../lib/reporteria'
import { PeriodPicker } from '../../components/PeriodPicker'
import { calcularPeriodo, type Periodo } from '../../lib/period'
import { Tarjeta, Panel, CargandoTablero, ErrorTablero, COLORES, ESTILO_TOOLTIP, n } from './_shared'

/** Tablero predefinido de Captaciones (Ahorros + Depósitos a Plazo Fijo). Portado de siga-web. */
export function TableroCaptaciones() {
  const [ahorrosTotal, setAhorrosTotal] = useState<QueryResult | null>(null)
  const [ahorrosPorTipo, setAhorrosPorTipo] = useState<QueryResult | null>(null)
  const [ahorrosPorAgencia, setAhorrosPorAgencia] = useState<QueryResult | null>(null)
  const [dpfTotal, setDpfTotal] = useState<QueryResult | null>(null)
  const [dpfPorPlazo, setDpfPorPlazo] = useState<QueryResult | null>(null)
  const [dpfPorTipo, setDpfPorTipo] = useState<QueryResult | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [periodo, setPeriodo] = useState<Periodo>(() => calcularPeriodo('todo'))

  useEffect(() => {
    const filtrosFecha: FilterValue[] = []
    if (periodo.desde) filtrosFecha.push({ id: 'fechaDesde', values: [periodo.desde] })
    if (periodo.hasta) filtrosFecha.push({ id: 'fechaHasta', values: [periodo.hasta] })

    setCargando(true)
    Promise.all([
      ejecutarConsulta({ dataset: 'ahorros', dimensions: [], measures: ['cuentas', 'socios', 'saldo', 'saldoProm'], filters: filtrosFecha, limit: 10 }),
      ejecutarConsulta({ dataset: 'ahorros', dimensions: ['tipoCuenta'], measures: ['cuentas', 'saldo'], filters: filtrosFecha, orderBy: 'saldo', orderDesc: true, limit: 20 }),
      ejecutarConsulta({ dataset: 'ahorros', dimensions: ['agencia'], measures: ['cuentas', 'saldo'], filters: filtrosFecha, limit: 20 }),
      ejecutarConsulta({ dataset: 'inversion', dimensions: [], measures: ['depositos', 'socios', 'monto', 'tasaProm', 'plazoProm'], filters: filtrosFecha, limit: 10 }),
      ejecutarConsulta({ dataset: 'inversion', dimensions: ['bandaPlazo'], measures: ['depositos', 'monto', 'tasaProm'], filters: filtrosFecha, orderBy: 'bandaPlazo', orderDesc: false, limit: 20 }),
      ejecutarConsulta({ dataset: 'inversion', dimensions: ['tipoDeposito'], measures: ['depositos', 'monto'], filters: filtrosFecha, limit: 20 }),
    ])
      .then(([at, atp, aa, dt, dp, dtp]) => {
        setAhorrosTotal(at); setAhorrosPorTipo(atp); setAhorrosPorAgencia(aa)
        setDpfTotal(dt); setDpfPorPlazo(dp); setDpfPorTipo(dtp)
      })
      .catch((e) => setError(e instanceof Error ? e.message : 'Error'))
      .finally(() => setCargando(false))
  }, [periodo.desde, periodo.hasta])

  if (cargando && !ahorrosTotal) return <CargandoTablero />
  if (error) return <ErrorTablero mensaje={error} />

  const at = ahorrosTotal?.rows[0]
  const dt = dpfTotal?.rows[0]
  const captacionTotal = n(at, 'saldo') + n(dt, 'monto')

  return (
    <div className="flex flex-col gap-5">
      <header className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <PiggyBank size={18} className="text-gold-500" />
          <div>
            <h1 className="text-lg font-bold tracking-tight text-graphite-100">Captaciones</h1>
            <p className="mt-0.5 text-xs text-graphite-600">Ahorros y Depósitos a Plazo Fijo (DPF)</p>
          </div>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          {cargando && <Loader2 className="animate-spin text-graphite-500" size={16} />}
          <PeriodPicker periodo={periodo} onChange={setPeriodo} />
        </div>
      </header>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Tarjeta label="Captación total" valor={formatearValor(captacionTotal, 'money')} sub="Ahorros + DPF" />
        <Tarjeta label="Saldo en ahorros" valor={formatearValor(n(at, 'saldo'), 'money')} sub={`${formatearValor(n(at, 'cuentas'), 'int')} cuentas · ${formatearValor(n(at, 'socios'), 'int')} socios`} />
        <Tarjeta label="Monto en DPF" valor={formatearValor(n(dt, 'monto'), 'money')} sub={`${formatearValor(n(dt, 'depositos'), 'int')} depósitos · ${formatearValor(n(dt, 'socios'), 'int')} socios`} />
        <Tarjeta label="Tasa DPF prom. ponderada" valor={formatearValor(n(dt, 'tasaProm'), 'pct')} sub={`Plazo prom. ${formatearValor(n(dt, 'plazoProm'), 'int')} días`} />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Ahorros por tipo de cuenta">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={ahorrosPorTipo?.rows ?? []} margin={{ top: 8, right: 8, bottom: 60, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="tipoCuenta" stroke="#68707b" fontSize={10} angle={-30} textAnchor="end" interval={0} height={70} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="saldo" name="Saldo" fill="#c9a665" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Ahorros por agencia">
          <div className="h-64">
            <ResponsiveContainer>
              <PieChart>
                <Pie data={ahorrosPorAgencia?.rows ?? []} dataKey="saldo" nameKey="agencia" cx="50%" cy="50%" innerRadius={55} outerRadius={95} paddingAngle={2}>
                  {ahorrosPorAgencia?.rows.map((_, i) => (
                    <Cell key={i} fill={COLORES[i % COLORES.length]} />
                  ))}
                </Pie>
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="DPF por banda de plazo">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={dpfPorPlazo?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="bandaPlazo" stroke="#68707b" fontSize={10} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="monto" name="Monto" fill="#3d6b86" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="DPF por tipo de depósito">
          <div className="h-64">
            <ResponsiveContainer>
              <PieChart>
                <Pie data={dpfPorTipo?.rows ?? []} dataKey="monto" nameKey="tipoDeposito" cx="50%" cy="50%" innerRadius={55} outerRadius={95} paddingAngle={2}>
                  {dpfPorTipo?.rows.map((_, i) => (
                    <Cell key={i} fill={COLORES[(i + 3) % COLORES.length]} />
                  ))}
                </Pie>
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <Panel titulo="Nota sobre el saldo de ahorros">
        <p className="text-xs text-graphite-600">
          El saldo de ahorros se arma sumando los ítems marcados como "disponible" en el core (excluye interés causado no pagado). Está pendiente de validar
          con Negocios si el saldo oficial reportado usa exactamente este criterio o distingue disponible de bloqueado/encaje.
        </p>
      </Panel>
    </div>
  )
}
