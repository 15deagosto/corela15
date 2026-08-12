using Corela15.Domain.Clientes;

namespace Corela15.Domain.Colocacion;

/// <summary>Bridge préstamo↔socio (deudor + codeudores) — verificado contra COLOCACION.PRESTAMO_CLIENTE.</summary>
public class PrestamoCliente
{
    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public bool Principal { get; set; }
    public bool Activo { get; set; } = true;
}
