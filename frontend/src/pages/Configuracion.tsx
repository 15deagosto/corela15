import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Settings, Plus, X, Pencil } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

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
                <Badge variant={item.activa ? 'exito' : 'peligro'}>{item.activa ? 'Activa' : 'Inactiva'}</Badge>
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
}

function TabRoles() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<Rol | null>(null)
  const [nombre, setNombre] = useState('')
  const [nivel, setNivel] = useState('10')

  const { data, isLoading } = useQuery<Rol[]>({
    queryKey: ['config-roles'],
    queryFn: async () => (await api.get('/api/configuracion/roles')).data,
  })

  const guardar = useMutation({
    mutationFn: async () =>
      editando
        ? (await api.put(`/api/configuracion/roles/${editando.id}`, { nombre, nivel: Number(nivel) })).data
        : (await api.post('/api/configuracion/roles', { nombre, nivel: Number(nivel) })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['config-roles'] })
      cerrar()
    },
  })

  const abrirNuevo = () => {
    setEditando(null)
    setNombre('')
    setNivel('10')
    setMostrarForm(true)
  }

  const abrirEditar = (item: Rol) => {
    setEditando(item)
    setNombre(item.nombre)
    setNivel(String(item.nivel))
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
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>Sin registros</EmptyState>}
          {data?.map((item) => (
            <tr key={item.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{item.nombre}</Td>
              <Td>{item.nivel}</Td>
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

// ---------- Página principal ----------

const TABS = [
  { id: 'empresa', label: 'Empresa' },
  { id: 'agencias', label: 'Agencias' },
  { id: 'roles', label: 'Roles' },
  { id: 'paises', label: 'Países' },
  { id: 'monedas', label: 'Monedas' },
  { id: 'tipos-identificacion', label: 'Tipos de identificación' },
] as const

type TabId = (typeof TABS)[number]['id']

export function Configuracion() {
  const [tab, setTab] = useState<TabId>('empresa')

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Settings} title="Configuración" subtitle="Parámetros y catálogos generales del sistema" />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {TABS.map((t) => (
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
    </div>
  )
}
