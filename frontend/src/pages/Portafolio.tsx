import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Briefcase, Plus, X, RefreshCw } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { api } from '../lib/api'

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
        { headers: { 'Idempotency-Key': crypto.randomUUID() } },
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
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
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

export function Portafolio() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [inversionARenovar, setInversionARenovar] = useState<Inversion | null>(null)

  const { data: instituciones } = useQuery<Institucion[]>({
    queryKey: ['portafolio-instituciones'],
    queryFn: async () => (await api.get('/api/portafolio/instituciones')).data,
  })

  const { data: inversiones, isLoading } = useQuery<Inversion[]>({
    queryKey: ['portafolio-inversiones'],
    queryFn: async () => (await api.get('/api/portafolio/inversiones')).data,
  })

  const cancelar = useMutation({
    mutationFn: async (id: string) =>
      (
        await api.post(`/api/portafolio/inversiones/${id}/cancelar`, undefined, {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
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

      {mostrarForm && instituciones && <AbrirInversionForm instituciones={instituciones} onClose={() => setMostrarForm(false)} />}

      {inversionARenovar && (
        <ModalPortal>
          <RenovarInversionModal inversion={inversionARenovar} onClose={() => setInversionARenovar(null)} />
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
                <Badge variant={estadoVariant(i.estado)}>{i.estado}</Badge>
              </Td>
              <Td>
                {i.estado === 'Activa' && (
                  <div className="flex items-center gap-3">
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
                  </div>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
