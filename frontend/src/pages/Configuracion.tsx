import { Fragment, useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Settings, Plus, X, Pencil, ShieldCheck, Building2, Calculator, PiggyBank, Landmark, Users, FileCheck } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'
import {
  TabTiposCuenta,
  TabTiposPrestamo,
  TabTasasTechoBce,
  TabTableroTasasDpf,
  TabCategoriasRiesgoCartera,
  TabGruposContables,
  TabTiposConvenio,
} from './ConfiguracionProductos'
import {
  TabEstadosCiviles,
  TabEducacion,
  TabVivienda,
  TabSectorVivienda,
  TabNacionalidades,
  TabCausasVinculacion,
  TabCalificacionesInternas,
  TabSectoresEconomicos,
} from './ConfiguracionSocios'

type ApiError = { response?: { data?: { detail?: string } } }

function mensajeError(error: unknown, fallback: string) {
  return (error as ApiError)?.response?.data?.detail ?? fallback
}

// ---------- Catálogo simple: código + nombre (Países, Tipos de identificación) ----------

interface CatalogoCodigoNombre {
  id: number
  codigo: string
  nombre: string
}

function TabCatalogoCodigoNombre({
  titulo,
  queryKey,
  endpoint,
  labelEntidad,
}: {
  titulo: string
  queryKey: string
  endpoint: string
  labelEntidad: string
}) {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<CatalogoCodigoNombre | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')

  const { data, isLoading } = useQuery<CatalogoCodigoNombre[]>({
    queryKey: [queryKey],
    queryFn: async () => (await api.get(endpoint)).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? (await api.put(`${endpoint}/${editando.id}`, { codigo, nombre })).data
        : (await api.post(endpoint, { codigo, nombre })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [queryKey] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setMostrarForm(true)
  }

  const abrirEditar = (item: CatalogoCodigoNombre) => {
    setEditando(item)
    setCodigo(item.codigo)
    setNombre(item.nombre)
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
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!codigo || !nombre) return
              guardar.mutate()
            }}
          >
            <input
              required
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-1"
            />
            <div className="flex items-center gap-2">
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
              <p className="sm:col-span-3 text-sm text-red-700">
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
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
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

// ---------- Monedas ----------

interface Moneda {
  id: number
  codigo: string
  nombre: string
  simbolo: string
}

function TabMonedas() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<Moneda | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [simbolo, setSimbolo] = useState('')

  const { data, isLoading } = useQuery<Moneda[]>({
    queryKey: ['config-monedas'],
    queryFn: async () => (await api.get('/api/configuracion/monedas')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? (await api.put(`/api/configuracion/monedas/${editando.id}`, { codigo, nombre, simbolo })).data
        : (await api.post('/api/configuracion/monedas', { codigo, nombre, simbolo })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-monedas'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setSimbolo('')
    setMostrarForm(true)
  }

  const abrirEditar = (item: Moneda) => {
    setEditando(item)
    setCodigo(item.codigo)
    setNombre(item.nombre)
    setSimbolo(item.simbolo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Monedas</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nueva
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-4"
            onSubmit={(e) => {
              e.preventDefault()
              if (!codigo || !nombre || !simbolo) return
              guardar.mutate()
            }}
          >
            <input
              required
              placeholder="Código (USD)"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              required
              placeholder="Símbolo ($)"
              value={simbolo}
              onChange={(e) => setSimbolo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <div className="flex items-center gap-2">
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
              <p className="sm:col-span-4 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar la moneda.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Símbolo</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
              <Td>{item.simbolo}</Td>
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

// ---------- Formas de cancelación (CxP) ----------
// Catálogo real de 6 códigos (CONTABILIDAD.FORMA_CANCELACION) — las
// banderas de clasificación (efectivo/cheque/transferencia/causal/cuenta)
// son la naturaleza real de cada forma, no se editan; solo nombre y
// estado activo (mismo criterio que categorías fijas de riesgo de
// cartera). Sin alta/baja: los 6 códigos son fijos.

interface FormaCancelacion {
  codigo: string
  nombre: string
  esEfectivo: boolean
  esCheque: boolean
  esTransferencia: boolean
  esCausal: boolean
  esCuenta: boolean
  activo: boolean
}

function TabFormasCancelacion() {
  const queryClient = useQueryClient()
  const [editando, setEditando] = useState<FormaCancelacion | null>(null)
  const [nombre, setNombre] = useState('')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<FormaCancelacion[]>({
    queryKey: ['config-formas-cancelacion'],
    queryFn: async () => (await api.get('/api/configuracion/formas-cancelacion')).data,
  })

  const guardar = useMutation({
    mutationFn: async () => api.put(`/api/configuracion/formas-cancelacion/${editando!.codigo}`, { nombre, activo }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-formas-cancelacion'] })
      cerrar()
    },
  })

  const abrirEditar = (item: FormaCancelacion) => {
    setEditando(item)
    setNombre(item.nombre)
    setActivo(item.activo)
  }

  const cerrar = () => setEditando(null)

  const clasificacion = (f: FormaCancelacion) =>
    [
      f.esEfectivo && 'Efectivo',
      f.esCheque && 'Cheque',
      f.esTransferencia && 'Transferencia',
      f.esCausal && 'Causal',
      f.esCuenta && 'Acreditación a cuenta',
    ]
      .filter(Boolean)
      .join(', ') || '—'

  return (
    <div>
      <div className="mb-4">
        <h3 className="text-sm font-medium text-graphite-600">Formas de cancelación</h3>
        <p className="mt-1 text-xs text-graphite-500">
          Catálogo real usado al pagar una cuenta por pagar (Tesorería). Los 6 códigos y su clasificación real son
          fijos — solo se puede editar el nombre y activar/desactivar.
        </p>
      </div>

      {editando && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-4 sm:items-center"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre) return
              guardar.mutate()
            }}
          >
            <input
              disabled
              value={editando.codigo}
              className="rounded-lg border border-black/[0.08] bg-black/[0.03] px-3 py-2 text-sm text-graphite-500"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-1"
            />
            <label className="flex items-center gap-1.5 text-sm text-graphite-600">
              <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} /> Activo
            </label>
            <div className="flex items-center gap-2">
              <button
                type="submit"
                disabled={guardar.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {guardar.isPending ? 'Guardando…' : 'Actualizar'}
              </button>
              <button type="button" onClick={cerrar} className="text-graphite-600 hover:text-graphite-100">
                <X size={18} />
              </button>
            </div>
            {guardar.isError && (
              <p className="sm:col-span-4 text-sm text-red-700">
                {mensajeError(guardar.error, 'No se pudo actualizar la forma de cancelación.')}
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
            <Th>Clasificación real</Th>
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
              <Td className="text-graphite-600">{clasificacion(item)}</Td>
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

// ---------- Agencias ----------

interface Agencia {
  id: number
  idEmpresa: number
  codigo: string
  nombre: string
  esOperativa: boolean
  activa: boolean
}

function TabAgencias() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<Agencia | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [esOperativa, setEsOperativa] = useState(true)
  const [activa, setActiva] = useState(true)

  const { data, isLoading } = useQuery<Agencia[]>({
    queryKey: ['config-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? (await api.put(`/api/configuracion/agencias/${editando.id}`, { nombre, esOperativa, activa })).data
        : (await api.post('/api/configuracion/agencias', { idEmpresa: 1, codigo, nombre, esOperativa })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-agencias'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setEsOperativa(true)
    setActiva(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: Agencia) => {
    setEditando(item)
    setCodigo(item.codigo)
    setNombre(item.nombre)
    setEsOperativa(item.esOperativa)
    setActiva(item.activa)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Agencias</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nueva
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-4"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre || (!editando && !codigo)) return
              guardar.mutate()
            }}
          >
            <input
              required
              disabled={!!editando}
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-60"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={esOperativa} onChange={(e) => setEsOperativa(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
              <span className="text-graphite-600">Operativa</span>
            </label>
            {editando && (
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={activa} onChange={(e) => setActiva(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                <span className="text-graphite-600">Activa</span>
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
              <p className="sm:col-span-4 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar la agencia.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Operativa</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
              <Td>
                <Badge variant={item.esOperativa ? 'exito' : 'neutral'}>{item.esOperativa ? 'Sí' : 'No'}</Badge>
              </Td>
              <Td>
                <Badge variant={item.activa ? 'exito' : 'neutral'}>{item.activa ? 'Activa' : 'Inactiva'}</Badge>
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

// ---------- Roles ----------

interface Rol {
  id: number
  nombre: string
  nivel: number
  diasCambioClave: number
  permiteConsolidadoCliente: boolean
}

interface MenuAsignado {
  idMenu: number
  codigo: string
  nombre: string
  asignado: boolean
}

interface TipoEstructuraAsignado {
  codigo: string
  nombre: string
  asignado: boolean
}

function PermisosDelRol({ rol, onClose }: { rol: Rol; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [seleccion, setSeleccion] = useState<Set<number> | null>(null)

  const { data: menus, isLoading } = useQuery<MenuAsignado[]>({
    queryKey: ['config-rol-menus', rol.id],
    queryFn: async () => (await api.get(`/api/configuracion/roles/${rol.id}/menus`)).data,
  })

  useEffect(() => {
    if (menus && seleccion === null) {
      setSeleccion(new Set(menus.filter((m) => m.asignado).map((m) => m.idMenu)))
    }
  }, [menus, seleccion])

  const guardar = useMutation({
    mutationFn: async () =>
      api.put(`/api/configuracion/roles/${rol.id}/menus`, { idsMenu: Array.from(seleccion ?? []) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-rol-menus', rol.id] })
    },
  })

  const alternar = (idMenu: number) => {
    setSeleccion((prev) => {
      const siguiente = new Set(prev ?? [])
      if (siguiente.has(idMenu)) {
        siguiente.delete(idMenu)
      } else {
        siguiente.add(idMenu)
      }
      return siguiente
    })
  }

  return (
    <tr>
      <Td colSpan={5}>
        <div className="glass-card animate-zoom-in rounded-xl p-4">
          <div className="mb-3 flex items-center justify-between">
            <h4 className="flex items-center gap-1.5 text-sm font-medium text-graphite-100">
              <ShieldCheck size={15} /> Módulos permitidos para {rol.nombre}
            </h4>
            <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
              <X size={16} />
            </button>
          </div>

          {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}

          {menus && seleccion && (
            <>
              <div className="mb-3 flex flex-wrap gap-2">
                {menus.map((m) => (
                  <button
                    key={m.idMenu}
                    type="button"
                    onClick={() => alternar(m.idMenu)}
                    className={`rounded-lg border px-3 py-1.5 text-sm font-medium transition-colors ${
                      seleccion.has(m.idMenu)
                        ? 'border-gold-500/50 bg-gold-500/10 text-gold-300'
                        : 'border-black/[0.08] text-graphite-600 hover:bg-black/[0.02]'
                    }`}
                  >
                    {m.nombre}
                  </button>
                ))}
              </div>

              <div className="flex items-center gap-3">
                <button
                  type="button"
                  onClick={() => guardar.mutate()}
                  disabled={guardar.isPending}
                  className="btn-hover rounded-lg bg-gold-500 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-60"
                >
                  {guardar.isPending ? 'Guardando…' : 'Guardar permisos'}
                </button>
                {guardar.isSuccess && <span className="text-xs text-petrol-700">Guardado.</span>}
                {guardar.isError && <span className="text-xs text-red-700">No se pudo guardar.</span>}
              </div>

              <p className="mt-3 text-xs text-graphite-600">
                Un cambio acá no afecta sesiones ya iniciadas — el usuario debe volver a iniciar sesión para que el
                nuevo permiso (o la quita) se refleje, porque los módulos permitidos se calculan una sola vez al
                login.
              </p>
            </>
          )}
        </div>
      </Td>
    </tr>
  )
}

// Segundo nivel de permiso, dentro del módulo "Estructuras y Procesos
// Financieros" — qué estructuras puntuales (OF01, y las que se sumen)
// puede generar este rol. Mismo patrón exacto que PermisosDelRol (arriba),
// solo que contra /roles/{id}/tipos-estructura en vez de /menus — es lo
// que permite que un rol tenga el módulo pero solo cierta(s) estructura(s)
// habilitada(s), el requisito real que motivó este segundo nivel.
function EstructurasDelRol({ rol, onClose }: { rol: Rol; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [seleccion, setSeleccion] = useState<Set<string> | null>(null)

  const { data: estructuras, isLoading } = useQuery<TipoEstructuraAsignado[]>({
    queryKey: ['config-rol-estructuras', rol.id],
    queryFn: async () => (await api.get(`/api/configuracion/roles/${rol.id}/tipos-estructura`)).data,
  })

  useEffect(() => {
    if (estructuras && seleccion === null) {
      setSeleccion(new Set(estructuras.filter((e) => e.asignado).map((e) => e.codigo)))
    }
  }, [estructuras, seleccion])

  const guardar = useMutation({
    mutationFn: async () =>
      api.put(`/api/configuracion/roles/${rol.id}/tipos-estructura`, {
        codigosTipoEstructura: Array.from(seleccion ?? []),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-rol-estructuras', rol.id] })
    },
  })

  const alternar = (codigo: string) => {
    setSeleccion((prev) => {
      const siguiente = new Set(prev ?? [])
      if (siguiente.has(codigo)) {
        siguiente.delete(codigo)
      } else {
        siguiente.add(codigo)
      }
      return siguiente
    })
  }

  return (
    <tr>
      <Td colSpan={5}>
        <div className="glass-card animate-zoom-in rounded-xl p-4">
          <div className="mb-3 flex items-center justify-between">
            <h4 className="flex items-center gap-1.5 text-sm font-medium text-graphite-100">
              <FileCheck size={15} /> Estructuras habilitadas para {rol.nombre}
            </h4>
            <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
              <X size={16} />
            </button>
          </div>

          <p className="mb-3 text-xs text-graphite-600">
            Dentro del módulo "Estructuras y Procesos Financieros", cuáles puede generar este rol — un rol puede
            tener el módulo y aun así no tener ninguna estructura habilitada.
          </p>

          {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}
          {!isLoading && (estructuras?.length ?? 0) === 0 && (
            <p className="text-sm text-graphite-600">Sin tipos de estructura registrados todavía.</p>
          )}

          {estructuras && seleccion && (
            <>
              <div className="mb-3 flex flex-wrap gap-2">
                {estructuras.map((e) => (
                  <button
                    key={e.codigo}
                    type="button"
                    onClick={() => alternar(e.codigo)}
                    className={`rounded-lg border px-3 py-1.5 text-sm font-medium transition-colors ${
                      seleccion.has(e.codigo)
                        ? 'border-gold-500/50 bg-gold-500/10 text-gold-300'
                        : 'border-black/[0.08] text-graphite-600 hover:bg-black/[0.02]'
                    }`}
                  >
                    {e.nombre}
                  </button>
                ))}
              </div>

              <div className="flex items-center gap-3">
                <button
                  type="button"
                  onClick={() => guardar.mutate()}
                  disabled={guardar.isPending}
                  className="btn-hover rounded-lg bg-gold-500 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-60"
                >
                  {guardar.isPending ? 'Guardando…' : 'Guardar estructuras'}
                </button>
                {guardar.isSuccess && <span className="text-xs text-petrol-700">Guardado.</span>}
                {guardar.isError && <span className="text-xs text-red-700">No se pudo guardar.</span>}
              </div>

              <p className="mt-3 text-xs text-graphite-600">
                Mismo criterio que los módulos: un cambio acá no afecta sesiones ya iniciadas, se aplica en el
                próximo login.
              </p>
            </>
          )}
        </div>
      </Td>
    </tr>
  )
}

function TabRoles() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<Rol | null>(null)
  const [nombre, setNombre] = useState('')
  const [nivel, setNivel] = useState('10')
  const [diasCambioClave, setDiasCambioClave] = useState('30')
  const [permiteConsolidadoCliente, setPermiteConsolidadoCliente] = useState(false)
  const [rolPermisos, setRolPermisos] = useState<Rol | null>(null)
  const [rolEstructuras, setRolEstructuras] = useState<Rol | null>(null)

  const { data, isLoading } = useQuery<Rol[]>({
    queryKey: ['config-roles'],
    queryFn: async () => (await api.get('/api/configuracion/roles')).data,
  })

  const guardar = useMutation({
    mutationFn: async () => {
      const body = { nombre, nivel: Number(nivel), diasCambioClave: Number(diasCambioClave), permiteConsolidadoCliente }
      const esCreacion = !editando
      const rol = esCreacion
        ? (await api.post('/api/configuracion/roles', body)).data
        : (await api.put(`/api/configuracion/roles/${editando.id}`, body)).data
      return { rol: rol as Rol, esCreacion }
    },
    onSuccess: ({ rol, esCreacion }) => {
      queryClient.invalidateQueries({ queryKey: ['config-roles'] })
      cerrar()
      // Un rol recién creado no tiene ningún módulo asignado todavía —
      // sin esto, sería invisible para cualquier usuario hasta que alguien
      // se acuerde de volver acá y abrir "Permisos" a mano.
      if (esCreacion) setRolPermisos(rol)
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setNombre('')
    setNivel('10')
    setDiasCambioClave('30')
    setPermiteConsolidadoCliente(false)
    setMostrarForm(true)
  }

  const abrirEditar = (item: Rol) => {
    setEditando(item)
    setNombre(item.nombre)
    setNivel(String(item.nivel))
    setDiasCambioClave(String(item.diasCambioClave))
    setPermiteConsolidadoCliente(item.permiteConsolidadoCliente)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Roles</h3>
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
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre) return
              guardar.mutate()
            }}
          >
            <input
              required
              placeholder="Nombre del rol"
              value={nombre}
              onChange={(e) => setNombre(e.target.value.toUpperCase())}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              required
              type="number"
              placeholder="Nivel"
              value={nivel}
              onChange={(e) => setNivel(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Días de vigencia de la clave</span>
              <input
                required
                type="number"
                min="1"
                value={diasCambioClave}
                onChange={(e) => setDiasCambioClave(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input
                type="checkbox"
                checked={permiteConsolidadoCliente}
                onChange={(e) => setPermiteConsolidadoCliente(e.target.checked)}
              />
              Ve el consolidado de cliente (multi-agencia)
            </label>
            <div className="flex items-center gap-2">
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
              <p className="sm:col-span-3 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar el rol.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Nombre</Th>
            <Th>Nivel</Th>
            <Th>Días clave</Th>
            <Th>Consolidado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <Fragment key={item.id}>
              <tr className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{item.nombre}</Td>
                <Td>{item.nivel}</Td>
                <Td>{item.diasCambioClave}</Td>
                <Td>{item.permiteConsolidadoCliente ? 'Sí' : 'No'}</Td>
                <Td>
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => abrirEditar(item)}
                      className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                    >
                      <Pencil size={13} /> Editar
                    </button>
                    <button
                      type="button"
                      onClick={() => setRolPermisos(rolPermisos?.id === item.id ? null : item)}
                      className="flex items-center gap-1 text-xs font-medium text-gold-400 hover:underline"
                    >
                      <ShieldCheck size={13} /> Permisos
                    </button>
                    <button
                      type="button"
                      onClick={() => setRolEstructuras(rolEstructuras?.id === item.id ? null : item)}
                      className="flex items-center gap-1 text-xs font-medium text-gold-400 hover:underline"
                    >
                      <FileCheck size={13} /> Estructuras
                    </button>
                  </div>
                </Td>
              </tr>
              {rolPermisos?.id === item.id && (
                <PermisosDelRol rol={item} onClose={() => setRolPermisos(null)} />
              )}
              {rolEstructuras?.id === item.id && (
                <EstructurasDelRol rol={item} onClose={() => setRolEstructuras(null)} />
              )}
            </Fragment>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

// ---------- Empresa (fila única) ----------

interface Empresa {
  id: number
  codigo: string
  nombre: string
  ruc: string
  idMoneda: number
  idPais: number
}

function TabEmpresa() {
  const queryClient = useQueryClient()
  const { data, isLoading } = useQuery<Empresa>({
    queryKey: ['config-empresa'],
    queryFn: async () => (await api.get('/api/configuracion/empresa')).data,
  })
  const { data: monedas } = useQuery<Moneda[]>({
    queryKey: ['config-monedas'],
    queryFn: async () => (await api.get('/api/configuracion/monedas')).data,
  })
  const { data: paises } = useQuery<CatalogoCodigoNombre[]>({
    queryKey: ['config-paises'],
    queryFn: async () => (await api.get('/api/configuracion/paises')).data,
  })

  const [nombre, setNombre] = useState('')
  const [ruc, setRuc] = useState('')
  const [idMoneda, setIdMoneda] = useState('')
  const [idPais, setIdPais] = useState('')

  useEffect(() => {
    if (data) {
      setNombre(data.nombre)
      setRuc(data.ruc)
      setIdMoneda(String(data.idMoneda))
      setIdPais(String(data.idPais))
    }
  }, [data])

  const guardar = useMutation({
    mutationFn: async () =>
      (
        await api.put('/api/configuracion/empresa', {
          nombre,
          ruc,
          idMoneda: Number(idMoneda),
          idPais: Number(idPais),
        })
      ).data,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['config-empresa'] }),
  })

  if (isLoading || !data) return <p className="text-sm text-graphite-600">Cargando…</p>

  return (
    <div>
      <h3 className="mb-4 text-sm font-medium text-graphite-600">Datos de la cooperativa</h3>
      <div className="glass-card max-w-2xl rounded-xl p-5">
        <form
          className="grid grid-cols-1 gap-4 sm:grid-cols-2"
          onSubmit={(e) => {
            e.preventDefault()
            guardar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            <span className="text-graphite-600">Razón social</span>
            <input
              required
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">RUC</span>
            <input
              required
              value={ruc}
              onChange={(e) => setRuc(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Moneda</span>
            <select
              value={idMoneda}
              onChange={(e) => setIdMoneda(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              {monedas?.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.codigo} — {m.nombre}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">País</span>
            <select
              value={idPais}
              onChange={(e) => setIdPais(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              {paises?.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.codigo} — {p.nombre}
                </option>
              ))}
            </select>
          </label>

          <div className="sm:col-span-2">
            <button
              type="submit"
              disabled={guardar.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {guardar.isPending ? 'Guardando…' : 'Guardar cambios'}
            </button>
          </div>

          {guardar.isError && (
            <p className="sm:col-span-2 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar la empresa.')}</p>
          )}
          {guardar.isSuccess && <p className="sm:col-span-2 text-sm text-petrol-700">Guardado.</p>}
        </form>
      </div>
    </div>
  )
}

// ---------- Plan de cuentas (Nivel 1) ----------

interface CuentaContablePlan {
  id: string
  codigo: string
  nombre: string
  grupo: string
  naturaleza: string
  esMayor: boolean
  activa: boolean
  codigoPadre: string | null
}

function TabPlanCuentas() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<CuentaContablePlan | null>(null)
  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [grupo, setGrupo] = useState('Activo')
  const [naturaleza, setNaturaleza] = useState('Deudora')
  const [idCuentaPadre, setIdCuentaPadre] = useState('')
  const [esMayor, setEsMayor] = useState(true)
  const [activa, setActiva] = useState(true)

  const { data, isLoading } = useQuery<CuentaContablePlan[]>({
    queryKey: ['config-plan-cuentas'],
    queryFn: async () => (await api.get('/api/configuracion/plan-cuentas')).data,
  })

  const grupos = data?.filter((c) => !c.esMayor) ?? []

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? (await api.put(`/api/configuracion/plan-cuentas/${editando.id}`, { nombre, activa })).data
        : (
            await api.post('/api/configuracion/plan-cuentas', {
              codigo,
              nombre,
              grupo,
              naturaleza,
              idCuentaPadre: idCuentaPadre || null,
              esMayor,
            })
          ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-plan-cuentas'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setCodigo('')
    setNombre('')
    setGrupo('Activo')
    setNaturaleza('Deudora')
    setIdCuentaPadre('')
    setEsMayor(true)
    setActiva(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: CuentaContablePlan) => {
    setEditando(item)
    setNombre(item.nombre)
    setActiva(item.activa)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Plan de cuentas</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={abrirNuevo}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nueva subcuenta
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card animate-zoom-in mb-4 rounded-xl p-4">
          <form
            className="grid grid-cols-1 gap-3 sm:grid-cols-3"
            onSubmit={(e) => {
              e.preventDefault()
              if (!nombre || (!editando && !codigo)) return
              guardar.mutate()
            }}
          >
            <input
              required
              disabled={!!editando}
              placeholder="Código"
              value={codigo}
              onChange={(e) => setCodigo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-60"
            />
            <input
              required
              placeholder="Nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 sm:col-span-2"
            />

            {!editando && (
              <>
                <select
                  value={grupo}
                  onChange={(e) => setGrupo(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
                >
                  {['Activo', 'Pasivo', 'Patrimonio', 'Gastos', 'Ingresos', 'CuentasContingentes', 'CuentasDeOrden'].map((g) => (
                    <option key={g} value={g}>
                      {g}
                    </option>
                  ))}
                </select>
                <select
                  value={naturaleza}
                  onChange={(e) => setNaturaleza(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
                >
                  <option value="Deudora">Deudora</option>
                  <option value="Acreedora">Acreedora</option>
                </select>
                <select
                  value={idCuentaPadre}
                  onChange={(e) => setIdCuentaPadre(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
                >
                  <option value="">Sin padre (grupo raíz)</option>
                  {grupos.map((g) => (
                    <option key={g.id} value={g.id}>
                      {g.codigo} — {g.nombre}
                    </option>
                  ))}
                </select>
                <label className="flex items-center gap-2 text-sm">
                  <input type="checkbox" checked={esMayor} onChange={(e) => setEsMayor(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                  <span className="text-graphite-600">Cuenta de detalle (recibe movimientos)</span>
                </label>
              </>
            )}

            {editando && (
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={activa} onChange={(e) => setActiva(e.target.checked)} className="h-4 w-4 rounded accent-[#b58e4a]" />
                <span className="text-graphite-600">Activa</span>
              </label>
            )}

            <div className="flex items-center gap-2 sm:col-span-3">
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
              <p className="sm:col-span-3 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar la cuenta.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Código</Th>
            <Th>Nombre</Th>
            <Th>Grupo</Th>
            <Th>Naturaleza</Th>
            <Th>Tipo</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.codigo}</Td>
              <Td>{item.nombre}</Td>
              <Td>{item.grupo}</Td>
              <Td>{item.naturaleza}</Td>
              <Td>
                <Badge variant={item.esMayor ? 'exito' : 'neutral'}>{item.esMayor ? 'Detalle' : 'Grupo'}</Badge>
              </Td>
              <Td>
                <Badge variant={item.activa ? 'exito' : 'neutral'}>{item.activa ? 'Activa' : 'Inactiva'}</Badge>
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

// ---------- Cuentas por producto (override contable de TipoTransaccion) ----------

interface TipoTransaccionOverride {
  id: number
  idTipoTransaccion: number
  tipoTransaccion: string
  idTipoCuenta: number
  tipoCuenta: string
  codigoCuentaDebito: string
  codigoCuentaCredito: string
  activo: boolean
}

interface TipoTransaccionOpcion {
  id: number
  codigo: string
  nombre: string
}

interface TipoCuentaOpcion {
  id: number
  codigo: string
  nombre: string
}

function TabCuentasPorProducto() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<TipoTransaccionOverride | null>(null)
  const [idTipoTransaccion, setIdTipoTransaccion] = useState<number | ''>('')
  const [idTipoCuenta, setIdTipoCuenta] = useState<number | ''>('')
  const [codigoCuentaDebito, setCodigoCuentaDebito] = useState('')
  const [codigoCuentaCredito, setCodigoCuentaCredito] = useState('')
  const [activo, setActivo] = useState(true)

  const { data, isLoading } = useQuery<TipoTransaccionOverride[]>({
    queryKey: ['config-tipo-transaccion-cuenta-producto'],
    queryFn: async () => (await api.get('/api/configuracion/tipo-transaccion-cuenta-producto')).data,
  })
  const { data: tiposTransaccion } = useQuery<TipoTransaccionOpcion[]>({
    queryKey: ['ahorros-tipos-transaccion'],
    queryFn: async () => (await api.get('/api/ahorros/tipos-transaccion')).data,
  })
  const { data: tiposCuenta } = useQuery<TipoCuentaOpcion[]>({
    queryKey: ['config-tipos-cuenta-opciones'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-cuenta')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? api.put(`/api/configuracion/tipo-transaccion-cuenta-producto/${editando.id}`, {
            codigoCuentaDebito, codigoCuentaCredito, activo,
          })
        : api.post('/api/configuracion/tipo-transaccion-cuenta-producto', {
            idTipoTransaccion, idTipoCuenta, codigoCuentaDebito, codigoCuentaCredito,
          }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-tipo-transaccion-cuenta-producto'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setIdTipoTransaccion('')
    setIdTipoCuenta('')
    setCodigoCuentaDebito('')
    setCodigoCuentaCredito('')
    setActivo(true)
    setMostrarForm(true)
  }

  const abrirEditar = (item: TipoTransaccionOverride) => {
    setEditando(item)
    setIdTipoTransaccion(item.idTipoTransaccion)
    setIdTipoCuenta(item.idTipoCuenta)
    setCodigoCuentaDebito(item.codigoCuentaDebito)
    setCodigoCuentaCredito(item.codigoCuentaCredito)
    setActivo(item.activo)
    setMostrarForm(true)
  }

  const cerrar = () => {
    setMostrarForm(false)
    setEditando(null)
  }

  return (
    <div>
      <p className="mb-4 text-xs text-graphite-600">
        Excepción real por producto al motor de <code>TipoTransaccion</code> (Depósito/Retiro en efectivo, etc.) —
        si un producto de ahorro necesita afectar cuentas contables distintas de las cuentas por defecto de la
        transacción (ej. Certificados de Aportación acredita <code>3103</code> Aportes de socios, no{' '}
        <code>2101</code> Depósitos), se configura acá en vez de tocar código.
      </p>

      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Overrides configurados</h3>
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
              if (!codigoCuentaDebito || !codigoCuentaCredito || (!editando && (idTipoTransaccion === '' || idTipoCuenta === ''))) return
              guardar.mutate()
            }}
          >
            <select
              required
              disabled={!!editando}
              value={idTipoTransaccion}
              onChange={(e) => setIdTipoTransaccion(e.target.value ? Number(e.target.value) : '')}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-50"
            >
              <option value="">Tipo de transacción…</option>
              {tiposTransaccion?.map((t) => (
                <option key={t.id} value={t.id}>{t.nombre}</option>
              ))}
            </select>
            <select
              required
              disabled={!!editando}
              value={idTipoCuenta}
              onChange={(e) => setIdTipoCuenta(e.target.value ? Number(e.target.value) : '')}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-50"
            >
              <option value="">Producto…</option>
              {tiposCuenta?.map((t) => (
                <option key={t.id} value={t.id}>{t.nombre}</option>
              ))}
            </select>
            <input
              required
              placeholder="Código cuenta débito"
              value={codigoCuentaDebito}
              onChange={(e) => setCodigoCuentaDebito(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <input
              required
              placeholder="Código cuenta crédito"
              value={codigoCuentaCredito}
              onChange={(e) => setCodigoCuentaCredito(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
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
              <p className="sm:col-span-4 text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar el override.')}</p>
            )}
          </form>
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Transacción</Th>
            <Th>Producto</Th>
            <Th>Débito</Th>
            <Th>Crédito</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin overrides configurados — todos los productos usan las cuentas por defecto.</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.tipoTransaccion}</Td>
              <Td>{item.tipoCuenta}</Td>
              <Td className="tabular-nums">{item.codigoCuentaDebito}</Td>
              <Td className="tabular-nums">{item.codigoCuentaCredito}</Td>
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

// ---------- Página principal ----------

// Agrupado por módulo — con 21 catálogos en una sola fila de pestañas se
// volvía tedioso encontrar algo (feedback directo del usuario). Dos
// niveles: primero el grupo (módulo de negocio), luego las pestañas de
// ese grupo — mismo patrón mental que el sidebar principal, aplicado
// adentro de Configuración.
const GRUPOS = [
  {
    id: 'general',
    label: 'General',
    icon: Building2,
    tabs: [
      { id: 'empresa', label: 'Empresa' },
      { id: 'agencias', label: 'Agencias' },
      { id: 'roles', label: 'Roles' },
      { id: 'paises', label: 'Países' },
      { id: 'monedas', label: 'Monedas' },
      { id: 'tipos-identificacion', label: 'Tipos de identificación' },
    ],
  },
  {
    id: 'contabilidad',
    label: 'Contabilidad',
    icon: Calculator,
    tabs: [
      { id: 'plan-cuentas', label: 'Plan de cuentas' },
      { id: 'tipos-comprobante', label: 'Tipos de comprobante' },
      { id: 'cuentas-por-producto', label: 'Cuentas por producto' },
      { id: 'formas-cancelacion', label: 'Formas de cancelación' },
    ],
  },
  {
    id: 'ahorros',
    label: 'Ahorros',
    icon: PiggyBank,
    tabs: [{ id: 'tipos-cuenta', label: 'Productos de ahorro' }],
  },
  {
    id: 'creditos',
    label: 'Créditos y Plazo Fijo',
    icon: Landmark,
    tabs: [
      { id: 'tipos-prestamo', label: 'Productos de crédito' },
      { id: 'tasas-techo-bce', label: 'Tasas techo BCE' },
      { id: 'tablero-dpf', label: 'Tasas DPF' },
      { id: 'categorias-riesgo', label: 'Riesgo de cartera' },
      { id: 'grupos-contables', label: 'Comité y grupos de aprobadores' },
      { id: 'tipos-convenio', label: 'Convenios' },
    ],
  },
  {
    id: 'socios',
    label: 'Socios',
    icon: Users,
    tabs: [
      { id: 'estados-civiles', label: 'Estados civiles' },
      { id: 'educacion', label: 'Educación' },
      { id: 'vivienda', label: 'Vivienda' },
      { id: 'sector-vivienda', label: 'Sector de vivienda' },
      { id: 'nacionalidades', label: 'Nacionalidades' },
      { id: 'causas-vinculacion', label: 'Causas de vinculación' },
      { id: 'calificaciones-internas', label: 'Calificaciones internas' },
      { id: 'sectores-economicos', label: 'Sectores económicos' },
    ],
  },
] as const

type GrupoId = (typeof GRUPOS)[number]['id']
type TabId = (typeof GRUPOS)[number]['tabs'][number]['id']

export function Configuracion() {
  const [grupo, setGrupo] = useState<GrupoId>('general')
  const [tab, setTab] = useState<TabId>('empresa')

  const grupoActivo = GRUPOS.find((g) => g.id === grupo)!

  const seleccionarGrupo = (id: GrupoId) => {
    setGrupo(id)
    setTab(GRUPOS.find((g) => g.id === id)!.tabs[0].id)
  }

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Settings} title="Configuración" subtitle="Parámetros y catálogos generales del sistema" />

      <div className="mb-4 flex flex-wrap gap-2">
        {GRUPOS.map((g) => {
          const GIcon = g.icon
          return (
            <button
              key={g.id}
              type="button"
              onClick={() => seleccionarGrupo(g.id)}
              className={`flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-sm font-medium transition-colors ${
                grupo === g.id
                  ? 'border-gold-500/50 bg-gold-500/10 text-gold-300'
                  : 'border-black/[0.08] text-graphite-600 hover:bg-black/[0.02]'
              }`}
            >
              <GIcon size={15} /> {g.label}
            </button>
          )
        })}
      </div>

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {grupoActivo.tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`rounded-t-lg px-3 py-2 text-sm font-medium transition ${
              tab === t.id
                ? 'border-b-2 border-gold-500 text-graphite-100'
                : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'empresa' && <TabEmpresa />}
      {tab === 'agencias' && <TabAgencias />}
      {tab === 'roles' && <TabRoles />}
      {tab === 'paises' && (
        <TabCatalogoCodigoNombre
          titulo="Países"
          queryKey="config-paises"
          endpoint="/api/configuracion/paises"
          labelEntidad="el país"
        />
      )}
      {tab === 'monedas' && <TabMonedas />}
      {tab === 'tipos-identificacion' && (
        <TabCatalogoCodigoNombre
          titulo="Tipos de identificación"
          queryKey="config-tipos-identificacion"
          endpoint="/api/configuracion/tipos-identificacion"
          labelEntidad="el tipo de identificación"
        />
      )}
      {tab === 'plan-cuentas' && <TabPlanCuentas />}
      {tab === 'tipos-comprobante' && (
        <TabCatalogoCodigoNombre
          titulo="Tipos de comprobante"
          queryKey="config-tipos-comprobante"
          endpoint="/api/configuracion/tipos-comprobante"
          labelEntidad="el tipo de comprobante"
        />
      )}
      {tab === 'cuentas-por-producto' && <TabCuentasPorProducto />}
      {tab === 'formas-cancelacion' && <TabFormasCancelacion />}
      {tab === 'tipos-cuenta' && <TabTiposCuenta />}
      {tab === 'tipos-prestamo' && <TabTiposPrestamo />}
      {tab === 'tasas-techo-bce' && <TabTasasTechoBce />}
      {tab === 'tablero-dpf' && <TabTableroTasasDpf />}
      {tab === 'categorias-riesgo' && <TabCategoriasRiesgoCartera />}
      {tab === 'grupos-contables' && <TabGruposContables />}
      {tab === 'tipos-convenio' && <TabTiposConvenio />}
      {tab === 'estados-civiles' && <TabEstadosCiviles />}
      {tab === 'educacion' && <TabEducacion />}
      {tab === 'vivienda' && <TabVivienda />}
      {tab === 'sector-vivienda' && <TabSectorVivienda />}
      {tab === 'nacionalidades' && <TabNacionalidades />}
      {tab === 'causas-vinculacion' && <TabCausasVinculacion />}
      {tab === 'calificaciones-internas' && <TabCalificacionesInternas />}
      {tab === 'sectores-economicos' && <TabSectoresEconomicos />}
    </div>
  )
}
