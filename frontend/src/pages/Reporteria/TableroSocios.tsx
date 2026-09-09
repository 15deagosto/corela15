import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, PieChart, Pie, Cell, Legend } from 'recharts'
import { Users, Info } from 'lucide-react'
import { ejecutarConsulta, formatearValor, formatCompact, type QueryResult, type FilterValue } from '../../lib/reporteria'
import { PeriodPicker } from '../../components/PeriodPicker'
import { calcularPeriodo, type Periodo } from '../../lib/period'
import { Tarjeta, Panel, CargandoTablero, ErrorTablero, COLORES, ESTILO_TOOLTIP, n } from './_shared'

/** Tablero predefinido de Socios: vista agregada de la base de clientes, nunca datos nominales. Portado de siga-web. */
export function TableroSocios() {
  const [resumen, setResumen] = useState<QueryResult | null>(null)
  const [porAgencia, setPorAgencia] = useState<QueryResult | null>(null)
  const [porSector, setPorSector] = useState<QueryResult | null>(null)
  const [porAntiguedad, setPorAntiguedad] = useState<QueryResult | null>(null)
  const [porAnio, setPorAnio] = useState<QueryResult | null>(null)
  const [porAsesor, setPorAsesor] = useState<QueryResult | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [periodo, setPeriodo] = useState<Periodo>(() => calcularPeriodo('todo'))

  useEffect(() => {
    const filtrosFecha: FilterValue[] = []
    if (periodo.desde) filtrosFecha.push({ id: 'fechaDesde', values: [periodo.desde] })
    if (periodo.hasta) filtrosFecha.push({ id: 'fechaHasta', values: [periodo.hasta] })

    setCargando(true)
    Promise.all([
      ejecutarConsulta({ dataset: 'socios', dimensions: [], measures: ['socios', 'antiguedadPromAnios'], filters: filtrosFecha, limit: 10 }),
      ejecutarConsulta({ dataset: 'socios', dimensions: ['agencia'], measures: ['socios'], filters: filtrosFecha, limit: 20 }),
      ejecutarConsulta({ dataset: 'socios', dimensions: ['sectorEconomico'], measures: ['socios'], filters: filtrosFecha, orderBy: 'socios', orderDesc: true, limit: 20 }),
      ejecutarConsulta({ dataset: 'socios', dimensions: ['antiguedadBanda'], measures: ['socios'], filters: filtrosFecha, limit: 20 }),
      ejecutarConsulta({ dataset: 'socios', dimensions: ['anioIngreso'], measures: ['socios'], filters: filtrosFecha, orderBy: 'anioIngreso', orderDesc: false, limit: 40 }),
      ejecutarConsulta({ dataset: 'socios', dimensions: ['asesor'], measures: ['socios'], filters: filtrosFecha, orderBy: 'socios', orderDesc: true, limit: 15 }),
    ])
      .then(([r, pa, ps, pab, pan, pas]) => {
        setResumen(r); setPorAgencia(pa); setPorSector(ps); setPorAntiguedad(pab); setPorAnio(pan); setPorAsesor(pas)
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
          <Users size={18} className="text-gold-500" />
          <div>
            <h1 className="text-lg font-bold tracking-tight text-graphite-100">Socios</h1>
            <p className="mt-0.5 text-xs text-graphite-600">Vista agregada de la base de socios — sin datos nominales</p>
          </div>
        </div>
        <PeriodPicker periodo={periodo} onChange={setPeriodo} />
      </header>

      <div className="flex items-start gap-2.5 rounded-xl border border-petrol-400/30 bg-petrol-500/[0.06] px-4 py-3">
        <Info size={15} className="mt-0.5 shrink-0 text-petrol-700" />
        <p className="text-xs leading-relaxed text-petrol-800">
          Este tablero solo muestra totales agregados (agencia, asesor, sector económico, antigüedad). No expone identificación, nombre, dirección, teléfono ni
          correo de socios.
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Tarjeta label="Socios" valor={formatearValor(n(r, 'socios'), 'int')} />
        <Tarjeta label="Antigüedad promedio" valor={`${formatearValor(n(r, 'antiguedadPromAnios'), 'num')} años`} />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Socios por agencia">
          <div className="h-64">
            <ResponsiveContainer>
              <PieChart>
                <Pie data={porAgencia?.rows ?? []} dataKey="socios" nameKey="agencia" cx="50%" cy="50%" innerRadius={55} outerRadius={95} paddingAngle={2}>
                  {porAgencia?.rows.map((_, i) => (
                    <Cell key={i} fill={COLORES[i % COLORES.length]} />
                  ))}
                </Pie>
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'int')} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Socios por sector económico">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porSector?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="sectorEconomico" stroke="#68707b" fontSize={10} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'int')} />
                <Bar dataKey="socios" name="Socios" fill="#c9a665" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Socios por antigüedad">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porAntiguedad?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="antiguedadBanda" stroke="#68707b" fontSize={9} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'int')} />
                <Bar dataKey="socios" name="Socios" fill="#3d6b86" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Evolución de ingreso de socios por año">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porAnio?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="anioIngreso" stroke="#68707b" fontSize={10} />
                <YAxis stroke="#68707b" fontSize={11} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'int')} />
                <Bar dataKey="socios" name="Socios nuevos" fill="#a17d3a" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <Panel titulo="Socios por asesor (top 15)">
        <p className="mb-2 text-[11px] text-graphite-600">
          Un socio puede figurar con el usuario genérico "Administrador del sistema" en vez de un asesor real cuando fue creado por migración/carga masiva o no
          tiene oficial asignado — no refleja necesariamente su asesor actual.
        </p>
        <div className="h-72">
          <ResponsiveContainer>
            <BarChart data={porAsesor?.rows ?? []} layout="vertical" margin={{ top: 8, right: 16, bottom: 8, left: 8 }}>
              <CartesianGrid stroke="#ddd3bd" horizontal={false} />
              <XAxis type="number" stroke="#68707b" fontSize={11} />
              <YAxis type="category" dataKey="asesor" stroke="#68707b" fontSize={10} width={150} />
              <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'int')} />
              <Bar dataKey="socios" name="Socios" fill="#b58e4a" radius={[0, 3, 3, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </Panel>
    </div>
  )
}
