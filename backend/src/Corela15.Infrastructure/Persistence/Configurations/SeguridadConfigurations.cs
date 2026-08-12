using Corela15.Domain.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuario", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.NombreUsuario).HasMaxLength(50).IsRequired();
        b.Property(x => x.HashContrasena).IsRequired();
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.NombreUsuario).IsUnique();
    }
}

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> b)
    {
        b.ToTable("rol", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class UsuarioRolConfiguration : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> b)
    {
        b.ToTable("usuario_rol", "seguridad");
        b.HasKey(x => new { x.IdUsuario, x.IdRol });

        b.HasOne(x => x.Usuario).WithMany(x => x.UsuarioRoles).HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Rol).WithMany(x => x.UsuarioRoles).HasForeignKey(x => x.IdRol)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AccionIngresoUsuarioConfiguration : IEntityTypeConfiguration<AccionIngresoUsuario>
{
    public void Configure(EntityTypeBuilder<AccionIngresoUsuario> b)
    {
        b.ToTable("accion_ingreso_usuario", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).UseIdentityAlwaysColumn();
        b.Property(x => x.DireccionIp).HasMaxLength(45);

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.FechaHora);
    }
}
