import jsPDF from 'jspdf'

/**
 * PDF real de Planificación semanal -- no es un reporte tabular genérico
 * (los de `exportar.ts`), es el reemplazo directo del PDF manual que cada
 * jefatura armaba a mano (ver `Planificacion_Semanal_TI_07-12sep2026.pdf`,
 * el documento de referencia real que dio origen a este módulo). Replica
 * su estructura real: banner institucional, leyenda de categorías con
 * color, grilla Lunes-a-Viernes con bloques posicionados por horario real,
 * sección aparte para Sábado (rango horario real distinto), y firma del
 * responsable -- dibujado directo con primitivas de jsPDF (rect/text),
 * nunca una tabla autoTable genérica.
 */

interface BloquePdf {
  diaSemana: number
  horaInicio: string
  horaFin: string
  etiqueta: string
  colorHex: string
  descripcion: string
}

interface PlanPdfData {
  area: string
  fechaInicioSemana: string
  nombreResponsable: string
  cargoResponsable: string
  bloques: BloquePdf[]
}

const DIAS_PDF = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado']

function hexToRgb(hex: string): [number, number, number] {
  const h = hex.replace('#', '')
  return [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16)]
}

function minutosDe(hora: string): number {
  const [h, m] = hora.split(':').map(Number)
  return h * 60 + m
}

export function exportarPlanificacionPdf(plan: PlanPdfData) {
  if (plan.bloques.length === 0) return

  const doc = new jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a4' })
  const PAGE_W = doc.internal.pageSize.getWidth()
  const PAGE_H = doc.internal.pageSize.getHeight()
  const MARGIN = 28

  // ---- Banner institucional ----
  const HEADER_H = 78
  doc.setFillColor(22, 33, 62)
  doc.rect(0, 0, PAGE_W, HEADER_H, 'F')
  doc.setFillColor(201, 162, 39)
  doc.rect(0, HEADER_H, PAGE_W, 3, 'F')

  doc.setTextColor(255, 255, 255)
  doc.setFont('helvetica', 'bold')
  doc.setFontSize(11)
  doc.text('COOPERATIVA DE AHORRO Y CRÉDITO 15 DE AGOSTO DE PILACOTO', MARGIN, 22)

  doc.setFont('helvetica', 'normal')
  doc.setFontSize(8)
  doc.setTextColor(201, 162, 39)
  doc.text(plan.area.toUpperCase(), MARGIN, 38)

  doc.setFont('helvetica', 'bold')
  doc.setFontSize(18)
  doc.setTextColor(255, 255, 255)
  doc.text('Planificación Semanal de Actividades', MARGIN, 58)

  const lunes = new Date(`${plan.fechaInicioSemana}T00:00:00`)
  const sabado = new Date(lunes)
  sabado.setDate(lunes.getDate() + 5)
  const fmt = (d: Date) => d.toLocaleDateString('es-EC', { day: '2-digit', month: 'long' })
  doc.setFont('helvetica', 'normal')
  doc.setFontSize(9)
  doc.text(`Semana: ${fmt(lunes)} al ${fmt(sabado)} de ${lunes.getFullYear()}`, MARGIN, 72)

  let y = HEADER_H + 16

  // ---- Leyenda real (solo las etiquetas realmente usadas esta semana) ----
  const etiquetasUsadas = Array.from(new Map(plan.bloques.map((b) => [b.etiqueta, b.colorHex])).entries())
  doc.setFontSize(7.5)
  let lx = MARGIN
  etiquetasUsadas.forEach(([nombre, color]) => {
    const [r, g, b] = hexToRgb(color)
    doc.setFont('helvetica', 'normal')
    const textW = doc.getTextWidth(nombre)
    if (lx + 14 + textW + 16 > PAGE_W - MARGIN) {
      lx = MARGIN
      y += 13
    }
    doc.setFillColor(r, g, b)
    doc.roundedRect(lx, y - 7, 8, 8, 1, 1, 'F')
    doc.setTextColor(70, 70, 70)
    doc.text(nombre, lx + 12, y)
    lx += 12 + textW + 16
  })
  y += 20

  // ---- Presupuesto de espacio vertical compartido entre las 2 grillas ----
  const bloquesSemana = plan.bloques.filter((b) => b.diaSemana >= 1 && b.diaSemana <= 5)
  const bloquesSabado = plan.bloques.filter((b) => b.diaSemana === 6)

  const rangoDe = (bloques: BloquePdf[]) => {
    if (bloques.length === 0) return { inicio: 0, fin: 0, total: 0 }
    const inicio = Math.floor(Math.min(...bloques.map((b) => minutosDe(b.horaInicio))) / 60) * 60
    const fin = Math.ceil(Math.max(...bloques.map((b) => minutosDe(b.horaFin))) / 60) * 60
    return { inicio, fin, total: fin - inicio }
  }
  const rangoSemana = rangoDe(bloquesSemana)
  const rangoSabado = rangoDe(bloquesSabado)

  const RESERVA_FOOTER = 55
  const RESERVA_POR_SECCION = 10 + 16 + 18 // título + encabezado de día + espacio final
  const secciones = (bloquesSemana.length > 0 ? 1 : 0) + (bloquesSabado.length > 0 ? 1 : 0)
  const disponible = PAGE_H - y - RESERVA_FOOTER - secciones * RESERVA_POR_SECCION
  const totalMinutos = Math.max(rangoSemana.total + rangoSabado.total, 1)
  const rowH = Math.min(0.62, disponible / totalMinutos)

  const dibujarSeccion = (
    titulo: string, dias: number[], bloques: BloquePdf[], rango: { inicio: number; fin: number; total: number },
    yStart: number, unaColumna: boolean,
  ): number => {
    if (bloques.length === 0) return yStart

    doc.setFont('helvetica', 'bold')
    doc.setFontSize(8)
    doc.setTextColor(140, 130, 60)
    doc.text(titulo.toUpperCase(), MARGIN, yStart)
    let yy = yStart + 10

    const gridW = PAGE_W - MARGIN * 2
    const timeColW = 40
    const colW = unaColumna ? gridW - timeColW : (gridW - timeColW) / dias.length
    const gridH = rango.total * rowH
    const headerRowH = 16

    doc.setFillColor(245, 243, 235)
    doc.rect(MARGIN, yy, gridW, headerRowH, 'F')
    doc.setFontSize(7.5)
    doc.setTextColor(90, 90, 90)
    doc.setFont('helvetica', 'bold')
    if (!unaColumna) {
      dias.forEach((d, i) => {
        const cx = MARGIN + timeColW + colW * i + colW / 2
        doc.text(DIAS_PDF[d - 1].toUpperCase(), cx, yy + 11, { align: 'center' })
      })
    } else {
      doc.text(DIAS_PDF[dias[0] - 1].toUpperCase(), MARGIN + timeColW + colW / 2, yy + 11, { align: 'center' })
    }
    yy += headerRowH

    doc.setDrawColor(228, 224, 210)
    for (let m = rango.inicio; m <= rango.fin; m += 60) {
      const ly = yy + (m - rango.inicio) * rowH
      doc.line(MARGIN, ly, MARGIN + gridW, ly)
      doc.setFontSize(6.5)
      doc.setTextColor(150, 150, 150)
      doc.setFont('helvetica', 'normal')
      doc.text(`${Math.floor(m / 60).toString().padStart(2, '0')}:00`, MARGIN + 2, ly + 8)
    }
    doc.setDrawColor(205, 200, 185)
    doc.rect(MARGIN, yy, gridW, gridH)
    doc.line(MARGIN + timeColW, yy, MARGIN + timeColW, yy + gridH)
    if (!unaColumna) {
      for (let i = 1; i < dias.length; i++) {
        const lx2 = MARGIN + timeColW + colW * i
        doc.line(lx2, yy, lx2, yy + gridH)
      }
    }

    dias.forEach((d, i) => {
      const colX = MARGIN + timeColW + colW * i
      const bd = bloques.filter((b) => b.diaSemana === d).sort((a, b) => minutosDe(a.horaInicio) - minutosDe(b.horaInicio))
      bd.forEach((b) => {
        const top = yy + (minutosDe(b.horaInicio) - rango.inicio) * rowH
        const h = Math.max((minutosDe(b.horaFin) - minutosDe(b.horaInicio)) * rowH, 13)
        const [r, g, bl] = hexToRgb(b.colorHex)
        doc.setFillColor(r, g, bl)
        doc.roundedRect(colX + 1.2, top + 0.8, colW - 2.4, h - 1.6, 1.5, 1.5, 'F')
        doc.setTextColor(255, 255, 255)
        doc.setFont('helvetica', 'bold')
        doc.setFontSize(6.3)
        doc.text(`${b.horaInicio.slice(0, 5)}–${b.horaFin.slice(0, 5)}`, colX + 4, top + 9)
        if (b.descripcion) {
          doc.setFont('helvetica', 'normal')
          doc.setFontSize(6.3)
          const lineas = doc.splitTextToSize(b.descripcion, colW - 8)
          const maxLineas = Math.max(1, Math.floor((h - 10) / 6.8))
          doc.text(lineas.slice(0, maxLineas), colX + 4, top + 17)
        }
      })
    })

    return yy + gridH + 18
  }

  y = dibujarSeccion('Lunes a viernes', [1, 2, 3, 4, 5], bloquesSemana, rangoSemana, y, false)
  y = dibujarSeccion('Sábado', [6], bloquesSabado, rangoSabado, y, true)

  // ---- Pie y firma real del responsable ----
  const footerY = PAGE_H - 38
  doc.setDrawColor(222, 218, 205)
  doc.line(MARGIN, footerY - 10, PAGE_W - MARGIN, footerY - 10)
  doc.setFont('helvetica', 'italic')
  doc.setFontSize(7)
  doc.setTextColor(140, 140, 140)
  doc.text(`Planificación semanal de actividades de ${plan.area} — Cooperativa de Ahorro y Crédito 15 de Agosto de Pilacoto.`, MARGIN, footerY)

  doc.setFont('helvetica', 'bold')
  doc.setFontSize(9)
  doc.setTextColor(40, 40, 40)
  doc.text(plan.nombreResponsable, PAGE_W - MARGIN, footerY - 4, { align: 'right' })
  doc.setFont('helvetica', 'normal')
  doc.setFontSize(8)
  doc.setTextColor(100, 100, 100)
  doc.text(plan.cargoResponsable || plan.area, PAGE_W - MARGIN, footerY + 8, { align: 'right' })

  doc.save(`planificacion-${plan.fechaInicioSemana}.pdf`)
}
