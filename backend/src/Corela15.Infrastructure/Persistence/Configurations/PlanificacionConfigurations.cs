using Corela15.Domain.Planificacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class AreaPlanificacionConfiguration : IEntityTypeConfiguration<AreaPlanificacion>
{
    public void Configure(EntityTypeBuilder<AreaPlanificacion> b)
    {
        b.ToTable("area", "planificacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class EtiquetaPlanificacionConfiguration : IEntityTypeConfiguration<EtiquetaPlanificacion>
{
    public void Configure(EntityTypeBuilder<EtiquetaPlanificacion> b)
    {
        b.ToTable("etiqueta", "planificacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.Property(x => x.ColorHex).HasMaxLength(7).IsRequired();
    }
}

public class PlanSemanalConfiguration : IEntityTypeConfiguration<PlanSemanal>
{
    public void Configure(EntityTypeBuilder<PlanSemanal> b)
    {
        b.ToTable("plan_semanal", "planificacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.NombreResponsable).HasMaxLength(200).IsRequired();
        b.Property(x => x.CargoResponsable).HasMaxLength(150).IsRequired();
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Area).WithMany().HasForeignKey(x => x.CodigoArea).OnDelete(DeleteBehavior.Restrict);

        // Una sola planificación real por área x semana -- reeditar la
        // misma semana actualiza esa fila, nunca duplica.
        b.HasIndex(x => new { x.CodigoArea, x.FechaInicioSemana }).IsUnique();
        // Filtro real de privacidad (ver PlanificacionService.ListarAsync).
        b.HasIndex(x => x.CreadoPor);
    }
}

public class PlanSemanalBloqueConfiguration : IEntityTypeConfiguration<PlanSemanalBloque>
{
    public void Configure(EntityTypeBuilder<PlanSemanalBloque> b)
    {
        b.ToTable("plan_semanal_bloque", "planificacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();

        b.HasOne(x => x.PlanSemanal).WithMany(x => x.Bloques).HasForeignKey(x => x.IdPlanSemanal).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Etiqueta).WithMany().HasForeignKey(x => x.CodigoEtiqueta).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdPlanSemanal);
    }
}
