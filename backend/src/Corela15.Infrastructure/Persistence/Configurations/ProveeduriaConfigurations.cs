using Corela15.Domain.Proveeduria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class TipoArticuloConfiguration : IEntityTypeConfiguration<TipoArticulo>
{
    public void Configure(EntityTypeBuilder<TipoArticulo> b)
    {
        b.ToTable("tipo_articulo", "proveeduria");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Detalle).HasMaxLength(300);

        b.HasOne(x => x.CuentaContableActivo).WithMany().HasForeignKey(x => x.IdCuentaContableActivo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CuentaContableGasto).WithMany().HasForeignKey(x => x.IdCuentaContableGasto).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ArticuloConfiguration : IEntityTypeConfiguration<Articulo>
{
    public void Configure(EntityTypeBuilder<Articulo> b)
    {
        b.ToTable("articulo", "proveeduria");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Marca).HasMaxLength(200);
        b.Property(x => x.Multiplo).HasMaxLength(50).IsRequired();

        b.HasOne(x => x.TipoArticulo).WithMany().HasForeignKey(x => x.CodigoTipoArticulo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BodegaConfiguration : IEntityTypeConfiguration<Bodega>
{
    public void Configure(EntityTypeBuilder<Bodega> b)
    {
        b.ToTable("bodega", "proveeduria");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioResponsable).WithMany().HasForeignKey(x => x.IdUsuarioResponsable).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BodegaArticuloConfiguration : IEntityTypeConfiguration<BodegaArticulo>
{
    public void Configure(EntityTypeBuilder<BodegaArticulo> b)
    {
        b.ToTable("bodega_articulo", "proveeduria");
        b.HasKey(x => x.Id);
        b.Property(x => x.PrecioUnitario).HasColumnType("numeric(18,4)");
        b.Property(x => x.ValorTotal).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.IdBodega).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Articulo).WithMany().HasForeignKey(x => x.CodigoArticulo).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdBodega, x.CodigoArticulo }).IsUnique();

        // Dos movimientos simultáneos sobre el mismo kardex no deben pisarse.
        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class BodegaArticuloMovimientoConfiguration : IEntityTypeConfiguration<BodegaArticuloMovimiento>
{
    public void Configure(EntityTypeBuilder<BodegaArticuloMovimiento> b)
    {
        b.ToTable("bodega_articulo_movimiento", "proveeduria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Valor).HasColumnType("numeric(18,2)");
        b.Property(x => x.Comentario).HasMaxLength(500);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.BodegaArticulo).WithMany().HasForeignKey(x => x.IdBodegaArticulo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SolicitudPedidoConfiguration : IEntityTypeConfiguration<SolicitudPedido>
{
    public void Configure(EntityTypeBuilder<SolicitudPedido> b)
    {
        b.ToTable("solicitud_pedido", "proveeduria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(500);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Bodega).WithMany().HasForeignKey(x => x.IdBodega).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioSolicitante).WithMany().HasForeignKey(x => x.IdUsuarioSolicitante).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SolicitudPedidoArticuloConfiguration : IEntityTypeConfiguration<SolicitudPedidoArticulo>
{
    public void Configure(EntityTypeBuilder<SolicitudPedidoArticulo> b)
    {
        b.ToTable("solicitud_pedido_articulo", "proveeduria");
        b.HasKey(x => x.Id);
        b.Property(x => x.PrecioUnitario).HasColumnType("numeric(18,4)");
        b.Property(x => x.Detalle).HasMaxLength(300);

        b.HasOne(x => x.Solicitud).WithMany().HasForeignKey(x => x.IdSolicitud).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Articulo).WithMany().HasForeignKey(x => x.CodigoArticulo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SolicitudPedidoEtapaConfiguration : IEntityTypeConfiguration<SolicitudPedidoEtapa>
{
    public void Configure(EntityTypeBuilder<SolicitudPedidoEtapa> b)
    {
        b.ToTable("solicitud_pedido_etapa", "proveeduria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Comentario).HasMaxLength(500);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Solicitud).WithMany().HasForeignKey(x => x.IdSolicitud).OnDelete(DeleteBehavior.Cascade);
    }
}
