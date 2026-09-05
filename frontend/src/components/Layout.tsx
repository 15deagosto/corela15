import { useState } from 'react'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { TabsBar } from './TabsBar'
import { TabsWorkspace } from './TabsWorkspace'
import { TabsProvider } from '../lib/TabsContext'

export function Layout() {
  const [sidebarAbierto, setSidebarAbierto] = useState(false)

  return (
    <TabsProvider>
      <div className="flex h-svh">
        <Sidebar open={sidebarAbierto} onClose={() => setSidebarAbierto(false)} />
        <div className="flex min-w-0 flex-1 flex-col">
          <TopBar onOpenSidebar={() => setSidebarAbierto(true)} />
          <TabsBar />
          <main className="flex-1 overflow-y-auto overflow-x-hidden px-4 py-6 sm:px-6 sm:py-8 lg:px-8">
            {/* Antes fijo en max-w-4xl (896px) sin importar el tamaño real de
                pantalla -- en un monitor ancho dejaba muchísimo espacio vacío
                a los lados en cualquier módulo (tablas, reportes, formularios).
                max-w-screen-2xl (1536px) deja usar el ancho real disponible en
                monitores normales/grandes, sin estirarse al infinito en
                pantallas ultra anchas; en pantallas chicas el max-width simplemente
                no llega a aplicar, se sigue comprimiendo con normalidad. */}
            <div className="mx-auto w-full max-w-screen-2xl">
              <TabsWorkspace />
            </div>
          </main>
        </div>
      </div>
    </TabsProvider>
  )
}
