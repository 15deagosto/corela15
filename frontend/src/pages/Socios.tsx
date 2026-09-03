import { Fragment, useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Users, ShieldAlert, IdCard, Plus, X, MessageSquareWarning, Landmark } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { api } from '../lib/api'

interface Socio {
  id: string
  idPersona: string
  numero: string
  nombre: string
  identificacion: string
  agencia: string
  estado: string
  codigoProvinciaDomicilio: string | null
  esPersonaNatural: boolean
}

interface TipoIdentificacion {
  id: number
  codigo: string
  nombre: string
}

interface Agencia {
  id: number
  nombre: string
  activa: boolean
}

interface Provincia {
  codigo: string
  nombre: string
}

interface CatalogoItem {
  codigo: string
  nombre: string
  activo: boolean
}

interface SocioPerfilDetalle {
  activos: number | null
  pasivos: number | null
  ingresos: number | null
  egresos: number | null
  esPep: boolean | null
  codigoEstadoCivil: string | null
  codigoEducacion: string | null
  codigoVivienda: string | null
  codigoSectorVivienda: string | null
  codigoNacionalidad: string | null
  codigoProfesion: string | null
  idActividadEconomica: number | null
  nombreActividadEconomica: string | null
  cobraBonoDesarrolloHumano: boolean | null
  esSeparacionDeBienes: boolean | null
  tieneDiscapacidad: boolean | null
  tieneCargasFamiliares: boolean | null
  numeroCargasFamiliares: number | null
  calleSecundaria: string | null
  codigoPostal: string | null
  referencia: string | null
  parentescoServicioBasico: string | null
  codigoCausaVinculacion: string | null
  codigoCalificacionInterna: string | null
  codigoSectorEconomico: string | null
  idUsuarioOficial: string | null
  nombreUsuarioOficial: string | null
  esExento: boolean | null
}

interface UsuarioParaOficial {
  id: string
  nombreUsuario: string
}

interface ActividadEconomicaBusqueda {
  id: number
  codigo: string
  nombre: string
  nivel: string
}

function ProfesionSelect({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const { data } = useQuery<CatalogoItem[]>({
    queryKey: ['socios-profesiones'],
    queryFn: async () => (await api.get('/api/socios/profesiones')).data,
  })
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
    >
      <option value="">Profesión…</option>
      {data?.map((c) => (
        <option key={c.codigo} value={c.codigo}>
          {c.nombre}
        </option>
      ))}
    </select>
  )
}

function ActividadEconomicaSelect({
  value,
  nombreActual,
  onChange,
}: {
  value: number | null
  nombreActual: string | null
  onChange: (id: number | null, nombre: string | null) => void
}) {
  const [q, setQ] = useState('')

  const { data: resultados } = useQuery<ActividadEconomicaBusqueda[]>({
    queryKey: ['socios-actividades-economicas', q],
    queryFn: async () => (await api.get('/api/socios/actividades-economicas', { params: { q } })).data,
    enabled: q.length >= 3,
  })

  if (value && !q) {
    return (
      <div className="flex items-center gap-1.5 rounded border border-black/[0.08] px-2 py-1 text-xs">
        <span className="truncate">{nombreActual ?? `Actividad #${value}`}</span>
        <button type="button" onClick={() => onChange(null, null)} className="text-graphite-600 hover:text-graphite-100">
          <X size={12} />
        </button>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-1">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar actividad económica (CIIU) por código o nombre…"
        className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
      />
      {q.length >= 3 && (resultados?.length ?? 0) > 0 && (
        <div className="max-h-32 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
          {resultados?.map((a) => (
            <button
              key={a.id}
              type="button"
              onClick={() => {
                onChange(a.id, a.nombre)
                setQ('')
              }}
              className="block w-full px-3 py-1.5 text-left text-xs hover:bg-black/[0.02]"
              title={a.nivel}
            >
              {a.codigo} — {a.nombre}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

function useCatalogo(endpoint: string) {
  return useQuery<CatalogoItem[]>({
    queryKey: ['catalogo', endpoint],
    queryFn: async () => (await api.get(`/api/configuracion/${endpoint}`)).data,
  })
}

function CatalogoSelect({
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
  const { data } = useCatalogo(endpoint)
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
    >
      <option value="">{placeholder}</option>
      {data?.filter((c) => c.activo || c.codigo === value).map((c) => (
        <option key={c.codigo} value={c.codigo}>
          {c.nombre}
        </option>
      ))}
    </select>
  )
}

interface PerfilLavado {
  id: string
  fecha: string
  patrimonio: number | null
  ingresoMensual: number | null
  bandaPatrimonio: number | null
  bandaIngreso: number | null
  totalPerfil: number | null
  categoria: number | null
}

interface Telefono {
  id: string
  telefono: string
  esTelefonoMovil: boolean
  esPrincipal: boolean
  notificacionSms: boolean
}

interface Conyuge {
  id: string
  idPersonaConyuge: string
  nombreConyuge: string
  identificacionConyuge: string
}

interface Representante {
  id: string
  idPersonaRepresentante: string
  nombreRepresentante: string
  identificacionRepresentante: string
  principal: boolean
  ejerceControl: boolean
}

interface PersonaBusqueda {
  id: string
  nombre: string
  identificacion: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function categoriaLabel(categoria: number | null) {
  if (categoria === 1) return 'Bajo'
  if (categoria === 2) return 'Medio'
  if (categoria === 3) return 'Alto'
  if (categoria === 4) return 'Muy alto'
  return 'Sin datos suficientes'
}

function categoriaVariant(categoria: number | null) {
  if (categoria === 1) return 'exito' as const
  if (categoria === 2) return 'alerta' as const
  if (categoria === 3 || categoria === 4) return 'peligro' as const
  return 'neutral' as const
}

function ProvinciaCelda({ socio }: { socio: Socio }) {
  const queryClient = useQueryClient()

  const { data: provincias } = useQuery<Provincia[]>({
    queryKey: ['socios-provincias'],
    queryFn: async () => (await api.get('/api/socios/provincias')).data,
  })

  const actualizar = useMutation({
    mutationFn: async (codigo: string) =>
      api.put(`/api/socios/${socio.id}/domicilio`, { codigoProvinciaDomicilio: codigo || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['socios'] })
    },
  })

  return (
    <select
      value={socio.codigoProvinciaDomicilio ?? ''}
      onChange={(e) => actualizar.mutate(e.target.value)}
      disabled={actualizar.isPending}
      className="rounded border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-50"
    >
      <option value="">Sin domicilio registrado</option>
      {provincias?.map((p) => (
        <option key={p.codigo} value={p.codigo}>
          {p.nombre}
        </option>
      ))}
    </select>
  )
}

function EstadoSocioCelda({ socio }: { socio: Socio }) {
  const queryClient = useQueryClient()

  const cambiar = useMutation({
    mutationFn: async (estado: string) => api.patch(`/api/socios/${socio.id}/estado`, { estado }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['socios'] }),
  })

  return (
    <select
      value={socio.estado}
      onChange={(e) => cambiar.mutate(e.target.value)}
      disabled={cambiar.isPending}
      className="rounded border border-black/[0.08] bg-white px-1.5 py-0.5 text-xs text-graphite-100 outline-none focus:border-gold-500/50 disabled:opacity-50"
    >
      <option value="Activo">Activo</option>
      <option value="Inactivo">Inactivo</option>
      <option value="Suspendido">Suspendido</option>
    </select>
  )
}

function SeccionDatosBasicos({ socio }: { socio: Socio }) {
  const queryClient = useQueryClient()
  const [nombre, setNombre] = useState('')
  const [apellido, setApellido] = useState('')
  const [activos, setActivos] = useState('')
  const [pasivos, setPasivos] = useState('')
  const [ingresos, setIngresos] = useState('')
  const [egresos, setEgresos] = useState('')
  const [esPep, setEsPep] = useState(false)
  const [estadoCivil, setEstadoCivil] = useState('')
  const [educacion, setEducacion] = useState('')
  const [vivienda, setVivienda] = useState('')
  const [sectorVivienda, setSectorVivienda] = useState('')
  const [nacionalidad, setNacionalidad] = useState('')
  const [profesion, setProfesion] = useState('')
  const [idActividadEconomica, setIdActividadEconomica] = useState<number | null>(null)
  const [nombreActividadEconomica, setNombreActividadEconomica] = useState<string | null>(null)
  const [causaVinculacion, setCausaVinculacion] = useState('')
  const [calificacionInterna, setCalificacionInterna] = useState('')
  const [sectorEconomico, setSectorEconomico] = useState('')
  const [cobraBonoDesarrolloHumano, setCobraBonoDesarrolloHumano] = useState(false)
  const [esSeparacionDeBienes, setEsSeparacionDeBienes] = useState(false)
  const [tieneDiscapacidad, setTieneDiscapacidad] = useState(false)
  const [tieneCargasFamiliares, setTieneCargasFamiliares] = useState(false)
  const [numeroCargasFamiliares, setNumeroCargasFamiliares] = useState('')
  const [calleSecundaria, setCalleSecundaria] = useState('')
  const [codigoPostal, setCodigoPostal] = useState('')
  const [referencia, setReferencia] = useState('')
  const [parentescoServicioBasico, setParentescoServicioBasico] = useState('')
  const [idUsuarioOficial, setIdUsuarioOficial] = useState('')
  const [esExento, setEsExento] = useState(false)
  const [editando, setEditando] = useState(false)

  const { data: perfil } = useQuery<SocioPerfilDetalle>({
    queryKey: ['socio-perfil', socio.idPersona],
    queryFn: async () => (await api.get(`/api/socios/personas/${socio.idPersona}/perfil`)).data,
    enabled: editando,
  })

  const { data: usuariosOficial } = useQuery<UsuarioParaOficial[]>({
    queryKey: ['socios-usuarios-oficial'],
    queryFn: async () => (await api.get('/api/socios/usuarios')).data,
    enabled: editando,
  })

  useEffect(() => {
    if (!perfil) return
    setActivos(perfil.activos?.toString() ?? '')
    setPasivos(perfil.pasivos?.toString() ?? '')
    setIngresos(perfil.ingresos?.toString() ?? '')
    setEgresos(perfil.egresos?.toString() ?? '')
    setEsPep(perfil.esPep ?? false)
    setEstadoCivil(perfil.codigoEstadoCivil ?? '')
    setEducacion(perfil.codigoEducacion ?? '')
    setVivienda(perfil.codigoVivienda ?? '')
    setSectorVivienda(perfil.codigoSectorVivienda ?? '')
    setNacionalidad(perfil.codigoNacionalidad ?? '')
    setProfesion(perfil.codigoProfesion ?? '')
    setIdActividadEconomica(perfil.idActividadEconomica ?? null)
    setNombreActividadEconomica(perfil.nombreActividadEconomica ?? null)
    setCausaVinculacion(perfil.codigoCausaVinculacion ?? '')
    setCalificacionInterna(perfil.codigoCalificacionInterna ?? '')
    setSectorEconomico(perfil.codigoSectorEconomico ?? '')
    setCobraBonoDesarrolloHumano(perfil.cobraBonoDesarrolloHumano ?? false)
    setEsSeparacionDeBienes(perfil.esSeparacionDeBienes ?? false)
    setTieneDiscapacidad(perfil.tieneDiscapacidad ?? false)
    setTieneCargasFamiliares(perfil.tieneCargasFamiliares ?? false)
    setNumeroCargasFamiliares(perfil.numeroCargasFamiliares?.toString() ?? '')
    setCalleSecundaria(perfil.calleSecundaria ?? '')
    setCodigoPostal(perfil.codigoPostal ?? '')
    setReferencia(perfil.referencia ?? '')
    setParentescoServicioBasico(perfil.parentescoServicioBasico ?? '')
    setIdUsuarioOficial(perfil.idUsuarioOficial ?? '')
    setEsExento(perfil.esExento ?? false)
  }, [perfil])

  const actualizar = useMutation({
    mutationFn: async () =>
      api.put(`/api/socios/personas/${socio.idPersona}`, {
        primerNombre: socio.esPersonaNatural ? nombre || undefined : undefined,
        apellidoPaterno: socio.esPersonaNatural ? apellido || undefined : undefined,
        razonSocial: !socio.esPersonaNatural ? nombre || undefined : undefined,
        esPep: socio.esPersonaNatural ? esPep : undefined,
        activos: activos === '' ? null : Number(activos),
        pasivos: pasivos === '' ? null : Number(pasivos),
        ingresos: ingresos === '' ? null : Number(ingresos),
        egresos: egresos === '' ? null : Number(egresos),
        codigoEstadoCivil: socio.esPersonaNatural ? estadoCivil || null : undefined,
        codigoEducacion: socio.esPersonaNatural ? educacion || null : undefined,
        codigoVivienda: socio.esPersonaNatural ? vivienda || null : undefined,
        codigoSectorVivienda: socio.esPersonaNatural ? sectorVivienda || null : undefined,
        codigoNacionalidad: socio.esPersonaNatural ? nacionalidad || null : undefined,
        codigoProfesion: socio.esPersonaNatural ? profesion || null : undefined,
        idActividadEconomica: idActividadEconomica,
        cobraBonoDesarrolloHumano: socio.esPersonaNatural ? cobraBonoDesarrolloHumano : undefined,
        esSeparacionDeBienes: socio.esPersonaNatural ? esSeparacionDeBienes : undefined,
        tieneDiscapacidad: socio.esPersonaNatural ? tieneDiscapacidad : undefined,
        tieneCargasFamiliares: socio.esPersonaNatural ? tieneCargasFamiliares : undefined,
        numeroCargasFamiliares: socio.esPersonaNatural && numeroCargasFamiliares !== '' ? Number(numeroCargasFamiliares) : undefined,
        calleSecundaria: calleSecundaria || null,
        codigoPostal: codigoPostal || null,
        referencia: referencia || null,
        parentescoServicioBasico: parentescoServicioBasico || null,
        codigoCausaVinculacion: causaVinculacion || null,
        codigoCalificacionInterna: calificacionInterna || null,
        codigoSectorEconomico: sectorEconomico || null,
        idUsuarioOficial: idUsuarioOficial || null,
        esExento,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['socios'] })
      queryClient.invalidateQueries({ queryKey: ['socio-perfil', socio.idPersona] })
      setEditando(false)
    },
  })

  if (!editando) {
    return (
      <div>
        <div className="mb-1 flex items-center justify-between">
          <p className="text-xs font-semibold uppercase tracking-wide text-graphite-600">Datos básicos y financieros</p>
          <button type="button" onClick={() => setEditando(true)} className="text-xs font-medium text-gold-400 hover:underline">
            Editar
          </button>
        </div>
        <p className="text-xs text-graphite-600">
          Nombre/razón social, datos financieros (score crediticio/LA-FT), y catálogos socioeconómicos reales (estado
          civil, educación, vivienda, nacionalidad, vinculación) — configurables desde Configuración.
        </p>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-2 rounded-lg border border-black/[0.08] bg-white p-2">
      <div className="mb-1 flex items-center justify-between">
        <p className="text-xs font-semibold uppercase tracking-wide text-graphite-600">Editar datos básicos</p>
        <button type="button" onClick={() => setEditando(false)} className="text-graphite-600 hover:text-graphite-100">
          <X size={14} />
        </button>
      </div>
      {socio.esPersonaNatural ? (
        <>
          <input
            value={nombre}
            onChange={(e) => setNombre(e.target.value)}
            placeholder="Primer nombre (dejar vacío para no cambiar)"
            className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
          />
          <input
            value={apellido}
            onChange={(e) => setApellido(e.target.value)}
            placeholder="Apellido paterno (dejar vacío para no cambiar)"
            className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
          />
          <label className="flex items-center gap-1 text-xs text-graphite-600">
            <input type="checkbox" checked={esPep} onChange={(e) => setEsPep(e.target.checked)} /> Es persona
            expuesta políticamente (PEP)
          </label>
        </>
      ) : (
        <input
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          placeholder="Razón social (dejar vacío para no cambiar)"
          className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
        />
      )}
      <div className="grid grid-cols-2 gap-2">
        <input
          value={activos}
          onChange={(e) => setActivos(e.target.value)}
          type="number"
          placeholder="Activos"
          className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
        />
        <input
          value={pasivos}
          onChange={(e) => setPasivos(e.target.value)}
          type="number"
          placeholder="Pasivos"
          className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
        />
        <input
          value={ingresos}
          onChange={(e) => setIngresos(e.target.value)}
          type="number"
          placeholder="Ingresos"
          className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
        />
        <input
          value={egresos}
          onChange={(e) => setEgresos(e.target.value)}
          type="number"
          placeholder="Egresos"
          className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
        />
      </div>

      {socio.esPersonaNatural && (
        <div className="grid grid-cols-2 gap-2 border-t border-black/[0.06] pt-2">
          <CatalogoSelect endpoint="estados-civiles" value={estadoCivil} onChange={setEstadoCivil} placeholder="Estado civil…" />
          <CatalogoSelect endpoint="educacion" value={educacion} onChange={setEducacion} placeholder="Educación…" />
          <CatalogoSelect endpoint="vivienda" value={vivienda} onChange={setVivienda} placeholder="Vivienda…" />
          <CatalogoSelect endpoint="sector-vivienda" value={sectorVivienda} onChange={setSectorVivienda} placeholder="Sector de vivienda…" />
          <CatalogoSelect endpoint="nacionalidades" value={nacionalidad} onChange={setNacionalidad} placeholder="Nacionalidad…" />
          <ProfesionSelect value={profesion} onChange={setProfesion} />
        </div>
      )}
      <div className="border-t border-black/[0.06] pt-2">
        <p className="mb-1 text-[11px] text-graphite-600">Actividad económica (CIIU)</p>
        <ActividadEconomicaSelect
          value={idActividadEconomica}
          nombreActual={nombreActividadEconomica}
          onChange={(id, nombre) => {
            setIdActividadEconomica(id)
            setNombreActividadEconomica(nombre)
          }}
        />
      </div>
      {socio.esPersonaNatural && (
        <div className="border-t border-black/[0.06] pt-2">
          <p className="mb-1 text-[11px] text-graphite-600">Datos socioeconómicos adicionales</p>
          <div className="grid grid-cols-2 gap-1.5">
            <label className="flex items-center gap-1.5 text-xs text-graphite-600">
              <input type="checkbox" checked={cobraBonoDesarrolloHumano} onChange={(e) => setCobraBonoDesarrolloHumano(e.target.checked)} />
              Cobra bono desarrollo humano
            </label>
            <label className="flex items-center gap-1.5 text-xs text-graphite-600">
              <input type="checkbox" checked={esSeparacionDeBienes} onChange={(e) => setEsSeparacionDeBienes(e.target.checked)} />
              Separación de bienes
            </label>
            <label className="flex items-center gap-1.5 text-xs text-graphite-600">
              <input type="checkbox" checked={tieneDiscapacidad} onChange={(e) => setTieneDiscapacidad(e.target.checked)} />
              Tiene discapacidad
            </label>
            <label className="flex items-center gap-1.5 text-xs text-graphite-600">
              <input type="checkbox" checked={tieneCargasFamiliares} onChange={(e) => setTieneCargasFamiliares(e.target.checked)} />
              Tiene cargas familiares
            </label>
            {tieneCargasFamiliares && (
              <input
                value={numeroCargasFamiliares}
                onChange={(e) => setNumeroCargasFamiliares(e.target.value)}
                type="number"
                min="0"
                placeholder="Número de cargas"
                className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
              />
            )}
          </div>
        </div>
      )}

      <div className="border-t border-black/[0.06] pt-2">
        <p className="mb-1 text-[11px] text-graphite-600">Domicilio y contacto adicional</p>
        <div className="grid grid-cols-2 gap-1.5">
          <input value={calleSecundaria} onChange={(e) => setCalleSecundaria(e.target.value)} placeholder="Calle secundaria"
            className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50" />
          <input value={codigoPostal} onChange={(e) => setCodigoPostal(e.target.value)} placeholder="Código postal"
            className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50" />
          <input value={referencia} onChange={(e) => setReferencia(e.target.value)} placeholder="Referencia de ubicación"
            className="col-span-2 rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50" />
          <input value={parentescoServicioBasico} onChange={(e) => setParentescoServicioBasico(e.target.value)} placeholder="Parentesco con titular de servicio básico"
            className="col-span-2 rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50" />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-2 border-t border-black/[0.06] pt-2">
        <CatalogoSelect endpoint="causas-vinculacion" value={causaVinculacion} onChange={setCausaVinculacion} placeholder="Causa de vinculación…" />
        <div className="grid grid-cols-2 gap-2">
          <CatalogoSelect endpoint="calificaciones-internas" value={calificacionInterna} onChange={setCalificacionInterna} placeholder="Calificación interna…" />
          <CatalogoSelect endpoint="sectores-economicos" value={sectorEconomico} onChange={setSectorEconomico} placeholder="Sector económico…" />
        </div>
        <select
          value={idUsuarioOficial}
          onChange={(e) => setIdUsuarioOficial(e.target.value)}
          className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
        >
          <option value="">Oficial responsable…</option>
          {usuariosOficial?.map((u) => (
            <option key={u.id} value={u.id}>
              {u.nombreUsuario}
            </option>
          ))}
        </select>
        <label className="flex items-center gap-1.5 text-xs text-graphite-600">
          <input type="checkbox" checked={esExento} onChange={(e) => setEsExento(e.target.checked)} />
          Exento de impuestos/comisiones
        </label>
      </div>

      <button
        type="button"
        disabled={actualizar.isPending}
        onClick={() => actualizar.mutate()}
        className="rounded bg-gold-500 px-3 py-1 text-xs font-medium text-white disabled:opacity-50"
      >
        Guardar
      </button>
    </div>
  )
}

function PerfilLavadoPanel({ idCliente }: { idCliente: string }) {
  const queryClient = useQueryClient()

  const { data: historial, isLoading } = useQuery<PerfilLavado[]>({
    queryKey: ['socios-perfil-lavado', idCliente],
    queryFn: async () => (await api.get(`/api/cobranzas/clientes/${idCliente}/perfil-lavado/historial`)).data,
  })

  const calcular = useMutation({
    mutationFn: async () => (await api.post(`/api/cobranzas/clientes/${idCliente}/perfil-lavado`)).data as PerfilLavado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['socios-perfil-lavado', idCliente] })
    },
  })

  const ultimo = historial?.[0]

  return (
    <div className="border-t border-black/[0.06] pt-3">
      <div className="mb-2 flex items-center justify-between">
        <p className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wide text-graphite-600">
          <ShieldAlert size={13} /> Perfil de riesgo LA/FT
        </p>
        <button
          type="button"
          onClick={() => calcular.mutate()}
          disabled={calcular.isPending}
          className="text-xs font-medium text-gold-400 hover:underline disabled:opacity-50"
        >
          {calcular.isPending ? 'Calculando…' : 'Calcular ahora'}
        </button>
      </div>

      <p className="mb-2 text-xs text-graphite-600">
        Basado en patrimonio (activos − pasivos) e ingreso mensual declarados, bandeados contra las tablas reales de
        la cooperativa. Cubre solo el grupo "Clientes" del modelo real (los grupos Canales/Zona/Transaccional
        requieren datos que este core no captura todavía).
      </p>

      {isLoading && <p className="text-xs text-graphite-600">Cargando…</p>}
      {!isLoading && !ultimo && <p className="text-xs text-graphite-600">Todavía no se ha calculado ningún perfil.</p>}

      {ultimo && (
        <div className="flex flex-wrap items-center gap-3">
          <Badge variant={categoriaVariant(ultimo.categoria)}>{categoriaLabel(ultimo.categoria)}</Badge>
          <span className="text-xs text-graphite-600">
            Patrimonio: {ultimo.patrimonio !== null ? formatoUsd(ultimo.patrimonio) : 'sin datos'}
          </span>
          <span className="text-xs text-graphite-600">
            Ingreso mensual: {ultimo.ingresoMensual !== null ? formatoUsd(ultimo.ingresoMensual) : 'sin datos'}
          </span>
          <span className="text-xs text-graphite-600">Calculado {ultimo.fecha}</span>
        </div>
      )}
    </div>
  )
}

function BuscarPersona({
  idExcluida,
  onSeleccionar,
}: {
  idExcluida: string
  onSeleccionar: (persona: PersonaBusqueda) => void
}) {
  const [q, setQ] = useState('')

  const { data: personas } = useQuery<PersonaBusqueda[]>({
    queryKey: ['socios-buscar-persona', q],
    queryFn: async () => (await api.get('/api/socios/buscar-persona', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  const opciones = personas?.filter((p) => p.id !== idExcluida)

  return (
    <div className="flex flex-col gap-1">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar persona por nombre o identificación…"
        className="rounded-lg border border-black/[0.08] bg-white px-3 py-1.5 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
      />
      {q.length >= 2 && (opciones?.length ?? 0) > 0 && (
        <div className="max-h-32 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
          {opciones?.map((p) => (
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

function SeccionTelefonos({ idPersona }: { idPersona: string }) {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [telefono, setTelefono] = useState('')
  const [esMovil, setEsMovil] = useState(true)
  const [esPrincipal, setEsPrincipal] = useState(false)
  const [notificacionSms, setNotificacionSms] = useState(false)

  const { data: telefonos } = useQuery<Telefono[]>({
    queryKey: ['socios-telefonos', idPersona],
    queryFn: async () => (await api.get(`/api/socios/personas/${idPersona}/telefonos`)).data,
  })

  const agregar = useMutation({
    mutationFn: async () =>
      api.post(`/api/socios/personas/${idPersona}/telefonos`, {
        telefono,
        esTelefonoMovil: esMovil,
        esPrincipal,
        notificacionSms,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['socios-telefonos', idPersona] })
      setTelefono('')
      setEsPrincipal(false)
      setNotificacionSms(false)
      setMostrarForm(false)
    },
  })

  const quitar = useMutation({
    mutationFn: async (id: string) => api.delete(`/api/socios/telefonos/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['socios-telefonos', idPersona] }),
  })

  return (
    <div>
      <div className="mb-1 flex items-center justify-between">
        <p className="text-xs font-semibold uppercase tracking-wide text-graphite-600">Teléfonos</p>
        {!mostrarForm && (
          <button type="button" onClick={() => setMostrarForm(true)} className="text-xs font-medium text-gold-400 hover:underline">
            <Plus size={12} className="inline" /> Agregar
          </button>
        )}
      </div>

      <ul className="mb-2 flex flex-col gap-1">
        {(telefonos?.length ?? 0) === 0 && <li className="text-xs text-graphite-600">Sin teléfonos registrados.</li>}
        {telefonos?.map((t) => (
          <li key={t.id} className="flex items-center justify-between text-xs">
            <span>
              {t.telefono} {t.esTelefonoMovil ? '(móvil)' : '(fijo)'} {t.esPrincipal && <Badge variant="exito">Principal</Badge>}
              {t.notificacionSms && <span className="ml-1 text-graphite-600">recibe SMS</span>}
            </span>
            <button
              type="button"
              disabled={quitar.isPending}
              onClick={() => quitar.mutate(t.id)}
              className="text-graphite-600 hover:text-red-700 disabled:opacity-50"
            >
              Quitar
            </button>
          </li>
        ))}
      </ul>

      {mostrarForm && (
        <div className="flex flex-col gap-2 rounded-lg border border-black/[0.08] bg-white p-2">
          <div className="flex items-center gap-2">
            <input
              value={telefono}
              onChange={(e) => setTelefono(e.target.value)}
              placeholder="Número"
              className="flex-1 rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
            />
            <button type="button" onClick={() => setMostrarForm(false)} className="text-graphite-600 hover:text-graphite-100">
              <X size={14} />
            </button>
          </div>
          <label className="flex items-center gap-1 text-xs text-graphite-600">
            <input type="checkbox" checked={esMovil} onChange={(e) => setEsMovil(e.target.checked)} /> Es móvil
          </label>
          <label className="flex items-center gap-1 text-xs text-graphite-600">
            <input type="checkbox" checked={esPrincipal} onChange={(e) => setEsPrincipal(e.target.checked)} /> Principal
          </label>
          <label className="flex items-center gap-1 text-xs text-graphite-600">
            <input type="checkbox" checked={notificacionSms} onChange={(e) => setNotificacionSms(e.target.checked)} /> Recibe SMS
          </label>
          <button
            type="button"
            disabled={!telefono || agregar.isPending}
            onClick={() => agregar.mutate()}
            className="rounded bg-gold-500 px-3 py-1 text-xs font-medium text-white disabled:opacity-50"
          >
            Guardar
          </button>
        </div>
      )}
    </div>
  )
}

function SeccionConyuge({ idPersonaNatural }: { idPersonaNatural: string }) {
  const queryClient = useQueryClient()

  const { data: conyuges } = useQuery<Conyuge[]>({
    queryKey: ['socios-conyuges', idPersonaNatural],
    queryFn: async () => (await api.get(`/api/socios/personas/${idPersonaNatural}/conyuges`)).data,
  })

  const agregar = useMutation({
    mutationFn: async (idPersonaConyuge: string) =>
      api.post(`/api/socios/personas/${idPersonaNatural}/conyuges`, { idPersonaConyuge }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['socios-conyuges', idPersonaNatural] }),
  })

  const quitar = useMutation({
    mutationFn: async (id: string) => api.delete(`/api/socios/conyuges/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['socios-conyuges', idPersonaNatural] }),
  })

  return (
    <div>
      <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-graphite-600">Cónyuge</p>
      {conyuges?.map((c) => (
        <div key={c.id} className="mb-2 flex items-center justify-between text-xs">
          <span>
            {c.nombreConyuge} — {c.identificacionConyuge}
          </span>
          <button
            type="button"
            disabled={quitar.isPending}
            onClick={() => quitar.mutate(c.id)}
            className="text-graphite-600 hover:text-red-700 disabled:opacity-50"
          >
            Quitar
          </button>
        </div>
      ))}
      {(conyuges?.length ?? 0) === 0 && (
        <BuscarPersona idExcluida={idPersonaNatural} onSeleccionar={(p) => agregar.mutate(p.id)} />
      )}
      {agregar.isError && (
        <p className="mt-1 text-xs text-red-700">
          {(agregar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? 'No se pudo registrar.'}
        </p>
      )}
    </div>
  )
}

function SeccionRepresentantes({ idPersona }: { idPersona: string }) {
  const queryClient = useQueryClient()
  const [ejerceControl, setEjerceControl] = useState(false)

  const { data: representantes } = useQuery<Representante[]>({
    queryKey: ['socios-representantes', idPersona],
    queryFn: async () => (await api.get(`/api/socios/personas/${idPersona}/representantes`)).data,
  })

  const agregar = useMutation({
    mutationFn: async (idPersonaRepresentante: string) =>
      api.post(`/api/socios/personas/${idPersona}/representantes`, {
        idPersonaRepresentante,
        principal: (representantes?.length ?? 0) === 0,
        ejerceControl,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['socios-representantes', idPersona] }),
  })

  const quitar = useMutation({
    mutationFn: async (id: string) => api.delete(`/api/socios/representantes/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['socios-representantes', idPersona] }),
  })

  return (
    <div>
      <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-graphite-600">Representantes</p>
      <ul className="mb-2 flex flex-col gap-1">
        {(representantes?.length ?? 0) === 0 && <li className="text-xs text-graphite-600">Sin representantes registrados.</li>}
        {representantes?.map((r) => (
          <li key={r.id} className="flex items-center justify-between text-xs">
            <span>
              {r.nombreRepresentante} — {r.identificacionRepresentante}
              {r.principal && <Badge variant="exito">Principal</Badge>}
              {r.ejerceControl && <span className="ml-1 text-graphite-600">ejerce control</span>}
            </span>
            <button
              type="button"
              disabled={quitar.isPending}
              onClick={() => quitar.mutate(r.id)}
              className="text-graphite-600 hover:text-red-700 disabled:opacity-50"
            >
              Quitar
            </button>
          </li>
        ))}
      </ul>
      <label className="mb-1 flex items-center gap-1 text-xs text-graphite-600">
        <input type="checkbox" checked={ejerceControl} onChange={(e) => setEjerceControl(e.target.checked)} /> El nuevo
        representante ejerce control efectivo
      </label>
      <BuscarPersona idExcluida={idPersona} onSeleccionar={(p) => agregar.mutate(p.id)} />
      {agregar.isError && (
        <p className="mt-1 text-xs text-red-700">
          {(agregar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? 'No se pudo registrar.'}
        </p>
      )}
    </div>
  )
}

interface CatalogoResuelto {
  codigo: string
  nombre: string
}

interface SocioTelefonoDetalle {
  telefono: string
  esTelefonoMovil: boolean
  esPrincipal: boolean
  notificacionSms: boolean
}

interface SocioConyugeDetalle {
  nombre: string
  identificacion: string
}

interface SocioRepresentanteDetalle {
  nombre: string
  identificacion: string
  principal: boolean
  ejerceControl: boolean
}

interface SocioPerfilLavadoResumen {
  fecha: string
  patrimonio: number | null
  ingresoMensual: number | null
  categoria: string | null
}

interface SocioDetalleCompleto {
  idCliente: string
  numero: string
  agencia: string
  estado: string
  creadoEn: string
  causaVinculacion: CatalogoResuelto | null
  calificacionInterna: CatalogoResuelto | null
  sectorEconomico: CatalogoResuelto | null
  idPersona: string
  identificacion: string
  tipoIdentificacion: string
  nombre: string
  email: string | null
  esPersonaNatural: boolean
  provinciaDomicilio: CatalogoResuelto | null
  numeroCasa: string | null
  barrio: string | null
  callePrincipal: string | null
  activos: number | null
  pasivos: number | null
  ingresos: number | null
  egresos: number | null
  fechaNacimiento: string | null
  esMasculino: boolean | null
  esPep: boolean | null
  estadoCivil: CatalogoResuelto | null
  educacion: CatalogoResuelto | null
  vivienda: CatalogoResuelto | null
  sectorVivienda: CatalogoResuelto | null
  nacionalidad: CatalogoResuelto | null
  fechaCreacionEmpresa: string | null
  esGrupo: boolean | null
  esInstitucionBancaria: boolean | null
  esPublica: boolean | null
  telefonos: SocioTelefonoDetalle[]
  conyuge: SocioConyugeDetalle | null
  representantes: SocioRepresentanteDetalle[]
  perfilLavado: SocioPerfilLavadoResumen | null
}

function CampoDetalle({ label, valor }: { label: string; valor: string | null | undefined }) {
  return (
    <div>
      <p className="text-[11px] uppercase tracking-wide text-graphite-600">{label}</p>
      <p className="text-sm text-graphite-100">{valor ?? '—'}</p>
    </div>
  )
}

function DetalleSocioModal({ idCliente, onClose }: { idCliente: string; onClose: () => void }) {
  const { data: d, isLoading } = useQuery<SocioDetalleCompleto>({
    queryKey: ['socio-detalle', idCliente],
    queryFn: async () => (await api.get(`/api/socios/${idCliente}/detalle`)).data,
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-xl p-5"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-graphite-100">Detalle del socio</h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={16} />
          </button>
        </div>

        {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}

        {d && (
          <div className="flex flex-col gap-5">
            <div>
              <div className="mb-2 flex items-center gap-2">
                <h3 className="text-base font-semibold text-graphite-100">{d.nombre}</h3>
                <Badge variant={d.estado === 'Activo' ? 'exito' : 'neutral'}>{d.estado}</Badge>
                <Badge variant="neutral">{d.esPersonaNatural ? 'Persona natural' : 'Persona jurídica'}</Badge>
              </div>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <CampoDetalle label="Número de socio" valor={d.numero} />
                <CampoDetalle label="Agencia" valor={d.agencia} />
                <CampoDetalle label="Identificación" valor={`${d.identificacion} (${d.tipoIdentificacion})`} />
                <CampoDetalle label="Email" valor={d.email} />
                <CampoDetalle label="Provincia de domicilio" valor={d.provinciaDomicilio?.nombre} />
                <CampoDetalle label="Barrio / calle" valor={[d.barrio, d.callePrincipal].filter(Boolean).join(' / ') || null} />
                <CampoDetalle label="Socio desde" valor={new Date(d.creadoEn).toLocaleDateString('es-EC')} />
              </div>
            </div>

            <div className="border-t border-black/[0.06] pt-3">
              <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Datos financieros</h4>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <CampoDetalle label="Activos" valor={d.activos != null ? formatoUsd(d.activos) : null} />
                <CampoDetalle label="Pasivos" valor={d.pasivos != null ? formatoUsd(d.pasivos) : null} />
                <CampoDetalle label="Ingresos" valor={d.ingresos != null ? formatoUsd(d.ingresos) : null} />
                <CampoDetalle label="Egresos" valor={d.egresos != null ? formatoUsd(d.egresos) : null} />
              </div>
            </div>

            {d.esPersonaNatural ? (
              <div className="border-t border-black/[0.06] pt-3">
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Datos socioeconómicos</h4>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                  <CampoDetalle label="Fecha de nacimiento" valor={d.fechaNacimiento} />
                  <CampoDetalle label="Género" valor={d.esMasculino === null ? null : d.esMasculino ? 'Masculino' : 'Femenino'} />
                  <CampoDetalle label="Estado civil" valor={d.estadoCivil?.nombre} />
                  <CampoDetalle label="Educación" valor={d.educacion?.nombre} />
                  <CampoDetalle label="Vivienda" valor={d.vivienda?.nombre} />
                  <CampoDetalle label="Sector de vivienda" valor={d.sectorVivienda?.nombre} />
                  <CampoDetalle label="Nacionalidad" valor={d.nacionalidad?.nombre} />
                  <CampoDetalle label="PEP" valor={d.esPep ? 'Sí' : 'No'} />
                </div>
              </div>
            ) : (
              <div className="border-t border-black/[0.06] pt-3">
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Datos de la empresa</h4>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                  <CampoDetalle label="Fecha de creación" valor={d.fechaCreacionEmpresa} />
                  <CampoDetalle label="Es grupo" valor={d.esGrupo ? 'Sí' : 'No'} />
                  <CampoDetalle label="Institución bancaria" valor={d.esInstitucionBancaria ? 'Sí' : 'No'} />
                  <CampoDetalle label="Es pública" valor={d.esPublica ? 'Sí' : 'No'} />
                </div>
              </div>
            )}

            <div className="border-t border-black/[0.06] pt-3">
              <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Vinculación</h4>
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
                <CampoDetalle label="Causa de vinculación" valor={d.causaVinculacion?.nombre} />
                <CampoDetalle label="Calificación interna" valor={d.calificacionInterna?.nombre} />
                <CampoDetalle label="Sector económico" valor={d.sectorEconomico?.nombre} />
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 border-t border-black/[0.06] pt-3 sm:grid-cols-3">
              <div>
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Teléfonos</h4>
                {d.telefonos.length === 0 && <p className="text-xs text-graphite-600">Sin teléfonos registrados.</p>}
                <ul className="flex flex-col gap-1">
                  {d.telefonos.map((t, i) => (
                    <li key={i} className="text-xs text-graphite-100">
                      {t.telefono} {t.esTelefonoMovil ? '(móvil)' : '(fijo)'}
                      {t.esPrincipal && <Badge variant="exito">Principal</Badge>}
                    </li>
                  ))}
                </ul>
              </div>
              <div>
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Cónyuge</h4>
                <p className="text-xs text-graphite-100">
                  {d.conyuge ? `${d.conyuge.nombre} — ${d.conyuge.identificacion}` : 'No registrado.'}
                </p>
              </div>
              <div>
                <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Representantes</h4>
                {d.representantes.length === 0 && <p className="text-xs text-graphite-600">Sin representantes registrados.</p>}
                <ul className="flex flex-col gap-1">
                  {d.representantes.map((r, i) => (
                    <li key={i} className="text-xs text-graphite-100">
                      {r.nombre} — {r.identificacion} {r.principal && <Badge variant="exito">Principal</Badge>}
                    </li>
                  ))}
                </ul>
              </div>
            </div>

            <div className="border-t border-black/[0.06] pt-3">
              <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-graphite-600">Perfil de riesgo LA/FT</h4>
              {d.perfilLavado ? (
                <div className="flex flex-wrap items-center gap-3">
                  <Badge variant={categoriaVariant(d.perfilLavado.categoria === 'Bajo' ? 1 : d.perfilLavado.categoria === 'Medio' ? 2 : d.perfilLavado.categoria === 'Alto' ? 3 : d.perfilLavado.categoria === 'MuyAlto' ? 4 : null)}>
                    {d.perfilLavado.categoria ?? 'Sin datos suficientes'}
                  </Badge>
                  <span className="text-xs text-graphite-600">
                    Patrimonio: {d.perfilLavado.patrimonio != null ? formatoUsd(d.perfilLavado.patrimonio) : 'sin datos'}
                  </span>
                  <span className="text-xs text-graphite-600">
                    Ingreso mensual: {d.perfilLavado.ingresoMensual != null ? formatoUsd(d.perfilLavado.ingresoMensual) : 'sin datos'}
                  </span>
                  <span className="text-xs text-graphite-600">Calculado {d.perfilLavado.fecha}</span>
                </div>
              ) : (
                <p className="text-xs text-graphite-600">Todavía no se ha calculado ningún perfil.</p>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

function DatosSocioPanel({ socio }: { socio: Socio }) {
  return (
    <tr className="border-b border-black/[0.04] bg-black/[0.01]">
      <td colSpan={6} className="px-4 py-3">
        <div className="mb-4">
          <SeccionDatosBasicos socio={socio} />
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <SeccionTelefonos idPersona={socio.idPersona} />
          {socio.esPersonaNatural ? (
            <SeccionConyuge idPersonaNatural={socio.idPersona} />
          ) : (
            <div className="text-xs text-graphite-600">Cónyuge no aplica (persona jurídica).</div>
          )}
          <SeccionRepresentantes idPersona={socio.idPersona} />
        </div>
        <div className="mt-4">
          <PerfilLavadoPanel idCliente={socio.id} />
        </div>
      </td>
    </tr>
  )
}

interface S01Elemento {
  tipoIdentificacion: string
  numeroIdentificacion: string
  paisNacimiento: string | null
  apellidosNombres: string
  fechaNacimiento: string
  genero: string
  valorCertifAportacion: number
  fechaIngreso: string
}

interface S01Reporte {
  cabecera: { codigoEstructura: string; ruc: string; fechaCorte: string; numeroTotalRegistros: number }
  detalle: S01Elemento[]
  advertencias: string[]
}

function SeccionReporteS01() {
  const { data, isLoading, isError, error } = useQuery<S01Reporte>({
    queryKey: ['socios-reporte-s01'],
    queryFn: async () => (await api.get('/api/socios/reportes/s01')).data,
  })

  return (
    <div>
      <p className="mb-4 text-xs text-graphite-600">
        Estructura real "Socios S01" (Manual Tecnológico SEPS v3.0 + XSD oficial) — solo socios persona natural
        activos, con género y fecha de nacimiento reales y el valor de sus Certificados de Aportación.
      </p>

      {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}
      {isError && (
        <p className="text-sm text-red-700">
          {(error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? 'No se pudo generar el reporte.'}
        </p>
      )}

      {data && (
        <>
          <div className="mb-4 grid grid-cols-2 gap-3 sm:grid-cols-4">
            <div className="glass-card rounded-xl p-3">
              <p className="text-xs text-graphite-600">Estructura</p>
              <p className="text-lg font-semibold text-graphite-100">{data.cabecera.codigoEstructura}</p>
            </div>
            <div className="glass-card rounded-xl p-3">
              <p className="text-xs text-graphite-600">RUC</p>
              <p className="text-lg font-semibold text-graphite-100">{data.cabecera.ruc}</p>
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

          {data.advertencias.length > 0 && (
            <div className="mb-4 rounded-lg border border-gold-500/30 bg-gold-500/10 p-3">
              <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-gold-300">Advertencias</p>
              <ul className="list-disc space-y-1 pl-4 text-xs text-graphite-600">
                {data.advertencias.map((a, i) => (
                  <li key={i}>{a}</li>
                ))}
              </ul>
            </div>
          )}

          <TableContainer>
            <thead>
              <tr>
                <Th>Tipo ID</Th>
                <Th>Identificación</Th>
                <Th>Nombre</Th>
                <Th>País nac.</Th>
                <Th>Nacimiento</Th>
                <Th>Género</Th>
                <Th>Certificados</Th>
                <Th>Fecha ingreso</Th>
              </tr>
            </thead>
            <tbody>
              {data.detalle.length === 0 && <EmptyState>Sin socios persona natural activos</EmptyState>}
              {data.detalle.map((e, i) => (
                <tr key={i} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                  <Td>{e.tipoIdentificacion}</Td>
                  <Td>{e.numeroIdentificacion}</Td>
                  <Td className="font-medium">{e.apellidosNombres}</Td>
                  <Td>{e.paisNacimiento ?? '—'}</Td>
                  <Td>{e.fechaNacimiento}</Td>
                  <Td>{e.genero}</Td>
                  <Td className="tabular-nums">${e.valorCertifAportacion.toFixed(2)}</Td>
                  <Td>{e.fechaIngreso}</Td>
                </tr>
              ))}
            </tbody>
          </TableContainer>
        </>
      )}
    </div>
  )
}

function NuevoSocioModal({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient()
  const [esPersonaNatural, setEsPersonaNatural] = useState(true)
  const [identificacion, setIdentificacion] = useState('')
  const [idTipoIdentificacion, setIdTipoIdentificacion] = useState<number | ''>('')
  const [idAgencia, setIdAgencia] = useState<number | ''>('')
  const [email, setEmail] = useState('')
  const [primerNombre, setPrimerNombre] = useState('')
  const [segundoNombre, setSegundoNombre] = useState('')
  const [apellidoPaterno, setApellidoPaterno] = useState('')
  const [apellidoMaterno, setApellidoMaterno] = useState('')
  const [fechaNacimiento, setFechaNacimiento] = useState('')
  const [esMasculino, setEsMasculino] = useState(true)
  const [razonSocial, setRazonSocial] = useState('')
  const [fechaCreacion, setFechaCreacion] = useState('')
  const [estadoCivil, setEstadoCivil] = useState('')
  const [educacion, setEducacion] = useState('')
  const [vivienda, setVivienda] = useState('')
  const [sectorVivienda, setSectorVivienda] = useState('')
  const [nacionalidad, setNacionalidad] = useState('')
  const [profesion, setProfesion] = useState('')
  const [idActividadEconomica, setIdActividadEconomica] = useState<number | null>(null)
  const [nombreActividadEconomica, setNombreActividadEconomica] = useState<string | null>(null)
  const [causaVinculacion, setCausaVinculacion] = useState('')
  const [calificacionInterna, setCalificacionInterna] = useState('')
  const [sectorEconomico, setSectorEconomico] = useState('')

  const { data: tipos } = useQuery<TipoIdentificacion[]>({
    queryKey: ['configuracion-tipos-identificacion'],
    queryFn: async () => (await api.get('/api/configuracion/tipos-identificacion')).data,
  })
  const { data: agencias } = useQuery<Agencia[]>({
    queryKey: ['configuracion-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      api.post('/api/socios', {
        esPersonaNatural,
        identificacion,
        idTipoIdentificacion,
        email: email || null,
        idAgencia,
        primerNombre: esPersonaNatural ? primerNombre : undefined,
        segundoNombre: esPersonaNatural ? segundoNombre || undefined : undefined,
        apellidoPaterno: esPersonaNatural ? apellidoPaterno : undefined,
        apellidoMaterno: esPersonaNatural ? apellidoMaterno || undefined : undefined,
        fechaNacimiento: esPersonaNatural ? fechaNacimiento || undefined : undefined,
        esMasculino: esPersonaNatural ? esMasculino : undefined,
        razonSocial: !esPersonaNatural ? razonSocial : undefined,
        fechaCreacion: !esPersonaNatural ? fechaCreacion || undefined : undefined,
        codigoEstadoCivil: esPersonaNatural ? estadoCivil || undefined : undefined,
        codigoEducacion: esPersonaNatural ? educacion || undefined : undefined,
        codigoVivienda: esPersonaNatural ? vivienda || undefined : undefined,
        codigoSectorVivienda: esPersonaNatural ? sectorVivienda || undefined : undefined,
        codigoNacionalidad: esPersonaNatural ? nacionalidad || undefined : undefined,
        codigoProfesion: esPersonaNatural ? profesion || undefined : undefined,
        idActividadEconomica: idActividadEconomica ?? undefined,
        codigoCausaVinculacion: causaVinculacion || undefined,
        codigoCalificacionInterna: calificacionInterna || undefined,
        codigoSectorEconomico: sectorEconomico || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['socios'] })
      onClose()
    },
  })

  const puedeGuardar =
    identificacion && idTipoIdentificacion !== '' && idAgencia !== '' &&
    (esPersonaNatural ? primerNombre && apellidoPaterno && fechaNacimiento : razonSocial && fechaCreacion)

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl p-5"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-sm font-semibold text-graphite-100">Nuevo socio</h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={16} />
          </button>
        </div>

        <div className="mb-3 flex gap-2">
          <button
            type="button"
            onClick={() => setEsPersonaNatural(true)}
            className={`flex-1 rounded-lg border px-3 py-1.5 text-xs font-medium ${esPersonaNatural ? 'border-gold-500 bg-gold-500/10 text-gold-300' : 'border-black/[0.08] text-graphite-600'}`}
          >
            Persona natural
          </button>
          <button
            type="button"
            onClick={() => setEsPersonaNatural(false)}
            className={`flex-1 rounded-lg border px-3 py-1.5 text-xs font-medium ${!esPersonaNatural ? 'border-gold-500 bg-gold-500/10 text-gold-300' : 'border-black/[0.08] text-graphite-600'}`}
          >
            Persona jurídica
          </button>
        </div>

        <div className="flex flex-col gap-2">
          <div className="grid grid-cols-2 gap-2">
            <select
              value={idTipoIdentificacion}
              onChange={(e) => setIdTipoIdentificacion(e.target.value ? Number(e.target.value) : '')}
              className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
            >
              <option value="">Tipo de identificación…</option>
              {tipos?.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.nombre}
                </option>
              ))}
            </select>
            <input
              value={identificacion}
              onChange={(e) => setIdentificacion(e.target.value)}
              placeholder="Número de identificación"
              className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
            />
          </div>

          <select
            value={idAgencia}
            onChange={(e) => setIdAgencia(e.target.value ? Number(e.target.value) : '')}
            className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
          >
            <option value="">Agencia…</option>
            {agencias?.filter((a) => a.activa).map((a) => (
              <option key={a.id} value={a.id}>
                {a.nombre}
              </option>
            ))}
          </select>

          <input
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Email (opcional)"
            className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
          />

          {esPersonaNatural ? (
            <>
              <div className="grid grid-cols-2 gap-2">
                <input
                  value={primerNombre}
                  onChange={(e) => setPrimerNombre(e.target.value)}
                  placeholder="Primer nombre"
                  className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
                />
                <input
                  value={segundoNombre}
                  onChange={(e) => setSegundoNombre(e.target.value)}
                  placeholder="Segundo nombre (opcional)"
                  className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
                />
                <input
                  value={apellidoPaterno}
                  onChange={(e) => setApellidoPaterno(e.target.value)}
                  placeholder="Apellido paterno"
                  className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
                />
                <input
                  value={apellidoMaterno}
                  onChange={(e) => setApellidoMaterno(e.target.value)}
                  placeholder="Apellido materno (opcional)"
                  className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
                />
              </div>
              <label className="flex items-center gap-2 text-xs text-graphite-600">
                Fecha de nacimiento
                <input
                  type="date"
                  value={fechaNacimiento}
                  onChange={(e) => setFechaNacimiento(e.target.value)}
                  className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
                />
              </label>
              <div className="flex gap-3 text-xs text-graphite-600">
                <label className="flex items-center gap-1">
                  <input type="radio" checked={esMasculino} onChange={() => setEsMasculino(true)} /> Masculino
                </label>
                <label className="flex items-center gap-1">
                  <input type="radio" checked={!esMasculino} onChange={() => setEsMasculino(false)} /> Femenino
                </label>
              </div>
            </>
          ) : (
            <>
              <input
                value={razonSocial}
                onChange={(e) => setRazonSocial(e.target.value)}
                placeholder="Razón social"
                className="rounded border border-black/[0.08] px-2 py-1.5 text-xs outline-none focus:border-gold-500/50"
              />
              <label className="flex items-center gap-2 text-xs text-graphite-600">
                Fecha de creación
                <input
                  type="date"
                  value={fechaCreacion}
                  onChange={(e) => setFechaCreacion(e.target.value)}
                  className="rounded border border-black/[0.08] px-2 py-1 text-xs outline-none focus:border-gold-500/50"
                />
              </label>
            </>
          )}

          {esPersonaNatural && (
            <div className="grid grid-cols-2 gap-2 border-t border-black/[0.06] pt-2">
              <CatalogoSelect endpoint="estados-civiles" value={estadoCivil} onChange={setEstadoCivil} placeholder="Estado civil (opcional)…" />
              <CatalogoSelect endpoint="educacion" value={educacion} onChange={setEducacion} placeholder="Educación (opcional)…" />
              <CatalogoSelect endpoint="vivienda" value={vivienda} onChange={setVivienda} placeholder="Vivienda (opcional)…" />
              <CatalogoSelect endpoint="sector-vivienda" value={sectorVivienda} onChange={setSectorVivienda} placeholder="Sector vivienda (opcional)…" />
              <CatalogoSelect endpoint="nacionalidades" value={nacionalidad} onChange={setNacionalidad} placeholder="Nacionalidad (opcional)…" />
              <ProfesionSelect value={profesion} onChange={setProfesion} />
            </div>
          )}
          {esPersonaNatural && (
            <div className="border-t border-black/[0.06] pt-2">
              <p className="mb-1 text-[11px] text-graphite-600">Actividad económica (CIIU, opcional)</p>
              <ActividadEconomicaSelect
                value={idActividadEconomica}
                nombreActual={nombreActividadEconomica}
                onChange={(id, nombre) => {
                  setIdActividadEconomica(id)
                  setNombreActividadEconomica(nombre)
                }}
              />
            </div>
          )}
          <div className="grid grid-cols-1 gap-2 border-t border-black/[0.06] pt-2">
            <CatalogoSelect endpoint="causas-vinculacion" value={causaVinculacion} onChange={setCausaVinculacion} placeholder="Causa de vinculación (opcional)…" />
            <div className="grid grid-cols-2 gap-2">
              <CatalogoSelect endpoint="calificaciones-internas" value={calificacionInterna} onChange={setCalificacionInterna} placeholder="Calificación interna (opcional)…" />
              <CatalogoSelect endpoint="sectores-economicos" value={sectorEconomico} onChange={setSectorEconomico} placeholder="Sector económico (opcional)…" />
            </div>
          </div>

          {crear.isError && (
            <p className="text-xs text-red-700">
              {(crear.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? 'No se pudo crear el socio.'}
            </p>
          )}

          <button
            type="button"
            disabled={!puedeGuardar || crear.isPending}
            onClick={() => crear.mutate()}
            className="mt-1 rounded bg-gold-500 px-3 py-2 text-xs font-medium text-white disabled:opacity-50"
          >
            {crear.isPending ? 'Creando…' : 'Crear socio'}
          </button>
        </div>
      </div>
    </div>
  )
}

interface CodigoNombre {
  codigo: string
  nombre: string
}

interface TipoProductoReclamo {
  id: number
  codigo: string
  nombre: string
}

interface ConceptoReclamoDetalle {
  codigo: string
  codigoConcepto: string
  concepto: string
  descripcion: string
}

interface ReclamoRespuesta {
  codigoTipoResolucion: string
  tipoResolucion: string
  montoRestituido: number | null
  interesSobreMonto: number | null
  descripcion: string
  creadoEn: string
  registradoPor: string
}

interface Reclamo {
  id: string
  idPersona: string
  persona: string
  identificacion: string
  canalRecepcion: string
  fechaRecepcion: string
  tipoProducto: string
  codigoConceptoDetalle: string
  conceptoDetalle: string
  codigoEstado: string
  estado: string
  descripcion: string
  creadoEn: string
  registradoPor: string
  respuesta: ReclamoRespuesta | null
}

function badgeEstadoReclamo(codigoEstado: string) {
  return codigoEstado === '2' ? <Badge variant="exito">Resuelto</Badge> : <Badge variant="alerta">En trámite</Badge>
}

function mensajeErrorSocios(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
}

function NuevoReclamoModal({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient()
  const [persona, setPersona] = useState<PersonaBusqueda | null>(null)
  const [codigoCanalRecepcion, setCodigoCanalRecepcion] = useState('')
  const [idTipoProducto, setIdTipoProducto] = useState('')
  const [codigoConceptoDetalle, setCodigoConceptoDetalle] = useState('')
  const [descripcion, setDescripcion] = useState('')

  const { data: canales } = useQuery<CodigoNombre[]>({
    queryKey: ['reclamos-canales'],
    queryFn: async () => (await api.get('/api/socios/reclamos/canales')).data,
  })
  const { data: tiposProducto } = useQuery<TipoProductoReclamo[]>({
    queryKey: ['reclamos-tipos-producto'],
    queryFn: async () => (await api.get('/api/socios/reclamos/tipos-producto')).data,
  })
  const { data: conceptos } = useQuery<ConceptoReclamoDetalle[]>({
    queryKey: ['reclamos-conceptos'],
    queryFn: async () => (await api.get('/api/socios/reclamos/conceptos')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/socios/reclamos', {
          idPersona: persona?.id,
          codigoCanalRecepcion,
          idTipoProducto: Number(idTipoProducto),
          codigoConceptoDetalle,
          descripcion,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reclamos'] })
      onClose()
    },
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="glass-card w-full max-w-lg rounded-xl p-5" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">Nuevo reclamo</h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            if (!persona || !codigoCanalRecepcion || !idTipoProducto || !codigoConceptoDetalle || !descripcion) return
            crear.mutate()
          }}
        >
          <div>
            <span className="mb-1 block text-sm text-graphite-600">Socio o cliente</span>
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
              <BuscarPersona idExcluida="" onSeleccionar={setPersona} />
            )}
          </div>

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Canal de recepción</span>
              <select
                required
                value={codigoCanalRecepcion}
                onChange={(e) => setCodigoCanalRecepcion(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar…</option>
                {canales?.map((c) => (
                  <option key={c.codigo} value={c.codigo}>
                    {c.nombre}
                  </option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Producto</span>
              <select
                required
                value={idTipoProducto}
                onChange={(e) => setIdTipoProducto(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar…</option>
                {tiposProducto?.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.nombre}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Concepto del reclamo</span>
            <select
              required
              value={codigoConceptoDetalle}
              onChange={(e) => setCodigoConceptoDetalle(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {conceptos?.map((c) => (
                <option key={c.codigo} value={c.codigo}>
                  [{c.concepto}] {c.descripcion}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Descripción del hecho</span>
            <textarea
              required
              rows={3}
              value={descripcion}
              onChange={(e) => setDescripcion(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <div>
            <button
              type="submit"
              disabled={crear.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {crear.isPending ? 'Registrando…' : 'Registrar reclamo'}
            </button>
          </div>

          {crear.isError && <p className="text-sm text-red-700">{mensajeErrorSocios(crear.error, 'No se pudo registrar el reclamo.')}</p>}
        </form>
      </div>
    </div>
  )
}

function ResponderReclamoInline({ reclamo, onClose }: { reclamo: Reclamo; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [codigoTipoResolucion, setCodigoTipoResolucion] = useState('')
  const [montoRestituido, setMontoRestituido] = useState('')
  const [interesSobreMonto, setInteresSobreMonto] = useState('')
  const [descripcion, setDescripcion] = useState('')

  const { data: tiposResolucion } = useQuery<CodigoNombre[]>({
    queryKey: ['reclamos-tipos-resolucion'],
    queryFn: async () => (await api.get('/api/socios/reclamos/tipos-resolucion')).data,
  })

  const responder = useMutation({
    mutationFn: async () =>
      (
        await api.post(`/api/socios/reclamos/${reclamo.id}/responder`, {
          codigoTipoResolucion,
          montoRestituido: montoRestituido ? Number(montoRestituido) : null,
          interesSobreMonto: interesSobreMonto ? Number(interesSobreMonto) : null,
          descripcion,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reclamos'] })
      onClose()
    },
  })

  return (
    <tr className="border-b border-black/[0.04] bg-black/[0.015] last:border-0">
      <Td colSpan={7}>
        <form
          className="flex flex-col gap-3 py-2"
          onSubmit={(e) => {
            e.preventDefault()
            if (!codigoTipoResolucion || !descripcion) return
            responder.mutate()
          }}
        >
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Tipo de resolución</span>
              <select
                required
                value={codigoTipoResolucion}
                onChange={(e) => setCodigoTipoResolucion(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar…</option>
                {tiposResolucion?.map((t) => (
                  <option key={t.codigo} value={t.codigo}>
                    {t.nombre}
                  </option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Monto restituido (opcional)</span>
              <input
                type="number"
                step="0.01"
                min="0"
                value={montoRestituido}
                onChange={(e) => setMontoRestituido(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Interés sobre el monto (opcional)</span>
              <input
                type="number"
                step="0.01"
                min="0"
                value={interesSobreMonto}
                onChange={(e) => setInteresSobreMonto(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          </div>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Descripción de la respuesta</span>
            <textarea
              required
              rows={2}
              value={descripcion}
              onChange={(e) => setDescripcion(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <div className="flex items-center gap-3">
            <button
              type="submit"
              disabled={responder.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              Registrar respuesta
            </button>
            <button type="button" onClick={onClose} className="text-sm text-graphite-600 hover:text-graphite-100">
              Cancelar
            </button>
          </div>
          {responder.isError && <p className="text-sm text-red-700">{mensajeErrorSocios(responder.error, 'No se pudo responder.')}</p>}
        </form>
      </Td>
    </tr>
  )
}

function SeccionReclamos() {
  const [mostrarNuevo, setMostrarNuevo] = useState(false)
  const [idResponder, setIdResponder] = useState<string | null>(null)

  const { data: reclamos, isLoading } = useQuery<Reclamo[]>({
    queryKey: ['reclamos'],
    queryFn: async () => (await api.get('/api/socios/reclamos')).data,
  })

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <p className="text-sm text-graphite-600">
          Canal formal de reclamos — requisito de la Ley Orgánica de Defensa del Consumidor.
        </p>
        <button
          type="button"
          onClick={() => setMostrarNuevo(true)}
          className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
        >
          <Plus size={16} /> Nuevo reclamo
        </button>
      </div>

      {mostrarNuevo && (
        <ModalPortal>
          <NuevoReclamoModal onClose={() => setMostrarNuevo(false)} />
        </ModalPortal>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Socio</Th>
            <Th>Canal</Th>
            <Th>Producto</Th>
            <Th>Concepto</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (reclamos?.length ?? 0) === 0 && <EmptyState>Todavía no hay reclamos registrados</EmptyState>}
          {reclamos?.map((r) => (
            <Fragment key={r.id}>
              <tr className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{r.persona}</Td>
                <Td>{r.canalRecepcion}</Td>
                <Td>{r.tipoProducto}</Td>
                <Td className="max-w-xs truncate" title={r.conceptoDetalle}>
                  {r.conceptoDetalle}
                </Td>
                <Td>{r.fechaRecepcion}</Td>
                <Td>{badgeEstadoReclamo(r.codigoEstado)}</Td>
                <Td>
                  {r.codigoEstado === '1' && (
                    <button
                      type="button"
                      onClick={() => setIdResponder(idResponder === r.id ? null : r.id)}
                      className="text-xs font-medium text-petrol-700 hover:underline"
                    >
                      Responder
                    </button>
                  )}
                  {r.respuesta && (
                    <span className="text-xs text-graphite-600" title={r.respuesta.descripcion}>
                      {r.respuesta.tipoResolucion}
                    </span>
                  )}
                </Td>
              </tr>
              {idResponder === r.id && <ResponderReclamoInline reclamo={r} onClose={() => setIdResponder(null)} />}
            </Fragment>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface MiembroOrganoGobierno {
  id: string
  idPersona: string
  persona: string
  identificacion: string
  esAsambleaGeneral: boolean
  fechaIniciaAsambleaGeneral: string | null
  fechaTerminaAsambleaGeneral: string | null
  esConsejoAdministracion: boolean
  fechaIniciaConsejoAdministracion: string | null
  fechaTerminaConsejoAdministracion: string | null
  esConsejoVigilancia: boolean
  fechaIniciaConsejoVigilancia: string | null
  fechaTerminaConsejoVigilancia: string | null
  activo: boolean
}

function NuevoMiembroOrganoGobiernoModal({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient()
  const [persona, setPersona] = useState<PersonaBusqueda | null>(null)
  const [esAsambleaGeneral, setEsAsambleaGeneral] = useState(false)
  const [esConsejoAdministracion, setEsConsejoAdministracion] = useState(false)
  const [esConsejoVigilancia, setEsConsejoVigilancia] = useState(false)
  const [fechaInicia, setFechaInicia] = useState('')
  const [fechaTermina, setFechaTermina] = useState('')

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/socios/organo-gobierno', {
          idPersona: persona?.id,
          esAsambleaGeneral,
          fechaIniciaAsambleaGeneral: esAsambleaGeneral ? fechaInicia || null : null,
          fechaTerminaAsambleaGeneral: esAsambleaGeneral ? fechaTermina || null : null,
          esConsejoAdministracion,
          fechaIniciaConsejoAdministracion: esConsejoAdministracion ? fechaInicia || null : null,
          fechaTerminaConsejoAdministracion: esConsejoAdministracion ? fechaTermina || null : null,
          esConsejoVigilancia,
          fechaIniciaConsejoVigilancia: esConsejoVigilancia ? fechaInicia || null : null,
          fechaTerminaConsejoVigilancia: esConsejoVigilancia ? fechaTermina || null : null,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['organo-gobierno'] })
      onClose()
    },
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div className="glass-card w-full max-w-lg rounded-xl p-5" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">Nuevo miembro de órgano de gobierno</h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            if (!persona || (!esAsambleaGeneral && !esConsejoAdministracion && !esConsejoVigilancia)) return
            crear.mutate()
          }}
        >
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
              <BuscarPersona idExcluida="" onSeleccionar={setPersona} />
            )}
          </div>

          <div className="flex flex-col gap-2 rounded-lg border border-black/[0.06] p-3">
            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input type="checkbox" checked={esAsambleaGeneral} onChange={(e) => setEsAsambleaGeneral(e.target.checked)} />
              Asamblea General
            </label>
            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input
                type="checkbox"
                checked={esConsejoAdministracion}
                onChange={(e) => setEsConsejoAdministracion(e.target.checked)}
              />
              Consejo de Administración
            </label>
            <label className="flex items-center gap-2 text-sm text-graphite-600">
              <input type="checkbox" checked={esConsejoVigilancia} onChange={(e) => setEsConsejoVigilancia(e.target.checked)} />
              Consejo de Vigilancia
            </label>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Fecha de inicio</span>
              <input
                type="date"
                value={fechaInicia}
                onChange={(e) => setFechaInicia(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Fecha de término</span>
              <input
                type="date"
                value={fechaTermina}
                onChange={(e) => setFechaTermina(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          </div>

          <div>
            <button
              type="submit"
              disabled={crear.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {crear.isPending ? 'Guardando…' : 'Registrar miembro'}
            </button>
          </div>

          {crear.isError && <p className="text-sm text-red-700">{mensajeErrorSocios(crear.error, 'No se pudo registrar.')}</p>}
        </form>
      </div>
    </div>
  )
}

function SeccionOrganoGobierno() {
  const queryClient = useQueryClient()
  const [mostrarNuevo, setMostrarNuevo] = useState(false)

  const { data: miembros, isLoading } = useQuery<MiembroOrganoGobierno[]>({
    queryKey: ['organo-gobierno'],
    queryFn: async () => (await api.get('/api/socios/organo-gobierno')).data,
  })

  const cambiarEstado = useMutation({
    mutationFn: async ({ id, activo }: { id: string; activo: boolean }) =>
      (await api.patch(`/api/socios/organo-gobierno/${id}/estado`, activo)).data,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['organo-gobierno'] }),
  })

  const cargos = (m: MiembroOrganoGobierno) => {
    const lista: string[] = []
    if (m.esAsambleaGeneral) lista.push('Asamblea General')
    if (m.esConsejoAdministracion) lista.push('Consejo de Administración')
    if (m.esConsejoVigilancia) lista.push('Consejo de Vigilancia')
    return lista.join(', ') || '—'
  }

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <p className="text-sm text-graphite-600">
          Asamblea General, Consejo de Administración y Consejo de Vigilancia — órganos de gobierno reales y
          obligatorios en una COAC ecuatoriana.
        </p>
        <button
          type="button"
          onClick={() => setMostrarNuevo(true)}
          className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
        >
          <Plus size={16} /> Nuevo miembro
        </button>
      </div>

      {mostrarNuevo && (
        <ModalPortal>
          <NuevoMiembroOrganoGobiernoModal onClose={() => setMostrarNuevo(false)} />
        </ModalPortal>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Persona</Th>
            <Th>Identificación</Th>
            <Th>Cargos</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (miembros?.length ?? 0) === 0 && <EmptyState>Todavía no hay miembros registrados</EmptyState>}
          {miembros?.map((m) => (
            <tr key={m.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{m.persona}</Td>
              <Td>{m.identificacion}</Td>
              <Td>{cargos(m)}</Td>
              <Td>{m.activo ? <Badge variant="exito">Activo</Badge> : <Badge variant="neutral">Inactivo</Badge>}</Td>
              <Td>
                <button
                  type="button"
                  onClick={() => cambiarEstado.mutate({ id: m.id, activo: !m.activo })}
                  className="text-xs font-medium text-petrol-700 hover:underline"
                >
                  {m.activo ? 'Desactivar' : 'Activar'}
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

const TABS = [
  { id: 'listado', label: 'Listado' },
  { id: 'reclamos', label: 'Reclamos', icon: MessageSquareWarning },
  { id: 'organo-gobierno', label: 'Consejo de Vigilancia', icon: Landmark },
  { id: 'reporte-s01', label: 'Reporte S01 (SEPS)' },
] as const
type TabId = (typeof TABS)[number]['id']

export function Socios() {
  const [q, setQ] = useState('')
  const [idAbierto, setIdAbierto] = useState<string | null>(null)
  const [idDetalle, setIdDetalle] = useState<string | null>(null)
  const [tab, setTab] = useState<TabId>('listado')
  const [mostrarNuevo, setMostrarNuevo] = useState(false)

  const { data, isLoading } = useQuery<Socio[]>({
    queryKey: ['socios', q],
    queryFn: async () => (await api.get('/api/socios', { params: { q: q || undefined } })).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Users} title="Socios" subtitle="Personas vinculadas a la cooperativa" />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {TABS.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`rounded-t-lg px-3 py-2 text-sm font-medium transition ${
              tab === t.id ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600 hover:text-graphite-100'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'reporte-s01' && <SeccionReporteS01 />}
      {tab === 'reclamos' && <SeccionReclamos />}
      {tab === 'organo-gobierno' && <SeccionOrganoGobierno />}

      {mostrarNuevo && (
        <ModalPortal>
          <NuevoSocioModal onClose={() => setMostrarNuevo(false)} />
        </ModalPortal>
      )}
      {idDetalle && (
        <ModalPortal>
          <DetalleSocioModal idCliente={idDetalle} onClose={() => setIdDetalle(null)} />
        </ModalPortal>
      )}

      {tab === 'listado' && (
        <>
      <div className="mb-4 flex items-center gap-3">
        <div className="flex-1">
          <SearchBar value={q} onChange={setQ} placeholder="Buscar por nombre, número o identificación…" />
        </div>
        <button
          type="button"
          onClick={() => setMostrarNuevo(true)}
          className="flex items-center gap-1 rounded-lg bg-gold-500 px-3 py-2 text-xs font-medium text-white hover:bg-gold-600"
        >
          <Plus size={14} /> Nuevo socio
        </button>
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Nombre</Th>
            <Th>Identificación</Th>
            <Th>Agencia</Th>
            <Th>Estado</Th>
            <Th>Provincia de domicilio</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (data?.length ?? 0) === 0 && <EmptyState>No se encontraron socios</EmptyState>}
          {data?.map((s) => (
            <Fragment key={s.id}>
              <tr
                onDoubleClick={() => setIdDetalle(s.id)}
                title="Doble clic para ver el detalle completo"
                className="cursor-pointer border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]"
              >
                <Td className="font-medium">{s.numero}</Td>
                <Td>{s.nombre}</Td>
                <Td>{s.identificacion}</Td>
                <Td>{s.agencia}</Td>
                <Td>
                  <EstadoSocioCelda socio={s} />
                </Td>
                <Td>
                  <div className="flex items-center gap-3">
                    <ProvinciaCelda socio={s} />
                    <button
                      type="button"
                      onClick={() => setIdAbierto(idAbierto === s.id ? null : s.id)}
                      className="flex items-center gap-1 text-xs font-medium text-gold-400 hover:underline"
                    >
                      <IdCard size={13} /> Datos del socio
                    </button>
                  </div>
                </Td>
              </tr>
              {idAbierto === s.id && <DatosSocioPanel socio={s} />}
            </Fragment>
          ))}
        </tbody>
      </TableContainer>
        </>
      )}
    </div>
  )
}
