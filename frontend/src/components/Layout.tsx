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
            <div className="mx-auto max-w-4xl">
              <TabsWorkspace />
            </div>
          </main>
        </div>
      </div>
    </TabsProvider>
  )
}
