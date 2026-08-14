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
]
