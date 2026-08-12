using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.CuentasPorCobrar;

public enum EstadoCuentaPorCobrar
{
    Vigente = 1,
    Cancelada = 2,
    Castigada = 3
}

/// <summary>
/// Cuentas por cobrar internas de la cooperativa — grupo CUC 16 (no confundir
/// con la cartera de crédito de los socios, Nivel 3). Espejo simplificado de
/// CUENTASPORCOBRAR.CUENTAPORCOBRAR.
/// </summary>
public class CuentaPorCobrar
{
    public Guid Id { get; set; }
    public string Concepto { get; set; } = string.Empty;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public int Cuotas { get; set; }
    public decimal MontoInicial { get; set; }
    public decimal Saldo { get; set; }
    public DateOnly FechaCreacion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public EstadoCuentaPorCobrar Estado { get; set; } = EstadoCuentaPorCobrar.Vigente;
}
