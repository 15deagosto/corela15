import type { LucideIcon } from 'lucide-react'

interface PageHeaderProps {
  icon: LucideIcon
  title: string
  subtitle: string
  actions?: React.ReactNode
}

export function PageHeader({ icon: Icon, title, subtitle, actions }: PageHeaderProps) {
  return (
    <div className="mb-6 flex items-start justify-between gap-4">
      <div className="flex items-start gap-3">
        <span className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-gold-500/15 text-gold-400">
          <Icon size={19} strokeWidth={1.75} />
        </span>
        <div>
          <h1 className="text-xl font-semibold text-graphite-100">{title}</h1>
          <p className="text-sm text-graphite-600">{subtitle}</p>
        </div>
      </div>
      {actions}
    </div>
  )
}
