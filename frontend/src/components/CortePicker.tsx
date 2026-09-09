import { useState } from 'react'
import { CalendarClock, ChevronDown } from 'lucide-react'

interface Props {
  corte: string | null
  onChange: (corte: string | null) => void
}

/**
 * Selector de fecha de corte para datasets con soporte de foto histórica
 * (FOR SYSTEM_TIME AS OF) -- reconstruye el estado real de la cartera en un
 * instante del pasado (saldo, mora, calificación tal como estaban ese día).
 * Portado de SIGA, adaptado al tema claro de Corela15.
 */
export function CortePicker({ corte, onChange }: Props) {
  const [abierto, setAbierto] = useState(false)
  const hoy = new Date().toISOString().slice(0, 10)

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setAbierto((v) => !v)}
        title="Foto histórica del estado de la cartera (saldo, mora, calificación) a una fecha pasada"
        className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-xs font-medium text-graphite-100 hover:bg-black/[0.02]"
      >
        <CalendarClock size={14} />
        {corte ? `Corte: ${corte}` : 'Hoy (en vivo)'}
        <ChevronDown size={12} />
      </button>

      {abierto && (
        <>
          <div className="fixed inset-0 z-40" onClick={() => setAbierto(false)} />
          <div className="absolute right-0 z-50 mt-2 w-64 space-y-2 rounded-xl border border-black/[0.08] bg-white p-3 shadow-xl">
            <button
              type="button"
              onClick={() => {
                onChange(null)
                setAbierto(false)
              }}
              className={`block w-full rounded-lg px-3 py-2 text-left text-xs ${
                corte === null ? 'bg-gold-500/10 text-gold-500' : 'text-graphite-100 hover:bg-black/[0.03]'
              }`}
            >
              Hoy (en vivo)
            </button>
            <div>
              <label className="mb-1 block text-[10px] text-graphite-600">Fecha de corte (foto histórica)</label>
              <input
                type="date"
                max={hoy}
                value={corte ?? ''}
                onChange={(e) => onChange(e.target.value || null)}
                className="w-full rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </div>
            <p className="text-[10px] leading-snug text-graphite-600">
              Reconstruye saldo, mora y calificación exactamente como estaban ese día — no filtra por cuándo se originó el crédito.
            </p>
          </div>
        </>
      )}
    </div>
  )
}
