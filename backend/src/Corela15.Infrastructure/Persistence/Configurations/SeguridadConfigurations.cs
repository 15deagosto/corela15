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
        b.Property(x => x.NombreCompleto).HasMaxLength(200);
        b.Property(x => x.Email).HasMaxLength(150);
        b.Property(x => x.CodigoUsuarioSoftbank).HasMaxLength(50);
        b.HasIndex(x => x.CodigoUsuarioSoftbank);
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

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> b)
    {
        b.ToTable("menu", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class RolMenuConfiguration : IEntityTypeConfiguration<RolMenu>
{
    public void Configure(EntityTypeBuilder<RolMenu> b)
    {
        b.ToTable("rol_menu", "seguridad");
        b.HasKey(x => new { x.IdRol, x.IdMenu });

        b.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Menu).WithMany().HasForeignKey(x => x.IdMenu).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TipoEstructuraConfiguration : IEntityTypeConfiguration<TipoEstructura>
{
    public void Configure(EntityTypeBuilder<TipoEstructura> b)
    {
        b.ToTable("tipo_estructura", "seguridad");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
    }
}

public class RolTipoEstructuraConfiguration : IEntityTypeConfiguration<RolTipoEstructura>
{
    public void Configure(EntityTypeBuilder<RolTipoEstructura> b)
    {
        b.ToTable("rol_tipo_estructura", "seguridad");
        b.HasKey(x => new { x.IdRol, x.CodigoTipoEstructura });

        b.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.TipoEstructura).WithMany().HasForeignKey(x => x.CodigoTipoEstructura).OnDelete(DeleteBehavior.Cascade);
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

public class SesionUsuarioConfiguration : IEntityTypeConfiguration<SesionUsuario>
{
    public void Configure(EntityTypeBuilder<SesionUsuario> b)
    {
        b.ToTable("sesion_usuario", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.DireccionIp).HasMaxLength(45);
        b.Property(x => x.RevocadaPor).HasMaxLength(100);

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.IdUsuario, x.Revocada });
    }
}

public class AccionUsuarioConfiguration : IEntityTypeConfiguration<AccionUsuario>
{
    public void Configure(EntityTypeBuilder<AccionUsuario> b)
    {
        b.ToTable("accion_usuario", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoAccion).HasMaxLength(20).IsRequired();
        b.Property(x => x.UsuarioRegistro).HasMaxLength(100).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();

        b.HasIndex(x => x.Fecha);
    }
}

public class SolicitudIdempotenteConfiguration : IEntityTypeConfiguration<SolicitudIdempotente>
{
    public void Configure(EntityTypeBuilder<SolicitudIdempotente> b)
    {
        b.ToTable("solicitud_idempotente", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.Clave).HasMaxLength(200).IsRequired();
        b.Property(x => x.Ruta).HasMaxLength(300).IsRequired();
        b.Property(x => x.CuerpoRespuesta).HasColumnType("jsonb").IsRequired();

        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.Clave, x.IdUsuario, x.Ruta }).IsUnique();
    }
}

public class HorarioAccesoUsuarioConfiguration : IEntityTypeConfiguration<HorarioAccesoUsuario>
{
    public void Configure(EntityTypeBuilder<HorarioAccesoUsuario> b)
    {
        b.ToTable("horario_acceso_usuario", "seguridad");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.IdUsuario, x.DiaSemana });
    }
}

public class UsuarioRolTemporalConfiguration : IEntityTypeConfiguration<UsuarioRolTemporal>
{
    public void Configure(EntityTypeBuilder<UsuarioRolTemporal> b)
    {
        b.ToTable("usuario_rol_temporal", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.Activo });
    }
}

public class UsuarioAgenciaTemporalConfiguration : IEntityTypeConfiguration<UsuarioAgenciaTemporal>
{
    public void Configure(EntityTypeBuilder<UsuarioAgenciaTemporal> b)
    {
        b.ToTable("usuario_agencia_temporal", "seguridad");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.AgenciaOrigen).WithMany().HasForeignKey(x => x.IdAgenciaOrigen).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AgenciaActual).WithMany().HasForeignKey(x => x.IdAgenciaActual).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.Activo });
    }
}
