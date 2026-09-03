import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ShieldAlert, Plus, X } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { BotonesExportar } from '../components/BotonesExportar'
import type { ColumnaExportable } from '../lib/exportar'
import { api } from '../lib/api'

interface Accion {
  id: number
  codigo: string
  nombre: string
}

interface PrestamoParaCobranza {
  id: string
  idCliente: string
  numero: string
  socio: string
  saldo: number
  estado: string
  diasMora: number
  codigoPeriodoMora: string
  nombrePeriodoMora: string
}

interface ResumenMoraTramo {
  codigo: string
  nombre: string
  cantidadPrestamos: number
  saldoTotal: number
}

interface GastoCobranzaEstimado {
  idPrestamo: string
  numero: string
  saldo: number
  diasMora: number
  montoCuotaVencida: number
  codigoTarifa: string | null
  gastoEstimado: number | null
}

interface UsuarioCumplimiento {
  id: string
  nombreUsuario: string
}

interface EstadoHallazgo {
  codigo: string
  nombre: string
}

interface HallazgoEtapaItem {
  codigoEstado: string
  estado: string
  comentario: string
  registradoPor: string
  fecha: string
}

interface HallazgoRespuestaItem {
  respuesta: string
  fecha: string
}

interface HallazgoUsuarioItem {
  id: string
  usuario: string
  estadoRespondido: boolean
  respuestas: HallazgoRespuestaItem[]
}

interface HallazgoItem {
  id: string
  nombre: string
  detalle: string
  reportadoPor: string
  codigoEstado: string
  estado: string
  fecha: string
  asignados: HallazgoUsuarioItem[]
  bitacora: HallazgoEtapaItem[]
}

function tramoVariant(codigo: string) {
  if (codigo === 'PREV') return 'neutral' as const
  if (codigo === 'GEST') return 'alerta' as const
  return 'peligro' as const
}

interface Gestion {
  id: string
  prestamoNumero: string
  socio: string
  accion: string
  tieneCompromisoPago: boolean
  observacion: string | null
  fecha: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function RegistrarGestionForm({
  prestamos,
  acciones,
  onClose,
}: {
  prestamos: PrestamoParaCobranza[]
  acciones: Accion[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [idPrestamo, setIdPrestamo] = useState('')
  const [codigoAccionGestion, setCodigoAccionGestion] = useState(acciones[0]?.codigo ?? '')
  const [tieneCompromisoPago, setTieneCompromisoPago] = useState(false)
  const [observacion, setObservacion] = useState('')

  const prestamoSeleccionado = prestamos.find((p) => p.id === idPrestamo)

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/cobranzas/gestiones', {
          idPrestamo,
          idCliente: prestamoSeleccionado?.idCliente,
          esDeudor: true,
          codigoAccionGestion,
          tieneCompromisoPago,
          observacion: observacion || null,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cobranzas-gestiones'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar gestión de cobranza</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idPrestamo) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Préstamo</span>
          <select
            required
            value={idPrestamo}
            onChange={(e) => setIdPrestamo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {prestamos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.numero} — {p.socio} (saldo {formatoUsd(p.saldo)}, {p.diasMora} días de mora — {p.nombrePeriodoMora})
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Acción</span>
          <select
            value={codigoAccionGestion}
            onChange={(e) => setCodigoAccionGestion(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            {acciones.map((a) => (
              <option key={a.codigo} value={a.codigo}>
                {a.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex items-center gap-2 text-sm sm:col-span-2">
          <input
            type="checkbox"
            checked={tieneCompromisoPago}
            onChange={(e) => setTieneCompromisoPago(e.target.checked)}
            className="h-4 w-4 rounded border-black/[0.2] accent-[#b58e4a]"
          />
          <span className="text-graphite-600">El socio se comprometió a pagar</span>
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Observación</span>
          <textarea
            value={observacion}
            onChange={(e) => setObservacion(e.target.value)}
            rows={2}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Guardando…' : 'Registrar'}
          </button>
        </div>

        {registrar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar la gestión.'}
          </p>
        )}
      </form>
    </div>
  )
}

const COLUMNAS_CARTERA_MORA: ColumnaExportable<PrestamoParaCobranza>[] = [
  { header: 'Préstamo', accessor: (p) => p.numero },
  { header: 'Socio', accessor: (p) => p.socio },
  { header: 'Saldo', accessor: (p) => p.saldo },
  { header: 'Días de mora', accessor: (p) => p.diasMora },
  { header: 'Tramo', accessor: (p) => p.nombrePeriodoMora },
]

const COLUMNAS_GESTIONES: ColumnaExportable<Gestion>[] = [
  { header: 'Préstamo', accessor: (g) => g.prestamoNumero },
  { header: 'Socio', accessor: (g) => g.socio },
  { header: 'Acción', accessor: (g) => g.accion },
  { header: 'Compromiso de pago', accessor: (g) => (g.tieneCompromisoPago ? 'Sí' : 'No') },
  { header: 'Observación', accessor: (g) => g.observacion ?? '' },
  { header: 'Fecha', accessor: (g) => g.fecha },
]

function SeccionCarteraEnMora({ prestamos }: { prestamos: PrestamoParaCobranza[] | undefined }) {
  const { data: resumen, isLoading } = useQuery<ResumenMoraTramo[]>({
    queryKey: ['cobranzas-mora-resumen'],
    queryFn: async () => (await api.get('/api/cobranzas/mora/resumen')).data,
  })

  const { data: gastos } = useQuery<GastoCobranzaEstimado[]>({
    queryKey: ['cobranzas-mora-gasto-cobranza'],
    queryFn: async () => (await api.get('/api/cobranzas/mora/gasto-cobranza')).data,
  })
  const gastoPorPrestamo = new Map((gastos ?? []).map((g) => [g.idPrestamo, g]))

  return (
    <div>
      <div className="mb-6 grid grid-cols-2 gap-3 sm:grid-cols-5">
        {resumen?.map((r) => (
          <div key={r.codigo} className="glass-card rounded-xl p-3">
            <Badge variant={tramoVariant(r.codigo)}>{r.nombre}</Badge>
            <p className="mt-2 text-2xl font-semibold tabular-nums text-graphite-100">{r.cantidadPrestamos}</p>
            <p className="text-xs text-graphite-600">{formatoUsd(r.saldoTotal)} en saldo</p>
          </div>
        ))}
      </div>

      <div className="mb-2 flex items-center justify-between">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-graphite-600">Préstamos vencidos</h2>
        <BotonesExportar
          nombreArchivo="cobranzas_cartera_mora"
          titulo="Cartera en mora"
          columnas={COLUMNAS_CARTERA_MORA}
          filas={prestamos ?? []}
        />
      </div>
      <TableContainer>
        <thead>
          <tr>
            <Th>Préstamo</Th>
            <Th>Socio</Th>
            <Th>Saldo</Th>
            <Th>Días de mora</Th>
            <Th>Tramo</Th>
            <Th>Gasto de cobranza (est.)</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (prestamos?.length ?? 0) === 0 && (
            <EmptyState>Ningún préstamo vigente está vencido — cartera al día</EmptyState>
          )}
          {prestamos?.map((p) => {
            const gasto = gastoPorPrestamo.get(p.id)
            return (
              <tr key={p.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{p.numero}</Td>
                <Td>{p.socio}</Td>
                <Td className="tabular-nums">{formatoUsd(p.saldo)}</Td>
                <Td className="tabular-nums">{p.diasMora}</Td>
                <Td>
                  <Badge variant={tramoVariant(p.codigoPeriodoMora)}>{p.nombrePeriodoMora}</Badge>
                </Td>
                <Td className="tabular-nums" title="Estimado según tarifario F01 SEPS — informativo, no se cobra automáticamente">
                  {gasto?.gastoEstimado != null ? formatoUsd(gasto.gastoEstimado) : '—'}
                </Td>
              </tr>
            )
          })}
        </tbody>
      </TableContainer>
    </div>
  )
}

function SeccionGestiones({ prestamos, acciones }: { prestamos: PrestamoParaCobranza[] | undefined; acciones: Accion[] | undefined }) {
  const [mostrarForm, setMostrarForm] = useState(false)

  const { data: gestiones, isLoading } = useQuery<Gestion[]>({
    queryKey: ['cobranzas-gestiones'],
    queryFn: async () => (await api.get('/api/cobranzas/gestiones')).data,
  })

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-graphite-600">Gestiones registradas</h2>
        <div className="flex items-center gap-2">
          <BotonesExportar
            nombreArchivo="cobranzas_gestiones"
            titulo="Gestiones de cobranza"
            columnas={COLUMNAS_GESTIONES}
            filas={gestiones ?? []}
          />
          {!mostrarForm && (
            <button
              type="button"
              onClick={() => setMostrarForm(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Registrar gestión
            </button>
          )}
        </div>
      </div>

      {mostrarForm && prestamos && acciones && (
        <RegistrarGestionForm prestamos={prestamos} acciones={acciones} onClose={() => setMostrarForm(false)} />
      )}

      {mostrarForm && (prestamos?.length ?? 0) === 0 && (
        <p className="mb-4 rounded-lg bg-gold-500/10 px-3 py-2 text-sm text-gold-300">
          No hay préstamos vencidos ahora mismo — no hay nada que gestionar en cobranza.
        </p>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Préstamo</Th>
            <Th>Socio</Th>
            <Th>Acción</Th>
            <Th>Compromiso de pago</Th>
            <Th>Observación</Th>
            <Th>Fecha</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (gestiones?.length ?? 0) === 0 && <EmptyState>Todavía no hay gestiones de cobranza registradas</EmptyState>}
          {gestiones?.map((g) => (
            <tr key={g.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{g.prestamoNumero}</Td>
              <Td>{g.socio}</Td>
              <Td>{g.accion}</Td>
              <Td>
                <Badge variant={g.tieneCompromisoPago ? 'exito' : 'neutral'}>
                  {g.tieneCompromisoPago ? 'Sí' : 'No'}
                </Badge>
              </Td>
              <Td>{g.observacion ?? '—'}</Td>
              <Td>{g.fecha}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface TipoListaControl {
  codigo: string
  nombre: string
}

interface PersonaBusqueda {
  id: string
  nombre: string
  identificacion: string
}

interface AlertaListaControl {
  id: string
  persona: string
  identificacion: string
  codigoTipoListaControl: string
  tipoListaControl: string
  detalle: string
  fechaDeteccion: string
  resuelta: boolean
  comentarioResolucion: string | null
  resueltoPor: string | null
  registradoPor: string
}

function RegistrarAlertaForm({ tipos, onClose }: { tipos: TipoListaControl[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [q, setQ] = useState('')
  const [idPersona, setIdPersona] = useState('')
  const [personaNombre, setPersonaNombre] = useState('')
  const [codigoTipo, setCodigoTipo] = useState(tipos[0]?.codigo ?? '')
  const [detalle, setDetalle] = useState('')

  const { data: personas } = useQuery<PersonaBusqueda[]>({
    queryKey: ['cobranzas-listacontrol-personas', q],
    queryFn: async () => (await api.get('/api/cobranzas/listas-control/personas', { params: { q: q || undefined } })).data,
    enabled: q.length >= 2,
  })

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/cobranzas/listas-control/alertas', {
          idPersona,
          codigoTipoListaControl: codigoTipo,
          detalle,
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cobranzas-listacontrol-alertas'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar alerta de lista de control</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idPersona || !detalle) return
          registrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Persona (buscar por nombre o identificación)</span>
          <input
            value={idPersona ? personaNombre : q}
            onChange={(e) => {
              setIdPersona('')
              setQ(e.target.value)
            }}
            placeholder="Escribir al menos 2 caracteres…"
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
          {!idPersona && q.length >= 2 && (personas?.length ?? 0) > 0 && (
            <div className="max-h-40 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
              {personas?.map((p) => (
                <button
                  key={p.id}
                  type="button"
                  onClick={() => {
                    setIdPersona(p.id)
                    setPersonaNombre(`${p.nombre} — ${p.identificacion}`)
                  }}
                  className="block w-full px-3 py-2 text-left text-sm hover:bg-black/[0.02]"
                >
                  {p.nombre} — {p.identificacion}
                </button>
              ))}
            </div>
          )}
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Tipo de lista de control</span>
          <select
            value={codigoTipo}
            onChange={(e) => setCodigoTipo(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            {tipos.map((t) => (
              <option key={t.codigo} value={t.codigo}>
                {t.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Detalle de la coincidencia</span>
          <textarea
            required
            value={detalle}
            onChange={(e) => setDetalle(e.target.value)}
            rows={2}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={!idPersona || registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Guardando…' : 'Registrar alerta'}
          </button>
        </div>

        {registrar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(registrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar la alerta.'}
          </p>
        )}
      </form>
    </div>
  )
}

function SeccionListasControl() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [soloNoResueltas, setSoloNoResueltas] = useState(true)
  const [resolviendo, setResolviendo] = useState<string | null>(null)
  const [comentarioResolucion, setComentarioResolucion] = useState('')
  const queryClient = useQueryClient()

  const { data: tipos } = useQuery<TipoListaControl[]>({
    queryKey: ['cobranzas-listacontrol-tipos'],
    queryFn: async () => (await api.get('/api/cobranzas/listas-control/tipos')).data,
  })

  const { data: alertas, isLoading } = useQuery<AlertaListaControl[]>({
    queryKey: ['cobranzas-listacontrol-alertas', soloNoResueltas],
    queryFn: async () =>
      (await api.get('/api/cobranzas/listas-control/alertas', { params: { soloNoResueltas } })).data,
  })

  const resolver = useMutation({
    mutationFn: async ({ id, comentario }: { id: string; comentario: string }) =>
      api.post(`/api/cobranzas/listas-control/alertas/${id}/resolver`, { comentario }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cobranzas-listacontrol-alertas'] })
      setResolviendo(null)
      setComentarioResolucion('')
    },
  })

  return (
    <div>
      <p className="mb-4 text-xs text-graphite-600">
        Mientras una persona tenga una alerta sin resolver, cualquier movimiento sobre sus cuentas de ahorro queda en
        espera de autorización de un supervisor (ver Cajas → Autorizaciones pendientes).
      </p>

      <div className="mb-4 flex items-center justify-between">
        <label className="flex items-center gap-2 text-sm text-graphite-600">
          <input
            type="checkbox"
            checked={soloNoResueltas}
            onChange={(e) => setSoloNoResueltas(e.target.checked)}
            className="h-4 w-4 rounded border-black/[0.2] accent-[#b58e4a]"
          />
          Mostrar solo sin resolver
        </label>
        {!mostrarForm && tipos && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <Plus size={16} /> Registrar alerta
          </button>
        )}
      </div>

      {mostrarForm && tipos && <RegistrarAlertaForm tipos={tipos} onClose={() => setMostrarForm(false)} />}

      <TableContainer>
        <thead>
          <tr>
            <Th>Persona</Th>
            <Th>Identificación</Th>
            <Th>Tipo de lista</Th>
            <Th>Detalle</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (alertas?.length ?? 0) === 0 && <EmptyState>No hay alertas de lista de control</EmptyState>}
          {alertas?.map((a) => (
            <tr key={a.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{a.persona}</Td>
              <Td>{a.identificacion}</Td>
              <Td>{a.tipoListaControl}</Td>
              <Td className="text-xs text-graphite-600">{a.detalle}</Td>
              <Td>{a.fechaDeteccion}</Td>
              <Td>
                <Badge variant={a.resuelta ? 'neutral' : 'peligro'}>{a.resuelta ? 'Resuelta' : 'Sin resolver'}</Badge>
              </Td>
              <Td>
                {!a.resuelta &&
                  (resolviendo === a.id ? (
                    <div className="flex items-center gap-2">
                      <input
                        autoFocus
                        value={comentarioResolucion}
                        onChange={(e) => setComentarioResolucion(e.target.value)}
                        placeholder="Motivo (ej. falso positivo)"
                        className="rounded border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
                      />
                      <button
                        type="button"
                        disabled={!comentarioResolucion || resolver.isPending}
                        onClick={() => resolver.mutate({ id: a.id, comentario: comentarioResolucion })}
                        className="text-xs font-medium text-gold-400 hover:underline disabled:opacity-50"
                      >
                        Confirmar
                      </button>
                      <button
                        type="button"
                        onClick={() => setResolviendo(null)}
                        className="text-xs text-graphite-600 hover:underline"
                      >
                        Cancelar
                      </button>
                    </div>
                  ) : (
                    <button
                      type="button"
                      onClick={() => setResolviendo(a.id)}
                      className="text-sm font-medium text-gold-400 hover:underline"
                    >
                      Resolver
                    </button>
                  ))}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

function NuevoHallazgoForm({ usuarios, onClose }: { usuarios: UsuarioCumplimiento[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [nombre, setNombre] = useState('')
  const [detalle, setDetalle] = useState('')
  const [idUsuarioReporta, setIdUsuarioReporta] = useState('')
  const [idsUsuarioAsignado, setIdsUsuarioAsignado] = useState<string[]>([])

  const toggleAsignado = (id: string) => {
    setIdsUsuarioAsignado((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]))
  }

  const crear = useMutation({
    mutationFn: async () =>
      (await api.post('/api/cobranzas/hallazgos', { nombre, detalle, idUsuarioReporta, idsUsuarioAsignado })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cobranzas-hallazgos'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Registrar hallazgo</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!nombre || !detalle || !idUsuarioReporta || idsUsuarioAsignado.length === 0) return
          crear.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Nombre del hallazgo</span>
          <input required value={nombre} onChange={(e) => setNombre(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Detalle</span>
          <textarea required rows={3} value={detalle} onChange={(e) => setDetalle(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
        </label>
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Reportado por</span>
          <select
            required
            value={idUsuarioReporta}
            onChange={(e) => setIdUsuarioReporta(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {usuarios.map((u) => (
              <option key={u.id} value={u.id}>
                {u.nombreUsuario}
              </option>
            ))}
          </select>
        </label>
        <div className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Auditados (asignar)</span>
          <div className="flex flex-wrap gap-1.5">
            {usuarios.map((u) => (
              <button
                key={u.id}
                type="button"
                onClick={() => toggleAsignado(u.id)}
                className={`rounded-full px-2.5 py-1 text-xs font-medium transition ${
                  idsUsuarioAsignado.includes(u.id)
                    ? 'bg-gold-500 text-white'
                    : 'bg-black/[0.04] text-graphite-600 hover:bg-black/[0.08]'
                }`}
              >
                {u.nombreUsuario}
              </button>
            ))}
          </div>
        </div>
        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={crear.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {crear.isPending ? 'Registrando…' : 'Registrar hallazgo'}
          </button>
        </div>
        {crear.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(crear.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo registrar el hallazgo.'}
          </p>
        )}
      </form>
    </div>
  )
}

function ResponderHallazgoForm({ idHallazgoUsuario, onDone }: { idHallazgoUsuario: string; onDone: () => void }) {
  const queryClient = useQueryClient()
  const [respuesta, setRespuesta] = useState('')

  const responder = useMutation({
    mutationFn: async () => api.post(`/api/cobranzas/hallazgos/asignaciones/${idHallazgoUsuario}/responder`, { respuesta }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cobranzas-hallazgos'] })
      onDone()
    },
  })

  return (
    <form
      className="mt-1 flex items-center gap-2"
      onSubmit={(e) => {
        e.preventDefault()
        if (!respuesta) return
        responder.mutate()
      }}
    >
      <input
        required
        value={respuesta}
        onChange={(e) => setRespuesta(e.target.value)}
        placeholder="Respuesta del auditado"
        className="w-full rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
      />
      <button type="submit" disabled={responder.isPending} className="text-xs font-medium text-petrol-700 hover:underline disabled:opacity-50">
        Enviar
      </button>
    </form>
  )
}

function CambiarEstadoHallazgoForm({ idHallazgo, onDone }: { idHallazgo: string; onDone: () => void }) {
  const queryClient = useQueryClient()
  const [codigoEstadoNuevo, setCodigoEstadoNuevo] = useState('')
  const [comentario, setComentario] = useState('')

  const { data: estados } = useQuery<EstadoHallazgo[]>({
    queryKey: ['cobranzas-estados-hallazgo'],
    queryFn: async () => (await api.get('/api/cobranzas/estados-hallazgo')).data,
  })

  const cambiar = useMutation({
    mutationFn: async () => api.post(`/api/cobranzas/hallazgos/${idHallazgo}/estado`, { codigoEstadoNuevo, comentario }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cobranzas-hallazgos'] })
      onDone()
    },
  })

  return (
    <form
      className="flex flex-wrap items-center gap-2"
      onSubmit={(e) => {
        e.preventDefault()
        if (!codigoEstadoNuevo || !comentario) return
        cambiar.mutate()
      }}
    >
      <select
        required
        value={codigoEstadoNuevo}
        onChange={(e) => setCodigoEstadoNuevo(e.target.value)}
        className="rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
      >
        <option value="">Nuevo estado…</option>
        {estados?.map((e) => (
          <option key={e.codigo} value={e.codigo}>
            {e.nombre}
          </option>
        ))}
      </select>
      <input
        required
        value={comentario}
        onChange={(e) => setComentario(e.target.value)}
        placeholder="Comentario"
        className="rounded-lg border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
      />
      <button type="submit" disabled={cambiar.isPending} className="text-xs font-medium text-petrol-700 hover:underline disabled:opacity-50">
        Registrar
      </button>
    </form>
  )
}

function hallazgoEstadoVariant(codigo: string) {
  if (codigo === 'FI') return 'exito' as const
  if (codigo === 'IN') return 'alerta' as const
  return 'neutral' as const
}

function SeccionHallazgos() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [idExpandido, setIdExpandido] = useState<string | null>(null)
  const [cambiandoEstado, setCambiandoEstado] = useState<string | null>(null)
  const [respondiendo, setRespondiendo] = useState<string | null>(null)

  const { data: usuarios } = useQuery<UsuarioCumplimiento[]>({
    queryKey: ['cobranzas-usuarios'],
    queryFn: async () => (await api.get('/api/cobranzas/usuarios')).data,
  })

  const { data: hallazgos, isLoading } = useQuery<HallazgoItem[]>({
    queryKey: ['cobranzas-hallazgos'],
    queryFn: async () => (await api.get('/api/cobranzas/hallazgos')).data,
  })

  return (
    <div>
      <div className="mb-4 flex items-center justify-between">
        <h3 className="text-sm font-medium text-graphite-600">Hallazgos de auditoría / cumplimiento</h3>
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Registrar hallazgo
          </button>
        )}
      </div>

      {mostrarForm && usuarios && <NuevoHallazgoForm usuarios={usuarios} onClose={() => setMostrarForm(false)} />}

      <TableContainer>
        <thead>
          <tr>
            <Th>Nombre</Th>
            <Th>Reportado por</Th>
            <Th>Asignados</Th>
            <Th>Estado</Th>
            <Th>Fecha</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (hallazgos?.length ?? 0) === 0 && <EmptyState>Todavía no hay hallazgos registrados</EmptyState>}
          {hallazgos?.map((h) => (
            <>
              <tr key={h.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{h.nombre}</Td>
                <Td>{h.reportadoPor}</Td>
                <Td>{h.asignados.map((a) => a.usuario).join(', ')}</Td>
                <Td>
                  <Badge variant={hallazgoEstadoVariant(h.codigoEstado)}>{h.estado}</Badge>
                </Td>
                <Td>{h.fecha}</Td>
                <Td>
                  <button
                    type="button"
                    onClick={() => setIdExpandido(idExpandido === h.id ? null : h.id)}
                    className="text-xs font-medium text-graphite-600 hover:underline"
                  >
                    Detalle
                  </button>
                </Td>
              </tr>
              {idExpandido === h.id && (
                <tr className="border-b border-black/[0.04] bg-black/[0.015]">
                  <td colSpan={6} className="px-4 py-3">
                    <p className="mb-2 text-xs"><span className="text-graphite-600">Detalle:</span> {h.detalle}</p>

                    <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-gold-400">Asignados</p>
                    <div className="mb-3 flex flex-col gap-2">
                      {h.asignados.map((a) => (
                        <div key={a.id} className="rounded-lg border border-black/[0.06] p-2 text-xs">
                          <div className="flex items-center justify-between">
                            <span className="font-medium text-graphite-100">{a.usuario}</span>
                            <Badge variant={a.estadoRespondido ? 'exito' : 'neutral'}>
                              {a.estadoRespondido ? 'Respondió' : 'Sin respuesta'}
                            </Badge>
                          </div>
                          {a.respuestas.map((r, i) => (
                            <p key={i} className="mt-1 text-graphite-600">
                              {new Date(r.fecha).toLocaleString('es-EC')}: {r.respuesta}
                            </p>
                          ))}
                          {respondiendo === a.id ? (
                            <ResponderHallazgoForm idHallazgoUsuario={a.id} onDone={() => setRespondiendo(null)} />
                          ) : (
                            <button
                              type="button"
                              onClick={() => setRespondiendo(a.id)}
                              className="mt-1 text-xs font-medium text-petrol-700 hover:underline"
                            >
                              Responder
                            </button>
                          )}
                        </div>
                      ))}
                    </div>

                    <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-gold-400">Bitácora</p>
                    <ul className="mb-3 flex flex-col gap-0.5 text-xs text-graphite-600">
                      {h.bitacora.map((e, i) => (
                        <li key={i}>
                          {new Date(e.fecha).toLocaleString('es-EC')} — <span className="font-medium text-graphite-100">{e.estado}</span> ({e.registradoPor}): {e.comentario}
                        </li>
                      ))}
                    </ul>

                    {cambiandoEstado === h.id ? (
                      <CambiarEstadoHallazgoForm idHallazgo={h.id} onDone={() => setCambiandoEstado(null)} />
                    ) : (
                      <button
                        type="button"
                        onClick={() => setCambiandoEstado(h.id)}
                        className="text-xs font-medium text-graphite-600 hover:underline"
                      >
                        Cambiar estado
                      </button>
                    )}
                  </td>
                </tr>
              )}
            </>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

const TABS = [
  { id: 'mora', label: 'Cartera en mora' },
  { id: 'gestiones', label: 'Gestiones' },
  { id: 'listas-control', label: 'Listas de control' },
  { id: 'hallazgos', label: 'Hallazgos de auditoría' },
] as const
type TabId = (typeof TABS)[number]['id']

export function CobranzasCumplimiento() {
  const [tab, setTab] = useState<TabId>('mora')

  const { data: acciones } = useQuery<Accion[]>({
    queryKey: ['cobranzas-acciones'],
    queryFn: async () => (await api.get('/api/cobranzas/acciones')).data,
  })

  const { data: prestamos } = useQuery<PrestamoParaCobranza[]>({
    queryKey: ['cobranzas-prestamos'],
    queryFn: async () => (await api.get('/api/cobranzas/prestamos')).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={ShieldAlert}
        title="Cobranzas y Cumplimiento"
        subtitle="Gestión de mora y prevención de lavado de activos"
      />

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

      {tab === 'mora' && <SeccionCarteraEnMora prestamos={prestamos} />}
      {tab === 'gestiones' && <SeccionGestiones prestamos={prestamos} acciones={acciones} />}
      {tab === 'listas-control' && <SeccionListasControl />}
      {tab === 'hallazgos' && <SeccionHallazgos />}
    </div>
  )
}
