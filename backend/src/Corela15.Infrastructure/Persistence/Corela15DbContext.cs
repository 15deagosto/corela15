using Corela15.Domain.ActivoFijo;
using Corela15.Domain.Ahorros;
using Corela15.Domain.Auditoria;
using Corela15.Domain.Cajas;
using Corela15.Domain.CallCenter;
using Corela15.Domain.Clientes;
using Corela15.Domain.Cobranza;
using Corela15.Domain.Colocacion;
using Corela15.Domain.Contabilidad;
using Corela15.Domain.Credito;
using Corela15.Domain.CuentasPorCobrar;
using Corela15.Domain.General;
using Corela15.Domain.HerramientaRural;
using Corela15.Domain.Inversion;
using Corela15.Domain.LavadoActivos;
using Corela15.Domain.Marketing;
using Corela15.Domain.Nomina;
using Corela15.Domain.Obligacion;
using Corela15.Domain.Planificacion;
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
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<TipoIdentificacion> TiposIdentificacion => Set<TipoIdentificacion>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Agencia> Agencias => Set<Agencia>();

    // SUJETO
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<PersonaNatural> PersonasNaturales => Set<PersonaNatural>();
    public DbSet<PersonaJuridica> PersonasJuridicas => Set<PersonaJuridica>();

    // CLIENTES
    public DbSet<Cliente> Clientes => Set<Cliente>();

    // SEGURIDAD
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<AccionIngresoUsuario> AccionesIngresoUsuario => Set<AccionIngresoUsuario>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<RolMenu> RolesMenu => Set<RolMenu>();
    public DbSet<SolicitudIdempotente> SolicitudesIdempotentes => Set<SolicitudIdempotente>();

    // CONTABILIDAD
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();
    public DbSet<TipoComprobanteContable> TiposComprobanteContable => Set<TipoComprobanteContable>();
    public DbSet<ComprobanteContable> ComprobantesContables => Set<ComprobanteContable>();
    public DbSet<MovimientoComprobanteContable> MovimientosComprobanteContable => Set<MovimientoComprobanteContable>();
    public DbSet<SaldoContable> SaldosContables => Set<SaldoContable>();
    public DbSet<TipoTransaccion> TiposTransaccion => Set<TipoTransaccion>();

    // AHORROS
    public DbSet<TipoCuenta> TiposCuenta => Set<TipoCuenta>();
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<CuentaCliente> CuentasClientes => Set<CuentaCliente>();
    public DbSet<ItemSaldo> ItemsSaldo => Set<ItemSaldo>();
    public DbSet<TipoCuentaItemSaldo> TiposCuentaItemSaldo => Set<TipoCuentaItemSaldo>();
    public DbSet<CuentaItemSaldo> CuentasItemSaldo => Set<CuentaItemSaldo>();
    public DbSet<CuentaMovimiento> CuentasMovimientos => Set<CuentaMovimiento>();

    // INVERSION (Plazo Fijo)
    public DbSet<Deposito> Depositos => Set<Deposito>();
    public DbSet<DepositoCliente> DepositosClientes => Set<DepositoCliente>();
    public DbSet<DepositoRenovacion> DepositosRenovaciones => Set<DepositoRenovacion>();
    public DbSet<ItemPlazoTasa> ItemsPlazoTasa => Set<ItemPlazoTasa>();

    // CREDITO (originación)
    public DbSet<TipoPrestamo> TiposPrestamo => Set<TipoPrestamo>();
    public DbSet<SolicitudPrestamo> SolicitudesPrestamo => Set<SolicitudPrestamo>();

    // COLOCACION (préstamo vivo)
    public DbSet<Rubro> Rubros => Set<Rubro>();
    public DbSet<TipoVencimiento> TiposVencimiento => Set<TipoVencimiento>();
    public DbSet<ClasificacionCartera> ClasificacionesCartera => Set<ClasificacionCartera>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<PrestamoCliente> PrestamosClientes => Set<PrestamoCliente>();
    public DbSet<PrestamoRubro> PrestamosRubros => Set<PrestamoRubro>();

    // COBRANZA
    public DbSet<PeriodoMora> PeriodosMora => Set<PeriodoMora>();
    public DbSet<AccionGestion> AccionesGestion => Set<AccionGestion>();
    public DbSet<GestionPrestamoCobranza> GestionesPrestamoCobranza => Set<GestionPrestamoCobranza>();

    // LAVADOACTIVOS
    public DbSet<CalificacionCliente> CalificacionesCliente => Set<CalificacionCliente>();

    // CAJAS
    public DbSet<Denominacion> Denominaciones => Set<Denominacion>();
    public DbSet<Ventanilla> Ventanillas => Set<Ventanilla>();
    public DbSet<VentanillaCuadre> VentanillasCuadre => Set<VentanillaCuadre>();

    // NOMINA
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<RolPagos> RolesPagos => Set<RolPagos>();
    public DbSet<RolPagosEmpleado> RolesPagosEmpleado => Set<RolPagosEmpleado>();

    // OBLIGACION / ACTIVOFIJO / CUENTASPORCOBRAR / PROVEEDURIA / PORTAFOLIO (Nivel 7)
    public DbSet<ObligacionFinanciera> ObligacionesFinancieras => Set<ObligacionFinanciera>();
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<CuentaPorCobrar> CuentasPorCobrar => Set<CuentaPorCobrar>();
    public DbSet<CuentaPorPagar> CuentasPorPagar => Set<CuentaPorPagar>();
    public DbSet<Articulo> Articulos => Set<Articulo>();
    public DbSet<InversionPortafolio> InversionesPortafolio => Set<InversionPortafolio>();

    // RIESGOOPERATIVO / REPORTECONTROL (Nivel 8)
    public DbSet<MacroProceso> MacroProcesos => Set<MacroProceso>();
    public DbSet<Proceso> Procesos => Set<Proceso>();
    public DbSet<NivelImpacto> NivelesImpacto => Set<NivelImpacto>();
    public DbSet<NivelProbabilidad> NivelesProbabilidad => Set<NivelProbabilidad>();
    public DbSet<NivelRiesgo> NivelesRiesgo => Set<NivelRiesgo>();
    public DbSet<EventoRiesgo> EventosRiesgo => Set<EventoRiesgo>();
    public DbSet<ReporteRegulatorio> ReportesRegulatorios => Set<ReporteRegulatorio>();

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
