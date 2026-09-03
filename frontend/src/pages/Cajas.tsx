import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Wallet, Plus, X, Lock, ShieldAlert, ListChecks, Landmark, Pencil, Receipt, Undo2, Hash, FileBarChart } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { BotonesExportar } from '../components/BotonesExportar'
import type { ColumnaExportable } from '../lib/exportar'
import { api } from '../lib/api'

interface VentanillaListItem {
  id: string
  usuario: string
  agencia: string
  fecha: string
  cerrada: boolean
  cuadrada: boolean
  saldoEfectivo: number
}

interface VentanillaMovimientoItem {
  fecha: string
  valor: number
  saldoResultante: number
  descripcion: string
  registradoPor: string
  idComprobanteContable: string | null
}

interface VentanillaCerradaResult {
  idCuadre: string
  estaCuadrado: boolean
  saldoEsperado: number
  diferenciaEfectivo: number
}

interface Agencia {
  id: number
  nombre: string
}

interface UsuarioParaCaja {
  id: string
  nombreUsuario: string
}

interface BovedaSaldoItem {
  codigoItem: string
  nombreItem: string
  codigoMoneda: string
  saldo: number
}

interface BovedaListItem {
  id: string
  idAgencia: number
  agencia: string
  idUsuarioResponsable: string
  usuarioResponsable: string
  existenciaMinima: number
  existenciaMaxima: number
  existenciaMinimaCaja: number
  existenciaMaximaCaja: number
  existenciaMinimaCajaChica: number
  existenciaMaximaCajaChica: number
  activa: boolean
  saldos: BovedaSaldoItem[]
}

interface ItemBovedaListItem {
  id: number
  codigo: string
  nombre: string
}

interface AutorizacionPendiente {
  id: string
  numeroCuenta: string
  socio: string
  codigoTipoTransaccion: string
  monto: number
  detalle: string
  creadoEn: string
  registradoPor: string
}

function formatoUsd(monto: number) {
  return monto.toLocaleString('es-EC', { style: 'currency', currency: 'USD' })
}

function SeccionAutorizaciones() {
  const queryClient = useQueryClient()
  const [rechazando, setRechazando] = useState<string | null>(null)
  const [comentario, setComentario] = useState('')

  const { data: pendientes, isLoading } = useQuery<AutorizacionPendiente[]>({
    queryKey: ['cajas-autorizaciones-pendientes'],
    queryFn: async () => (await api.get('/api/cajas/autorizaciones-transaccion')).data,
  })

  const aprobar = useMutation({
    mutationFn: async (id: string) => (await api.post(`/api/cajas/autorizaciones-transaccion/${id}/aprobar`)).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-autorizaciones-pendientes'] })
      queryClient.invalidateQueries({ queryKey: ['ahorros-cuentas'] })
    },
  })

  const rechazar = useMutation({
    mutationFn: async (id: string) =>
      api.post(`/api/cajas/autorizaciones-transaccion/${id}/rechazar`, { comentario }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-autorizaciones-pendientes'] })
      setRechazando(null)
      setComentario('')
    },
  })

  if (!isLoading && (pendientes?.length ?? 0) === 0) return null

  return (
    <div className="mb-8">
      <h2 className="mb-2 flex items-center gap-1.5 text-sm font-semibold uppercase tracking-wide text-gold-400">
        <ShieldAlert size={15} /> Autorizaciones de transacción pendientes (titular PEP)
      </h2>
      <p className="mb-2 text-xs text-graphite-600">
        Estas transacciones no se ejecutaron todavía — el saldo de la cuenta no cambia hasta aprobarlas.
      </p>
      <TableContainer>
        <thead>
          <tr>
            <Th>Cuenta</Th>
            <Th>Socio</Th>
            <Th>Transacción</Th>
            <Th>Monto</Th>
            <Th>Registrado por</Th>
            <Th>Acciones</Th>
          </tr>
        </thead>
        <tbody>
          {pendientes?.map((a) => (
            <tr key={a.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{a.numeroCuenta}</Td>
              <Td>{a.socio}</Td>
              <Td>{a.codigoTipoTransaccion}</Td>
              <Td className="tabular-nums">{formatoUsd(a.monto)}</Td>
              <Td>{a.registradoPor}</Td>
              <Td>
                {rechazando === a.id ? (
                  <div className="flex items-center gap-2">
                    <input
                      autoFocus
                      value={comentario}
                      onChange={(e) => setComentario(e.target.value)}
                      placeholder="Motivo del rechazo"
                      className="rounded border border-black/[0.08] bg-white px-2 py-1 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
                    />
                    <button
                      type="button"
                      disabled={!comentario || rechazar.isPending}
                      onClick={() => rechazar.mutate(a.id)}
                      className="text-xs font-medium text-red-700 hover:underline disabled:opacity-50"
                    >
                      Confirmar
                    </button>
                    <button
                      type="button"
                      onClick={() => setRechazando(null)}
                      className="text-xs text-graphite-600 hover:underline"
                    >
                      Cancelar
                    </button>
                  </div>
                ) : (
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      disabled={aprobar.isPending}
                      onClick={() => aprobar.mutate(a.id)}
                      className="text-xs font-medium text-gold-400 hover:underline disabled:opacity-50"
                    >
                      Aprobar
                    </button>
                    <button
                      type="button"
                      onClick={() => setRechazando(a.id)}
                      className="text-xs font-medium text-graphite-600 hover:underline"
                    >
                      Rechazar
                    </button>
                  </div>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

function AbrirVentanillaForm({ onClose }: { onClose: () => void }) {
  const queryClient = useQueryClient()

  const abrir = useMutation({
    mutationFn: async () => (await api.post('/api/cajas/ventanillas', { idAgencia: 1 })).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-ventanillas'] })
      onClose()
    },
  })

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Abrir mi ventanilla del día</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => abrir.mutate()}
          disabled={abrir.isPending}
          className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {abrir.isPending ? 'Abriendo…' : 'Abrir ventanilla'}
        </button>
      </div>

      {abrir.isError && (
        <p className="mt-3 text-sm text-red-700">
          {(abrir.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
            'No se pudo abrir la ventanilla.'}
        </p>
      )}
    </div>
  )
}

function CerrarVentanillaForm({ ventanilla, onClose }: { ventanilla: VentanillaListItem; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [totalEfectivoContado, setTotalEfectivoContado] = useState('')
  const [totalCheque, setTotalCheque] = useState('0')
  const [resultado, setResultado] = useState<VentanillaCerradaResult | null>(null)

  const cerrar = useMutation({
    mutationFn: async () =>
      (
        await api.post(`/api/cajas/ventanillas/${ventanilla.id}/cerrar`, {
          totalEfectivoContado: Number(totalEfectivoContado),
          totalCheque: Number(totalCheque),
        })
      ).data as VentanillaCerradaResult,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['cajas-ventanillas'] })
      setResultado(data)
    },
  })

  if (resultado) {
    return (
      <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">
            Ventanilla cerrada — {ventanilla.usuario} ({ventanilla.agencia})
          </h3>
          <Badge variant={resultado.estaCuadrado ? 'exito' : 'peligro'}>
            {resultado.estaCuadrado ? 'Cuadrada' : 'Descuadrada'}
          </Badge>
        </div>
        <div className="grid grid-cols-1 gap-3 text-sm sm:grid-cols-3">
          <div>
            <span className="block text-xs text-graphite-600">Saldo esperado (real)</span>
            <span className="font-medium tabular-nums text-graphite-100">{formatoUsd(resultado.saldoEsperado)}</span>
          </div>
          <div>
            <span className="block text-xs text-graphite-600">Efectivo contado</span>
            <span className="font-medium tabular-nums text-graphite-100">{formatoUsd(Number(totalEfectivoContado))}</span>
          </div>
          <div>
            <span className="block text-xs text-graphite-600">Diferencia</span>
            <span className={`font-medium tabular-nums ${resultado.diferenciaEfectivo === 0 ? 'text-petrol-700' : 'text-red-700'}`}>
              {formatoUsd(resultado.diferenciaEfectivo)}
            </span>
          </div>
        </div>
        <button
          type="button"
          onClick={onClose}
          className="btn-hover mt-4 rounded-lg bg-graphite-100 px-4 py-2 text-sm font-medium text-white"
        >
          Cerrar
        </button>
      </div>
    )
  }

  return (
    <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
      <div className="mb-4 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">
          Cerrar ventanilla — {ventanilla.usuario} ({ventanilla.agencia})
        </h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>

      <p className="mb-3 text-xs text-graphite-600">
        Saldo esperado en efectivo según los movimientos reales de hoy: <span className="font-medium">{formatoUsd(ventanilla.saldoEfectivo)}</span>
      </p>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!totalEfectivoContado) return
          cerrar.mutate()
        }}
      >
        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Efectivo contado</span>
          <input
            required
            type="number"
            step="0.01"
            min="0"
            value={totalEfectivoContado}
            onChange={(e) => setTotalEfectivoContado(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Cheques</span>
          <input
            type="number"
            step="0.01"
            min="0"
            value={totalCheque}
            onChange={(e) => setTotalCheque(e.target.value)}
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
          />
        </label>

        <div className="flex items-end gap-2">
          <button
            type="submit"
            disabled={cerrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {cerrar.isPending ? 'Cerrando…' : 'Cerrar y cuadrar'}
          </button>
        </div>

        {cerrar.isError && (
          <p className="sm:col-span-2 text-sm text-red-700">
            {(cerrar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
              'No se pudo cerrar la ventanilla.'}
          </p>
        )}
      </form>
    </div>
  )
}

function MovimientosVentanilla({ idVentanilla }: { idVentanilla: string }) {
  const { data, isLoading } = useQuery<VentanillaMovimientoItem[]>({
    queryKey: ['cajas-ventanilla-movimientos', idVentanilla],
    queryFn: async () => (await api.get(`/api/cajas/ventanillas/${idVentanilla}/movimientos`)).data,
  })

  if (isLoading) return <p className="px-4 py-3 text-xs text-graphite-600">Cargando movimientos…</p>
  if ((data?.length ?? 0) === 0) return <p className="px-4 py-3 text-xs text-graphite-600">Sin movimientos de efectivo hoy.</p>

  return (
    <div className="px-4 py-3">
      <table className="w-full text-xs">
        <thead>
          <tr className="text-left text-graphite-600">
            <th className="pb-1 font-medium">Fecha</th>
            <th className="pb-1 font-medium">Descripción</th>
            <th className="pb-1 text-right font-medium">Valor</th>
            <th className="pb-1 text-right font-medium">Saldo resultante</th>
            <th className="pb-1 font-medium">Registrado por</th>
          </tr>
        </thead>
        <tbody>
          {data?.map((m, i) => (
            <tr key={i} className="border-t border-black/[0.04]">
              <td className="py-1 text-graphite-600">{new Date(m.fecha).toLocaleString('es-EC')}</td>
              <td className="py-1">{m.descripcion}</td>
              <td className={`py-1 text-right tabular-nums ${m.valor >= 0 ? 'text-petrol-700' : 'text-red-700'}`}>
                {formatoUsd(m.valor)}
              </td>
              <td className="py-1 text-right tabular-nums">{formatoUsd(m.saldoResultante)}</td>
              <td className="py-1">{m.registradoPor}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function SeccionVentanillas() {
  const [mostrarForm, setMostrarForm] = useState(false)
  const [ventanillaACerrar, setVentanillaACerrar] = useState<VentanillaListItem | null>(null)
  const [idExpandido, setIdExpandido] = useState<string | null>(null)

  const { data: ventanillas, isLoading } = useQuery<VentanillaListItem[]>({
    queryKey: ['cajas-ventanillas'],
    queryFn: async () => (await api.get('/api/cajas/ventanillas')).data,
  })

  return (
    <div>
      <div className="mb-4 flex items-center justify-end">
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
          >
            <Plus size={16} /> Abrir ventanilla
          </button>
        )}
      </div>

      {mostrarForm && <AbrirVentanillaForm onClose={() => setMostrarForm(false)} />}

      {ventanillaACerrar && (
        <CerrarVentanillaForm ventanilla={ventanillaACerrar} onClose={() => setVentanillaACerrar(null)} />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Cajero</Th>
            <Th>Agencia</Th>
            <Th>Fecha</Th>
            <Th>Estado</Th>
            <Th>Saldo efectivo</Th>
            <Th>Cuadre</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (ventanillas?.length ?? 0) === 0 && <EmptyState>Todavía no hay ventanillas registradas</EmptyState>}
          {ventanillas?.map((v) => (
            <>
              <tr key={v.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
                <Td className="font-medium">{v.usuario}</Td>
                <Td>{v.agencia}</Td>
                <Td>{v.fecha}</Td>
                <Td>
                  <Badge variant={v.cerrada ? 'neutral' : 'exito'}>{v.cerrada ? 'Cerrada' : 'Abierta'}</Badge>
                </Td>
                <Td className="tabular-nums">{formatoUsd(v.saldoEfectivo)}</Td>
                <Td>
                  {v.cerrada ? <Badge variant={v.cuadrada ? 'exito' : 'peligro'}>{v.cuadrada ? 'Cuadrada' : 'Descuadrada'}</Badge> : '—'}
                </Td>
                <Td>
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => setIdExpandido(idExpandido === v.id ? null : v.id)}
                      className="flex items-center gap-1 text-xs font-medium text-graphite-600 hover:underline"
                    >
                      <ListChecks size={13} /> Movimientos
                    </button>
                    {!v.cerrada && (
                      <button
                        type="button"
                        onClick={() => setVentanillaACerrar(v)}
                        className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                      >
                        <Lock size={13} /> Cerrar
                      </button>
                    )}
                  </div>
                </Td>
              </tr>
              {idExpandido === v.id && (
                <tr className="border-b border-black/[0.04] bg-black/[0.015]">
                  <td colSpan={7}>
                    <MovimientosVentanilla idVentanilla={v.id} />
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

const TABS_CAJAS = [
  { id: 'ventanillas', label: 'Ventanillas' },
  { id: 'boveda', label: 'Bóveda' },
  { id: 'pago-externo', label: 'Pago externo' },
  { id: 'formas-numeradas', label: 'Formas numeradas' },
  { id: 'reportes', label: 'Reportes' },
] as const
type TabCajasId = (typeof TABS_CAJAS)[number]['id']

export function Cajas() {
  const [tab, setTab] = useState<TabCajasId>('ventanillas')

  return (
    <div className="animate-fade-in">
      <PageHeader icon={Wallet} title="Cajas" subtitle="Apertura, transacciones y cuadre diario de ventanillas" />

      <SeccionAutorizaciones />

      <div className="mb-6 flex flex-wrap gap-1 border-b border-black/[0.06]">
        {TABS_CAJAS.map((t) => (
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

      {tab === 'ventanillas' && <SeccionVentanillas />}
      {tab === 'boveda' && <SeccionBoveda />}
      {tab === 'pago-externo' && <SeccionPagoExterno />}
      {tab === 'formas-numeradas' && <SeccionFormasNumeradas />}
      {tab === 'reportes' && <SeccionReportesCajas />}
    </div>
  )
}

function SeccionBoveda() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [editando, setEditando] = useState<BovedaListItem | null>(null)
  const [ajustando, setAjustando] = useState<{ boveda: BovedaListItem; item: ItemBovedaListItem } | null>(null)

  const { data: bovedas, isLoading } = useQuery<BovedaListItem[]>({
    queryKey: ['cajas-bovedas'],
    queryFn: async () => (await api.get('/api/cajas/bovedas')).data,
  })

  const { data: agencias } = useQuery<Agencia[]>({
    queryKey: ['config-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const { data: usuarios } = useQuery<UsuarioParaCaja[]>({
    queryKey: ['cajas-usuarios'],
    queryFn: async () => (await api.get('/api/cajas/usuarios')).data,
  })

  const { data: itemsBoveda } = useQuery<ItemBovedaListItem[]>({
    queryKey: ['cajas-items-boveda'],
    queryFn: async () => (await api.get('/api/cajas/items-boveda')).data,
  })

  return (
    <div className="mt-8">
      <h2 className="mb-2 flex items-center justify-between text-sm font-semibold uppercase tracking-wide text-gold-400">
        <span className="flex items-center gap-1.5">
          <Landmark size={15} /> Bóveda por agencia
        </span>
        {!mostrarForm && !editando && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-1.5 text-xs font-medium text-white"
          >
            <Plus size={14} /> Nueva bóveda
          </button>
        )}
      </h2>
      <p className="mb-3 text-xs text-graphite-600">
        Configuración persistente de límites de existencia por agencia — no es una sesión diaria como la ventanilla.
        El saldo por ítem se ajusta manualmente hasta que exista un caso de uso real de transferencia caja↔bóveda.
      </p>

      {(mostrarForm || editando) && (
        <BovedaForm
          agencias={agencias ?? []}
          usuarios={usuarios ?? []}
          editando={editando}
          onClose={() => {
            setMostrarForm(false)
            setEditando(null)
          }}
        />
      )}

      {ajustando && (
        <AjustarSaldoBovedaForm
          boveda={ajustando.boveda}
          item={ajustando.item}
          onClose={() => setAjustando(null)}
        />
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Agencia</Th>
            <Th>Responsable</Th>
            <Th>Existencia máx. bóveda</Th>
            <Th>Existencia máx. caja</Th>
            <Th>Estado</Th>
            <Th>Saldos</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (bovedas?.length ?? 0) === 0 && <EmptyState>Todavía no hay bóvedas configuradas</EmptyState>}
          {bovedas?.map((b) => (
            <tr key={b.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{b.agencia}</Td>
              <Td>{b.usuarioResponsable}</Td>
              <Td className="tabular-nums">{formatoUsd(b.existenciaMaxima)}</Td>
              <Td className="tabular-nums">{formatoUsd(b.existenciaMaximaCaja)}</Td>
              <Td>
                <Badge variant={b.activa ? 'exito' : 'neutral'}>{b.activa ? 'Activa' : 'Inactiva'}</Badge>
              </Td>
              <Td>
                <div className="flex flex-col gap-0.5">
                  {b.saldos.length === 0 && <span className="text-graphite-600">—</span>}
                  {b.saldos.map((s, i) => (
                    <span key={i} className="tabular-nums">
                      {s.nombreItem}: {formatoUsd(s.saldo)}
                    </span>
                  ))}
                </div>
              </Td>
              <Td>
                <div className="flex flex-col gap-1">
                  <button
                    type="button"
                    onClick={() => setEditando(b)}
                    className="flex items-center gap-1 text-xs font-medium text-petrol-700 hover:underline"
                  >
                    <Pencil size={12} /> Editar
                  </button>
                  {itemsBoveda?.map((item) => (
                    <button
                      key={item.id}
                      type="button"
                      onClick={() => setAjustando({ boveda: b, item })}
                      className="text-left text-xs font-medium text-graphite-600 hover:underline"
                    >
                      Ajustar {item.nombre}
                    </button>
                  ))}
                </div>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )

  function BovedaForm({
    agencias,
    usuarios,
    editando,
    onClose,
  }: {
    agencias: Agencia[]
    usuarios: UsuarioParaCaja[]
    editando: BovedaListItem | null
    onClose: () => void
  }) {
    const [idAgencia, setIdAgencia] = useState(editando ? String(editando.idAgencia) : '')
    const [idUsuarioResponsable, setIdUsuarioResponsable] = useState(editando?.idUsuarioResponsable ?? '')
    const [existenciaMinima, setExistenciaMinima] = useState(String(editando?.existenciaMinima ?? '0'))
    const [existenciaMaxima, setExistenciaMaxima] = useState(String(editando?.existenciaMaxima ?? ''))
    const [existenciaMinimaCaja, setExistenciaMinimaCaja] = useState(String(editando?.existenciaMinimaCaja ?? '0'))
    const [existenciaMaximaCaja, setExistenciaMaximaCaja] = useState(String(editando?.existenciaMaximaCaja ?? ''))
    const [existenciaMinimaCajaChica, setExistenciaMinimaCajaChica] = useState(String(editando?.existenciaMinimaCajaChica ?? '0'))
    const [existenciaMaximaCajaChica, setExistenciaMaximaCajaChica] = useState(String(editando?.existenciaMaximaCajaChica ?? '500'))
    const [activa, setActiva] = useState(editando?.activa ?? true)

    const guardar = useMutation({
      mutationFn: async () => {
        const body = {
          idUsuarioResponsable,
          existenciaMinima: Number(existenciaMinima),
          existenciaMaxima: Number(existenciaMaxima),
          existenciaMinimaCaja: Number(existenciaMinimaCaja),
          existenciaMaximaCaja: Number(existenciaMaximaCaja),
          existenciaMinimaCajaChica: Number(existenciaMinimaCajaChica),
          existenciaMaximaCajaChica: Number(existenciaMaximaCajaChica),
        }
        if (editando) {
          return (await api.put(`/api/cajas/bovedas/${editando.id}`, { ...body, activa })).data
        }
        return (await api.post('/api/cajas/bovedas', { idAgencia: Number(idAgencia), ...body })).data
      },
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['cajas-bovedas'] })
        onClose()
      },
    })

    return (
      <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">{editando ? `Editar bóveda — ${editando.agencia}` : 'Nueva bóveda'}</h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>

        <form
          className="grid grid-cols-1 gap-4 sm:grid-cols-3"
          onSubmit={(e) => {
            e.preventDefault()
            guardar.mutate()
          }}
        >
          {!editando && (
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
          )}

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Responsable</span>
            <select
              required
              value={idUsuarioResponsable}
              onChange={(e) => setIdUsuarioResponsable(e.target.value)}
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

          {editando && (
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={activa} onChange={(e) => setActiva(e.target.checked)} />
              <span className="text-graphite-600">Activa</span>
            </label>
          )}

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Existencia mín. bóveda</span>
            <input type="number" step="0.01" min="0" value={existenciaMinima} onChange={(e) => setExistenciaMinima(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Existencia máx. bóveda</span>
            <input required type="number" step="0.01" min="0" value={existenciaMaxima} onChange={(e) => setExistenciaMaxima(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Existencia mín. caja</span>
            <input type="number" step="0.01" min="0" value={existenciaMinimaCaja} onChange={(e) => setExistenciaMinimaCaja(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Existencia máx. caja</span>
            <input required type="number" step="0.01" min="0" value={existenciaMaximaCaja} onChange={(e) => setExistenciaMaximaCaja(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Existencia mín. caja chica</span>
            <input type="number" step="0.01" min="0" value={existenciaMinimaCajaChica} onChange={(e) => setExistenciaMinimaCajaChica(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Existencia máx. caja chica</span>
            <input required type="number" step="0.01" min="0" value={existenciaMaximaCajaChica} onChange={(e) => setExistenciaMaximaCajaChica(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50" />
          </label>

          <div className="flex items-end">
            <button
              type="submit"
              disabled={guardar.isPending}
              className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
            >
              {guardar.isPending ? 'Guardando…' : 'Guardar'}
            </button>
          </div>

          {guardar.isError && (
            <p className="sm:col-span-3 text-sm text-red-700">
              {(guardar.error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ??
                'No se pudo guardar la bóveda.'}
            </p>
          )}
        </form>
      </div>
    )
  }

  function AjustarSaldoBovedaForm({
    boveda,
    item,
    onClose,
  }: {
    boveda: BovedaListItem
    item: ItemBovedaListItem
    onClose: () => void
  }) {
    const saldoActual = boveda.saldos.find((s) => s.codigoItem === item.codigo)?.saldo ?? 0
    const [nuevoSaldo, setNuevoSaldo] = useState(String(saldoActual))

    const ajustar = useMutation({
      mutationFn: async () =>
        api.post(`/api/cajas/bovedas/${boveda.id}/saldos/ajustar`, {
          idItemBoveda: item.id,
          idMoneda: 1,
          nuevoSaldo: Number(nuevoSaldo),
        }),
      onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['cajas-bovedas'] })
        onClose()
      },
    })

    return (
      <div className="glass-card animate-zoom-in mb-6 rounded-xl p-5">
        <div className="mb-4 flex items-center justify-between">
          <h3 className="font-medium text-graphite-100">
            Ajustar saldo de {item.nombre} — {boveda.agencia}
          </h3>
          <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
            <X size={18} />
          </button>
        </div>
        <form
          className="flex items-end gap-3"
          onSubmit={(e) => {
            e.preventDefault()
            ajustar.mutate()
          }}
        >
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">Nuevo saldo declarado</span>
            <input
              required
              type="number"
              step="0.01"
              min="0"
              value={nuevoSaldo}
              onChange={(e) => setNuevoSaldo(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <button
            type="submit"
            disabled={ajustar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {ajustar.isPending ? 'Guardando…' : 'Guardar'}
          </button>
        </form>
        {ajustar.isError && <p className="mt-2 text-sm text-red-700">No se pudo ajustar el saldo.</p>}
      </div>
    )
  }
}

interface PagoExternoProducto {
  id: number
  nombre: string
  tituloReferencia: string
  requiereDatosFactura: boolean
  tieneComision: boolean
}

interface PagoExternoTransaccionItem {
  id: string
  producto: string
  referencia: string
  documento: string | null
  valor: number
  comision: number
  fechaProceso: string
  reversada: boolean
  registradoPor: string
}

function mensajeErrorCajas(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
}

function SeccionPagoExterno() {
  const queryClient = useQueryClient()
  const [idProducto, setIdProducto] = useState('')
  const [referencia, setReferencia] = useState('')
  const [valor, setValor] = useState('')
  const [comision, setComision] = useState('')

  const { data: productos } = useQuery<PagoExternoProducto[]>({
    queryKey: ['cajas-pago-externo-productos'],
    queryFn: async () => (await api.get('/api/cajas/pago-externo/productos')).data,
  })

  const { data: transacciones } = useQuery<PagoExternoTransaccionItem[]>({
    queryKey: ['cajas-pago-externo-transacciones'],
    queryFn: async () => (await api.get('/api/cajas/pago-externo/transacciones')).data,
  })

  const productoSeleccionado = productos?.find((p) => p.id === Number(idProducto))

  const registrar = useMutation({
    mutationFn: async () =>
      (
        await api.post(
          '/api/cajas/pago-externo/transacciones',
          {
            idProducto: Number(idProducto),
            referencia,
            documento: null,
            valor: Number(valor),
            comision: comision ? Number(comision) : 0,
          },
          { headers: { 'Idempotency-Key': crypto.randomUUID() } },
        )
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-pago-externo-transacciones'] })
      setReferencia('')
      setValor('')
      setComision('')
    },
  })

  const reversar = useMutation({
    mutationFn: async (id: string) =>
      api.post(
        `/api/cajas/pago-externo/transacciones/${id}/reversar`,
        undefined,
        { headers: { 'Idempotency-Key': crypto.randomUUID() } },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-pago-externo-transacciones'] })
    },
  })

  return (
    <div className="mt-8">
      <h2 className="mb-2 flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-graphite-600">
        <Receipt size={15} /> Pago externo — recaudación de servicios de terceros
      </h2>

      <div className="glass-card mb-4 rounded-xl p-4">
        <p className="mb-3 text-sm text-graphite-600">
          Registro real del cobro en efectivo de un servicio de un tercero (agua, luz, IESS, telefonía…). El valor
          total entra a Caja; el neto a remitir al proveedor queda en Recaudaciones para el sector público.
        </p>
        <form
          className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end"
          onSubmit={(e) => {
            e.preventDefault()
            if (!idProducto || !referencia || !valor) return
            registrar.mutate()
          }}
        >
          <label className="flex min-w-[220px] flex-1 flex-col gap-1 text-sm">
            <span className="text-graphite-600">Servicio</span>
            <select
              value={idProducto}
              onChange={(e) => setIdProducto(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            >
              <option value="">Seleccionar…</option>
              {productos?.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nombre}
                </option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-graphite-600">{productoSeleccionado?.tituloReferencia ?? 'Referencia'}</span>
            <input
              value={referencia}
              onChange={(e) => setReferencia(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          <label className="flex w-28 flex-col gap-1 text-sm">
            <span className="text-graphite-600">Valor</span>
            <input
              type="number"
              step="0.01"
              min="0.01"
              value={valor}
              onChange={(e) => setValor(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </label>
          {productoSeleccionado?.tieneComision && (
            <label className="flex w-28 flex-col gap-1 text-sm">
              <span className="text-graphite-600">Comisión</span>
              <input
                type="number"
                step="0.01"
                min="0"
                value={comision}
                onChange={(e) => setComision(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
          )}
          <button
            type="submit"
            disabled={registrar.isPending}
            className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            {registrar.isPending ? 'Registrando…' : 'Registrar cobro'}
          </button>
        </form>
        {registrar.isError && (
          <p className="mt-2 text-sm text-red-700">{mensajeErrorCajas(registrar.error, 'No se pudo registrar el cobro.')}</p>
        )}
      </div>

      <TableContainer>
        <thead>
          <tr>
            <Th>Servicio</Th>
            <Th>Referencia</Th>
            <Th>Valor</Th>
            <Th>Comisión</Th>
            <Th>Fecha</Th>
            <Th>Registrado por</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {(transacciones?.length ?? 0) === 0 && <EmptyState>Todavía no hay cobros registrados</EmptyState>}
          {transacciones?.map((t) => (
            <tr key={t.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td>{t.producto}</Td>
              <Td>{t.referencia}</Td>
              <Td className="tabular-nums">{formatoUsd(t.valor)}</Td>
              <Td className="tabular-nums">{formatoUsd(t.comision)}</Td>
              <Td>{new Date(t.fechaProceso).toLocaleString('es-EC')}</Td>
              <Td>{t.registradoPor}</Td>
              <Td>
                <Badge variant={t.reversada ? 'peligro' : 'exito'}>{t.reversada ? 'Reversada' : 'Registrada'}</Badge>
              </Td>
              <Td>
                {!t.reversada && (
                  <button
                    type="button"
                    disabled={reversar.isPending}
                    onClick={() => reversar.mutate(t.id)}
                    className="flex items-center gap-1 text-xs font-medium text-graphite-600 hover:underline disabled:opacity-50"
                  >
                    <Undo2 size={13} /> Reversar
                  </button>
                )}
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface TipoFormaNumerada {
  codigo: string
  nombre: string
}

interface FormaNumeradaItem {
  id: string
  idAgencia: number
  agencia: string
  codigoTipo: string
  tipo: string
  inicio: number
  fin: number
  fechaAsignacion: string
  registradoPor: string
  activo: boolean
}

function SeccionFormasNumeradas() {
  const queryClient = useQueryClient()
  const [mostrarForm, setMostrarForm] = useState(false)
  const [idAgencia, setIdAgencia] = useState('')
  const [codigoTipo, setCodigoTipo] = useState('')
  const [inicio, setInicio] = useState('')
  const [fin, setFin] = useState('')

  const { data: agencias } = useQuery<Agencia[]>({
    queryKey: ['config-agencias'],
    queryFn: async () => (await api.get('/api/configuracion/agencias')).data,
  })

  const { data: tipos } = useQuery<TipoFormaNumerada[]>({
    queryKey: ['cajas-formas-numeradas-tipos'],
    queryFn: async () => (await api.get('/api/cajas/formas-numeradas/tipos')).data,
  })

  const { data: formas } = useQuery<FormaNumeradaItem[]>({
    queryKey: ['cajas-formas-numeradas'],
    queryFn: async () => (await api.get('/api/cajas/formas-numeradas')).data,
  })

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/cajas/formas-numeradas', {
          idAgencia: Number(idAgencia),
          codigoTipo,
          inicio: Number(inicio),
          fin: Number(fin),
        })
      ).data,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-formas-numeradas'] })
      setMostrarForm(false)
      setIdAgencia('')
      setCodigoTipo('')
      setInicio('')
      setFin('')
    },
  })

  const toggleActivo = useMutation({
    mutationFn: async ({ id, activo }: { id: string; activo: boolean }) =>
      api.patch(`/api/cajas/formas-numeradas/${id}/estado`, { activo }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cajas-formas-numeradas'] })
    },
  })

  return (
    <div className="mt-8">
      <div className="mb-2 flex items-center justify-between">
        <h2 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-graphite-600">
          <Hash size={15} /> Formas numeradas — control de formularios pre-numerados
        </h2>
        {!mostrarForm && (
          <button
            type="button"
            onClick={() => setMostrarForm(true)}
            className="flex items-center gap-1 text-xs font-medium text-gold-400 hover:underline"
          >
            <Plus size={13} /> Asignar rango
          </button>
        )}
      </div>

      {mostrarForm && (
        <div className="glass-card mb-4 rounded-xl p-4">
          <form
            className="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end"
            onSubmit={(e) => {
              e.preventDefault()
              if (!idAgencia || !codigoTipo || !inicio || !fin) return
              crear.mutate()
            }}
          >
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Agencia</span>
              <select
                value={idAgencia}
                onChange={(e) => setIdAgencia(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              >
                <option value="">Seleccionar…</option>
                {agencias?.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.nombre}
                  </option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Tipo de formulario</span>
              <select
                value={codigoTipo}
                onChange={(e) => setCodigoTipo(e.target.value)}
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
            <label className="flex w-28 flex-col gap-1 text-sm">
              <span className="text-graphite-600">Número inicial</span>
              <input
                type="number"
                min="1"
                value={inicio}
                onChange={(e) => setInicio(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <label className="flex w-28 flex-col gap-1 text-sm">
              <span className="text-graphite-600">Número final</span>
              <input
                type="number"
                min="1"
                value={fin}
                onChange={(e) => setFin(e.target.value)}
                className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </label>
            <div className="flex items-center gap-3">
              <button
                type="submit"
                disabled={crear.isPending}
                className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                Asignar
              </button>
              <button type="button" onClick={() => setMostrarForm(false)} className="text-sm text-graphite-600 hover:text-graphite-100">
                Cancelar
              </button>
            </div>
          </form>
          {crear.isError && <p className="mt-2 text-sm text-red-700">{mensajeErrorCajas(crear.error, 'No se pudo asignar el rango.')}</p>}
        </div>
      )}

      <TableContainer>
        <thead>
          <tr>
            <Th>Agencia</Th>
            <Th>Tipo</Th>
            <Th>Rango</Th>
            <Th>Fecha</Th>
            <Th>Registrado por</Th>
            <Th>Estado</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {(formas?.length ?? 0) === 0 && <EmptyState>Todavía no hay rangos asignados</EmptyState>}
          {formas?.map((f) => (
            <tr key={f.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td>{f.agencia}</Td>
              <Td>{f.tipo}</Td>
              <Td className="tabular-nums">
                {f.inicio} — {f.fin}
              </Td>
              <Td>{f.fechaAsignacion}</Td>
              <Td>{f.registradoPor}</Td>
              <Td>
                <Badge variant={f.activo ? 'exito' : 'neutral'}>{f.activo ? 'Activo' : 'Inactivo'}</Badge>
              </Td>
              <Td>
                <button
                  type="button"
                  disabled={toggleActivo.isPending}
                  onClick={() => toggleActivo.mutate({ id: f.id, activo: !f.activo })}
                  className="text-xs font-medium text-graphite-600 hover:underline disabled:opacity-50"
                >
                  {f.activo ? 'Desactivar' : 'Activar'}
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

interface VentanillaAperturadaItem {
  agencia: string
  cajero: string
  fecha: string
  cerrada: boolean
  cuadrada: boolean
}

interface ConsolidadoTransaccionCajaItem {
  cajero: string
  fecha: string
  numeroTransacciones: number
  totalIngresos: number
  totalEgresos: number
}

const COLUMNAS_VENTANILLAS: ColumnaExportable<VentanillaAperturadaItem>[] = [
  { header: 'Agencia', accessor: (v) => v.agencia },
  { header: 'Cajero', accessor: (v) => v.cajero },
  { header: 'Fecha', accessor: (v) => v.fecha },
  { header: 'Estado', accessor: (v) => (v.cerrada ? 'Cerrada' : 'Abierta') },
  { header: 'Cuadre', accessor: (v) => (v.cerrada ? (v.cuadrada ? 'Cuadrada' : 'Descuadrada') : '—') },
]

const COLUMNAS_CONSOLIDADO: ColumnaExportable<ConsolidadoTransaccionCajaItem>[] = [
  { header: 'Cajero', accessor: (c) => c.cajero },
  { header: 'Fecha', accessor: (c) => c.fecha },
  { header: 'N° transacciones', accessor: (c) => c.numeroTransacciones },
  { header: 'Ingresos', accessor: (c) => c.totalIngresos },
  { header: 'Egresos', accessor: (c) => c.totalEgresos },
]

function SeccionReportesCajas() {
  const [reporte, setReporte] = useState<'ventanillas' | 'consolidado'>('ventanillas')
  const [desde, setDesde] = useState(() => new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10))
  const [hasta, setHasta] = useState(() => new Date().toISOString().slice(0, 10))

  const { data: ventanillas } = useQuery<VentanillaAperturadaItem[]>({
    queryKey: ['cajas-reporte-ventanillas', desde, hasta],
    queryFn: async () => (await api.get('/api/cajas/reportes/ventanillas-aperturadas', { params: { desde, hasta } })).data,
    enabled: reporte === 'ventanillas',
  })

  const { data: consolidado } = useQuery<ConsolidadoTransaccionCajaItem[]>({
    queryKey: ['cajas-reporte-consolidado', desde, hasta],
    queryFn: async () => (await api.get('/api/cajas/reportes/consolidado-transacciones', { params: { desde, hasta } })).data,
    enabled: reporte === 'consolidado',
  })

  return (
    <div className="mt-8">
      <div className="mb-2 flex items-center justify-between">
        <h2 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-graphite-600">
          <FileBarChart size={15} /> Reportes
        </h2>
        {reporte === 'ventanillas' && (
          <BotonesExportar
            nombreArchivo="cajas_ventanillas_aperturadas"
            titulo="Ventanillas aperturadas"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_VENTANILLAS}
            filas={ventanillas ?? []}
          />
        )}
        {reporte === 'consolidado' && (
          <BotonesExportar
            nombreArchivo="cajas_consolidado_transacciones"
            titulo="Consolidado de transacciones"
            subtitulo={`Del ${desde} al ${hasta}`}
            columnas={COLUMNAS_CONSOLIDADO}
            filas={consolidado ?? []}
          />
        )}
      </div>
      <div className="mb-4 flex flex-wrap items-end gap-3">
        <div className="flex gap-1 rounded-lg border border-black/[0.08] p-1">
          {(
            [
              ['ventanillas', 'Ventanillas aperturadas'],
              ['consolidado', 'Consolidado de transacciones'],
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
      </div>

      {reporte === 'ventanillas' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Agencia</Th>
              <Th>Cajero</Th>
              <Th>Fecha</Th>
              <Th>Estado</Th>
              <Th>Cuadre</Th>
            </tr>
          </thead>
          <tbody>
            {(ventanillas?.length ?? 0) === 0 && <EmptyState>Sin ventanillas aperturadas en el rango</EmptyState>}
            {ventanillas?.map((v, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td>{v.agencia}</Td>
                <Td className="font-medium">{v.cajero}</Td>
                <Td>{v.fecha}</Td>
                <Td>
                  <Badge variant={v.cerrada ? 'neutral' : 'exito'}>{v.cerrada ? 'Cerrada' : 'Abierta'}</Badge>
                </Td>
                <Td>
                  {v.cerrada ? <Badge variant={v.cuadrada ? 'exito' : 'peligro'}>{v.cuadrada ? 'Cuadrada' : 'Descuadrada'}</Badge> : '—'}
                </Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}

      {reporte === 'consolidado' && (
        <TableContainer>
          <thead>
            <tr>
              <Th>Cajero</Th>
              <Th>Fecha</Th>
              <Th>N° transacciones</Th>
              <Th>Ingresos</Th>
              <Th>Egresos</Th>
            </tr>
          </thead>
          <tbody>
            {(consolidado?.length ?? 0) === 0 && <EmptyState>Sin transacciones en el rango</EmptyState>}
            {consolidado?.map((c, i) => (
              <tr key={i} className="border-b border-black/[0.04] last:border-0">
                <Td className="font-medium">{c.cajero}</Td>
                <Td>{c.fecha}</Td>
                <Td className="tabular-nums">{c.numeroTransacciones}</Td>
                <Td className="tabular-nums">{formatoUsd(c.totalIngresos)}</Td>
                <Td className="tabular-nums">{formatoUsd(c.totalEgresos)}</Td>
              </tr>
            ))}
          </tbody>
        </TableContainer>
      )}
    </div>
  )
}
