using Corela15.Domain.Inversion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class DepositoConfiguration : IEntityTypeConfiguration<Deposito>
{
    public void Configure(EntityTypeBuilder<Deposito> b)
    {
        b.ToTable("deposito", "inversion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Monto).HasColumnType("numeric(18,2)");
        b.Property(x => x.Tasa).HasColumnType("numeric(9,4)");
        b.Property(x => x.VariacionTasa).HasColumnType("numeric(9,4)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class DepositoClienteConfiguration : IEntityTypeConfiguration<DepositoCliente>
{
    public void Configure(EntityTypeBuilder<DepositoCliente> b)
    {
        b.ToTable("deposito_cliente", "inversion");
        b.HasKey(x => new { x.IdDeposito, x.IdCliente });

        b.HasOne(x => x.Deposito).WithMany().HasForeignKey(x => x.IdDeposito).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DepositoRenovacionConfiguration : IEntityTypeConfiguration<DepositoRenovacion>
{
    public void Configure(EntityTypeBuilder<DepositoRenovacion> b)
    {
        b.ToTable("deposito_renovacion", "inversion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Valor).HasColumnType("numeric(18,2)");
        b.Property(x => x.ValorIncremento).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.DepositoOrigen).WithMany().HasForeignKey(x => x.IdDepositoOrigen)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DepositoDestino).WithMany().HasForeignKey(x => x.IdDepositoDestino)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ItemPlazoTasaConfiguration : IEntityTypeConfiguration<ItemPlazoTasa>
{
    public void Configure(EntityTypeBuilder<ItemPlazoTasa> b)
    {
        b.ToTable("item_plazo_tasa", "inversion");
        b.HasKey(x => x.Id);
        b.Property(x => x.MontoMin).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoMax).HasColumnType("numeric(18,2)");
        b.Property(x => x.Tasa).HasColumnType("numeric(9,4)");
        b.Property(x => x.TipoPersona).HasConversion<string>().HasMaxLength(20);
    }
}
