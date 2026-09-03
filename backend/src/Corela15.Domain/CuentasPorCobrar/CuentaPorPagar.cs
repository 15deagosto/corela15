using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.CuentasPorCobrar;

/// <summary>
/// Verificado contra CUENTASPORCOBRAR.ESTADO_CUENTAPORPAGAR (4 códigos
/// reales: A Activo, C Cancelado, N Anulado, V Vencido). "Vencido" no se
/// modela como estado propio — se calcula al vuelo comparando
/// FechaVencimiento contra hoy, mismo criterio ya usado en
/// EstadoCuentaPorCobrar (Vigente/Cancelada/Castigada) para no duplicar
/// un estado derivado. "Anulada" sí es real y distinta de "Castigada" del
/// lado de CxC: una cuenta por pagar se anula cuando se registró por
/// error (nunca hubo obligación real), no cuando se da de baja como
/// incobrable (ese concepto no aplica a un pasivo propio).
/// </summary>
public enum EstadoCuentaPorPagar
{
    Vigente = 1,
    Cancelada = 2,
    Anulada = 3
}

/// <summary>
/// Cuentas por pagar internas de la cooperativa — grupo CUC 25. Espejo
/// de CUENTASPORCOBRAR.CUENTAPORPAGAR (mismo esquema que CuentaPorCobrar,
/// lado pasivo — el proveedor/beneficiario es una Persona, igual que el
/// deudor de una cuenta por cobrar).
/// </summary>
public class CuentaPorPagar
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
    public EstadoCuentaPorPagar Estado { get; set; } = EstadoCuentaPorPagar.Vigente;
}
