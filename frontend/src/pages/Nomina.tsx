import { useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Briefcase,
  Plus,
  X,
  Trash2,
  Gift,
  Calendar,
  PiggyBank,
  TreePalm,
  Pencil,
  UserPlus,
  Users,
  ShieldCheck,
  IdCard,
  FileText,
  Receipt,
} from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { api } from '../lib/api'

interface Empleado {
  id: string
  idPersona: string
  nombre: string
  identificacion: string
  idCargo: number
  cargo: string
  idAgencia: number
  agencia: string
  recibeFondosReserva: boolean
  fechaIngreso: string
  estado: string
  sueldoActual: number | null
}

interface PersonaBusqueda {
  id: string
  nombre: string
  identificacion: string
}

interface CargoItem {
  id: number
  nombre: string
  seProrrateaSueldo: boolean
  esCargoExterno: boolean
  activo: boolean
}

interface AgenciaItem {
  id: number
  nombre: string
}

interface ParametroNomina {
  id: number
  salarioBasicoUnificado: number
}

interface BeneficioEmpleadoItem {
  idEmpleado: string
  nombre: string
  proyectado: number
  acumulado: number
  pagado: number
  pendiente: number
  ultimoDevengo: string | null
}

interface DevengoBeneficiosResult {
  fecha: string
  empleadosProcesados: number
  totalDecimoTercero: number
  totalDecimoCuarto: number
  totalFondosReserva: number
  totalProvisionVacaciones: number
  totalAportePatronal: number
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

function mensajeError(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
}

const ESTADOS_EMPLEADO = ['Activo', 'Vacaciones', 'LicenciaSinSueldo', 'Desvinculado'] as const

function badgeEstadoEmpleado(estado: string) {
  if (estado === 'Activo') return <Badge variant="exito">Activo</Badge>
  if (estado === 'Desvinculado') return <Badge variant="peligro">Desvinculado</Badge>
  return <Badge variant="alerta">{estado}</Badge>
}

function lineaVacia(): LineaForm {
  return { idEmpleado: '', ingresos: '', egresos: '0', diasLaborados: '30' }
}

function GenerarRolPagosForm({ empleados, onClose }: { empleados: Empleado[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [periodo, setPeriodo] = useState('')
  const [tipo, setTipo] = useState('Mensual')
  const [lineas, setLineas] = useState<LineaForm[]>([lineaVacia()])

  const empleadosActivos = empleados.filter((e) => e.estado === 'Activo')

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
                {empleadosActivos.map((e) => (
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

        {generar.isError && <p className="text-sm text-red-700">{mensajeError(generar.error, 'No se pudo generar el rol de pagos.')}</p>}
      </form>
    </div>
  )
}

function BuscarPersonaNomina({ onSeleccionar }: { onSeleccionar: (p: PersonaBusqueda) => void }) {
  const [q, setQ] = useState('')

  const { data: personas } = useQuery<PersonaBusqueda[]>({
    queryKey: ['nomina-buscar-persona', q],
    queryFn: async () => (await api.get('/api/nomina/personas/buscar', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  return (
    <div className="flex flex-col gap-1">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar persona por nombre o identificación…"
        className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
      />
      {q.length >= 2 && (personas?.length ?? 0) > 0 && (
        <div className="max-h-40 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
          {personas?.map((p) => (
            <button
              key={p.id}
              type="button"
              onClick={() => {
                onSeleccionar(p)
                setQ('')
              }}
              className="block w-full px-3 py-1.5 text-left text-sm hover:bg-black/[0.02]"
            >
              {p.nombre} — {p.identificacion}
            </button>
          ))}
        </div>
      )}
      {q.length >= 2 && (personas?.length ?? 0) === 0 && (
        <p className="px-1 text-xs text-graphite-600">Sin resultados, o la persona ya es empleado.</p>
      )}
    </div>
  )
}

interface TipoIdentificacionItem {
  id: number
  codigo: string
  nombre: string
}

function NuevoEmpleadoForm({
  cargos,
  agencias,
  onClose,
  onCreado,
}: {
  cargos: CargoItem[]
  agencias: AgenciaItem[]
  onClose: () => void
  onCreado: (idEmpleado: string) => void
}) {
  const queryClient = useQueryClient()
  const [modo, setModo] = useState<'existente' | 'nueva'>('existente')
  const [persona, setPersona] = useState<PersonaBusqueda | null>(null)

  const [idTipoIdentificacion, setIdTipoIdentificacion] = useState('')
  const [identificacion, setIdentificacion] = useState('')
  const [primerNombre, setPrimerNombre] = useState('')
  const [segundoNombre, setSegundoNombre] = useState('')
  const [apellidoPaterno, setApellidoPaterno] = useState('')
  const [apellidoMaterno, setApellidoMaterno] = useState('')
  const [fechaNacimiento, setFechaNacimiento] = useState('')
  const [esMasculino, setEsMasculino] = useState(true)
  const [email, setEmail] = useState('')

  const [idCargo, setIdCargo] = useState('')
  const [idAgencia, setIdAgencia] = useState('')
  const [fechaIngreso, setFechaIngreso] = useState('')
  const [sueldoInicial, setSueldoInicial] = useState('')
  const [recibeFondosReserva, setRecibeFondosReserva] = useState(true)

  const { data: tiposIdentificacion } = useQuery<TipoIdentificacionItem[]>({
    queryKey: ['configuracion-tipos-identificacion'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-identificacion')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/nomina/empleados', {
          idPersona: modo === 'existente' ? persona?.id : null,
          idAgencia: Number(idAgencia),
          idCargo: Number(idCargo),
          fechaIngreso,
          recibeFondosReserva,
          sueldoInicial: sueldoInicial ? Number(sueldoInicial) : null,
          idTipoIdentificacion: modo === 'nueva' ? Number(idTipoIdentificacion) : null,
          identificacion: modo === 'nueva' ? identificacion : null,
          primerNombre: modo === 'nueva' ? primerNombre : null,
          segundoNombre: modo === 'nueva' ? segundoNombre || null : null,
          apellidoPaterno: modo === 'nueva' ? apellidoPaterno : null,
          apellidoMaterno: modo === 'nueva' ? apellidoMaterno || null : null,
          fechaNacimiento: modo === 'nueva' ? fechaNacimiento : null,
          esMasculino: modo === 'nueva' ? esMasculino : null,
          email: modo === 'nueva' ? email || null : null,
        })
      ).data as { idEmpleado: string },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['nomina-empleados'] })
      onCreado(data.idEmpleado)
    },
  })

  const puedeGuardar =
    modo === 'existente'
      ? !!persona
      : !!idTipoIdentificacion && !!identificacion && !!primerNombre && !!apellidoPaterno && !!fechaNacimiento

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nuevo empleado</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault()
          if (!puedeGuardar || !idCargo || !idAgencia || !fechaIngreso) return
          crear.mutate()
        }}
      >
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setModo('existente')}
            className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
              modo === 'existente' ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.03]'
            }`}
          >
            Persona existente
          </button>
          <button
            type="button"
            onClick={() => setModo('nueva')}
            className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
              modo === 'nueva' ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.03]'
            }`}
          >
            Persona nueva
          </button>
        </div>

        {modo === 'existente' ? (
          <div>
            <span className="mb-1 block text-sm text-graphite-600">Persona</span>
            {persona ? (
              <div className="flex items-center justify-between rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm">
                <span>
                  {persona.nombre} — {persona.identificacion}
                </span>
                <button type="button" onClick={() => setPersona(null)} className="text-graphite-600 hover:text-red-700">
                  <X size={14} />
                </button>
              </div>
            ) : (
              <BuscarPersonaNomina onSeleccionar={setPersona} />
            )}
          </div>
        ) : (
          <div className="flex flex-col gap-3 rounded-lg border border-black/[0.06] p-3">
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Tipo de identificación</span>
                <select
                  required
                  value={idTipoIdentificacion}
                  onChange={(e) => setIdTipoIdentificacion(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                >
                  <option value="">Seleccionar…</option>
                  {tiposIdentificacion?.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.nombre}
                    </option>
                  ))}
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Identificación</span>
                <input
                  required
                  value={identificacion}
                  onChange={(e) => setIdentificacion(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
            </div>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Primer nombre</span>
                <input
                  required
                  value={primerNombre}
                  onChange={(e) => setPrimerNombre(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Segundo nombre</span>
                <input
                  value={segundoNombre}
                  onChange={(e) => setSegundoNombre(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Apellido paterno</span>
                <input
                  required
                  value={apellidoPaterno}
                  onChange={(e) => setApellidoPaterno(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Apellido materno</span>
                <input
                  value={apellidoMaterno}
                  onChange={(e) => setApellidoMaterno(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
            </div>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Fecha de nacimiento</span>
                <input
                  required
                  type="date"
                  value={fechaNacimiento}
                  onChange={(e) => setFechaNacimiento(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Género</span>
                <select
                  value={esMasculino ? 'M' : 'F'}
                  onChange={(e) => setEsMasculino(e.target.value === 'M')}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                >
                  <option value="M">Masculino</option>
                  <option value="F">Femenino</option>
                </select>
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Email</span>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
            </div>
          </div>
        )}

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Cargo</span>
            <select
              required
              value={idCargo}
              onChange={(e) => setIdCargo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {cargos.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Agencia</span>
            <select
              required
              value={idAgencia}
              onChange={(e) => setIdAgencia(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {agencias.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.nombre}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Fecha de ingreso</span>
            <input
              required
              type="date"
              value={fechaIngreso}
              onChange={(e) => setFechaIngreso(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Sueldo mensual inicial</span>
            <input
              type="number"
              step="0.01"
              min="0"
              placeholder="Opcional"
              value={sueldoInicial}
              onChange={(e) => setSueldoInicial(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
        </div>

        <label className="flex items-center gap-2 text-sm text-graphite-600">
          <input
            type="checkbox"
            checked={recibeFondosReserva}
            onChange={(e) => setRecibeFondosReserva(e.target.checked)}
            className="rounded border-black/[0.2]"
          />
          Recibe fondos de reserva (a partir del año de antigüedad)
        </label>

        <p className="text-xs text-graphite-600">
          Después de guardar se abre el detalle completo del empleado — ahí se registran datos adicionales, contrato,
          IESS/Ministerio de Trabajo y domicilio.
        </p>

        <div className="flex items-center gap-2">
          <button
            type="submit"
            disabled={crear.isPending || !puedeGuardar}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {crear.isPending ? 'Guardando…' : 'Guardar empleado'}
          </button>
        </div>

        {crear.isError && <p className="text-sm text-red-700">{mensajeError(crear.error, 'No se pudo crear el empleado.')}</p>}
      </form>
    </div>
  )
}

interface TipoContratoItem {
  codigo: string
  nombre: string
  esTiempoCompleto: boolean
  esTiempoParcial: boolean
  tieneFechaSalida: boolean
}

interface EmpleadoContratoItem {
  id: string
  codigoTipoContrato: string
  tipoContrato: string
  numeroContrato: number
  fechaIngreso: string
  fechaSalida: string | null
  activo: boolean
}

interface EmpleadoDatosAdicionales {
  idEmpleado: string
  codigoIess: string | null
  fechaIngresoIess: string | null
  fechaSalidaIess: string | null
  fechaIngresoMinisterioLaboral: string | null
  fechaSalidaMinisterioLaboral: string | null
  numeroCargasFamiliares: number | null
  pagoDecimoMensual: boolean
  pagoFondosReservaRol: boolean
  extensionConyugal: boolean
}

interface CalculoImpuestoRenta {
  id: string
  anio: number
  ingresoAnualProyectado: number
  aportePersonalIess: number
  baseImponible: number
  impuestoCausadoAnual: number
  retencionMensual: number
  fechaCalculo: string
}

function CampoLectura({ label, valor }: { label: string; valor: ReactNode }) {
  return (
    <div className="flex items-baseline justify-between gap-4 border-b border-black/[0.04] py-2 text-sm last:border-0">
      <span className="text-graphite-600">{label}</span>
      <span className="text-right font-medium text-graphite-100">{valor}</span>
    </div>
  )
}

function BotonEditar({ onClick }: { onClick: () => void }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] px-3 py-1.5 text-sm font-medium text-graphite-100 hover:bg-black/[0.03]"
    >
      <Pencil size={14} /> Editar
    </button>
  )
}

function SeccionContratos({ idEmpleado }: { idEmpleado: string }) {
  const queryClient = useQueryClient()
  const [codigoTipoContrato, setCodigoTipoContrato] = useState('')
  const [fechaIngreso, setFechaIngreso] = useState('')

  const { data: contratos } = useQuery<EmpleadoContratoItem[]>({
    queryKey: ['nomina-contratos', idEmpleado],
    queryFn: async () => (await api.get(`/api/nomina/empleados/${idEmpleado}/contratos`)).data,
  })

  const { data: tipos } = useQuery<TipoContratoItem[]>({
    queryKey: ['nomina-tipos-contrato'],
    queryFn: async () => (await api.get('/api/nomina/tipos-contrato')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      (await api.post(`/api/nomina/empleados/${idEmpleado}/contratos`, { codigoTipoContrato, fechaIngreso })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-contratos', idEmpleado] })
      setCodigoTipoContrato('')
      setFechaIngreso('')
    },
  })

  return (
    <div className="flex flex-col gap-6">
      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-3 flex items-center gap-2 text-sm font-semibold text-graphite-100">
          <FileText size={16} /> Historial de contratos
        </h3>
        {(contratos?.length ?? 0) === 0 ? (
          <p className="text-sm text-graphite-600">Sin contratos registrados todavía.</p>
        ) : (
          <div className="flex flex-col">
            {contratos?.map((c) => (
              <div key={c.id} className="flex items-center justify-between border-b border-black/[0.04] py-2.5 text-sm last:border-0">
                <div>
                  <span className="font-medium text-graphite-100">#{c.numeroContrato} {c.tipoContrato}</span>
                  <p className="text-graphite-600">
                    {c.fechaIngreso}
                    {c.fechaSalida ? ` a ${c.fechaSalida}` : ''}
                  </p>
                </div>
                {c.activo ? <Badge variant="exito">Activo</Badge> : <Badge variant="neutral">Cerrado</Badge>}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-3 text-sm font-semibold text-graphite-100">Registrar nuevo contrato</h3>
        <form
          className="flex flex-col gap-3 sm:flex-row sm:items-end"
          onSubmit={(e) => {
            e.preventDefault()
            if (!codigoTipoContrato || !fechaIngreso) return
            crear.mutate()
          }}
        >
          <label className="flex flex-1 flex-col gap-1 text-sm">
            <span className="text-graphite-600">Tipo de contrato</span>
            <select
              value={codigoTipoContrato}
              onChange={(e) => setCodigoTipoContrato(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {tipos?.map((t) => (
                <option key={t.codigo} value={t.codigo}>
                  {t.nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Fecha de ingreso</span>
            <input
              type="date"
              value={fechaIngreso}
              onChange={(e) => setFechaIngreso(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <button
            type="submit"
            disabled={crear.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            + Nuevo contrato
          </button>
        </form>
        {crear.isError && (
          <p className="mt-2 text-sm text-red-700">{mensajeError(crear.error, 'No se pudo registrar el contrato.')}</p>
        )}
      </div>
    </div>
  )
}

function SeccionDatosAdicionales({ idEmpleado }: { idEmpleado: string }) {
  const queryClient = useQueryClient()
  const [editando, setEditando] = useState(false)

  const { data: datos } = useQuery<EmpleadoDatosAdicionales>({
    queryKey: ['nomina-datos-adicionales', idEmpleado],
    queryFn: async () => (await api.get(`/api/nomina/empleados/${idEmpleado}/datos-adicionales`)).data,
  })

  const [form, setForm] = useState<EmpleadoDatosAdicionales | null>(null)

  const guardar = useMutation({
    mutationFn: async () => (await api.put(`/api/nomina/empleados/${idEmpleado}/datos-adicionales`, form)).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-datos-adicionales', idEmpleado] })
      setEditando(false)
    },
  })

  if (!datos) return null

  const actual = form ?? datos

  return (
    <div className="glass-card rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
          <IdCard size={16} /> IESS / Ministerio de Trabajo
        </h3>
        {!editando && (
          <BotonEditar
            onClick={() => {
              setForm(datos)
              setEditando(true)
            }}
          />
        )}
      </div>

      {!editando ? (
        <div className="flex flex-col">
          <CampoLectura label="Código IESS" valor={datos.codigoIess ?? '—'} />
          <CampoLectura label="Fecha de ingreso IESS" valor={datos.fechaIngresoIess ?? '—'} />
          <CampoLectura label="Fecha de ingreso Ministerio de Trabajo" valor={datos.fechaIngresoMinisterioLaboral ?? '—'} />
          <CampoLectura label="Cargas familiares" valor={datos.numeroCargasFamiliares ?? '—'} />
          <CampoLectura label="Pago décimo mensual" valor={datos.pagoDecimoMensual ? 'Sí' : 'No'} />
          <CampoLectura label="Pago fondos de reserva en rol" valor={datos.pagoFondosReservaRol ? 'Sí' : 'No'} />
          <CampoLectura label="Extensión conyugal" valor={datos.extensionConyugal ? 'Sí' : 'No'} />
        </div>
      ) : (
        <form
          className="flex flex-col gap-3"
          onSubmit={(e) => {
            e.preventDefault()
            guardar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Código IESS</span>
            <input
              value={actual.codigoIess ?? ''}
              onChange={(ev) => setForm({ ...actual, codigoIess: ev.target.value || null })}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Fecha de ingreso IESS</span>
              <input
                type="date"
                value={actual.fechaIngresoIess ?? ''}
                onChange={(ev) => setForm({ ...actual, fechaIngresoIess: ev.target.value || null })}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Fecha de ingreso Ministerio de Trabajo</span>
              <input
                type="date"
                value={actual.fechaIngresoMinisterioLaboral ?? ''}
                onChange={(ev) => setForm({ ...actual, fechaIngresoMinisterioLaboral: ev.target.value || null })}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          </div>
          <label className="flex max-w-xs flex-col gap-1 text-sm">
            <span className="text-graphite-600">Cargas familiares</span>
            <input
              type="number"
              min="0"
              value={actual.numeroCargasFamiliares ?? ''}
              onChange={(ev) => setForm({ ...actual, numeroCargasFamiliares: ev.target.value ? Number(ev.target.value) : null })}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <div className="flex flex-col gap-2 rounded-lg border border-black/[0.06] p-3">
            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input
                type="checkbox"
                checked={actual.pagoDecimoMensual}
                onChange={(ev) => setForm({ ...actual, pagoDecimoMensual: ev.target.checked })}
                className="rounded border-black/[0.2]"
              />
              Pago décimo mensual
            </label>
            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input
                type="checkbox"
                checked={actual.pagoFondosReservaRol}
                onChange={(ev) => setForm({ ...actual, pagoFondosReservaRol: ev.target.checked })}
                className="rounded border-black/[0.2]"
              />
              Pago fondos de reserva en rol
            </label>
            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input
                type="checkbox"
                checked={actual.extensionConyugal}
                onChange={(ev) => setForm({ ...actual, extensionConyugal: ev.target.checked })}
                className="rounded border-black/[0.2]"
              />
              Extensión conyugal
            </label>
          </div>
          <div className="flex items-center gap-3">
            <button
              type="submit"
              disabled={guardar.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              Guardar
            </button>
            <button type="button" onClick={() => setEditando(false)} className="text-sm text-graphite-600 hover:text-graphite-100">
              Cancelar
            </button>
          </div>
          {guardar.isError && <p className="text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar.')}</p>}
        </form>
      )}
    </div>
  )
}

function SeccionImpuestoRenta({ idEmpleado }: { idEmpleado: string }) {
  const queryClient = useQueryClient()

  const { data: historial } = useQuery<CalculoImpuestoRenta[]>({
    queryKey: ['nomina-impuesto-renta', idEmpleado],
    queryFn: async () => (await api.get(`/api/nomina/empleados/${idEmpleado}/impuesto-renta/historial`)).data,
  })

  const calcular = useMutation({
    mutationFn: async () => (await api.post(`/api/nomina/empleados/${idEmpleado}/impuesto-renta/calcular`)).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-impuesto-renta', idEmpleado] })
    },
  })

  const ultimo = historial?.[0]

  return (
    <div className="glass-card rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
          <Receipt size={16} /> Impuesto a la Renta (relación de dependencia)
        </h3>
        <button
          type="button"
          disabled={calcular.isPending}
          onClick={() => calcular.mutate()}
          className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {calcular.isPending ? 'Calculando…' : 'Calcular proyección'}
        </button>
      </div>
      {calcular.isError && (
        <p className="mb-3 text-sm text-red-700">{mensajeError(calcular.error, 'No se pudo calcular.')}</p>
      )}
      {ultimo ? (
        <div className="flex flex-col">
          <CampoLectura label="Ingreso anual proyectado" valor={formatoUsd(ultimo.ingresoAnualProyectado)} />
          <CampoLectura label="Aporte personal IESS" valor={formatoUsd(ultimo.aportePersonalIess)} />
          <CampoLectura label="Base imponible" valor={formatoUsd(ultimo.baseImponible)} />
          <CampoLectura label="Impuesto causado anual" valor={formatoUsd(ultimo.impuestoCausadoAnual)} />
          <CampoLectura
            label="Retención mensual"
            valor={<span className="text-base text-gold-300">{formatoUsd(ultimo.retencionMensual)}</span>}
          />
          <CampoLectura label="Calculado el" valor={ultimo.fechaCalculo} />
        </div>
      ) : (
        <p className="text-sm text-graphite-600">Sin cálculo todavía — usá "Calcular proyección".</p>
      )}
    </div>
  )
}

interface CatalogoItem {
  codigo: string
  nombre: string
  activo: boolean
}

function useCatalogoNomina(endpoint: string) {
  return useQuery<CatalogoItem[]>({
    queryKey: ['catalogo', endpoint],
    queryFn: async () => (await api.get(`/api/configuracion/${endpoint}`)).data,
  })
}

function CatalogoSelectNomina({
  endpoint,
  value,
  onChange,
  placeholder,
}: {
  endpoint: string
  value: string
  onChange: (v: string) => void
  placeholder: string
}) {
  const { data } = useCatalogoNomina(endpoint)
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
    >
      <option value="">{placeholder}</option>
      {data
        ?.filter((c) => c.activo || c.codigo === value)
        .map((c) => (
          <option key={c.codigo} value={c.codigo}>
            {c.nombre}
          </option>
        ))}
    </select>
  )
}

interface ProvinciaItem {
  codigo: string
  nombre: string
}

interface EmpleadoTelefonoItem {
  id: string
  telefono: string
  esTelefonoMovil: boolean
  esPrincipal: boolean
  notificacionSms: boolean
}

interface EmpleadoPersonaDetalle {
  idPersona: string
  nombre: string
  identificacion: string
  email: string | null
  callePrincipal: string | null
  numeroCasa: string | null
  barrio: string | null
  provinciaDomicilio: { codigo: string; nombre: string } | null
  fechaNacimiento: string | null
  esMasculino: boolean | null
  estadoCivil: { codigo: string; nombre: string } | null
  educacion: { codigo: string; nombre: string } | null
  nacionalidad: { codigo: string; nombre: string } | null
  telefonos: EmpleadoTelefonoItem[]
}

function SeccionDatosPersona({ idEmpleado }: { idEmpleado: string }) {
  const queryClient = useQueryClient()
  const [editando, setEditando] = useState(false)
  const [telefonoNuevo, setTelefonoNuevo] = useState('')

  const { data: detalle } = useQuery<EmpleadoPersonaDetalle>({
    queryKey: ['nomina-persona-detalle', idEmpleado],
    queryFn: async () => (await api.get(`/api/nomina/empleados/${idEmpleado}/persona-detalle`)).data,
  })

  const { data: provincias } = useQuery<ProvinciaItem[]>({
    queryKey: ['socios-provincias'],
    queryFn: async () => (await api.get('/api/socios/provincias')).data,
  })

  const [form, setForm] = useState<{
    email: string
    callePrincipal: string
    numeroCasa: string
    barrio: string
    codigoProvinciaDomicilio: string
    codigoEstadoCivil: string
    codigoEducacion: string
    codigoNacionalidad: string
  } | null>(null)

  const guardar = useMutation({
    mutationFn: async () => (await api.put(`/api/nomina/empleados/${idEmpleado}/persona`, form)).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-persona-detalle', idEmpleado] })
      setEditando(false)
    },
  })

  const agregarTelefono = useMutation({
    mutationFn: async () =>
      (
        await api.post(`/api/nomina/empleados/${idEmpleado}/telefonos`, {
          telefono: telefonoNuevo,
          esTelefonoMovil: true,
          esPrincipal: (detalle?.telefonos.length ?? 0) === 0,
          notificacionSms: true,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-persona-detalle', idEmpleado] })
      setTelefonoNuevo('')
    },
  })

  const quitarTelefono = useMutation({
    mutationFn: async (id: string) => (await api.delete(`/api/nomina/telefonos/${id}`)).data,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['nomina-persona-detalle', idEmpleado] }),
  })

  if (!detalle) return null

  return (
    <div className="flex flex-col gap-6">
      <div className="glass-card rounded-xl p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
            <IdCard size={16} /> Domicilio y datos personales
          </h3>
          {!editando && (
            <BotonEditar
              onClick={() => {
                setForm({
                  email: detalle.email ?? '',
                  callePrincipal: detalle.callePrincipal ?? '',
                  numeroCasa: detalle.numeroCasa ?? '',
                  barrio: detalle.barrio ?? '',
                  codigoProvinciaDomicilio: detalle.provinciaDomicilio?.codigo ?? '',
                  codigoEstadoCivil: detalle.estadoCivil?.codigo ?? '',
                  codigoEducacion: detalle.educacion?.codigo ?? '',
                  codigoNacionalidad: detalle.nacionalidad?.codigo ?? '',
                })
                setEditando(true)
              }}
            />
          )}
        </div>

        {!editando ? (
          <div className="flex flex-col">
            <CampoLectura label="Email" valor={detalle.email ?? '—'} />
            <CampoLectura
              label="Domicilio"
              valor={
                detalle.callePrincipal
                  ? `${detalle.callePrincipal} ${detalle.numeroCasa ?? ''}${detalle.barrio ? ` — ${detalle.barrio}` : ''}`
                  : '—'
              }
            />
            <CampoLectura label="Provincia" valor={detalle.provinciaDomicilio?.nombre ?? '—'} />
            <CampoLectura label="Estado civil" valor={detalle.estadoCivil?.nombre ?? '—'} />
            <CampoLectura label="Educación" valor={detalle.educacion?.nombre ?? '—'} />
            <CampoLectura label="Nacionalidad" valor={detalle.nacionalidad?.nombre ?? '—'} />
          </div>
        ) : (
          form && (
            <form
              className="flex flex-col gap-3"
              onSubmit={(e) => {
                e.preventDefault()
                guardar.mutate()
              }}
            >
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Email</span>
                <input
                  value={form.email}
                  onChange={(ev) => setForm({ ...form, email: ev.target.value })}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
              <label className="flex flex-col gap-1 text-sm">
                <span className="text-graphite-600">Calle principal</span>
                <input
                  value={form.callePrincipal}
                  onChange={(ev) => setForm({ ...form, callePrincipal: ev.target.value })}
                  className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                />
              </label>
              <div className="grid grid-cols-2 gap-3">
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-graphite-600">Número</span>
                  <input
                    value={form.numeroCasa}
                    onChange={(ev) => setForm({ ...form, numeroCasa: ev.target.value })}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-graphite-600">Barrio</span>
                  <input
                    value={form.barrio}
                    onChange={(ev) => setForm({ ...form, barrio: ev.target.value })}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                  />
                </label>
              </div>
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-graphite-600">Provincia</span>
                  <select
                    value={form.codigoProvinciaDomicilio}
                    onChange={(ev) => setForm({ ...form, codigoProvinciaDomicilio: ev.target.value })}
                    className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
                  >
                    <option value="">Seleccionar…</option>
                    {provincias?.map((p) => (
                      <option key={p.codigo} value={p.codigo}>
                        {p.nombre}
                      </option>
                    ))}
                  </select>
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-graphite-600">Estado civil</span>
                  <CatalogoSelectNomina
                    endpoint="estados-civiles"
                    value={form.codigoEstadoCivil}
                    onChange={(v) => setForm({ ...form, codigoEstadoCivil: v })}
                    placeholder="Seleccionar…"
                  />
                </label>
                <label className="flex flex-col gap-1 text-sm">
                  <span className="text-graphite-600">Educación</span>
                  <CatalogoSelectNomina
                    endpoint="educacion"
                    value={form.codigoEducacion}
                    onChange={(v) => setForm({ ...form, codigoEducacion: v })}
                    placeholder="Seleccionar…"
                  />
                </label>
              </div>
              <label className="flex max-w-xs flex-col gap-1 text-sm">
                <span className="text-graphite-600">Nacionalidad</span>
                <CatalogoSelectNomina
                  endpoint="nacionalidades"
                  value={form.codigoNacionalidad}
                  onChange={(v) => setForm({ ...form, codigoNacionalidad: v })}
                  placeholder="Seleccionar…"
                />
              </label>
              <div className="flex items-center gap-3">
                <button
                  type="submit"
                  disabled={guardar.isPending}
                  className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
                >
                  Guardar
                </button>
                <button type="button" onClick={() => setEditando(false)} className="text-sm text-graphite-600 hover:text-graphite-100">
                  Cancelar
                </button>
              </div>
              {guardar.isError && <p className="text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo guardar.')}</p>}
            </form>
          )
        )}
      </div>

      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-3 text-sm font-semibold text-graphite-100">Teléfonos</h3>
        <div className="mb-3 flex flex-col">
          {detalle.telefonos.length === 0 && <p className="text-sm text-graphite-600">Sin teléfonos registrados.</p>}
          {detalle.telefonos.map((t) => (
            <div key={t.id} className="flex items-center justify-between border-b border-black/[0.04] py-2 text-sm last:border-0">
              <span className="flex items-center gap-2">
                {t.telefono} {t.esPrincipal && <Badge variant="exito">Principal</Badge>}
              </span>
              <button type="button" onClick={() => quitarTelefono.mutate(t.id)} className="text-graphite-600 hover:text-red-700">
                <X size={14} />
              </button>
            </div>
          ))}
        </div>
        <form
          className="flex items-center gap-2"
          onSubmit={(e) => {
            e.preventDefault()
            if (!telefonoNuevo) return
            agregarTelefono.mutate()
          }}
        >
          <input
            placeholder="Nuevo teléfono"
            value={telefonoNuevo}
            onChange={(e) => setTelefonoNuevo(e.target.value)}
            className="flex-1 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
          <button
            type="submit"
            disabled={agregarTelefono.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            + Agregar
          </button>
        </form>
      </div>
    </div>
  )
}

function TabDatosYCargo({
  empleado,
  cargos,
  agencias,
  onGuardado,
}: {
  empleado: Empleado
  cargos: CargoItem[]
  agencias: AgenciaItem[]
  onGuardado: () => void
}) {
  const queryClient = useQueryClient()
  const [idCargo, setIdCargo] = useState(String(empleado.idCargo))
  const [idAgencia, setIdAgencia] = useState(String(empleado.idAgencia))
  const [recibeFondosReserva, setRecibeFondosReserva] = useState(empleado.recibeFondosReserva)
  const [estado, setEstado] = useState(empleado.estado)

  const guardar = useMutation({
    mutationFn: async () =>
      (
        await api.put(`/api/nomina/empleados/${empleado.id}`, {
          idAgencia: Number(idAgencia),
          idCargo: Number(idCargo),
          recibeFondosReserva,
          estado,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-empleados'] })
      onGuardado()
    },
  })

  return (
    <div className="flex flex-col gap-6">
      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-3 text-sm font-semibold text-graphite-100">Identidad</h3>
        <div className="flex flex-col">
          <CampoLectura label="Nombre" valor={empleado.nombre} />
          <CampoLectura label="Identificación" valor={empleado.identificacion} />
          <CampoLectura label="Sueldo actual" valor={empleado.sueldoActual !== null ? formatoUsd(empleado.sueldoActual) : '—'} />
        </div>
      </div>

      <div className="glass-card rounded-xl p-4">
        <h3 className="mb-3 text-sm font-semibold text-graphite-100">Cargo y estado</h3>
        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            guardar.mutate()
          }}
        >
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Cargo</span>
              <select
                value={idCargo}
                onChange={(e) => setIdCargo(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              >
                {cargos.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nombre}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Agencia</span>
              <select
                value={idAgencia}
                onChange={(e) => setIdAgencia(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              >
                {agencias.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.nombre}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Estado</span>
              <select
                value={estado}
                onChange={(e) => setEstado(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              >
                {ESTADOS_EMPLEADO.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <label className="flex items-center gap-2 text-sm text-graphite-600">
            <input
              type="checkbox"
              checked={recibeFondosReserva}
              onChange={(e) => setRecibeFondosReserva(e.target.checked)}
              className="rounded border-black/[0.2]"
            />
            Recibe fondos de reserva
          </label>

          <div>
            <button
              type="submit"
              disabled={guardar.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {guardar.isPending ? 'Guardando…' : 'Guardar cambios'}
            </button>
          </div>

          {guardar.isError && (
            <p className="text-sm text-red-700">{mensajeError(guardar.error, 'No se pudo actualizar el empleado.')}</p>
          )}
        </form>
      </div>
    </div>
  )
}

const TABS_GESTION_EMPLEADO = [
  { id: 'cargo', label: 'Datos y cargo', icon: Briefcase },
  { id: 'personales', label: 'Datos personales', icon: IdCard },
  { id: 'contratos', label: 'Contratos', icon: FileText },
  { id: 'iess', label: 'IESS / Min. Trabajo', icon: ShieldCheck },
  { id: 'renta', label: 'Impuesto a la Renta', icon: Receipt },
] as const

type TabGestionEmpleadoId = (typeof TABS_GESTION_EMPLEADO)[number]['id']

function GestionarEmpleadoModal({
  empleado,
  cargos,
  agencias,
  onClose,
  tabInicial = 'cargo',
  recienCreado = false,
}: {
  empleado: Empleado
  cargos: CargoItem[]
  agencias: AgenciaItem[]
  onClose: () => void
  tabInicial?: TabGestionEmpleadoId
  recienCreado?: boolean
}) {
  const [tab, setTab] = useState<TabGestionEmpleadoId>(tabInicial)

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card flex max-h-[85vh] w-full max-w-4xl flex-col overflow-hidden rounded-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-black/[0.06] p-5 pb-4">
          <h2 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
            <IdCard size={16} /> {recienCreado ? `Empleado ${empleado.nombre} creado — completá su información` : `Gestionar ${empleado.nombre}`}
          </h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        {recienCreado && (
          <p className="border-b border-black/[0.06] bg-gold-500/5 px-5 py-2 text-xs text-graphite-600">
            Ya quedó registrado como empleado activo — completá acá su domicilio, contrato, datos de IESS y lo que
            haga falta.
          </p>
        )}

        <div className="flex flex-1 overflow-hidden">
          <div className="flex w-52 flex-shrink-0 flex-col gap-0.5 border-r border-black/[0.06] p-2">
            {TABS_GESTION_EMPLEADO.map((t) => (
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
            {tab === 'cargo' && (
              <TabDatosYCargo empleado={empleado} cargos={cargos} agencias={agencias} onGuardado={() => {}} />
            )}
            {tab === 'personales' && <SeccionDatosPersona idEmpleado={empleado.id} />}
            {tab === 'contratos' && <SeccionContratos idEmpleado={empleado.id} />}
            {tab === 'iess' && <SeccionDatosAdicionales idEmpleado={empleado.id} />}
            {tab === 'renta' && <SeccionImpuestoRenta idEmpleado={empleado.id} />}
          </div>
        </div>
      </div>
    </div>
  )
}

function SeccionEmpleados() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [idGestionar, setIdGestionar] = useState<string | null>(null)
  const [recienCreado, setRecienCreado] = useState(false)

  const { data: empleados, isLoading } = useQuery<Empleado[]>({
    queryKey: ['nomina-empleados'],
    queryFn: async () => (await api.get('/api/nomina/empleados')).data,
  })

  const { data: cargos } = useQuery<CargoItem[]>({
    queryKey: ['nomina-cargos'],
    queryFn: async () => (await api.get('/api/nomina/cargos')).data,
  })

  const { data: agencias } = useQuery<AgenciaItem[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const empleadoGestionar = empleados?.find((e) => e.id === idGestionar) ?? null

  return (
    <div>
      <div className="mb-4 flex justify-end">
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <UserPlus size={16} /> Nuevo empleado
          </button>
        )}
      </div>

      {mostrarForm && cargos && agencias && (
        <NuevoEmpleadoForm
          cargos={cargos}
          agencias={agencias}
          onClose={() => setMostrarForm(false)}
          onCreado={(idEmpleado) => {
            setMostrarForm(false)
            setRecienCreado(true)
            setIdGestionar(idEmpleado)
          }}
        />
      )}

      {empleadoGestionar && cargos && agencias && (
        <ModalPortal>
          <GestionarEmpleadoModal
            empleado={empleadoGestionar}
            cargos={cargos}
            agencias={agencias}
            recienCreado={recienCreado}
            tabInicial={recienCreado ? 'personales' : 'cargo'}
            onClose={() => {
              setIdGestionar(null)
              setRecienCreado(false)
            }}
          />
        </ModalPortal>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Nombre</Th>
            <Th>Identificación</Th>
            <Th>Cargo</Th>
            <Th>Agencia</Th>
            <Th>Sueldo actual</Th>
            <Th>Fecha ingreso</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (empleados?.length ?? 0) === 0 && <EmptyState>Todavía no hay empleados registrados</EmptyState>}
          {empleados?.map((e) => (
            <tr
              key={e.id}
              onDoubleClick={() => setIdGestionar(e.id)}
              title="Doble clic para gestionar"
              className="cursor-pointer border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]"
            >
              <Td className="font-medium">{e.nombre}</Td>
              <Td>{e.identificacion}</Td>
              <Td>{e.cargo}</Td>
              <Td>{e.agencia}</Td>
              <Td>{e.sueldoActual !== null ? formatoUsd(e.sueldoActual) : '—'}</Td>
              <Td>{e.fechaIngreso}</Td>
              <Td>{badgeEstadoEmpleado(e.estado)}</Td>
              <Td>
                <button
                  type="button"
                  onClick={() => setIdGestionar(e.id)}
                  className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] px-3 py-1.5 text-xs font-medium text-graphite-100 hover:bg-black/[0.03]"
                >
                  <IdCard size={13} /> Gestionar
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

const BENEFICIOS = [
  { slug: 'decimo-tercero', nombre: 'Décimo Tercero', icon: Gift },
  { slug: 'decimo-cuarto', nombre: 'Décimo Cuarto', icon: Calendar },
  { slug: 'fondos-reserva', nombre: 'Fondos de Reserva', icon: PiggyBank },
  { slug: 'provision-vacaciones', nombre: 'Provisión de Vacaciones', icon: TreePalm },
  { slug: 'aporte-patronal', nombre: 'Aporte Patronal IESS', icon: ShieldCheck },
] as const

function TablaBeneficio({ slug, nombre }: { slug: string; nombre: string }) {
  const queryClient = useQueryClient()

  const { data, isLoading } = useQuery<BeneficioEmpleadoItem[]>({
    queryKey: ['nomina-beneficio', slug],
    queryFn: async () => (await api.get(`/api/nomina/beneficios/${slug}`)).data,
  })

  const pagar = useMutation({
    mutationFn: async (idEmpleado: string) =>
      (await api.post(`/api/nomina/beneficios/${slug}/pagar`, { idEmpleado })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-beneficio', slug] })
    },
  })

  return (
    <TableContainer>
      <thead>
        <tr>
          <Th>Empleado</Th>
          <Th>Acumulado</Th>
          <Th>Pagado</Th>
          <Th>Pendiente</Th>
          <Th>Último devengo</Th>
          <Th></Th>
        </tr>
      </thead>
      <tbody>
        {isLoading && <EmptyState>Cargando…</EmptyState>}
        {!isLoading && (data?.length ?? 0) === 0 && (
          <EmptyState>Sin registros de {nombre.toLowerCase()} todavía — ejecuta el devengo mensual primero</EmptyState>
        )}
        {data?.map((b) => (
          <tr key={b.idEmpleado} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
            <Td className="font-medium">{b.nombre}</Td>
            <Td>{formatoUsd(b.acumulado)}</Td>
            <Td>{formatoUsd(b.pagado)}</Td>
            <Td>
              {b.pendiente > 0 ? (
                <Badge variant="alerta">{formatoUsd(b.pendiente)}</Badge>
              ) : (
                <Badge variant="neutral">$0.00</Badge>
              )}
            </Td>
            <Td>{b.ultimoDevengo ?? '—'}</Td>
            <Td>
              {b.pendiente > 0 && (
                <button
                  type="button"
                  disabled={pagar.isPending}
                  onClick={() => pagar.mutate(b.idEmpleado)}
                  className="rounded-lg border border-black/[0.08] px-2.5 py-1 text-xs font-medium text-graphite-100 hover:bg-black/[0.03] disabled:opacity-60"
                >
                  Pagar
                </button>
              )}
            </Td>
          </tr>
        ))}
      </tbody>
    </TableContainer>
  )
}

function ParametroSbuCard() {
  const queryClient = useQueryClient()
  const [editando, setEditando] = useState(false)
  const [valor, setValor] = useState('')

  const { data: parametro } = useQuery<ParametroNomina>({
    queryKey: ['nomina-parametro'],
    queryFn: async () => (await api.get('/api/nomina/parametro')).data,
  })

  const guardar = useMutation({
    mutationFn: async () => (await api.put('/api/nomina/parametro', { salarioBasicoUnificado: Number(valor) })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-parametro'] })
      setEditando(false)
    },
  })

  return (
    <div className="glass-card mb-4 flex items-center justify-between rounded-xl p-4">
      <div>
        <p className="text-xs text-graphite-600">Salario Básico Unificado vigente (base de Décimo Cuarto)</p>
        {editando ? (
          <form
            className="mt-1 flex items-center gap-1.5"
            onSubmit={(e) => {
              e.preventDefault()
              guardar.mutate()
            }}
          >
            <span className="text-sm text-graphite-600">$</span>
            <input
              autoFocus
              type="number"
              step="0.01"
              min="0"
              value={valor}
              onChange={(e) => setValor(e.target.value)}
              className="w-24 rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
            <button type="submit" disabled={guardar.isPending} className="text-xs font-medium text-petrol-700 hover:underline">
              Guardar
            </button>
            <button type="button" onClick={() => setEditando(false)} className="text-graphite-600 hover:text-graphite-100">
              <X size={14} />
            </button>
          </form>
        ) : (
          <p className="mt-1 text-xl font-semibold text-graphite-100">
            {parametro ? formatoUsd(parametro.salarioBasicoUnificado) : '—'}
          </p>
        )}
        {guardar.isError && <p className="mt-1 text-xs text-red-700">{mensajeError(guardar.error, 'No se pudo actualizar.')}</p>}
      </div>
      {!editando && parametro && (
        <button
          type="button"
          onClick={() => {
            setValor(String(parametro.salarioBasicoUnificado))
            setEditando(true)
          }}
          className="text-graphite-600 hover:text-petrol-700"
        >
          <Pencil size={15} />
        </button>
      )}
    </div>
  )
}

function SeccionBeneficios() {
  const [activo, setActivo] = useState<(typeof BENEFICIOS)[number]['slug']>('decimo-tercero')
  const queryClient = useQueryClient()

  const devengo = useMutation({
    mutationFn: async () => (await api.post('/api/nomina/beneficios/devengo')).data as DevengoBeneficiosResult,
    onSuccess: () => {
      BENEFICIOS.forEach((b) => queryClient.invalidateQueries({ queryKey: ['nomina-beneficio', b.slug] }))
    },
  })

  return (
    <div>
      <ParametroSbuCard />

      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap gap-2">
          {BENEFICIOS.map((b) => (
            <button
              key={b.slug}
              type="button"
              onClick={() => setActivo(b.slug)}
              className={`flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                activo === b.slug ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.03]'
              }`}
            >
              <b.icon size={15} /> {b.nombre}
            </button>
          ))}
        </div>

        <button
          type="button"
          disabled={devengo.isPending}
          onClick={() => devengo.mutate()}
          className="btn-hover rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {devengo.isPending ? 'Ejecutando…' : 'Ejecutar devengo del mes'}
        </button>
      </div>

      {devengo.isSuccess && (
        <p className="mb-4 rounded-lg border border-black/[0.06] bg-white/60 p-3 text-sm text-graphite-600">
          Devengo ejecutado sobre {devengo.data.empleadosProcesados} empleado(s) — Décimo Tercero{' '}
          {formatoUsd(devengo.data.totalDecimoTercero)}, Décimo Cuarto {formatoUsd(devengo.data.totalDecimoCuarto)}, Fondos
          de Reserva {formatoUsd(devengo.data.totalFondosReserva)}, Provisión de Vacaciones{' '}
          {formatoUsd(devengo.data.totalProvisionVacaciones)}, Aporte Patronal IESS{' '}
          {formatoUsd(devengo.data.totalAportePatronal)}.
        </p>
      )}
      {devengo.isError && <p className="mb-4 text-sm text-red-700">{mensajeError(devengo.error, 'No se pudo ejecutar el devengo.')}</p>}

      <TablaBeneficio slug={activo} nombre={BENEFICIOS.find((b) => b.slug === activo)!.nombre} />
    </div>
  )
}

interface TipoAccionPersonal {
  id: number
  detalle: string
  activaContrato: boolean
  desactivaContrato: boolean
  esCambioCargoSueldo: boolean
  esCambioAgenciaDepartamento: boolean
  esFormaPagoDecimos: boolean
  esFormaPagoFondosReserva: boolean
}

interface SolicitudAccionPersonalEtapa {
  codigoEstado: string
  comentario: string | null
  fecha: string
  registradoPor: string
}

interface SolicitudAccionPersonal {
  id: string
  numeroAccion: number
  idEmpleado: string
  empleado: string
  idTipoAccionPersonal: number
  tipoAccionPersonal: string
  detalle: string
  codigoEstado: string
  estado: string
  fecha: string
  idCargoNuevo: number | null
  nuevoSueldo: number | null
  idAgenciaNueva: number | null
  nuevoValorRecibeFondosReserva: boolean | null
  etapas: SolicitudAccionPersonalEtapa[]
}

function badgeEstadoAccion(codigoEstado: string) {
  if (codigoEstado === 'AP') return <Badge variant="exito">Aprobada</Badge>
  if (codigoEstado === 'AN') return <Badge variant="peligro">Anulada</Badge>
  return <Badge variant="alerta">Ingresada</Badge>
}

function NuevaAccionPersonalForm({
  empleados,
  tipos,
  cargos,
  agencias,
  onClose,
}: {
  empleados: Empleado[]
  tipos: TipoAccionPersonal[]
  cargos: CargoItem[]
  agencias: AgenciaItem[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idEmpleado, setIdEmpleado] = useState('')
  const [idTipo, setIdTipo] = useState('')
  const [detalle, setDetalle] = useState('')
  const [idCargoNuevo, setIdCargoNuevo] = useState('')
  const [nuevoSueldo, setNuevoSueldo] = useState('')
  const [idAgenciaNueva, setIdAgenciaNueva] = useState('')
  const [nuevoValorRecibeFondosReserva, setNuevoValorRecibeFondosReserva] = useState(true)

  const tipo = tipos.find((t) => String(t.id) === idTipo)

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/nomina/acciones-personal', {
          idEmpleado,
          idTipoAccionPersonal: Number(idTipo),
          detalle,
          idCargoNuevo: tipo?.esCambioCargoSueldo ? Number(idCargoNuevo) : null,
          nuevoSueldo: tipo?.esCambioCargoSueldo && nuevoSueldo ? Number(nuevoSueldo) : null,
          idAgenciaNueva: tipo?.esCambioAgenciaDepartamento ? Number(idAgenciaNueva) : null,
          nuevoValorRecibeFondosReserva: tipo?.esFormaPagoFondosReserva ? nuevoValorRecibeFondosReserva : null,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-acciones-personal'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nueva acción de personal</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="flex flex-col gap-4"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idEmpleado || !idTipo || !detalle) return
          crear.mutate()
        }}
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Empleado</span>
            <select
              required
              value={idEmpleado}
              onChange={(e) => setIdEmpleado(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {empleados.map((emp) => (
                <option key={emp.id} value={emp.id}>
                  {emp.nombre} — {emp.cargo}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Tipo de acción</span>
            <select
              required
              value={idTipo}
              onChange={(e) => setIdTipo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {tipos.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.detalle}
                </option>
              ))}
            </select>
          </label>
        </div>

        {tipo?.esCambioCargoSueldo && (
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Cargo nuevo</span>
            <select
              required
              value={idCargoNuevo}
              onChange={(e) => setIdCargoNuevo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {cargos.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </select>
          </label>
        )}

        {tipo?.esCambioCargoSueldo && (
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Nuevo sueldo mensual (opcional)</span>
            <input
              type="number"
              step="0.01"
              min="0"
              placeholder="Dejar vacío si no cambia el sueldo"
              value={nuevoSueldo}
              onChange={(e) => setNuevoSueldo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
        )}

        {tipo?.esCambioAgenciaDepartamento && (
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Agencia nueva</span>
            <select
              required
              value={idAgenciaNueva}
              onChange={(e) => setIdAgenciaNueva(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {agencias.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.nombre}
                </option>
              ))}
            </select>
          </label>
        )}

        {tipo?.esFormaPagoFondosReserva && (
          <label className="flex items-center gap-2 text-sm text-graphite-600">
            <input
              type="checkbox"
              checked={nuevoValorRecibeFondosReserva}
              onChange={(e) => setNuevoValorRecibeFondosReserva(e.target.checked)}
              className="rounded border-black/[0.2]"
            />
            El empleado pasa a recibir fondos de reserva
          </label>
        )}

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Detalle</span>
          <textarea
            required
            rows={2}
            value={detalle}
            onChange={(e) => setDetalle(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-center gap-2">
          <button
            type="submit"
            disabled={crear.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {crear.isPending ? 'Guardando…' : 'Ingresar solicitud'}
          </button>
        </div>

        {crear.isError && <p className="text-sm text-red-700">{mensajeError(crear.error, 'No se pudo crear la solicitud.')}</p>}
      </form>
    </div>
  )
}

function FilaAccionPersonal({ solicitud }: { solicitud: SolicitudAccionPersonal }) {
  const queryClient = useQueryClient()
  const [expandido, setExpandido] = useState(false)
  const [motivoAnular, setMotivoAnular] = useState('')
  const [mostrarAnular, setMostrarAnular] = useState(false)

  const aprobar = useMutation({
    mutationFn: async () => (await api.post(`/api/nomina/acciones-personal/${solicitud.id}/aprobar`, { comentario: 'Aprobado' })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-acciones-personal'] })
      queryClient.invalidateQueries({ queryKey: ['nomina-empleados'] })
    },
  })

  const anular = useMutation({
    mutationFn: async () => (await api.post(`/api/nomina/acciones-personal/${solicitud.id}/anular`, { motivo: motivoAnular })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['nomina-acciones-personal'] })
      setMostrarAnular(false)
      setMotivoAnular('')
    },
  })

  return (
    <>
      <tr className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
        <Td className="font-medium">#{solicitud.numeroAccion}</Td>
        <Td>{solicitud.empleado}</Td>
        <Td>{solicitud.tipoAccionPersonal}</Td>
        <Td className="max-w-xs truncate" title={solicitud.detalle}>
          {solicitud.detalle}
          {solicitud.nuevoSueldo !== null && (
            <span className="ml-1 text-graphite-600">→ {formatoUsd(solicitud.nuevoSueldo)}</span>
          )}
        </Td>
        <Td>{solicitud.fecha}</Td>
        <Td>{badgeEstadoAccion(solicitud.codigoEstado)}</Td>
        <Td>
          <div className="flex flex-wrap items-center gap-2">
            <button type="button" onClick={() => setExpandido((v) => !v)} className="text-xs font-medium text-petrol-700 hover:underline">
              {expandido ? 'Ocultar' : 'Bitácora'}
            </button>
            {solicitud.codigoEstado === 'IN' && (
              <>
                <button
                  type="button"
                  disabled={aprobar.isPending}
                  onClick={() => aprobar.mutate()}
                  className="rounded-lg bg-gold-500 px-2.5 py-1 text-xs font-medium text-white disabled:opacity-60"
                >
                  Aprobar
                </button>
                <button
                  type="button"
                  onClick={() => setMostrarAnular((v) => !v)}
                  className="rounded-lg border border-black/[0.08] px-2.5 py-1 text-xs font-medium text-graphite-100 hover:bg-black/[0.03]"
                >
                  Anular
                </button>
              </>
            )}
          </div>
          {aprobar.isError && <p className="mt-1 text-xs text-red-700">{mensajeError(aprobar.error, 'No se pudo aprobar.')}</p>}
        </Td>
      </tr>
      {mostrarAnular && (
        <tr className="border-b border-black/[0.04] bg-black/[0.015] last:border-0">
          <Td colSpan={7}>
            <form
              className="flex items-center gap-2 py-1"
              onSubmit={(e) => {
                e.preventDefault()
                if (!motivoAnular) return
                anular.mutate()
              }}
            >
              <input
                required
                value={motivoAnular}
                onChange={(e) => setMotivoAnular(e.target.value)}
                placeholder="Motivo de la anulación…"
                className="flex-1 rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
              <button type="submit" disabled={anular.isPending} className="rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white">
                Confirmar anulación
              </button>
              <button type="button" onClick={() => setMostrarAnular(false)} className="text-xs text-graphite-600 hover:text-graphite-100">
                Cancelar
              </button>
            </form>
            {anular.isError && <p className="mt-1 text-xs text-red-700">{mensajeError(anular.error, 'No se pudo anular.')}</p>}
          </Td>
        </tr>
      )}
      {expandido && (
        <tr className="border-b border-black/[0.04] bg-black/[0.015] last:border-0">
          <Td colSpan={7}>
            <ul className="flex flex-col gap-1 py-1 text-xs text-graphite-600">
              {solicitud.etapas.map((et, idx) => (
                <li key={idx}>
                  <span className="font-medium text-graphite-100">{et.codigoEstado === 'IN' ? 'Ingresada' : et.codigoEstado === 'AP' ? 'Aprobada' : 'Anulada'}</span>{' '}
                  — {new Date(et.fecha).toLocaleString('es-EC')} por {et.registradoPor}
                  {et.comentario ? ` — ${et.comentario}` : ''}
                </li>
              ))}
            </ul>
          </Td>
        </tr>
      )}
    </>
  )
}

function SeccionAccionPersonal() {
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: empleados } = useQuery<Empleado[]>({
    queryKey: ['nomina-empleados'],
    queryFn: async () => (await api.get('/api/nomina/empleados')).data,
  })

  const { data: tipos } = useQuery<TipoAccionPersonal[]>({
    queryKey: ['nomina-tipos-accion-personal'],
    queryFn: async () => (await api.get('/api/nomina/tipos-accion-personal')).data,
  })

  const { data: cargos } = useQuery<CargoItem[]>({
    queryKey: ['nomina-cargos'],
    queryFn: async () => (await api.get('/api/nomina/cargos')).data,
  })

  const { data: agencias } = useQuery<AgenciaItem[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const { data: solicitudes, isLoading } = useQuery<SolicitudAccionPersonal[]>({
    queryKey: ['nomina-acciones-personal'],
    queryFn: async () => (await api.get('/api/nomina/acciones-personal')).data,
  })

  return (
    <div>
      <div className="mb-4 flex justify-end">
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <Plus size={16} /> Nueva acción de personal
          </button>
        )}
      </div>

      {mostrarForm && empleados && tipos && cargos && agencias && (
        <NuevaAccionPersonalForm
          empleados={empleados}
          tipos={tipos}
          cargos={cargos}
          agencias={agencias}
          onClose={() => setMostrarForm(false)}
        />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Empleado</Th>
            <Th>Tipo</Th>
            <Th>Detalle</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (solicitudes?.length ?? 0) === 0 && <EmptyState>Todavía no hay acciones de personal registradas</EmptyState>}
          {solicitudes?.map((s) => (
            <FilaAccionPersonal key={s.id} solicitud={s} />
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

export function Nomina() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [tab, setTab] = useState<'roles' | 'beneficios' | 'empleados' | 'accion-personal'>('roles')

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
        subtitle="Empleados, roles de pago y beneficios sociales"
        actions={
          tab === 'roles' &&
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

      <div className="mb-4 flex gap-2">
        <button
          type="button"
          onClick={() => setTab('empleados')}
          className={`flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
            tab === 'empleados' ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.03]'
          }`}
        >
          <Users size={15} /> Empleados
        </button>
        <button
          type="button"
          onClick={() => setTab('roles')}
          className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
            tab === 'roles' ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.03]'
          }`}
        >
          Roles de pago
        </button>
        <button
          type="button"
          onClick={() => setTab('beneficios')}
          className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
            tab === 'beneficios' ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.03]'
          }`}
        >
          Beneficios sociales
        </button>
        <button
          type="button"
          onClick={() => setTab('accion-personal')}
          className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
            tab === 'accion-personal' ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.03]'
          }`}
        >
          Acción de personal
        </button>
      </div>

      {tab === 'empleados' && <SeccionEmpleados />}

      {tab === 'roles' && (
        <>
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
        </>
      )}

      {tab === 'beneficios' && <SeccionBeneficios />}

      {tab === 'accion-personal' && <SeccionAccionPersonal />}
    </div>
  )
}
