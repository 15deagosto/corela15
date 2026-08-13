using Corela15.Domain.Auditoria;
using Corela15.Domain.CallCenter;
using Corela15.Domain.HerramientaRural;
using Corela15.Domain.Marketing;
using Corela15.Domain.Planificacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class AreaAuditoriaConfiguration : IEntityTypeConfiguration<AreaAuditoria>
{
    public void Configure(EntityTypeBuilder<AreaAuditoria> b)
    {
        b.ToTable("area_auditoria", "auditoria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class SeguimientoConfiguration : IEntityTypeConfiguration<Seguimiento>
{
    public void Configure(EntityTypeBuilder<Seguimiento> b)
    {
        b.ToTable("seguimiento", "auditoria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.AreaAuditoria).WithMany().HasForeignKey(x => x.IdAreaAuditoria).OnDelete(DeleteBehavior.Restrict);
        // Reusa contabilidad.NivelRiesgo... en realidad riesgo.nivel_riesgo (Nivel 8) — ver comentario en Seguimiento.cs.
        b.HasOne(x => x.NivelRiesgo).WithMany().HasForeignKey(x => x.IdNivelRiesgo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TipoComentarioConfiguration : IEntityTypeConfiguration<TipoComentario>
{
    public void Configure(EntityTypeBuilder<TipoComentario> b)
    {
        b.ToTable("tipo_comentario", "callcenter");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> b)
    {
        b.ToTable("comentario", "callcenter");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(1000).IsRequired();

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoComentario).WithMany().HasForeignKey(x => x.IdTipoComentario).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RifaConfiguration : IEntityTypeConfiguration<Rifa>
{
    public void Configure(EntityTypeBuilder<Rifa> b)
    {
        b.ToTable("rifa", "marketing");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
    }
}

public class RifaPremioConfiguration : IEntityTypeConfiguration<RifaPremio>
{
    public void Configure(EntityTypeBuilder<RifaPremio> b)
    {
        b.ToTable("rifa_premio", "marketing");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.ValorReferencial).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Rifa).WithMany(x => x.Premios).HasForeignKey(x => x.IdRifa).OnDelete(DeleteBehavior.Cascade);
    }
}

public class IndicadorConfiguration : IEntityTypeConfiguration<Indicador>
{
    public void Configure(EntityTypeBuilder<Indicador> b)
    {
        b.ToTable("indicador", "planificacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class PlanificacionAnualConfiguration : IEntityTypeConfiguration<PlanificacionAnual>
{
    public void Configure(EntityTypeBuilder<PlanificacionAnual> b)
    {
        b.ToTable("planificacion_anual", "planificacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Anio).IsUnique();
    }
}

public class TipoProductoAgrarioConfiguration : IEntityTypeConfiguration<TipoProductoAgrario>
{
    public void Configure(EntityTypeBuilder<TipoProductoAgrario> b)
    {
        b.ToTable("tipo_producto_agrario", "herramientarural");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
    }
}
