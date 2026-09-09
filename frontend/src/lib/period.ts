// Períodos rápidos para filtrar por fecha en los tableros predefinidos de
// Reportería Gerencial. Se traducen a valores ISO (yyyy-MM-dd) que van
// directo a los filtros fechaDesde/fechaHasta del modelo semántico -- el
// backend los parametriza, nunca se concatenan. Portado de SIGA.

export type PeriodoPreset = 'todo' | 'anioActual' | 'anioAnterior' | 'ultimos12m' | 'esteMes' | 'personalizado'

export interface Periodo {
  preset: PeriodoPreset
  desde: string | null
  hasta: string | null
}

function iso(d: Date): string {
  return d.toISOString().slice(0, 10)
}

export function calcularPeriodo(preset: PeriodoPreset, personalizado?: { desde: string | null; hasta: string | null }): Periodo {
  const hoy = new Date()
  switch (preset) {
    case 'todo':
      return { preset, desde: null, hasta: null }
    case 'anioActual':
      return { preset, desde: `${hoy.getFullYear()}-01-01`, hasta: iso(hoy) }
    case 'anioAnterior': {
      const y = hoy.getFullYear() - 1
      return { preset, desde: `${y}-01-01`, hasta: `${y}-12-31` }
    }
    case 'ultimos12m': {
      const desde = new Date(hoy)
      desde.setMonth(desde.getMonth() - 12)
      return { preset, desde: iso(desde), hasta: iso(hoy) }
    }
    case 'esteMes': {
      const desde = new Date(hoy.getFullYear(), hoy.getMonth(), 1)
      return { preset, desde: iso(desde), hasta: iso(hoy) }
    }
    case 'personalizado':
      return { preset, desde: personalizado?.desde ?? null, hasta: personalizado?.hasta ?? null }
  }
}

export const PERIODO_LABELS: Record<PeriodoPreset, string> = {
  todo: 'Todo el histórico',
  anioActual: 'Año actual',
  anioAnterior: 'Año anterior',
  ultimos12m: 'Últimos 12 meses',
  esteMes: 'Este mes',
  personalizado: 'Personalizado',
}
