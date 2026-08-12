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
        b.Property(x => x.PerfilComportamiento).HasColumnType("numeric(9,4)");
        b.Property(x => x.PerfilTransaccional).HasColumnType("numeric(9,4)");
        b.Property(x => x.TotalPerfil).HasColumnType("numeric(9,4)");

        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdCliente, x.Fecha });
    }
}
