using Corela15.Domain.Reporteria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class TableroConfiguration : IEntityTypeConfiguration<Tablero>
{
    public void Configure(EntityTypeBuilder<Tablero> b)
    {
        b.ToTable("tablero", "reporteria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(500);
        b.Property(x => x.Definicion).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.Propietario).HasMaxLength(50).IsRequired();
        b.Property(x => x.RolesPermitidos).HasColumnType("text[]");
        b.HasIndex(x => x.Propietario);
    }
}

public class FavoritoTableroConfiguration : IEntityTypeConfiguration<FavoritoTablero>
{
    public void Configure(EntityTypeBuilder<FavoritoTablero> b)
    {
        b.ToTable("favorito_tablero", "reporteria");
        b.HasKey(x => new { x.Usuario, x.IdTablero });
        b.Property(x => x.Usuario).HasMaxLength(50);
        b.HasOne(x => x.Tablero).WithMany().HasForeignKey(x => x.IdTablero).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditoriaConsultaReporteriaConfiguration : IEntityTypeConfiguration<AuditoriaConsultaReporteria>
{
    public void Configure(EntityTypeBuilder<AuditoriaConsultaReporteria> b)
    {
        b.ToTable("auditoria_consulta", "reporteria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityAlwaysColumn();
        b.Property(x => x.Usuario).HasMaxLength(50).IsRequired();
        b.Property(x => x.Dataset).HasMaxLength(30).IsRequired();
        b.Property(x => x.Error).HasMaxLength(1000);
        b.HasIndex(x => x.FechaHora);
        b.HasIndex(x => x.Usuario);
    }
}
