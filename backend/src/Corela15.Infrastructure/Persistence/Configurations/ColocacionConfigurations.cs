using Corela15.Domain.Colocacion;
using Corela15.Domain.Credito;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corela15.Infrastructure.Persistence.Configurations;

public class TipoRubroConfiguration : IEntityTypeConfiguration<TipoRubro>
{
    public void Configure(EntityTypeBuilder<TipoRubro> b)
    {
        b.ToTable("tipo_rubro", "colocacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(20);
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }
}

public class RubroConfiguration : IEntityTypeConfiguration<Rubro>
{
    public void Configure(EntityTypeBuilder<Rubro> b)
    {
        b.ToTable("rubro", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
        b.Property(x => x.TarifaImpuesto).HasColumnType("numeric(9,4)");

        b.HasOne(x => x.TipoRubro).WithMany().HasForeignKey(x => x.CodigoTipoRubro).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrestamoRubroCuentaPorCobrarConfiguration : IEntityTypeConfiguration<PrestamoRubroCuentaPorCobrar>
{
    public void Configure(EntityTypeBuilder<PrestamoRubroCuentaPorCobrar> b)
    {
        b.ToTable("prestamo_rubro_cuenta_por_cobrar", "colocacion");
        b.HasKey(x => x.IdPrestamoRubro);

        b.HasOne(x => x.PrestamoRubro).WithMany().HasForeignKey(x => x.IdPrestamoRubro).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CuentaPorCobrar).WithMany().HasForeignKey(x => x.IdCuentaPorCobrar).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.IdCuentaPorCobrar).IsUnique();
    }
}

public class TipoVencimientoConfiguration : IEntityTypeConfiguration<TipoVencimiento>
{
    public void Configure(EntityTypeBuilder<TipoVencimiento> b)
    {
        b.ToTable("tipo_vencimiento", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class ClasificacionCarteraConfiguration : IEntityTypeConfiguration<ClasificacionCartera>
{
    public void Configure(EntityTypeBuilder<ClasificacionCartera> b)
    {
        b.ToTable("clasificacion_cartera", "colocacion", t => t.HasCheckConstraint(
            "ck_clasificacion_cartera_rango", "dias_fin >= dias_inicio"));
        b.HasKey(x => x.Id);

        b.HasOne(x => x.CuentaContable).WithMany().HasForeignKey(x => x.IdCuentaContable)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoVencimiento).WithMany().HasForeignKey(x => x.IdTipoVencimiento)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CategoriaRiesgoCarteraConfiguration : IEntityTypeConfiguration<CategoriaRiesgoCartera>
{
    public void Configure(EntityTypeBuilder<CategoriaRiesgoCartera> b)
    {
        b.ToTable("categoria_riesgo_cartera", "colocacion", t => t.HasCheckConstraint(
            "ck_categoria_riesgo_cartera_rango", "dias_mora_fin >= dias_mora_inicio"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(5).IsRequired();
        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.PorcentajeProvision).HasColumnType("numeric(9,4)");
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class PrestamoConfiguration : IEntityTypeConfiguration<Prestamo>
{
    public void Configure(EntityTypeBuilder<Prestamo> b)
    {
        b.ToTable("prestamo", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        b.Property(x => x.DeudaInicial).HasColumnType("numeric(18,2)");
        b.Property(x => x.Saldo).HasColumnType("numeric(18,2)");
        b.Property(x => x.Tasa).HasColumnType("numeric(9,4)");
        b.Property(x => x.Tea).HasColumnType("numeric(9,4)");
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);
        b.Property(x => x.CodigoUsuarioAsesor).HasMaxLength(100);
        b.Property(x => x.CodigoTipoConvenio).HasMaxLength(20);

        b.HasOne(x => x.TipoPrestamo).WithMany().HasForeignKey(x => x.IdTipoPrestamo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agencia).WithMany().HasForeignKey(x => x.IdAgencia).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoConvenio).WithMany().HasForeignKey(x => x.CodigoTipoConvenio).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.Numero).IsUnique();

        // Filtro real y directo en reportería (ReporteCreditosMoraAsesor)
        // — sin este índice, filtrar "mi cartera" por asesor recorre la
        // tabla completa. Agregado en la misma ronda que se escribió el
        // primer query real que lo necesita, no especulativo.
        b.HasIndex(x => x.CodigoUsuarioAsesor);

        // Estado gatea casi toda consulta real sobre Prestamo (mora,
        // precancelados, cancelados, calificación, provisión) — mismo
        // criterio: agregado junto con las consultas reales que lo usan.
        b.HasIndex(x => x.Estado);

        // Mismo criterio: agregado junto con el reporte real que agrupa/
        // filtra por convenio (PrestamoPorTipoConvenio).
        b.HasIndex(x => x.CodigoTipoConvenio);

        // Dos pagos de cuota simultáneos sobre el mismo préstamo ya no
        // pueden pisarse — ver nota en AhorrosConfigurations.
        b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion();
    }
}

public class PrestamoClienteConfiguration : IEntityTypeConfiguration<PrestamoCliente>
{
    public void Configure(EntityTypeBuilder<PrestamoCliente> b)
    {
        b.ToTable("prestamo_cliente", "colocacion");
        b.HasKey(x => new { x.IdPrestamo, x.IdCliente });

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrestamoRubroConfiguration : IEntityTypeConfiguration<PrestamoRubro>
{
    public void Configure(EntityTypeBuilder<PrestamoRubro> b)
    {
        b.ToTable("prestamo_rubro", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Proyectado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Calculado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Cobrado).HasColumnType("numeric(18,2)");
        b.Property(x => x.Estado).HasMaxLength(20).IsRequired();

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Rubro).WithMany().HasForeignKey(x => x.IdRubro).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.IdPrestamo, x.NumeroCuota, x.IdRubro }).IsUnique();

        // Cubre exactamente el WHERE real de IMoraCarteraService.
        // CalcularAsync — el cálculo compartido que hoy alimenta 5+
        // consumidores (provisión, cobranza, gasto de cobranza, y los
        // reportes de mora/calificación de Colocación): filtra por
        // IdPrestamo + Estado='P' + FechaFin<hoy, ordenado por FechaFin.
        // Sin este índice compuesto, cada préstamo vigente dispara un
        // scan completo de sus propios rubros en cada corrida.
        b.HasIndex(x => new { x.IdPrestamo, x.Estado, x.FechaFin });

        // Reporte real de abonos por rango de fecha (ReportePorTipoConvenio)
        // filtra por FechaCobro — agregado junto con ese query real.
        b.HasIndex(x => x.FechaCobro);
    }
}

public class PrestamoGarantiaConfiguration : IEntityTypeConfiguration<PrestamoGarantia>
{
    public void Configure(EntityTypeBuilder<PrestamoGarantia> b)
    {
        b.ToTable("prestamo_garantia", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstadoGarantia).HasMaxLength(5).IsRequired();
        b.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
        b.Property(x => x.ModificadoPor).HasMaxLength(100);

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ClienteGarante).WithMany().HasForeignKey(x => x.IdClienteGarante).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EstadoGarantia).WithMany().HasForeignKey(x => x.CodigoEstadoGarantia).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AutoDebitoSpiLogConfiguration : IEntityTypeConfiguration<AutoDebitoSpiLog>
{
    public void Configure(EntityTypeBuilder<AutoDebitoSpiLog> b)
    {
        b.ToTable("auto_debito_spi_log", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Motivo).HasMaxLength(300).IsRequired();
        b.Property(x => x.Monto).HasColumnType("numeric(18,2)");

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Restrict);

        // Un registro por préstamo por día, se debite o se omita — correr el
        // batch dos veces el mismo día es seguro por diseño, ver AutoDebitoSpiLog.cs.
        b.HasIndex(x => new { x.Fecha, x.IdPrestamo }).IsUnique();
    }
}

public class DiferimientoCuotaConfiguration : IEntityTypeConfiguration<DiferimientoCuota>
{
    public void Configure(EntityTypeBuilder<DiferimientoCuota> b)
    {
        b.ToTable("diferimiento_cuota", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Comentario).HasMaxLength(500).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrestamoCastigadoConfiguration : IEntityTypeConfiguration<PrestamoCastigado>
{
    public void Configure(EntityTypeBuilder<PrestamoCastigado> b)
    {
        b.ToTable("prestamo_castigado", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.SaldoTransferido).HasColumnType("numeric(18,2)");
        b.Property(x => x.Comentario).HasMaxLength(500).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdPrestamo).IsUnique();
    }
}

public class EstadoCustodioPagareConfiguration : IEntityTypeConfiguration<EstadoCustodioPagare>
{
    public void Configure(EntityTypeBuilder<EstadoCustodioPagare> b)
    {
        b.ToTable("estado_custodio_pagare", "colocacion");
        b.HasKey(x => x.Codigo);
        b.Property(x => x.Codigo).HasMaxLength(2);
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
    }
}

public class PagareCustodiaConfiguration : IEntityTypeConfiguration<PagareCustodia>
{
    public void Configure(EntityTypeBuilder<PagareCustodia> b)
    {
        b.ToTable("pagare_custodia", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstado).HasMaxLength(2).IsRequired();
        b.Property(x => x.Ubicacion).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.Prestamo).WithMany().HasForeignKey(x => x.IdPrestamo).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.CodigoEstado).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdPrestamo).IsUnique();

        b.HasMany(x => x.Movimientos).WithOne(x => x.PagareCustodia)
            .HasForeignKey(x => x.IdPagareCustodia).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PagareCustodiaMovimientoConfiguration : IEntityTypeConfiguration<PagareCustodiaMovimiento>
{
    public void Configure(EntityTypeBuilder<PagareCustodiaMovimiento> b)
    {
        b.ToTable("pagare_custodia_movimiento", "colocacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.CodigoEstado).HasMaxLength(2).IsRequired();
        b.Property(x => x.Ubicacion).HasMaxLength(200).IsRequired();
        b.Property(x => x.RegistradoPor).HasMaxLength(200).IsRequired();
    }
}
