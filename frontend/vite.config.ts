import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5174,
    strictPort: true,
    // host: true (= 0.0.0.0) — escucha en todas las interfaces de red, no
    // solo localhost, para que otros equipos de la misma red puedan
    // acceder por la IP LAN de esta máquina (ej. http://172.16.0.77:5174).
    // Sin esto, Vite solo acepta conexiones desde la propia máquina.
    host: true,
  },
})
