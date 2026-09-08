import {
  Users,
  ShieldCheck,
  Calculator,
  PiggyBank,
  Landmark,
  ShieldAlert,
  Wallet,
  Briefcase,
  Building2,
  AlertTriangle,
  Settings,
  Package,
  LineChart,
  FileCheck,
  Boxes,
  LifeBuoy,
  CalendarRange,
  KeyRound,
  type LucideIcon,
} from 'lucide-react'

export type EstadoModulo = 'disponible' | 'en-construccion' | 'proximamente'

export interface Modulo {
  slug: string
  path: string
  nombre: string
  descripcion: string
  icon: LucideIcon
  estado: EstadoModulo
  /**
   * App real aparte (propio backend/frontend/base, propio despliegue) --
   * no un módulo nativo de este core. El ícono abre esta URL en una
   * pestaña nueva del navegador en vez de montarse dentro del workspace
   * de pestañas internas (Sidebar.tsx/Home.tsx la manejan aparte). El
   * permiso `slug` sigue controlando quién ve el ícono; la autenticación
   * real de adentro es propia de esa app, separada de Corela15.
   */
  externalUrl?: string
}

// Nombres pensados para quien usa el sistema (cajero, asesor, admin) — no
// exponer jerga interna de "Nivel 0/1/2..." acá, eso vive solo en CLAUDE.md
// y en los .md de arquitectura para referencia del equipo de desarrollo.
export const modulos: Modulo[] = [
  {
    slug: 'socios',
    path: '/socios',
    nombre: 'Socios',
    descripcion: 'Personas, identificación y vínculo con la cooperativa',
    icon: Users,
    estado: 'disponible',
  },
  {
    slug: 'usuarios-roles',
    path: '/usuarios-roles',
    nombre: 'Usuarios y roles',
    descripcion: 'Accesos, permisos y seguridad del sistema',
    icon: ShieldCheck,
    estado: 'disponible',
  },
  {
    slug: 'contabilidad',
    path: '/contabilidad',
    nombre: 'Contabilidad',
    descripcion: 'Plan de cuentas, asientos y saldos',
    icon: Calculator,
    estado: 'en-construccion',
  },
  {
    slug: 'ahorros',
    path: '/ahorros',
    nombre: 'Ahorros',
    descripcion: 'Cuentas de ahorro y captación a la vista',
    icon: PiggyBank,
    estado: 'en-construccion',
  },
  {
    slug: 'creditos',
    path: '/creditos',
    nombre: 'Créditos y Plazo Fijo',
    descripcion: 'Solicitudes, préstamos y certificados de depósito',
    icon: Landmark,
    estado: 'en-construccion',
  },
  {
    slug: 'cobranzas-cumplimiento',
    path: '/cobranzas-cumplimiento',
    nombre: 'Cobranzas y Cumplimiento',
    descripcion: 'Gestión de mora y prevención de lavado de activos',
    icon: ShieldAlert,
    estado: 'en-construccion',
  },
  {
    slug: 'cajas',
    path: '/cajas',
    nombre: 'Cajas',
    descripcion: 'Apertura, transacciones y cuadre diario de ventanillas',
    icon: Wallet,
    estado: 'en-construccion',
  },
  {
    slug: 'nomina',
    path: '/nomina',
    nombre: 'Nómina',
    descripcion: 'Empleados y roles de pago',
    icon: Briefcase,
    estado: 'en-construccion',
  },
  {
    slug: 'tesoreria',
    path: '/tesoreria',
    nombre: 'Tesorería',
    descripcion: 'Cuentas por cobrar internas de la cooperativa',
    icon: Building2,
    estado: 'en-construccion',
  },
  {
    slug: 'riesgo',
    path: '/riesgo',
    nombre: 'Riesgo',
    descripcion: 'Registro de eventos de riesgo (matriz impacto × probabilidad)',
    icon: AlertTriangle,
    estado: 'en-construccion',
  },
  {
    slug: 'configuracion',
    path: '/configuracion',
    nombre: 'Configuración',
    descripcion: 'Parámetros y catálogos generales del sistema',
    icon: Settings,
    estado: 'en-construccion',
  },
  {
    slug: 'activofijo',
    path: '/activofijo',
    nombre: 'Activo Fijo',
    descripcion: 'Bienes, depreciación, traslados y bajas',
    icon: Package,
    estado: 'disponible',
  },
  {
    slug: 'portafolio',
    path: '/portafolio',
    nombre: 'Portafolio',
    descripcion: 'Inversiones propias en otras instituciones financieras',
    icon: LineChart,
    estado: 'disponible',
  },
  {
    slug: 'financiero',
    path: '/financiero',
    nombre: 'Financiero',
    descripcion: 'Cheques de terceros recibidos en depósito',
    icon: FileCheck,
    estado: 'disponible',
  },
  {
    slug: 'proveeduria',
    path: '/proveeduria',
    nombre: 'Proveeduría',
    descripcion: 'Bodega de suministros — stock, pedidos y consumo',
    icon: Boxes,
    estado: 'disponible',
  },
  {
    slug: 'estructuras-financieras',
    path: '/estructuras-financieras',
    nombre: 'Estructuras y Procesos Financieros',
    descripcion: 'Generador de estructuras regulatorias (OF01 y las que se sumen)',
    icon: FileCheck,
    estado: 'disponible',
  },
  {
    slug: 'mesa-servicio',
    path: '/mesa-servicio',
    nombre: 'Mesa de Servicio',
    descripcion: 'Control de incidencias — disponible para todos los usuarios',
    icon: LifeBuoy,
    estado: 'disponible',
  },
  {
    slug: 'planificacion',
    path: '/planificacion',
    nombre: 'Planificación',
    descripcion: 'Planificación semanal de actividades por área',
    icon: CalendarRange,
    estado: 'disponible',
  },
  {
    slug: 'credvault',
    path: '/credvault',
    nombre: 'CredVault',
    descripcion: 'Bóveda de credenciales de TI — se abre en una pestaña aparte',
    icon: KeyRound,
    estado: 'disponible',
    externalUrl: 'https://credvault.cooperativa15deagosto.fin.ec',
  },
]
