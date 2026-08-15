using Corela15.Domain.Contabilidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class CuentaContableConfiguration : IEntityTypeConfiguration<CuentaContable>
{
    public void Configure(EntityTypeBuilder<CuentaContable> b)
    {
        b.ToTable("cuenta_contable", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Grupo).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Naturaleza).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.CuentaPadre).WithMany().HasForeignKey(x => x.IdCuentaPadre)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class TipoComprobanteContableConfiguration : IEntityTypeConfiguration<TipoComprobanteContable>
{
    public void Configure(EntityTypeBuilder<TipoComprobanteContable> b)
    {
        b.ToTable("tipo_comprobante_contable", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ComprobanteContableConfiguration : IEntityTypeConfiguration<ComprobanteContable>
{
    public void Configure(EntityTypeBuilder<ComprobanteContable> b)
    {
        b.ToTable("comprobante_contable", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Descripcion).HasMaxLength(500);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.TipoComprobante).WithMany().HasForeignKey(x => x.IdTipoComprobante)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia)
            .OnDelete(DeleteBehavior.Restrict);

        // Numeración única por tipo de comprobante (igual que talonarios físicos: cada
        // tipo -Ingreso, Egreso, Diario- lleva su propia secuencia, no una global).
        b.HasIndex(x => new { x.IdTipoComprobante, x.Numero }).IsUnique();
    }
}

public class MovimientoComprobanteContableConfiguration : IEntityTypeConfiguration<MovimientoComprobanteContable>
{
    public void Configure(EntityTypeBuilder<MovimientoComprobanteContable> b)
    {
        b.ToTable("movimiento_comprobante_contable", "contabilidad", t => t.HasCheckConstraint(
            "ck_movimiento_debito_o_credito",
            "(debito = 0 OR credito = 0) AND (debito + credito) > 0"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Debito).HasColumnType("numeric(18,2)");
        b.Property(x => x.Credito).HasColumnType("numeric(18,2)");
        b.Property(x => x.Descripcion).HasMaxLength(300);

        b.HasOne(x => x.Comprobante).WithMany(x => x.Movimientos).HasForeignKey(x => x.IdComprobante)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.IdCuentaContable)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdComprobante, x.NumeroLinea }).IsUnique();
    }
}

public class TipoTransaccionConfiguration : IEntityTypeConfiguration<TipoTransaccion>
{
    public void Configure(EntityTypeBuilder<TipoTransaccion> b)
    {
        b.ToTable("tipo_transaccion", "contabilidad", t => t.HasCheckConstraint(
            "ck_tipo_transaccion_signo", "signo_saldo_cuenta IN (-1, 1)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();

        b.HasOne(x => x.CuentaContableDebito).WithMany().HasForeignKey(x => x.IdCuentaContableDebito)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CuentaContableCredito).WithMany().HasForeignKey(x => x.IdCuentaContableCredito)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoComprobante).WithMany().HasForeignKey(x => x.IdTipoComprobante)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class SaldoContableConfiguration : IEntityTypeConfiguration<SaldoContable>
{
    public void Configure(EntityTypeBuilder<SaldoContable> b)
    {
        b.ToTable("saldo_contable", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.TotalDebitos).HasColumnType("numeric(18,2)");
        b.Property(x => x.TotalCreditos).HasColumnType("numeric(18,2)");
        b.Property(x => x.SaldoFinal).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.IdCuentaContable)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdCuentaContable, x.Periodo }).IsUnique();

        // El punto de mayor contención real del sistema: todo comprobante
        // toca esta fila. ComprobanteContableService reintenta ante
        // conflicto (ver RegistrarAsync) en vez de burbujear un 409 al
        // usuario por una colisión esperada bajo carga concurrente normal.
        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class PeriodoContableConfiguration : IEntityTypeConfiguration<PeriodoContable>
{
    public void Configure(EntityTypeBuilder<PeriodoContable> b)
    {
        b.ToTable("periodo_contable", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.CerradoPor).HasMaxLength(100);
        b.HasIndex(x => x.Periodo).IsUnique();
    }
}

public class CierreEjercicioConfiguration : IEntityTypeConfiguration<CierreEjercicio>
{
    public void Configure(EntityTypeBuilder<CierreEjercicio> b)
    {
        b.ToTable("cierre_ejercicio", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.TotalIngresos).HasColumnType("numeric(18,2)");
        b.Property(x => x.TotalGastos).HasColumnType("numeric(18,2)");
        b.Property(x => x.Utilidad).HasColumnType("numeric(18,2)");
        b.Property(x => x.CerradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.ComprobanteContable).WithMany().HasForeignKey(x => x.IdComprobanteContable)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Anio).IsUnique();
    }
}
