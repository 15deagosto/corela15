using Corela15.Domain.Clientes;

namespace Corela15.Domain.Ahorros;

/// <summary>
/// Bridge cuenta↔socio. <see cref="Principal"/> identifica al titular
/// principal entre cotitulares — para no duplicar montos entre cotitulares
/// al agregar saldos por socio (ver Regla #1 de la investigación original).
/// </summary>
public class CuentaCliente
{
    public Guid IdCuenta { get; set; }
    public Cuenta Cuenta { get; set; } = null!;

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public bool Principal { get; set; }
}
