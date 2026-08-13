using Corela15.Domain.ReporteControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class ReporteRegulatorioConfiguration : IEntityTypeConfiguration<ReporteRegulatorio>
{
    public void Configure(EntityTypeBuilder<ReporteRegulatorio> b)
    {
        b.ToTable("reporte_regulatorio", "reportecontrol");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Entidad).HasConversion<string>().HasMaxLength(10);
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
