import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, Plus, X } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { api } from '../lib/api'

interface Proceso {
  id: number
  nombre: string
  macroProceso: string
  critico: boolean
}

interface NivelEscala {
  id: number
  nombre: string
  nivel: number
}

interface EventoRiesgo {
  id: string
  proceso: string
  descripcion: string
  nivelImpacto: string
  nivelProbabilidad: string
  nivelRiesgo: string
  colorNivelRiesgo: string
  fechaIdentificacion: string
}

function RegistrarEventoForm({
  procesos,
  nivelesImpacto,
  nivelesProbabilidad,
  onClose,
}: {
  procesos: Proceso[]
  nivelesImpacto: NivelEscala[]
  nivelesProbabilidad: NivelEscala[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idProceso, setIdProceso] = useState('')
  const [descripcion, setDescripcion] = useState('')
  const [idNivelImpacto, setIdNivelImpacto] = useState('')
  const [idNivelProbabilidad, setIdNivelProbabilidad] = useState('')

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/riesgo/eventos', {
          idProceso: Number(idProceso),
          descripcion,
          idNivelImpacto: Number(idNivelImpacto),
          idNivelProbabilidad: Number(idNivelProbabilidad),
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['riesgo-eventos'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar evento de riesgo</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idProceso || !descripcion || !idNivelImpacto || !idNivelProbabilidad) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Proceso</span>
          <select
            required
            value={idProceso}
            onChange={(e) => setIdProceso(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {procesos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.macroProceso} — {p.nombre}
                {p.critico ? ' (crítico)' : ''}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Descripción del evento</span>
          <textarea
            required
            value={descripcion}
            onChange={(e) => setDescripcion(e.target.value)}
            rows={2}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Impacto</span>
          <select
            required
            value={idNivelImpacto}
            onChange={(e) => setIdNivelImpacto(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {nivelesImpacto.map((n) => (
              <option key={n.id} value={n.id}>
                {n.nivel} — {n.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Probabilidad</span>
          <select
            required
            value={idNivelProbabilidad}
            onChange={(e) => setIdNivelProbabilidad(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {nivelesProbabilidad.map((n) => (
              <option key={n.id} value={n.id}>
                {n.nivel} — {n.nombre}
              </option>
            ))}
          </select>
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Registrando…' : 'Registrar'}
          </button>
        </div>

        {registrar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar el evento de riesgo.'}
          </p>
        )}
      </form>
    </div>
  )
}

export function Riesgo() {
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: procesos } = useQuery<Proceso[]>({
    queryKey: ['riesgo-procesos'],
    queryFn: async () => (await api.get('/api/riesgo/procesos')).data,
  })

  const { data: nivelesImpacto } = useQuery<NivelEscala[]>({
    queryKey: ['riesgo-niveles-impacto'],
    queryFn: async () => (await api.get('/api/riesgo/niveles-impacto')).data,
  })

  const { data: nivelesProbabilidad } = useQuery<NivelEscala[]>({
    queryKey: ['riesgo-niveles-probabilidad'],
    queryFn: async () => (await api.get('/api/riesgo/niveles-probabilidad')).data,
  })

  const { data: eventos, isLoading } = useQuery<EventoRiesgo[]>({
    queryKey: ['riesgo-eventos'],
    queryFn: async () => (await api.get('/api/riesgo/eventos')).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={AlertTriangle}
        title="Riesgo"
        subtitle="Registro de eventos de riesgo (matriz impacto × probabilidad)"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Registrar evento
            </button>
          )
        }
      />

      {mostrarForm && procesos && nivelesImpacto && nivelesProbabilidad && (
        <RegistrarEventoForm
          procesos={procesos}
          nivelesImpacto={nivelesImpacto}
          nivelesProbabilidad={nivelesProbabilidad}
          onClose={() => setMostrarForm(false)}
        />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Proceso</Th>
            <Th>Descripción</Th>
            <Th>Impacto</Th>
            <Th>Probabilidad</Th>
            <Th>Nivel</Th>
            <Th>Fecha</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (eventos?.length ?? 0) === 0 && <EmptyState>Todavía no hay eventos de riesgo registrados</EmptyState>}
          {eventos?.map((ev) => (
            <tr key={ev.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{ev.proceso}</Td>
              <Td>{ev.descripcion}</Td>
              <Td>{ev.nivelImpacto}</Td>
              <Td>{ev.nivelProbabilidad}</Td>
              <Td>
                <span
                  className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium text-white"
                  style={{ backgroundColor: ev.colorNivelRiesgo }}
                >
                  {ev.nivelRiesgo}
                </span>
              </Td>
              <Td>{ev.fechaIdentificacion}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
