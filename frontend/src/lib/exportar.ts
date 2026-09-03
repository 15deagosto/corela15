import * as XLSX from 'xlsx'
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'

/**
 * Exportación real de reportes a Excel/PDF/CSV — pedido explícito del
 * usuario ("los reportes deben poder exportarse a excel, pdf, csv...").
 * `xlsx` (SheetJS) se usa acá solo en modo escritura (json_to_sheet +
 * writeFile, nunca lee un archivo externo) — las 2 vulnerabilidades
 * reales conocidas del paquete (Prototype Pollution/ReDoS,
 * GHSA-4r6h-8v6p-xvw6 / GHSA-5pgg-2g8v-p4x9) son de parseo de un XLSX
 * externo no confiable, no de generación — no aplican a este uso.
 */

export interface ColumnaExportable<T> {
  header: string
  accessor: (fila: T) => string | number
}

function celda(valor: string | number): string {
  return typeof valor === 'number' ? valor.toString() : valor
}

export function exportarCsv<T>(nombreArchivo: string, columnas: ColumnaExportable<T>[], filas: T[]) {
  const encabezado = columnas.map((c) => `"${c.header.replace(/"/g, '""')}"`).join(',')
  const lineas = filas.map((f) =>
    columnas.map((c) => `"${celda(c.accessor(f)).replace(/"/g, '""')}"`).join(','),
  )
  const contenido = '﻿' + [encabezado, ...lineas].join('\r\n')
  const blob = new Blob([contenido], { type: 'text/csv;charset=utf-8;' })
  descargarBlob(blob, `${nombreArchivo}.csv`)
}

export function exportarExcel<T>(nombreArchivo: string, nombreHoja: string, columnas: ColumnaExportable<T>[], filas: T[]) {
  const datos = filas.map((f) => {
    const fila: Record<string, string | number> = {}
    columnas.forEach((c) => {
      fila[c.header] = c.accessor(f)
    })
    return fila
  })
  const hoja = XLSX.utils.json_to_sheet(datos)
  const libro = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(libro, hoja, nombreHoja.slice(0, 31))
  XLSX.writeFile(libro, `${nombreArchivo}.xlsx`)
}

export function exportarPdf<T>(
  nombreArchivo: string,
  titulo: string,
  columnas: ColumnaExportable<T>[],
  filas: T[],
  subtitulo?: string,
) {
  const doc = new jsPDF({ orientation: columnas.length > 6 ? 'landscape' : 'portrait' })

  doc.setFontSize(13)
  doc.text(titulo, 14, 15)
  if (subtitulo) {
    doc.setFontSize(9)
    doc.setTextColor(100)
    doc.text(subtitulo, 14, 21)
  }

  autoTable(doc, {
    startY: subtitulo ? 26 : 22,
    head: [columnas.map((c) => c.header)],
    body: filas.map((f) => columnas.map((c) => celda(c.accessor(f)))),
    styles: { fontSize: 8, cellPadding: 2 },
    headStyles: { fillColor: [212, 165, 74] },
    margin: { left: 14, right: 14 },
  })

  doc.save(`${nombreArchivo}.pdf`)
}

function descargarBlob(blob: Blob, nombreArchivo: string) {
  const url = URL.createObjectURL(blob)
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = nombreArchivo
  document.body.appendChild(enlace)
  enlace.click()
  document.body.removeChild(enlace)
  URL.revokeObjectURL(url)
}
