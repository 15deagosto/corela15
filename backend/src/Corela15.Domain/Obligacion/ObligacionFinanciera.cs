using Corela15.Domain.General;

namespace Corela15.Domain.Obligacion;

public enum EstadoObligacion
{
    Vigente = 1,
    Cancelada = 2
}

/// <summary>
/// Deuda que la cooperativa toma con terceros (bancos, CONAFIPS) — grupo CUC
/// 26. Espejo simplificado de OBLIGACION.OBLIGACION_FINANCIERA (verificado:
/// 0 filas en Softbank hoy, la coop no tiene obligaciones vigentes, pero la
/// estructura es real y exigida por el catálogo). Estructuralmente un
/// espejo de Colocacion.Prestamo pero al revés: acá la cooperativa es la
/// deudora, no el socio.
/// </summary>
public class ObligacionFinanciera
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? NumeroPagare { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public decimal DeudaInicial { get; set; }
    public decimal ValorEntregado { get; set; }
    public decimal SaldoActual { get; set; }
    public EstadoObligacion Estado { get; set; } = EstadoObligacion.Vigente;
}
