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

public class TipoTransaccionCuentaProductoConfiguration : IEntityTypeConfiguration<TipoTransaccionCuentaProducto>
{
    public void Configure(EntityTypeBuilder<TipoTransaccionCuentaProducto> b)
    {
        b.ToTable("tipo_transaccion_cuenta_producto", "contabilidad");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.TipoTransaccion).WithMany().HasForeignKey(x => x.IdTipoTransaccion)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.TipoCuenta).WithMany().HasForeignKey(x => x.IdTipoCuenta)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.CuentaContableDebito).WithMany().HasForeignKey(x => x.IdCuentaContableDebito)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CuentaContableCredito).WithMany().HasForeignKey(x => x.IdCuentaContableCredito)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdTipoTransaccion, x.IdTipoCuenta }).IsUnique();
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

public class FormaCancelacionConfiguration : IEntityTypeConfiguration<FormaCancelacion>
{
    public void Configure(EntityTypeBuilder<FormaCancelacion> b)
    {
        b.ToTable("forma_cancelacion", "contabilidad");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(5);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> b)
    {
        b.ToTable("proveedor", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Identificacion).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
        b.Property(x => x.Direccion).HasMaxLength(300);
        b.Property(x => x.Telefono).HasMaxLength(40);
        b.Property(x => x.Email).HasMaxLength(200);

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoIdentificacion).WithMany().HasForeignKey(x => x.IdTipoIdentificacion).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Identificacion).IsUnique();
    }
}

public class TipoComprobanteCompraConfiguration : IEntityTypeConfiguration<TipoComprobanteCompra>
{
    public void Configure(EntityTypeBuilder<TipoComprobanteCompra> b)
    {
        b.ToTable("tipo_comprobante_compra", "contabilidad");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(5);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> b)
    {
        b.ToTable("compra", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(15).IsRequired();
        b.Property(x => x.CodigoSustento).HasMaxLength(5).IsRequired();
        b.Property(x => x.CodigoTipoComprobante).HasMaxLength(5).IsRequired();
        b.Property(x => x.Establecimiento).HasMaxLength(12).IsRequired();
        b.Property(x => x.PuntoEmision).HasMaxLength(12).IsRequired();
        b.Property(x => x.Secuencial).HasMaxLength(18).IsRequired();
        b.Property(x => x.Autorizacion).HasMaxLength(60).IsRequired();
        b.Property(x => x.Concepto).HasMaxLength(500).IsRequired();
        b.Property(x => x.Subtotal).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoIva).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoRetencion).HasColumnType("numeric(18,2)");
        b.Property(x => x.Total).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Proveedor).WithMany().HasForeignKey(x => x.IdProveedor).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoComprobante).WithMany().HasForeignKey(x => x.CodigoTipoComprobante).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x => x.Detalle).WithOne(x => x.Compra).HasForeignKey(x => x.IdCompra).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.Numero).IsUnique();
    }
}

public class CompraDetalleConfiguration : IEntityTypeConfiguration<CompraDetalle>
{
    public void Configure(EntityTypeBuilder<CompraDetalle> b)
    {
        b.ToTable("compra_detalle", "contabilidad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(500).IsRequired();
        b.Property(x => x.Cantidad).HasColumnType("numeric(18,4)");
        b.Property(x => x.ValorUnitario).HasColumnType("numeric(18,4)");
        b.Property(x => x.PorcentajeIva).HasColumnType("numeric(6,4)");
        b.Property(x => x.Subtotal).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoIva).HasColumnType("numeric(18,2)");
        b.Property(x => x.Total).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.IdCuentaContable).OnDelete(DeleteBehavior.Restrict);
    }
}
