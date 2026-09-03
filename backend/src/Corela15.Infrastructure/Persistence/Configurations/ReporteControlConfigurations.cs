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

public class TarifaServicioFinancieroConfiguration : IEntityTypeConfiguration<TarifaServicioFinanciero>
{
    public void Configure(EntityTypeBuilder<TarifaServicioFinanciero> b)
    {
        b.ToTable("tarifa_servicio_financiero", "reportecontrol");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Tarifa).HasColumnType("numeric(9,2)");
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TarifaGastoCobranzaConfiguration : IEntityTypeConfiguration<TarifaGastoCobranza>
{
    public void Configure(EntityTypeBuilder<TarifaGastoCobranza> b)
    {
        b.ToTable("tarifa_gasto_cobranza", "reportecontrol");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.CuotaInicio).HasColumnType("numeric(18,2)");
        b.Property(x => x.CuotaFinal).HasColumnType("numeric(18,2)");
        b.Property(x => x.Detalle).HasMaxLength(200).IsRequired();
        b.Property(x => x.Valor).HasColumnType("numeric(9,2)");
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
