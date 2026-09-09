import type { ComponentType } from 'react'
import { Home } from '../pages/Home'
import { Socios } from '../pages/Socios'
import { UsuariosRoles } from '../pages/UsuariosRoles'
import { Contabilidad } from '../pages/Contabilidad'
import { Ahorros } from '../pages/Ahorros'
import { Creditos } from '../pages/Creditos'
import { CobranzasCumplimiento } from '../pages/CobranzasCumplimiento'
import { Cajas } from '../pages/Cajas'
import { Nomina } from '../pages/Nomina'
import { Tesoreria } from '../pages/Tesoreria'
import { Riesgo } from '../pages/Riesgo'
import { Configuracion } from '../pages/Configuracion'
import { ActivoFijo } from '../pages/ActivoFijo'
import { Portafolio } from '../pages/Portafolio'
import { Financiero } from '../pages/Financiero'
import { Proveeduria } from '../pages/Proveeduria'
import { EstructurasFinancieras } from '../pages/EstructurasFinancieras'
import { MesaServicio } from '../pages/MesaServicio'
import { Planificacion } from '../pages/Planificacion'
import { Reporteria } from '../pages/Reporteria'

/** Componente real por slug de pestaña — única fuente de verdad que usa `TabsWorkspace` para montar cada módulo abierto. */
export const componentesPorSlug: Record<string, ComponentType> = {
  inicio: Home,
  socios: Socios,
  'usuarios-roles': UsuariosRoles,
  contabilidad: Contabilidad,
  ahorros: Ahorros,
  creditos: Creditos,
  'cobranzas-cumplimiento': CobranzasCumplimiento,
  cajas: Cajas,
  nomina: Nomina,
  tesoreria: Tesoreria,
  riesgo: Riesgo,
  configuracion: Configuracion,
  activofijo: ActivoFijo,
  portafolio: Portafolio,
  financiero: Financiero,
  proveeduria: Proveeduria,
  'estructuras-financieras': EstructurasFinancieras,
  'mesa-servicio': MesaServicio,
  planificacion: Planificacion,
  'reporteria-gerencial': Reporteria,
}
