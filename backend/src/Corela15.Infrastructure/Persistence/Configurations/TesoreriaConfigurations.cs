using Corela15.Domain.ActivoFijo;
using Corela15.Domain.CuentasPorCobrar;
using Corela15.Domain.Obligacion;
using Corela15.Domain.Portafolio;
using Corela15.Domain.Proveeduria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class ObligacionFinancieraConfiguration : IEntityTypeConfiguration<ObligacionFinanciera>
{
    public void Configure(EntityTypeBuilder<ObligacionFinanciera> b)
    {
        b.ToTable("obligacion_financiera", "obligacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.NumeroPagare).HasMaxLength(50);
        b.Property(x => x.DeudaInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.ValorEntregado).HasColumnType("numeric(18,2)");
        b.Property(x => x.SaldoActual).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ActivoConfiguration : IEntityTypeConfiguration<Activo>
{
    public void Configure(EntityTypeBuilder<Activo> b)
    {
        b.ToTable("activo", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(300).IsRequired();
        b.Property(x => x.Valor).HasColumnType("numeric(18,2)");
        b.Property(x => x.Marca).HasMaxLength(100);
        b.Property(x => x.Modelo).HasMaxLength(100);
        b.Property(x => x.Serie).HasMaxLength(100);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
    }
}

public class CuentaPorCobrarConfiguration : IEntityTypeConfiguration<CuentaPorCobrar>
{
    public void Configure(EntityTypeBuilder<CuentaPorCobrar> b)
    {
        b.ToTable("cuenta_por_cobrar", "cuentasporcobrar");
        b.HasKey(x => x.Id);
        b.Property(x => x.Concepto).HasMaxLength(300).IsRequired();
        b.Property(x => x.MontoInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);

        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class CuentaPorPagarConfiguration : IEntityTypeConfiguration<CuentaPorPagar>
{
    public void Configure(EntityTypeBuilder<CuentaPorPagar> b)
    {
        b.ToTable("cuenta_por_pagar", "cuentasporcobrar");
        b.HasKey(x => x.Id);
        b.Property(x => x.Concepto).HasMaxLength(300).IsRequired();
        b.Property(x => x.MontoInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ArticuloConfiguration : IEntityTypeConfiguration<Articulo>
{
    public void Configure(EntityTypeBuilder<Articulo> b)
    {
        b.ToTable("articulo", "proveeduria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Marca).HasMaxLength(100);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class InversionPortafolioConfiguration : IEntityTypeConfiguration<InversionPortafolio>
{
    public void Configure(EntityTypeBuilder<InversionPortafolio> b)
    {
        b.ToTable("inversion_portafolio", "portafolio");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Institucion).HasMaxLength(200).IsRequired();
        b.Property(x => x.Monto).HasColumnType("numeric(18,2)");
        b.Property(x => x.Tasa).HasColumnType("numeric(9,4)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
