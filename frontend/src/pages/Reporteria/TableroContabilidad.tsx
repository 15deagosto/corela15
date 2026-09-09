import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Cell } from 'recharts'
import { AlertTriangle, Calculator } from 'lucide-react'
import { ejecutarConsulta, formatearValor, formatCompact, type QueryResult } from '../../lib/reporteria'
import { Tarjeta, Panel, CargandoTablero, ErrorTablero, ESTILO_TOOLTIP, n } from './_shared'

const COLORES: Record<string, string> = {
  Activo: '#c9a665', Pasivo: '#3d6b86', Patrimonio: '#82642c', Gastos: '#a94442', Ingresos: '#4a8a5c',
  'Cuentas contingentes': '#6b6357', 'Cuentas de orden': '#5a5560', Otro: '#8a8a8a',
}

/**
 * Tablero predefinido de Contabilidad — Balance de Comprobación.
 *
 * NO presenta el saldo de Activo/Pasivo/Patrimonio como cifra oficial:
 * hay un residuo real (Activo+Pasivo+Patrimonio debería sumar $0) sin
 * confirmar contra el Balance de Comprobación oficial — se muestra el
 * residuo explícito en vez de ocultarlo. Ver "Reportería Gerencial —
 * pendiente Ronda D" en CLAUDE.md. Portado de siga-web.
 */
export function TableroContabilidad() {
  const [porGrupo, setPorGrupo] = useState<QueryResult | null>(null)
  const [porAgencia, setPorAgencia] = useState<QueryResult | null>(null)
  const [mayoresCuentas, setMayoresCuentas] = useState<QueryResult | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)

  useEffect(() => {
    setCargando(true)
    Promise.all([
      ejecutarConsulta({ dataset: 'contabilidad', dimensions: ['grupoMayor'], measures: ['saldo', 'cuentas'], filters: [], orderBy: 'grupoMayor', orderDesc: false, limit: 20 }),
      ejecutarConsulta({ dataset: 'contabilidad', dimensions: ['agencia'], measures: ['saldo'], filters: [], limit: 20 }),
      ejecutarConsulta({ dataset: 'contabilidad', dimensions: ['cuenta', 'grupoMayor'], measures: ['saldo'], filters: [], orderBy: 'saldo', orderDesc: true, limit: 15 }),
    ])
      .then(([pg, pa, mc]) => {
        setPorGrupo(pg); setPorAgencia(pa); setMayoresCuentas(mc)
      })
      .catch((e) => setError(e instanceof Error ? e.message : 'Error'))
      .finally(() => setCargando(false))
  }, [])

  if (cargando && !porGrupo) return <CargandoTablero />
  if (error) return <ErrorTablero mensaje={error} />

  const grupos = porGrupo?.rows ?? []
  const saldoDe = (nombre: string) => n(grupos.find((r) => r.grupoMayor === nombre), 'saldo')
  const cuentasDe = (nombre: string) => n(grupos.find((r) => r.grupoMayor === nombre), 'cuentas')
  const activo = saldoDe('Activo')
  const pasivo = saldoDe('Pasivo')
  const patrimonio = saldoDe('Patrimonio')
  const residuo = activo + pasivo + patrimonio
  const residuoPct = activo !== 0 ? (residuo / activo) * 100 : 0
  const dentroTolerancia = Math.abs(residuoPct) <= 0.1

  return (
    <div className="flex flex-col gap-5">
      <header className="flex items-center gap-2">
        <Calculator size={18} className="text-gold-500" />
        <div>
          <h1 className="text-lg font-bold tracking-tight text-graphite-100">Contabilidad</h1>
          <p className="mt-0.5 text-xs text-graphite-600">Balance de Comprobación — corte más reciente disponible</p>
        </div>
      </header>

      <div className="flex gap-3 rounded-2xl border border-amber-500/40 bg-amber-500/[0.06] p-4">
        <AlertTriangle size={18} className="mt-0.5 shrink-0 text-amber-500" />
        <div className="space-y-1 text-xs text-graphite-700">
          <p className="font-semibold text-amber-700">En validación — no usar como cifra oficial de Activo/Pasivo/Patrimonio.</p>
          <p>
            Activo + Pasivo + Patrimonio debería sumar $0,00 y actualmente da <strong className="tabular-nums">{formatearValor(residuo, 'money')}</strong> ({residuoPct.toFixed(2)}% del
            activo) — pendiente de confirmar contra el Balance de Comprobación oficial de Contabilidad antes de publicar estas cifras como definitivas.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Tarjeta label="Activo" valor={formatearValor(activo, 'money')} sub={`${formatearValor(cuentasDe('Activo'), 'int')} cuentas`} />
        <Tarjeta label="Pasivo" valor={formatearValor(pasivo, 'money')} />
        <Tarjeta label="Patrimonio" valor={formatearValor(patrimonio, 'money')} />
        <Tarjeta
          label="Residuo (debería ser $0)"
          valor={formatearValor(residuo, 'money')}
          sub={dentroTolerancia ? 'Dentro de tolerancia (±0,1%)' : `Fuera de tolerancia — ${residuoPct.toFixed(2)}%`}
          alerta={!dentroTolerancia}
        />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Panel titulo="Saldo por grupo mayor del plan de cuentas">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={grupos} margin={{ top: 8, right: 8, bottom: 40, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="grupoMayor" stroke="#68707b" fontSize={10} angle={-20} textAnchor="end" interval={0} height={55} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="saldo" name="Saldo" radius={[3, 3, 0, 0]}>
                  {grupos.map((r, i) => (
                    <Cell key={i} fill={COLORES[String(r.grupoMayor)] ?? '#8a8a8a'} />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>

        <Panel titulo="Saldo por agencia (todos los grupos)">
          <div className="h-64">
            <ResponsiveContainer>
              <BarChart data={porAgencia?.rows ?? []} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
                <CartesianGrid stroke="#ddd3bd" vertical={false} />
                <XAxis dataKey="agencia" stroke="#68707b" fontSize={10} />
                <YAxis stroke="#68707b" fontSize={11} tickFormatter={formatCompact} />
                <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, 'money')} />
                <Bar dataKey="saldo" name="Saldo" fill="#5a8ba8" radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </Panel>
      </div>

      <Panel titulo="Mayores cuentas de detalle por saldo (valor absoluto)">
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-black/[0.06] text-left text-graphite-600">
                <th className="py-2 pr-3">Cuenta</th>
                <th className="py-2 pr-3">Grupo mayor</th>
                <th className="py-2 pr-3 text-right">Saldo</th>
              </tr>
            </thead>
            <tbody>
              {(mayoresCuentas?.rows ?? []).map((r, i) => (
                <tr key={i} className="border-b border-black/[0.04]">
                  <td className="py-1.5 pr-3 text-graphite-100">{String(r.cuenta)}</td>
                  <td className="py-1.5 pr-3 text-graphite-600">{String(r.grupoMayor)}</td>
                  <td className="py-1.5 pr-3 text-right tabular-nums text-graphite-100">{formatearValor(n(r, 'saldo'), 'money')}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Panel>

      <Panel titulo="Nota sobre el alcance de este dataset">
        <p className="text-xs text-graphite-600">
          Solo incluye cuentas de detalle (con movimiento propio), no cuentas de grupo/resumen — sumarlas de nuevo duplicaría el saldo. Fija el corte al día más
          reciente disponible; no admite comparar varias fechas a la vez todavía.
        </p>
      </Panel>
    </div>
  )
}
