using Corela15.Domain.Clientes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("cliente", "clientes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioOficial).WithMany().HasForeignKey(x => x.IdUsuarioOficial).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Numero).IsUnique();

        // Decisión de diseño explícita (ver Domain/Clientes/Cliente.cs): una Persona
        // puede tener varios Cliente en el tiempo, pero nunca dos ACTIVOS a la vez.
        b.HasIndex(x => x.IdPersona)
            .HasFilter("estado = 'Activo'")
            .IsUnique()
            .HasDatabaseName("ix_cliente_persona_unico_si_activo");

        b.Property(x => x.CodigoCausaVinculacion).HasMaxLength(3);
        b.HasOne(x => x.CausaVinculacion).WithMany().HasForeignKey(x => x.CodigoCausaVinculacion).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CodigoCalificacionInterna).HasMaxLength(2);
        b.HasOne(x => x.CalificacionInterna).WithMany().HasForeignKey(x => x.CodigoCalificacionInterna).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CodigoSectorEconomico).HasMaxLength(2);
        b.HasOne(x => x.SectorEconomico).WithMany().HasForeignKey(x => x.CodigoSectorEconomico).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CausaVinculacionConfiguration : IEntityTypeConfiguration<CausaVinculacion>
{
    public void Configure(EntityTypeBuilder<CausaVinculacion> b)
    {
        b.ToTable("causa_vinculacion", "clientes");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(3);
        b.Property(x => x.Descripcion).HasMaxLength(600).IsRequired();
    }
}

public class CalificacionInternaConfiguration : IEntityTypeConfiguration<CalificacionInterna>
{
    public void Configure(EntityTypeBuilder<CalificacionInterna> b)
    {
        b.ToTable("calificacion_interna", "clientes");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class SectorEconomicoConfiguration : IEntityTypeConfiguration<SectorEconomico>
{
    public void Configure(EntityTypeBuilder<SectorEconomico> b)
    {
        b.ToTable("sector_economico", "clientes");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}
