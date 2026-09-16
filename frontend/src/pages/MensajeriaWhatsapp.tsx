import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { MessageCircle, Send } from 'lucide-react'
import { PageHeader } from '../components/PageHeader'
import { TableContainer, Th, Td, EmptyState } from '../components/Table'
import { Badge } from '../components/Badge'
import { api } from '../lib/api'

interface PersonaConTelefonos {
  id: string
  nombre: string
  identificacion: string
  telefonos: string[]
}

interface MensajeWhatsapp {
  id: string
  numeroDestino: string
  idPersonaDestino: string | null
  nombrePersonaDestino: string | null
  texto: string
  estado: string
  idMensajeExterno: string | null
  detalleError: string | null
  agencia: string
  enviadoPor: string
  creadoEn: string
}

const MAX_CARACTERES = 1000

function estadoVariant(estado: string) {
  return estado === 'Enviado' ? ('exito' as const) : ('peligro' as const)
}

function detalleError(error: unknown, fallback: string) {
  return (error as { response?: { data?: { detail?: string } } })?.response?.data?.detail ?? fallback
}

function BuscarPersonaWhatsapp({ onSeleccionar }: { onSeleccionar: (p: PersonaConTelefonos) => void }) {
  const [q, setQ] = useState('')

  const { data: personas } = useQuery<PersonaConTelefonos[]>({
    queryKey: ['mensajeria-buscar-persona', q],
    queryFn: async () => (await api.get('/api/mensajeria/personas/buscar', { params: { q } })).data,
    enabled: q.length >= 2,
  })

  return (
    <div className="flex flex-col gap-1">
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar socio por nombre o identificación…"
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
              {p.telefonos.length === 0 && <span className="ml-1 text-graphite-500">(sin teléfono registrado)</span>}
            </button>
          ))}
        </div>
      )}
      {q.length >= 2 && (personas?.length ?? 0) === 0 && (
        <p className="px-1 text-xs text-graphite-600">Sin resultados.</p>
      )}
    </div>
  )
}

function EnviarWhatsappForm() {
  const queryClient = useQueryClient()
  const [persona, setPersona] = useState<PersonaConTelefonos | null>(null)
  const [numero, setNumero] = useState('')
  const [texto, setTexto] = useState('')

  const enviar = useMutation({
    mutationFn: async () =>
      api.post(
        '/api/mensajeria/whatsapp/enviar',
        { numeroDestino: numero, idPersonaDestino: persona?.id ?? null, texto },
        { headers: { 'Idempotency-Key': crypto.randomUUID() } },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['mensajeria-whatsapp-historial'] })
      setPersona(null)
      setNumero('')
      setTexto('')
    },
  })

  return (
    <div className="glass-card mb-6 rounded-xl p-5">
      <h3 className="mb-1 font-medium text-graphite-100">Enviar WhatsApp</h3>
      <p className="mb-4 text-xs text-graphite-500">
        Llega como un aviso de la Cooperativa 15 de Agosto de Pilacoto. Es de solo envío: si el socio responde, esa
        respuesta no aparece acá — usalo para avisos puntuales (confirmar un dato, pedir que se acerque, recordar un
        pago), no para sostener una conversación.
      </p>

      <form
        className="grid grid-cols-1 gap-4 sm:grid-cols-2"
        onSubmit={(e) => {
          e.preventDefault()
          if (!numero.trim() || !texto.trim()) return
          enviar.mutate()
        }}
      >
        <div className="flex flex-col gap-2">
          <span className="text-sm text-graphite-600">Buscar socio (opcional, autocompleta el teléfono)</span>
          <BuscarPersonaWhatsapp
            onSeleccionar={(p) => {
              setPersona(p)
              setNumero(p.telefonos[0] ?? '')
            }}
          />
          {persona && (
            <p className="rounded-lg bg-black/[0.02] px-3 py-1.5 text-xs text-graphite-600">
              Seleccionado: <span className="font-medium text-graphite-100">{persona.nombre}</span>{' '}
              <button
                type="button"
                onClick={() => setPersona(null)}
                className="ml-1 text-gold-500 hover:underline"
              >
                quitar
              </button>
            </p>
          )}
        </div>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-graphite-600">Número de WhatsApp</span>
          {persona && persona.telefonos.length > 1 ? (
            <select
              required
              value={numero}
              onChange={(e) => setNumero(e.target.value)}
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            >
              {persona.telefonos.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          ) : (
            <input
              required
              type="text"
              value={numero}
              onChange={(e) => setNumero(e.target.value)}
              placeholder="0987654321"
              className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
            />
          )}
        </label>

        <label className="flex flex-col gap-1 text-sm sm:col-span-2">
          <span className="text-graphite-600">Mensaje</span>
          <textarea
            required
            rows={3}
            maxLength={MAX_CARACTERES}
            value={texto}
            onChange={(e) => setTexto(e.target.value)}
            placeholder="Escriba el mensaje para el socio…"
            className="rounded-lg border border-black/[0.08] bg-white px-3 py-2 text-sm text-graphite-100 outline-none focus:border-gold-500/50"
          />
          <span className="self-end text-xs text-graphite-500">
            {texto.length}/{MAX_CARACTERES}
          </span>
        </label>

        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={enviar.isPending}
            className="btn-hover flex items-center gap-1.5 rounded-lg bg-gold-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-60"
          >
            <Send size={16} /> {enviar.isPending ? 'Enviando…' : 'Enviar WhatsApp'}
          </button>

          {enviar.isError && (
            <p className="mt-2 text-sm text-red-700">{detalleError(enviar.error, 'No se pudo enviar el mensaje.')}</p>
          )}
          {enviar.isSuccess && !enviar.isError && (
            <p className="mt-2 text-sm text-petrol-700">Mensaje enviado correctamente.</p>
          )}
        </div>
      </form>
    </div>
  )
}

export function MensajeriaWhatsapp() {
  const { data: historial, isLoading } = useQuery<MensajeWhatsapp[]>({
    queryKey: ['mensajeria-whatsapp-historial'],
    queryFn: async () => (await api.get('/api/mensajeria/whatsapp/historial', { params: { dias: 30 } })).data,
  })

  return (
    <div className="animate-fade-in">
      <PageHeader
        icon={MessageCircle}
        title="Mensajería (WhatsApp)"
        subtitle="Enviar un WhatsApp a un socio usando un número real de la cooperativa"
      />

      <EnviarWhatsappForm />

      <h3 className="mb-2 mt-6 text-sm font-medium text-graphite-600">Historial (últimos 30 días)</h3>
      <TableContainer>
        <thead>
          <tr>
            <Th>Destinatario</Th>
            <Th>Número</Th>
            <Th>Mensaje</Th>
            <Th>Estado</Th>
            <Th>Enviado por</Th>
            <Th>Fecha</Th>
          </tr>
        </thead>
        <tbody>
          {isLoading && <EmptyState>Cargando…</EmptyState>}
          {!isLoading && (historial?.length ?? 0) === 0 && <EmptyState>Todavía no se ha enviado ningún mensaje</EmptyState>}
          {historial?.map((m) => (
            <tr key={m.id} className="border-b border-black/[0.04] last:border-0 hover:bg-black/[0.015]">
              <Td className="font-medium">{m.nombrePersonaDestino ?? '—'}</Td>
              <Td>{m.numeroDestino}</Td>
              <Td className="max-w-xs truncate" title={m.texto}>
                {m.texto}
              </Td>
              <Td>
                <Badge variant={estadoVariant(m.estado)} title={m.detalleError ?? undefined}>
                  {m.estado}
                </Badge>
              </Td>
              <Td>{m.enviadoPor}</Td>
              <Td>{new Date(m.creadoEn).toLocaleString('es-EC')}</Td>
            </tr>
          ))}
        </tbody>
      </TableContainer>
    </div>
  )
}
