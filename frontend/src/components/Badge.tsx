import type { ReactNode } from 'react'

const variantes = {
  neutral: 'bg-graphite-950 text-graphite-600',
  exito: 'bg-petrol-800/10 text-petrol-700',
  alerta: 'bg-gold-500/15 text-gold-300',
  peligro: 'bg-red-600/10 text-red-700',
} as const

interface BadgeProps {
  children: ReactNode
  variant?: keyof typeof variantes
}

export function Badge({ children, variant = 'neutral' }: BadgeProps) {
  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${variantes[variant]}`}>
      {children}
    </span>
  )
}
