using Corela15.Domain.General;
using Corela15.Domain.Seguridad;

namespace Corela15.Domain.Cajas;

/// <summary>
/// Sesión de caja de un cajero por día — verificado contra CAJAS.VENTANILLA.
/// El detalle de movimientos de efectivo por denominación
/// (VENTANILLA_ITEMCAJA_MOVIMIENTOTRANSACCION_EFECTIVO, 2,2M filas en
/// Softbank, la tabla más grande del esquema) queda fuera de alcance
/// inicial — se agrega cuando se construya el caso de uso real de
/// transacción de ventanilla, junto con el vínculo a
/// FINANCIERO.MOVIMIENTO_AFECTACION (Nivel 1).
/// </summary>
public class Ventanilla
{
    public Guid Id { get; set; }

    public DateOnly Fecha { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public bool Cuadrada { get; set; }
    public bool Cerrada { get; set; }
    public bool PuedeTransaccionar { get; set; } = true;
}
