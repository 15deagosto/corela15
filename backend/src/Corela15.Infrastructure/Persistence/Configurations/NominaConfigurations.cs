using Corela15.Domain.Nomina;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> b)
    {
        b.ToTable("empleado", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.SueldoActual).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Cargo).WithMany().HasForeignKey(x => x.IdCargo).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdPersona).IsUnique();
    }
}

public class CargoConfiguration : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> b)
    {
        b.ToTable("cargo", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
    }
}

public class ParametroNominaConfiguration : IEntityTypeConfiguration<ParametroNomina>
{
    public void Configure(EntityTypeBuilder<ParametroNomina> b)
    {
        b.ToTable("parametro_nomina", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.SalarioBasicoUnificado).HasColumnType("numeric(18,2)");
    }
}

public class EmpleadoDecimoTerceroConfiguration : IEntityTypeConfiguration<EmpleadoDecimoTercero>
{
    public void Configure(EntityTypeBuilder<EmpleadoDecimoTercero> b)
    {
        b.ToTable("empleado_decimo_tercero", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Proyectado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Acumulado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Pagado).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdEmpleado).IsUnique();
    }
}

public class EmpleadoDecimoCuartoConfiguration : IEntityTypeConfiguration<EmpleadoDecimoCuarto>
{
    public void Configure(EntityTypeBuilder<EmpleadoDecimoCuarto> b)
    {
        b.ToTable("empleado_decimo_cuarto", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Proyectado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Acumulado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Pagado).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdEmpleado).IsUnique();
    }
}

public class EmpleadoFondosReservaConfiguration : IEntityTypeConfiguration<EmpleadoFondosReserva>
{
    public void Configure(EntityTypeBuilder<EmpleadoFondosReserva> b)
    {
        b.ToTable("empleado_fondos_reserva", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Proyectado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Acumulado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Pagado).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdEmpleado).IsUnique();
    }
}

public class EmpleadoProvisionVacacionConfiguration : IEntityTypeConfiguration<EmpleadoProvisionVacacion>
{
    public void Configure(EntityTypeBuilder<EmpleadoProvisionVacacion> b)
    {
        b.ToTable("empleado_provision_vacacion", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Acumulado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Pagado).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdEmpleado).IsUnique();
    }
}

public class EstadoAccionPersonalConfiguration : IEntityTypeConfiguration<EstadoAccionPersonal>
{
    public void Configure(EntityTypeBuilder<EstadoAccionPersonal> b)
    {
        b.ToTable("estado_accion_personal", "nomina");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class TipoAccionPersonalConfiguration : IEntityTypeConfiguration<TipoAccionPersonal>
{
    public void Configure(EntityTypeBuilder<TipoAccionPersonal> b)
    {
        b.ToTable("tipo_accion_personal", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Detalle).HasMaxLength(200).IsRequired();
    }
}

public class SolicitudAccionPersonalConfiguration : IEntityTypeConfiguration<SolicitudAccionPersonal>
{
    public void Configure(EntityTypeBuilder<SolicitudAccionPersonal> b)
    {
        b.ToTable("solicitud_accion_personal", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Detalle).HasMaxLength(800).IsRequired();
        b.Property(x => x.CodigoEstado).HasMaxLength(2).IsRequired();
        b.Property(x => x.NuevoSueldo).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoAccionPersonal).WithMany().HasForeignKey(x => x.IdTipoAccionPersonal).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<EstadoAccionPersonal>().WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Etapas).WithOne(x => x.SolicitudAccionPersonal)
            .HasForeignKey(x => x.IdSolicitudAccionPersonal).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SolicitudAccionPersonalEtapaConfiguration : IEntityTypeConfiguration<SolicitudAccionPersonalEtapa>
{
    public void Configure(EntityTypeBuilder<SolicitudAccionPersonalEtapa> b)
    {
        b.ToTable("solicitud_accion_personal_etapa", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstado).HasMaxLength(2).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(200).IsRequired();
    }
}

public class EmpleadoProvisionVacacionDetalleConfiguration : IEntityTypeConfiguration<EmpleadoProvisionVacacionDetalle>
{
    public void Configure(EntityTypeBuilder<EmpleadoProvisionVacacionDetalle> b)
    {
        b.ToTable("empleado_provision_vacacion_detalle", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.UltimoSueldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Dias).HasColumnType("numeric(9,2)");
        b.Property(x => x.ValorAnteriorProvision).HasColumnType("numeric(18,2)");
        b.Property(x => x.ValorActualProvision).HasColumnType("numeric(18,2)");
        b.Property(x => x.ValorAProvisionar).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.ProvisionVacacion).WithMany().HasForeignKey(x => x.IdProvisionVacacion)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RolPagosConfiguration : IEntityTypeConfiguration<RolPagos>
{
    public void Configure(EntityTypeBuilder<RolPagos> b)
    {
        b.ToTable("rol_pagos", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => new { x.Periodo, x.Tipo }).IsUnique();
    }
}

public class RolPagosEmpleadoConfiguration : IEntityTypeConfiguration<RolPagosEmpleado>
{
    public void Configure(EntityTypeBuilder<RolPagosEmpleado> b)
    {
        b.ToTable("rol_pagos_empleado", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Ingresos).HasColumnType("numeric(18,2)");
        b.Property(x => x.Egresos).HasColumnType("numeric(18,2)");
        b.Property(x => x.Total).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.RolPagos).WithMany(x => x.Empleados).HasForeignKey(x => x.IdRolPagos)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdRolPagos, x.IdEmpleado }).IsUnique();
    }
}

public class TipoContratoConfiguration : IEntityTypeConfiguration<TipoContrato>
{
    public void Configure(EntityTypeBuilder<TipoContrato> b)
    {
        b.ToTable("tipo_contrato", "nomina");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class EmpleadoContratoConfiguration : IEntityTypeConfiguration<EmpleadoContrato>
{
    public void Configure(EntityTypeBuilder<EmpleadoContrato> b)
    {
        b.ToTable("empleado_contrato", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoTipoContrato).HasMaxLength(2).IsRequired();

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoContrato).WithMany().HasForeignKey(x => x.CodigoTipoContrato).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdEmpleado);
    }
}

public class EmpleadoDatosAdicionalesConfiguration : IEntityTypeConfiguration<EmpleadoDatosAdicionales>
{
    public void Configure(EntityTypeBuilder<EmpleadoDatosAdicionales> b)
    {
        b.ToTable("empleado_datos_adicionales", "nomina");
        b.HasKey(x => x.IdEmpleado);
        b.Property(x => x.CodigoIess).HasMaxLength(50);

        b.HasOne(x => x.Empleado).WithOne().HasForeignKey<EmpleadoDatosAdicionales>(x => x.IdEmpleado).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmpleadoAportePatronalConfiguration : IEntityTypeConfiguration<EmpleadoAportePatronal>
{
    public void Configure(EntityTypeBuilder<EmpleadoAportePatronal> b)
    {
        b.ToTable("empleado_aporte_patronal", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Proyectado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Acumulado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Pagado).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdEmpleado).IsUnique();
    }
}

public class TramoImpuestoRentaConfiguration : IEntityTypeConfiguration<TramoImpuestoRenta>
{
    public void Configure(EntityTypeBuilder<TramoImpuestoRenta> b)
    {
        b.ToTable("tramo_impuesto_renta", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.FraccionBasica).HasColumnType("numeric(18,2)");
        b.Property(x => x.ExcesoHasta).HasColumnType("numeric(18,2)");
        b.Property(x => x.ImpuestoFraccionBasica).HasColumnType("numeric(18,2)");
        b.Property(x => x.PorcentajeExcedente).HasColumnType("numeric(5,2)");
    }
}

public class CalculoImpuestoRentaConfiguration : IEntityTypeConfiguration<CalculoImpuestoRenta>
{
    public void Configure(EntityTypeBuilder<CalculoImpuestoRenta> b)
    {
        b.ToTable("calculo_impuesto_renta", "nomina");
        b.HasKey(x => x.Id);
        b.Property(x => x.IngresoAnualProyectado).HasColumnType("numeric(18,2)");
        b.Property(x => x.BaseImponible).HasColumnType("numeric(18,2)");
        b.Property(x => x.ImpuestoCausadoAnual).HasColumnType("numeric(18,2)");
        b.Property(x => x.RetencionMensual).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdEmpleado, x.Anio });
    }
}
