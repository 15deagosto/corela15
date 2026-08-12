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
        b.Property(x => x.Cargo).HasMaxLength(100).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Persona).WithMany().HasForeignKey(x => x.IdPersona).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdPersona).IsUnique();
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
