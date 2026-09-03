using Corela15.Domain.Financiero;
using Corela15.Domain.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class BancoConfiguration : IEntityTypeConfiguration<Banco>
{
    public void Configure(EntityTypeBuilder<Banco> b)
    {
        b.ToTable("banco", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
    }
}

public class ChequeConfiguration : IEntityTypeConfiguration<Cheque>
{
    public void Configure(EntityTypeBuilder<Cheque> b)
    {
        b.ToTable("cheque", "financiero");
        b.HasKey(x => x.Id);
        b.Property(x => x.CuentaCorriente).HasMaxLength(80).IsRequired();
        b.Property(x => x.NumeroCheque).HasMaxLength(80).IsRequired();
        b.Property(x => x.Valor).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Banco).WithMany().HasForeignKey(x => x.IdBanco).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cuenta).WithMany().HasForeignKey(x => x.IdCuenta).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        // Dos operaciones simultáneas sobre el mismo cheque (efectivizar +
        // protestar) no deben pisarse.
        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class ChequeProtestoConfiguration : IEntityTypeConfiguration<ChequeProtesto>
{
    public void Configure(EntityTypeBuilder<ChequeProtesto> b)
    {
        b.ToTable("cheque_protesto", "financiero");
        b.HasKey(x => x.Id);
        b.Property(x => x.Documento).HasMaxLength(200);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Cheque).WithMany().HasForeignKey(x => x.IdCheque).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdCheque).IsUnique();
    }
}
