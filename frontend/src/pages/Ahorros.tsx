import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { PiggyBank, Plus, X, ArrowDownCircle, ArrowUpCircle, TrendingUp, Lock, Unlock, FileBarChart } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { SearchBar } from '../components/SearchBar'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { BotonesExportar } from '../components/BotonesExportar'
import { ModalPortal } from '../components/ModalPortal'
import type { ColumnaExportable } from '../lib/exportar'
import { api } from '../lib/api'

interface Producto {
  id: number
  codigo: string
  nombre: string
  permiteDebitoPrestamo: boolean
  activo: boolean
}

interface CuentaAhorro {
  id: string
  numero: string
  producto: string
  agencia: string
  estado: string
  fechaApertura: string
  saldoDisponible: number
  permiteDebitoPrestamo: boolean
  acreditaPrestamo: boolean
}

interface Socio {
  id: string
  numero: string
  nombre: string
}

interface TipoTransaccion {
  codigo: string
  nombre: string
  signoSaldoCuenta: number
}

interface AbrirCuentaPayload {
  idCliente: string
  idTipoCuenta: number
  idAgencia: number
  montoInicial: number
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function AbrirCuentaForm({ productos, onClose }: { productos: Producto[]; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [idCliente, setIdCliente] = useState('')
  const [idTipoCuenta, setIdTipoCuenta] = useState(productos[0]?.id ?? 0)
  const [montoInicial, setMontoInicial] = useState('0')

  const { data: socios } = useQuery<Socio[]>({
    queryKey: ['socios', ''],
    queryFn: async () => (await api.get('/api/socios')).data,
  })

  const abrir = useMutation({
    mutationFn: async (payload: AbrirCuentaPayload) => (await api.post('/api/ahorros/cuentas', payload)).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Abrir cuenta de ahorro</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!idCliente) return
          abrir.mutate({
            idCliente,
            idTipoCuenta,
            idAgencia: 1,
            montoInicial: Number(montoInicial) || 0,
          })
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Socio</span>
          <select
            required
            value={idCliente}
            onChange={(e) => setIdCliente(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            <option value="">Seleccionar…</option>
            {socios?.map((s) => (
              <option key={s.id} value={s.id}>
                {s.numero} — {s.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Producto</span>
          <select
            value={idTipoCuenta}
            onChange={(e) => setIdTipoCuenta(Number(e.target.value))}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          >
            {productos.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Depósito inicial (USD)</span>
          <input
            type="number"
            min="0"
            step="0.01"
            value={montoInicial}
            onChange={(e) => setMontoInicial(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={abrir.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {abrir.isPending ? 'Abriendo…' : 'Abrir cuenta'}
          </button>
        </div>

        {abrir.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            No se pudo abrir la cuenta. Revisá los datos e intentá de nuevo.
          </p>
        )}
      </form>
    </div>
  )
}

function MovimientoModal({ cuenta, onClose }: { cuenta: CuentaAhorro; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [codigoTipoTransaccion, setCodigoTipoTransaccion] = useState('DEP-EFEC')
  const [monto, setMonto] = useState('0')

  const { data: tipos } = useQuery<TipoTransaccion[]>({
    queryKey: ['ahorros-tipos-transaccion'],
    queryFn: async () => (await api.get('/api/ahorros/tipos-transaccion')).data,
  })

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          `/api/ahorros/cuentas/${cuenta.id}/movimientos`,
          { codigoTipoTransaccion, monto: Number(monto) || 0 },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data as { idAutorizacionPendiente: string | null },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
      queryClient.invalidateQueries({ queryKey: ['cajas-autorizaciones-pendientes'] })
      if (!data.idAutorizacionPendiente) onClose()
    },
  })

  const mensajeError = (registrar.error as { response?: { data?: { detail?: string } } } | undefined)?.response?.data
    ?.detail

  return (
    <div className="fixed inset-0 z-20 flex items-center justify-center bg-black/30 p-4">
      <div className="glass-strong animate-zoom-in w-full max-w-sm rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h3 className="font-medium text-graphite-100">Movimiento — cuenta {cuenta.numero}</h3>
            <p className="text-xs text-graphite-600">Saldo disponible: {formatoUsd(cuenta.saldoDisponible)}</p>
          </div>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        {registrar.isSuccess && registrar.data.idAutorizacionPendiente && (
          <div className="flex flex-col gap-3">
            <p className="rounded-lg bg-gold-500/10 px-3 py-2 text-sm text-gold-300">
              El titular está marcado PEP — la transacción queda en espera de autorización de un supervisor (Cajas →
              Autorizaciones pendientes) y todavía no se ejecutó, el saldo no cambió.
            </p>
            <button
              type="button"
              onClick={onClose}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white"
            >
              Cerrar
            </button>
          </div>
        )}

        {!(registrar.isSuccess && registrar.data.idAutorizacionPendiente) && (
        <form
          className="flex flex-col gap-4"
          onSubmit={(e) => {
            e.preventDefault()
            registrar.mutate()
          }}
        >
          <div className="flex gap-2">
            {tipos?.map((t) => (
              <button
                key={t.codigo}
                type="button"
                onClick={() => setCodigoTipoTransaccion(t.codigo)}
                className={`flex flex-1 items-center justify-center gap-1.5 rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
                  codigoTipoTransaccion === t.codigo
                    ? 'border-gold-500/50 bg-gold-500/10 text-gold-300'
                    : 'border-black/[0.08] text-graphite-600 hover:bg-black/[0.02]'
                }`}
              >
                {t.signoSaldoCuenta > 0 ? <ArrowDownCircle size={16} /> : <ArrowUpCircle size={16} />}
                {t.nombre}
              </button>
            ))}
          </div>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Monto (USD)</span>
            <input
              type="number"
              min="0.01"
              step="0.01"
              required
              value={monto}
              onChange={(e) => setMonto(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>

          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Procesando…' : 'Confirmar'}
          </button>

          {registrar.isError && (
            <p className="text-sm text-red-700">{mensajeError ?? 'No se pudo registrar el movimiento.'}</p>
          )}
        </form>
        )}
      </div>
    </div>
  )
}

export function Ahorros() {
  const [q, setQ] = useState('')
  const [mostrarForm, setMostrarForm] = useState(false)
  const [cuentaMovimiento, setCuentaMovimiento] = useState<CuentaAhorro | null>(null)

  const { data: productos } = useQuery<Producto[]>({
    queryKey: ['ahorros-productos'],
    queryFn: async () => (await api.get('/api/ahorros/productos')).data,
  })

  const { data: cuentas, isLoading } = useQuery<CuentaAhorro[]>({
    queryKey: ['ahorros-cuentas', q],
    queryFn: async () => (await api.get('/api/ahorros/cuentas', { params: { q: q || undefined } })).data,
  })

  const queryClient = useQueryClient()

  const toggleAcreditaPrestamo = useMutation({
    mutationFn: async ({ idCuenta, activar }: { idCuenta: string; activar: boolean }) =>
      api.patch(`/api/ahorros/cuentas/${idCuenta}/acredita-prestamo`, { activar }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
    },
  })

  const cambiarEstado = useMutation({
    mutationFn: async ({ idCuenta, estado }: { idCuenta: string; estado: string }) =>
      api.patch(`/api/ahorros/cuentas/${idCuenta}/estado`, { estado }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
    },
  })

  const devengo = useMutation({
    mutationFn: async () => (await api.post('/api/ahorros/devengo-interes/ejecutar')).data as {
      fecha: string
      cuentasProcesadas: number
      totalDevengado: number
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
    },
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={PiggyBank}
        title="Ahorros"
        subtitle="Cuentas de ahorro y captación a la vista"
        actions={
          !mostrarForm && (
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => devengo.mutate()}
                disabled={devengo.isPending}
                className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm font-medium text-graphite-100 hover:bg-black/[0.02] disabled:opacity-60"
              >
                <TrendingUp size={16} />
                {devengo.isPending ? 'Calculando…' : 'Ejecutar devengo de interés'}
              </button>
              <button
                type="button"
                onClick={() => setMostrarForm(true)}
                className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
              >
                <Plus size={16} /> Abrir cuenta
              </button>
            </div>
          )
        }
      />

      {devengo.isSuccess && (
        <p className="mb-4 rounded-lg bg-petrol-800/10 px-3 py-2 text-sm text-petrol-700">
          Devengo del {devengo.data.fecha}: {devengo.data.cuentasProcesadas} cuenta(s) procesadas, $
          {devengo.data.totalDevengado.toFixed(2)} devengados en total.
        </p>
      )}

      {mostrarForm && productos && <AbrirCuentaForm productos={productos} onClose={() => setMostrarForm(false)} />}
      {cuentaMovimiento && (
        <ModalPortal>
          <MovimientoModal cuenta={cuentaMovimiento} onClose={() => setCuentaMovimiento(null)} />
        </ModalPortal>
      )}

      <div className="mb-6 flex flex-wrap gap-2">
        {productos?.map((p) => (
          <div key={p.id} className="glass-card rounded-lg px-3 py-2 text-sm">
            <p className="font-medium text-graphite-100">{p.nombre}</p>
            <p className="text-xs text-graphite-600">
              {p.permiteDebitoPrestamo ? 'Admite débito de préstamo' : 'Sin débito de préstamo'}
            </p>
          </div>
        ))}
      </div>

      <div className="mb-4">
        <SearchBar value={q} onChange={setQ} placeholder="Buscar por número de cuenta…" />
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Número</Th>
            <Th>Producto</Th>
            <Th>Agencia</Th>
            <Th>Saldo</Th>
            <Th>Estado</Th>
            <Th>Débito de préstamo (SPI)</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (cuentas?.length ?? 0) === 0 && (
            <EmptyState>Todavía no hay cuentas de ahorro abiertas</EmptyState>
          )}
          {cuentas?.map((c) => (
            <tr key={c.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{c.numero}</Td>
              <Td>{c.producto}</Td>
              <Td>{c.agencia}</Td>
              <Td className="tabular-nums">{formatoUsd(c.saldoDisponible)}</Td>
              <Td>
                <Badge variant={c.estado === 'Activa' ? 'exito' : c.estado === 'Bloqueada' ? 'peligro' : 'neutral'}>
                  {c.estado}
                </Badge>
              </Td>
              <Td>
                {!c.permiteDebitoPrestamo ? (
                  <span className="text-xs text-graphite-600">No aplica a este producto</span>
                ) : (
                  <button
                    type="button"
                    disabled={c.estado !== 'Activa' || toggleAcreditaPrestamo.isPending}
                    onClick={() => toggleAcreditaPrestamo.mutate({ idCuenta: c.id, activar: !c.acreditaPrestamo })}
                    className="disabled:opacity-50"
                    title="Habilita que esta cuenta reciba el débito automático de la cuota del socio (una de las tres configuraciones cruzadas del auto-débito SPI)"
                  >
                    <Badge variant={c.acreditaPrestamo ? 'exito' : 'neutral'}>
                      {c.acreditaPrestamo ? 'Habilitada' : 'Deshabilitada'}
                    </Badge>
                  </button>
                )}
              </Td>
              <Td>
                <div className="flex items-center gap-3">
                  {c.estado === 'Activa' && (
                    <button
                      type="button"
                      onClick={() => setCuentaMovimiento(c)}
                      className="text-sm font-medium text-gold-400 hover:underline"
                    >
                      Movimiento
                    </button>
                  )}
                  {(c.estado === 'Activa' || c.estado === 'Bloqueada') && (
                    <button
                      type="button"
                      disabled={cambiarEstado.isPending}
                      onClick={() => cambiarEstado.mutate({ idCuenta: c.id, estado: c.estado === 'Activa' ? 'Bloqueada' : 'Activa' })}
                      className="flex items-center gap-1 text-xs font-medium text-graphite-600 hover:underline disabled:opacity-50"
                    >
                      {c.estado === 'Activa' ? (
                        <>
                          <Lock size={13} /> Bloquear
                        </>
                      ) : (
                        <>
                          <Unlock size={13} /> Desbloquear
                        </>
                      )}
                    </button>
                  )}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>

      <SeccionReportesAhorros />
    </div>
  )
}

interface CuentaAperturadaFila {
  numero: string
  producto: string
  agencia: string
  cliente: string
  fechaApertura: string
  saldoInicial: number
}

interface CuentaBloqueadaFila {
  numero: string
  producto: string
  agencia: string
  cliente: string
  fecha: string | null
  registradoPor: string | null
}

interface TransaccionAhorroFila {
  numeroCuenta: string
  tipo: string
  monto: number
  saldoResultante: number
  fecha: string
  registradoPor: string
}

const COLUMNAS_APERTURADAS: ColumnaExportable<CuentaAperturadaFila>[] = [
  { header: 'Número', accessor: (c) => c.numero },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Agencia', accessor: (c) => c.agencia },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Fecha apertura', accessor: (c) => c.fechaApertura },
  { header: 'Saldo inicial', accessor: (c) => c.saldoInicial },
]

const COLUMNAS_BLOQUEADAS: ColumnaExportable<CuentaBloqueadaFila>[] = [
  { header: 'Número', accessor: (c) => c.numero },
  { header: 'Producto', accessor: (c) => c.producto },
  { header: 'Agencia', accessor: (c) => c.agencia },
  { header: 'Socio', accessor: (c) => c.cliente },
  { header: 'Fecha', accessor: (c) => c.fecha ?? '' },
  { header: 'Registrado por', accessor: (c) => c.registradoPor ?? '' },
]

const COLUMNAS_TRANSACCIONES: ColumnaExportable<TransaccionAhorroFila>[] = [
  { header: 'Cuenta', accessor: (t) => t.numeroCuenta },
  { header: 'Tipo', accessor: (t) => t.tipo },
  { header: 'Monto', accessor: (t) => t.monto },
  { header: 'Saldo resultante', accessor: (t) => t.saldoResultante },
  { header: 'Fecha', accessor: (t) => t.fecha },
  { header: 'Registrado por', accessor: (t) => t.registradoPor },
]

function SeccionReportesAhorros() {
  const [reporte, setReporte] = useState<'aperturadas' | 'bloqueadas' | 'transacciones'>('aperturadas')
  const [desde, setDesde] = useState(() => new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10))
  const [hasta, setHasta] = useState(() => new Date().toISOString().slice(0, 10))

  const { data: aperturadas } = useQuery<CuentaAperturadaFila[]>({
    queryKey: ['ahorros-reporte-aperturadas', desde, hasta],
    queryFn: async () => (await api.get('/api/ahorros/reportes/cuentas-aperturadas', { params: { desde, hasta } })).data,
    enabled: reporte === 'aperturadas',
  })

  const { data: bloqueadas } = useQuery<CuentaBloqueadaFila[]>({
    queryKey: ['ahorros-reporte-bloqueadas'],
    queryFn: async () => (await api.get('/api/ahorros/reportes/cuentas-bloqueadas')).data,
    enabled: reporte === 'bloqueadas',
  })

  const { data: transacciones } = useQuery<TransaccionAhorroFila[]>({
    queryKey: ['ahorros-reporte-transacciones', desde, hasta],
    queryFn: async () => (await api.get('/api/ahorros/reportes/transacciones', { params: { desde, hasta } })).data,
    enabled: reporte === 'transacciones',
  })

  return (
    <div className="mt-8">
      <div className="mb-2 flex items-center justify-between">
        <h2 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-graphite-600">
          <FileBarChart size={15} /> Reportes
        </h2>
        {reporte === 'aperturadas' && (
          <BotonesExportar
            nombreArchivo="ahorros_cuentas_aperturadas"
            titulo="Cuentas aperturadas"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_APERTURADAS}
            filas={aperturadas ?? []}
          />
        )}
        {reporte === 'bloqueadas' && (
          <BotonesExportar
            nombreArchivo="ahorros_cuentas_bloqueadas"
            titulo="Cuentas bloqueadas"
            columnas={COLUMNAS_BLOQUEADAS}
            filas={bloqueadas ?? []}
          />
        )}
        {reporte === 'transacciones' && (
          <BotonesExportar
            nombreArchivo="ahorros_transacciones"
            titulo="Transacciones de ahorros"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_TRANSACCIONES}
            filas={transacciones ?? []}
          />
        )}
      </div>
      <div className="mb-4 flex flex-wrap items-end gap-3">
        <div className="flex gap-1 rounded-lg border border-black/[0.08] p-1">
          {(
            [
              ['aperturadas', 'Cuentas aperturadas'],
              ['bloqueadas', 'Cuentas bloqueadas'],
              ['transacciones', 'Transacciones'],
            ] as const
          ).map(([id, label]) => (
            <button
              key={id}
              type="button"
              onClick={() => setReporte(id)}
              className={`rounded-md px-3 py-1.5 text-xs font-medium transition-colors ${
                reporte === id ? 'bg-gold-500 text-white' : 'text-graphite-600 hover:bg-black/[0.02]'
              }`}
            >
              {label}
            </button>
          ))}
        </div>
        {reporte !== 'bloqueadas' && (
          <>
            <label className="flex flex-col gap-1 text-xs">
              <span className="text-graphite-600">Desde</span>
              <input
                type="date"
                value={desde}
                onChange={(e) => setDesde(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex flex-col gap-1 text-xs">
              <span className="text-graphite-600">Hasta</span>
              <input
                type="date"
                value={hasta}
                onChange={(e) => setHasta(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-2 py-1.5 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          </>
        )}
      </div>

      {reporte === 'aperturadas' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Número</Th>
              <Th>Producto</Th>
              <Th>Agencia</Th>
              <Th>Socio</Th>
              <Th>Fecha apertura</Th>
              <Th>Saldo inicial</Th>
            </tr>
          </thead>
          <tbody>
            {(aperturadas?.length ?? 0) === 0 && <EmptyState>Sin cuentas aperturadas en el rango</EmptyState>}
            {aperturadas?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numero}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.agencia}</Td>
                <Td>{c.cliente}</Td>
                <Td>{c.fechaApertura}</Td>
                <Td className="tabular-nums">{formatoUsd(c.saldoInicial)}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'bloqueadas' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Número</Th>
              <Th>Producto</Th>
              <Th>Agencia</Th>
              <Th>Socio</Th>
              <Th>Fecha</Th>
              <Th>Bloqueado por</Th>
            </tr>
          </thead>
          <tbody>
            {(bloqueadas?.length ?? 0) === 0 && <EmptyState>Sin cuentas bloqueadas actualmente</EmptyState>}
            {bloqueadas?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.numero}</Td>
                <Td>{c.producto}</Td>
                <Td>{c.agencia}</Td>
                <Td>{c.cliente}</Td>
                <Td>{c.fecha ? new Date(c.fecha).toLocaleString('es-EC') : '—'}</Td>
                <Td>{c.registradoPor ?? '—'}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'transacciones' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Cuenta</Th>
              <Th>Tipo</Th>
              <Th>Monto</Th>
              <Th>Saldo resultante</Th>
              <Th>Fecha</Th>
              <Th>Registrado por</Th>
            </tr>
          </thead>
          <tbody>
            {(transacciones?.length ?? 0) === 0 && <EmptyState>Sin transacciones en el rango</EmptyState>}
            {transacciones?.map((t, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{t.numeroCuenta}</Td>
                <Td>{t.tipo}</Td>
                <Td className="tabular-nums">{formatoUsd(t.monto)}</Td>
                <Td className="tabular-nums">{formatoUsd(t.saldoResultante)}</Td>
                <Td>{new Date(t.fecha).toLocaleString('es-EC')}</Td>
                <Td>{t.registradoPor}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}
    </div>
  )
}
