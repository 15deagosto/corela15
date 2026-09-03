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
        b.Property(x => x.CodigoTipoCreditoSeps).HasMaxLength(2);
        b.Property(x => x.CodigoTipoSeguro).HasMaxLength(2);
        b.HasIndex(x => x.Codigo).IsUnique();

        b.HasOne(x => x.TipoSeguro).WithMany().HasForeignKey(x => x.CodigoTipoSeguro).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TipoSeguroConfiguration : IEntityTypeConfiguration<TipoSeguro>
{
    public void Configure(EntityTypeBuilder<TipoSeguro> b)
    {
        b.ToTable("tipo_seguro", "credito");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.ValorMensual).HasColumnType("numeric(18,2)");
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

        b.Property(x => x.CodigoTipoConvenio).HasMaxLength(20);

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoPrestamo).WithMany().HasForeignKey(x => x.IdTipoPrestamo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EtapaActual).WithMany().HasForeignKey(x => x.IdEtapaActual).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoConvenio).WithMany().HasForeignKey(x => x.CodigoTipoConvenio).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Numero).IsUnique();
    }
}

public class TipoConvenioConfiguration : IEntityTypeConfiguration<TipoConvenio>
{
    public void Configure(EntityTypeBuilder<TipoConvenio> b)
    {
        b.ToTable("tipo_convenio", "credito");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(500).IsRequired();
        b.Property(x => x.ValorAhorro).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SolicitudPrestamoEtapaHistConfiguration : IEntityTypeConfiguration<SolicitudPrestamoEtapaHist>
{
    public void Configure(EntityTypeBuilder<SolicitudPrestamoEtapaHist> b)
    {
        b.ToTable("solicitud_prestamo_etapa_hist", "credito");
        b.HasKey(x => x.Id);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.SolicitudPrestamo).WithMany().HasForeignKey(x => x.IdSolicitudPrestamo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Etapa).WithMany().HasForeignKey(x => x.IdEtapa).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EstadoGarantiaConfiguration : IEntityTypeConfiguration<EstadoGarantia>
{
    public void Configure(EntityTypeBuilder<EstadoGarantia> b)
    {
        b.ToTable("estado_garantia", "credito");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(5);
        b.Property(x => x.Detalle).HasMaxLength(100).IsRequired();
    }
}

public class SolicitudPrestamoGarantiaConfiguration : IEntityTypeConfiguration<SolicitudPrestamoGarantia>
{
    public void Configure(EntityTypeBuilder<SolicitudPrestamoGarantia> b)
    {
        b.ToTable("solicitud_prestamo_garantia", "credito");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(500);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.SolicitudPrestamo).WithMany().HasForeignKey(x => x.IdSolicitudPrestamo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ClienteGarante).WithMany().HasForeignKey(x => x.IdClienteGarante).OnDelete(DeleteBehavior.Restrict);
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
