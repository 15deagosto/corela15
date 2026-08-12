import type { CSSProperties, ReactNode } from 'react'

export function TableContainer({ children }: { children: ReactNode }) {
  return (
    <div className="overflow-hidden rounded-xl border border-black/[0.06] bg-white shadow-sm">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">{children}</table>
      </div>
    </div>
  )
}

export function Th({ children }: { children: ReactNode }) {
  return (
    <th className="border-b border-black/[0.06] bg-black/[0.015] px-4 py-2.5 text-left text-xs font-semibold uppercase tracking-wide text-graphite-600">
      {children}
    </th>
  )
}

export function Td({
  children,
  className = '',
  style,
}: {
  children: ReactNode
  className?: string
  style?: CSSProperties
}) {
  return (
    <td className={`px-4 py-3 text-graphite-100 ${className}`} style={style}>
      {children}
    </td>
  )
}

export function EmptyState({ children }: { children: ReactNode }) {
  return (
    <tr>
      <td colSpan={100} className="px-4 py-12 text-center text-sm text-graphite-600">
        {children}
      </td>
    </tr>
  )
}
