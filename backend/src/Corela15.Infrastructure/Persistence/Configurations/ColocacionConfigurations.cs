using Corela15.Domain.Colocacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class RubroConfiguration : IEntityTypeConfiguration<Rubro>
{
    public void Configure(EntityTypeBuilder<Rubro> b)
    {
        b.ToTable("rubro", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TipoVencimientoConfiguration : IEntityTypeConfiguration<TipoVencimiento>
{
    public void Configure(EntityTypeBuilder<TipoVencimiento> b)
    {
        b.ToTable("tipo_vencimiento", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ClasificacionCarteraConfiguration : IEntityTypeConfiguration<ClasificacionCartera>
{
    public void Configure(EntityTypeBuilder<ClasificacionCartera> b)
    {
        b.ToTable("clasificacion_cartera", "colocacion", t => t.HasCheckConstraint(
            "ck_clasificacion_cartera_rango", "dias_fin >= dias_inicio"));
        b.HasKey(x => x.Id);

        b.HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.IdCuentaContable)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoVencimiento).WithMany().HasForeignKey(x => x.IdTipoVencimiento)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrestamoConfiguration : IEntityTypeConfiguration<Prestamo>
{
    public void Configure(EntityTypeBuilder<Prestamo> b)
    {
        b.ToTable("prestamo", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        b.Property(x => x.DeudaInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Tasa).HasColumnType("numeric(9,4)");
        b.Property(x => x.Tea).HasColumnType("numeric(9,4)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.TipoPrestamo).WithMany().HasForeignKey(x => x.IdTipoPrestamo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Numero).IsUnique();
    }
}

public class PrestamoClienteConfiguration : IEntityTypeConfiguration<PrestamoCliente>
{
    public void Configure(EntityTypeBuilder<PrestamoCliente> b)
    {
        b.ToTable("prestamo_cliente", "colocacion");
        b.HasKey(x => new { x.IdPrestamo, x.IdCliente });

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrestamoRubroConfiguration : IEntityTypeConfiguration<PrestamoRubro>
{
    public void Configure(EntityTypeBuilder<PrestamoRubro> b)
    {
        b.ToTable("prestamo_rubro", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Proyectado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Calculado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Cobrado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Rubro).WithMany().HasForeignKey(x => x.IdRubro).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdPrestamo, x.NumeroCuota, x.IdRubro }).IsUnique();
    }
}
