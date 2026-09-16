using Corela15.Domain.Mensajeria;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class MensajeWhatsappConfiguration : IEntityTypeConfiguration<MensajeWhatsapp>
{
    public void Configure(EntityTypeBuilder<MensajeWhatsapp> b)
    {
        b.ToTable("mensaje_whatsapp", "mensajeria");
        b.HasKey(x => x.Id);
        b.Property(x => x.NumeroDestino).HasMaxLength(20).IsRequired();
        b.Property(x => x.Texto).HasMaxLength(1024).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.IdMensajeExterno).HasMaxLength(100);
        b.Property(x => x.DetalleError).HasMaxLength(500);
        b.Property(x => x.EnviadoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.PersonaDestino).WithMany().HasForeignKey(x => x.IdPersonaDestino).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.NumeroDestino);
        b.HasIndex(x => x.CreadoEn);
    }
}
