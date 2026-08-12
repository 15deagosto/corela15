using Corela15.Domain.Common;
using Corela15.Domain.General;

namespace Corela15.Domain.Contabilidad;

public enum EstadoComprobante
{
    Registrado = 1,
    Anulado = 2
}

/// <summary>
/// El asiento contable (cabecera). Espejo de CONTABILIDAD.COMPROBANTECONTABLE.
/// El balance (suma débitos == suma créditos) es un invariante que se valida
/// en Application al registrar el comprobante, no acá — Domain solo modela
/// la forma, no el caso de uso.
/// </summary>
public class ComprobanteContable : AuditableEntity
{
    public Guid Id { get; set; }
    public long Numero { get; set; }
    public DateOnly Fecha { get; set; }

    public int IdTipoComprobante { get; set; }
    public TipoComprobanteContable TipoComprobante { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public string? Descripcion { get; set; }
    public EstadoComprobante Estado { get; set; } = EstadoComprobante.Registrado;

    public List<MovimientoComprobanteContable> Movimientos { get; set; } = new();
}
