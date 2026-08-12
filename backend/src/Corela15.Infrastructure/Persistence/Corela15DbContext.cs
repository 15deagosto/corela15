using Corela15.Domain.Clientes;
using Corela15.Domain.Contabilidad;
using Corela15.Domain.General;
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

    // CONTABILIDAD
    public DbSet<CuentaContable> CuentasContables => Set<CuentaContable>();
    public DbSet<TipoComprobanteContable> TiposComprobanteContable => Set<TipoComprobanteContable>();
    public DbSet<ComprobanteContable> ComprobantesContables => Set<ComprobanteContable>();
    public DbSet<MovimientoComprobanteContable> MovimientosComprobanteContable => Set<MovimientoComprobanteContable>();
    public DbSet<SaldoContable> SaldosContables => Set<SaldoContable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Corela15DbContext).Assembly);
    }
}
