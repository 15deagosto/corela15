import { useEffect, useState } from 'react'
import { BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts'
import { Loader2, AlertCircle } from 'lucide-react'
import { ejecutarConsulta, formatearValor, formatCompact, type QueryResult, type Widget } from '../../lib/reporteria'
import { ESTILO_TOOLTIP, COLORES } from './_shared'

interface Props {
  widget: Widget
}

/** Renderiza un widget guardado (kpi/bar/line/pie/tabla) ejecutando su propia consulta. Portado de WidgetCard.tsx de siga-web. */
export function WidgetCard({ widget }: Props) {
  const [result, setResult] = useState<QueryResult | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false
    setLoading(true)
    setError(null)
    ejecutarConsulta({
      dataset: widget.dataset,
      dimensions: widget.dimensions,
      measures: widget.measures,
      filters: widget.filters,
      orderBy: widget.orderBy ?? undefined,
      orderDesc: widget.orderDesc,
      limit: widget.limit ?? 1000,
    })
      .then((r) => {
        if (!cancelado) setResult(r)
      })
      .catch((e) => !cancelado && setError(e instanceof Error ? e.message : 'Error'))
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [widget.dataset, JSON.stringify(widget.dimensions), JSON.stringify(widget.measures), JSON.stringify(widget.filters), widget.orderBy, widget.orderDesc, widget.limit])

  const dim = widget.dimensions[0]
  const measFmt = (id: string) => result?.columns.find((c) => c.id === id)?.format ?? 'num'

  return (
    <div className="glass-card flex h-full flex-col overflow-hidden rounded-2xl">
      <div className="shrink-0 border-b border-black/[0.06] px-4 py-3">
        <h3 className="truncate text-xs font-bold text-graphite-100">{widget.title}</h3>
      </div>
      <div className="min-h-0 flex-1 overflow-auto p-3">
        {loading && (
          <div className="flex h-full items-center justify-center gap-2 text-xs text-graphite-500">
            <Loader2 className="animate-spin" size={14} /> Cargando…
          </div>
        )}
        {error && (
          <div className="flex h-full items-center justify-center gap-1.5 px-2 text-center text-xs text-red-700">
            <AlertCircle size={14} /> {error}
          </div>
        )}
        {!loading && !error && result && <Render widget={widget} result={result} dim={dim} measFmt={measFmt} />}
      </div>
    </div>
  )
}

function Render({ widget, result, dim, measFmt }: { widget: Widget; result: QueryResult; dim?: string; measFmt: (id: string) => string }) {
  const rows = result.rows
  const meas = widget.measures

  if (widget.kind === 'kpi') {
    const primero = rows[0] ?? {}
    return (
      <div className="flex h-full flex-col justify-center gap-3">
        {meas.map((m) => (
          <div key={m}>
            <div className="text-[10px] uppercase tracking-wider text-graphite-600">{result.columns.find((c) => c.id === m)?.label ?? m}</div>
            <div className="mt-0.5 text-2xl font-extrabold tabular-nums text-graphite-100">{formatearValor(primero[m], measFmt(m))}</div>
          </div>
        ))}
      </div>
    )
  }

  if (rows.length === 0) {
    return <div className="flex h-full items-center justify-center text-xs text-graphite-600">Sin datos</div>
  }

  if (widget.kind === 'bar' && dim) {
    return (
      <ResponsiveContainer>
        <BarChart data={rows} margin={{ top: 4, right: 8, bottom: 8, left: 8 }}>
          <CartesianGrid stroke="#ddd3bd" vertical={false} />
          <XAxis dataKey={dim} stroke="#68707b" fontSize={10} />
          <YAxis stroke="#68707b" fontSize={10} tickFormatter={formatCompact} />
          <Tooltip {...ESTILO_TOOLTIP} formatter={(v, name) => [formatearValor(v, measFmt(String(name))), result.columns.find((c) => c.id === name)?.label ?? String(name)]} />
          {meas.map((m, i) => (
            <Bar key={m} dataKey={m} fill={COLORES[i % COLORES.length]} radius={[3, 3, 0, 0]} />
          ))}
        </BarChart>
      </ResponsiveContainer>
    )
  }

  if (widget.kind === 'line' && dim) {
    return (
      <ResponsiveContainer>
        <LineChart data={rows} margin={{ top: 4, right: 8, bottom: 8, left: 8 }}>
          <CartesianGrid stroke="#ddd3bd" vertical={false} />
          <XAxis dataKey={dim} stroke="#68707b" fontSize={10} />
          <YAxis stroke="#68707b" fontSize={10} tickFormatter={formatCompact} />
          <Tooltip {...ESTILO_TOOLTIP} formatter={(v, name) => [formatearValor(v, measFmt(String(name))), result.columns.find((c) => c.id === name)?.label ?? String(name)]} />
          {meas.map((m, i) => (
            <Line key={m} type="monotone" dataKey={m} stroke={COLORES[i % COLORES.length]} strokeWidth={2} dot={false} />
          ))}
        </LineChart>
      </ResponsiveContainer>
    )
  }

  if (widget.kind === 'pie' && dim && meas[0]) {
    return (
      <ResponsiveContainer>
        <PieChart>
          <Pie data={rows} dataKey={meas[0]} nameKey={dim} cx="50%" cy="50%" innerRadius={40} outerRadius={75} paddingAngle={2}>
            {rows.map((_, i) => (
              <Cell key={i} fill={COLORES[i % COLORES.length]} />
            ))}
          </Pie>
          <Tooltip {...ESTILO_TOOLTIP} formatter={(v) => formatearValor(v, measFmt(meas[0]))} />
          <Legend wrapperStyle={{ fontSize: 10 }} />
        </PieChart>
      </ResponsiveContainer>
    )
  }

  // tabla (o fallback si falta dimensión para un gráfico)
  return (
    <table className="w-full text-[11px]">
      <thead className="sticky top-0 border-b border-black/[0.06] bg-white text-graphite-600">
        <tr>
          {result.columns.map((c) => (
            <th key={c.id} className={`px-2 py-1.5 font-semibold ${c.kind === 'measure' ? 'text-right' : 'text-left'}`}>
              {c.label}
            </th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.map((r, i) => (
          <tr key={i} className="border-b border-black/[0.04] hover:bg-black/[0.02]">
            {result.columns.map((c) => (
              <td key={c.id} className={`px-2 py-1 ${c.kind === 'measure' ? 'text-right tabular-nums' : 'text-graphite-600'}`}>
                {formatearValor(r[c.id], c.format)}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  )
}
