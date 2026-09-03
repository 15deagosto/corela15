using Corela15.Domain.Cumplimiento;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class EstadoHallazgoConfiguration : IEntityTypeConfiguration<EstadoHallazgo>
{
    public void Configure(EntityTypeBuilder<EstadoHallazgo> b)
    {
        b.ToTable("estado_hallazgo", "cumplimiento");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(5);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class HallazgoConfiguration : IEntityTypeConfiguration<Hallazgo>
{
    public void Configure(EntityTypeBuilder<Hallazgo> b)
    {
        b.ToTable("hallazgo", "cumplimiento");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(500).IsRequired();
        b.Property(x => x.Detalle).HasMaxLength(2000).IsRequired();
        b.Property(x => x.CodigoEstado).HasMaxLength(5).IsRequired();
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.UsuarioReporta).WithMany().HasForeignKey(x => x.IdUsuarioReporta).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);
    }
}

public class HallazgoUsuarioConfiguration : IEntityTypeConfiguration<HallazgoUsuario>
{
    public void Configure(EntityTypeBuilder<HallazgoUsuario> b)
    {
        b.ToTable("hallazgo_usuario", "cumplimiento");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Hallazgo).WithMany().HasForeignKey(x => x.IdHallazgo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdHallazgo);
    }
}

public class HallazgoUsuarioRespuestaConfiguration : IEntityTypeConfiguration<HallazgoUsuarioRespuesta>
{
    public void Configure(EntityTypeBuilder<HallazgoUsuarioRespuesta> b)
    {
        b.ToTable("hallazgo_usuario_respuesta", "cumplimiento");
        b.HasKey(x => x.Id);
        b.Property(x => x.Respuesta).HasMaxLength(2000).IsRequired();

        b.HasOne(x => x.HallazgoUsuario).WithMany().HasForeignKey(x => x.IdHallazgoUsuario).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.IdHallazgoUsuario);
    }
}

public class HallazgoEtapaConfiguration : IEntityTypeConfiguration<HallazgoEtapa>
{
    public void Configure(EntityTypeBuilder<HallazgoEtapa> b)
    {
        b.ToTable("hallazgo_etapa", "cumplimiento");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstado).HasMaxLength(5).IsRequired();
        b.Property(x => x.Comentario).HasMaxLength(2000);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Hallazgo).WithMany().HasForeignKey(x => x.IdHallazgo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdHallazgo);
    }
}
