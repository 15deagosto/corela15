import { useEffect, useRef, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { MessagesSquare, Send, Plus, Users, Compass, X, Paperclip, Download, FileText } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'
import { useAuth } from '../lib/AuthContext'
import { iniciarConexionComunicacion } from '../lib/comunicacionHub'

interface CanalListItem {
  id: string
  nombre: string
  esDirecto: boolean
  ultimoMensajeTexto: string | null
  ultimoMensajeAutor: string | null
  ultimoMensajeFecha: string | null
  noLeidos: number
}

interface Mensaje {
  id: string
  idCanal: string
  idUsuarioRemitente: string
  nombreRemitente: string
  texto: string | null
  creadoEn: string
  nombreArchivoAdjunto: string | null
  contentTypeAdjunto: string | null
  tamanoBytesAdjunto: number | null
}

const EXTENSIONES_ADJUNTO_PERMITIDAS = '.jpg,.jpeg,.png,.gif,.webp,.pdf,.doc,.docx,.xls,.xlsx,.txt,.ppt,.pptx,.csv'

function formatoTamano(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

/** Imagen previsualizada inline (fetch autenticado a blob, como la descarga de Biblioteca de Documentos); cualquier otro tipo de archivo se muestra como chip descargable. */
function AdjuntoMensaje({ idMensaje, nombre, contentType, tamanoBytes }: { idMensaje: string; nombre: string; contentType: string | null; tamanoBytes: number | null }) {
  const esImagen = contentType?.startsWith('image/') ?? false
  const [urlImagen, setUrlImagen] = useState<string | null>(null)
  const [cargandoImagen, setCargandoImagen] = useState(esImagen)

  useEffect(() => {
    if (!esImagen) return
    let objectUrl: string | null = null
    let cancelado = false
    api.get(`/api/comunicacion/mensajes/${idMensaje}/adjunto`, { responseType: 'blob' }).then((r) => {
      if (cancelado) return
      objectUrl = window.URL.createObjectURL(new Blob([r.data]))
      setUrlImagen(objectUrl)
      setCargandoImagen(false)
    })
    return () => {
      cancelado = true
      if (objectUrl) window.URL.revokeObjectURL(objectUrl)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [idMensaje, esImagen])

  const descargar = async () => {
    const respuesta = await api.get(`/api/comunicacion/mensajes/${idMensaje}/adjunto`, { responseType: 'blob' })
    const url = window.URL.createObjectURL(new Blob([respuesta.data]))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = nombre
    document.body.appendChild(enlace)
    enlace.click()
    enlace.remove()
    window.URL.revokeObjectURL(url)
  }

  if (esImagen) {
    return (
      <button type="button" onClick={descargar} title="Clic para descargar" className="block max-w-[240px] overflow-hidden rounded-lg">
        {cargandoImagen ? (
          <div className="flex h-32 w-48 items-center justify-center bg-black/[0.04] text-xs text-graphite-500">Cargando imagen…</div>
        ) : (
          <img src={urlImagen ?? undefined} alt={nombre} className="max-h-64 w-auto rounded-lg" />
        )}
      </button>
    )
  }

  return (
    <button
      type="button"
      onClick={descargar}
      className="flex items-center gap-2 rounded-lg bg-black/[0.06] px-3 py-2 text-left text-xs hover:bg-black/[0.1]"
      title="Descargar"
    >
      <FileText size={16} className="flex-shrink-0" />
      <span className="flex flex-col">
        <span className="max-w-[180px] truncate font-medium">{nombre}</span>
        {tamanoBytes !== null && <span className="text-graphite-500">{formatoTamano(tamanoBytes)}</span>}
      </span>
      <Download size={13} className="ml-1 flex-shrink-0" />
    </button>
  )
}

interface CanalDescubrible {
  id: string
  nombre: string
  descripcion: string | null
  yaSoyMiembro: boolean
}

interface UsuarioParaChat {
  id: string
  nombre: string
}

function detalleError(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
}

function formatoHora(fecha: string) {
  return new Date(fecha).toLocaleTimeString('es-EC', { hour: '2-digit', minute: '2-digit' })
}

function NuevoCanalForm({ onCreado, onClose }: { onCreado: (idCanal: string) => void; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [nombre, setNombre] = useState('')
  const [q, setQ] = useState('')
  const [seleccionados, setSeleccionados] = useState<UsuarioParaChat[]>([])

  const { data: usuarios } = useQuery<UsuarioParaChat[]>({
    queryKey: ['comunicacion-buscar-usuarios', q],
    queryFn: async () => (await api.get('/api/comunicacion/usuarios/buscar', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  const crear = useMutation({
    mutationFn: async () =>
      (
        await api.post('/api/comunicacion/canales', {
          nombre,
          descripcion: null,
          idsMiembrosIniciales: seleccionados.map((u) => u.id),
        })
      ).data as { id: string },
    onSuccess: (canal) => {
      queryClient.invalidateQueries({ queryKey: ['comunicacion-canales'] })
      onCreado(canal.id)
    },
  })

  return (
    <div className="glass-card mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nuevo canal</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      <form
        className="flex flex-col gap-3"
        onSubmit={(e) => {
          e.preventDefault()
          if (!nombre.trim()) return
          crear.mutate()
        }}
      >
        <input
          required
          value={nombre}
          onChange={(e) => setNombre(e.target.value)}
          placeholder="Nombre del canal (ej. Atención Agencia Pilacoto)"
          className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
        />

        <div className="flex flex-wrap gap-1.5">
          {seleccionados.map((u) => (
            <span key={u.id} className="flex items-center gap-1 rounded-full bg-black/[0.04] px-2.5 py-0.5 text-xs text-graphite-100">
              {u.nombre}
              <button type="button" onClick={() => setSeleccionados((s) => s.filter((x) => x.id !== u.id))}>
                <X size={12} />
              </button>
            </span>
          ))}
        </div>

        <input
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder="Agregar miembros — buscar por nombre o usuario…"
          className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
        />
        {q.length >= 2 && (usuarios?.length ?? 0) > 0 && (
          <div className="max-h-32 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
            {usuarios
              ?.filter((u) => !seleccionados.some((s) => s.id === u.id))
              .map((u) => (
                <button
                  key={u.id}
                  type="button"
                  onClick={() => {
                    setSeleccionados((s) => [...s, u])
                    setQ('')
                  }}
                  className="block w-full px-3 py-1.5 text-left text-sm hover:bg-black/[0.02]"
                >
                  {u.nombre}
                </button>
              ))}
          </div>
        )}

        <button
          type="submit"
          disabled={crear.isPending}
          className="btn-hover self-start rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
        >
          {crear.isPending ? 'Creando…' : 'Crear canal'}
        </button>
        {crear.isError && <p className="text-sm text-red-700">{detalleError(crear.error, 'No se pudo crear el canal.')}</p>}
      </form>
    </div>
  )
}

function BuscarUsuarioParaDirecto({ onSeleccionar, onClose }: { onSeleccionar: (u: UsuarioParaChat) => void; onClose: () => void }) {
  const [q, setQ] = useState('')
  const { data: usuarios } = useQuery<UsuarioParaChat[]>({
    queryKey: ['comunicacion-buscar-usuarios', q],
    queryFn: async () => (await api.get('/api/comunicacion/usuarios/buscar', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  return (
    <div className="glass-card mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Nuevo mensaje directo</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      <input
        autoFocus
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar persona por nombre o usuario…"
        className="mb-2 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
      />
      {q.length >= 2 && (
        <div className="max-h-40 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
          {usuarios?.map((u) => (
            <button
              key={u.id}
              type="button"
              onClick={() => onSeleccionar(u)}
              className="block w-full px-3 py-1.5 text-left text-sm hover:bg-black/[0.02]"
            >
              {u.nombre}
            </button>
          ))}
          {usuarios?.length === 0 && <p className="px-3 py-1.5 text-xs text-graphite-600">Sin resultados.</p>}
        </div>
      )}
    </div>
  )
}

function DescubrirCanales({ onUnido, onClose }: { onUnido: (idCanal: string) => void; onClose: () => void }) {
  const queryClient = useQueryClient()
  const { data: canales, isLoading } = useQuery<CanalDescubrible[]>({
    queryKey: ['comunicacion-descubrir'],
    queryFn: async () => (await api.get('/api/comunicacion/canales/descubrir')).data,
  })
  const { sesion } = useAuth()

  const unirse = useMutation({
    mutationFn: async (idCanal: string) =>
      api.post(`/api/comunicacion/canales/${idCanal}/miembros`, { idUsuario: sesion!.idUsuario }),
    onSuccess: async (_r, idCanal) => {
      queryClient.invalidateQueries({ queryKey: ['comunicacion-canales'] })
      queryClient.invalidateQueries({ queryKey: ['comunicacion-descubrir'] })
      const hub = await iniciarConexionComunicacion()
      await hub.invoke('UnirseACanal', idCanal)
      onUnido(idCanal)
    },
  })

  return (
    <div className="glass-card mb-4 rounded-xl p-4">
      <div className="mb-3 flex items-center justify-between">
        <h3 className="font-medium text-graphite-100">Descubrir canales</h3>
        <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
          <X size={18} />
        </button>
      </div>
      {isLoading && <p className="text-sm text-graphite-600">Cargando…</p>}
      <div className="flex flex-col gap-2">
        {canales?.map((c) => (
          <div key={c.id} className="flex items-center justify-between rounded-lg bg-black/[0.02] px-3 py-2">
            <div>
              <p className="text-sm font-medium text-graphite-100">{c.nombre}</p>
              {c.descripcion && <p className="text-xs text-graphite-600">{c.descripcion}</p>}
            </div>
            {c.yaSoyMiembro ? (
              <Badge variant="exito">Ya sos miembro</Badge>
            ) : (
              <button
                type="button"
                disabled={unirse.isPending}
                onClick={() => unirse.mutate(c.id)}
                className="text-xs font-medium text-gold-500 hover:underline disabled:opacity-50"
              >
                Unirse
              </button>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}

export function ComunicacionInterna() {
  const queryClient = useQueryClient()
  const { sesion } = useAuth()
  const [canalSeleccionado, setCanalSeleccionado] = useState<string | null>(null)
  const [texto, setTexto] = useState('')
  const [archivo, setArchivo] = useState<File | null>(null)
  const archivoInputRef = useRef<HTMLInputElement>(null)
  const [mostrarNuevoCanal, setMostrarNuevoCanal] = useState(false)
  const [mostrarBuscarDirecto, setMostrarBuscarDirecto] = useState(false)
  const [mostrarDescubrir, setMostrarDescubrir] = useState(false)
  const canalSeleccionadoRef = useRef<string | null>(null)
  const mensajesFinRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    canalSeleccionadoRef.current = canalSeleccionado
  }, [canalSeleccionado])

  const { data: canales } = useQuery<CanalListItem[]>({
    queryKey: ['comunicacion-canales'],
    queryFn: async () => (await api.get('/api/comunicacion/canales')).data,
    refetchInterval: 30000, // respaldo si el WebSocket se cae y todavía no reconectó
  })

  const { data: mensajes } = useQuery<Mensaje[]>({
    queryKey: ['comunicacion-mensajes', canalSeleccionado],
    queryFn: async () => (await api.get(`/api/comunicacion/canales/${canalSeleccionado}/mensajes`)).data,
    enabled: !!canalSeleccionado,
  })

  // Conexión en tiempo real: una sola vez por montaje real de la pantalla.
  useEffect(() => {
    let activo = true

    iniciarConexionComunicacion().then((hub) => {
      hub.on('mensajeNuevo', (mensaje: Mensaje) => {
        if (!activo) return
        if (mensaje.idCanal === canalSeleccionadoRef.current) {
          queryClient.setQueryData<Mensaje[]>(['comunicacion-mensajes', mensaje.idCanal], (prev) =>
            prev?.some((m) => m.id === mensaje.id) ? prev : [...(prev ?? []), mensaje],
          )
          api.post(`/api/comunicacion/canales/${mensaje.idCanal}/marcar-leido`).catch(() => undefined)
        }
        queryClient.invalidateQueries({ queryKey: ['comunicacion-canales'] })
      })

      hub.on('agregadoACanal', () => {
        if (!activo) return
        queryClient.invalidateQueries({ queryKey: ['comunicacion-canales'] })
        queryClient.invalidateQueries({ queryKey: ['comunicacion-descubrir'] })
      })
    })

    return () => {
      activo = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    mensajesFinRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [mensajes])

  const seleccionarCanal = (idCanal: string) => {
    setCanalSeleccionado(idCanal)
    api.post(`/api/comunicacion/canales/${idCanal}/marcar-leido`).then(() => {
      queryClient.invalidateQueries({ queryKey: ['comunicacion-canales'] })
    })
  }

  const enviar = useMutation({
    mutationFn: async () => {
      const form = new FormData()
      if (texto.trim()) form.append('Texto', texto.trim())
      if (archivo) form.append('Archivo', archivo)
      return api.post(`/api/comunicacion/canales/${canalSeleccionado}/mensajes`, form, {
        headers: { 'Content-Type': 'multipart/form-data', 'Idempotency-Key': crypto.randomUUID() },
      })
    },
    onSuccess: () => {
      setTexto('')
      setArchivo(null)
      if (archivoInputRef.current) archivoInputRef.current.value = ''
    },
  })

  const iniciarDirecto = useMutation({
    mutationFn: async (idUsuario: string) => (await api.post(`/api/comunicacion/directo/${idUsuario}`)).data as { id: string },
    onSuccess: async (canal) => {
      queryClient.invalidateQueries({ queryKey: ['comunicacion-canales'] })
      const hub = await iniciarConexionComunicacion()
      await hub.invoke('UnirseACanal', canal.id)
      setCanalSeleccionado(canal.id)
      setMostrarBuscarDirecto(false)
    },
  })

  const canalActivo = canales?.find((c) => c.id === canalSeleccionado)

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={MessagesSquare}
        title="Comunicación interna"
        subtitle="Chat propio del core — entre áreas (ej. Cajas ↔ Balcón de Servicio) o directo entre personas"
      />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-[320px_1fr]">
        <div className="glass-card flex max-h-[70vh] flex-col rounded-xl p-3">
          <div className="mb-2 flex items-center gap-1.5">
            <button
              type="button"
              onClick={() => {
                setMostrarNuevoCanal((v) => !v)
                setMostrarBuscarDirecto(false)
                setMostrarDescubrir(false)
              }}
              className="flex items-center gap-1 rounded-lg px-2 py-1.5 text-xs font-medium text-graphite-600 hover:bg-black/[0.03]"
              title="Nuevo canal"
            >
              <Plus size={14} /> Canal
            </button>
            <button
              type="button"
              onClick={() => {
                setMostrarBuscarDirecto((v) => !v)
                setMostrarNuevoCanal(false)
                setMostrarDescubrir(false)
              }}
              className="flex items-center gap-1 rounded-lg px-2 py-1.5 text-xs font-medium text-graphite-600 hover:bg-black/[0.03]"
              title="Nuevo mensaje directo"
            >
              <Users size={14} /> Directo
            </button>
            <button
              type="button"
              onClick={() => {
                setMostrarDescubrir((v) => !v)
                setMostrarNuevoCanal(false)
                setMostrarBuscarDirecto(false)
              }}
              className="flex items-center gap-1 rounded-lg px-2 py-1.5 text-xs font-medium text-graphite-600 hover:bg-black/[0.03]"
              title="Descubrir canales"
            >
              <Compass size={14} /> Descubrir
            </button>
          </div>

          <div className="flex-1 overflow-y-auto">
            {canales?.length === 0 && <p className="px-2 py-4 text-xs text-graphite-600">Todavía no tenés canales.</p>}
            {canales?.map((c) => (
              <button
                key={c.id}
                type="button"
                onClick={() => seleccionarCanal(c.id)}
                className={`mb-1 flex w-full flex-col items-start rounded-lg px-3 py-2 text-left transition ${
                  c.id === canalSeleccionado ? 'bg-gold-500/15' : 'hover:bg-black/[0.03]'
                }`}
              >
                <div className="flex w-full items-center justify-between">
                  <span className="text-sm font-medium text-graphite-100">
                    {c.esDirecto ? '' : '# '}
                    {c.nombre}
                  </span>
                  {c.noLeidos > 0 && (
                    <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-gold-500 px-1.5 text-[11px] font-semibold text-white">
                      {c.noLeidos}
                    </span>
                  )}
                </div>
                {c.ultimoMensajeTexto && (
                  <span className="w-full truncate text-xs text-graphite-600">
                    {c.ultimoMensajeAutor}: {c.ultimoMensajeTexto}
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>

        <div className="glass-card flex max-h-[70vh] flex-col rounded-xl p-4">
          {mostrarNuevoCanal && (
            <NuevoCanalForm
              onCreado={async (idCanal) => {
                setMostrarNuevoCanal(false)
                const hub = await iniciarConexionComunicacion()
                await hub.invoke('UnirseACanal', idCanal)
                setCanalSeleccionado(idCanal)
              }}
              onClose={() => setMostrarNuevoCanal(false)}
            />
          )}
          {mostrarBuscarDirecto && (
            <BuscarUsuarioParaDirecto
              onSeleccionar={(u) => iniciarDirecto.mutate(u.id)}
              onClose={() => setMostrarBuscarDirecto(false)}
            />
          )}
          {mostrarDescubrir && (
            <DescubrirCanales
              onUnido={(idCanal) => {
                setMostrarDescubrir(false)
                setCanalSeleccionado(idCanal)
              }}
              onClose={() => setMostrarDescubrir(false)}
            />
          )}

          {!canalSeleccionado && !mostrarNuevoCanal && !mostrarBuscarDirecto && !mostrarDescubrir && (
            <div className="flex flex-1 items-center justify-center text-sm text-graphite-600">
              Elegí un canal o iniciá un mensaje directo.
            </div>
          )}

          {canalSeleccionado && (
            <>
              <h3 className="mb-3 border-b border-black/[0.06] pb-2 font-medium text-graphite-100">
                {canalActivo?.esDirecto ? '' : '# '}
                {canalActivo?.nombre}
              </h3>

              <div className="flex-1 space-y-3 overflow-y-auto pr-1">
                {mensajes?.map((m) => {
                  const esPropio = m.idUsuarioRemitente === sesion?.idUsuario
                  return (
                    <div key={m.id} className={`flex flex-col ${esPropio ? 'items-end' : 'items-start'}`}>
                      {!esPropio && <span className="mb-0.5 text-xs font-medium text-graphite-600">{m.nombreRemitente}</span>}
                      <div
                        className={`flex max-w-[75%] flex-col gap-1.5 rounded-xl px-3 py-2 text-sm ${
                          esPropio ? 'bg-gold-500 text-white' : 'bg-black/[0.04] text-graphite-100'
                        }`}
                      >
                        {m.nombreArchivoAdjunto && (
                          <AdjuntoMensaje
                            idMensaje={m.id}
                            nombre={m.nombreArchivoAdjunto}
                            contentType={m.contentTypeAdjunto}
                            tamanoBytes={m.tamanoBytesAdjunto}
                          />
                        )}
                        {m.texto && <span>{m.texto}</span>}
                      </div>
                      <span className="mt-0.5 text-[11px] text-graphite-500">{formatoHora(m.creadoEn)}</span>
                    </div>
                  )
                })}
                <div ref={mensajesFinRef} />
              </div>

              {archivo && (
                <div className="mt-2 flex items-center justify-between rounded-lg border border-black/[0.08] bg-white px-3 py-1.5 text-xs">
                  <span className="truncate">
                    {archivo.name} <span className="text-graphite-500">({formatoTamano(archivo.size)})</span>
                  </span>
                  <button type="button" onClick={() => { setArchivo(null); if (archivoInputRef.current) archivoInputRef.current.value = '' }} className="text-graphite-500 hover:text-red-700">
                    <X size={14} />
                  </button>
                </div>
              )}
              <form
                className="mt-3 flex items-center gap-2 border-t border-black/[0.06] pt-3"
                onSubmit={(e) => {
                  e.preventDefault()
                  if (!texto.trim() && !archivo) return
                  enviar.mutate()
                }}
              >
                <input
                  ref={archivoInputRef}
                  type="file"
                  accept={EXTENSIONES_ADJUNTO_PERMITIDAS}
                  onChange={(e) => setArchivo(e.target.files?.[0] ?? null)}
                  className="hidden"
                  id="comunicacion-archivo-input"
                />
                <label
                  htmlFor="comunicacion-archivo-input"
                  title="Adjuntar imagen o archivo"
                  className="flex cursor-pointer items-center justify-center rounded-lg border border-black/[0.08] p-2 text-graphite-600 hover:bg-black/[0.03]"
                >
                  <Paperclip size={16} />
                </label>
                <input
                  value={texto}
                  onChange={(e) => setTexto(e.target.value)}
                  maxLength={2000}
                  placeholder="Escribí un mensaje…"
                  className="flex-1 rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
                />
                <button
                  type="submit"
                  disabled={enviar.isPending || (!texto.trim() && !archivo)}
                  className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white disabled:opacity-60"
                >
                  <Send size={16} />
                </button>
              </form>
              {enviar.isError && <p className="mt-1 text-xs text-red-700">{detalleError(enviar.error, 'No se pudo enviar el mensaje.')}</p>}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
