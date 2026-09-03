using Corela15.Domain.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class PaisConfiguration : IEntityTypeConfiguration<Pais>
{
    public void Configure(EntityTypeBuilder<Pais> b)
    {
        b.ToTable("pais", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(3).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ProvinciaConfiguration : IEntityTypeConfiguration<Provincia>
{
    public void Configure(EntityTypeBuilder<Provincia> b)
    {
        b.ToTable("provincia", "general");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class MonedaConfiguration : IEntityTypeConfiguration<Moneda>
{
    public void Configure(EntityTypeBuilder<Moneda> b)
    {
        b.ToTable("moneda", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(3).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        b.Property(x => x.Simbolo).HasMaxLength(5).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TipoIdentificacionConfiguration : IEntityTypeConfiguration<TipoIdentificacion>
{
    public void Configure(EntityTypeBuilder<TipoIdentificacion> b)
    {
        b.ToTable("tipo_identificacion", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> b)
    {
        b.ToTable("empresa", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Ruc).HasMaxLength(13).IsRequired();
        b.HasOne(x => x.Moneda).WithMany().HasForeignKey(x => x.IdMoneda).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Pais).WithMany().HasForeignKey(x => x.IdPais).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Ruc).IsUnique();
    }
}

public class AgenciaConfiguration : IEntityTypeConfiguration<Agencia>
{
    public void Configure(EntityTypeBuilder<Agencia> b)
    {
        b.ToTable("agencia", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.IdEmpresa).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdEmpresa, x.Codigo }).IsUnique();
    }
}

public class TipoActividadConfiguration : IEntityTypeConfiguration<TipoActividad>
{
    public void Configure(EntityTypeBuilder<TipoActividad> b)
    {
        b.ToTable("tipo_actividad", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class ActividadEconomicaConfiguration : IEntityTypeConfiguration<ActividadEconomica>
{
    public void Configure(EntityTypeBuilder<ActividadEconomica> b)
    {
        b.ToTable("actividad_economica", "general");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(4000).IsRequired();

        b.HasOne(x => x.TipoActividad).WithMany().HasForeignKey(x => x.IdTipoActividad).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActividadPadre).WithMany().HasForeignKey(x => x.IdActividadPadre).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Codigo).IsUnique();
        b.HasIndex(x => x.IdActividadPadre);
    }
}
