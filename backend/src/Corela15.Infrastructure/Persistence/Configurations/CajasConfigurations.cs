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
