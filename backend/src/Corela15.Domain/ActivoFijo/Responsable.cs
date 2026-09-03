using Corela15.Domain.General;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.ActivoFijo;

/// <summary>
/// Persona responsable de la custodia de uno o más activos fijos —
/// verificado contra ACTIVOFIJO.RESPONSABLE. En Softbank referencia
/// "IDAGENCIADEPARTAMENTO" (no existe una tabla AGENCIADEPARTAMENTO
/// real separada, verificado) — resuelve directo a la agencia real.
/// </summary>
public class Responsable
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
