using Corela15.Domain.General;

namespace Corela15.Domain.ActivoFijo;

/// <summary>
/// Cabecera de una corrida de depreciación mensual por agencia —
/// verificado contra ACTIVOFIJO.DEPRECIACION_AGENCIA (mismo patrón que
/// DevengoInteresLog: única por Agencia+Fecha, correr el batch dos veces
/// el mismo mes es seguro por diseño).
/// </summary>
public class DepreciacionAgencia
{
    public Guid Id { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    /// <summary>Último día del mes calculado.</summary>
    public DateOnly Fecha { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}

/// <summary>
/// Línea real de depreciación por activo dentro de una corrida —
/// verificado contra ACTIVOFIJO.DEPRECIACION_AGENCIA_DETALLE.
/// </summary>
public class DepreciacionAgenciaDetalle
{
    public Guid Id { get; set; }

    public Guid IdDepreciacionAgencia { get; set; }
    public DepreciacionAgencia DepreciacionAgencia { get; set; } = null!;

    public Guid IdActivo { get; set; }
    public Activo Activo { get; set; } = null!;

    public decimal DepreciacionPeriodo { get; set; }
    public decimal DepreciacionAcumulada { get; set; }
    public decimal SaldoLibros { get; set; }
    public bool DepreciadoTotal { get; set; }
}
