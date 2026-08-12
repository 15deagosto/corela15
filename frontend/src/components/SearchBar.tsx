import { Search } from 'lucide-react'

interface SearchBarProps {
  value: string
  onChange: (value: string) => void
  placeholder?: string
}

export function SearchBar({ value, onChange, placeholder = 'Buscar…' }: SearchBarProps) {
  return (
    <div className="relative w-full max-w-xs">
      <Search size={15} strokeWidth={2} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-graphite-700" />
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full rounded-lg border border-black/[0.08] bg-white py-2 pl-9 pr-3 text-sm text-graphite-100 outline-none placeholder:text-graphite-700 focus:border-gold-500/50 focus:ring-2 focus:ring-gold-500/15"
      />
    </div>
  )
}
