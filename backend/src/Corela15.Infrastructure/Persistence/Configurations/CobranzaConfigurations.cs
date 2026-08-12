using Corela15.Domain.Cobranza;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class PeriodoMoraConfiguration : IEntityTypeConfiguration<PeriodoMora>
{
    public void Configure(EntityTypeBuilder<PeriodoMora> b)
    {
        b.ToTable("periodo_mora", "cobranza");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class AccionGestionConfiguration : IEntityTypeConfiguration<AccionGestion>
{
    public void Configure(EntityTypeBuilder<AccionGestion> b)
    {
        b.ToTable("accion_gestion", "cobranza");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class GestionPrestamoCobranzaConfiguration : IEntityTypeConfiguration<GestionPrestamoCobranza>
{
    public void Configure(EntityTypeBuilder<GestionPrestamoCobranza> b)
    {
        b.ToTable("gestion_prestamo_cobranza", "cobranza");
        b.HasKey(x => x.Id);
        b.Property(x => x.Observacion).HasMaxLength(500);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AccionGestion).WithMany().HasForeignKey(x => x.IdAccionGestion).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdPrestamo);
        b.HasIndex(x => x.Fecha);
    }
}
