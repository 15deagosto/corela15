using Corela15.Domain.ActivoFijo;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Auditoria;
using Corela15.Domain.Cajas;
using Corela15.Domain.CallCenter;
using Corela15.Domain.Financiero;
using Corela15.Domain.Clientes;
using Corela15.Domain.Cobranza;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Contabilidad;
using Corela15.Domain.Cumplimiento;
using Corela15.Domain.Credito;
using Corela15.Domain.CuentasPorCobrar;
using Corela15.Domain.FlujoTrabajo;
using Corela15.Domain.General;
using Corela15.Domain.HerramientaRural;
using Corela15.Domain.Inversion;
using Corela15.Domain.LavadoActivos;
using Corela15.Domain.Marketing;
using Corela15.Domain.MesaServicio;
using Corela15.Domain.Planificacion;
using Corela15.Domain.Nomina;
using Corela15.Domain.Obligacion;
using Corela15.Domain.Portafolio;
using Corela15.Domain.Proveeduria;
using Corela15.Domain.ReporteControl;
using Corela15.Domain.Riesgo;
using Corela15.Domain.Seguridad;
using Corela15.Domain.Sujeto;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Persistence;

public class Corela15DbContext(DbContextOptions<Corela15DbContext> options) : DbContext(options)
{
    // GENERAL
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<TipoIdentificacion> TiposIdentificacion => Set<TipoIdentificacion>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Agencia> Agencias => Set<Agencia>();
    public DbSet<TipoActividad> TiposActividad => Set<TipoActividad>();
    public DbSet<ActividadEconomica> ActividadesEconomicas => Set<ActividadEconomica>();
    public DbSet<Banco> Bancos => Set<Banco>();

    // FINANCIERO
    public DbSet<Cheque> Cheques => Set<Cheque>();
    public DbSet<ChequeProtesto> ChequesProtesto => Set<ChequeProtesto>();

    // SUJETO
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Profesion> Profesiones => Set<Profesion>();
    public DbSet<CanalReclamo> CanalesReclamo => Set<CanalReclamo>();
    public DbSet<EstadoReclamo> EstadosReclamo => Set<EstadoReclamo>();
    public DbSet<TipoResolucionReclamo> TiposResolucionReclamo => Set<TipoResolucionReclamo>();
    public DbSet<TipoProductoReclamo> TiposProductoReclamo => Set<TipoProductoReclamo>();
    public DbSet<ConceptoReclamo> ConceptosReclamo => Set<ConceptoReclamo>();
    public DbSet<ConceptoReclamoDetalle> ConceptosReclamoDetalle => Set<ConceptoReclamoDetalle>();
    public DbSet<Reclamo> Reclamos => Set<Reclamo>();
    public DbSet<ReclamoRespuesta> ReclamosRespuesta => Set<ReclamoRespuesta>();
    public DbSet<MiembroOrganoGobierno> MiembrosOrganoGobierno => Set<MiembroOrganoGobierno>();
    public DbSet<PersonaNatural> PersonasNaturales => Set<PersonaNatural>();
    public DbSet<PersonaJuridica> PersonasJuridicas => Set<PersonaJuridica>();
    public DbSet<Conyuge> Conyuges => Set<Conyuge>();
    public DbSet<Representante> Representantes => Set<Representante>();
    public DbSet<PersonaTelefono> PersonasTelefonos => Set<PersonaTelefono>();
    public DbSet<EstadoCivil> EstadosCiviles => Set<EstadoCivil>();
    public DbSet<Educacion> Educaciones => Set<Educacion>();
    public DbSet<Vivienda> Viviendas => Set<Vivienda>();
    public DbSet<SectorVivienda> SectoresVivienda => Set<SectorVivienda>();
    public DbSet<Nacionalidad> Nacionalidades => Set<Nacionalidad>();

    // CLIENTES
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<CausaVinculacion> CausasVinculacion => Set<CausaVinculacion>();
    public DbSet<CalificacionInterna> CalificacionesInternas => Set<CalificacionInterna>();
    public DbSet<SectorEconomico> SectoresEconomicos => Set<SectorEconomico>();

    // SEGURIDAD
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<AccionIngresoUsuario> AccionesIngresoUsuario => Set<AccionIngresoUsuario>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<RolMenu> RolesMenu => Set<RolMenu>();
    public DbSet<TipoEstructura> TiposEstructura => Set<TipoEstructura>();
    public DbSet<RolTipoEstructura> RolesTipoEstructura => Set<RolTipoEstructura>();
    public DbSet<DatasetReporteria> DatasetsReporteria => Set<DatasetReporteria>();
    public DbSet<RolDatasetReporteria> RolesDatasetReporteria => Set<RolDatasetReporteria>();
    public DbSet<UsuarioMenu> UsuariosMenu => Set<UsuarioMenu>();
    public DbSet<UsuarioDatasetReporteria> UsuariosDatasetReporteria => Set<UsuarioDatasetReporteria>();
    public DbSet<Opcion> Opciones => Set<Opcion>();
    public DbSet<RolOpcion> RolesOpcion => Set<RolOpcion>();
    public DbSet<UsuarioOpcion> UsuariosOpcion => Set<UsuarioOpcion>();
    public DbSet<Corela15.Domain.Reporteria.Tablero> TablerosReporteria => Set<Corela15.Domain.Reporteria.Tablero>();
    public DbSet<Corela15.Domain.Reporteria.FavoritoTablero> FavoritosTablero => Set<Corela15.Domain.Reporteria.FavoritoTablero>();
    public DbSet<Corela15.Domain.Reporteria.AuditoriaConsultaReporteria> AuditoriaConsultaReporteria => Set<Corela15.Domain.Reporteria.AuditoriaConsultaReporteria>();
    public DbSet<SolicitudIdempotente> SolicitudesIdempotentes => Set<SolicitudIdempotente>();
    public DbSet<SesionUsuario> SesionesUsuario => Set<SesionUsuario>();
    public DbSet<AccionUsuario> AccionesUsuario => Set<AccionUsuario>();
    public DbSet<HorarioAccesoUsuario> HorariosAccesoUsuario => Set<HorarioAccesoUsuario>();
    public DbSet<UsuarioRolTemporal> UsuariosRolTemporal => Set<UsuarioRolTemporal>();
    public DbSet<UsuarioAgenciaTemporal> UsuariosAgenciaTemporal => Set<UsuarioAgenciaTemporal>();

    // CONTABILIDAD
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();
    public DbSet<TipoComprobanteContable> TiposComprobanteContable => Set<TipoComprobanteContable>();
    public DbSet<ComprobanteContable> ComprobantesContables => Set<ComprobanteContable>();
    public DbSet<MovimientoComprobanteContable> MovimientosComprobanteContable => Set<MovimientoComprobanteContable>();
    public DbSet<SaldoContable> SaldosContables => Set<SaldoContable>();
    public DbSet<PeriodoContable> PeriodosContables => Set<PeriodoContable>();
    public DbSet<CierreEjercicio> CierresEjercicio => Set<CierreEjercicio>();
    public DbSet<TipoTransaccion> TiposTransaccion => Set<TipoTransaccion>();
    public DbSet<TipoTransaccionCuentaProducto> TiposTransaccionCuentaProducto => Set<TipoTransaccionCuentaProducto>();
    public DbSet<FormaCancelacion> FormasCancelacion => Set<FormaCancelacion>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<TipoComprobanteCompra> TiposComprobanteCompra => Set<TipoComprobanteCompra>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<CompraDetalle> ComprasDetalle => Set<CompraDetalle>();

    // AHORROS
    public DbSet<TipoCuenta> TiposCuenta => Set<TipoCuenta>();
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<CuentaCliente> CuentasClientes => Set<CuentaCliente>();
    public DbSet<ItemSaldo> ItemsSaldo => Set<ItemSaldo>();
    public DbSet<TipoCuentaItemSaldo> TiposCuentaItemSaldo => Set<TipoCuentaItemSaldo>();
    public DbSet<CuentaItemSaldo> CuentasItemSaldo => Set<CuentaItemSaldo>();
    public DbSet<CuentaMovimiento> CuentasMovimientos => Set<CuentaMovimiento>();
    public DbSet<DevengoInteresLog> DevengosInteresLog => Set<DevengoInteresLog>();

    // INVERSION (Plazo Fijo)
    public DbSet<Deposito> Depositos => Set<Deposito>();
    public DbSet<DepositoCliente> DepositosClientes => Set<DepositoCliente>();
    public DbSet<DepositoRenovacion> DepositosRenovaciones => Set<DepositoRenovacion>();
    public DbSet<ItemPlazoTasa> ItemsPlazoTasa => Set<ItemPlazoTasa>();

    // CREDITO (originación)
    public DbSet<TipoPrestamo> TiposPrestamo => Set<TipoPrestamo>();
    public DbSet<TipoSeguro> TiposSeguro => Set<TipoSeguro>();
    public DbSet<TipoConvenio> TiposConvenio => Set<TipoConvenio>();
    public DbSet<SolicitudPrestamo> SolicitudesPrestamo => Set<SolicitudPrestamo>();
    public DbSet<ScoreCrediticio> ScoresCrediticios => Set<ScoreCrediticio>();
    public DbSet<TasaTechoBce> TasasTechoBce => Set<TasaTechoBce>();
    public DbSet<EstadoGarantia> EstadosGarantia => Set<EstadoGarantia>();
    public DbSet<SolicitudPrestamoGarantia> SolicitudesPrestamoGarantia => Set<SolicitudPrestamoGarantia>();
    public DbSet<SolicitudPrestamoEtapaHist> SolicitudesPrestamoEtapaHist => Set<SolicitudPrestamoEtapaHist>();

    // FLUJOTRABAJO (motor de aprobaciones genérico)
    public DbSet<TipoEtapa> TiposEtapa => Set<TipoEtapa>();
    public DbSet<Etapa> Etapas => Set<Etapa>();
    public DbSet<EtapaRetorno> EtapasRetorno => Set<EtapaRetorno>();
    public DbSet<GrupoContable> GruposContables => Set<GrupoContable>();
    public DbSet<GrupoContableUsuario> GruposContablesUsuarios => Set<GrupoContableUsuario>();
    public DbSet<EtapaGrupoContable> EtapasGrupoContable => Set<EtapaGrupoContable>();

    // COLOCACION (préstamo vivo)
    public DbSet<TipoRubro> TiposRubro => Set<TipoRubro>();
    public DbSet<Rubro> Rubros => Set<Rubro>();
    public DbSet<TipoVencimiento> TiposVencimiento => Set<TipoVencimiento>();
    public DbSet<ClasificacionCartera> ClasificacionesCartera => Set<ClasificacionCartera>();
    public DbSet<CategoriaRiesgoCartera> CategoriasRiesgoCartera => Set<CategoriaRiesgoCartera>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<PrestamoCliente> PrestamosClientes => Set<PrestamoCliente>();
    public DbSet<PrestamoRubro> PrestamosRubros => Set<PrestamoRubro>();
    public DbSet<PrestamoRubroCuentaPorCobrar> PrestamosRubrosCuentasPorCobrar => Set<PrestamoRubroCuentaPorCobrar>();
    public DbSet<AutoDebitoSpiLog> AutoDebitosSpiLog => Set<AutoDebitoSpiLog>();
    public DbSet<DiferimientoCuota> DiferimientosCuota => Set<DiferimientoCuota>();
    public DbSet<PrestamoCastigado> PrestamosCastigados => Set<PrestamoCastigado>();
    public DbSet<EstadoCustodioPagare> EstadosCustodioPagare => Set<EstadoCustodioPagare>();
    public DbSet<PagareCustodia> PagaresCustodia => Set<PagareCustodia>();
    public DbSet<PagareCustodiaMovimiento> PagaresCustodiaMovimiento => Set<PagareCustodiaMovimiento>();
    public DbSet<PrestamoGarantia> PrestamosGarantias => Set<PrestamoGarantia>();

    // COBRANZA
    public DbSet<PeriodoMora> PeriodosMora => Set<PeriodoMora>();
    public DbSet<AccionGestion> AccionesGestion => Set<AccionGestion>();
    public DbSet<GestionPrestamoCobranza> GestionesPrestamoCobranza => Set<GestionPrestamoCobranza>();

    // CUMPLIMIENTO
    public DbSet<EstadoHallazgo> EstadosHallazgo => Set<EstadoHallazgo>();
    public DbSet<Hallazgo> Hallazgos => Set<Hallazgo>();
    public DbSet<HallazgoUsuario> HallazgosUsuario => Set<HallazgoUsuario>();
    public DbSet<HallazgoUsuarioRespuesta> HallazgosUsuarioRespuesta => Set<HallazgoUsuarioRespuesta>();
    public DbSet<HallazgoEtapa> HallazgosEtapa => Set<HallazgoEtapa>();

    // LAVADOACTIVOS
    public DbSet<CalificacionCliente> CalificacionesCliente => Set<CalificacionCliente>();
    public DbSet<TipoListaControl> TiposListaControl => Set<TipoListaControl>();
    public DbSet<AlertaListaControl> AlertasListaControl => Set<AlertaListaControl>();
    public DbSet<RangoPatrimonioLavado> RangosPatrimonioLavado => Set<RangoPatrimonioLavado>();
    public DbSet<RangoIngresoLavado> RangosIngresoLavado => Set<RangoIngresoLavado>();

    // CAJAS
    public DbSet<Denominacion> Denominaciones => Set<Denominacion>();
    public DbSet<Ventanilla> Ventanillas => Set<Ventanilla>();
    public DbSet<VentanillaCuadre> VentanillasCuadre => Set<VentanillaCuadre>();
    public DbSet<ItemCaja> ItemsCaja => Set<ItemCaja>();
    public DbSet<VentanillaItemCaja> VentanillasItemCaja => Set<VentanillaItemCaja>();
    public DbSet<VentanillaItemCajaMovimiento> VentanillasItemCajaMovimiento => Set<VentanillaItemCajaMovimiento>();
    public DbSet<AutorizacionTransaccion> AutorizacionesTransaccion => Set<AutorizacionTransaccion>();
    public DbSet<ItemBoveda> ItemsBoveda => Set<ItemBoveda>();
    public DbSet<Boveda> Bovedas => Set<Boveda>();
    public DbSet<BovedaItemBoveda> BovedasItemBoveda => Set<BovedaItemBoveda>();
    public DbSet<PagoExternoProducto> PagoExternoProductos => Set<PagoExternoProducto>();
    public DbSet<PagoExternoTransaccion> PagoExternoTransacciones => Set<PagoExternoTransaccion>();
    public DbSet<TipoFormaNumerada> TiposFormaNumerada => Set<TipoFormaNumerada>();
    public DbSet<FormaNumerada> FormasNumeradas => Set<FormaNumerada>();

    // NOMINA
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<RolPagos> RolesPagos => Set<RolPagos>();
    public DbSet<RolPagosEmpleado> RolesPagosEmpleado => Set<RolPagosEmpleado>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<ParametroNomina> ParametrosNomina => Set<ParametroNomina>();
    public DbSet<EmpleadoDecimoTercero> EmpleadosDecimoTercero => Set<EmpleadoDecimoTercero>();
    public DbSet<EmpleadoDecimoCuarto> EmpleadosDecimoCuarto => Set<EmpleadoDecimoCuarto>();
    public DbSet<EmpleadoFondosReserva> EmpleadosFondosReserva => Set<EmpleadoFondosReserva>();
    public DbSet<EmpleadoProvisionVacacion> EmpleadosProvisionVacacion => Set<EmpleadoProvisionVacacion>();
    public DbSet<EmpleadoProvisionVacacionDetalle> EmpleadosProvisionVacacionDetalle => Set<EmpleadoProvisionVacacionDetalle>();
    public DbSet<EstadoAccionPersonal> EstadosAccionPersonal => Set<EstadoAccionPersonal>();
    public DbSet<TipoAccionPersonal> TiposAccionPersonal => Set<TipoAccionPersonal>();
    public DbSet<SolicitudAccionPersonal> SolicitudesAccionPersonal => Set<SolicitudAccionPersonal>();
    public DbSet<SolicitudAccionPersonalEtapa> SolicitudAccionPersonalEtapas => Set<SolicitudAccionPersonalEtapa>();
    public DbSet<TipoContrato> TiposContrato => Set<TipoContrato>();
    public DbSet<EmpleadoContrato> EmpleadosContrato => Set<EmpleadoContrato>();
    public DbSet<EmpleadoDatosAdicionales> EmpleadosDatosAdicionales => Set<EmpleadoDatosAdicionales>();
    public DbSet<EmpleadoAportePatronal> EmpleadosAportePatronal => Set<EmpleadoAportePatronal>();
    public DbSet<TramoImpuestoRenta> TramosImpuestoRenta => Set<TramoImpuestoRenta>();
    public DbSet<CalculoImpuestoRenta> CalculosImpuestoRenta => Set<CalculoImpuestoRenta>();

    // OBLIGACION / ACTIVOFIJO / CUENTASPORCOBRAR / PROVEEDURIA / PORTAFOLIO (Nivel 7)
    public DbSet<ObligacionFinanciera> ObligacionesFinancieras => Set<ObligacionFinanciera>();
    public DbSet<EstadoObligacionFinanciera> EstadosObligacionFinanciera => Set<EstadoObligacionFinanciera>();
    public DbSet<PeriodicidadPago> PeriodicidadesPago => Set<PeriodicidadPago>();
    public DbSet<ClaseObligacionFinanciera> ClasesObligacionFinanciera => Set<ClaseObligacionFinanciera>();
    public DbSet<FormaCancelacionObligacion> FormasCancelacionObligacion => Set<FormaCancelacionObligacion>();
    public DbSet<Estructura> EstructurasActivoFijo => Set<Estructura>();
    public DbSet<Responsable> ResponsablesActivoFijo => Set<Responsable>();
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<ActivoResponsable> ActivosResponsables => Set<ActivoResponsable>();
    public DbSet<DepreciacionAgencia> DepreciacionesAgencia => Set<DepreciacionAgencia>();
    public DbSet<DepreciacionAgenciaDetalle> DepreciacionesAgenciaDetalle => Set<DepreciacionAgenciaDetalle>();
    public DbSet<MotivoTrasladoActivo> MotivosTrasladoActivo => Set<MotivoTrasladoActivo>();
    public DbSet<TrasladoActivo> TrasladosActivo => Set<TrasladoActivo>();
    public DbSet<MotivoBaja> MotivosBajaActivo => Set<MotivoBaja>();
    public DbSet<SolicitudActivoBaja> SolicitudesActivoBaja => Set<SolicitudActivoBaja>();
    public DbSet<CuentaPorCobrar> CuentasPorCobrar => Set<CuentaPorCobrar>();
    public DbSet<CuentaPorPagar> CuentasPorPagar => Set<CuentaPorPagar>();
    public DbSet<TipoArticulo> TiposArticulo => Set<TipoArticulo>();
    public DbSet<Articulo> Articulos => Set<Articulo>();
    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<BodegaArticulo> BodegasArticulo => Set<BodegaArticulo>();
    public DbSet<BodegaArticuloMovimiento> BodegasArticuloMovimiento => Set<BodegaArticuloMovimiento>();
    public DbSet<SolicitudPedido> SolicitudesPedido => Set<SolicitudPedido>();
    public DbSet<SolicitudPedidoArticulo> SolicitudesPedidoArticulo => Set<SolicitudPedidoArticulo>();
    public DbSet<SolicitudPedidoEtapa> SolicitudesPedidoEtapa => Set<SolicitudPedidoEtapa>();
    public DbSet<TipoInstitucion> TiposInstitucion => Set<TipoInstitucion>();
    public DbSet<Institucion> Instituciones => Set<Institucion>();
    public DbSet<InversionPortafolio> InversionesPortafolio => Set<InversionPortafolio>();
    public DbSet<InversionRenovacion> InversionesRenovacion => Set<InversionRenovacion>();

    // RIESGOOPERATIVO / REPORTECONTROL (Nivel 8)
    public DbSet<MacroProceso> MacroProcesos => Set<MacroProceso>();
    public DbSet<Proceso> Procesos => Set<Proceso>();
    public DbSet<NivelImpacto> NivelesImpacto => Set<NivelImpacto>();
    public DbSet<NivelProbabilidad> NivelesProbabilidad => Set<NivelProbabilidad>();
    public DbSet<NivelRiesgo> NivelesRiesgo => Set<NivelRiesgo>();
    public DbSet<EventoRiesgo> EventosRiesgo => Set<EventoRiesgo>();
    public DbSet<CategoriaIncidencia> CategoriasIncidencia => Set<CategoriaIncidencia>();
    public DbSet<PrioridadTicket> PrioridadesTicket => Set<PrioridadTicket>();
    public DbSet<EstadoTicket> EstadosTicket => Set<EstadoTicket>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComentario> TicketComentarios => Set<TicketComentario>();
    public DbSet<TicketEtapaHist> TicketEtapaHist => Set<TicketEtapaHist>();
    public DbSet<AreaPlanificacion> AreasPlanificacion => Set<AreaPlanificacion>();
    public DbSet<EtiquetaPlanificacion> EtiquetasPlanificacion => Set<EtiquetaPlanificacion>();
    public DbSet<PlanSemanal> PlanesSemanales => Set<PlanSemanal>();
    public DbSet<PlanSemanalBloque> PlanesSemanalesBloques => Set<PlanSemanalBloque>();
    public DbSet<EstadoAvanceRiesgo> EstadosAvanceRiesgo => Set<EstadoAvanceRiesgo>();
    public DbSet<AvanceRiesgo> AvancesRiesgo => Set<AvanceRiesgo>();
    public DbSet<AvanceRiesgoDetalle> AvancesRiesgoDetalle => Set<AvanceRiesgoDetalle>();
    public DbSet<AvanceRiesgoEtapa> AvancesRiesgoEtapa => Set<AvanceRiesgoEtapa>();
    public DbSet<IndicadorLiquidez> IndicadoresLiquidez => Set<IndicadorLiquidez>();
    public DbSet<ParametroLiquidez> ParametrosLiquidez => Set<ParametroLiquidez>();
    public DbSet<ReporteRegulatorio> ReportesRegulatorios => Set<ReporteRegulatorio>();
    public DbSet<TarifaServicioFinanciero> TarifasServicioFinanciero => Set<TarifaServicioFinanciero>();
    public DbSet<TarifaGastoCobranza> TarifasGastoCobranza => Set<TarifaGastoCobranza>();

    // Periféricos
    public DbSet<AreaAuditoria> AreasAuditoria => Set<AreaAuditoria>();
    public DbSet<Seguimiento> Seguimientos => Set<Seguimiento>();
    public DbSet<TipoComentario> TiposComentario => Set<TipoComentario>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Rifa> Rifas => Set<Rifa>();
    public DbSet<RifaPremio> RifasPremios => Set<RifaPremio>();
    public DbSet<Indicador> Indicadores => Set<Indicador>();
    public DbSet<PlanificacionAnual> PlanificacionesAnuales => Set<PlanificacionAnual>();
    public DbSet<TipoProductoAgrario> TiposProductoAgrario => Set<TipoProductoAgrario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Corela15DbContext).Assembly);
    }

}
