import { useEffect, useRef, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { MessagesSquare, Send, Plus, Users, Compass, X, Paperclip, Download, FileText, Copy, Check, Forward } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { Badge } from '../components/Badge'
import { ModalPortal } from '../components/ModalPortal'
import { api } from '../lib/api'
import { idempotencyKey } from '../lib/idempotencyKey'
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

interface Adjunto {
  id: string
  nombreArchivo: string
  contentType: string
  tamanoBytes: number
}

interface Mensaje {
  id: string
  idCanal: string
  idUsuarioRemitente: string
  nombreRemitente: string
  texto: string | null
  creadoEn: string
  adjuntos: Adjunto[]
}

const EXTENSIONES_ADJUNTO_PERMITIDAS = '.jpg,.jpeg,.png,.gif,.webp,.pdf,.doc,.docx,.xls,.xlsx,.txt,.ppt,.pptx,.csv'
const MAX_ADJUNTOS_POR_MENSAJE = 3

function formatoTamano(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

async function descargarAdjunto(idAdjunto: string, nombre: string) {
  const respuesta = await api.get(`/api/comunicacion/adjuntos/${idAdjunto}`, { responseType: 'blob' })
  const url = window.URL.createObjectURL(new Blob([respuesta.data]))
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = nombre
  document.body.appendChild(enlace)
  enlace.click()
  enlace.remove()
  window.URL.revokeObjectURL(url)
}

/**
 * Convierte cualquier imagen (blob real, del formato que sea — jpeg/webp/gif)
 * a PNG real vía un canvas oculto antes de copiarla — la causa real más común
 * de que "Copiar" falle en silencio: la Clipboard API de imágenes solo
 * garantiza soporte real y consistente entre navegadores para `image/png`,
 * no para cualquier tipo MIME de origen (jpeg falla en varios navegadores
 * reales aunque el contexto sí sea seguro).
 */
async function blobComoPng(blob: Blob): Promise<Blob> {
  const bitmap = await createImageBitmap(blob)
  const canvas = document.createElement('canvas')
  canvas.width = bitmap.width
  canvas.height = bitmap.height
  const ctx = canvas.getContext('2d')
  if (!ctx) throw new Error('No se pudo obtener el contexto 2D del canvas')
  ctx.drawImage(bitmap, 0, 0)
  return await new Promise<Blob>((resolve, reject) => {
    canvas.toBlob((png) => (png ? resolve(png) : reject(new Error('canvas.toBlob devolvió null'))), 'image/png')
  })
}

/** Vista previa real de la imagen a tamaño completo — clic en la miniatura ya no descarga directo, abre esto (con opción real de Descargar o Copiar). */
function LightboxImagen({ url, nombre, onClose }: { url: string; nombre: string; onClose: () => void }) {
  const [copiado, setCopiado] = useState<'ok' | 'error' | null>(null)

  const copiar = async () => {
    try {
      if (!navigator.clipboard?.write) throw new Error('navigator.clipboard.write no disponible -- probablemente contexto no seguro (HTTP en vez de HTTPS)')
      const respuesta = await fetch(url)
      const blobOriginal = await respuesta.blob()
      // Siempre se copia como PNG real (ver blobComoPng) -- image/png es el
      // único formato con soporte real garantizado por la Clipboard API en
      // todos los navegadores, sin importar el formato real del archivo.
      const blobPng = await blobComoPng(blobOriginal)
      await navigator.clipboard.write([new ClipboardItem({ 'image/png': blobPng })])
      setCopiado('ok')
    } catch (error) {
      // Se avisa en vez de fallar en silencio -- "Descargar" siempre queda
      // como alternativa real.
      console.warn('No se pudo copiar la imagen al portapapeles:', error)
      setCopiado('error')
    }
    setTimeout(() => setCopiado(null), 2500)
  }

  return (
    <ModalPortal>
      <div className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-black/80 p-4" onClick={onClose}>
        <div className="mb-3 flex items-center gap-2" onClick={(e) => e.stopPropagation()}>
          <span className="max-w-[50vw] truncate text-sm text-white/90">{nombre}</span>
        </div>
        <img src={url} alt={nombre} className="max-h-[75vh] max-w-[90vw] rounded-lg object-contain" onClick={(e) => e.stopPropagation()} />
        <div className="mt-4 flex items-center gap-3" onClick={(e) => e.stopPropagation()}>
          <a
            href={url}
            download={nombre}
            className="btn-hover flex items-center gap-1.5 rounded-full bg-white px-4 py-2 text-sm font-medium text-graphite-100"
          >
            <Download size={15} /> Descargar
          </a>
          <button
            type="button"
            onClick={copiar}
            className="flex items-center gap-1.5 rounded-full border border-white/30 px-4 py-2 text-sm font-medium text-white hover:bg-white/10"
          >
            {copiado === 'ok' ? <Check size={15} /> : <Copy size={15} />}
            {copiado === 'ok' ? 'Copiada' : copiado === 'error' ? 'No se pudo copiar' : 'Copiar'}
          </button>
          <button type="button" onClick={onClose} className="flex items-center gap-1.5 rounded-full border border-white/30 px-4 py-2 text-sm font-medium text-white hover:bg-white/10">
            <X size={15} /> Cerrar
          </button>
        </div>
      </div>
    </ModalPortal>
  )
}

/** Imagen previsualizada inline (fetch autenticado a blob, como la descarga de Biblioteca de Documentos); cualquier otro tipo de archivo se muestra como chip descargable. */
function AdjuntoMensaje({ adjunto }: { adjunto: Adjunto }) {
  const esImagen = adjunto.contentType.startsWith('image/')
  const [urlImagen, setUrlImagen] = useState<string | null>(null)
  const [cargandoImagen, setCargandoImagen] = useState(esImagen)
  const [mostrarLightbox, setMostrarLightbox] = useState(false)

  useEffect(() => {
    if (!esImagen) return
    let objectUrl: string | null = null
    let cancelado = false
    api.get(`/api/comunicacion/adjuntos/${adjunto.id}`, { responseType: 'blob' }).then((r) => {
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
  }, [adjunto.id, esImagen])

  if (esImagen) {
    return (
      <>
        <button type="button" onClick={() => urlImagen && setMostrarLightbox(true)} title="Clic para ver en grande" className="block max-w-[240px] overflow-hidden rounded-lg">
          {cargandoImagen ? (
            <div className="flex h-32 w-48 items-center justify-center bg-black/[0.04] text-xs text-graphite-500">Cargando imagen…</div>
          ) : (
            <img src={urlImagen ?? undefined} alt={adjunto.nombreArchivo} className="max-h-64 w-auto rounded-lg" />
          )}
        </button>
        {mostrarLightbox && urlImagen && (
          <LightboxImagen url={urlImagen} nombre={adjunto.nombreArchivo} onClose={() => setMostrarLightbox(false)} />
        )}
      </>
    )
  }

  return (
    <button
      type="button"
      onClick={() => descargarAdjunto(adjunto.id, adjunto.nombreArchivo)}
      className="flex items-center gap-2 rounded-lg bg-black/[0.06] px-3 py-2 text-left text-xs hover:bg-black/[0.1]"
      title="Descargar"
    >
      <FileText size={16} className="flex-shrink-0" />
      <span className="flex flex-col">
        <span className="max-w-[180px] truncate font-medium">{adjunto.nombreArchivo}</span>
        <span className="text-graphite-500">{formatoTamano(adjunto.tamanoBytes)}</span>
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

const COLORES_AVATAR = [
  'bg-gold-500', 'bg-petrol-600', 'bg-graphite-500', 'bg-emerald-600',
  'bg-sky-600', 'bg-rose-500', 'bg-amber-600', 'bg-indigo-500',
]

function colorAvatar(nombre: string) {
  let hash = 0
  for (const c of nombre) hash = (hash * 31 + c.charCodeAt(0)) >>> 0
  return COLORES_AVATAR[hash % COLORES_AVATAR.length]
}

/** Avatar real con iniciales — mismo color estable para el mismo nombre, sin depender de una foto de perfil que este core no tiene. */
function Avatar({ nombre, esCanal, tamano = 'md' }: { nombre: string; esCanal?: boolean; tamano?: 'sm' | 'md' }) {
  const inicial = esCanal ? '#' : (nombre.trim()[0]?.toUpperCase() ?? '?')
  const clases = tamano === 'sm' ? 'h-7 w-7 text-[11px]' : 'h-9 w-9 text-xs'
  return (
    <span className={`flex ${clases} flex-shrink-0 items-center justify-center rounded-full font-semibold text-white ${colorAvatar(nombre)}`}>
      {inicial}
    </span>
  )
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

/**
 * Reenviar un mensaje real (texto y/o adjunto) a otra conversación — el
 * backend nunca vuelve a subir el archivo, solo crea un mensaje nuevo que
 * apunta al mismo adjunto ya guardado. Permite reenviar a cualquiera de
 * mis conversaciones existentes, o buscar una persona para iniciar (o
 * reusar) un directo nuevo — se puede reenviar a varias sin cerrar el
 * panel, cada destino queda marcado "Enviado" apenas se confirma.
 */
function ReenviarModal({ mensaje, canales, onClose }: { mensaje: Mensaje; canales: CanalListItem[]; onClose: () => void }) {
  const [q, setQ] = useState('')
  const [enviadosA, setEnviadosA] = useState<Set<string>>(new Set())
  const { data: usuarios } = useQuery<UsuarioParaChat[]>({
    queryKey: ['comunicacion-buscar-usuarios', q],
    queryFn: async () => (await api.get('/api/comunicacion/usuarios/buscar', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  const reenviarACanal = useMutation({
    mutationFn: async (idCanal: string) =>
      api.post(`/api/comunicacion/mensajes/${mensaje.id}/reenviar`, { idCanalDestino: idCanal }, {
        headers: { 'Idempotency-Key': idempotencyKey() },
      }),
    onSuccess: (_r, idCanal) => setEnviadosA((prev) => new Set(prev).add(idCanal)),
  })

  const reenviarAPersona = useMutation({
    mutationFn: async (idUsuario: string) => {
      const canal = (await api.post(`/api/comunicacion/directo/${idUsuario}`)).data as { id: string }
      await api.post(`/api/comunicacion/mensajes/${mensaje.id}/reenviar`, { idCanalDestino: canal.id }, {
        headers: { 'Idempotency-Key': idempotencyKey() },
      })
      return canal.id
    },
    onSuccess: (idCanal) => {
      setEnviadosA((prev) => new Set(prev).add(idCanal))
      setQ('')
    },
  })

  return (
    <ModalPortal>
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
        <div className="glass-card flex max-h-[80vh] w-full max-w-sm flex-col overflow-hidden rounded-xl" onClick={(e) => e.stopPropagation()}>
          <div className="flex items-center justify-between border-b border-black/[0.06] p-4 pb-3">
            <h3 className="flex items-center gap-2 font-medium text-graphite-100">
              <Forward size={16} /> Reenviar
            </h3>
            <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
              <X size={18} />
            </button>
          </div>

          <div className="border-b border-black/[0.06] px-4 py-2 text-xs text-graphite-600">
            {mensaje.adjuntos.length > 0 && (
              <p>📎 {mensaje.adjuntos.map((a) => a.nombreArchivo).join(', ')}</p>
            )}
            {mensaje.texto && <p className="truncate">{mensaje.texto}</p>}
          </div>

          <div className="p-3">
            <input
              value={q}
              onChange={(e) => setQ(e.target.value)}
              placeholder="Buscar persona para un directo nuevo…"
              className="w-full rounded-full border border-black/[0.08] bg-white px-3.5 py-1.5 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
            />
          </div>

          <div className="flex-1 overflow-y-auto px-2 pb-3">
            {q.length >= 2 ? (
              usuarios?.map((u) => (
                <button
                  key={u.id}
                  type="button"
                  disabled={reenviarAPersona.isPending}
                  onClick={() => reenviarAPersona.mutate(u.id)}
                  className="flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-left hover:bg-black/[0.03] disabled:opacity-60"
                >
                  <Avatar nombre={u.nombre} tamano="sm" />
                  <span className="flex-1 truncate text-sm text-graphite-100">{u.nombre}</span>
                  <Check size={14} className="text-emerald-600 opacity-0" />
                </button>
              ))
            ) : (
              canales.map((c) => {
                const enviado = enviadosA.has(c.id)
                return (
                  <button
                    key={c.id}
                    type="button"
                    disabled={reenviarACanal.isPending || enviado}
                    onClick={() => reenviarACanal.mutate(c.id)}
                    className="flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-left hover:bg-black/[0.03] disabled:opacity-60"
                  >
                    <Avatar nombre={c.nombre} esCanal={!c.esDirecto} tamano="sm" />
                    <span className="flex-1 truncate text-sm text-graphite-100">{c.nombre}</span>
                    {enviado && (
                      <span className="flex items-center gap-1 text-xs font-medium text-emerald-600">
                        <Check size={13} /> Enviado
                      </span>
                    )}
                  </button>
                )
              })
            )}
          </div>

          {(reenviarACanal.isError || reenviarAPersona.isError) && (
            <p className="border-t border-black/[0.06] px-4 py-2 text-xs text-red-700">
              {detalleError(reenviarACanal.error ?? reenviarAPersona.error, 'No se pudo reenviar.')}
            </p>
          )}

          <div className="border-t border-black/[0.06] p-3">
            <button type="button" onClick={onClose} className="btn-hover w-full rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white">
              Listo
            </button>
          </div>
        </div>
      </div>
    </ModalPortal>
  )
}

export function ComunicacionInterna() {
  const queryClient = useQueryClient()
  const { sesion } = useAuth()
  const [canalSeleccionado, setCanalSeleccionado] = useState<string | null>(null)
  const [texto, setTexto] = useState('')
  const [archivos, setArchivos] = useState<File[]>([])
  const archivoInputRef = useRef<HTMLInputElement>(null)
  const [mostrarNuevoCanal, setMostrarNuevoCanal] = useState(false)
  const [mostrarBuscarDirecto, setMostrarBuscarDirecto] = useState(false)
  const [mostrarDescubrir, setMostrarDescubrir] = useState(false)
  const [mostrarMenuNuevo, setMostrarMenuNuevo] = useState(false)
  const [mensajeAReenviar, setMensajeAReenviar] = useState<Mensaje | null>(null)
  const [busquedaCanal, setBusquedaCanal] = useState('')
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
      for (const archivo of archivos) form.append('Archivos', archivo)
      return api.post(`/api/comunicacion/canales/${canalSeleccionado}/mensajes`, form, {
        headers: { 'Idempotency-Key': idempotencyKey() },
      })
    },
    onSuccess: () => {
      setTexto('')
      setArchivos([])
      if (archivoInputRef.current) archivoInputRef.current.value = ''
    },
  })

  // Agrega uno o varios archivos respetando el máximo real de 3 por
  // mensaje -- usado tanto por el selector de archivo como por pegar
  // (Ctrl+V) una captura de pantalla real desde el portapapeles.
  const agregarArchivos = (nuevos: File[]) => {
    setArchivos((prev) => [...prev, ...nuevos].slice(0, MAX_ADJUNTOS_POR_MENSAJE))
  }

  const manejarPegado = (e: React.ClipboardEvent) => {
    const items = Array.from(e.clipboardData.items).filter((item) => item.kind === 'file')
    if (items.length === 0) return
    e.preventDefault()
    const archivosPegados = items
      .map((item) => item.getAsFile())
      .filter((f): f is File => f !== null)
      .map((f, i) => (f.name && f.name !== 'image.png' ? f : new File([f], `captura-${Date.now()}-${i}.png`, { type: f.type })))
    agregarArchivos(archivosPegados)
  }

  const iniciarDirecto = useMutation({
    mutationFn: async (idUsuario: string) => (await api.post(`/api/comunicacion/directo/${idUsuario}`)).data as { id: string },
    onSuccess: async (canal) => {
      queryClient.invalidateQueries({ queryKey: ['comunicacion-canales'] })
      const hub = await iniciarConexionComunicacion()
      await hub.invoke('UnirseACanal', canal.id)
      setCanalSeleccionado(canal.id)
      setMostrarBuscarDirecto(false)
      setBusquedaCanal('')
    },
  })

  const canalActivo = canales?.find((c) => c.id === canalSeleccionado)
  const busquedaCanalLimpia = busquedaCanal.trim()
  const canalesFiltrados = (canales ?? []).filter((c) => c.nombre.toLowerCase().includes(busquedaCanalLimpia.toLowerCase()))

  // La misma barra de búsqueda también busca personas reales -- clic
  // directo arma (o reusa) la conversación directa con esa persona, sin
  // tener que ir a "Nuevo mensaje directo" aparte.
  const { data: usuariosBusqueda } = useQuery<UsuarioParaChat[]>({
    queryKey: ['comunicacion-buscar-usuarios', busquedaCanalLimpia],
    queryFn: async () => (await api.get('/api/comunicacion/usuarios/buscar', { params: { q: busquedaCanalLimpia } })).data,
    enabled: busquedaCanalLimpia.length >= 2,
  })

  const abrirPanel = (panel: 'canal' | 'directo' | 'descubrir') => {
    setMostrarMenuNuevo(false)
    setMostrarNuevoCanal(panel === 'canal')
    setMostrarBuscarDirecto(panel === 'directo')
    setMostrarDescubrir(panel === 'descubrir')
  }

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={MessagesSquare}
        title="Comunicación interna"
        subtitle="Chat propio del core — entre áreas (ej. Cajas ↔ Balcón de Servicio) o directo entre personas"
      />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-[320px_1fr]">
        <div className="glass-card flex h-[78vh] flex-col rounded-xl p-3">
          <div className="mb-2 flex items-center gap-2">
            <div className="relative flex-1">
              <input
                value={busquedaCanal}
                onChange={(e) => setBusquedaCanal(e.target.value)}
                placeholder="Buscar conversación o persona…"
                className="w-full rounded-full border border-black/[0.08] bg-white px-3.5 py-1.5 text-xs text-graphite-100 outline-none focus:border-gold-500/50"
              />
            </div>
            <div className="relative">
              <button
                type="button"
                onClick={() => setMostrarMenuNuevo((v) => !v)}
                title="Nueva conversación"
                className="btn-hover flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full bg-gold-500 text-white"
              >
                <Plus size={16} />
              </button>
              {mostrarMenuNuevo && (
                <>
                  <div className="fixed inset-0 z-10" onClick={() => setMostrarMenuNuevo(false)} />
                  <div className="absolute right-0 top-10 z-20 w-48 overflow-hidden rounded-xl border border-black/[0.08] bg-white py-1 shadow-lg">
                    <button type="button" onClick={() => abrirPanel('directo')} className="flex w-full items-center gap-2 px-3 py-2 text-left text-xs text-graphite-100 hover:bg-black/[0.03]">
                      <Users size={14} /> Mensaje directo
                    </button>
                    <button type="button" onClick={() => abrirPanel('canal')} className="flex w-full items-center gap-2 px-3 py-2 text-left text-xs text-graphite-100 hover:bg-black/[0.03]">
                      <Plus size={14} /> Crear canal
                    </button>
                    <button type="button" onClick={() => abrirPanel('descubrir')} className="flex w-full items-center gap-2 px-3 py-2 text-left text-xs text-graphite-100 hover:bg-black/[0.03]">
                      <Compass size={14} /> Descubrir canales
                    </button>
                  </div>
                </>
              )}
            </div>
          </div>

          <div className="flex-1 overflow-y-auto">
            {canales?.length === 0 && (
              <div className="flex flex-col items-center gap-2 px-2 py-10 text-center">
                <MessagesSquare size={28} className="text-graphite-300" />
                <p className="text-xs text-graphite-600">Todavía no tenés conversaciones — tocá "+" para empezar una.</p>
              </div>
            )}
            {canales && canales.length > 0 && canalesFiltrados.length === 0 && busquedaCanalLimpia.length < 2 && (
              <p className="px-2 py-4 text-xs text-graphite-600">Ninguna conversación coincide con "{busquedaCanal}".</p>
            )}
            {canalesFiltrados.map((c) => (
              <button
                key={c.id}
                type="button"
                onClick={() => seleccionarCanal(c.id)}
                className={`mb-1 flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-left transition ${
                  c.id === canalSeleccionado ? 'bg-gold-500/15' : 'hover:bg-black/[0.03]'
                }`}
              >
                <Avatar nombre={c.nombre} esCanal={!c.esDirecto} />
                <div className="min-w-0 flex-1">
                  <div className="flex w-full items-center justify-between">
                    <span className={`truncate text-sm ${c.noLeidos > 0 ? 'font-semibold text-graphite-100' : 'font-medium text-graphite-100'}`}>
                      {c.nombre}
                    </span>
                    {c.noLeidos > 0 && (
                      <span className="ml-1.5 flex h-5 min-w-5 flex-shrink-0 items-center justify-center rounded-full bg-gold-500 px-1.5 text-[11px] font-semibold text-white">
                        {c.noLeidos}
                      </span>
                    )}
                  </div>
                  {c.ultimoMensajeTexto && (
                    <span className={`block w-full truncate text-xs ${c.noLeidos > 0 ? 'text-graphite-700' : 'text-graphite-500'}`}>
                      {!c.esDirecto && `${c.ultimoMensajeAutor}: `}{c.ultimoMensajeTexto}
                    </span>
                  )}
                </div>
              </button>
            ))}

            {busquedaCanalLimpia.length >= 2 && usuariosBusqueda && usuariosBusqueda.length > 0 && (
              <div className="mt-2 border-t border-black/[0.06] pt-2">
                <p className="px-2.5 pb-1 text-[11px] font-semibold uppercase tracking-wide text-graphite-500">Personas</p>
                {usuariosBusqueda
                  .filter((u) => !canalesFiltrados.some((c) => c.esDirecto && c.nombre === u.nombre))
                  .map((u) => (
                    <button
                      key={u.id}
                      type="button"
                      disabled={iniciarDirecto.isPending}
                      onClick={() => iniciarDirecto.mutate(u.id)}
                      className="mb-1 flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-left transition hover:bg-black/[0.03] disabled:opacity-60"
                    >
                      <Avatar nombre={u.nombre} tamano="sm" />
                      <span className="flex-1 truncate text-sm text-graphite-100">{u.nombre}</span>
                      <span className="text-[11px] text-graphite-500">Nuevo chat</span>
                    </button>
                  ))}
              </div>
            )}
          </div>
        </div>

        <div className="glass-card flex h-[78vh] flex-col rounded-xl p-4">
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

          {canalSeleccionado && canalActivo && (
            <>
              <div className="mb-3 flex items-center gap-2.5 border-b border-black/[0.06] pb-3">
                <Avatar nombre={canalActivo.nombre} esCanal={!canalActivo.esDirecto} />
                <h3 className="font-medium text-graphite-100">{canalActivo.nombre}</h3>
              </div>

              <div className="flex-1 space-y-3 overflow-y-auto pr-1">
                {mensajes?.map((m) => {
                  const esPropio = m.idUsuarioRemitente === sesion?.idUsuario
                  return (
                    <div key={m.id} className={`flex items-end gap-2 ${esPropio ? 'flex-row-reverse' : 'flex-row'}`}>
                      {!esPropio && <Avatar nombre={m.nombreRemitente} tamano="sm" />}
                      <div className={`flex max-w-[70%] flex-col ${esPropio ? 'items-end' : 'items-start'}`}>
                        {!esPropio && !canalActivo.esDirecto && (
                          <span className="mb-0.5 px-1 text-xs font-medium text-graphite-600">{m.nombreRemitente}</span>
                        )}
                        <div
                          className={`flex flex-col gap-1.5 rounded-2xl px-3.5 py-2 text-sm ${
                            esPropio ? 'rounded-br-sm bg-gold-500 text-white' : 'rounded-bl-sm bg-black/[0.045] text-graphite-100'
                          }`}
                        >
                          {m.adjuntos.map((a) => (
                            <AdjuntoMensaje key={a.id} adjunto={a} />
                          ))}
                          {m.texto && <span className="whitespace-pre-wrap break-words">{m.texto}</span>}
                        </div>
                        <span className="mt-0.5 flex items-center gap-1.5 px-1 text-[11px] text-graphite-500">
                          {formatoHora(m.creadoEn)}
                          <button
                            type="button"
                            onClick={() => setMensajeAReenviar(m)}
                            title="Reenviar"
                            className="text-graphite-400 hover:text-gold-500"
                          >
                            <Forward size={12} />
                          </button>
                        </span>
                      </div>
                    </div>
                  )
                })}
                <div ref={mensajesFinRef} />
              </div>

              {archivos.length > 0 && (
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {archivos.map((a, i) => (
                    <div key={`${a.name}-${i}`} className="flex items-center gap-1.5 rounded-lg border border-black/[0.08] bg-white px-2.5 py-1 text-xs">
                      <span className="max-w-[160px] truncate">{a.name}</span>
                      <span className="text-graphite-500">({formatoTamano(a.size)})</span>
                      <button type="button" onClick={() => setArchivos((prev) => prev.filter((_, idx) => idx !== i))} className="text-graphite-500 hover:text-red-700">
                        <X size={13} />
                      </button>
                    </div>
                  ))}
                </div>
              )}
              <form
                className="mt-3 flex items-center gap-2 border-t border-black/[0.06] pt-3"
                onSubmit={(e) => {
                  e.preventDefault()
                  if (!texto.trim() && archivos.length === 0) return
                  enviar.mutate()
                }}
              >
                <input
                  ref={archivoInputRef}
                  type="file"
                  multiple
                  accept={EXTENSIONES_ADJUNTO_PERMITIDAS}
                  onChange={(e) => {
                    agregarArchivos(Array.from(e.target.files ?? []))
                    if (archivoInputRef.current) archivoInputRef.current.value = ''
                  }}
                  className="hidden"
                  id="comunicacion-archivo-input"
                />
                <label
                  htmlFor="comunicacion-archivo-input"
                  title={`Adjuntar imagen o archivo (máx. ${MAX_ADJUNTOS_POR_MENSAJE})`}
                  className={`flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full text-graphite-600 hover:bg-black/[0.05] ${
                    archivos.length >= MAX_ADJUNTOS_POR_MENSAJE ? 'pointer-events-none opacity-40' : 'cursor-pointer'
                  }`}
                >
                  <Paperclip size={17} />
                </label>
                <input
                  value={texto}
                  onChange={(e) => setTexto(e.target.value)}
                  onPaste={manejarPegado}
                  maxLength={2000}
                  placeholder="Escribí un mensaje o pegá una captura (Ctrl+V)…"
                  className="flex-1 rounded-full border border-black/[0.08] bg-white px-4 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
                />
                <button
                  type="submit"
                  disabled={enviar.isPending || (!texto.trim() && archivos.length === 0)}
                  title="Enviar"
                  className="btn-hover flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-gold-500 text-white disabled:opacity-50"
                >
                  <Send size={15} />
                </button>
              </form>
              {enviar.isError && <p className="mt-1 text-xs text-red-700">{detalleError(enviar.error, 'No se pudo enviar el mensaje.')}</p>}
            </>
          )}
        </div>
      </div>

      {mensajeAReenviar && (
        <ReenviarModal mensaje={mensajeAReenviar} canales={canales ?? []} onClose={() => setMensajeAReenviar(null)} />
      )}
    </div>
  )
}
