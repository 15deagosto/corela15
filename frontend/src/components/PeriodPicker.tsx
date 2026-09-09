import { useState } from 'react'
import { CalendarRange, ChevronDown } from 'lucide-react'
import { calcularPeriodo, PERIODO_LABELS, type Periodo, type PeriodoPreset } from '../lib/period'

const PRESETS: PeriodoPreset[] = ['todo', 'anioActual', 'anioAnterior', 'ultimos12m', 'esteMes', 'personalizado']

interface Props {
  periodo: Periodo
  onChange: (p: Periodo) => void
}

/** Selector de período rápido para los tableros de Reportería Gerencial. Portado de SIGA, adaptado al tema claro de Corela15. */
export function PeriodPicker({ periodo, onChange }: Props) {
  const [abierto, setAbierto] = useState(false)

  const elegir = (preset: PeriodoPreset) => {
    if (preset === 'personalizado') {
      onChange(calcularPeriodo('personalizado', { desde: periodo.desde, hasta: periodo.hasta }))
      return // deja el panel abierto para que ingrese las fechas
    }
    onChange(calcularPeriodo(preset))
    setAbierto(false)
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setAbierto((v) => !v)}
        className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-xs font-medium text-graphite-100 hover:bg-black/[0.02]"
      >
        <CalendarRange size={14} />
        {PERIODO_LABELS[periodo.preset]}
        <ChevronDown size={12} />
      </button>

      {abierto && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setAbierto(false)} />
          <div className="absolute right-0 z-50 mt-2 w-64 rounded-xl border border-black/[0.08] bg-white p-2 shadow-xl">
            {PRESETS.map((p) => (
              <button
                key={p}
                type="button"
                onClick={() => elegir(p)}
                className={`block w-full rounded-lg px-3 py-2 text-left text-xs transition-colors ${
                  periodo.preset === p ? 'bg-gold-500/10 text-gold-500' : 'text-graphite-100 hover:bg-black/[0.03]'
                }`}
              >
                {PERIODO_LABELS[p]}
              </button>
            ))}

            {periodo.preset === 'personalizado' && (
              <div className="mt-1 space-y-2 border-t border-black/[0.06] p-2 pt-2">
                <div>
                  <label className="mb-1 block text-[10px] text-graphite-600">Desde</label>
                  <input
                    type="date"
                    value={periodo.desde ?? ''}
                    onChange={(e) => onChange({ ...periodo, desde: e.target.value || null })}
                    className="w-full rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
                  />
                </div>
                <div>
                  <label className="mb-1 block text-[10px] text-graphite-600">Hasta</label>
                  <input
                    type="date"
                    value={periodo.hasta ?? ''}
                    onChange={(e) => onChange({ ...periodo, hasta: e.target.value || null })}
                    className="w-full rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
                  />
                </div>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  )
}
