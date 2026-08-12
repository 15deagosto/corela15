using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.CuentasPorCobrar;

public enum EstadoCuentaPorPagar
{
    Vigente = 1,
    Pagada = 2
}

/// <summary>
/// Cuentas por pagar internas de la cooperativa — grupo CUC 25. Espejo
/// simplificado de CUENTASPORCOBRAR.CUENTAPORPAGAR (mismo esquema que
/// CuentaPorCobrar, lado pasivo — el proveedor/beneficiario es una Persona,
/// igual que el deudor de una cuenta por cobrar).
/// </summary>
public class CuentaPorPagar
{
    public Guid Id { get; set; }
    public string Concepto { get; set; } = string.Empty;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public decimal MontoInicial { get; set; }
    public decimal Saldo { get; set; }
    public DateOnly FechaCreacion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public EstadoCuentaPorPagar Estado { get; set; } = EstadoCuentaPorPagar.Vigente;
}
