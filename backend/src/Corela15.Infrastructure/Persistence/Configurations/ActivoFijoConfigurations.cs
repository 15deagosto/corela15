using Corela15.Domain.ActivoFijo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class EstructuraConfiguration : IEntityTypeConfiguration<Estructura>
{
    public void Configure(EntityTypeBuilder<Estructura> b)
    {
        b.ToTable("estructura", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.PorcentajeDepreciacionAnual).HasColumnType("numeric(9,4)");

        b.HasOne(x => x.CuentaContableActivo).WithMany().HasForeignKey(x => x.IdCuentaContableActivo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CuentaContableDeprecia).WithMany().HasForeignKey(x => x.IdCuentaContableDeprecia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CuentaContableGasto).WithMany().HasForeignKey(x => x.IdCuentaContableGasto).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ResponsableConfiguration : IEntityTypeConfiguration<Responsable>
{
    public void Configure(EntityTypeBuilder<Responsable> b)
    {
        b.ToTable("responsable", "activofijo");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ActivoConfiguration : IEntityTypeConfiguration<Activo>
{
    public void Configure(EntityTypeBuilder<Activo> b)
    {
        b.ToTable("activo", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(100);
        b.Property(x => x.Detalle).HasMaxLength(500).IsRequired();
        b.Property(x => x.Valor).HasColumnType("numeric(18,2)");
        b.Property(x => x.DepreciacionAcumulada).HasColumnType("numeric(18,2)");
        b.Property(x => x.Marca).HasMaxLength(200);
        b.Property(x => x.Modelo).HasMaxLength(200);
        b.Property(x => x.Serie).HasMaxLength(200);
        b.Property(x => x.Color).HasMaxLength(100);
        b.Property(x => x.Motor).HasMaxLength(100);
        b.Property(x => x.Chasis).HasMaxLength(100);
        b.Property(x => x.Placa).HasMaxLength(20);
        b.Property(x => x.Cilindraje).HasMaxLength(50);
        b.Property(x => x.Condicion).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Estructura).WithMany().HasForeignKey(x => x.IdEstructura).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        // Código físico real (ver ACTIVOFIJO.ACTIVO.CODIGOHOMOLOGADO en
        // Softbank, verificado sin duplicados en producción) — único
        // cuando se captura, pero el campo es opcional (varios activos
        // reales no lo tienen), así que el índice es parcial: permite
        // múltiples NULL, exige unicidad real solo entre los que sí lo
        // tienen.
        b.HasIndex(x => x.Codigo).IsUnique().HasFilter("codigo IS NOT NULL");

        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class ActivoResponsableConfiguration : IEntityTypeConfiguration<ActivoResponsable>
{
    public void Configure(EntityTypeBuilder<ActivoResponsable> b)
    {
        b.ToTable("activo_responsable", "activofijo");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Activo).WithMany().HasForeignKey(x => x.IdActivo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Responsable).WithMany().HasForeignKey(x => x.IdResponsable).OnDelete(DeleteBehavior.Restrict);
    }
}

public class DepreciacionAgenciaConfiguration : IEntityTypeConfiguration<DepreciacionAgencia>
{
    public void Configure(EntityTypeBuilder<DepreciacionAgencia> b)
    {
        b.ToTable("depreciacion_agencia", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdAgencia, x.Fecha }).IsUnique();
    }
}

public class DepreciacionAgenciaDetalleConfiguration : IEntityTypeConfiguration<DepreciacionAgenciaDetalle>
{
    public void Configure(EntityTypeBuilder<DepreciacionAgenciaDetalle> b)
    {
        b.ToTable("depreciacion_agencia_detalle", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.DepreciacionPeriodo).HasColumnType("numeric(18,2)");
        b.Property(x => x.DepreciacionAcumulada).HasColumnType("numeric(18,2)");
        b.Property(x => x.SaldoLibros).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.DepreciacionAgencia).WithMany().HasForeignKey(x => x.IdDepreciacionAgencia).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Activo).WithMany().HasForeignKey(x => x.IdActivo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MotivoTrasladoActivoConfiguration : IEntityTypeConfiguration<MotivoTrasladoActivo>
{
    public void Configure(EntityTypeBuilder<MotivoTrasladoActivo> b)
    {
        b.ToTable("motivo_traslado_activo", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class TrasladoActivoConfiguration : IEntityTypeConfiguration<TrasladoActivo>
{
    public void Configure(EntityTypeBuilder<TrasladoActivo> b)
    {
        b.ToTable("traslado_activo", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Concepto).HasMaxLength(300).IsRequired();
        b.Property(x => x.Razon).HasMaxLength(500);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Activo).WithMany().HasForeignKey(x => x.IdActivo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MotivoTraslado).WithMany().HasForeignKey(x => x.IdMotivoTraslado).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ResponsableOrigen).WithMany().HasForeignKey(x => x.IdResponsableOrigen).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AgenciaOrigen).WithMany().HasForeignKey(x => x.IdAgenciaOrigen).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ResponsableDestino).WithMany().HasForeignKey(x => x.IdResponsableDestino).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AgenciaDestino).WithMany().HasForeignKey(x => x.IdAgenciaDestino).OnDelete(DeleteBehavior.Restrict);
    }
}

public class MotivoBajaConfiguration : IEntityTypeConfiguration<MotivoBaja>
{
    public void Configure(EntityTypeBuilder<MotivoBaja> b)
    {
        b.ToTable("motivo_baja", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(200).IsRequired();
    }
}

public class SolicitudActivoBajaConfiguration : IEntityTypeConfiguration<SolicitudActivoBaja>
{
    public void Configure(EntityTypeBuilder<SolicitudActivoBaja> b)
    {
        b.ToTable("solicitud_activo_baja", "activofijo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(500);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Activo).WithMany().HasForeignKey(x => x.IdActivo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.MotivoBaja).WithMany().HasForeignKey(x => x.IdMotivoBaja).OnDelete(DeleteBehavior.Restrict);
    }
}
