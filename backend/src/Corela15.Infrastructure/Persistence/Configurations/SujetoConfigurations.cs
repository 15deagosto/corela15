using Corela15.Domain.Sujeto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> b)
    {
        b.ToTable("persona", "sujeto");
        b.HasKey(x => x.Id);
        b.Property(x => x.Identificacion).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Activos).HasColumnType("numeric(18,2)");
        b.Property(x => x.Pasivos).HasColumnType("numeric(18,2)");
        b.Property(x => x.Ingresos).HasColumnType("numeric(18,2)");
        b.Property(x => x.Egresos).HasColumnType("numeric(18,2)");
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.TipoIdentificacion).WithMany().HasForeignKey(x => x.IdTipoIdentificacion)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Pais).WithMany().HasForeignKey(x => x.IdPais).OnDelete(DeleteBehavior.Restrict);

        // Una identificación (cédula/RUC/pasaporte) es única por tipo — evita duplicar la misma persona dos veces.
        b.HasIndex(x => new { x.IdTipoIdentificacion, x.Identificacion }).IsUnique();
    }
}

public class PersonaNaturalConfiguration : IEntityTypeConfiguration<PersonaNatural>
{
    public void Configure(EntityTypeBuilder<PersonaNatural> b)
    {
        b.ToTable("persona_natural", "sujeto");
        b.HasKey(x => x.IdPersona);
        b.Property(x => x.PrimerNombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.SegundoNombre).HasMaxLength(100);
        b.Property(x => x.ApellidoPaterno).HasMaxLength(100).IsRequired();
        b.Property(x => x.ApellidoMaterno).HasMaxLength(100);

        b.HasOne(x => x.Persona).WithOne(x => x.PersonaNatural)
            .HasForeignKey<PersonaNatural>(x => x.IdPersona).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PersonaJuridicaConfiguration : IEntityTypeConfiguration<PersonaJuridica>
{
    public void Configure(EntityTypeBuilder<PersonaJuridica> b)
    {
        b.ToTable("persona_juridica", "sujeto");
        b.HasKey(x => x.IdPersona);
        b.Property(x => x.RazonSocial).HasMaxLength(300).IsRequired();

        b.HasOne(x => x.Persona).WithOne(x => x.PersonaJuridica)
            .HasForeignKey<PersonaJuridica>(x => x.IdPersona).OnDelete(DeleteBehavior.Cascade);
    }
}
