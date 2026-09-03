using Corela15.Domain.Cajas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class DenominacionConfiguration : IEntityTypeConfiguration<Denominacion>
{
    public void Configure(EntityTypeBuilder<Denominacion> b)
    {
        b.ToTable("denominacion", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(10);
        b.Property(x => x.Valor).HasColumnType("numeric(9,2)");
        b.HasIndex(x => new { x.Tipo, x.Valor }).IsUnique();
    }
}

public class VentanillaConfiguration : IEntityTypeConfiguration<Ventanilla>
{
    public void Configure(EntityTypeBuilder<Ventanilla> b)
    {
        b.ToTable("ventanilla", "cajas");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdUsuario, x.Fecha }).IsUnique();

        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class VentanillaCuadreConfiguration : IEntityTypeConfiguration<VentanillaCuadre>
{
    public void Configure(EntityTypeBuilder<VentanillaCuadre> b)
    {
        b.ToTable("ventanilla_cuadre", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.TotalEfectivo).HasColumnType("numeric(18,2)");
        b.Property(x => x.TotalCheque).HasColumnType("numeric(18,2)");
        b.Property(x => x.Total).HasColumnType("numeric(18,2)");
        b.Property(x => x.DiferenciaEfectivo).HasColumnType("numeric(18,2)");
        b.Property(x => x.DiferenciaCheque).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Ventanilla).WithMany().HasForeignKey(x => x.IdVentanilla).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.IdVentanilla);
    }
}

public class ItemCajaConfiguration : IEntityTypeConfiguration<ItemCaja>
{
    public void Configure(EntityTypeBuilder<ItemCaja> b)
    {
        b.ToTable("item_caja", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class VentanillaItemCajaConfiguration : IEntityTypeConfiguration<VentanillaItemCaja>
{
    public void Configure(EntityTypeBuilder<VentanillaItemCaja> b)
    {
        b.ToTable("ventanilla_item_caja", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.SaldoCuadre).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Ventanilla).WithMany().HasForeignKey(x => x.IdVentanilla).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ItemCaja).WithMany().HasForeignKey(x => x.IdItemCaja).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Moneda).WithMany().HasForeignKey(x => x.IdMoneda).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdVentanilla, x.IdItemCaja, x.IdMoneda }).IsUnique();
    }
}

public class VentanillaItemCajaMovimientoConfiguration : IEntityTypeConfiguration<VentanillaItemCajaMovimiento>
{
    public void Configure(EntityTypeBuilder<VentanillaItemCajaMovimiento> b)
    {
        b.ToTable("ventanilla_item_caja_movimiento", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Valor).HasColumnType("numeric(18,2)");
        b.Property(x => x.SaldoResultante).HasColumnType("numeric(18,2)");
        b.Property(x => x.Descripcion).HasMaxLength(300).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.VentanillaItemCaja).WithMany().HasForeignKey(x => x.IdVentanillaItemCaja).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ComprobanteContable).WithMany().HasForeignKey(x => x.IdComprobanteContable).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdVentanillaItemCaja);
    }
}

public class ItemBovedaConfiguration : IEntityTypeConfiguration<ItemBoveda>
{
    public void Configure(EntityTypeBuilder<ItemBoveda> b)
    {
        b.ToTable("item_boveda", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class BovedaConfiguration : IEntityTypeConfiguration<Boveda>
{
    public void Configure(EntityTypeBuilder<Boveda> b)
    {
        b.ToTable("boveda", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExistenciaMinima).HasColumnType("numeric(18,2)");
        b.Property(x => x.ExistenciaMaxima).HasColumnType("numeric(18,2)");
        b.Property(x => x.ExistenciaMinimaCaja).HasColumnType("numeric(18,2)");
        b.Property(x => x.ExistenciaMaximaCaja).HasColumnType("numeric(18,2)");
        b.Property(x => x.ExistenciaMinimaCajaChica).HasColumnType("numeric(18,2)");
        b.Property(x => x.ExistenciaMaximaCajaChica).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioResponsable).WithMany().HasForeignKey(x => x.IdUsuarioResponsable).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdAgencia).IsUnique();
    }
}

public class BovedaItemBovedaConfiguration : IEntityTypeConfiguration<BovedaItemBoveda>
{
    public void Configure(EntityTypeBuilder<BovedaItemBoveda> b)
    {
        b.ToTable("boveda_item_boveda", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Boveda).WithMany().HasForeignKey(x => x.IdBoveda).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ItemBoveda).WithMany().HasForeignKey(x => x.IdItemBoveda).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Moneda).WithMany().HasForeignKey(x => x.IdMoneda).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdBoveda, x.IdItemBoveda, x.IdMoneda }).IsUnique();
    }
}

public class AutorizacionTransaccionConfiguration : IEntityTypeConfiguration<AutorizacionTransaccion>
{
    public void Configure(EntityTypeBuilder<AutorizacionTransaccion> b)
    {
        b.ToTable("autorizacion_transaccion", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoTipoTransaccion).HasMaxLength(20).IsRequired();
        b.Property(x => x.Monto).HasColumnType("numeric(18,2)");
        b.Property(x => x.Detalle).HasMaxLength(300).IsRequired();
        b.Property(x => x.AutorizadoPor).HasMaxLength(100);
        b.Property(x => x.ComentarioRechazo).HasMaxLength(500);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Cuenta).WithMany().HasForeignKey(x => x.IdCuenta).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Procesado);
    }
}

public class PagoExternoProductoConfiguration : IEntityTypeConfiguration<PagoExternoProducto>
{
    public void Configure(EntityTypeBuilder<PagoExternoProducto> b)
    {
        b.ToTable("pago_externo_producto", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
        b.Property(x => x.TituloReferencia).HasMaxLength(100).IsRequired();
    }
}

public class PagoExternoTransaccionConfiguration : IEntityTypeConfiguration<PagoExternoTransaccion>
{
    public void Configure(EntityTypeBuilder<PagoExternoTransaccion> b)
    {
        b.ToTable("pago_externo_transaccion", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Referencia).HasMaxLength(300).IsRequired();
        b.Property(x => x.Documento).HasMaxLength(200);
        b.Property(x => x.Valor).HasColumnType("numeric(18,2)");
        b.Property(x => x.Comision).HasColumnType("numeric(18,2)");
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ReversadaPor).HasMaxLength(100);

        b.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdProducto).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.FechaProceso);
    }
}

public class TipoFormaNumeradaConfiguration : IEntityTypeConfiguration<TipoFormaNumerada>
{
    public void Configure(EntityTypeBuilder<TipoFormaNumerada> b)
    {
        b.ToTable("tipo_forma_numerada", "cajas");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(10);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class FormaNumeradaConfiguration : IEntityTypeConfiguration<FormaNumerada>
{
    public void Configure(EntityTypeBuilder<FormaNumerada> b)
    {
        b.ToTable("forma_numerada", "cajas");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoTipo).HasMaxLength(10).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Tipo).WithMany().HasForeignKey(x => x.CodigoTipo).OnDelete(DeleteBehavior.Restrict);
    }
}
