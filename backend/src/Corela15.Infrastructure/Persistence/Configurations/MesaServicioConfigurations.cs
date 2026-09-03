using Corela15.Domain.MesaServicio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class CategoriaIncidenciaConfiguration : IEntityTypeConfiguration<CategoriaIncidencia>
{
    public void Configure(EntityTypeBuilder<CategoriaIncidencia> b)
    {
        b.ToTable("categoria_incidencia", "mesaservicio");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class PrioridadTicketConfiguration : IEntityTypeConfiguration<PrioridadTicket>
{
    public void Configure(EntityTypeBuilder<PrioridadTicket> b)
    {
        b.ToTable("prioridad_ticket", "mesaservicio");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class EstadoTicketConfiguration : IEntityTypeConfiguration<EstadoTicket>
{
    public void Configure(EntityTypeBuilder<EstadoTicket> b)
    {
        b.ToTable("estado_ticket", "mesaservicio");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.ToTable("ticket", "mesaservicio");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Numero).IsUnique();
        b.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(4000).IsRequired();
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);
        b.Property(x => x.ComentarioCalificacion).HasMaxLength(1000);

        b.HasOne(x => x.Categoria).WithMany().HasForeignKey(x => x.CodigoCategoria).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Prioridad).WithMany().HasForeignKey(x => x.CodigoPrioridad).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UsuarioAsignado).WithMany().HasForeignKey(x => x.IdUsuarioAsignado).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.CodigoEstado);
        b.HasIndex(x => x.IdUsuarioAsignado);
    }
}

public class TicketComentarioConfiguration : IEntityTypeConfiguration<TicketComentario>
{
    public void Configure(EntityTypeBuilder<TicketComentario> b)
    {
        b.ToTable("ticket_comentario", "mesaservicio");
        b.HasKey(x => x.Id);
        b.Property(x => x.Comentario).HasMaxLength(2000).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.IdTicket).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.IdTicket);
    }
}

public class TicketEtapaHistConfiguration : IEntityTypeConfiguration<TicketEtapaHist>
{
    public void Configure(EntityTypeBuilder<TicketEtapaHist> b)
    {
        b.ToTable("ticket_etapa_hist", "mesaservicio");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstadoAnterior).HasMaxLength(20).IsRequired();
        b.Property(x => x.CodigoEstadoNuevo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Comentario).HasMaxLength(2000);
        b.Property(x => x.RegistradoPor).HasMaxLength(100).IsRequired();

        b.HasOne(x => x.Ticket).WithMany().HasForeignKey(x => x.IdTicket).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => x.IdTicket);
    }
}
