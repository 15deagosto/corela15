import type { ReactNode } from 'react'
import { Loader2, AlertCircle } from 'lucide-react'

/** Paleta compartida por los tableros predefinidos, tomada tal cual de siga-web (marfil/dorado/petróleo). */
export const COLORES = ['#c9a665', '#5a8ba8', '#a17d3a', '#3d6b86', '#b58e4a', '#2c5670', '#82642c', '#204357']

export const ESTILO_TOOLTIP = {
  contentStyle: { background: '#ffffff', border: '1px solid #a2a8b1', borderRadius: 8, fontSize: 12 },
  itemStyle: { color: '#1d212a' },
  labelStyle: { color: '#b58e4a', fontWeight: 600, marginBottom: 4 },
  cursor: { fill: 'rgba(0,0,0,0.04)' },
}

export function Tarjeta({ label, valor, sub, alerta }: { label: string; valor: string; sub?: string; alerta?: boolean }) {
  return (
    <div className={`glass-card rounded-2xl p-4 ${alerta ? 'border border-amber-500/40' : ''}`}>
      <div className="text-[11px] uppercase tracking-wider text-graphite-600">{label}</div>
      <div className={`mt-1 text-2xl font-extrabold tabular-nums ${alerta ? 'text-amber-600' : 'text-graphite-100'}`}>{valor}</div>
      {sub && <div className={`mt-1 text-xs ${alerta ? 'text-amber-600/80' : 'text-graphite-600'}`}>{sub}</div>}
    </div>
  )
}

export function Panel({ titulo, children }: { titulo: string; children: ReactNode }) {
  return (
    <section className="glass-panel rounded-2xl p-5">
      <h2 className="mb-3 text-sm font-bold text-graphite-100">{titulo}</h2>
      {children}
    </section>
  )
}

export function CargandoTablero() {
  return (
    <div className="flex h-64 items-center justify-center gap-2 text-graphite-500">
      <Loader2 className="animate-spin" size={18} /> Cargando tablero…
    </div>
  )
}

export function ErrorTablero({ mensaje }: { mensaje: string }) {
  return (
    <div className="flex h-64 items-center justify-center gap-2 text-red-700">
      <AlertCircle size={18} /> {mensaje}
    </div>
  )
}

export function n(row: Record<string, unknown> | undefined, k: string): number {
  return Number(row?.[k] ?? 0)
}
