import { FileSpreadsheet, FileText, FileDown } from 'lucide-react'
import { exportarCsv, exportarExcel, exportarPdf, type ColumnaExportable } from '../lib/exportar'

interface BotonesExportarProps<T> {
  nombreArchivo: string
  titulo: string
  subtitulo?: string
  columnas: ColumnaExportable<T>[]
  filas: T[]
}

/**
 * Botones de exportación real reusables por todo reporte del proyecto —
 * mismo patrón visual en todas partes, un solo lugar donde corregir el
 * comportamiento de exportación si hace falta.
 */
export function BotonesExportar<T>({ nombreArchivo, titulo, subtitulo, columnas, filas }: BotonesExportarProps<T>) {
  const sinDatos = filas.length === 0

  return (
    <div className="flex items-center gap-2">
      <button
        type="button"
        disabled={sinDatos}
        onClick={() => exportarExcel(nombreArchivo, titulo, columnas, filas)}
        className="flex items-center gap-1 rounded-lg border border-black/[0.08] px-2.5 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-40"
        title="Exportar a Excel"
      >
        <FileSpreadsheet size={13} /> Excel
      </button>
      <button
        type="button"
        disabled={sinDatos}
        onClick={() => exportarPdf(nombreArchivo, titulo, columnas, filas, subtitulo)}
        className="flex items-center gap-1 rounded-lg border border-black/[0.08] px-2.5 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-40"
        title="Exportar a PDF"
      >
        <FileText size={13} /> PDF
      </button>
      <button
        type="button"
        disabled={sinDatos}
        onClick={() => exportarCsv(nombreArchivo, columnas, filas)}
        className="flex items-center gap-1 rounded-lg border border-black/[0.08] px-2.5 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-40"
        title="Exportar a CSV"
      >
        <FileDown size={13} /> CSV
      </button>
    </div>
  )
}
