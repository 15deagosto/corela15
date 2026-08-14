import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Briefcase, Plus, X, Trash2 } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface Empleado {
  id: string
  nombre: string
  cargo: string
}

interface RolPagosListItem {
  id: string
  periodo: string
  tipo: string
  estado: string
  cantidadEmpleados: number
  totalGeneral: number
}

interface LineaForm {
  idEmpleado: string
  ingresos: string
  egresos: string
  diasLaborados: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function lineaVacia(): LineaForm {
  return { idEmpleado: '', ingresos: '', egresos: '0', diasLaborados: '30' }
}

function GenerarRolPagosForm({ empleados, onClose }: { empleados: Empleado[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [periodo, setPeriodo] = useState('')
  const [tipo, setTipo] = useState('Mensual')
  const [lineas, setLineas] = useState<LineaForm[]>([lineaVacia()])

  const generar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/nomina/roles-pagos', {
          periodo,
          tipo,
          lineas: lineas
            .filter((l) => l.idEmpleado)
            .map((l) => ({
              idEmpleado: l.idEmpleado,
              ingresos: Number(l.ingresos || 0),
              egresos: Number(l.egresos || 0),
              diasLaborados: Number(l.diasLaborados || 0),
            })),
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-roles-pagos'] })
      onClose()
    },
  })

  const actualizarLinea = (idx: number, cambios: Partial<LineaForm>) => {
    setLineas((prev) => prev.map((l, i) => (i === idx ? { ...l, ...cambios } : l)))
  }

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Generar rol de pagos</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault()
          if (!periodo || lineas.filter((l) => l.idEmpleado).length === 0) return
          generar.mutate()
        }}
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Período</span>
            <input
              required
              type="date"
              value={periodo}
              onChange={(e) => setPeriodo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Tipo</span>
            <select
              value={tipo}
              onChange={(e) => setTipo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="Mensual">Mensual</option>
              <option value="Quincenal">Quincenal</option>
            </select>
          </label>
        </div>

        <div className="flex flex-col gap-2">
          {lineas.map((linea, idx) => (
            <div key={idx} className="grid grid-cols-1 gap-2 rounded-lg border border-black/[0.06] p-3 sm:grid-cols-5">
              <select
                value={linea.idEmpleado}
                onChange={(e) => actualizarLinea(idx, { idEmpleado: e.target.value })}
                className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-2"
              >
                <option value="">Empleado…</option>
                {empleados.map((e) => (
                  <option key={e.id} value={e.id}>
                    {e.nombre} — {e.cargo}
                  </option>
                ))}
              </select>
              <input
                type="number"
                step="0.01"
                min="0"
                placeholder="Ingresos"
                value={linea.ingresos}
                onChange={(e) => actualizarLinea(idx, { ingresos: e.target.value })}
                className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
              <input
                type="number"
                step="0.01"
                min="0"
                placeholder="Egresos"
                value={linea.egresos}
                onChange={(e) => actualizarLinea(idx, { egresos: e.target.value })}
                className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
              <div className="flex items-center gap-1">
                <input
                  type="number"
                  min="0"
                  max="31"
                  placeholder="Días"
                  value={linea.diasLaborados}
                  onChange={(e) => actualizarLinea(idx, { diasLaborados: e.target.value })}
                  className="w-full rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
                />
                {lineas.length > 1 && (
                  <button
                    type="button"
                    onClick={() => setLineas((prev) => prev.filter((_, i) => i !== idx))}
                    className="text-graphite-600 hover:text-red-700"
                  >
                    <Trash2 size={16} />
                  </button>
                )}
              </div>
            </div>
          ))}

          <button
            type="button"
            onClick={() => setLineas((prev) => [...prev, lineaVacia()])}
            className="self-start text-xs font-medium text-petrol-700 hover:underline"
          >
            + Agregar empleado
          </button>
        </div>

        <div className="flex items-center gap-2">
          <button
            type="submit"
            disabled={generar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {generar.isPending ? 'Generando…' : 'Generar rol'}
          </button>
        </div>

        {generar.isError && (
          <p className="text-sm text-red-700">
            {(generar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo generar el rol de pagos.'}
          </p>
        )}
      </form>
    </div>
  )
}

export function Nomina() {
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: empleados } = useQuery<Empleado[]>({
    queryKey: ['nomina-empleados'],
    queryFn: async () => (await api.get('/api/nomina/empleados')).data,
  })

  const { data: roles, isLoading } = useQuery<RolPagosListItem[]>({
    queryKey: ['nomina-roles-pagos'],
    queryFn: async () => (await api.get('/api/nomina/roles-pagos')).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={Briefcase}
        title="Nómina"
        subtitle="Empleados y roles de pago"
        actions={
          !mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Generar rol de pagos
            </button>
          )
        }
      />

      {mostrarForm && empleados && <GenerarRolPagosForm empleados={empleados} onClose={() => setMostrarForm(false)} />}

      <TableContainer>
        <thead>
          <tr>
            <Th>Período</Th>
            <Th>Tipo</Th>
            <Th>Empleados</Th>
            <Th>Total</Th>
            <Th>Estado</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (roles?.length ?? 0) === 0 && <EmptyState>Todavía no hay roles de pago generados</EmptyState>}
          {roles?.map((r) => (
            <tr key={r.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{r.periodo}</Td>
              <Td>{r.tipo}</Td>
              <Td>{r.cantidadEmpleados}</Td>
              <Td>{formatoUsd(r.totalGeneral)}</Td>
              <Td>
                <Badge variant="exito">{r.estado}</Badge>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
