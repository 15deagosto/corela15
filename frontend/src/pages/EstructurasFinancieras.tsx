import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { FileCheck, Lock, Plus, X, IdCard, Landmark, Percent, CalendarDays, Download, RefreshCw } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { BotonesExportar } from '../components/BotonesExportar'
import { ModalPortal } from '../components/ModalPortal'
import { useAuth } from '../lib/AuthContext'
import { api } from '../lib/api'

interface Catalogo {
  codigo: string
  nombre: string
}

interface ObligacionFinanciera {
  id: string
  agencia: string
  tipoIdentificacionAcreedor: string
  identificacionAcreedor: string
  codigoPaisAcreedor: string
  paisAcreedor: string
  numeroObligacion: string
  destinoLineaCredito: string
  montoLineaCredito: number
  montoPorUtilizar: number
  codigoEstado: string
  estado: string
  saldo: number
  codigoCuentaContable: string
  nombreCuentaContable: string
  tasaInteres: number
  interesesPorPagar: number
  pagaComision: boolean
  tasaInteresComision: number | null
  valorComision: number | null
  fechaConcesion: string
  fechaVencimiento: string
  codigoPeriodicidadPago: string
  periodicidadPago: string
  tienePeriodoGracia: boolean
  numeroPeriodosGracia: number | null
  codigoClase: string
  clase: string
  valorVencido: number | null
  codigoFormaCancelacion: string | null
  formaCancelacion: string | null
  numeroObligacionAnterior: string | null
}

interface Of01Result {
  cabecera: { codigoEstructura: string; ruc: string; fechaCorte: string; numeroTotalRegistros: number }
  detalle: Array<Record<string, unknown>>
  advertencias: string[]
}

const BASE = '/api/estructuras-financieras/obligaciones'

const INPUT_CLASS =
  'w-full rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50'

function estadoVariant(codigo: string): 'exito' | 'peligro' | 'neutral' | 'alerta' {
  if (codigo === 'VN') return 'peligro'
  if (codigo === 'CN') return 'neutral'
  if (codigo === 'NV') return 'alerta'
  return 'exito'
}

const TABS_MODULO = [
  { id: 'obligaciones', label: 'Obligaciones financieras' },
  { id: 'generar', label: 'Generar OF01' },
] as const
type TabModulo = (typeof TABS_MODULO)[number]['id']

export function EstructurasFinancieras() {
  const { tieneEstructura } = useAuth()
  const [tab, setTab] = useState<TabModulo>('obligaciones')

  const autorizado = tieneEstructura('OF01')

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={FileCheck}
        title="Estructuras y Procesos Financieros"
        subtitle="Generador de estructuras regulatorias — captura de datos, validación y exportación"
      />

      {!autorizado ? (
        <div className="glass-card rounded-xl p-5">
          <div className="mb-2 flex items-center gap-2">
            <Lock size={15} className="text-graphite-600" />
            <h3 className="font-medium text-graphite-100">Obligaciones Financieras (OF01)</h3>
          </div>
          <p className="text-sm text-graphite-600">
            No tienes permiso para esta estructura — pídele a un administrador que te lo asigne desde
            Configuración → Roles → Estructuras.
          </p>
        </div>
      ) : (
        <>
          <div className="mb-4 flex gap-1 border-b border-black/[0.06]">
            {TABS_MODULO.map((t) => (
              <button
                key={t.id}
                type="button"
                onClick={() => setTab(t.id)}
                className={`px-4 py-2.5 text-sm font-medium transition-colors ${
                  tab === t.id
                    ? 'border-b-2 border-gold-500 text-graphite-100'
                    : 'text-graphite-600 hover:text-graphite-100'
                }`}
              >
                {t.label}
              </button>
            ))}
          </div>

          {tab === 'obligaciones' ? <SeccionObligaciones /> : <SeccionGenerarOf01 />}
        </>
      )}
    </div>
  )
}

function SeccionObligaciones() {
  const queryClient = useQueryClient()
  const [modalNuevo, setModalNuevo] = useState(false)
  const [obligacionEditar, setObligacionEditar] = useState<ObligacionFinanciera | null>(null)

  const { data: obligaciones, isLoading } = useQuery<ObligacionFinanciera[]>({
    queryKey: ['of01-obligaciones'],
    queryFn: async () => (await api.get(BASE)).data,
  })

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <p className="text-sm text-graphite-600">
          Deuda real de la cooperativa con otras instituciones (grupo 26 del Catálogo Único de Cuentas) — base de
          la estructura OF01 exigida desde el corte del 30 de septiembre de 2026.
        </p>
        <button
          type="button"
          onClick={() => setModalNuevo(true)}
          className="btn-hover flex flex-shrink-0 items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
        >
          <Plus size={15} /> Nueva obligación
        </button>
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Acreedor</Th>
            <Th>Cuenta</Th>
            <Th>Saldo</Th>
            <Th>Estado</Th>
            <Th>Vencimiento</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading ? (
            <EmptyState>Cargando...</EmptyState>
          ) : !obligaciones || obligaciones.length === 0 ? (
            <EmptyState>Ninguna obligación financiera capturada todavía.</EmptyState>
          ) : (
            obligaciones.map((o) => (
              <tr
                key={o.id}
                className="cursor-pointer border-t border-black/[0.04] hover:bg-black/[0.015]"
                onDoubleClick={() => setObligacionEditar(o)}
                title="Doble clic para gestionar"
              >
                <Td>{o.numeroObligacion}</Td>
                <Td>
                  {o.tipoIdentificacionAcreedor} {o.identificacionAcreedor}
                </Td>
                <Td>
                  {o.codigoCuentaContable} — {o.nombreCuentaContable}
                </Td>
                <Td className="text-right tabular-nums">${o.saldo.toFixed(2)}</Td>
                <Td>
                  <Badge variant={estadoVariant(o.codigoEstado)}>{o.estado}</Badge>
                </Td>
                <Td>{o.fechaVencimiento}</Td>
              </tr>
            ))
          )}
        </tbody>
      </TableContainer>

      {modalNuevo && (
        <ModalPortal>
          <ObligacionModal
            modo="crear"
            onClose={() => setModalNuevo(false)}
            onGuardado={() => {
              setModalNuevo(false)
              queryClient.invalidateQueries({ queryKey: ['of01-obligaciones'] })
            }}
          />
        </ModalPortal>
      )}

      {obligacionEditar && (
        <ModalPortal>
          <ObligacionModal
            modo="editar"
            obligacion={obligacionEditar}
            onClose={() => setObligacionEditar(null)}
            onGuardado={() => {
              setObligacionEditar(null)
              queryClient.invalidateQueries({ queryKey: ['of01-obligaciones'] })
            }}
          />
        </ModalPortal>
      )}
    </div>
  )
}

const TABS_MODAL_CREAR = [
  { id: 'acreedor', label: 'Acreedor', icon: IdCard },
  { id: 'linea', label: 'Línea y cuenta', icon: Landmark },
  { id: 'condiciones', label: 'Condiciones', icon: Percent },
  { id: 'fechas', label: 'Fechas y forma', icon: CalendarDays },
] as const

const TABS_MODAL_EDITAR = [
  { id: 'linea', label: 'Línea y estado', icon: Landmark },
  { id: 'condiciones', label: 'Condiciones', icon: Percent },
  { id: 'fechas', label: 'Fechas y forma', icon: CalendarDays },
] as const

function ObligacionModal({
  modo,
  obligacion,
  onClose,
  onGuardado,
}: {
  modo: 'crear' | 'editar'
  obligacion?: ObligacionFinanciera
  onClose: () => void
  onGuardado: () => void
}) {
  const tabs = modo === 'crear' ? TABS_MODAL_CREAR : TABS_MODAL_EDITAR
  const [tab, setTab] = useState<string>(modo === 'crear' ? 'acreedor' : 'linea')
  const [error, setError] = useState<string | null>(null)

  const { data: paises } = useQuery<Catalogo[]>({
    queryKey: ['of01-cat-paises'],
    queryFn: async () => (await api.get(`${BASE}/catalogos/paises`)).data,
  })
  const { data: estados } = useQuery<Catalogo[]>({
    queryKey: ['of01-cat-estados'],
    queryFn: async () => (await api.get(`${BASE}/catalogos/estados`)).data,
  })
  const { data: periodicidades } = useQuery<Catalogo[]>({
    queryKey: ['of01-cat-periodicidades'],
    queryFn: async () => (await api.get(`${BASE}/catalogos/periodicidades-pago`)).data,
  })
  const { data: clases } = useQuery<Catalogo[]>({
    queryKey: ['of01-cat-clases'],
    queryFn: async () => (await api.get(`${BASE}/catalogos/clases`)).data,
  })
  const { data: formasCancelacion } = useQuery<Catalogo[]>({
    queryKey: ['of01-cat-formas-cancelacion'],
    queryFn: async () => (await api.get(`${BASE}/catalogos/formas-cancelacion`)).data,
  })
  const { data: cuentas } = useQuery<Catalogo[]>({
    queryKey: ['of01-cat-cuentas'],
    queryFn: async () => (await api.get(`${BASE}/catalogos/cuentas-contables`)).data,
  })

  const [form, setForm] = useState(() => ({
    idAgencia: 1,
    tipoIdentificacionAcreedor: obligacion?.tipoIdentificacionAcreedor ?? 'R',
    identificacionAcreedor: obligacion?.identificacionAcreedor ?? '',
    codigoPaisAcreedor: obligacion?.codigoPaisAcreedor ?? 'ECU',
    numeroObligacion: obligacion?.numeroObligacion ?? '',
    destinoLineaCredito: obligacion?.destinoLineaCredito ?? '',
    montoLineaCredito: obligacion?.montoLineaCredito ?? 0,
    montoPorUtilizar: obligacion?.montoPorUtilizar ?? 0,
    codigoEstado: obligacion?.codigoEstado ?? 'NV',
    saldo: obligacion?.saldo ?? 0,
    idCuentaContable: '',
    codigoCuentaContable: obligacion?.codigoCuentaContable ?? '',
    tasaInteres: obligacion?.tasaInteres ?? 0,
    interesesPorPagar: obligacion?.interesesPorPagar ?? 0,
    pagaComision: obligacion?.pagaComision ?? false,
    tasaInteresComision: obligacion?.tasaInteresComision ?? null as number | null,
    valorComision: obligacion?.valorComision ?? null as number | null,
    fechaConcesion: obligacion?.fechaConcesion ?? '',
    fechaVencimiento: obligacion?.fechaVencimiento ?? '',
    codigoPeriodicidadPago: obligacion?.codigoPeriodicidadPago ?? 'ME',
    tienePeriodoGracia: obligacion?.tienePeriodoGracia ?? false,
    numeroPeriodosGracia: obligacion?.numeroPeriodosGracia ?? null as number | null,
    codigoClase: obligacion?.codigoClase ?? 'N',
    valorVencido: obligacion?.valorVencido ?? null as number | null,
    codigoFormaCancelacion: obligacion?.codigoFormaCancelacion ?? null as string | null,
    numeroObligacionAnterior: obligacion?.numeroObligacionAnterior ?? null as string | null,
  }))

  const crear = useMutation({
    mutationFn: async () => {
      const cuenta = cuentas?.find((c) => c.codigo === form.codigoCuentaContable)
      if (!cuenta) throw new Error('Seleccioná una cuenta contable real')
      await api.post(BASE, { ...form, idCuentaContable: idPorCodigoCuenta(cuentas, form.codigoCuentaContable) })
    },
    onSuccess: onGuardado,
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  const actualizar = useMutation({
    mutationFn: async () => {
      await api.put(`${BASE}/${obligacion!.id}`, {
        montoPorUtilizar: form.montoPorUtilizar,
        codigoEstado: form.codigoEstado,
        saldo: form.saldo,
        tasaInteres: form.tasaInteres,
        interesesPorPagar: form.interesesPorPagar,
        pagaComision: form.pagaComision,
        tasaInteresComision: form.tasaInteresComision,
        valorComision: form.valorComision,
        tienePeriodoGracia: form.tienePeriodoGracia,
        numeroPeriodosGracia: form.numeroPeriodosGracia,
        valorVencido: form.valorVencido,
        codigoFormaCancelacion: form.codigoFormaCancelacion,
      })
    },
    onSuccess: onGuardado,
    onError: (e: unknown) => setError(mensajeError(e)),
  })

  function actualizarCampo<K extends keyof typeof form>(campo: K, valor: (typeof form)[K]) {
    setForm((f) => ({ ...f, [campo]: valor }))
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
      <div
        className="glass-card flex max-h-[90vh] w-full max-w-3xl flex-col overflow-hidden rounded-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between border-b border-black/[0.06] p-5 pb-4">
          <h2 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
            <Landmark size={16} />
            {modo === 'crear' ? 'Nueva obligación financiera' : `Gestionar obligación ${obligacion?.numeroObligacion}`}
          </h2>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <div className="flex flex-1 overflow-hidden">
          <div className="flex w-44 flex-shrink-0 flex-col gap-0.5 border-r border-black/[0.06] p-2">
            {tabs.map((t) => (
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

          <div className="flex-1 space-y-3 overflow-y-auto p-5">
            {error && <p className="rounded-lg bg-red-500/10 px-3 py-2 text-xs text-red-600">{error}</p>}

            {tab === 'acreedor' && modo === 'crear' && (
              <>
                <Campo label="Tipo de identificación">
                  <select
                    value={form.tipoIdentificacionAcreedor}
                    onChange={(e) => actualizarCampo('tipoIdentificacionAcreedor', e.target.value)}
                    className={INPUT_CLASS}
                  >
                    <option value="R">R — RUC (persona jurídica nacional)</option>
                    <option value="X">X — Extranjero</option>
                  </select>
                </Campo>
                <Campo label="Identificación del acreedor">
                  <input
                    value={form.identificacionAcreedor}
                    onChange={(e) => actualizarCampo('identificacionAcreedor', e.target.value)}
                    className={INPUT_CLASS}
                  />
                </Campo>
                <Campo label="País del acreedor">
                  <select
                    value={form.codigoPaisAcreedor}
                    onChange={(e) => actualizarCampo('codigoPaisAcreedor', e.target.value)}
                    className={INPUT_CLASS}
                  >
                    {paises?.map((p) => (
                      <option key={p.codigo} value={p.codigo}>
                        {p.nombre}
                      </option>
                    ))}
                  </select>
                </Campo>
                <Campo label="Número de obligación">
                  <input
                    value={form.numeroObligacion}
                    onChange={(e) => actualizarCampo('numeroObligacion', e.target.value)}
                    className={INPUT_CLASS}
                  />
                </Campo>
                <Campo label="Número de obligación anterior (opcional — novación/refinanciamiento)">
                  <input
                    value={form.numeroObligacionAnterior ?? ''}
                    onChange={(e) => actualizarCampo('numeroObligacionAnterior', e.target.value || null)}
                    className={INPUT_CLASS}
                  />
                </Campo>
              </>
            )}

            {tab === 'linea' && (
              <>
                {modo === 'crear' && (
                  <>
                    <Campo label="Destino de la línea de crédito">
                      <input
                        value={form.destinoLineaCredito}
                        onChange={(e) => actualizarCampo('destinoLineaCredito', e.target.value)}
                        className={INPUT_CLASS}
                      />
                    </Campo>
                    <Campo label="Monto de la línea de crédito">
                      <input
                        type="number"
                        step="0.01"
                        value={form.montoLineaCredito}
                        onChange={(e) => actualizarCampo('montoLineaCredito', Number(e.target.value))}
                        className={INPUT_CLASS}
                      />
                    </Campo>
                    <Campo label="Cuenta contable (grupo 26, CUC)">
                      <select
                        value={form.codigoCuentaContable}
                        onChange={(e) => actualizarCampo('codigoCuentaContable', e.target.value)}
                        className={INPUT_CLASS}
                      >
                        <option value="">Seleccionar...</option>
                        {cuentas?.map((c) => (
                          <option key={c.codigo} value={c.codigo}>
                            {c.codigo} — {c.nombre}
                          </option>
                        ))}
                      </select>
                    </Campo>
                  </>
                )}
                <Campo label="Monto por utilizar">
                  <input
                    type="number"
                    step="0.01"
                    value={form.montoPorUtilizar}
                    onChange={(e) => actualizarCampo('montoPorUtilizar', Number(e.target.value))}
                    className={INPUT_CLASS}
                  />
                </Campo>
                <Campo label="Estado">
                  <select
                    value={form.codigoEstado}
                    onChange={(e) => actualizarCampo('codigoEstado', e.target.value)}
                    className={INPUT_CLASS}
                  >
                    {estados?.map((es) => (
                      <option key={es.codigo} value={es.codigo}>
                        {es.nombre}
                      </option>
                    ))}
                  </select>
                </Campo>
                <Campo label="Saldo">
                  <input
                    type="number"
                    step="0.01"
                    value={form.saldo}
                    onChange={(e) => actualizarCampo('saldo', Number(e.target.value))}
                    className={INPUT_CLASS}
                  />
                </Campo>
              </>
            )}

            {tab === 'condiciones' && (
              <>
                <Campo label="Tasa de interés (%)">
                  <input
                    type="number"
                    step="0.01"
                    value={form.tasaInteres}
                    onChange={(e) => actualizarCampo('tasaInteres', Number(e.target.value))}
                    className={INPUT_CLASS}
                  />
                </Campo>
                <Campo label="Intereses por pagar">
                  <input
                    type="number"
                    step="0.01"
                    value={form.interesesPorPagar}
                    onChange={(e) => actualizarCampo('interesesPorPagar', Number(e.target.value))}
                    className={INPUT_CLASS}
                  />
                </Campo>
                <label className="flex items-center gap-2 text-sm text-graphite-100">
                  <input
                    type="checkbox"
                    checked={form.pagaComision}
                    onChange={(e) => actualizarCampo('pagaComision', e.target.checked)}
                  />
                  Paga comisión
                </label>
                {form.pagaComision && (
                  <>
                    <Campo label="Tasa de la comisión (%)">
                      <input
                        type="number"
                        step="0.01"
                        value={form.tasaInteresComision ?? ''}
                        onChange={(e) => actualizarCampo('tasaInteresComision', Number(e.target.value))}
                        className={INPUT_CLASS}
                      />
                    </Campo>
                    <Campo label="Valor de la comisión">
                      <input
                        type="number"
                        step="0.01"
                        value={form.valorComision ?? ''}
                        onChange={(e) => actualizarCampo('valorComision', Number(e.target.value))}
                        className={INPUT_CLASS}
                      />
                    </Campo>
                  </>
                )}
                <label className="flex items-center gap-2 text-sm text-graphite-100">
                  <input
                    type="checkbox"
                    checked={form.tienePeriodoGracia}
                    onChange={(e) => actualizarCampo('tienePeriodoGracia', e.target.checked)}
                  />
                  Tiene período de gracia
                </label>
                {form.tienePeriodoGracia && (
                  <Campo label="Número de períodos de gracia">
                    <input
                      type="number"
                      value={form.numeroPeriodosGracia ?? ''}
                      onChange={(e) => actualizarCampo('numeroPeriodosGracia', Number(e.target.value))}
                      className={INPUT_CLASS}
                    />
                  </Campo>
                )}
              </>
            )}

            {tab === 'fechas' && (
              <>
                {modo === 'crear' && (
                  <>
                    <Campo label="Fecha de concesión">
                      <input
                        type="date"
                        value={form.fechaConcesion}
                        onChange={(e) => actualizarCampo('fechaConcesion', e.target.value)}
                        className={INPUT_CLASS}
                      />
                    </Campo>
                    <Campo label="Fecha de vencimiento">
                      <input
                        type="date"
                        value={form.fechaVencimiento}
                        onChange={(e) => actualizarCampo('fechaVencimiento', e.target.value)}
                        className={INPUT_CLASS}
                      />
                    </Campo>
                    <Campo label="Periodicidad de pago">
                      <select
                        value={form.codigoPeriodicidadPago}
                        onChange={(e) => actualizarCampo('codigoPeriodicidadPago', e.target.value)}
                        className={INPUT_CLASS}
                      >
                        {periodicidades?.map((p) => (
                          <option key={p.codigo} value={p.codigo}>
                            {p.nombre}
                          </option>
                        ))}
                      </select>
                    </Campo>
                    <Campo label="Clase de la obligación">
                      <select
                        value={form.codigoClase}
                        onChange={(e) => actualizarCampo('codigoClase', e.target.value)}
                        className={INPUT_CLASS}
                      >
                        {clases?.map((c) => (
                          <option key={c.codigo} value={c.codigo}>
                            {c.nombre}
                          </option>
                        ))}
                      </select>
                    </Campo>
                  </>
                )}
                {form.codigoEstado === 'VN' && (
                  <Campo label="Valor vencido">
                    <input
                      type="number"
                      step="0.01"
                      value={form.valorVencido ?? ''}
                      onChange={(e) => actualizarCampo('valorVencido', Number(e.target.value))}
                      className={INPUT_CLASS}
                    />
                  </Campo>
                )}
                {form.codigoEstado === 'CN' && (
                  <Campo label="Forma de cancelación">
                    <select
                      value={form.codigoFormaCancelacion ?? ''}
                      onChange={(e) => actualizarCampo('codigoFormaCancelacion', e.target.value || null)}
                      className={INPUT_CLASS}
                    >
                      <option value="">Seleccionar...</option>
                      {formasCancelacion?.map((f) => (
                        <option key={f.codigo} value={f.codigo}>
                          {f.nombre}
                        </option>
                      ))}
                    </select>
                  </Campo>
                )}
              </>
            )}
          </div>
        </div>

        <div className="flex justify-end gap-2 border-t border-black/[0.06] p-4">
          <button type="button" onClick={onClose} className="rounded-lg px-3 py-2 text-sm text-graphite-600">
            Cancelar
          </button>
          <button
            type="button"
            onClick={() => (modo === 'crear' ? crear.mutate() : actualizar.mutate())}
            disabled={crear.isPending || actualizar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          >
            {modo === 'crear' ? 'Registrar' : 'Guardar cambios'}
          </button>
        </div>
      </div>
    </div>
  )
}

function idPorCodigoCuenta(cuentas: Catalogo[] | undefined, codigo: string): string {
  // Las cuentas vienen del catálogo por código; el backend resuelve por id
  // real — este helper solo existe porque el <select> captura el código,
  // más legible para quien captura, y el id se recupera aparte.
  void cuentas
  return codigo
}

function Campo({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="mb-1 block text-xs font-medium text-graphite-600">{label}</label>
      {children}
    </div>
  )
}

function mensajeError(e: unknown): string {
  const err = e as { response?: { data?: { detail?: string } }; message?: string }
  return err.response?.data?.detail ?? err.message ?? 'Ocurrió un error inesperado'
}

interface SincronizacionResult {
  encontradasEnSoftbank: number
  sincronizadas: number
  omitidas: number
  detalle: string[]
}

function SeccionGenerarOf01() {
  const hoy = new Date().toISOString().slice(0, 10)
  const [fechaCorte, setFechaCorte] = useState(hoy)
  const [resultado, setResultado] = useState<Of01Result | null>(null)
  const [resultadoSync, setResultadoSync] = useState<SincronizacionResult | null>(null)
  const queryClient = useQueryClient()

  const generar = useMutation({
    mutationFn: async () => (await api.get(`${BASE}/generar?fechaCorte=${fechaCorte}`)).data as Of01Result,
    onSuccess: setResultado,
  })

  // Única pantalla de todo el sistema que puede traer datos reales de
  // Softbank -- protegido por los mismos 2 permisos del módulo completo
  // (Menu:estructuras-financieras + Estructura:OF01). El resto de la app
  // sigue trabajando 100% contra Postgres, sin excepción.
  const sincronizar = useMutation({
    mutationFn: async () => (await api.post(`${BASE}/sincronizar`)).data as SincronizacionResult,
    onSuccess: (data) => {
      setResultadoSync(data)
      queryClient.invalidateQueries({ queryKey: ['obligaciones-financieras'] })
    },
  })

  return (
    <div className="space-y-4">
      <div className="glass-card flex flex-wrap items-end gap-3 rounded-xl p-4">
        <Campo label="Fecha de corte">
          <input
            type="date"
            value={fechaCorte}
            onChange={(e) => setFechaCorte(e.target.value)}
            className={INPUT_CLASS}
          />
        </Campo>
        <button
          type="button"
          onClick={() => generar.mutate()}
          disabled={generar.isPending}
          className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        >
          <Download size={15} /> Generar OF01
        </button>
        <button
          type="button"
          onClick={() => sincronizar.mutate()}
          disabled={sincronizar.isPending}
          title="Trae/actualiza obligaciones financieras reales desde Softbank hacia esta base -- la única conexión de todo el sistema a Softbank vive acá"
          className="btn-hover flex items-center gap-1.5 rounded-lg border border-gold-500 px-4 py-2 text-sm font-medium text-gold-500 disabled:opacity-50"
        >
          <RefreshCw size={15} className={sincronizar.isPending ? 'animate-spin' : ''} /> Sincronizar desde Softbank
        </button>
      </div>

      {sincronizar.isError && (
        <div className="rounded-lg border border-red-300 bg-red-50 p-3 text-sm text-red-700">
          {mensajeError(sincronizar.error)}
        </div>
      )}

      {resultadoSync && (
        <div className="glass-card rounded-xl p-4 text-sm">
          <p className="font-medium text-graphite-100">
            {resultadoSync.encontradasEnSoftbank} obligaciones encontradas en Softbank —{' '}
            {resultadoSync.sincronizadas} sincronizadas, {resultadoSync.omitidas} omitidas.
          </p>
          {resultadoSync.detalle.length > 0 && (
            <ul className="mt-2 list-disc space-y-0.5 pl-5 text-xs text-graphite-600">
              {resultadoSync.detalle.map((d, i) => (
                <li key={i}>{d}</li>
              ))}
            </ul>
          )}
        </div>
      )}

      {resultado && (
        <>
          {resultado.advertencias.length > 0 && (
            <div className="glass-card rounded-xl border border-gold-500/30 bg-gold-500/5 p-4">
              <h4 className="mb-1 text-xs font-semibold text-gold-600">Advertencias</h4>
              <ul className="list-inside list-disc text-xs text-graphite-600">
                {resultado.advertencias.map((a, i) => (
                  <li key={i}>{a}</li>
                ))}
              </ul>
            </div>
          )}

          <div className="glass-card grid grid-cols-2 gap-4 rounded-xl p-4 sm:grid-cols-4">
            <div>
              <p className="text-xs text-graphite-600">Estructura</p>
              <p className="font-medium text-graphite-100">{resultado.cabecera.codigoEstructura}</p>
            </div>
            <div>
              <p className="text-xs text-graphite-600">RUC</p>
              <p className="font-medium text-graphite-100">{resultado.cabecera.ruc}</p>
            </div>
            <div>
              <p className="text-xs text-graphite-600">Fecha de corte</p>
              <p className="font-medium text-graphite-100">{resultado.cabecera.fechaCorte}</p>
            </div>
            <div>
              <p className="text-xs text-graphite-600">Registros</p>
              <p className="font-medium text-graphite-100">{resultado.cabecera.numeroTotalRegistros}</p>
            </div>
          </div>

          <div className="flex justify-end">
            <BotonesExportar
              nombreArchivo={`OF01_${resultado.cabecera.fechaCorte}`}
              titulo="OF01 — Obligaciones Financieras"
              subtitulo={`RUC ${resultado.cabecera.ruc} · Corte ${resultado.cabecera.fechaCorte}`}
              columnas={COLUMNAS_OF01}
              filas={resultado.detalle}
            />
          </div>

          <TableContainer>
            <thead>
              <tr>
                <Th>Número</Th>
                <Th>Acreedor</Th>
                <Th>Cuenta</Th>
                <Th>Saldo</Th>
                <Th>Estado</Th>
                <Th>Vencimiento</Th>
              </tr>
            </thead>
            <tbody>
              {resultado.detalle.length === 0 ? (
                <EmptyState>Sin registros a esta fecha de corte.</EmptyState>
              ) : (
                resultado.detalle.map((d, i) => (
                  <tr key={i} className="border-t border-black/[0.04]">
                    <Td>{String(d.numeroObligacion)}</Td>
                    <Td>
                      {String(d.tipoIdentificacionAcreedor)} {String(d.identificacionAcreedor)}
                    </Td>
                    <Td>{String(d.codigoCuentaContable)}</Td>
                    <Td className="text-right tabular-nums">${Number(d.saldo).toFixed(2)}</Td>
                    <Td>{String(d.codigoEstado)}</Td>
                    <Td>{String(d.fechaVencimiento)}</Td>
                  </tr>
                ))
              )}
            </tbody>
          </TableContainer>
        </>
      )}
    </div>
  )
}

const COLUMNAS_OF01 = [
  { header: 'Número obligación', accessor: (r: Record<string, unknown>) => String(r.numeroObligacion) },
  { header: 'Tipo identificación acreedor', accessor: (r: Record<string, unknown>) => String(r.tipoIdentificacionAcreedor) },
  { header: 'Identificación acreedor', accessor: (r: Record<string, unknown>) => String(r.identificacionAcreedor) },
  { header: 'País acreedor', accessor: (r: Record<string, unknown>) => String(r.codigoPaisAcreedor) },
  { header: 'Destino línea crédito', accessor: (r: Record<string, unknown>) => String(r.destinoLineaCredito) },
  { header: 'Monto línea crédito', accessor: (r: Record<string, unknown>) => Number(r.montoLineaCredito) },
  { header: 'Monto por utilizar', accessor: (r: Record<string, unknown>) => Number(r.montoPorUtilizar) },
  { header: 'Estado', accessor: (r: Record<string, unknown>) => String(r.codigoEstado) },
  { header: 'Saldo', accessor: (r: Record<string, unknown>) => Number(r.saldo) },
  { header: 'Cuenta contable', accessor: (r: Record<string, unknown>) => String(r.codigoCuentaContable) },
  { header: 'Tasa de interés', accessor: (r: Record<string, unknown>) => Number(r.tasaInteres) },
  { header: 'Intereses por pagar', accessor: (r: Record<string, unknown>) => Number(r.interesesPorPagar) },
  { header: 'Fecha concesión', accessor: (r: Record<string, unknown>) => String(r.fechaConcesion) },
  { header: 'Fecha vencimiento', accessor: (r: Record<string, unknown>) => String(r.fechaVencimiento) },
  { header: 'Periodicidad de pago', accessor: (r: Record<string, unknown>) => String(r.codigoPeriodicidadPago) },
  { header: 'Clase', accessor: (r: Record<string, unknown>) => String(r.codigoClase) },
]
