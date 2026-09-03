using Corela15.Domain.Riesgo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class MacroProcesoConfiguration : IEntityTypeConfiguration<MacroProceso>
{
    public void Configure(EntityTypeBuilder<MacroProceso> b)
    {
        b.ToTable("macroproceso", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class ProcesoConfiguration : IEntityTypeConfiguration<Proceso>
{
    public void Configure(EntityTypeBuilder<Proceso> b)
    {
        b.ToTable("proceso", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.HasOne(x => x.MacroProceso).WithMany().HasForeignKey(x => x.IdMacroProceso).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NivelImpactoConfiguration : IEntityTypeConfiguration<NivelImpacto>
{
    public void Configure(EntityTypeBuilder<NivelImpacto> b)
    {
        b.ToTable("nivel_impacto", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class NivelProbabilidadConfiguration : IEntityTypeConfiguration<NivelProbabilidad>
{
    public void Configure(EntityTypeBuilder<NivelProbabilidad> b)
    {
        b.ToTable("nivel_probabilidad", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class NivelRiesgoConfiguration : IEntityTypeConfiguration<NivelRiesgo>
{
    public void Configure(EntityTypeBuilder<NivelRiesgo> b)
    {
        b.ToTable("nivel_riesgo", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.Color).HasMaxLength(20);
        b.Property(x => x.RangoInicio).HasColumnType("numeric(9,4)");
        b.Property(x => x.RangoFin).HasColumnType("numeric(9,4)");
    }
}

public class EventoRiesgoConfiguration : IEntityTypeConfiguration<EventoRiesgo>
{
    public void Configure(EntityTypeBuilder<EventoRiesgo> b)
    {
        b.ToTable("evento_riesgo", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();

        b.HasOne(x => x.Proceso).WithMany().HasForeignKey(x => x.IdProceso).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.NivelImpacto).WithMany().HasForeignKey(x => x.IdNivelImpacto).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.NivelProbabilidad).WithMany().HasForeignKey(x => x.IdNivelProbabilidad).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.NivelRiesgo).WithMany().HasForeignKey(x => x.IdNivelRiesgo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class IndicadorLiquidezConfiguration : IEntityTypeConfiguration<IndicadorLiquidez>
{
    public void Configure(EntityTypeBuilder<IndicadorLiquidez> b)
    {
        b.ToTable("indicador_liquidez", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.FondosDisponibles).HasColumnType("numeric(18,2)");
        b.Property(x => x.DepositosCortoPlazo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Coeficiente).HasColumnType("numeric(9,4)");
        b.Property(x => x.MinimoRegulatorio).HasColumnType("numeric(9,4)");
        b.HasIndex(x => x.Fecha);
    }
}

public class EstadoAvanceRiesgoConfiguration : IEntityTypeConfiguration<EstadoAvanceRiesgo>
{
    public void Configure(EntityTypeBuilder<EstadoAvanceRiesgo> b)
    {
        b.ToTable("estado_avance_riesgo", "riesgo");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(5);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class AvanceRiesgoConfiguration : IEntityTypeConfiguration<AvanceRiesgo>
{
    public void Configure(EntityTypeBuilder<AvanceRiesgo> b)
    {
        b.ToTable("avance_riesgo", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstado).HasMaxLength(5).IsRequired();
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.EventoRiesgo).WithMany().HasForeignKey(x => x.IdEventoRiesgo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioResponsable).WithMany().HasForeignKey(x => x.IdUsuarioResponsable).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdEventoRiesgo);
    }
}

public class AvanceRiesgoDetalleConfiguration : IEntityTypeConfiguration<AvanceRiesgoDetalle>
{
    public void Configure(EntityTypeBuilder<AvanceRiesgoDetalle> b)
    {
        b.ToTable("avance_riesgo_detalle", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventoDetectado).HasMaxLength(1000).IsRequired();
        b.Property(x => x.PosibleCausa).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Tratamiento).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Inconvenientes).HasMaxLength(500);
        b.Property(x => x.AccionesSugeridas).HasMaxLength(500);

        b.HasOne(x => x.AvanceRiesgo).WithMany().HasForeignKey(x => x.IdAvanceRiesgo).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.IdAvanceRiesgo);
    }
}

public class AvanceRiesgoEtapaConfiguration : IEntityTypeConfiguration<AvanceRiesgoEtapa>
{
    public void Configure(EntityTypeBuilder<AvanceRiesgoEtapa> b)
    {
        b.ToTable("avance_riesgo_etapa", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstado).HasMaxLength(5).IsRequired();
        b.Property(x => x.Comentario).HasMaxLength(500);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.AvanceRiesgoDetalle).WithMany().HasForeignKey(x => x.IdAvanceRiesgoDetalle).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdAvanceRiesgoDetalle);
    }
}

public class ParametroLiquidezConfiguration : IEntityTypeConfiguration<ParametroLiquidez>
{
    public void Configure(EntityTypeBuilder<ParametroLiquidez> b)
    {
        b.ToTable("parametro_liquidez", "riesgo");
        b.HasKey(x => x.Id);
        b.Property(x => x.MinimoRegulatorio).HasColumnType("numeric(9,4)");
        b.Property(x => x.ActualizadoPor).HasMaxLength(100).IsRequired();
    }
}
