using Corela15.Domain.Comunicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class CanalConfiguration : IEntityTypeConfiguration<Canal>
{
    public void Configure(EntityTypeBuilder<Canal> b)
    {
        b.ToTable("canal", "comunicacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100);
        b.Property(x => x.Descripcion).HasMaxLength(300);
        b.Property(x => x.ClaveDirecta).HasMaxLength(80);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();

        b.HasIndex(x => x.ClaveDirecta).IsUnique();
    }
}

public class CanalMiembroConfiguration : IEntityTypeConfiguration<CanalMiembro>
{
    public void Configure(EntityTypeBuilder<CanalMiembro> b)
    {
        b.ToTable("canal_miembro", "comunicacion");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Canal).WithMany().HasForeignKey(x => x.IdCanal).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdCanal, x.IdUsuario }).IsUnique();
        b.HasIndex(x => x.IdUsuario);
    }
}

public class MensajeConfiguration : IEntityTypeConfiguration<Mensaje>
{
    public void Configure(EntityTypeBuilder<Mensaje> b)
    {
        b.ToTable("mensaje", "comunicacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Texto).HasMaxLength(2000);
        b.Property(x => x.RutaAdjunto).HasMaxLength(500);
        b.Property(x => x.NombreArchivoAdjunto).HasMaxLength(255);
        b.Property(x => x.ContentTypeAdjunto).HasMaxLength(150);

        b.HasOne(x => x.Canal).WithMany().HasForeignKey(x => x.IdCanal).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.UsuarioRemitente).WithMany().HasForeignKey(x => x.IdUsuarioRemitente).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdCanal, x.CreadoEn });
    }
}
