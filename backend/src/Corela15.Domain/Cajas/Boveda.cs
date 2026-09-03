using Corela15.Domain.General;
using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Cajas;

/// <summary>
/// Configuración real de bóveda por agencia — verificado contra CAJAS.BOVEDA:
/// a diferencia de Ventanilla (sesión diaria por cajero), Bóveda es una
/// configuración persistente por agencia (límites mínimo/máximo de
/// existencia real, un responsable) que no se "abre y cierra" cada día.
/// El detalle transaccional de movimientos caja↔bóveda queda fuera de
/// alcance (sin caso de uso real todavía, ver BovedaItemBoveda).
/// </summary>
public class Boveda
{
    public Guid Id { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public Guid IdUsuarioResponsable { get; set; }
    public Usuario UsuarioResponsable { get; set; } = null!;

    public decimal ExistenciaMinima { get; set; }
    public decimal ExistenciaMaxima { get; set; }
    public decimal ExistenciaMinimaCaja { get; set; }
    public decimal ExistenciaMaximaCaja { get; set; }
    public decimal ExistenciaMinimaCajaChica { get; set; }
    public decimal ExistenciaMaximaCajaChica { get; set; }

    public bool Activa { get; set; } = true;
}
