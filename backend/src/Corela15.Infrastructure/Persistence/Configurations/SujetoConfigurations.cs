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
        b.Property(x => x.CodigoProvinciaDomicilio).HasMaxLength(2);
        b.HasOne(x => x.ProvinciaDomicilio).WithMany().HasForeignKey(x => x.CodigoProvinciaDomicilio)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActividadEconomica).WithMany().HasForeignKey(x => x.IdActividadEconomica)
            .OnDelete(DeleteBehavior.Restrict);

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

        b.Property(x => x.CodigoEstadoCivil).HasMaxLength(2);
        b.HasOne(x => x.EstadoCivil).WithMany().HasForeignKey(x => x.CodigoEstadoCivil).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CodigoEducacion).HasMaxLength(2);
        b.HasOne(x => x.Educacion).WithMany().HasForeignKey(x => x.CodigoEducacion).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CodigoVivienda).HasMaxLength(2);
        b.HasOne(x => x.Vivienda).WithMany().HasForeignKey(x => x.CodigoVivienda).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CodigoSectorVivienda).HasMaxLength(2);
        b.HasOne(x => x.SectorVivienda).WithMany().HasForeignKey(x => x.CodigoSectorVivienda).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CodigoNacionalidad).HasMaxLength(3);
        b.HasOne(x => x.Nacionalidad).WithMany().HasForeignKey(x => x.CodigoNacionalidad).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.CodigoProfesion).HasMaxLength(20);
        b.HasOne(x => x.Profesion).WithMany().HasForeignKey(x => x.CodigoProfesion).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EstadoCivilConfiguration : IEntityTypeConfiguration<EstadoCivil>
{
    public void Configure(EntityTypeBuilder<EstadoCivil> b)
    {
        b.ToTable("estado_civil", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class EducacionConfiguration : IEntityTypeConfiguration<Educacion>
{
    public void Configure(EntityTypeBuilder<Educacion> b)
    {
        b.ToTable("educacion", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class ViviendaConfiguration : IEntityTypeConfiguration<Vivienda>
{
    public void Configure(EntityTypeBuilder<Vivienda> b)
    {
        b.ToTable("vivienda", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class SectorViviendaConfiguration : IEntityTypeConfiguration<SectorVivienda>
{
    public void Configure(EntityTypeBuilder<SectorVivienda> b)
    {
        b.ToTable("sector_vivienda", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class NacionalidadConfiguration : IEntityTypeConfiguration<Nacionalidad>
{
    public void Configure(EntityTypeBuilder<Nacionalidad> b)
    {
        b.ToTable("nacionalidad", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(3);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class ConyugeConfiguration : IEntityTypeConfiguration<Conyuge>
{
    public void Configure(EntityTypeBuilder<Conyuge> b)
    {
        b.ToTable("conyuge", "sujeto");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.PersonaNatural).WithMany().HasForeignKey(x => x.IdPersonaNatural).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.PersonaConyuge).WithMany().HasForeignKey(x => x.IdPersonaConyuge).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RepresentanteConfiguration : IEntityTypeConfiguration<Representante>
{
    public void Configure(EntityTypeBuilder<Representante> b)
    {
        b.ToTable("representante", "sujeto");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.PersonaRepresentante).WithMany().HasForeignKey(x => x.IdPersonaRepresentante).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PersonaTelefonoConfiguration : IEntityTypeConfiguration<PersonaTelefono>
{
    public void Configure(EntityTypeBuilder<PersonaTelefono> b)
    {
        b.ToTable("persona_telefono", "sujeto");
        b.HasKey(x => x.Id);
        b.Property(x => x.Telefono).HasMaxLength(30).IsRequired();

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Cascade);
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

public class ProfesionConfiguration : IEntityTypeConfiguration<Profesion>
{
    public void Configure(EntityTypeBuilder<Profesion> b)
    {
        b.ToTable("profesion", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
    }
}

public class CanalReclamoConfiguration : IEntityTypeConfiguration<CanalReclamo>
{
    public void Configure(EntityTypeBuilder<CanalReclamo> b)
    {
        b.ToTable("canal_reclamo", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class EstadoReclamoConfiguration : IEntityTypeConfiguration<EstadoReclamo>
{
    public void Configure(EntityTypeBuilder<EstadoReclamo> b)
    {
        b.ToTable("estado_reclamo", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class TipoResolucionReclamoConfiguration : IEntityTypeConfiguration<TipoResolucionReclamo>
{
    public void Configure(EntityTypeBuilder<TipoResolucionReclamo> b)
    {
        b.ToTable("tipo_resolucion_reclamo", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class TipoProductoReclamoConfiguration : IEntityTypeConfiguration<TipoProductoReclamo>
{
    public void Configure(EntityTypeBuilder<TipoProductoReclamo> b)
    {
        b.ToTable("tipo_producto_reclamo", "sujeto");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class ConceptoReclamoConfiguration : IEntityTypeConfiguration<ConceptoReclamo>
{
    public void Configure(EntityTypeBuilder<ConceptoReclamo> b)
    {
        b.ToTable("concepto_reclamo", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
    }
}

public class ConceptoReclamoDetalleConfiguration : IEntityTypeConfiguration<ConceptoReclamoDetalle>
{
    public void Configure(EntityTypeBuilder<ConceptoReclamoDetalle> b)
    {
        b.ToTable("concepto_reclamo_detalle", "sujeto");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(4);
        b.Property(x => x.CodigoConcepto).HasMaxLength(2).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(400).IsRequired();

        b.HasOne(x => x.Concepto).WithMany().HasForeignKey(x => x.CodigoConcepto).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReclamoConfiguration : IEntityTypeConfiguration<Reclamo>
{
    public void Configure(EntityTypeBuilder<Reclamo> b)
    {
        b.ToTable("reclamo", "sujeto");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoCanalRecepcion).HasMaxLength(2).IsRequired();
        b.Property(x => x.CodigoConceptoDetalle).HasMaxLength(4).IsRequired();
        b.Property(x => x.CodigoEstado).HasMaxLength(2).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(2000).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CanalRecepcion).WithMany().HasForeignKey(x => x.CodigoCanalRecepcion).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoProducto).WithMany().HasForeignKey(x => x.IdTipoProducto).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ConceptoDetalle).WithMany().HasForeignKey(x => x.CodigoConceptoDetalle).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Respuesta).WithOne(x => x.Reclamo).HasForeignKey<ReclamoRespuesta>(x => x.IdReclamo)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReclamoRespuestaConfiguration : IEntityTypeConfiguration<ReclamoRespuesta>
{
    public void Configure(EntityTypeBuilder<ReclamoRespuesta> b)
    {
        b.ToTable("reclamo_respuesta", "sujeto");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoTipoResolucion).HasMaxLength(2).IsRequired();
        b.Property(x => x.MontoRestituido).HasColumnType("numeric(18,2)");
        b.Property(x => x.InteresSobreMonto).HasColumnType("numeric(18,2)");
        b.Property(x => x.Descripcion).HasMaxLength(2000).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.TipoResolucion).WithMany().HasForeignKey(x => x.CodigoTipoResolucion).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdReclamo).IsUnique();
    }
}

public class MiembroOrganoGobiernoConfiguration : IEntityTypeConfiguration<MiembroOrganoGobierno>
{
    public void Configure(EntityTypeBuilder<MiembroOrganoGobierno> b)
    {
        b.ToTable("miembro_organo_gobierno", "sujeto");
        b.HasKey(x => x.Id);
        b.Property(x => x.RegistradoPor).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
    }
}
