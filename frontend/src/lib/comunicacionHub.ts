import * as signalR from '@microsoft/signalr'

const STORAGE_KEY = 'corela15:sesion'

function leerToken(): string | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) return null
  try {
    return (JSON.parse(raw) as { token?: string }).token ?? null
  } catch {
    return null
  }
}

/**
 * Conexión real en tiempo real de Comunicación interna (WebSocket propio
 * del core, ver Corela15.Api/Hubs/ComunicacionHub.cs) — un solo objeto de
 * conexión por sesión de pestaña (Singleton simple, evita abrir un
 * WebSocket nuevo cada vez que se monta la pantalla de chat, ej. al
 * cambiar de pestaña interna y volver). `accessTokenFactory` es lo que le
 * permite al cliente de SignalR resolver el JWT real en cada intento de
 * conexión/reconexión, sin volver a pedir login.
 */
let conexion: signalR.HubConnection | null = null

export function obtenerConexionComunicacion(): signalR.HubConnection {
  if (conexion) return conexion

  const baseUrl = import.meta.env.VITE_API_URL ?? `http://${window.location.hostname}:5080`

  conexion = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/comunicacion`, {
      accessTokenFactory: () => leerToken() ?? '',
    })
    .withAutomaticReconnect()
    .build()

  return conexion
}

export async function iniciarConexionComunicacion(): Promise<signalR.HubConnection> {
  const hub = obtenerConexionComunicacion()
  if (hub.state === signalR.HubConnectionState.Disconnected) {
    await hub.start()
  }
  return hub
}
