using Corela15.Domain.Portafolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class TipoInstitucionConfiguration : IEntityTypeConfiguration<TipoInstitucion>
{
    public void Configure(EntityTypeBuilder<TipoInstitucion> b)
    {
        b.ToTable("tipo_institucion", "portafolio");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(10);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class InstitucionConfiguration : IEntityTypeConfiguration<Institucion>
{
    public void Configure(EntityTypeBuilder<Institucion> b)
    {
        b.ToTable("institucion", "portafolio");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(10);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.TipoInstitucion).WithMany().HasForeignKey(x => x.CodigoTipoInstitucion).OnDelete(DeleteBehavior.Restrict);
    }
}

public class InversionPortafolioConfiguration : IEntityTypeConfiguration<InversionPortafolio>
{
    public void Configure(EntityTypeBuilder<InversionPortafolio> b)
    {
        b.ToTable("inversion_portafolio", "portafolio");
        b.HasKey(x => x.Id);
        b.Property(x => x.Documento).HasMaxLength(100).IsRequired();
        b.Property(x => x.ValorNominal).HasColumnType("numeric(18,2)");
        b.Property(x => x.Tasa).HasColumnType("numeric(9,4)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Institucion).WithMany().HasForeignKey(x => x.CodigoInstitucion).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Documento).IsUnique();

        // Dos operaciones simultáneas sobre la misma inversión (cancelar +
        // renovar) no deben pisarse — mismo patrón que Deposito.
        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class InversionRenovacionConfiguration : IEntityTypeConfiguration<InversionRenovacion>
{
    public void Configure(EntityTypeBuilder<InversionRenovacion> b)
    {
        b.ToTable("inversion_renovacion", "portafolio");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.InversionOrigen).WithMany().HasForeignKey(x => x.IdInversionOrigen).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.InversionDestino).WithMany().HasForeignKey(x => x.IdInversionDestino).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdInversionDestino).IsUnique();
    }
}
