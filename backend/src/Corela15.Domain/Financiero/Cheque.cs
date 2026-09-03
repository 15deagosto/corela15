using Corela15.Domain.Ahorros;
using Corela15.Domain.General;

namespace Corela15.Domain.Financiero;

/// <summary>Verificado contra FINANCIERO.ESTADO_CHEQUE (7 códigos reales).</summary>
public enum EstadoCheque
{
    Ingresado = 1,
    DepositadoEnBanco = 2,
    Efectivizado = 3,
    Protestado = 4,
    Anulado = 5,
    EnProceso = 6,
    Extraviado = 7,
}

/// <summary>
/// Cheque de terceros recibido en depósito por un socio — verificado
/// contra FINANCIERO.CHEQUE (485 filas reales) + AHORROS.CUENTA_CHEQUE
/// (bridge real 1:1 al mismo volumen, acá integrado como FK directa en
/// vez de tabla puente separada, mismo criterio de simplificación ya
/// aplicado a otros bridges 1:1 de esta sesión). Primer módulo real de
/// "Financiero" (cheques como instrumento de pago distinto del
/// efectivo) — antes este core no tenía ningún concepto de cheque.
/// </summary>
public class Cheque
{
    public Guid Id { get; set; }

    public int IdBanco { get; set; }
    public Banco Banco { get; set; } = null!;

    public string CuentaCorriente { get; set; } = string.Empty;
    public string NumeroCheque { get; set; } = string.Empty;
    public decimal Valor { get; set; }

    public Guid IdCuenta { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public DateOnly FechaIngreso { get; set; }
    public EstadoCheque Estado { get; set; } = EstadoCheque.Ingresado;

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}

/// <summary>
/// Bitácora real de protesto — verificado contra FINANCIERO.
/// CHEQUE_PROTESTO (16 filas reales).
/// </summary>
public class ChequeProtesto
{
    public Guid Id { get; set; }

    public Guid IdCheque { get; set; }
    public Cheque Cheque { get; set; } = null!;

    public string? Documento { get; set; }
    public DateTimeOffset FechaProceso { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}
