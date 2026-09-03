import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus, X, Pencil } from 'lucide-react'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

type ApiError = { response?: { data?: { detail?: string } } }

function mensajeError(error: unknown, fallback: string) {
  return (error as ApiError)?.response?.data?.detail ?? fallback
}

interface CatalogoCodigoActivo {
  codigo: string
  nombre: string
  activo: boolean
}

// Catálogos socioeconómicos reales de Socios (verificados contra Softbank —
// ver CLAUDE.md "CRUD real de Socios y Usuarios y roles"). Mismo patrón que
// TabCatalogoCodigoNombre en Configuracion.tsx, pero la clave primaria real
// es el código (string, no un id numérico) y cada fila tiene un flag
// Activo/Activa editable — nunca se borra, solo se desactiva.
export function TabCatalogoSocioeconomico({
  titulo,
  queryKey,
  endpoint,
  labelEntidad,
  codigoMaxLength,
}: {
  titulo: string
  queryKey: string
  endpoint: string
  labelEntidad: string
  codigoMaxLength: number
}) {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<CatalogoCodigoActivo | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<CatalogoCodigoActivo[]>({
    queryKey: [queryKey],
    queryFn: async () => (await api.get(endpoint)).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? api.put(`${endpoint}/${editando.codigo}`, { nombre, activo })
        : api.post(endpoint, { codigo, nombre }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [queryKey] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: CatalogoCodigoActivo) => {
    setEditando(item)
    setCodigo(item.codigo)
    setNombre(item.nombre)
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">{titulo}</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nuevo
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-4 sm:items-center"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre || (!editando && !codigo)) return
              guardar.mutate()
            }}
          >
            <input
              required
              disabled={!!editando}
              maxLength={codigoMaxLength}
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value.toUpperCase())}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-50"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-2"
            />
            {editando && (
              <label className="flex items-center gap-1.5 text-sm text-graphite-600">
                <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} /> Activo
              </label>
            )}
            <div className="flex items-center gap-2 sm:col-span-4">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : editando ? 'Actualizar' : 'Crear'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-4 text-sm text-red-700">
                {mensajeError(guardar.error, `No se pudo guardar ${labelEntidad}.`)}
              </p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.codigo} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
              <Td>
                <Badge variant={item.activo ? 'exito' : 'neutral'}>{item.activo ? 'Activo' : 'Inactivo'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => abrirEditar(item)}
                  className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                >
                  <Pencil size={13} /> Editar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

export function TabEstadosCiviles() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Estados civiles"
      queryKey="config-estados-civiles"
      endpoint="/api/configuracion/estados-civiles"
      labelEntidad="el estado civil"
      codigoMaxLength={2}
    />
  )
}

export function TabEducacion() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Nivel de educación"
      queryKey="config-educacion"
      endpoint="/api/configuracion/educacion"
      labelEntidad="el nivel de educación"
      codigoMaxLength={2}
    />
  )
}

export function TabVivienda() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Tipo de vivienda"
      queryKey="config-vivienda"
      endpoint="/api/configuracion/vivienda"
      labelEntidad="el tipo de vivienda"
      codigoMaxLength={2}
    />
  )
}

export function TabSectorVivienda() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Sector de vivienda"
      queryKey="config-sector-vivienda"
      endpoint="/api/configuracion/sector-vivienda"
      labelEntidad="el sector de vivienda"
      codigoMaxLength={2}
    />
  )
}

export function TabNacionalidades() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Nacionalidades"
      queryKey="config-nacionalidades"
      endpoint="/api/configuracion/nacionalidades"
      labelEntidad="la nacionalidad"
      codigoMaxLength={3}
    />
  )
}

export function TabCausasVinculacion() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Causas de vinculación"
      queryKey="config-causas-vinculacion"
      endpoint="/api/configuracion/causas-vinculacion"
      labelEntidad="la causa de vinculación"
      codigoMaxLength={3}
    />
  )
}

export function TabCalificacionesInternas() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Calificaciones internas"
      queryKey="config-calificaciones-internas"
      endpoint="/api/configuracion/calificaciones-internas"
      labelEntidad="la calificación interna"
      codigoMaxLength={2}
    />
  )
}

export function TabSectoresEconomicos() {
  return (
    <TabCatalogoSocioeconomico
      titulo="Sectores económicos"
      queryKey="config-sectores-economicos"
      endpoint="/api/configuracion/sectores-economicos"
      labelEntidad="el sector económico"
      codigoMaxLength={2}
    />
  )
}
