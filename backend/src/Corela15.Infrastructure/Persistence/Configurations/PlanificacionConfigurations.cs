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

        // Un solo espacio personal por usuario -- el índice único es lo
        // que garantiza que crearlo "la primera vez que se necesita" nunca
        // duplique, aunque dos requests lleguen a la vez.
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdUsuario).IsUnique().HasFilter("id_usuario IS NOT NULL");
    }
}

public class EtiquetaPlanificacionConfiguration : IEntityTypeConfiguration<EtiquetaPlanificacion>
{
    public void Configure(EntityTypeBuilder<EtiquetaPlanificacion> b)
    {
        b.ToTable("etiqueta", "planificacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.Property(x => x.ColorHex).HasMaxLength(7).IsRequired();

        b.HasOne(x => x.Area).WithMany().HasForeignKey(x => x.CodigoArea).OnDelete(DeleteBehavior.Restrict);
        // Nunca dos etiquetas con el mismo nombre real dentro de la misma
        // área -- así "obtener o crear" puede confiar en buscar por
        // (área, nombre) sin ambigüedad.
        b.HasIndex(x => new { x.CodigoArea, x.Nombre }).IsUnique();
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
        b.Property(x => x.EnviadaPor).HasMaxLength(100);
        b.Property(x => x.NotaGerencia).HasMaxLength(2000);
        b.Property(x => x.EstadoAprobacion).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.AprobadoPor).HasMaxLength(100);

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
        b.Property(x => x.NotaGerencia).HasMaxLength(1000);

        b.HasOne(x => x.PlanSemanal).WithMany(x => x.Bloques).HasForeignKey(x => x.IdPlanSemanal).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Etiqueta).WithMany().HasForeignKey(x => x.CodigoEtiqueta).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Disposicion).WithMany().HasForeignKey(x => x.IdDisposicion).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdPlanSemanal);
        b.HasIndex(x => x.IdDisposicion);
    }
}

public class DisposicionGerenciaConfiguration : IEntityTypeConfiguration<DisposicionGerencia>
{
    public void Configure(EntityTypeBuilder<DisposicionGerencia> b)
    {
        b.ToTable("disposicion_gerencia", "planificacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(2000);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
    }
}

public class DisposicionGerenciaAreaConfiguration : IEntityTypeConfiguration<DisposicionGerenciaArea>
{
    public void Configure(EntityTypeBuilder<DisposicionGerenciaArea> b)
    {
        b.ToTable("disposicion_gerencia_area", "planificacion");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Disposicion).WithMany(x => x.Areas).HasForeignKey(x => x.IdDisposicion).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Area).WithMany().HasForeignKey(x => x.CodigoArea).OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.Cumplimiento).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.ComentarioCumplimiento).HasMaxLength(1000);
        b.Property(x => x.CalificadoPor).HasMaxLength(100);

        b.HasIndex(x => new { x.IdDisposicion, x.CodigoArea }).IsUnique();
        b.HasIndex(x => x.CodigoArea);
    }
}

public class PlanSemanalIndicadorConfiguration : IEntityTypeConfiguration<PlanSemanalIndicador>
{
    public void Configure(EntityTypeBuilder<PlanSemanalIndicador> b)
    {
        b.ToTable("plan_semanal_indicador", "planificacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Meta).HasColumnType("numeric(18,2)");
        b.Property(x => x.Real).HasColumnType("numeric(18,2)");
        b.Property(x => x.Comentario).HasMaxLength(500);

        b.HasOne(x => x.PlanSemanal).WithMany(x => x.Indicadores).HasForeignKey(x => x.IdPlanSemanal).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Indicador).WithMany().HasForeignKey(x => x.IdIndicador).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdPlanSemanal, x.IdIndicador }).IsUnique();
    }
}
