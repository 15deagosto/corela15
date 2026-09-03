using Corela15.Domain.LavadoActivos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class CalificacionClienteConfiguration : IEntityTypeConfiguration<CalificacionCliente>
{
    public void Configure(EntityTypeBuilder<CalificacionCliente> b)
    {
        b.ToTable("calificacion_cliente", "lavadoactivos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Patrimonio).HasColumnType("numeric(18,2)");
        b.Property(x => x.IngresoMensual).HasColumnType("numeric(18,2)");
        b.Property(x => x.BandaPatrimonio).HasColumnType("numeric(9,4)");
        b.Property(x => x.BandaIngreso).HasColumnType("numeric(9,4)");
        b.Property(x => x.TotalPerfil).HasColumnType("numeric(9,4)");
        b.Property(x => x.Categoria).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdCliente, x.Fecha });
    }
}

public class RangoPatrimonioLavadoConfiguration : IEntityTypeConfiguration<RangoPatrimonioLavado>
{
    public void Configure(EntityTypeBuilder<RangoPatrimonioLavado> b)
    {
        b.ToTable("rango_patrimonio_lavado", "lavadoactivos");
        b.HasKey(x => x.Id);
        b.Property(x => x.ValorInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.ValorFinal).HasColumnType("numeric(18,2)");
        b.Property(x => x.Valor).HasColumnType("numeric(9,4)");
    }
}

public class RangoIngresoLavadoConfiguration : IEntityTypeConfiguration<RangoIngresoLavado>
{
    public void Configure(EntityTypeBuilder<RangoIngresoLavado> b)
    {
        b.ToTable("rango_ingreso_lavado", "lavadoactivos");
        b.HasKey(x => x.Id);
        b.Property(x => x.ValorInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.ValorFinal).HasColumnType("numeric(18,2)");
        b.Property(x => x.Valor).HasColumnType("numeric(9,4)");
    }
}

public class TipoListaControlConfiguration : IEntityTypeConfiguration<TipoListaControl>
{
    public void Configure(EntityTypeBuilder<TipoListaControl> b)
    {
        b.ToTable("tipo_lista_control", "lavadoactivos");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(5);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class AlertaListaControlConfiguration : IEntityTypeConfiguration<AlertaListaControl>
{
    public void Configure(EntityTypeBuilder<AlertaListaControl> b)
    {
        b.ToTable("alerta_lista_control", "lavadoactivos");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoTipoListaControl).HasMaxLength(5).IsRequired();
        b.Property(x => x.Detalle).HasMaxLength(1000).IsRequired();
        b.Property(x => x.ComentarioResolucion).HasMaxLength(1000);
        b.Property(x => x.ResueltoPor).HasMaxLength(100);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoListaControl).WithMany().HasForeignKey(x => x.CodigoTipoListaControl).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Resuelta);
    }
}
