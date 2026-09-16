using Corela15.Domain.Documentos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class DocumentoConfiguration : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> b)
    {
        b.ToTable("documento", "documentos");
        b.HasKey(x => x.Id);

        b.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
        b.Property(x => x.Area).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Version).HasMaxLength(20).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.NombreArchivoOriginal).HasMaxLength(255).IsRequired();
        b.Property(x => x.RutaAlmacenamiento).HasMaxLength(500).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        b.Property(x => x.InstanciaAprobacion).HasMaxLength(200);
        b.Property(x => x.InstanciaRevision).HasMaxLength(200);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasIndex(x => x.Area);
        b.HasIndex(x => x.Tipo);
        b.HasIndex(x => x.Estado);
        b.HasIndex(x => x.Activo);

        // Dos ediciones simultáneas del mismo documento no deben pisarse.
        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}
