using Corela15.Domain.Ahorros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class TipoCuentaConfiguration : IEntityTypeConfiguration<TipoCuenta>
{
    public void Configure(EntityTypeBuilder<TipoCuenta> b)
    {
        b.ToTable("tipo_cuenta", "ahorros");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.SaldoMinimo).HasColumnType("numeric(18,2)");
        b.Property(x => x.SaldoMinimoConPrestamo).HasColumnType("numeric(18,2)");
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> b)
    {
        b.ToTable("cuenta", "ahorros");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.TipoCuenta).WithMany().HasForeignKey(x => x.IdTipoCuenta).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Numero).IsUnique();
    }
}

public class CuentaClienteConfiguration : IEntityTypeConfiguration<CuentaCliente>
{
    public void Configure(EntityTypeBuilder<CuentaCliente> b)
    {
        b.ToTable("cuenta_cliente", "ahorros");
        b.HasKey(x => new { x.IdCuenta, x.IdCliente });

        b.HasOne(x => x.Cuenta).WithMany().HasForeignKey(x => x.IdCuenta).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ItemSaldoConfiguration : IEntityTypeConfiguration<ItemSaldo>
{
    public void Configure(EntityTypeBuilder<ItemSaldo> b)
    {
        b.ToTable("item_saldo", "ahorros");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TipoCuentaItemSaldoConfiguration : IEntityTypeConfiguration<TipoCuentaItemSaldo>
{
    public void Configure(EntityTypeBuilder<TipoCuentaItemSaldo> b)
    {
        b.ToTable("tipo_cuenta_item_saldo", "ahorros");
        b.HasKey(x => new { x.IdTipoCuenta, x.IdItemSaldo });

        b.HasOne(x => x.TipoCuenta).WithMany().HasForeignKey(x => x.IdTipoCuenta).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ItemSaldo).WithMany().HasForeignKey(x => x.IdItemSaldo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CuentaItemSaldoConfiguration : IEntityTypeConfiguration<CuentaItemSaldo>
{
    public void Configure(EntityTypeBuilder<CuentaItemSaldo> b)
    {
        b.ToTable("cuenta_item_saldo", "ahorros");
        b.HasKey(x => x.Id);
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Cuenta).WithMany().HasForeignKey(x => x.IdCuenta).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ItemSaldo).WithMany().HasForeignKey(x => x.IdItemSaldo).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdCuenta, x.IdItemSaldo }).IsUnique();
    }
}

public class CuentaMovimientoConfiguration : IEntityTypeConfiguration<CuentaMovimiento>
{
    public void Configure(EntityTypeBuilder<CuentaMovimiento> b)
    {
        b.ToTable("cuenta_movimiento", "ahorros");
        b.HasKey(x => x.Id);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Monto).HasColumnType("numeric(18,2)");
        b.Property(x => x.SaldoResultante).HasColumnType("numeric(18,2)");
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Cuenta).WithMany().HasForeignKey(x => x.IdCuenta).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ComprobanteContable).WithMany().HasForeignKey(x => x.IdComprobanteContable)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdCuenta, x.FechaHora });
    }
}
