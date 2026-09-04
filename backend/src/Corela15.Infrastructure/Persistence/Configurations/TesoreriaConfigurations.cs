using Corela15.Domain.CuentasPorCobrar;
using Corela15.Domain.Obligacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class ObligacionFinancieraConfiguration : IEntityTypeConfiguration<ObligacionFinanciera>
{
    public void Configure(EntityTypeBuilder<ObligacionFinanciera> b)
    {
        b.ToTable("obligacion_financiera", "obligacion");
        b.HasKey(x => x.Id);

        b.Property(x => x.TipoIdentificacionAcreedor).HasMaxLength(1).IsRequired();
        b.Property(x => x.IdentificacionAcreedor).HasMaxLength(13).IsRequired();
        b.Property(x => x.CodigoPaisAcreedor).HasMaxLength(3).IsRequired();
        b.Property(x => x.NumeroObligacion).HasMaxLength(15).IsRequired();
        b.Property(x => x.DestinoLineaCredito).HasMaxLength(200).IsRequired();
        b.Property(x => x.MontoLineaCredito).HasColumnType("numeric(15,2)");
        b.Property(x => x.MontoPorUtilizar).HasColumnType("numeric(15,2)");
        b.Property(x => x.CodigoEstado).HasMaxLength(2).IsRequired();
        b.Property(x => x.Saldo).HasColumnType("numeric(15,2)");
        b.Property(x => x.TasaInteres).HasColumnType("numeric(6,2)");
        b.Property(x => x.InteresesPorPagar).HasColumnType("numeric(15,2)");
        b.Property(x => x.TasaInteresComision).HasColumnType("numeric(6,2)");
        b.Property(x => x.ValorComision).HasColumnType("numeric(15,2)");
        b.Property(x => x.CodigoPeriodicidadPago).HasMaxLength(2).IsRequired();
        b.Property(x => x.CodigoClase).HasMaxLength(1).IsRequired();
        b.Property(x => x.ValorVencido).HasColumnType("numeric(15,2)");
        b.Property(x => x.CodigoFormaCancelacion).HasMaxLength(1);
        b.Property(x => x.NumeroObligacionAnterior).HasMaxLength(15);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PaisAcreedor).WithMany().HasForeignKey(x => x.CodigoPaisAcreedor).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.IdCuentaContable).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PeriodicidadPago).WithMany().HasForeignKey(x => x.CodigoPeriodicidadPago).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Clase).WithMany().HasForeignKey(x => x.CodigoClase).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FormaCancelacion).WithMany().HasForeignKey(x => x.CodigoFormaCancelacion).OnDelete(DeleteBehavior.Restrict);

        // Control real de duplicados del manual (§4): mismo tipo+identificación
        // de acreedor + número de obligación + cuenta contable no puede
        // repetirse — ahora también como restricción real de base de datos
        // (antes solo se validaba en el servicio; la sincronización real
        // Softbank→Corela15, que escribe directo, también necesita esta
        // garantía para poder hacer upsert sin duplicar filas).
        b.HasIndex(x => new { x.TipoIdentificacionAcreedor, x.IdentificacionAcreedor, x.NumeroObligacion, x.IdCuentaContable }).IsUnique();
    }
}

public class EstadoObligacionFinancieraConfiguration : IEntityTypeConfiguration<EstadoObligacionFinanciera>
{
    public void Configure(EntityTypeBuilder<EstadoObligacionFinanciera> b)
    {
        b.ToTable("estado_obligacion_financiera", "obligacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class PeriodicidadPagoConfiguration : IEntityTypeConfiguration<PeriodicidadPago>
{
    public void Configure(EntityTypeBuilder<PeriodicidadPago> b)
    {
        b.ToTable("periodicidad_pago", "obligacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class ClaseObligacionFinancieraConfiguration : IEntityTypeConfiguration<ClaseObligacionFinanciera>
{
    public void Configure(EntityTypeBuilder<ClaseObligacionFinanciera> b)
    {
        b.ToTable("clase_obligacion_financiera", "obligacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(1);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class FormaCancelacionObligacionConfiguration : IEntityTypeConfiguration<FormaCancelacionObligacion>
{
    public void Configure(EntityTypeBuilder<FormaCancelacionObligacion> b)
    {
        b.ToTable("forma_cancelacion_obligacion", "obligacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(1);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class CuentaPorCobrarConfiguration : IEntityTypeConfiguration<CuentaPorCobrar>
{
    public void Configure(EntityTypeBuilder<CuentaPorCobrar> b)
    {
        b.ToTable("cuenta_por_cobrar", "cuentasporcobrar");
        b.HasKey(x => x.Id);
        b.Property(x => x.Concepto).HasMaxLength(300).IsRequired();
        b.Property(x => x.MontoInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);

        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class CuentaPorPagarConfiguration : IEntityTypeConfiguration<CuentaPorPagar>
{
    public void Configure(EntityTypeBuilder<CuentaPorPagar> b)
    {
        b.ToTable("cuenta_por_pagar", "cuentasporcobrar");
        b.HasKey(x => x.Id);
        b.Property(x => x.Concepto).HasMaxLength(300).IsRequired();
        b.Property(x => x.MontoInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);

        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}


