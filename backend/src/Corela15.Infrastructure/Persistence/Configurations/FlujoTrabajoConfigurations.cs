using Corela15.Domain.FlujoTrabajo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class TipoEtapaConfiguration : IEntityTypeConfiguration<TipoEtapa>
{
    public void Configure(EntityTypeBuilder<TipoEtapa> b)
    {
        b.ToTable("tipo_etapa", "flujotrabajo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class EtapaConfiguration : IEntityTypeConfiguration<Etapa>
{
    public void Configure(EntityTypeBuilder<Etapa> b)
    {
        b.ToTable("etapa", "flujotrabajo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.TipoEtapa).WithMany().HasForeignKey(x => x.IdTipoEtapa).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EtapaRetornoConfiguration : IEntityTypeConfiguration<EtapaRetorno>
{
    public void Configure(EntityTypeBuilder<EtapaRetorno> b)
    {
        b.ToTable("etapa_retorno", "flujotrabajo");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Etapa).WithMany().HasForeignKey(x => x.IdEtapa).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EtapaDestino).WithMany().HasForeignKey(x => x.IdEtapaRetorno).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GrupoContableConfiguration : IEntityTypeConfiguration<GrupoContable>
{
    public void Configure(EntityTypeBuilder<GrupoContable> b)
    {
        b.ToTable("grupo_contable", "flujotrabajo");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(10);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.MontoMinimo).HasColumnType("numeric(18,2)");
        b.Property(x => x.MontoMaximo).HasColumnType("numeric(18,2)");
    }
}

public class GrupoContableUsuarioConfiguration : IEntityTypeConfiguration<GrupoContableUsuario>
{
    public void Configure(EntityTypeBuilder<GrupoContableUsuario> b)
    {
        b.ToTable("grupo_contable_usuario", "flujotrabajo");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.GrupoContable).WithMany().HasForeignKey(x => x.CodigoGrupoContable).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EtapaGrupoContableConfiguration : IEntityTypeConfiguration<EtapaGrupoContable>
{
    public void Configure(EntityTypeBuilder<EtapaGrupoContable> b)
    {
        b.ToTable("etapa_grupo_contable", "flujotrabajo");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Etapa).WithMany().HasForeignKey(x => x.IdEtapa).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GrupoContable).WithMany().HasForeignKey(x => x.CodigoGrupoContable).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdEtapa, x.IdAgencia }).IsUnique();
    }
}
