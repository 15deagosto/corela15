using Corela15.Domain.Credito;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class TipoPrestamoConfiguration : IEntityTypeConfiguration<TipoPrestamo>
{
    public void Configure(EntityTypeBuilder<TipoPrestamo> b)
    {
        b.ToTable("tipo_prestamo", "credito");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.Property(x => x.MontoMinimo).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoMaximo).HasColumnType("numeric(18,2)");
        b.Property(x => x.TasaAnual).HasColumnType("numeric(9,4)");
        b.Property(x => x.SegmentoBce).HasMaxLength(60).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TasaTechoBceConfiguration : IEntityTypeConfiguration<TasaTechoBce>
{
    public void Configure(EntityTypeBuilder<TasaTechoBce> b)
    {
        b.ToTable("tasa_techo_bce", "credito");
        b.HasKey(x => x.Id);
        b.Property(x => x.Segmento).HasMaxLength(60).IsRequired();
        b.Property(x => x.TasaMaxima).HasColumnType("numeric(9,4)");
        b.HasIndex(x => new { x.Segmento, x.FechaVigenciaDesde }).IsUnique();
    }
}

public class SolicitudPrestamoConfiguration : IEntityTypeConfiguration<SolicitudPrestamo>
{
    public void Configure(EntityTypeBuilder<SolicitudPrestamo> b)
    {
        b.ToTable("solicitud_prestamo", "credito");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        b.Property(x => x.MontoSolicitado).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoAprobado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoPrestamo).WithMany().HasForeignKey(x => x.IdTipoPrestamo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Numero).IsUnique();
    }
}

public class ScoreCrediticioConfiguration : IEntityTypeConfiguration<ScoreCrediticio>
{
    public void Configure(EntityTypeBuilder<ScoreCrediticio> b)
    {
        b.ToTable("score_crediticio", "credito");
        b.HasKey(x => x.Id);
        b.Property(x => x.Categoria).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RatioIngresoEgreso).HasColumnType("numeric(9,4)");
        b.Property(x => x.RatioEndeudamiento).HasColumnType("numeric(9,4)");

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdCliente, x.Fecha });
    }
}
