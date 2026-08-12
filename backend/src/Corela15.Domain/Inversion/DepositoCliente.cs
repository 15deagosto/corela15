using Corela15.Domain.Clientes;

namespace Corela15.Domain.Inversion;

/// <summary>
/// Bridge depósito↔socio, confirmado en la base real (INVERSION.DEPOSITO_CLIENTE)
/// — mismo patrón que Ahorros.CuentaCliente: <see cref="Principal"/> identifica
/// al titular entre cotitulares.
/// </summary>
public class DepositoCliente
{
    public Guid IdDeposito { get; set; }
    public Deposito Deposito { get; set; } = null!;

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public bool Principal { get; set; }
    public bool Activo { get; set; } = true;
}
