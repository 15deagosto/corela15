import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Briefcase, Plus, X, RefreshCw, ShieldCheck } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { BotonesExportar } from '../components/BotonesExportar'
import type { ColumnaExportable } from '../lib/exportar'
import { api } from '../lib/api'
import { idempotencyKey } from '../lib/idempotencyKey'

interface Institucion {
  codigo: string
  nombre: string
  tipoInstitucion: string
  activa: boolean
}

interface Inversion {
  id: string
  documento: string
  institucion: string
  valorNominal: number
  tasa: number
  fechaCompra: string
  fechaVencimiento: string
  estado: string
  codigoCalificacionRiesgo: string | null
  nombreCalificacionRiesgo: string | null
  codigoCalificadoraRiesgo: string | null
  nombreCalificadoraRiesgo: string | null
  fechaUltimaCalificacion: string | null
  provisionConstituida: number | null
}

interface CatalogoItem {
  codigo: string
  nombre: string
  activo: boolean
}

interface I02Elemento {
  documento: string
  nombreInstitucion: string
  fechaCompra: string
  fechaVencimiento: string
  cuentaContable: string
  valorLibros: number
  estado: string
  codigoCalificacionRiesgo: string | null
  codigoCalificadoraRiesgo: string | null
  fechaUltimaCalificacion: string | null
  tasa: number
  provisionConstituida: number | null
}

interface I02Reporte {
  cabecera: { codigoEstructura: string; ruc: string; fechaCorte: string; numeroTotalRegistros: number }
  detalle: I02Elemento[]
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function estadoVariant(estado: string) {
  if (estado === 'Cancelada') return 'neutral' as const
  if (estado === 'Anulada') return 'peligro' as const
  return 'exito' as const
}

function AbrirInversionForm({ instituciones, onClose }: { instituciones: Institucion[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [documento, setDocumento] = useState('')
  const [codigoInstitucion, setCodigoInstitucion] = useState('')
  const [valorNominal, setValorNominal] = useState('')
  const [tasa, setTasa] = useState('')
  const [fechaCompra, setFechaCompra] = useState('')
  const [fechaVencimiento, setFechaVencimiento] = useState('')

  const abrir = useMutation({
    mutationFn: async () =>
      api.post(
        '/api/portafolio/inversiones',
        {
          documento,
          idAgencia: 1,
          codigoInstitucion,
          valorNominal: Number(valorNominal),
          tasa: Number(tasa) / 100,
          fechaCompra,
          fechaVencimiento,
        },
        { headers: { 'Idempotency-Key': idempotencyKey() } },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portafolio-inversiones'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nueva inversión</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!documento || !codigoInstitucion || !valorNominal || !tasa || !fechaCompra || !fechaVencimiento) return
          abrir.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Documento (Certificado de Inversión)</span>
          <input
            required
            type="text"
            value={documento}
            onChange={(e) => setDocumento(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Institución</span>
          <select
            required
            value={codigoInstitucion}
            onChange={(e) => setCodigoInstitucion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {instituciones.map((i) => (
              <option key={i.codigo} value={i.codigo}>
                {i.nombre} — {i.tipoInstitucion}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Valor nominal</span>
          <input
            required
            type="number"
            step="0.01"
            min="0.01"
            value={valorNominal}
            onChange={(e) => setValorNominal(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Tasa anual (%)</span>
          <input
            required
            type="number"
            step="0.01"
            min="0.01"
            value={tasa}
            onChange={(e) => setTasa(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div />

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Fecha de compra</span>
          <input
            required
            type="date"
            value={fechaCompra}
            onChange={(e) => setFechaCompra(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Fecha de vencimiento</span>
          <input
            required
            type="date"
            value={fechaVencimiento}
            onChange={(e) => setFechaVencimiento(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end">
          <button
            type="submit"
            disabled={abrir.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {abrir.isPending ? 'Registrando…' : 'Abrir inversión'}
          </button>
        </div>

        {abrir.isError && (
          <p className="sm:col-span-3 text-sm text-red-700">
            {(abrir.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo abrir la inversión.'}
          </p>
        )}
      </form>
    </div>
  )
}

function RenovarInversionModal({ inversion, onClose }: { inversion: Inversion; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [documentoNuevo, setDocumentoNuevo] = useState('')
  const [fechaVencimientoNueva, setFechaVencimientoNueva] = useState('')

  const renovar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/portafolio/inversiones/${inversion.id}/renovar`,
          { documentoNuevo, fechaVencimientoNueva },
          { headers: { 'Idempotency-Key': idempotencyKey() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portafolio-inversiones'] })
    },
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="glass-card animate-zoom-in max-h-[90vh] w-full max-w-md overflow-y-auto rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">Renovar — {inversion.documento}</h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        {!renovar.isSuccess ? (
          <form
            className="flex flex-col gap-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!documentoNuevo || !fechaVencimientoNueva) return
              renovar.mutate()
            }}
          >
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Documento nuevo</span>
              <input
                required
                type="text"
                value={documentoNuevo}
                onChange={(e) => setDocumentoNuevo(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Nueva fecha de vencimiento</span>
              <input
                required
                type="date"
                value={fechaVencimientoNueva}
                onChange={(e) => setFechaVencimientoNueva(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <button
              type="submit"
              disabled={renovar.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {renovar.isPending ? 'Renovando…' : 'Renovar'}
            </button>
            {renovar.isError && (
              <p className="text-sm text-red-700">
                {(renovar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
                  'No se pudo renovar la inversión.'}
              </p>
            )}
          </form>
        ) : (
          <div className="flex flex-col gap-3">
            <p className="rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
              Inversión renovada bajo el documento <strong>{documentoNuevo}</strong>.
              {(renovar.data as { interesGanado: number }).interesGanado > 0 && (
                <>
                  {' '}
                  Se liquidó {formatoUsd((renovar.data as { interesGanado: number }).interesGanado)} de interés
                  devengado del documento original.
                </>
              )}
            </p>
            <button
              type="button"
              onClick={onClose}
              className="btn-hover self-start rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white"
            >
              Cerrar
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

function CalificarRiesgoModal({
  inversion,
  calificaciones,
  calificadoras,
  onClose,
}: {
  inversion: Inversion
  calificaciones: CatalogoItem[]
  calificadoras: CatalogoItem[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [codigoCalificacionRiesgo, setCodigoCalificacionRiesgo] = useState(inversion.codigoCalificacionRiesgo ?? '')
  const [codigoCalificadoraRiesgo, setCodigoCalificadoraRiesgo] = useState(inversion.codigoCalificadoraRiesgo ?? '')
  const [fechaUltimaCalificacion, setFechaUltimaCalificacion] = useState(inversion.fechaUltimaCalificacion ?? '')
  const [provisionConstituida, setProvisionConstituida] = useState(String(inversion.provisionConstituida ?? ''))

  const guardar = useMutation({
    mutationFn: async () =>
      api.patch(`/api/portafolio/inversiones/${inversion.id}/calificacion-riesgo`, {
        codigoCalificacionRiesgo: codigoCalificacionRiesgo || null,
        codigoCalificadoraRiesgo: codigoCalificadoraRiesgo || null,
        fechaUltimaCalificacion: fechaUltimaCalificacion || null,
        provisionConstituida: provisionConstituida === '' ? null : Number(provisionConstituida),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portafolio-inversiones'] })
      onClose()
    },
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="glass-card animate-zoom-in max-h-[90vh] w-full max-w-md overflow-y-auto rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">Calificación de riesgo — {inversion.documento}</h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>
        <p className="mb-3 text-xs text-graphite-600">
          Catálogo real SEPS (estructura I02) — calificación del emisor/depositario y la calificadora que la emitió.
        </p>
        <form
          className="flex flex-col gap-3"
          onSubmit={(e) => {
            e.preventDefault()
            guardar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Calificación de riesgo</span>
            <select
              value={codigoCalificacionRiesgo}
              onChange={(e) => setCodigoCalificacionRiesgo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Sin calificar</option>
              {calificaciones.map((c) => (
                <option key={c.codigo} value={c.codigo}>
                  {c.codigo} — {c.nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Calificadora</span>
            <select
              value={codigoCalificadoraRiesgo}
              onChange={(e) => setCodigoCalificadoraRiesgo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Sin especificar</option>
              {calificadoras.map((c) => (
                <option key={c.codigo} value={c.codigo}>
                  {c.nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Fecha de la última calificación</span>
            <input
              type="date"
              value={fechaUltimaCalificacion}
              onChange={(e) => setFechaUltimaCalificacion(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Provisión constituida (opcional)</span>
            <input
              type="number"
              step="0.01"
              min="0"
              value={provisionConstituida}
              onChange={(e) => setProvisionConstituida(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <button
            type="submit"
            disabled={guardar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {guardar.isPending ? 'Guardando…' : 'Guardar calificación'}
          </button>
          {guardar.isError && (
            <p className="text-sm text-red-700">
              {(guardar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
                'No se pudo guardar la calificación.'}
            </p>
          )}
        </form>
      </div>
    </div>
  )
}

const COLUMNAS_I02: ColumnaExportable<I02Elemento>[] = [
  { header: 'Documento', accessor: (e) => e.documento },
  { header: 'Institución', accessor: (e) => e.nombreInstitucion },
  { header: 'Cuenta contable', accessor: (e) => e.cuentaContable },
  { header: 'Valor en libros', accessor: (e) => e.valorLibros.toFixed(2) },
  { header: 'Tasa', accessor: (e) => `${(e.tasa * 100).toFixed(2)}%` },
  { header: 'Fecha compra', accessor: (e) => e.fechaCompra },
  { header: 'Fecha vencimiento', accessor: (e) => e.fechaVencimiento },
  { header: 'Calificación', accessor: (e) => e.codigoCalificacionRiesgo ?? '—' },
  { header: 'Calificadora', accessor: (e) => e.codigoCalificadoraRiesgo ?? '—' },
  { header: 'Fecha calificación', accessor: (e) => e.fechaUltimaCalificacion ?? '—' },
  { header: 'Provisión constituida', accessor: (e) => e.provisionConstituida?.toFixed(2) ?? '—' },
]

function SeccionReporteI02() {
  const { data, isLoading, isError } = useQuery<I02Reporte>({
    queryKey: ['portafolio-reporte-i02'],
    queryFn: async () => (await api.get('/api/portafolio/reportes/i02')).data,
  })

  return (
    <div>
      <p className="mb-4 text-xs text-graphite-600">
        Estructura real "Saldos de Inversiones I02" (SEPS) — cuenta contable resuelta con la misma regla real que ya
        usa el motor de apertura/renovación (plazo × sector de la institución contraparte), nunca un valor fijo.
      </p>
      {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}
      {isError && <p className="text-sm text-red-700">No se pudo generar el reporte.</p>}
      {data && (
        <>
          <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
              <div className="glass-card rounded-xl p-3">
                <p className="text-xs text-graphite-600">Estructura</p>
                <p className="text-lg font-semibold text-graphite-100">{data.cabecera.codigoEstructura}</p>
              </div>
              <div className="glass-card rounded-xl p-3">
                <p className="text-xs text-graphite-600">Fecha de corte</p>
                <p className="text-lg font-semibold text-graphite-100">{data.cabecera.fechaCorte}</p>
              </div>
              <div className="glass-card rounded-xl p-3">
                <p className="text-xs text-graphite-600">Registros</p>
                <p className="text-lg font-semibold text-graphite-100">{data.cabecera.numeroTotalRegistros}</p>
              </div>
            </div>
            <BotonesExportar
              nombreArchivo="portafolio-i02"
              titulo="Saldos de Inversiones (I02)"
              subtitulo={`Fecha de corte: ${data.cabecera.fechaCorte}`}
              columnas={COLUMNAS_I02}
              filas={data.detalle}
            />
          </div>
          <TableContainer>
            <thead>
              <tr>
                <Th>Documento</Th>
                <Th>Institución</Th>
                <Th>Cuenta contable</Th>
                <Th>Valor en libros</Th>
                <Th>Calificación</Th>
                <Th>Calificadora</Th>
              </tr>
            </thead>
            <tbody>
              {data.detalle.length === 0 && <EmptyState>Sin inversiones activas</EmptyState>}
              {data.detalle.map((e, i) => (
                <tr key={i} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                  <Td className="font-medium">{e.documento}</Td>
                  <Td>{e.nombreInstitucion}</Td>
                  <Td className="tabular-nums">{e.cuentaContable}</Td>
                  <Td className="tabular-nums">{formatoUsd(e.valorLibros)}</Td>
                  <Td>{e.codigoCalificacionRiesgo ?? <Badge variant="neutral">Sin calificar</Badge>}</Td>
                  <Td>{e.codigoCalificadoraRiesgo ?? '—'}</Td>
                </tr>
              ))}
            </tbody>
          </TableContainer>
        </>
      )}
    </div>
  )
}

const TABS = [
  { id: 'listado', label: 'Listado' },
  { id: 'reporte-i02', label: 'Reporte I02 (SEPS)' },
] as const

export function Portafolio() {
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<(typeof TABS)[number]['id']>('listado')
  const [mostrarForm, setMostrarForm] = useState(false)
  const [inversionARenovar, setInversionARenovar] = useState<Inversion | null>(null)
  const [inversionACalificar, setInversionACalificar] = useState<Inversion | null>(null)

  const { data: instituciones } = useQuery<Institucion[]>({
    queryKey: ['portafolio-instituciones'],
    queryFn: async () => (await api.get('/api/portafolio/instituciones')).data,
  })

  const { data: inversiones, isLoading } = useQuery<Inversion[]>({
    queryKey: ['portafolio-inversiones'],
    queryFn: async () => (await api.get('/api/portafolio/inversiones')).data,
  })

  const { data: calificaciones } = useQuery<CatalogoItem[]>({
    queryKey: ['portafolio-calificaciones-riesgo'],
    queryFn: async () => (await api.get('/api/portafolio/calificaciones-riesgo')).data,
  })

  const { data: calificadoras } = useQuery<CatalogoItem[]>({
    queryKey: ['portafolio-calificadoras-riesgo'],
    queryFn: async () => (await api.get('/api/portafolio/calificadoras-riesgo')).data,
  })

  const cancelar = useMutation({
    mutationFn: async (id: string) =>
      (
        await api.post(`/api/portafolio/inversiones/${id}/cancelar`, undefined, {
          headers: { 'Idempotency-Key': idempotencyKey() },
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portafolio-inversiones'] })
    },
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={Briefcase}
        title="Portafolio"
        subtitle="Inversiones propias de la cooperativa en otras instituciones financieras"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Nueva inversión
            </button>
          )
        }
      />

      <div className="mb-4 flex gap-1 border-b border-black/[0.06]">
        {TABS.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`px-3 py-2 text-sm font-medium ${
              tab === t.id ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'reporte-i02' && <SeccionReporteI02 />}

      {tab === 'listado' && (
        <>
      {mostrarForm && instituciones && <AbrirInversionForm instituciones={instituciones} onClose={() => setMostrarForm(false)} />}

      {inversionARenovar && (
        <ModalPortal>
          <RenovarInversionModal inversion={inversionARenovar} onClose={() => setInversionARenovar(null)} />
        </ModalPortal>
      )}

      {inversionACalificar && calificaciones && calificadoras && (
        <ModalPortal>
          <CalificarRiesgoModal
            inversion={inversionACalificar}
            calificaciones={calificaciones}
            calificadoras={calificadoras}
            onClose={() => setInversionACalificar(null)}
          />
        </ModalPortal>
      )}

      {cancelar.isSuccess && (
        <p className="mb-4 rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
          Inversión cancelada: {formatoUsd((cancelar.data as { valorDevuelto: number }).valorDevuelto)} de capital
          {(cancelar.data as { interesGanado: number }).interesGanado > 0 && (
            <> + {formatoUsd((cancelar.data as { interesGanado: number }).interesGanado)} de interés devengado proporcional</>
          )}
          .
        </p>
      )}
      {cancelar.isError && (
        <p className="mb-4 text-sm text-red-700">
          {(cancelar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
            'No se pudo cancelar la inversión.'}
        </p>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Documento</Th>
            <Th>Institución</Th>
            <Th>Valor nominal</Th>
            <Th>Tasa</Th>
            <Th>Compra</Th>
            <Th>Vencimiento</Th>
            <Th>Calificación</Th>
            <Th>Estado</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (inversiones?.length ?? 0) === 0 && <EmptyState>Todavía no hay inversiones registradas</EmptyState>}
          {inversiones?.map((i) => (
            <tr key={i.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{i.documento}</Td>
              <Td>{i.institucion}</Td>
              <Td className="tabular-nums">{formatoUsd(i.valorNominal)}</Td>
              <Td>{(i.tasa * 100).toFixed(2)}%</Td>
              <Td>{i.fechaCompra}</Td>
              <Td>{i.fechaVencimiento}</Td>
              <Td>
                {i.codigoCalificacionRiesgo ? (
                  <Badge
                    variant="exito"
                    title={i.fechaUltimaCalificacion ? `Calificado el ${i.fechaUltimaCalificacion}` : undefined}
                  >
                    {i.codigoCalificacionRiesgo}
                  </Badge>
                ) : (
                  <Badge variant="neutral">Sin calificar</Badge>
                )}
              </Td>
              <Td>
                <Badge variant={estadoVariant(i.estado)}>{i.estado}</Badge>
              </Td>
              <Td>
                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={() => setInversionACalificar(i)}
                    className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                  >
                    <ShieldCheck size={13} /> Calificar
                  </button>
                  {i.estado === 'Activa' && (
                    <>
                      <button
                        type="button"
                        onClick={() => setInversionARenovar(i)}
                        className="flex items-center gap-1 text-xs font-medium text-gold-400 hover:underline"
                      >
                        <RefreshCw size={13} /> Renovar
                      </button>
                      <button
                        type="button"
                        disabled={cancelar.isPending}
                        onClick={() => cancelar.mutate(i.id)}
                        className="text-xs font-medium text-petrol-700 hover:underline disabled:opacity-50"
                      >
                        Cancelar
                      </button>
                    </>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
        </>
      )}
    </div>
  )
}
