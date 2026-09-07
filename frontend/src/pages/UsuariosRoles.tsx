import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ShieldCheck, KeyRound, UserCog, Plus, X, Clock, Building2, Settings2 } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { api } from '../lib/api'

const DIAS_SEMANA = ['Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo']

interface UsuarioDetalle {
  id: string
  nombreUsuario: string
  nombrePersona: string | null
  email: string | null
  codigoUsuarioSoftbank: string | null
  idAgencia: number
  agencia: string
  puedeIngresarSistema: boolean
  tieneBloqueo: boolean
  usaDispositivoMovil: boolean
  permiteRiesgoOperativo: boolean
  permiteConsultaEmpleados: boolean
  validaIp: boolean
  cambiaClave: boolean
  diasCambioClave: number | null
  codigoAreaPlanificacion: string | null
  areaPlanificacion: string | null
}

interface HorarioAcceso {
  id: string
  diaSemana: number
  horaInicio: string
  horaFin: string
  esReceso: boolean
  activo: boolean
}

interface Rol {
  id: number
  nombre: string
}

interface RolTemporal {
  id: string
  idRol: number
  rol: string
  fechaCaducidad: string
  vigente: boolean
}

interface AgenciaTemporal {
  id: string
  idAgenciaOrigen: number
  agenciaOrigen: string
  idAgenciaActual: number
  agenciaActual: string
  fechaCaducidad: string
  vigente: boolean
}

interface UsuarioRol {
  id: string
  nombreUsuario: string
  nombrePersona: string | null
  email: string | null
  agencia: string
  activo: boolean
  tieneBloqueo: boolean
  roles: string[]
}

interface SesionUsuario {
  id: string
  emitidaEn: string
  expiraEn: string
  direccionIp: string | null
  revocada: boolean
  revocadaEn: string | null
  revocadaPor: string | null
  vigente: boolean
}

interface Agencia {
  id: number
  nombre: string
  activa: boolean
}

interface PersonaBusqueda {
  id: string
  nombre: string
  identificacion: string
}

interface RolAsignado {
  id: number
  nombre: string
  asignado: boolean
}

function formatoFecha(iso: string) {
  return new Date(iso).toLocaleString('es-EC')
}

type ApiError = { response?: { data?: { detail?: string } } }
function mensajeError(error: unknown, fallback: string) {
  return (error as ApiError)?.response?.data?.detail ?? fallback
}

function BuscarPersonaSimple({ onSeleccionar }: { onSeleccionar: (p: PersonaBusqueda) => void }) {
  const [q, setQ] = useState('')
  const { data: personas } = useQuery<PersonaBusqueda[]>({
    queryKey: ['usuarios-buscar-persona', q],
    queryFn: async () => (await api.get('/api/socios/buscar-persona', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  return (
    <div className="flex flex-col gap-1">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Vincular con una persona (opcional)…"
        className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
      />
      {q.length >= 2 && (personas?.length ?? 0) > 0 && (
        <div className="max-h-32 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
          {personas?.map((p) => (
            <button
              key={p.id}
              type="button"
              onClick={() => {
                onSeleccionar(p)
                setQ('')
              }}
              className="block w-full px-3 py-1.5 text-left text-xs hover:bg-black/[0.02]"
            >
              {p.nombre} — {p.identificacion}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

// ---------- Alta de usuario: solo lo esencial. Todo lo demás (permisos
// finos, horarios, roles, asignaciones temporales) dependen de que el
// usuario ya exista en la base (los endpoints son /api/usuarios/{id}/...),
// así que no se pueden completar en el mismo POST de alta. En vez de
// esconder esa configuración detrás de un botón "Gestionar" que hay que
// recordar buscar después, la creación pasa DIRECTO al panel completo con
// pestañas (misma UI que "Gestionar") apenas el usuario se crea — nada se
// pierde, no hace falta un segundo clic para llegar a lo importante
// (roles, sobre todo: un usuario recién creado sin rol asignado no puede
// hacer nada todavía). ----------

function NuevoUsuarioModal({ onClose, onCreado }: { onClose: () => void; onCreado: (usuario: UsuarioRol) => void }) {
  const queryClient = useQueryClient()
  const [nombreUsuario, setNombreUsuario] = useState('')
  const [contrasenaInicial, setContrasenaInicial] = useState('')
  const [idAgencia, setIdAgencia] = useState<number | ''>('')
  const [persona, setPersona] = useState<PersonaBusqueda | null>(null)

  const { data: agencias } = useQuery<Agencia[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/usuarios', {
          nombreUsuario,
          contrasenaInicial,
          idAgencia,
          idPersona: persona?.id ?? null,
          // Valores por defecto razonables — ajustables de inmediato en el
          // panel que se abre a continuación (pestaña Seguridad).
          usaDispositivoMovil: false,
          permiteRiesgoOperativo: false,
          permiteConsultaEmpleados: false,
          validaIp: false,
          cambiaClave: true,
          diasCambioClave: null,
        })
      ).data as { id: string },
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
      onCreado({
        id: res.id,
        nombreUsuario,
        nombrePersona: persona?.nombre ?? null,
        email: null,
        agencia: agencias?.find((a) => a.id === idAgencia)?.nombre ?? '',
        activo: true,
        tieneBloqueo: false,
        roles: [],
      })
    },
  })

  const puedeGuardar = nombreUsuario && contrasenaInicial.length >= 8 && idAgencia !== ''

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card max-h-[90vh] w-full max-w-md overflow-y-auto rounded-xl p-5"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-graphite-100">Nuevo usuario</h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={16} />
          </button>
        </div>

        <div className="flex flex-col gap-3">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Nombre de usuario</span>
            <input
              value={nombreUsuario}
              onChange={(e) => setNombreUsuario(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Contraseña inicial</span>
            <input
              value={contrasenaInicial}
              onChange={(e) => setContrasenaInicial(e.target.value)}
              type="password"
              placeholder="Mínimo 8 caracteres"
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Agencia</span>
            <select
              value={idAgencia}
              onChange={(e) => setIdAgencia(e.target.value ? Number(e.target.value) : '')}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {agencias?.filter((a) => a.activa).map((a) => (
                <option key={a.id} value={a.id}>
                  {a.nombre}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Persona vinculada (opcional)</span>
            {persona ? (
              <div className="flex items-center justify-between rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm">
                <span>{persona.nombre}</span>
                <button type="button" onClick={() => setPersona(null)} className="text-xs text-graphite-600 hover:text-red-700">
                  Quitar
                </button>
              </div>
            ) : (
              <BuscarPersonaSimple onSeleccionar={setPersona} />
            )}
          </label>

          <p className="rounded-lg bg-black/[0.02] px-3 py-2 text-xs text-graphite-600">
            Al crear el usuario, seguís directo a asignarle roles, permisos, horarios de acceso y agencia —
            nada queda pendiente de buscar después.
          </p>

          {crear.isError && <p className="text-sm text-red-700">{mensajeError(crear.error, 'No se pudo crear el usuario.')}</p>}

          <button
            type="button"
            disabled={!puedeGuardar || crear.isPending}
            onClick={() => crear.mutate()}
            className="btn-hover mt-1 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {crear.isPending ? 'Creando…' : 'Crear y continuar →'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ---------- Panel "Gestionar usuario": un solo punto de entrada, con
// pestañas claras en vez de media docena de botones sueltos en la fila
// de la tabla. Cada pestaña explica en una línea qué hace antes de
// mostrar el control, para que no haya que adivinar qué significa cada
// toggle. ----------

const TABS_GESTION = [
  { id: 'datos', label: 'Datos y acceso', icon: UserCog },
  { id: 'seguridad', label: 'Seguridad', icon: Settings2 },
  { id: 'roles', label: 'Roles', icon: ShieldCheck },
  { id: 'horarios', label: 'Horarios', icon: Clock },
  { id: 'temporal', label: 'Asignaciones temporales', icon: Building2 },
  { id: 'sesiones', label: 'Sesiones', icon: KeyRound },
] as const
type TabGestionId = (typeof TABS_GESTION)[number]['id']

function TabDatosYAcceso({ usuario, onGuardado }: { usuario: UsuarioRol; onGuardado: () => void }) {
  const queryClient = useQueryClient()
  const [idAgencia, setIdAgencia] = useState<number | ''>('')
  const [puedeIngresar, setPuedeIngresar] = useState(true)
  const [cargado, setCargado] = useState(false)

  const { data: agencias } = useQuery<Agencia[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })
  const { data: detalle } = useQuery<UsuarioDetalle>({
    queryKey: ['usuario-detalle', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}`)).data,
  })
  const { data: areasPlanificacion } = useQuery<{ codigo: string; nombre: string }[]>({
    queryKey: ['planificacion-areas'],
    queryFn: async () => (await api.get('/api/planificacion/areas')).data,
  })
  const [codigoAreaPlan, setCodigoAreaPlan] = useState<string>('')
  const areaPlanCambio = useMutation({
    mutationFn: async () =>
      api.patch(`/api/usuarios/${usuario.id}/area-planificacion`, {
        codigoAreaPlanificacion: codigoAreaPlan || null,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['usuario-detalle', usuario.id] }),
  })

  useEffect(() => {
    if (!detalle || cargado) return
    setIdAgencia(detalle.idAgencia)
    setPuedeIngresar(detalle.puedeIngresarSistema)
    setCodigoAreaPlan(detalle.codigoAreaPlanificacion ?? '')
    setCargado(true)
  }, [detalle, cargado])

  const actualizar = useMutation({
    mutationFn: async () =>
      api.put(`/api/usuarios/${usuario.id}`, {
        idAgencia, idPersona: null, puedeIngresarSistema: puedeIngresar,
        usaDispositivoMovil: detalle!.usaDispositivoMovil, permiteRiesgoOperativo: detalle!.permiteRiesgoOperativo,
        permiteConsultaEmpleados: detalle!.permiteConsultaEmpleados, validaIp: detalle!.validaIp,
        cambiaClave: detalle!.cambiaClave, diasCambioClave: detalle!.diasCambioClave,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
      queryClient.invalidateQueries({ queryKey: ['usuario-detalle', usuario.id] })
      onGuardado()
    },
  })

  const [claveNueva, setClaveNueva] = useState('')
  const resetear = useMutation({
    mutationFn: async () => api.post(`/api/usuarios/${usuario.id}/resetear-clave`, { claveNueva }),
    onSuccess: () => setClaveNueva(''),
  })

  if (!detalle) return <p className="text-sm text-graphite-600">Cargando…</p>

  return (
    <div className="flex flex-col gap-5">
      {(detalle.nombrePersona || detalle.email || detalle.codigoUsuarioSoftbank) && (
        <div className="grid grid-cols-2 gap-x-4 gap-y-1 rounded-lg border border-black/[0.06] bg-black/[0.015] p-3 text-xs">
          {detalle.nombrePersona && (
            <div>
              <p className="font-medium text-graphite-600">Nombre real</p>
              <p className="text-graphite-100">{detalle.nombrePersona}</p>
            </div>
          )}
          {detalle.email && (
            <div>
              <p className="font-medium text-graphite-600">Correo</p>
              <p className="text-graphite-100">{detalle.email}</p>
            </div>
          )}
          {detalle.codigoUsuarioSoftbank && (
            <div>
              <p className="font-medium text-graphite-600">Código en Softbank</p>
              <p className="text-graphite-100" title="Trazabilidad real hacia el usuario de origen — para futuras migraciones de datos adicionales.">
                {detalle.codigoUsuarioSoftbank}
              </p>
            </div>
          )}
        </div>
      )}
      <div className="flex flex-col gap-3">
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Agencia</span>
          <select
            value={idAgencia}
            onChange={(e) => setIdAgencia(e.target.value ? Number(e.target.value) : '')}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            {agencias?.filter((a) => a.activa).map((a) => (
              <option key={a.id} value={a.id}>
                {a.nombre}
              </option>
            ))}
          </select>
        </label>
        <label className="flex items-center gap-2 text-sm text-graphite-600">
          <input type="checkbox" checked={puedeIngresar} onChange={(e) => setPuedeIngresar(e.target.checked)} />
          Puede ingresar al sistema
        </label>
        <button
          type="button"
          disabled={actualizar.isPending}
          onClick={() => actualizar.mutate()}
          className="btn-hover self-start rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        >
          {actualizar.isPending ? 'Guardando…' : 'Guardar cambios'}
        </button>
      </div>

      <div className="border-t border-black/[0.06] pt-4">
        <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-graphite-600">Área de planificación</p>
        <p className="mb-2 text-xs text-graphite-600">
          Quien tenga un área asignada acá puede ver/cargar el plan semanal de esa jefatura en el módulo
          Planificación — un jefe y su asistente comparten el mismo plan al tener la misma área.
        </p>
        <div className="flex items-center gap-2">
          <select
            value={codigoAreaPlan}
            onChange={(e) => setCodigoAreaPlan(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Sin área asignada</option>
            {areasPlanificacion?.map((a) => (
              <option key={a.codigo} value={a.codigo}>
                {a.nombre}
              </option>
            ))}
          </select>
          <button
            type="button"
            disabled={areaPlanCambio.isPending}
            onClick={() => areaPlanCambio.mutate()}
            className="btn-hover rounded-lg border border-black/[0.08] px-3 py-2 text-xs font-medium text-graphite-100 disabled:opacity-50"
          >
            {areaPlanCambio.isPending ? 'Guardando…' : 'Guardar'}
          </button>
        </div>
      </div>

      <div className="border-t border-black/[0.06] pt-4">
        <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Resetear contraseña</p>
        <p className="mb-2 text-xs text-graphite-600">Cierra todas las sesiones activas del usuario al aplicarse.</p>
        <div className="flex items-center gap-2">
          <input
            value={claveNueva}
            onChange={(e) => setClaveNueva(e.target.value)}
            type="password"
            placeholder="Nueva contraseña (mín. 8 caracteres)"
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
          <button
            type="button"
            disabled={claveNueva.length < 8 || resetear.isPending}
            onClick={() => resetear.mutate()}
            className="rounded-lg bg-graphite-800 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {resetear.isPending ? 'Reseteando…' : 'Resetear'}
          </button>
        </div>
        {resetear.isSuccess && <p className="mt-2 text-xs text-petrol-700">Listo — se cerraron las sesiones activas del usuario.</p>}
      </div>
    </div>
  )
}

function TabSeguridad({ usuario }: { usuario: UsuarioRol }) {
  const queryClient = useQueryClient()
  const [usaDispositivoMovil, setUsaDispositivoMovil] = useState(false)
  const [permiteRiesgoOperativo, setPermiteRiesgoOperativo] = useState(false)
  const [permiteConsultaEmpleados, setPermiteConsultaEmpleados] = useState(false)
  const [validaIp, setValidaIp] = useState(false)
  const [cambiaClave, setCambiaClave] = useState(true)
  const [diasCambioClave, setDiasCambioClave] = useState('')
  const [cargado, setCargado] = useState(false)

  const { data: detalle } = useQuery<UsuarioDetalle>({
    queryKey: ['usuario-detalle', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}`)).data,
  })

  useEffect(() => {
    if (!detalle || cargado) return
    setUsaDispositivoMovil(detalle.usaDispositivoMovil)
    setPermiteRiesgoOperativo(detalle.permiteRiesgoOperativo)
    setPermiteConsultaEmpleados(detalle.permiteConsultaEmpleados)
    setValidaIp(detalle.validaIp)
    setCambiaClave(detalle.cambiaClave)
    setDiasCambioClave(detalle.diasCambioClave?.toString() ?? '')
    setCargado(true)
  }, [detalle, cargado])

  const guardar = useMutation({
    mutationFn: async () =>
      api.put(`/api/usuarios/${usuario.id}`, {
        idAgencia: detalle!.idAgencia, idPersona: null, puedeIngresarSistema: detalle!.puedeIngresarSistema,
        usaDispositivoMovil, permiteRiesgoOperativo, permiteConsultaEmpleados, validaIp,
        cambiaClave, diasCambioClave: diasCambioClave === '' ? null : Number(diasCambioClave),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-detalle', usuario.id] })
    },
  })

  if (!detalle) return <p className="text-sm text-graphite-600">Cargando…</p>

  return (
    <div className="flex flex-col gap-4">
      <div className="rounded-lg border border-black/[0.06] p-3">
        <p className="text-sm font-medium text-graphite-100">Exige cambio de contraseña</p>
        <p className="mb-2 text-xs text-graphite-600">El sistema le pedirá cambiar la clave según el plazo configurado.</p>
        <div className="flex items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-graphite-600">
            <input type="checkbox" checked={cambiaClave} onChange={(e) => setCambiaClave(e.target.checked)} />
            Activo
          </label>
          <input
            value={diasCambioClave}
            onChange={(e) => setDiasCambioClave(e.target.value)}
            type="number"
            placeholder="Días (usa el del rol si vacío)"
            className="w-56 rounded-lg border border-black/[0.08] bg-white px-3 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </div>
      </div>

      <div className="rounded-lg border border-black/[0.06] p-3">
        <p className="mb-2 text-sm font-medium text-graphite-100">Accesos especiales</p>
        <div className="flex flex-col gap-2">
          <label className="flex items-center gap-2 text-sm text-graphite-600">
            <input type="checkbox" checked={usaDispositivoMovil} onChange={(e) => setUsaDispositivoMovil(e.target.checked)} />
            Puede usar la app móvil
          </label>
          <label className="flex items-center gap-2 text-sm text-graphite-600">
            <input type="checkbox" checked={permiteRiesgoOperativo} onChange={(e) => setPermiteRiesgoOperativo(e.target.checked)} />
            Puede registrar/gestionar riesgo operativo
          </label>
          <label className="flex items-center gap-2 text-sm text-graphite-600">
            <input type="checkbox" checked={permiteConsultaEmpleados} onChange={(e) => setPermiteConsultaEmpleados(e.target.checked)} />
            Puede consultar datos de empleados (Nómina)
          </label>
          <label className="flex items-center gap-2 text-sm text-graphite-600">
            <input type="checkbox" checked={validaIp} onChange={(e) => setValidaIp(e.target.checked)} />
            Restringir el acceso por dirección IP
          </label>
        </div>
      </div>

      <button
        type="button"
        disabled={guardar.isPending}
        onClick={() => guardar.mutate()}
        className="btn-hover self-start rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
      >
        {guardar.isPending ? 'Guardando…' : 'Guardar cambios'}
      </button>
      {guardar.isSuccess && <span className="text-xs text-petrol-700">Guardado.</span>}
    </div>
  )
}

function TabRoles({ usuario }: { usuario: UsuarioRol }) {
  const queryClient = useQueryClient()
  const [seleccion, setSeleccion] = useState<Set<number> | null>(null)

  const { data: roles, isLoading } = useQuery<RolAsignado[]>({
    queryKey: ['usuario-roles', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}/roles`)).data,
  })

  useEffect(() => {
    if (roles && seleccion === null) {
      setSeleccion(new Set(roles.filter((r) => r.asignado).map((r) => r.id)))
    }
  }, [roles, seleccion])

  const guardar = useMutation({
    mutationFn: async () => api.put(`/api/usuarios/${usuario.id}/roles`, { idsRol: Array.from(seleccion ?? []) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-roles', usuario.id] })
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })

  const alternar = (id: number) => {
    setSeleccion((prev) => {
      const siguiente = new Set(prev ?? [])
      if (siguiente.has(id)) siguiente.delete(id)
      else siguiente.add(id)
      return siguiente
    })
  }

  if (isLoading || !roles || !seleccion) return <p className="text-sm text-graphite-600">Cargando…</p>

  return (
    <div>
      <p className="mb-3 text-xs text-graphite-600">Roles permanentes asignados a este usuario.</p>
      <div className="mb-4 flex flex-wrap gap-2">
        {roles.map((r) => (
          <button
            key={r.id}
            type="button"
            onClick={() => alternar(r.id)}
            className={`rounded-lg border px-3 py-1.5 text-sm font-medium transition-colors ${
              seleccion.has(r.id)
                ? 'border-gold-500/50 bg-gold-500/10 text-gold-300'
                : 'border-black/[0.08] text-graphite-600 hover:bg-black/[0.02]'
            }`}
          >
            {r.nombre}
          </button>
        ))}
      </div>
      <button
        type="button"
        onClick={() => guardar.mutate()}
        disabled={guardar.isPending}
        className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
      >
        {guardar.isPending ? 'Guardando…' : 'Guardar roles'}
      </button>
    </div>
  )
}

function TabHorarios({ usuario }: { usuario: UsuarioRol }) {
  const queryClient = useQueryClient()
  const [diaSemana, setDiaSemana] = useState('1')
  const [horaInicio, setHoraInicio] = useState('08:00')
  const [horaFin, setHoraFin] = useState('17:00')
  const [esReceso, setEsReceso] = useState(false)

  const { data: horarios, isLoading } = useQuery<HorarioAcceso[]>({
    queryKey: ['usuario-horarios', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}/horarios`)).data,
  })

  const agregar = useMutation({
    mutationFn: async () =>
      api.post(`/api/usuarios/${usuario.id}/horarios`, {
        diaSemana: Number(diaSemana), horaInicio: `${horaInicio}:00`, horaFin: `${horaFin}:00`, esReceso,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-horarios', usuario.id] })
    },
  })

  const quitar = useMutation({
    mutationFn: async (idHorario: string) => api.delete(`/api/usuarios/${usuario.id}/horarios/${idHorario}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-horarios', usuario.id] })
    },
  })

  return (
    <div>
      <p className="mb-3 text-xs text-graphite-600">
        Sin ninguna ventana configurada, el usuario puede ingresar en cualquier momento. Al agregar al menos una
        ventana de ingreso, el login se rechaza fuera de esas horas — y dentro de cualquier ventana de receso.
      </p>

      {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}

      <div className="mb-4 flex flex-col gap-1.5">
        {horarios?.length === 0 && <span className="text-xs text-graphite-600">Sin horario configurado — acceso libre.</span>}
        {horarios?.map((h) => (
          <div key={h.id} className="flex items-center justify-between rounded-lg border border-black/[0.08] px-3 py-2 text-sm">
            <span className="flex items-center gap-2">
              <span className="font-medium text-graphite-100">{DIAS_SEMANA[h.diaSemana - 1]}</span>
              {h.horaInicio.slice(0, 5)}–{h.horaFin.slice(0, 5)}
              {h.esReceso && <Badge variant="alerta">Receso</Badge>}
            </span>
            <button type="button" onClick={() => quitar.mutate(h.id)} className="text-graphite-600 hover:text-red-700">
              <X size={14} />
            </button>
          </div>
        ))}
      </div>

      <form
        className="flex flex-wrap items-end gap-2 rounded-lg border border-black/[0.06] p-3"
        onSubmit={(e) => {
          e.preventDefault()
          agregar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-xs">
          <span className="text-graphite-600">Día</span>
          <select
            value={diaSemana}
            onChange={(e) => setDiaSemana(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm outline-none focus:border-gold-500/50"
          >
            {DIAS_SEMANA.map((d, i) => (
              <option key={d} value={i + 1}>
                {d}
              </option>
            ))}
          </select>
        </label>
        <label className="flex flex-col gap-1 text-xs">
          <span className="text-graphite-600">Desde</span>
          <input type="time" value={horaInicio} onChange={(e) => setHoraInicio(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex flex-col gap-1 text-xs">
          <span className="text-graphite-600">Hasta</span>
          <input type="time" value={horaFin} onChange={(e) => setHoraFin(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex items-center gap-1.5 pb-1.5 text-sm text-graphite-600">
          <input type="checkbox" checked={esReceso} onChange={(e) => setEsReceso(e.target.checked)} /> Es receso
        </label>
        <button type="submit" disabled={agregar.isPending} className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50">
          {agregar.isPending ? 'Agregando…' : 'Agregar ventana'}
        </button>
      </form>
    </div>
  )
}

function TabAsignacionesTemporales({ usuario }: { usuario: UsuarioRol }) {
  const queryClient = useQueryClient()
  const [idRol, setIdRol] = useState<number | ''>('')
  const [fechaCaducidadRol, setFechaCaducidadRol] = useState('')
  const [idAgenciaActual, setIdAgenciaActual] = useState<number | ''>('')
  const [fechaCaducidadAgencia, setFechaCaducidadAgencia] = useState('')

  const { data: roles } = useQuery<Rol[]>({
    queryKey: ['config-roles-simple'],
    queryFn: async () => (await api.get('/api/configuracion/roles')).data,
  })
  const { data: agencias } = useQuery<Agencia[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })
  const { data: rolesTemporales, isLoading: cargandoRolTemp } = useQuery<RolTemporal[]>({
    queryKey: ['usuario-roles-temporales', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}/roles-temporales`)).data,
  })
  const { data: agenciaTemporal, isLoading: cargandoAgenciaTemp } = useQuery<AgenciaTemporal | null>({
    queryKey: ['usuario-agencia-temporal', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}/agencia-temporal`)).data,
  })

  const asignarRol = useMutation({
    mutationFn: async () =>
      api.post(`/api/usuarios/${usuario.id}/roles-temporales`, { idRol, fechaCaducidad: new Date(fechaCaducidadRol).toISOString() }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-roles-temporales', usuario.id] })
      setIdRol('')
      setFechaCaducidadRol('')
    },
  })
  const quitarRol = useMutation({
    mutationFn: async (id: string) => api.delete(`/api/usuarios/${usuario.id}/roles-temporales/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['usuario-roles-temporales', usuario.id] }),
  })

  const asignarAgencia = useMutation({
    mutationFn: async () =>
      api.post(`/api/usuarios/${usuario.id}/agencia-temporal`, { idAgenciaActual, fechaCaducidad: new Date(fechaCaducidadAgencia).toISOString() }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-agencia-temporal', usuario.id] })
      setIdAgenciaActual('')
      setFechaCaducidadAgencia('')
    },
  })
  const quitarAgencia = useMutation({
    mutationFn: async (id: string) => api.delete(`/api/usuarios/${usuario.id}/agencia-temporal/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['usuario-agencia-temporal', usuario.id] }),
  })

  return (
    <div className="flex flex-col gap-6">
      <div>
        <p className="mb-1 text-sm font-medium text-graphite-100">Rol temporal</p>
        <p className="mb-3 text-xs text-graphite-600">
          Se suma a los roles permanentes (licencia, vacaciones, reemplazo) — nunca los reemplaza, y deja de aplicar
          solo al vencer.
        </p>
        {cargandoRolTemp && <p className="text-sm text-graphite-600">Cargando…</p>}
        <div className="mb-3 flex flex-col gap-1.5">
          {rolesTemporales?.length === 0 && <span className="text-xs text-graphite-600">Sin rol temporal asignado.</span>}
          {rolesTemporales?.map((rt) => (
            <div key={rt.id} className="flex items-center justify-between rounded-lg border border-black/[0.08] px-3 py-2 text-sm">
              <span>
                {rt.rol} — hasta {formatoFecha(rt.fechaCaducidad)}{' '}
                <Badge variant={rt.vigente ? 'exito' : 'neutral'}>{rt.vigente ? 'Vigente' : 'Vencido'}</Badge>
              </span>
              <button type="button" onClick={() => quitarRol.mutate(rt.id)} className="text-graphite-600 hover:text-red-700">
                <X size={14} />
              </button>
            </div>
          ))}
        </div>
        <form
          className="flex flex-wrap items-end gap-2 rounded-lg border border-black/[0.06] p-3"
          onSubmit={(e) => {
            e.preventDefault()
            if (idRol === '' || !fechaCaducidadRol) return
            asignarRol.mutate()
          }}
        >
          <select
            value={idRol}
            onChange={(e) => setIdRol(e.target.value ? Number(e.target.value) : '')}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm outline-none focus:border-gold-500/50"
          >
            <option value="">Rol…</option>
            {roles?.map((r) => (
              <option key={r.id} value={r.id}>
                {r.nombre}
              </option>
            ))}
          </select>
          <input
            type="datetime-local"
            value={fechaCaducidadRol}
            onChange={(e) => setFechaCaducidadRol(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm outline-none focus:border-gold-500/50"
          />
          <button type="submit" disabled={asignarRol.isPending} className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50">
            {asignarRol.isPending ? 'Asignando…' : 'Asignar'}
          </button>
          {asignarRol.isError && <span className="text-xs text-red-700">No se pudo asignar.</span>}
        </form>
      </div>

      <div className="border-t border-black/[0.06] pt-5">
        <p className="mb-1 text-sm font-medium text-graphite-100">Agencia temporal</p>
        <p className="mb-3 text-xs text-graphite-600">
          Reasignación temporal (cubrir a un cajero de otra agencia) — se calcula en el próximo login. Asignar una
          nueva reemplaza cualquier reasignación previa activa.
        </p>
        {cargandoAgenciaTemp && <p className="text-sm text-graphite-600">Cargando…</p>}
        {agenciaTemporal ? (
          <div className="mb-3 flex items-center justify-between rounded-lg border border-black/[0.08] px-3 py-2 text-sm">
            <span>
              {agenciaTemporal.agenciaOrigen} → {agenciaTemporal.agenciaActual} — hasta{' '}
              {formatoFecha(agenciaTemporal.fechaCaducidad)}{' '}
              <Badge variant={agenciaTemporal.vigente ? 'exito' : 'neutral'}>{agenciaTemporal.vigente ? 'Vigente' : 'Vencida'}</Badge>
            </span>
            <button type="button" onClick={() => quitarAgencia.mutate(agenciaTemporal.id)} className="text-graphite-600 hover:text-red-700">
              <X size={14} />
            </button>
          </div>
        ) : (
          <p className="mb-3 text-xs text-graphite-600">Sin reasignación activa.</p>
        )}
        <form
          className="flex flex-wrap items-end gap-2 rounded-lg border border-black/[0.06] p-3"
          onSubmit={(e) => {
            e.preventDefault()
            if (idAgenciaActual === '' || !fechaCaducidadAgencia) return
            asignarAgencia.mutate()
          }}
        >
          <select
            value={idAgenciaActual}
            onChange={(e) => setIdAgenciaActual(e.target.value ? Number(e.target.value) : '')}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm outline-none focus:border-gold-500/50"
          >
            <option value="">Agencia destino…</option>
            {agencias?.filter((a) => a.activa).map((a) => (
              <option key={a.id} value={a.id}>
                {a.nombre}
              </option>
            ))}
          </select>
          <input
            type="datetime-local"
            value={fechaCaducidadAgencia}
            onChange={(e) => setFechaCaducidadAgencia(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm outline-none focus:border-gold-500/50"
          />
          <button type="submit" disabled={asignarAgencia.isPending} className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50">
            {asignarAgencia.isPending ? 'Asignando…' : 'Asignar'}
          </button>
          {asignarAgencia.isError && <span className="text-xs text-red-700">No se pudo asignar.</span>}
        </form>
      </div>
    </div>
  )
}

function TabSesiones({ usuario }: { usuario: UsuarioRol }) {
  const queryClient = useQueryClient()

  const { data: sesiones, isLoading } = useQuery<SesionUsuario[]>({
    queryKey: ['usuario-sesiones', usuario.id],
    queryFn: async () => (await api.get(`/api/usuarios/${usuario.id}/sesiones`)).data,
  })

  const revocarTodas = useMutation({
    mutationFn: async () => api.post(`/api/usuarios/${usuario.id}/sesiones/revocar-todas`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuario-sesiones', usuario.id] })
    },
  })

  const hayVigentes = sesiones?.some((s) => s.vigente) ?? false

  return (
    <div>
      <div className="mb-3 flex items-center gap-3">
        <button
          type="button"
          onClick={() => revocarTodas.mutate()}
          disabled={!hayVigentes || revocarTodas.isPending}
          className="btn-hover rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-40"
        >
          {revocarTodas.isPending ? 'Cerrando…' : 'Cerrar todas las sesiones'}
        </button>
        {revocarTodas.isSuccess && (
          <span className="text-xs text-petrol-700">Listo — cualquier token emitido antes de ahora ya no es válido.</span>
        )}
        {!hayVigentes && !revocarTodas.isSuccess && <span className="text-xs text-graphite-600">No hay sesiones activas para cerrar.</span>}
      </div>

      {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}

      {sesiones && (
        <div className="max-h-72 overflow-y-auto rounded-lg border border-black/[0.06]">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-black/[0.02] text-left text-graphite-600">
                <th className="px-3 py-2 font-medium">Emitida</th>
                <th className="px-3 py-2 font-medium">Expira</th>
                <th className="px-3 py-2 font-medium">IP</th>
                <th className="px-3 py-2 font-medium">Estado</th>
              </tr>
            </thead>
            <tbody>
              {sesiones.length === 0 && (
                <tr>
                  <td colSpan={4} className="px-3 py-4 text-center text-graphite-600">
                    Sin sesiones registradas
                  </td>
                </tr>
              )}
              {sesiones.map((s) => (
                <tr key={s.id} className="border-t border-black/[0.04]">
                  <td className="px-3 py-2 text-graphite-100">{formatoFecha(s.emitidaEn)}</td>
                  <td className="px-3 py-2 text-graphite-100">{formatoFecha(s.expiraEn)}</td>
                  <td className="px-3 py-2 text-graphite-600">{s.direccionIp ?? '—'}</td>
                  <td className="px-3 py-2">
                    {s.vigente ? (
                      <Badge variant="exito">Vigente</Badge>
                    ) : s.revocada ? (
                      <Badge variant="peligro">Revocada</Badge>
                    ) : (
                      <Badge variant="neutral">Expirada</Badge>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}

function GestionarUsuarioModal({
  usuario,
  onClose,
  tabInicial = 'datos',
  recienCreado = false,
}: {
  usuario: UsuarioRol
  onClose: () => void
  tabInicial?: TabGestionId
  recienCreado?: boolean
}) {
  const [tab, setTab] = useState<TabGestionId>(tabInicial)

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card flex max-h-[85vh] w-full max-w-3xl flex-col overflow-hidden rounded-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-black/[0.06] p-5 pb-4">
          <h2 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
            <UserCog size={16} /> {recienCreado ? `Usuario ${usuario.nombreUsuario} creado — completá su configuración` : `Gestionar ${usuario.nombreUsuario}`}
          </h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        {recienCreado && (
          <p className="border-b border-black/[0.06] bg-gold-500/5 px-5 py-2 text-xs text-graphite-600">
            Empezá por <strong>Roles</strong> — sin al menos uno asignado, este usuario no puede ver ningún módulo
            todavía.
          </p>
        )}

        <div className="flex flex-1 overflow-hidden">
          <div className="flex w-44 flex-shrink-0 flex-col gap-0.5 border-r border-black/[0.06] p-2">
            {TABS_GESTION.map((t) => (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className={`flex items-center gap-2 rounded-lg px-3 py-2 text-left text-xs font-medium transition-colors ${
                  tab === t.id ? 'bg-gold-500/10 text-gold-300' : 'text-graphite-600 hover:bg-black/[0.02]'
                }`}
              >
                <t.icon size={14} /> {t.label}
              </button>
            ))}
          </div>

          <div className="flex-1 overflow-y-auto p-5">
            {tab === 'datos' && <TabDatosYAcceso usuario={usuario} onGuardado={() => {}} />}
            {tab === 'seguridad' && <TabSeguridad usuario={usuario} />}
            {tab === 'roles' && <TabRoles usuario={usuario} />}
            {tab === 'horarios' && <TabHorarios usuario={usuario} />}
            {tab === 'temporal' && <TabAsignacionesTemporales usuario={usuario} />}
            {tab === 'sesiones' && <TabSesiones usuario={usuario} />}
          </div>
        </div>
      </div>
    </div>
  )
}

export function UsuariosRoles() {
  const [q, setQ] = useState('')
  const [agenciaFiltro, setAgenciaFiltro] = useState('TODAS')
  const [usuarioGestionar, setUsuarioGestionar] = useState<UsuarioRol | null>(null)
  const [usuarioRecienCreado, setUsuarioRecienCreado] = useState(false)
  const [mostrarNuevo, setMostrarNuevo] = useState(false)
  const queryClient = useQueryClient()

  const { data, isLoading } = useQuery<UsuarioRol[]>({
    queryKey: ['usuarios', q],
    queryFn: async () => (await api.get('/api/usuarios', { params: { q: q || undefined } })).data,
  })

  // Agrupado por agencia en vez de una sola lista plana — con 40+ usuarios
  // reales importados, una tabla única se volvía imposible de escanear.
  // Mismo patrón de pestañas ya usado en el resto del proyecto (ej.
  // Configuración agrupada por módulo).
  const agencias = useMemo(
    () => Array.from(new Set((data ?? []).map((u) => u.agencia))).sort((a, b) => a.localeCompare(b)),
    [data],
  )
  const dataFiltrada = useMemo(
    () => (agenciaFiltro === 'TODAS' ? (data ?? []) : (data ?? []).filter((u) => u.agencia === agenciaFiltro)),
    [data, agenciaFiltro],
  )

  const toggleBloqueo = useMutation({
    mutationFn: async ({ id, bloquear }: { id: string; bloquear: boolean }) =>
      api.patch(`/api/usuarios/${id}/bloqueo`, { bloquear }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })

  return (
    <div className="animate-fade-in">
      <PageHeader icon={ShieldCheck} title="Usuarios y roles" subtitle="Accesos, permisos y seguridad del sistema" />

      {mostrarNuevo && (
        <ModalPortal>
          <NuevoUsuarioModal
            onClose={() => setMostrarNuevo(false)}
            onCreado={(u) => {
              setMostrarNuevo(false)
              setUsuarioRecienCreado(true)
              setUsuarioGestionar(u)
            }}
          />
        </ModalPortal>
      )}
      {usuarioGestionar && (
        <ModalPortal>
          <GestionarUsuarioModal
            usuario={usuarioGestionar}
            tabInicial={usuarioRecienCreado ? 'roles' : 'datos'}
            recienCreado={usuarioRecienCreado}
            onClose={() => {
              setUsuarioGestionar(null)
              setUsuarioRecienCreado(false)
            }}
          />
        </ModalPortal>
      )}

      <div className="mb-4 flex items-center gap-3">
        <div className="flex-1">
          <SearchBar value={q} onChange={setQ} placeholder="Buscar usuario o nombre…" />
        </div>
        <button
          type="button"
          onClick={() => setMostrarNuevo(true)}
          className="flex items-center gap-1 rounded-lg bg-gold-500 px-3 py-2 text-xs font-medium text-white hover:bg-gold-600"
        >
          <Plus size={14} /> Nuevo usuario
        </button>
      </div>

      {agencias.length > 1 && (
        <div className="mb-4 flex flex-wrap gap-1 rounded-lg border border-black/[0.08] p-1">
          <button
            type="button"
            onClick={() => setAgenciaFiltro('TODAS')}
            className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
              agenciaFiltro === 'TODAS' ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.02]'
            }`}
          >
            Todas ({data?.length ?? 0})
          </button>
          {agencias.map((a) => (
            <button
              key={a}
              type="button"
              onClick={() => setAgenciaFiltro(a)}
              className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
                agenciaFiltro === a ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.02]'
              }`}
            >
              {a} ({data?.filter((u) => u.agencia === a).length ?? 0})
            </button>
          ))}
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Usuario</Th>
            <Th>Nombre</Th>
            <Th>Correo</Th>
            {agenciaFiltro === 'TODAS' && <Th>Agencia</Th>}
            <Th>Estado</Th>
            <Th>Roles</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && dataFiltrada.length === 0 && <EmptyState>No se encontraron usuarios</EmptyState>}
          {dataFiltrada.map((u) => (
            <tr key={u.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="whitespace-nowrap font-medium">{u.nombreUsuario}</Td>
              <Td className="max-w-[200px] truncate" title={u.nombrePersona ?? undefined}>
                {u.nombrePersona ?? '—'}
              </Td>
              <Td className="max-w-[190px] truncate text-xs text-graphite-600" title={u.email ?? undefined}>
                {u.email ?? '—'}
              </Td>
              {agenciaFiltro === 'TODAS' && <Td className="whitespace-nowrap">{u.agencia}</Td>}
              <Td>
                <div className="flex items-center gap-1.5">
                  <Badge variant={u.activo ? 'exito' : 'neutral'}>{u.activo ? 'Activo' : 'Inactivo'}</Badge>
                  <button
                    type="button"
                    disabled={toggleBloqueo.isPending}
                    onClick={() => toggleBloqueo.mutate({ id: u.id, bloquear: !u.tieneBloqueo })}
                    className="disabled:opacity-50"
                    title={u.tieneBloqueo ? 'Desbloquear acceso' : 'Bloquear acceso (cierra sus sesiones activas)'}
                  >
                    <Badge variant={u.tieneBloqueo ? 'peligro' : 'neutral'}>
                      {u.tieneBloqueo ? 'Bloqueado' : 'Sin bloqueo'}
                    </Badge>
                  </button>
                </div>
              </Td>
              <Td>
                {u.roles.length === 0 ? (
                  <span className="text-xs text-graphite-700">Sin roles</span>
                ) : (
                  <div className="flex flex-wrap items-center gap-1" title={u.roles.join(', ')}>
                    {u.roles.slice(0, 2).map((r) => (
                      <Badge key={r}>{r}</Badge>
                    ))}
                    {u.roles.length > 2 && <Badge variant="neutral">+{u.roles.length - 2}</Badge>}
                  </div>
                )}
              </Td>
              <Td>
                <button
                  type="button"
                  onClick={() => setUsuarioGestionar(u)}
                  className="flex items-center gap-1 whitespace-nowrap text-xs font-medium text-gold-400 hover:underline"
                >
                  <UserCog size={13} /> Gestionar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
