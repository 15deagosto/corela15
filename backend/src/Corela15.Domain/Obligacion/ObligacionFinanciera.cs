using Corela15.Domain.Contabilidad;
using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.Obligacion;

/// <summary>
/// Deuda real que la cooperativa toma con terceros (bancos, CONAFIPS,
/// entidades del exterior) — grupo 26 del CUC. Verificado y reconstruido
/// campo por campo contra el "Manual Técnico de la Estructura de
/// Obligaciones Financieras del SFPS" (SEPS, v1.0, 13/03/2026) — la
/// estructura OF01, exigida desde el corte 30/09/2026. Reemplaza el
/// espejo simplificado original (documentado como tal desde Nivel 7,
/// nunca tuvo filas reales) — 0 filas en Softbank y en Corela15 hasta
/// esta ronda, sin riesgo de romper datos existentes.
/// </summary>
public class ObligacionFinanciera
{
    public Guid Id { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    // Campos 1-4 del manual: identificación de la institución acreedora.
    // Tipo restringido a R (RUC, personas jurídicas) o X (extranjero) —
    // el resto de la Tabla 02 general queda inhabilitado para OF01,
    // validado en el servicio, no una FK a un catálogo de 5 valores.
    public string TipoIdentificacionAcreedor { get; set; } = string.Empty;
    public string IdentificacionAcreedor { get; set; } = string.Empty;
    public string CodigoPaisAcreedor { get; set; } = string.Empty;
    public Nacionalidad PaisAcreedor { get; set; } = null!;
    public string NumeroObligacion { get; set; } = string.Empty;

    // Campos 5-9
    public string DestinoLineaCredito { get; set; } = string.Empty;
    public decimal MontoLineaCredito { get; set; }
    public decimal MontoPorUtilizar { get; set; }
    public string CodigoEstado { get; set; } = string.Empty;
    public EstadoObligacionFinanciera Estado { get; set; } = null!;
    public decimal Saldo { get; set; }

    // Campos 10-11: cuenta+subcuenta reales del CUC (grupo 26, ya sembrado
    // completo) — un solo FK a la subcuenta (6 dígitos), la cuenta de 4
    // dígitos se resuelve por navegación (CodigoPadre truncado), sin
    // duplicar el dato.
    public Guid IdCuentaContable { get; set; }
    public CuentaContable CuentaContable { get; set; } = null!;

    // Campos 12-16
    public decimal TasaInteres { get; set; }
    public decimal InteresesPorPagar { get; set; }
    public bool PagaComision { get; set; }
    public decimal? TasaInteresComision { get; set; }
    public decimal? ValorComision { get; set; }

    // Campos 17-21
    public DateOnly FechaConcesion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public string CodigoPeriodicidadPago { get; set; } = string.Empty;
    public PeriodicidadPago PeriodicidadPago { get; set; } = null!;
    public bool TienePeriodoGracia { get; set; }
    public int? NumeroPeriodosGracia { get; set; }

    // Campos 22-25
    public string CodigoClase { get; set; } = string.Empty;
    public ClaseObligacionFinanciera Clase { get; set; } = null!;
    public decimal? ValorVencido { get; set; }
    public string? CodigoFormaCancelacion { get; set; }
    public FormaCancelacionObligacion? FormaCancelacion { get; set; }
    public string? NumeroObligacionAnterior { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
