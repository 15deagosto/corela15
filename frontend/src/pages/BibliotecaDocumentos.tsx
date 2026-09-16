import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Library, Plus, X, Download, UploadCloud, Ban, Grid3x3, Lock, Trash2 } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { BotonesExportar } from '../components/BotonesExportar'
import { ModalPortal } from '../components/ModalPortal'
import { api } from '../lib/api'
import type { ColumnaExportable } from '../lib/exportar'

interface CatalogoItem {
  codigo: string
  nombre: string
}

interface DocumentoItem {
  id: string
  titulo: string
  area: string
  tipo: string
  version: string
  estado: string
  nombreArchivoOriginal: string
  tamanoBytes: number
  instanciaAprobacion: string | null
  instanciaRevision: string | null
  fechaAprobacion: string | null
  proximaRevision: string | null
  notas: string | null
  creadoPor: string
  creadoEn: string
  modificadoEn: string | null
}

interface MatrizCelda {
  total: number
  vigentes: number
}

interface MatrizFila {
  area: string
  celdas: Record<string, MatrizCelda>
}

interface MatrizDocumental {
  filas: MatrizFila[]
}

/** ACL real por área — "veTodo" (ADMINISTRADOR o biblioteca-documentos-gerencia) ve/administra todo sin filas de accesoPorArea. Mismo modelo mental que una carpeta compartida de red: sin fila acá, no se ve nada de esa área. */
interface MiAcceso {
  veTodo: boolean
  accesoPorArea: Record<string, string>
}

interface AreaAccesoItem {
  id: string
  idUsuario: string
  nombreUsuario: string
  area: string
  nivelAcceso: string
  creadoEn: string
  creadoPor: string
}

function mensajeError(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
}

function formatoTamano(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function estadoVariant(estado: string) {
  if (estado === 'Vigente') return 'exito' as const
  if (estado === 'Obsoleto') return 'peligro' as const
  if (estado === 'EnRevision') return 'alerta' as const
  return 'neutral' as const
}

function nombreCatalogo(catalogo: CatalogoItem[] | undefined, codigo: string) {
  return catalogo?.find((c) => c.codigo === codigo)?.nombre ?? codigo
}

function tieneLectura(acceso: MiAcceso | undefined, area: string) {
  if (!acceso) return false
  return acceso.veTodo || area in acceso.accesoPorArea
}

function tieneEscritura(acceso: MiAcceso | undefined, area: string) {
  if (!acceso) return false
  return acceso.veTodo || acceso.accesoPorArea[area] === 'Escritura'
}

const inputClase =
  'rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50'

function useCatalogos() {
  const areas = useQuery<CatalogoItem[]>({
    queryKey: ['biblioteca-documentos-areas'],
    queryFn: async () => (await api.get('/api/biblioteca-documentos/areas')).data,
  })
  const tipos = useQuery<CatalogoItem[]>({
    queryKey: ['biblioteca-documentos-tipos'],
    queryFn: async () => (await api.get('/api/biblioteca-documentos/tipos')).data,
  })
  const estados = useQuery<CatalogoItem[]>({
    queryKey: ['biblioteca-documentos-estados'],
    queryFn: async () => (await api.get('/api/biblioteca-documentos/estados')).data,
  })
  return { areas: areas.data, tipos: tipos.data, estados: estados.data }
}

function useMiAcceso() {
  return useQuery<MiAcceso>({
    queryKey: ['biblioteca-documentos-mi-acceso'],
    queryFn: async () => (await api.get('/api/biblioteca-documentos/mi-acceso')).data,
  })
}

function NuevoDocumentoModal({
  areasEscribibles,
  tipos,
  onClose,
}: {
  areasEscribibles: CatalogoItem[]
  tipos: CatalogoItem[]
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [titulo, setTitulo] = useState('')
  const [area, setArea] = useState('')
  const [tipo, setTipo] = useState('')
  const [version, setVersion] = useState('1.0')
  const [instanciaAprobacion, setInstanciaAprobacion] = useState('')
  const [instanciaRevision, setInstanciaRevision] = useState('')
  const [fechaAprobacion, setFechaAprobacion] = useState('')
  const [proximaRevision, setProximaRevision] = useState('')
  const [notas, setNotas] = useState('')
  const [archivo, setArchivo] = useState<File | null>(null)

  const crear = useMutation({
    mutationFn: async () => {
      const form = new FormData()
      form.append('Titulo', titulo)
      form.append('Area', area)
      form.append('Tipo', tipo)
      form.append('Version', version)
      if (instanciaAprobacion) form.append('InstanciaAprobacion', instanciaAprobacion)
      if (instanciaRevision) form.append('InstanciaRevision', instanciaRevision)
      if (fechaAprobacion) form.append('FechaAprobacion', fechaAprobacion)
      if (proximaRevision) form.append('ProximaRevision', proximaRevision)
      if (notas) form.append('Notas', notas)
      form.append('Archivo', archivo as File)
      return api.post('/api/biblioteca-documentos', form, {
        headers: { 'Content-Type': 'multipart/form-data', 'Idempotency-Key': crypto.randomUUID() },
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos'] })
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos-matriz'] })
      onClose()
    },
  })

  return (
    <ModalPortal>
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
        <div
          className="glass-card flex max-h-[90vh] w-full max-w-2xl flex-col overflow-y-auto rounded-xl p-5"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="mb-4 flex items-center justify-between">
            <h3 className="font-medium text-graphite-100">Nuevo documento</h3>
            <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
              <X size={18} />
            </button>
          </div>

          <form
            className="grid grid-cols-1 gap-4 sm:grid-cols-2"
            onSubmit={(e) => {
              e.preventDefault()
              if (!titulo || !area || !tipo || !archivo) return
              crear.mutate()
            }}
          >
            <label className="flex flex-col gap-1 text-sm sm:col-span-2">
              <span className="text-graphite-600">Título</span>
              <input required type="text" value={titulo} onChange={(e) => setTitulo(e.target.value)} className={inputClase} />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Área responsable</span>
              <select required value={area} onChange={(e) => setArea(e.target.value)} className={inputClase}>
                <option value="">Seleccionar…</option>
                {areasEscribibles.map((a) => (
                  <option key={a.codigo} value={a.codigo}>
                    {a.nombre}
                  </option>
                ))}
              </select>
              {areasEscribibles.length === 0 && (
                <span className="text-xs text-red-700">No tenés acceso de escritura a ninguna área todavía.</span>
              )}
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Tipo de documento</span>
              <select required value={tipo} onChange={(e) => setTipo(e.target.value)} className={inputClase}>
                <option value="">Seleccionar…</option>
                {tipos.map((t) => (
                  <option key={t.codigo} value={t.codigo}>
                    {t.nombre}
                  </option>
                ))}
              </select>
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Versión</span>
              <input type="text" value={version} onChange={(e) => setVersion(e.target.value)} className={inputClase} />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Archivo (pdf, doc, docx, xls, xlsx, txt, ppt, pptx — máx. 20MB)</span>
              <input
                required
                type="file"
                accept=".pdf,.doc,.docx,.xls,.xlsx,.txt,.ppt,.pptx"
                onChange={(e) => setArchivo(e.target.files?.[0] ?? null)}
                className="text-xs text-graphite-600"
              />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Instancia de aprobación (opcional)</span>
              <input type="text" value={instanciaAprobacion} onChange={(e) => setInstanciaAprobacion(e.target.value)} className={inputClase} placeholder="Ej. Consejo de Administración" />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Instancia de revisión (opcional)</span>
              <input type="text" value={instanciaRevision} onChange={(e) => setInstanciaRevision(e.target.value)} className={inputClase} placeholder="Ej. Comité de Seguridad" />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Fecha de aprobación (opcional)</span>
              <input type="date" value={fechaAprobacion} onChange={(e) => setFechaAprobacion(e.target.value)} className={inputClase} />
            </label>

            <label className="flex flex-col gap-1 text-sm">
              <span className="text-graphite-600">Próxima revisión sugerida (opcional)</span>
              <input type="date" value={proximaRevision} onChange={(e) => setProximaRevision(e.target.value)} className={inputClase} />
            </label>

            <label className="flex flex-col gap-1 text-sm sm:col-span-2">
              <span className="text-graphite-600">Notas (opcional)</span>
              <textarea value={notas} onChange={(e) => setNotas(e.target.value)} rows={2} className={inputClase} />
            </label>

            <div className="sm:col-span-2">
              <button
                type="submit"
                disabled={crear.isPending || areasEscribibles.length === 0}
                className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
              >
                {crear.isPending ? 'Subiendo…' : 'Guardar documento'}
              </button>
            </div>

            {crear.isError && (
              <p className="sm:col-span-2 text-sm text-red-700">{mensajeError(crear.error, 'No se pudo guardar el documento.')}</p>
            )}
          </form>
        </div>
      </div>
    </ModalPortal>
  )
}

type TabGestion = 'metadatos' | 'version'

function GestionarDocumentoModal({
  documento,
  areas,
  areasEscribibles,
  tipos,
  estados,
  puedeEditar,
  onClose,
}: {
  documento: DocumentoItem
  areas: CatalogoItem[]
  areasEscribibles: CatalogoItem[]
  tipos: CatalogoItem[]
  estados: CatalogoItem[]
  puedeEditar: boolean
  onClose: () => void
}) {
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<TabGestion>('metadatos')

  const [titulo, setTitulo] = useState(documento.titulo)
  const [area, setArea] = useState(documento.area)
  const [tipo, setTipo] = useState(documento.tipo)
  const [estado, setEstado] = useState(documento.estado)
  const [instanciaAprobacion, setInstanciaAprobacion] = useState(documento.instanciaAprobacion ?? '')
  const [instanciaRevision, setInstanciaRevision] = useState(documento.instanciaRevision ?? '')
  const [fechaAprobacion, setFechaAprobacion] = useState(documento.fechaAprobacion ?? '')
  const [proximaRevision, setProximaRevision] = useState(documento.proximaRevision ?? '')
  const [notas, setNotas] = useState(documento.notas ?? '')

  const [nuevaVersion, setNuevaVersion] = useState('')
  const [archivoNuevo, setArchivoNuevo] = useState<File | null>(null)

  // El selector de área en edición siempre ofrece, además de las áreas
  // donde el usuario tiene Escritura, la área ACTUAL del documento (para
  // no forzar un cambio de área solo por estar viendo el formulario).
  const opcionesArea = [
    ...areasEscribibles,
    ...(areasEscribibles.some((a) => a.codigo === documento.area) ? [] : [{ codigo: documento.area, nombre: nombreCatalogo(areas, documento.area) }]),
  ]

  const guardarMetadatos = useMutation({
    mutationFn: async () =>
      api.put(`/api/biblioteca-documentos/${documento.id}`, {
        titulo, area, tipo, estado,
        instanciaAprobacion: instanciaAprobacion || null,
        instanciaRevision: instanciaRevision || null,
        fechaAprobacion: fechaAprobacion || null,
        proximaRevision: proximaRevision || null,
        notas: notas || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos'] })
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos-matriz'] })
      onClose()
    },
  })

  const subirVersion = useMutation({
    mutationFn: async () => {
      const form = new FormData()
      form.append('Version', nuevaVersion)
      form.append('Archivo', archivoNuevo as File)
      return api.post(`/api/biblioteca-documentos/${documento.id}/nueva-version`, form, {
        headers: { 'Content-Type': 'multipart/form-data', 'Idempotency-Key': crypto.randomUUID() },
      })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos'] })
      onClose()
    },
  })

  const desactivar = useMutation({
    mutationFn: async () => api.delete(`/api/biblioteca-documentos/${documento.id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos'] })
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos-matriz'] })
      onClose()
    },
  })

  const descargar = async () => {
    const respuesta = await api.get(`/api/biblioteca-documentos/${documento.id}/descargar`, { responseType: 'blob' })
    const url = window.URL.createObjectURL(new Blob([respuesta.data]))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = documento.nombreArchivoOriginal
    document.body.appendChild(enlace)
    enlace.click()
    enlace.remove()
    window.URL.revokeObjectURL(url)
  }

  return (
    <ModalPortal>
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={onClose}>
        <div
          className="glass-card flex max-h-[85vh] w-full max-w-3xl flex-col overflow-hidden rounded-xl"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="flex items-center justify-between border-b border-black/[0.06] p-5 pb-4">
            <h2 className="flex items-center gap-2 text-sm font-semibold text-graphite-100">
              <Library size={16} /> Gestionar: {documento.titulo}
            </h2>
            <button type="button" onClick={onClose} className="text-graphite-600 hover:text-graphite-100">
              <X size={18} />
            </button>
          </div>

          <div className="flex flex-1 overflow-hidden">
            <div className="flex w-48 flex-shrink-0 flex-col gap-0.5 border-r border-black/[0.06] p-2">
              <button
                type="button"
                onClick={() => setTab('metadatos')}
                className={`rounded-lg px-3 py-2 text-left text-xs font-medium transition-colors ${
                  tab === 'metadatos' ? 'bg-gold-500/10 text-gold-300' : 'text-graphite-600 hover:bg-black/[0.02]'
                }`}
              >
                Metadatos
              </button>
              {puedeEditar && (
                <button
                  type="button"
                  onClick={() => setTab('version')}
                  className={`rounded-lg px-3 py-2 text-left text-xs font-medium transition-colors ${
                    tab === 'version' ? 'bg-gold-500/10 text-gold-300' : 'text-graphite-600 hover:bg-black/[0.02]'
                  }`}
                >
                  Nueva versión
                </button>
              )}
              <div className="mt-2 border-t border-black/[0.06] pt-2">
                <button type="button" onClick={descargar} className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-xs font-medium text-petrol-700 hover:bg-black/[0.02]">
                  <Download size={13} /> Descargar
                </button>
                {puedeEditar && (
                  <button
                    type="button"
                    onClick={() => {
                      if (confirm(`¿Desactivar "${documento.titulo}"? Deja de aparecer en el listado (nunca se borra el archivo real).`)) desactivar.mutate()
                    }}
                    className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-xs font-medium text-red-700 hover:bg-black/[0.02]"
                  >
                    <Ban size={13} /> Desactivar
                  </button>
                )}
              </div>
            </div>

            <div className="flex-1 overflow-y-auto p-5">
              {tab === 'metadatos' && (
                <fieldset disabled={!puedeEditar}>
                  {!puedeEditar && (
                    <p className="mb-3 text-xs text-graphite-500">No tenés acceso de escritura sobre el área de este documento — solo lectura.</p>
                  )}
                  <form
                    className="grid grid-cols-1 gap-4 sm:grid-cols-2"
                    onSubmit={(e) => {
                      e.preventDefault()
                      guardarMetadatos.mutate()
                    }}
                  >
                    <label className="flex flex-col gap-1 text-sm sm:col-span-2">
                      <span className="text-graphite-600">Título</span>
                      <input required type="text" value={titulo} onChange={(e) => setTitulo(e.target.value)} className={inputClase} />
                    </label>
                    <label className="flex flex-col gap-1 text-sm">
                      <span className="text-graphite-600">Área</span>
                      <select value={area} onChange={(e) => setArea(e.target.value)} className={inputClase}>
                        {opcionesArea.map((a) => (
                          <option key={a.codigo} value={a.codigo}>{a.nombre}</option>
                        ))}
                      </select>
                    </label>
                    <label className="flex flex-col gap-1 text-sm">
                      <span className="text-graphite-600">Tipo</span>
                      <select value={tipo} onChange={(e) => setTipo(e.target.value)} className={inputClase}>
                        {tipos.map((t) => (
                          <option key={t.codigo} value={t.codigo}>{t.nombre}</option>
                        ))}
                      </select>
                    </label>
                    <label className="flex flex-col gap-1 text-sm">
                      <span className="text-graphite-600">Estado</span>
                      <select value={estado} onChange={(e) => setEstado(e.target.value)} className={inputClase}>
                        {estados.map((e_) => (
                          <option key={e_.codigo} value={e_.codigo}>{e_.nombre}</option>
                        ))}
                      </select>
                    </label>
                    <div />
                    <label className="flex flex-col gap-1 text-sm">
                      <span className="text-graphite-600">Instancia de aprobación</span>
                      <input type="text" value={instanciaAprobacion} onChange={(e) => setInstanciaAprobacion(e.target.value)} className={inputClase} />
                    </label>
                    <label className="flex flex-col gap-1 text-sm">
                      <span className="text-graphite-600">Instancia de revisión</span>
                      <input type="text" value={instanciaRevision} onChange={(e) => setInstanciaRevision(e.target.value)} className={inputClase} />
                    </label>
                    <label className="flex flex-col gap-1 text-sm">
                      <span className="text-graphite-600">Fecha de aprobación</span>
                      <input type="date" value={fechaAprobacion} onChange={(e) => setFechaAprobacion(e.target.value)} className={inputClase} />
                    </label>
                    <label className="flex flex-col gap-1 text-sm">
                      <span className="text-graphite-600">Próxima revisión</span>
                      <input type="date" value={proximaRevision} onChange={(e) => setProximaRevision(e.target.value)} className={inputClase} />
                    </label>
                    <label className="flex flex-col gap-1 text-sm sm:col-span-2">
                      <span className="text-graphite-600">Notas</span>
                      <textarea value={notas} onChange={(e) => setNotas(e.target.value)} rows={2} className={inputClase} />
                    </label>
                    {puedeEditar && (
                      <div className="sm:col-span-2">
                        <button type="submit" disabled={guardarMetadatos.isPending} className="btn-hover rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60">
                          {guardarMetadatos.isPending ? 'Guardando…' : 'Guardar cambios'}
                        </button>
                      </div>
                    )}
                    {guardarMetadatos.isError && (
                      <p className="sm:col-span-2 text-sm text-red-700">{mensajeError(guardarMetadatos.error, 'No se pudo guardar.')}</p>
                    )}
                  </form>
                </fieldset>
              )}

              {tab === 'version' && puedeEditar && (
                <form
                  className="flex flex-col gap-4"
                  onSubmit={(e) => {
                    e.preventDefault()
                    if (!archivoNuevo) return
                    subirVersion.mutate()
                  }}
                >
                  <p className="text-xs text-graphite-500">
                    Archivo actual: <span className="font-medium text-graphite-100">{documento.nombreArchivoOriginal}</span> (
                    {formatoTamano(documento.tamanoBytes)}, versión {documento.version}) — el archivo anterior no se
                    borra del NAS, solo deja de ser el vigente.
                  </p>
                  <label className="flex flex-col gap-1 text-sm">
                    <span className="text-graphite-600">Versión nueva</span>
                    <input type="text" value={nuevaVersion} onChange={(e) => setNuevaVersion(e.target.value)} placeholder={documento.version} className={inputClase} />
                  </label>
                  <label className="flex flex-col gap-1 text-sm">
                    <span className="text-graphite-600">Archivo nuevo</span>
                    <input
                      required
                      type="file"
                      accept=".pdf,.doc,.docx,.xls,.xlsx,.txt,.ppt,.pptx"
                      onChange={(e) => setArchivoNuevo(e.target.files?.[0] ?? null)}
                      className="text-xs text-graphite-600"
                    />
                  </label>
                  <div>
                    <button type="submit" disabled={subirVersion.isPending} className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60">
                      <UploadCloud size={15} /> {subirVersion.isPending ? 'Subiendo…' : 'Subir nueva versión'}
                    </button>
                  </div>
                  {subirVersion.isError && <p className="text-sm text-red-700">{mensajeError(subirVersion.error, 'No se pudo subir el archivo.')}</p>}
                </form>
              )}
            </div>
          </div>
        </div>
      </div>
    </ModalPortal>
  )
}

function MatrizCobertura({ areas, tipos }: { areas: CatalogoItem[]; tipos: CatalogoItem[] }) {
  const { data, isLoading } = useQuery<MatrizDocumental>({
    queryKey: ['biblioteca-documentos-matriz'],
    queryFn: async () => (await api.get('/api/biblioteca-documentos/matriz')).data,
  })

  if (isLoading) return <p className="text-sm text-graphite-600">Cargando…</p>

  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[900px] border-collapse text-xs">
        <thead>
          <tr>
            <th className="sticky left-0 border-b border-black/[0.08] bg-white p-2 text-left font-medium text-graphite-600">Área</th>
            {tipos.map((t) => (
              <th key={t.codigo} className="border-b border-black/[0.08] p-2 text-center font-medium text-graphite-600">
                {t.nombre}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data?.filas.map((fila) => (
            <tr key={fila.area} className="border-b border-black/[0.04]">
              <td className="sticky left-0 bg-white p-2 font-medium text-graphite-100">{nombreCatalogo(areas, fila.area)}</td>
              {tipos.map((t) => {
                const celda = fila.celdas[t.codigo]
                const vacio = !celda || celda.total === 0
                return (
                  <td key={t.codigo} className="p-2 text-center">
                    {vacio ? (
                      <span className="text-graphite-300">—</span>
                    ) : (
                      <span className={celda.vigentes > 0 ? 'font-medium text-petrol-700' : 'font-medium text-gold-400'}>
                        {celda.vigentes}/{celda.total}
                      </span>
                    )}
                  </td>
                )
              })}
            </tr>
          ))}
        </tbody>
      </table>
      <p className="mt-3 text-xs text-graphite-500">
        vigentes/total por área × tipo — una celda vacía es un hueco real de cobertura documental que esa jefatura
        todavía no ha subido.
      </p>
    </div>
  )
}

function BuscarUsuarioAcceso({ onSeleccionar }: { onSeleccionar: (u: { id: string; nombreUsuario: string }) => void }) {
  const [q, setQ] = useState('')
  const { data: usuarios } = useQuery<{ id: string; nombreUsuario: string }[]>({
    queryKey: ['biblioteca-documentos-buscar-usuario', q],
    queryFn: async () => (await api.get('/api/biblioteca-documentos/accesos/usuarios/buscar', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  return (
    <div className="flex flex-col gap-1">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar usuario por nombre…"
        className={`${inputClase} py-1.5`}
      />
      {q.length >= 2 && (usuarios?.length ?? 0) > 0 && (
        <div className="max-h-32 overflow-y-auto rounded-lg border border-black/[0.08] bg-white">
          {usuarios?.map((u) => (
            <button
              key={u.id}
              type="button"
              onClick={() => {
                onSeleccionar(u)
                setQ('')
              }}
              className="block w-full px-3 py-1.5 text-left text-xs hover:bg-black/[0.02]"
            >
              {u.nombreUsuario}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

function SeccionAccesos({ areas }: { areas: CatalogoItem[] }) {
  const queryClient = useQueryClient()
  const [filtroArea, setFiltroArea] = useState('')
  const [usuarioSeleccionado, setUsuarioSeleccionado] = useState<{ id: string; nombreUsuario: string } | null>(null)
  const [areaOtorgar, setAreaOtorgar] = useState('')
  const [nivelOtorgar, setNivelOtorgar] = useState('Lectura')

  const { data: accesos, isLoading } = useQuery<AreaAccesoItem[]>({
    queryKey: ['biblioteca-documentos-accesos', filtroArea],
    queryFn: async () => (await api.get('/api/biblioteca-documentos/accesos', { params: { area: filtroArea || undefined } })).data,
  })

  const otorgar = useMutation({
    mutationFn: async () =>
      api.post('/api/biblioteca-documentos/accesos', {
        idUsuario: usuarioSeleccionado!.id,
        area: areaOtorgar,
        nivelAcceso: nivelOtorgar,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos-accesos'] })
      setUsuarioSeleccionado(null)
      setAreaOtorgar('')
      setNivelOtorgar('Lectura')
    },
  })

  const quitar = useMutation({
    mutationFn: async (id: string) => api.delete(`/api/biblioteca-documentos/accesos/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['biblioteca-documentos-accesos'] }),
  })

  return (
    <div>
      <p className="mb-4 text-xs text-graphite-500">
        Mismo modelo que una carpeta compartida de red: por defecto nadie ve los documentos de un área ajena — acá se
        otorga acceso de Lectura o Escritura por usuario y área. Escritura incluye Lectura (crear, editar, subir
        nueva versión, desactivar).
      </p>

      <div className="glass-card mb-6 rounded-xl p-4">
        <h3 className="mb-3 text-sm font-medium text-graphite-100">Otorgar acceso</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-4 sm:items-end">
          <div className="flex flex-col gap-1 text-xs sm:col-span-2">
            <span className="text-graphite-600">Usuario</span>
            {usuarioSeleccionado ? (
              <div className="flex items-center justify-between rounded-lg border border-black/[0.08] bg-white px-3 py-1.5 text-sm">
                {usuarioSeleccionado.nombreUsuario}
                <button type="button" onClick={() => setUsuarioSeleccionado(null)} className="text-graphite-500 hover:text-red-700">
                  <X size={14} />
                </button>
              </div>
            ) : (
              <BuscarUsuarioAcceso onSeleccionar={setUsuarioSeleccionado} />
            )}
          </div>
          <label className="flex flex-col gap-1 text-xs">
            <span className="text-graphite-600">Área</span>
            <select value={areaOtorgar} onChange={(e) => setAreaOtorgar(e.target.value)} className={`${inputClase} py-1.5`}>
              <option value="">Seleccionar…</option>
              {areas.map((a) => (
                <option key={a.codigo} value={a.codigo}>{a.nombre}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-1 text-xs">
            <span className="text-graphite-600">Nivel</span>
            <select value={nivelOtorgar} onChange={(e) => setNivelOtorgar(e.target.value)} className={`${inputClase} py-1.5`}>
              <option value="Lectura">Lectura</option>
              <option value="Escritura">Escritura</option>
            </select>
          </label>
        </div>
        <button
          type="button"
          disabled={!usuarioSeleccionado || !areaOtorgar || otorgar.isPending}
          onClick={() => otorgar.mutate()}
          className="btn-hover mt-3 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        >
          {otorgar.isPending ? 'Otorgando…' : 'Otorgar acceso'}
        </button>
        {otorgar.isError && <p className="mt-2 text-sm text-red-700">{mensajeError(otorgar.error, 'No se pudo otorgar el acceso.')}</p>}
      </div>

      <label className="mb-3 flex max-w-xs flex-col gap-1 text-xs">
        <span className="text-graphite-600">Filtrar por área</span>
        <select value={filtroArea} onChange={(e) => setFiltroArea(e.target.value)} className={`${inputClase} py-1.5`}>
          <option value="">Todas</option>
          {areas.map((a) => (
            <option key={a.codigo} value={a.codigo}>{a.nombre}</option>
          ))}
        </select>
      </label>

      <TableContainer>
        <thead>
          <tr>
            <Th>Usuario</Th>
            <Th>Área</Th>
            <Th>Nivel</Th>
            <Th>Otorgado por</Th>
            <Th></Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (accesos?.length ?? 0) === 0 && <EmptyState>Sin accesos otorgados todavía</EmptyState>}
          {accesos?.map((a) => (
            <tr key={a.id} className="border-b border-black/[0.04] last:border-0">
              <Td className="font-medium">{a.nombreUsuario}</Td>
              <Td>{nombreCatalogo(areas, a.area)}</Td>
              <Td>
                <Badge variant={a.nivelAcceso === 'Escritura' ? 'alerta' : 'neutral'}>{a.nivelAcceso}</Badge>
              </Td>
              <Td>{a.creadoPor}</Td>
              <Td>
                <button
                  type="button"
                  onClick={() => {
                    if (confirm(`¿Quitar el acceso de ${a.nombreUsuario} a ${nombreCatalogo(areas, a.area)}?`)) quitar.mutate(a.id)
                  }}
                  className="text-red-700 hover:underline"
                  title="Quitar acceso"
                >
                  <Trash2 size={14} />
                </button>
              </Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}

export function BibliotecaDocumentos() {
  const { data: miAcceso } = useMiAcceso()
  const { areas, tipos, estados } = useCatalogos()
  const [tab, setTab] = useState<'documentos' | 'matriz' | 'accesos'>('documentos')
  const [mostrarNuevo, setMostrarNuevo] = useState(false)
  const [seleccionado, setSeleccionado] = useState<DocumentoItem | null>(null)
  const [filtroArea, setFiltroArea] = useState('')
  const [filtroTipo, setFiltroTipo] = useState('')
  const [filtroEstado, setFiltroEstado] = useState('')
  const [busqueda, setBusqueda] = useState('')

  const veTodo = miAcceso?.veTodo ?? false
  const areasEscribibles = (areas ?? []).filter((a) => tieneEscritura(miAcceso, a.codigo))
  const areasLegibles = veTodo ? (areas ?? []) : (areas ?? []).filter((a) => tieneLectura(miAcceso, a.codigo))
  const puedeCrear = areasEscribibles.length > 0

  const { data: documentos, isLoading } = useQuery<DocumentoItem[]>({
    queryKey: ['biblioteca-documentos', filtroArea, filtroTipo, filtroEstado, busqueda],
    queryFn: async () =>
      (
        await api.get('/api/biblioteca-documentos', {
          params: { area: filtroArea || undefined, tipo: filtroTipo || undefined, estado: filtroEstado || undefined, q: busqueda || undefined },
        })
      ).data,
    enabled: !!miAcceso,
  })

  const columnas: ColumnaExportable<DocumentoItem>[] = [
    { header: 'Título', accessor: (d) => d.titulo },
    { header: 'Área', accessor: (d) => nombreCatalogo(areas, d.area) },
    { header: 'Tipo', accessor: (d) => nombreCatalogo(tipos, d.tipo) },
    { header: 'Versión', accessor: (d) => d.version },
    { header: 'Estado', accessor: (d) => nombreCatalogo(estados, d.estado) },
    { header: 'Instancia de aprobación', accessor: (d) => d.instanciaAprobacion ?? '' },
    { header: 'Próxima revisión', accessor: (d) => d.proximaRevision ?? '' },
    { header: 'Subido por', accessor: (d) => d.creadoPor },
  ]

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={Library}
        title="Biblioteca de Documentos"
        subtitle="Políticas, reglamentos, manuales, procedimientos y formatos institucionales — archivo real en el NAS de la cooperativa"
        actions={
          tab === 'documentos' &&
          puedeCrear && (
            <button
              type="button"
              onClick={() => setMostrarNuevo(true)}
              className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-3 py-2 text-sm font-medium text-white"
            >
              <Plus size={16} /> Nuevo documento
            </button>
          )
        }
      />

      {!veTodo && (
        <p className="mb-4 flex items-center gap-1.5 text-xs text-graphite-500">
          <Lock size={12} /> Solo ves los documentos de las áreas a las que tenés acceso otorgado.
        </p>
      )}

      <div className="mb-4 flex gap-1 border-b border-black/[0.06]">
        <button
          type="button"
          onClick={() => setTab('documentos')}
          className={`px-3 py-2 text-sm font-medium ${tab === 'documentos' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600'}`}
        >
          Documentos
        </button>
        {veTodo && (
          <>
            <button
              type="button"
              onClick={() => setTab('matriz')}
              className={`flex items-center gap-1.5 px-3 py-2 text-sm font-medium ${tab === 'matriz' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600'}`}
            >
              <Grid3x3 size={14} /> Matriz de cobertura
            </button>
            <button
              type="button"
              onClick={() => setTab('accesos')}
              className={`flex items-center gap-1.5 px-3 py-2 text-sm font-medium ${tab === 'accesos' ? 'border-b-2 border-gold-500 text-graphite-100' : 'text-graphite-600'}`}
            >
              <Lock size={14} /> Accesos por área
            </button>
          </>
        )}
      </div>

      {mostrarNuevo && tipos && (
        <NuevoDocumentoModal areasEscribibles={areasEscribibles} tipos={tipos} onClose={() => setMostrarNuevo(false)} />
      )}

      {seleccionado && areas && tipos && estados && (
        <GestionarDocumentoModal
          documento={seleccionado}
          areas={areas}
          areasEscribibles={areasEscribibles}
          tipos={tipos}
          estados={estados}
          puedeEditar={tieneEscritura(miAcceso, seleccionado.area)}
          onClose={() => setSeleccionado(null)}
        />
      )}

      {tab === 'documentos' && (
        <>
          <div className="mb-4 flex flex-wrap items-end gap-3">
            <label className="flex flex-col gap-1 text-xs">
              <span className="text-graphite-600">Área</span>
              <select value={filtroArea} onChange={(e) => setFiltroArea(e.target.value)} className={`${inputClase} py-1.5`}>
                <option value="">Todas (las que puedo ver)</option>
                {areasLegibles.map((a) => (
                  <option key={a.codigo} value={a.codigo}>{a.nombre}</option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-xs">
              <span className="text-graphite-600">Tipo</span>
              <select value={filtroTipo} onChange={(e) => setFiltroTipo(e.target.value)} className={`${inputClase} py-1.5`}>
                <option value="">Todos</option>
                {tipos?.map((t) => (
                  <option key={t.codigo} value={t.codigo}>{t.nombre}</option>
                ))}
              </select>
            </label>
            <label className="flex flex-col gap-1 text-xs">
              <span className="text-graphite-600">Estado</span>
              <select value={filtroEstado} onChange={(e) => setFiltroEstado(e.target.value)} className={`${inputClase} py-1.5`}>
                <option value="">Todos</option>
                {estados?.map((e) => (
                  <option key={e.codigo} value={e.codigo}>{e.nombre}</option>
                ))}
              </select>
            </label>
            <label className="flex flex-1 flex-col gap-1 text-xs">
              <span className="text-graphite-600">Buscar</span>
              <input type="text" value={busqueda} onChange={(e) => setBusqueda(e.target.value)} placeholder="Título o notas…" className={`${inputClase} py-1.5`} />
            </label>
            <BotonesExportar nombreArchivo="biblioteca-documentos" titulo="Biblioteca de Documentos" columnas={columnas} filas={documentos ?? []} />
          </div>

          <p className="mb-3 text-xs text-graphite-500">Doble clic en una fila para gestionar (metadatos, nueva versión, descargar, desactivar).</p>

          <TableContainer>
            <thead>
              <tr>
                <Th>Título</Th>
                <Th>Área</Th>
                <Th>Tipo</Th>
                <Th>Versión</Th>
                <Th>Estado</Th>
                <Th>Archivo</Th>
                <Th>Próxima revisión</Th>
                <Th>Subido por</Th>
              </tr>
            </thead>
            <tbody>
              {(isLoading || !miAcceso) && <EmptyState>Cargando…</EmptyState>}
              {!isLoading && miAcceso && (documentos?.length ?? 0) === 0 && (
                <EmptyState>
                  {areasLegibles.length === 0 && !veTodo
                    ? 'No tenés acceso a ningún área todavía — pedile a un administrador que te lo otorgue.'
                    : 'No hay documentos que coincidan con el filtro'}
                </EmptyState>
              )}
              {documentos?.map((d) => (
                <tr
                  key={d.id}
                  onDoubleClick={() => setSeleccionado(d)}
                  title="Doble clic para gestionar"
                  className="cursor-pointer border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]"
                >
                  <Td className="font-medium">{d.titulo}</Td>
                  <Td>{nombreCatalogo(areas, d.area)}</Td>
                  <Td>{nombreCatalogo(tipos, d.tipo)}</Td>
                  <Td>{d.version}</Td>
                  <Td>
                    <Badge variant={estadoVariant(d.estado)}>{nombreCatalogo(estados, d.estado)}</Badge>
                  </Td>
                  <Td className="max-w-[220px] truncate" title={d.nombreArchivoOriginal}>
                    {d.nombreArchivoOriginal} <span className="text-graphite-400">({formatoTamano(d.tamanoBytes)})</span>
                  </Td>
                  <Td>{d.proximaRevision ?? '—'}</Td>
                  <Td>{d.creadoPor}</Td>
                </tr>
              ))}
            </tbody>
          </TableContainer>
        </>
      )}

      {tab === 'matriz' && veTodo && areas && tipos && <MatrizCobertura areas={areas} tipos={tipos} />}
      {tab === 'accesos' && veTodo && areas && <SeccionAccesos areas={areas} />}
    </div>
  )
}
